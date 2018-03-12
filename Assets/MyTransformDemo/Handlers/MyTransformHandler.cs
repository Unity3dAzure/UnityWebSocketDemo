using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System;
using UnityWebSocket;

public class MyTransformHandler : DataHandler {
  // Web Socket JSON data handler
  public override void OnData(byte[] data) {
    string jsonText = Encoding.UTF8.GetString(data);
    try {
      var obj = JsonUtility.FromJson<MyTransform>(jsonText);
      RaiseOnReceivedData(this, new MyTransformEventArgs(obj));
    } catch (ArgumentException exception) {
      Debug.LogWarningFormat("Failed to parse JSON string. Reason: {0} \n'{1}'", exception.Message, jsonText);
    }
  }

}
