using System;
using System.Collections;
using System.Collections.Generic;
using Unity3dAzure.WebSockets;
using UnityEngine;

namespace Unity3dAzure.BingSpeech {
  public sealed class SpeechTextMeshReceiver : TextMeshReceiver {

    public override void OnReceivedData (object sender, EventArgs args) {
      if (args == null) {
        return;
      }

      Type argsType = args.GetType ();

      if (argsType == typeof (SpeechMessageEventArgs)) {
        var myArgs = args as SpeechMessageEventArgs;
        text = myArgs.Text;
        needsUpdated = true;
      } else if (argsType == typeof (TurnEndEventArgs)) {
        // Debug.Log ("*** TURN END ***");
      }
    }

  }
}
