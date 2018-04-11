using System;
namespace Unity3dAzure.BingSpeech {
  [Serializable]
  public class SpeechPhrase {
    public string DisplayText;
    public int Offset;
    public int Duration;
    public string RecognitionStatus;
  }
}
