using System;
namespace Unity3dAzure.BingSpeech {
  public class TurnEndEventArgs : EventArgs {
    public string RequestId { get; private set; }

    public TurnEndEventArgs (string requestId) {
      this.RequestId = requestId;
    }
  }
}
