using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System;
using Unity3dAzure.WebSockets;
using System.Text;

namespace Unity3dAzure.BotFramework {
  public class BotService : UnityWebSocket {

    [SerializeField]
    private string directLineKey; // Direct Line key
    private string token; // TODO: refresh temp token 

    private string ConversationId;

    [SerializeField]
    private string UserName = "UnityUser";

    

    [SerializeField]
    private bool AutoStart = true;

    [SerializeField]
    private bool AutoConnect = false;


    // Use this for initialization
    void Start() {
      if (string.IsNullOrEmpty(directLineKey)) {
        Debug.LogError("Direct Line key required");
        this.enabled = false;
        return;
      }
      if (AutoStart) {
        StartConversation();
      }
    }

    public void StartConversation() {
      StartCoroutine(PostStartConversation());
    }

    private IEnumerator PostStartConversation() {
      using (UnityWebRequest www = UnityWebRequest.Post("https://directline.botframework.com/v3/directline/conversations", "")) {
        www.SetRequestHeader("Authorization", "Bearer " + directLineKey);
        www.chunkedTransfer = false;
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError) {
          Debug.Log("UWR Error: " + www.error);
        } else {
          Debug.Log("Request ready:\n" + www.downloadHandler.text);
          try {
            TokenResponse res = JsonUtility.FromJson<TokenResponse>(www.downloadHandler.text);
            token = res.token;
            ConversationId = res.conversationId;
            ConnectToWebSocket(res.streamUrl);
          } catch (Exception ex) {
            Debug.LogError("Failed to parse token response:" + ex.Message + " body:" + www.downloadHandler.text);
          }
        }
      }
    }

    public override void SendInputText(InputField input) {
      if (input == null) {
        Debug.LogError("No input field set");
        return;
      }
      SendBotMessage(input.text);
    }

    public void SendBotMessage(string message) {
      if (string.IsNullOrEmpty(message) || string.IsNullOrEmpty(ConversationId)) {
        Debug.LogError("Error no message or conversation id");
        return;
      }
      Debug.Log("Send message:" + message + " ConversationId:" + ConversationId);
      StartCoroutine(PostMessage(message, ConversationId, UserName));
    }

    private IEnumerator PostMessage(string message, string conversationId, string user = "user") {
      UserActivity activity = UserActivity.CreateMessage(message, user);
      string json = JsonUtility.ToJson(activity);
      byte[] bytes = Encoding.UTF8.GetBytes(json);
      string url = "https://directline.botframework.com/v3/directline/conversations/" + conversationId + "/activities";
      Debug.Log("activity:" + json + "url:" + url);
      using (UnityWebRequest www = new UnityWebRequest(url)) {
        www.SetRequestHeader("Authorization", "Bearer " + directLineKey);
        www.SetRequestHeader("Content-Type", "application/json");
        www.SetRequestHeader("Accept", "application/json");
        www.chunkedTransfer = false;
        www.uploadHandler = new UploadHandlerRaw(bytes);
        www.uploadHandler.contentType = "application/json";
        www.downloadHandler = new DownloadHandlerBuffer();
        www.method = UnityWebRequest.kHttpVerbPOST;
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError) {
          Debug.Log("UWR Error: " + www.error + " status:" + www.responseCode.ToString());
        } else {
          Debug.Log("Sent message to bot:\n" + www.downloadHandler.text);
        }
      }
    }

    private void ConnectToWebSocket(string url) {
      if (string.IsNullOrEmpty(token)) {
        Debug.LogError("Failed to set headers - token is required.");
        return;
      }

      WebSocketUri = url;
      Debug.Log("Bing Speech endpoint: " + WebSocketUri);

      if (AutoConnect) {
        Connect();
      }
    }

    #region Web Socket handlers

    protected override void OnWebSocketOpen(object sender, EventArgs e) {
      Debug.Log("Web socket is open");
    }

    protected override void OnWebSocketClose(object sender, WebSocketCloseEventArgs e) {
      Debug.Log("Web socket closed with reason: " + e.Reason );
      if (!e.WasClean) {
        DisconnectWebSocket();
      }
      DettachHandlers();
    }

    protected override void OnWebSocketMessage(object sender, WebSocketMessageEventArgs e) {
      Debug.LogFormat("Web socket {1} message:\n{0}", e.Data, e.IsBinary ? "binary" : "string");

      // Raise web socket data handler event
      if (OnData != null) {
        OnData(e.RawData, e.Data, e.IsBinary);
      }
    }

    protected override void OnWebSocketError(object sender, WebSocketErrorEventArgs e) {
      Debug.LogError("Web socket error: " + e.Message);
      DisconnectWebSocket();
    }

    #endregion

  }
}
