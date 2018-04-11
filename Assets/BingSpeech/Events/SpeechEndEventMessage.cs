using System;

namespace Unity3dAzure.BingSpeech {
  public class SpeechEndEventArgs : EventArgs {
    public string RequestId { get; private set; }

    public int Offset { get; private set; }

    public SpeechEndEventArgs (string requestId, int offset) {
      this.RequestId = requestId;
      this.Offset = offset;
    }
  }
}
