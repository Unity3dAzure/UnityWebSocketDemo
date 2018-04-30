using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity3dAzure.WebSockets;
using UnityEngine;

public class MyTransformHandler : DataHandler {
  // Web Socket JSON data handler
  public override void OnData (byte[] rawData, string jsonText, Boolean isBinary) {
    try {
      var obj = JsonUtility.FromJson<TransformData> (jsonText);
      RaiseOnReceivedData (this, new MyTransformEventArgs (obj));
    } catch (ArgumentException exception) {
      Debug.LogWarningFormat ("Failed to parse JSON string. Reason: {0} \n'{1}'", exception.Message, jsonText);
    }
  }
}
