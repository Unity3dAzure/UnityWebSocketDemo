using System;
namespace Unity3dAzure.BingSpeech {
  public class TurnStartEventArgs : EventArgs {
    public string RequestId { get; private set; }
    public string ServiceTag { get; private set; }

    public TurnStartEventArgs (string requestId, string serviceTag = "") {
      this.RequestId = requestId;
      this.ServiceTag = serviceTag;
    }
  }
}
