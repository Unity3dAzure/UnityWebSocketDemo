using System.Collections;
using System.Collections.Generic;
using System;

namespace Unity3dAzure.BotFramework {
  [Serializable]
  public class MessageActivity {
    public string type;
    public string id;
    public string timestamp;
    public string localTimestamp;
    public string channelId;
    public string serviceUrl;
    public ActivityFrom from;
    public ActivityFrom recipient;
    public ActivityConversation conversation;
    public string text;
    public string inputHint;
    public string replyToId;
  }
}
