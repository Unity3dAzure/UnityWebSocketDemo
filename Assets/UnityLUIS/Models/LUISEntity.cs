using System;
namespace Unity3dAzure.LUIS {
  [Serializable]
  public class LUISEntity {
    public string entity;
    public string type;
    public int startIndex;
    public int endIndex;
    public float score;
    public LUISEntityResolution resolution;
  }

  [Serializable]
  public class LUISEntityResolution {
    public string[] values;
  }
}
