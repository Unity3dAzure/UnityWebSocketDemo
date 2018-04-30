using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity3dAzure.WebSockets;

// This receiver acts as a controller for multiple gameobject targets
public class MyTransformReceiverTargets : DataReceiver {

  [SerializeField]
  public List<GameObject> targets;

  // Local store for incoming object data
  private List<TransformData> data;
  private TransformData currentValue;

  // props
  private List<MyTargetProps> props = new List<MyTargetProps>();
  private List<TransformData> transforms = new List<TransformData>();

  [SerializeField]
  private float UpdateTime = 2.0f; // seconds
  private float currentTime = 0f;

  void Start() {
    if (targets == null || targets.Count == 0) {
      return;
    }

    foreach (GameObject target in targets) {
      props.Add(target.GetComponent<MyTargetProps>());
      transforms.Add(new TransformData(target.transform));
    }

    reset();
  }

  void reset() {
    int i = 0;
    foreach (GameObject target in targets) {
      transforms[i] = new TransformData(target.transform);
      i++;
    }
  }

  void Update() {
    if (targets == null || targets.Count == 0 || data == null || data.Count == 0) {
      return;
    }

    if (currentValue == null) {
      // Set current value and remove from local store
      currentValue = data[0];
      data.RemoveAt(0);
    }

    if (currentTime <= UpdateTime) {
      currentTime += Time.deltaTime;

      float x, y, z, rate, animationTime;
      int i = 0;
      TransformData t;
      MyTargetProps properties;
      foreach (GameObject target in targets) {
        properties = props[i];
        t = transforms[i];
        if (properties != null) {
          animationTime = Math.Min(properties.AnimationTime, UpdateTime);
        } else {
          animationTime = 0;
        }
        rate = currentTime / animationTime;

        target.transform.localPosition = Vector3.Lerp(t.position, currentValue.position, rate);
        target.transform.localRotation = Quaternion.Lerp(t.rotation, currentValue.rotation, rate);
        target.transform.localScale = Vector3.Lerp(t.scale, currentValue.scale, rate);

        i++;
      }

    } else {
      currentTime = 0;

      // Update transform original position, rotation, scale
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
