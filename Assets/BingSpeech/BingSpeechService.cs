using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Unity3dAzure.LUIS;
using Unity3dAzure.WebSockets;
using UnityEngine;
using UnityEngine.Networking;

using KeyValue = System.Collections.Generic.KeyValuePair<string, string>;

namespace Unity3dAzure.BingSpeech {
  // Bing Speech Service WebSocket
  // https://docs.microsoft.com/en-us/azure/cognitive-services/speech/api-reference-rest/websocketprotocol
  public sealed class BingSpeechService : UnityWebSocket {

    [SerializeField]
    private string key; // Bing Speech API Key

    [SerializeField]
    private LanguageMode languageMode = LanguageMode.en_GB;

    // state
    private string token;
    private string connectionId;
    private string requestId;

    // has web socket been opened
    private Boolean isSocketStarted = false;
    private Boolean isSpeechConfigSent = false;
    private SpeechConfig speechConfig;

    [SerializeField]
    private bool AutoConnect = false;

    private float timer = 0;
    private float idleTimer = 0;
    private const uint MAX_IDLE_DURATION = 180; // 3 mins
    private const uint MAX_DURATION = 600; // 10 mins

    const string CarriageReturn = "\r\n";

    private Boolean isStarted = false;
    private LUISApp luisApp;

    #region Bing Speech Web Socket connection

    void Start () {
      // Generate Bing Speech config for this machine
      speechConfig = new SpeechConfig ();

      // Config Websocket
      ConfigureEndpoint (languageMode); // sets WebSocketUri
      Headers = new List<UnityKeyValue> ();

      luisApp = gameObject.GetComponent<LUISApp> ();

      if (AutoConnect) {
        RequestTokenAndConnect ();
      }

      isStarted = true;
    }

    void Update () {
      if (!isSocketStarted) {
        return;
      }
      idleTimer += Time.deltaTime;
      timer += Time.deltaTime;
      if (idleTimer > MAX_IDLE_DURATION || timer > MAX_DURATION) {
        Debug.Log ("Stop and close web socket. Reached max duration.");
        Close ();
        return;
      }
    }

    void OnEnable () { }

    void OnDisable () {
      DisconnectWebSocket ();
    }

    private void SubscribeMic () {
      MicRecorder.OnRecordedData += ReceivedWavBytes;
      MicRecorder.OnRecordingStopped += NewTurn;
      Debug.Log ("Subscribed to Mic recorded data");
    }

    private void UnsubscribeMic () {
      MicRecorder.OnRecordedData -= ReceivedWavBytes;
      MicRecorder.OnRecordingStopped -= NewTurn;
      Debug.Log ("Unsubscribed from Mic recorded data");
    }

    public void ConfigureEndpoint (LanguageMode language) {
      if (AutoConnect && isStarted) {
        Debug.LogWarning ("Changing language is not supported while AutoConnect is turned on.");
      }
      if (isAttached) {
        Debug.LogWarning ("Changing language is not supported after connection.");
      }
      string languageParam = language.ToString ().Replace ("_", "-");
      string endpoint = "wss://speech.platform.bing.com/speech/recognition/interactive/cognitiveservices/v1?format=simple&language=" + languageParam;
      WebSocketUri = endpoint;
    }

    #endregion

    private void RequestTokenAndConnect () {
      if (string.IsNullOrEmpty (key)) {
        Debug.LogError ("Bing Speech Key required");
        this.enabled = false;
        return;
      }
      Debug.Log ("Requesting token...");
      StartCoroutine (RequestToken ());
    }

    private void SetHeadersAndConnect () {
      if (string.IsNullOrEmpty (token)) {
        Debug.LogError ("Failed to set headers - token is required.");
        return;
      }
      connectionId = GetId ();
      Headers.Add (new UnityKeyValue ("Authorization", "Bearer " + token));
      Headers.Add (new UnityKeyValue ("X-ConnectionId", connectionId));

      Debug.Log ("Bing Speech endpoint: " + WebSocketUri);
      //Debug.Log ("Connecting with id: " + connectionId + " token:\n" + token);

      ConnectWebSocket ();
    }

    IEnumerator RequestToken () {
      using (UnityWebRequest www = UnityWebRequest.Post ("https://api.cognitive.microsoft.com/sts/v1.0/issueToken", "")) {
        www.SetRequestHeader ("Ocp-Apim-Subscription-Key", key);
        www.chunkedTransfer = false;
        yield return www.SendWebRequest ();

        if (www.isNetworkError || www.isHttpError) {
          Debug.Log (www.error);
        } else {
          // Save token
          token = www.downloadHandler.text;
          Debug.Log ("Token:\n" + www.downloadHandler.text);
          SetHeadersAndConnect ();
        }
      }
    }

    // Returns Client UTC clock time stamp in ISO 8601 format: "yyyy-MM-ddTHH\\:mm\\:ss.fffffffZ"
    public static string GetXTimestamp () {
      return DateTime.UtcNow.ToString ("o");
    }

    // Returns UUID in "no-dash" format
    public static string GetId () {
      return Guid.NewGuid ().ToString ("N");
    }

    private void SendSpeechConfig () {
      // Send a speech.config message only once per connection (and before sending any audio messages)
      if (isSpeechConfigSent) {
        SubscribeMic ();
        return;
      }

      var path = new KeyValue ("Path", "speech.config");
      var contentType = new KeyValue ("Content-Type", "application/json; charset=utf-8");
      var timestamp = new KeyValue ("X-Timestamp", GetXTimestamp ());
      string body = speechConfig.ToString ();

      string message = GenerateMessage (body, path, contentType, timestamp);
      Debug.Log ("Send message:\n" + message);
      this.SendText (message, SendSpeechConfigCallback);
    }

    private void SendSpeechConfigCallback (Boolean isSent) {
      if (!isSent) {
        Debug.LogError ("Error, failed sending speech config.");
        return;
      }
      isSpeechConfigSent = true;
      Debug.Log ("Speech config sent - ready!");
      SubscribeMic ();
    }

    public static string GenerateMessage (string body, params KeyValuePair<string, string>[] headers) {
      string header = GenerateHeader (headers);
      return header + body;
    }

    public static string GenerateHeader (params KeyValuePair<string, string>[] headers) {
      StringBuilder sb = new StringBuilder ();
      for (int i = 0; i < headers.Length; i++) {
        KeyValuePair<string, string> header = headers[i];
        sb.Append (String.Format ("{0}: {1}", header.Key, header.Value));
        sb.Append (CarriageReturn);
      }
      sb.Append (CarriageReturn);
      return sb.ToString ();
    }

    public void SendStreamingAssetsFile (string filename) {
      StartCoroutine (LoadStreamingAssetFileBytes (filename, LoadStreamingAssetFileBytesComplete));
    }

    private IEnumerator LoadStreamingAssetFileBytes (string filename, Action<byte[]> callback) {
      string filePath = Path.Combine (Application.streamingAssetsPath, filename);
      if (!File.Exists (filePath)) {
        Debug.LogError ("No file found in Unity 'Assets/StreamingAssets' directory: " + filePath);
        callback (null);
      } else if (filePath.Contains ("://")) {
        UnityWebRequest www = UnityWebRequest.Get (filePath);
        yield return www.SendWebRequest ();
        callback (www.downloadHandler.data);
      } else {
        callback (File.ReadAllBytes (filePath));
      }
    }

    private void LoadStreamingAssetFileBytesComplete (byte[] fileData) {
      if (fileData == null) {
        return;
      }
      Debug.Log ("Send audio file bytes... " + fileData.Length);
      SendAudio (fileData);
    }

    public void NewTurn () {
      requestId = GetId ();
      Debug.Log ("*** NEW TURN *** " + requestId);
    }

    public void SendAudio (byte[] data) {
      if (string.IsNullOrEmpty (requestId)) {
        requestId = GetId ();
      }

      var path = new KeyValue ("Path", "audio");
      var contentType = new KeyValue ("Content-Type", "audio/x-wav");
      var timestamp = new KeyValue ("X-Timestamp", GetXTimestamp ());
      var xRequestId = new KeyValue ("X-RequestId", requestId);

      string header = GenerateHeader (path, xRequestId, timestamp, contentType);

      Byte[] headerEncodedBytes = Encoding.ASCII.GetBytes (header);

      int headerSize = headerEncodedBytes.Length;
      UInt16 headerSize16 = Convert.ToUInt16 (headerSize);
      byte[] headerSizeBytes = BitConverter.GetBytes (headerSize16);
      if (BitConverter.IsLittleEndian) {
        Array.Reverse (headerSizeBytes);
      }
      byte[] prefix = headerSizeBytes;

      byte[] audioMessage = prefix.Concat (headerEncodedBytes).Concat (data).ToArray ();

      // Check header
      //CheckHeader (data);

      this.SendBytes (audioMessage);
    }

    private Boolean CheckHeader (byte[] data) {
      byte[] fileHeaderChars = new byte[4];
      Array.Copy (data, 0, fileHeaderChars, 0, 4);
      string fileHeader = Encoding.ASCII.GetString (fileHeaderChars);

      if (fileHeader.Equals ("RIFF")) {
        Debug.Log ("Sending wav file bytes (includes wav header) \n" + fileHeader + "...");
        return true;
      } else {
        Debug.Log ("Sending wav bytes only...");
        return false;
      }
    }

    private void StartTimers () {
      isSocketStarted = true;
      timer = 0;
      idleTimer = 0;
    }

    private void CancelTimers () {
      isSocketStarted = false;
      timer = 0;
      idleTimer = 0;
    }

    private void ResetIdleTimer () {
      idleTimer = 0;
    }

    #region Mic Recorder handlers

    private void ReceivedWavBytes (byte[] data) {
      //Debug.Log ("ReceivedWavBytes: " + data.Length);
      // Send audio message to Bing Speech
      SendAudio (data);
    }

    #endregion

    #region Web Socket methods

    public override void Connect () {
      if (string.IsNullOrEmpty (token)) {
        RequestTokenAndConnect ();
      } else {
        ConnectWebSocket ();
      }
    }

    public override void Close () {
      CancelTimers ();
      UnsubscribeMic ();
      DisconnectWebSocket ();
    }

    #endregion

    #region Web Socket handlers

    protected override void OnWebSocketOpen (object sender, EventArgs e) {
      Debug.Log ("Web socket is open");
      StartTimers ();
      SendSpeechConfig ();
    }

    protected override void OnWebSocketClose (object sender, WebSocketCloseEventArgs e) {
      Debug.Log ("Web socket closed with reason: " + e.Reason + " Time: " + timer);
      UnsubscribeMic ();
      DettachHandlers ();
    }

    protected override void OnWebSocketMessage (object sender, WebSocketMessageEventArgs e) {
      Debug.LogFormat ("Web socket {1} message:\n{0}", e.Data, e.IsBinary ? "binary" : "string");

      // Renew activity timer
      ResetIdleTimer ();

      // Only process text messages
      if (string.IsNullOrEmpty (e.Data)) {
        return;
      }

      // Handle "turn.end" message as a new request id will be need to be generated!
      var match = Regex.Match (e.Data, "^Path:([A-z\\.]+)", RegexOptions.Multiline);
      if (match.Groups.Count == 2 && match.Groups[1].Value.Equals ("turn.end")) {
        NewTurn ();
      } else if (match.Groups.Count == 2 && match.Groups[1].Value.Equals ("speech.phrase")) {
        // If a LUIS app script is attached then hand-off "speech.phrase" message as LUIS query request
        if (luisApp != null) {
          string jsonBody = e.Data.Substring (e.Data.IndexOf ("{"));
          SpeechPhrase phrase = SpeechMessageHandler.ParseSpeechPhrase (jsonBody);
          if (!string.IsNullOrEmpty (phrase.DisplayText)) {
            Debug.Log ("Send phrase to LUIS app: " + phrase.DisplayText);
            luisApp.AddQueryToQueue (phrase.DisplayText);
          }
        }
      }

      // Raise web socket data handler event
      if (OnData != null) {
        OnData (e.RawData, e.Data, e.IsBinary);
      }
    }

    protected override void OnWebSocketError (object sender, WebSocketErrorEventArgs e) {
      Debug.LogError ("Web socket error: " + e.Message);
      CancelTimers ();
      DisconnectWebSocket ();
    }

    #endregion

  }
}
