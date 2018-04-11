using System;

namespace Unity3dAzure.BingSpeech {
  public class SpeechMessageEventArgs : EventArgs {
    public string RequestId { get; private set; }

    public string Text { get; private set; }

    public Boolean IsPhrase { get; private set; }

    public SpeechMessageEventArgs (string requestId, string text, Boolean isPhrase) {
      this.RequestId = requestId;
      this.Text = text;
      this.IsPhrase = isPhrase;
    }
  }
}
