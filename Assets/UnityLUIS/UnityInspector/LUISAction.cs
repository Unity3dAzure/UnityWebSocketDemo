using System;
using UnityEngine;
using UnityEngine.Events;

namespace Unity3dAzure.LUIS {

  [Serializable]
  public class LUISAction {
    [Tooltip ("The top scoring intent to match.")]
    public string topScoringIntent;

    [Tooltip ("The entity type to match.")]
    public string entityType;

    [Tooltip ("The handler to be invoked.")]
    public LUISEvent Response;
  }

}
