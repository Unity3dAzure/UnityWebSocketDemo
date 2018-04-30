using System;
public class MyTransformEventArgs : EventArgs {
  public TransformData Data { get; private set; }

  public MyTransformEventArgs(TransformData data) {
    this.Data = data;
  }
}
