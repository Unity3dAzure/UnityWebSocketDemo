using System.Collections;
using System.Collections.Generic;
using System;

namespace Unity3dAzure.BotFramework {
  [Serializable]
  public class MessageActivities {
    public MessageActivity[] activities;
    public string watermark;
  }
}
