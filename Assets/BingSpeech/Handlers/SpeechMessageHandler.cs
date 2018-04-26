using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Unity3dAzure.WebSockets;
using UnityEngine;

namespace Unity3dAzure.BingSpeech {
  public class SpeechMessageHandler : DataHandler {
    // Web Socket JSON data handler
    public override void OnData (byte[] rawData, string text, Boolean isBinary) {
      //string text = Encoding.UTF8.GetString(rawData);

      string path = "";
      string requestId = "";
      string body = "";

      // detect message path type
      var match = Regex.Match (text, "^Path:([A-z\\.]+)", RegexOptions.Multiline);
      if (match.Groups.Count == 2 && match.Groups[1].Value.Length > 0) {
        path = match.Groups[1].Value;
      }

      // detect message request id
      match = Regex.Match (text, "^X-RequestId:([A-z0-9]+)", RegexOptions.Multiline);
      if (match.Groups.Count == 2 && match.Groups[1].Value.Length > 0) {
        requestId = match.Groups[1].Value;
      }

      // detect message json body
      // match = Regex.Match (text, "^\\n?(\\{.*\\})", RegexOptions.Multiline | RegexOptions.Singleline);
      // if (match.Groups.Count == 2 && match.Groups[1].Value.Length > 0) {
      //   body = match.Groups[1].Value.Trim ();
      // }
      body = text.Substring (text.IndexOf ("{"));

      if (String.IsNullOrEmpty (path) || String.IsNullOrEmpty (requestId) || String.IsNullOrEmpty (body)) {
        Debug.LogError ("Failed to parse speech message header and body:\n" + text);
        return;
      }

      // decide what to do depending on message path type
      if (path.Equals ("speech.hypothesis")) {
        SpeechHypothesis hypothesis = ParseSpeechHypothesis (body);
        RaiseOnReceivedData (this, new SpeechMessageEventArgs (requestId, hypothesis.Text, false));
      } else if (path.Equals ("speech.phrase")) {
        SpeechPhrase phrase = ParseSpeechPhrase (body);
        RaiseOnReceivedData (this, new SpeechMessageEventArgs (requestId, phrase.DisplayText, phrase.RecognitionStatus.Equals ("Success")));
      } else if (path.Equals ("speech.startDetected")) {
        SpeechDetected startDetected = ParseSpeechDetected (body);
        RaiseOnReceivedData (this, new SpeechStartEventArgs (requestId, startDetected.Offset));
      } else if (path.Equals ("speech.endDetected")) {
        SpeechDetected endDetected = ParseSpeechDetected (body);
        RaiseOnReceivedData (this, new SpeechEndEventArgs (requestId, endDetected.Offset));
      } else if (path.Equals ("turn.start")) {
        RaiseOnReceivedData (this, new TurnStartEventArgs (requestId, ""));
      } else if (path.Equals ("turn.end")) {
        RaiseOnReceivedData (this, new TurnEndEventArgs (requestId));
      } else {
        Debug.LogWarning ("Unhandled message path type: " + path);
      }
    }

    #region Parse JSON body helpers

    public static SpeechHypothesis ParseSpeechHypothesis (string json) {
      try {
        return JsonUtility.FromJson<SpeechHypothesis> (json);
      } catch (ArgumentException exception) {
        Debug.LogWarningFormat ("Failed to parse SpeechHypothesis. Reason: {0} \n'{1}'", exception.Message, json);
        return null;
      }
    }

    public static SpeechPhrase ParseSpeechPhrase (string json) {
      try {
        return JsonUtility.FromJson<SpeechPhrase> (json);
      } catch (ArgumentException exception) {
        Debug.LogWarningFormat ("Failed to parse SpeechPhrase. Reason: {0} \n'{1}'", exception.Message, json);
        return null;
      }
    }

    public static SpeechDetected ParseSpeechDetected (string json) {
      try {
        return JsonUtility.FromJson<SpeechDetected> (json);
      } catch (ArgumentException exception) {
        Debug.LogWarningFormat ("Failed to parse SpeechDetected. Reason: {0} \n'{1}'", exception.Message, json);
        return null;
      }
    }

    #endregion

  }
}
