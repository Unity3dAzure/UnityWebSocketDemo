using System;

namespace Unity3dAzure.BingSpeech {
  public class SpeechStartEventArgs : EventArgs {
    public string RequestId { get; private set; }

    public int Offset { get; private set; }

    public SpeechStartEventArgs (string requestId, int offset = 0) {
      this.RequestId = requestId;
      this.Offset = offset;
    }
  }
}
