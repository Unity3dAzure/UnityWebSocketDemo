using UnityEngine;
using System;

[Serializable]
public class TransformData {
  public Vector3 position;
  public Quaternion rotation;
  public Vector3 scale;

  public TransformData(Vector3 localPosition, Quaternion localRotation, Vector3 localScale) {
    this.position = localPosition;
    this.rotation = localRotation;
    this.scale = localScale;
  }

  public TransformData(Transform transform) {
    this.position = transform.localPosition;
    this.rotation = transform.localRotation;
    this.scale = transform.localScale;
  }
}
