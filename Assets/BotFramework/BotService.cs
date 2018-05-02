using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System;
using Unity3dAzure.WebSockets;
using System.Text;
using System.Text.RegularExpressions;

namespace Unity3dAzure.BotFramework {
  public class BotService : UnityWebSocket {
    [Header("Connect with Direct Line secret key")]
    [SerializeField]
    private string DirectLineSecretKey; // Direct Line secret key

    [Header("Connect with a generated token")]
    [SerializeField]
    private string Token;

    // Conversation state
    private string ConversationId;
    private string Watermark;

    // Token timer
    private const uint EXPIRES_IN = 1800; // max time to refresh token
    private float MaxTime = EXPIRES_IN;
    private float Timer = 0; // time since token was generated
    private uint TimeBefore = 5; // amount of seconds to refresh token before MaxTime is up
    private bool HasTimerStarted = false;

    [Space]
    [SerializeField]
    private string UserName = "UnityUser";

    [SerializeField]
    private bool AutoConnect = false;

    // Use this for initialization
    void Start() {
      if (AutoConnect) {
        Connect();
      }
    }

    void Update() {
      if (!HasTimerStarted) {
        return;
      }
      Timer += Time.deltaTime;
      if (Timer > MaxTime) {
        // refresh
        HasTimerStarted = false;
        Timer = 0;
        RefreshToken();
      }
    }

    public void Stop() {
      HasTimerStarted = false;
      Timer = 0;
      // close web socket
      Close();
    }

    #region Refresh Token

    private void UpdateToken(TokenResponse response) {
      StartUsingToken(response.token, response.conversationId, response.expires_in);
    }

    public void StartUsingToken(string token, string conversationId = null, uint expiresIn = EXPIRES_IN) {
      Token = token;
      ConversationId = conversationId;
      if (expiresIn > 0) {
        MaxTime = expiresIn - TimeBefore;
      }
      // restart Update timer
      Timer = 0;
      HasTimerStarted = true;
    }

    public void RefreshToken() {
      if (HasTimerStarted) {
        return;
      }
      StartCoroutine(PostRefreshToken(Token));
    }

    private IEnumerator PostRefreshToken(string token) {
      Debug.Log("*** Refesh token *** " + Timer);
      using (UnityWebRequest www = UnityWebRequest.Post("https://directline.botframework.com/v3/directline/tokens/refresh", "")) {
        www.SetRequestHeader("Authorization", "Bearer " + token);
        www.chunkedTransfer = false;
        yield return www.SendWebRequest();
        if (www.isHttpError || www.isNetworkError) {
          Debug.LogError("Refresh request error: " + www.error + " status:" + www.responseCode.ToString());
        } else {
          Debug.Log("Refresh request received:\n" + www.downloadHandler.text);
          try {
            TokenResponse response = JsonUtility.FromJson<TokenResponse>(www.downloadHandler.text);
            UpdateToken(response);
          } catch (Exception ex) {
            Debug.LogError("Failed to parse token response:" + ex.Message + " body:" + www.downloadHandler.text);
          }
        }
      }
    }

    #endregion

    private void StartConversation() {
      if (!string.IsNullOrEmpty(DirectLineSecretKey)) {
        StartConversationWithToken(DirectLineSecretKey);
      } else if (!string.IsNullOrEmpty(Token)) {
        StartConversationWithToken(Token);
      } else {
        Debug.LogWarning("Direct Line secret key or token required");
      }
    }

    private void StartConversationWithToken(string token) {
      if (!string.IsNullOrEmpty(ConversationId) || !string.IsNullOrEmpty(Watermark)) {
        // resume conversation
        StartCoroutine(GetConversation(token, ConversationId, Watermark));
      } else {
        // start conversation
        StartCoroutine(PostConversation(token));
      }
    }

    private IEnumerator PostConversation(string token) {
      using (UnityWebRequest www = UnityWebRequest.Post("https://directline.botframework.com/v3/directline/conversations", "")) {
        www.SetRequestHeader("Authorization", "Bearer " + token);
        www.chunkedTransfer = false;
        yield return www.SendWebRequest();
        if (www.isHttpError || www.isNetworkError) {
          Debug.LogError("Post Conversation request error: " + www.error + " status:" + www.responseCode.ToString());
        } else {
          Debug.Log("Post Conversations request received:\n" + www.downloadHandler.text);
          try {
            TokenResponse response = JsonUtility.FromJson<TokenResponse>(www.downloadHandler.text);
            UpdateToken(response);
            ConnectWebSocketWithUrl(response.streamUrl);
          } catch (Exception ex) {
            Debug.LogError("Failed to parse token response:" + ex.Message + " body:" + www.downloadHandler.text);
          }
        }
      }
    }

    private IEnumerator GetConversation(string token, string conversationId, string watermark) {
      string url = "https://directline.botframework.com/v3/directline/conversations/" + conversationId;
      if (!string.IsNullOrEmpty(watermark)) {
        url = url + "?watermark=" + watermark;
      }
      using (UnityWebRequest www = UnityWebRequest.Get(url)) {
        www.SetRequestHeader("Authorization", "Bearer " + token);
        yield return www.SendWebRequest();
        if (www.isHttpError || www.isNetworkError) {
          Debug.LogError("Get Conversations request error: " + www.error + " status:" + www.responseCode.ToString() + " Conversation id:" + conversationId + " Watermark:" + watermark + " Token:\n" + token);
        } else {
          Debug.Log("Get Conversations request received:\n" + www.downloadHandler.text);
          try {
            TokenResponse response = JsonUtility.FromJson<TokenResponse>(www.downloadHandler.text);
            UpdateToken(response);
            ConnectWebSocketWithUrl(response.streamUrl);
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
      if (_ws == null || !_ws.IsOpen()) {
        Debug.LogWarning("Web socket not open. A web socket connection is required to receive responses from the bot.");
      }
      if (string.IsNullOrEmpty(message) || string.IsNullOrEmpty(ConversationId)) {
        Debug.LogError("Error no message or conversation id");
        return;
      }
      if (!string.IsNullOrEmpty(DirectLineSecretKey)) {
        Debug.Log("Send message using secret:" + message + " ConversationId:" + ConversationId);
        StartCoroutine(PostMessage(DirectLineSecretKey, message, ConversationId, UserName));
      } else if (!string.IsNullOrEmpty(Token)) {
        Debug.Log("Send message using token:" + message + " ConversationId:" + ConversationId);
        StartCoroutine(PostMessage(Token, message, ConversationId, UserName));
      } else {
        Debug.LogError("Unable to send message. A Direct Line secret key or token is required.");
      }
    }

    private IEnumerator PostMessage(string token, string message, string conversationId, string user) {
      UserActivity activity = UserActivity.CreateMessage(message, user);
      string json = JsonUtility.ToJson(activity);
      byte[] bytes = Encoding.UTF8.GetBytes(json);
      string url = "https://directline.botframework.com/v3/directline/conversations/" + conversationId + "/activities";
      Debug.Log("activity:" + json + "url:" + url);
      using (UnityWebRequest www = new UnityWebRequest(url)) {
        www.SetRequestHeader("Authorization", "Bearer " + token);
        www.SetRequestHeader("Content-Type", "application/json");
        www.SetRequestHeader("Accept", "application/json");
        www.chunkedTransfer = false;
        www.uploadHandler = new UploadHandlerRaw(bytes);
        www.uploadHandler.contentType = "application/json";
        www.downloadHandler = new DownloadHandlerBuffer();
        www.method = UnityWebRequest.kHttpVerbPOST;
        yield return www.SendWebRequest();
        if (www.isHttpError || www.isNetworkError) {
          Debug.LogError("Post Message error: " + www.error + " status:" + www.responseCode.ToString());
        } else {
          Debug.Log("Sent message to bot:\n" + www.downloadHandler.text);
        }
      }
    }

    private void ConnectWebSocketWithUrl(string url) {
      Debug.Log("Connect Web Socket with url: " + url);
      WebSocketUri = url;
      ConnectWebSocket();
    }

    #region Web Socket methods

    public override void Connect() {
      StartConversation();
    }

    #endregion

    #region Web Socket handlers

    protected override void OnWebSocketOpen(object sender, EventArgs e) {
      Debug.Log("Web socket is open");
    }

    protected override void OnWebSocketClose(object sender, WebSocketCloseEventArgs e) {
      Debug.Log("Web socket closed with reason: " + e.Reason );
      DettachHandlers();
    }

    protected override void OnWebSocketMessage(object sender, WebSocketMessageEventArgs e) {
      Debug.LogFormat("Web socket {1} message:\n{0}", e.Data, e.IsBinary ? "binary" : "string");

      // Update watermark id
      var match = Regex.Match(e.Data, "\"watermark\":\\s?\"([A-z0-9])\"", RegexOptions.Multiline);
      if (match.Groups.Count == 2) {
        Watermark = match.Groups[1].Value;
        Debug.Log("Watermark id:" + Watermark);
      }

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
