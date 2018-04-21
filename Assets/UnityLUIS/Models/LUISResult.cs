using System;
using UnityEngine;
namespace Unity3dAzure.LUIS {
  [Serializable]
  public class LUISResult {
    public string query;
    public LUISIntent topScoringIntent;
    public LUISIntent[] intents;
    public LUISEntity[] entities;

    public override string ToString () {
      return JsonUtility.ToJson (this);
    }
  }
}
