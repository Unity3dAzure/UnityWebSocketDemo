using System;
using System.Collections;
using System.Collections.Generic;
using Unity3dAzure.WebSockets;
using UnityEngine;

namespace Unity3dAzure.BotFramework {
  public class MessageActivitiesHandler : DataHandler {
    // Web Socket JSON data handler
    public override void OnData(byte[] rawData, string text, Boolean isBinary) {

      // ignore empty messages
      if (String.IsNullOrEmpty(text)) {
        return;
      }

      // parse activities message
      MessageActivities response = ParseMessageActivities(text);
      if (response.activities == null || response.activities.Length < 1) {
        Debug.LogWarning("No activities message found:\n" + text);
        return;
      }

      // Handle bot is typing status message - type: "typing"
      if (String.IsNullOrEmpty(response.activities[0].text) && string.Equals(response.activities[0].type, "typing")) {
        Debug.Log("Bot is typing...");
        return;
      }

      if (response.activities.Length > 1) {
        Debug.LogWarning("Handle case if more than 1 activity is received.");
      }

      MessageActivity message = response.activities[0];

      // decide what to do depending on message path type
      if (String.IsNullOrEmpty(message.inputHint)) {
        RaiseOnReceivedData(this, new BotMessageEventArgs(message.text, false));
      } else if (!String.IsNullOrEmpty(message.inputHint)) {
        RaiseOnReceivedData(this, new BotMessageEventArgs(message.text, true));
      } else {
        Debug.LogWarning("Unhandled message type: " + message.inputHint + " message: " + message.text);
      }
    }

    #region Parse JSON body helpers

    public static MessageActivities ParseMessageActivities(string json) {
      try {
        return JsonUtility.FromJson<MessageActivities>(json);
      } catch (ArgumentException exception) {
        Debug.LogWarningFormat("Failed to parse bot message. Reason: {0} \n'{1}'", exception.Message, json);
        return null;
      }
    }

    #endregion
  }
}
