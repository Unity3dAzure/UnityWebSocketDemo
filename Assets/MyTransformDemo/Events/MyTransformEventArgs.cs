using System;
public class MyTransformEventArgs : EventArgs {
  public MyTransform Data { get; private set; }

  public MyTransformEventArgs(MyTransform data) {
    this.Data = data;
  }
}
