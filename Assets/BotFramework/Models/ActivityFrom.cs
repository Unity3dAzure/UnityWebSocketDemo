using System.Collections;
using System.Collections.Generic;
using System;

namespace Unity3dAzure.BotFramework {
  [Serializable]
  public class ActivityFrom {
    public string id;
    public string name;

    public ActivityFrom(string userId) {
      this.id = userId;
    }
  }

}
