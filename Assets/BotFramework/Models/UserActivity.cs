using System.Collections;
using System.Collections.Generic;
using System;

namespace Unity3dAzure.BotFramework {
  [Serializable]
  public class UserActivity  {

    public string type;
    public ActivityFrom from;
    public string text;

    public UserActivity(string message, string userId = "user", string type = "message") {
      this.text = message;
      this.from = new ActivityFrom(userId);
      this.type = type;
    }

    public static UserActivity CreateMessage(string message, string userId = "user") {
      return new UserActivity(message, userId);
    }
  }
}
