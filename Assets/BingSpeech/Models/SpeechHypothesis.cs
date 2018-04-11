using System;
namespace Unity3dAzure.BingSpeech {
  [Serializable]
  public class SpeechHypothesis {
    public string Text;
    public int Offset;
    public int Duration;
  }
}
