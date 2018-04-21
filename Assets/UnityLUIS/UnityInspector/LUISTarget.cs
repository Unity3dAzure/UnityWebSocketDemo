using System;
using UnityEngine;
using UnityEngine.Events;

namespace Unity3dAzure.LUIS {

  [Serializable]
  public struct LUISTarget {
    public GameObject target;
    public string entityName;
  }

}
