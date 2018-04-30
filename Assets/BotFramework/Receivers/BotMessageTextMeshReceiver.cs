using System;
using System.Collections;
using System.Collections.Generic;
using Unity3dAzure.WebSockets;
using UnityEngine;

namespace Unity3dAzure.BotFramework {
  public sealed class BotMessageTextMeshReceiver : TextMeshReceiver {

    public override void OnReceivedData(object sender, EventArgs args) {
      if (args == null) {
        return;
      }

      if (args.GetType() != typeof(BotMessageEventArgs)) {
        return;
      }

      var myArgs = args as BotMessageEventArgs;

      // only update text from bot
      if (myArgs.IsBot) {
        text = myArgs.Text;
        needsUpdated = true;
      }
    }
  }
}
