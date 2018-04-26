using System;
using UnityEngine;
using UnityEngine.Events;

namespace Unity3dAzure.LUIS {
  [System.Serializable]
  public class LUISEvent : UnityEvent<GameObject, LUISEntity> { }
}
