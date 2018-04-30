using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity3dAzure.WebSockets;

public class MyTransformReceiver : DataReceiver {
  // Local store for incoming object data
  private List<TransformData> data;
  private TransformData currentValue;

  [SerializeField]
  private float AnimationTime = 0.5f; // seconds
  private float currentTime = 0f;

  private Vector3 originalPosition;
  private Quaternion originalRotation;
  private Vector3 originalScale;

  private void reset() {
    originalPosition = transform.localPosition;
    originalRotation = transform.localRotation;
    originalScale = transform.localScale;
  }

  // Use this for initialization
  void Start() {
    reset();
  }

  // Update is called once per frame
  void Update() {
    if (data == null || data.Count == 0) {
      return;
    }

    if (currentValue == null) {
      // Set current value and remove from local store
      currentValue = data[0];
      data.RemoveAt(0);
    }

    // Lerp transform: position, rotation and scale
    if (currentTime <= AnimationTime) {
      currentTime += Time.deltaTime;

      float x, y, z, rate;
      rate = currentTime / AnimationTime;

      transform.localPosition = Vector3.Lerp(originalPosition, currentValue.position, rate);
      transform.localRotation = Quaternion.Lerp(originalRotation, currentValue.rotation, rate);
      transform.localScale = Vector3.Lerp(originalScale, currentValue.scale, rate);

    } else {
      currentTime = 0;
      reset();
      // Update current value and remove from local store
      currentValue = data[0];
      data.RemoveAt(0);
    }
  }

  public override void OnReceivedData(object sender, EventArgs args) {
    if (args == null) {
      return;
    }

    // return early if wrong type of EventArgs
    var myArgs = args as MyTransformEventArgs;
    if (myArgs == null) {
      return;
    }

    // Add object data into local store
    if (data == null) {
      data = new List<TransformData>();
    }
    data.Add(myArgs.Data);
  }

}
