using System.Text;
using UnityEngine;

[System.Serializable]
public class MyTransform {
  public Point3 position;
  public Point3 rotation;
  public Point3 scale;

  public override string ToString() {
    return string.Format("Position: {0} Rotation: {1} Scale: {2}",
      position,
      rotation,
      scale
    );
  }
}

[System.Serializable]
public class Point3 {
  public float x;
  public float y;
  public float z;

  public override string ToString() {
    return string.Format("{0},{1},{2}", x, y, z);
  }

  public Vector3 ToVector3() {
    return new Vector3(x, y, z);
  }
}
