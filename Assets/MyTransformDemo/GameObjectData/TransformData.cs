using UnityEngine;

class TransformData {
  public Vector3 localPosition;
  public Quaternion localRotation;
  public Vector3 localScale;

  public TransformData(Vector3 localPosition, Quaternion localRotation, Vector3 localScale) {
    this.localPosition = localPosition;
    this.localRotation = localRotation;
    this.localScale = localScale;
  }

  public TransformData(Transform transform) {
    this.localPosition = transform.localPosition;
    this.localRotation = transform.localRotation;
    this.localScale = transform.localScale;
  }
}
