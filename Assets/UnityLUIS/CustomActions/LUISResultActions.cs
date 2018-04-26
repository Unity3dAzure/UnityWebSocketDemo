using System;
using UnityEngine;
using UnityEngine.Events;

namespace Unity3dAzure.LUIS {
  public class LUISResultActions : MonoBehaviour {

    [Header ("Change Size controls")]
    [Range (0.1f, 2.0f)]
    public float ScaleAmount = 0.5f;

    [Header ("Change Position controls")]
    [Range (0.1f, 5.0f)]
    public float MoveDistance = 1.0f;
    [Range (1.0f, 10.0f)]
    public float GazeDistance = 4.0f;

    public void ChangeColour (GameObject target, LUISEntity entity) {
      string color = GetEntityResolutionValue (entity); //entity.entity;
      Debug.Log ("Change color of target: " + target.name + " to color:" + color);
      Color myColor;
      if (ColorUtility.TryParseHtmlString (color, out myColor)) {
        target.GetComponent<Renderer> ().material.color = myColor;
      } else {
        Debug.LogWarning ("ColorUtility can't parse the color entity: " + color);
      }
    }

    public void ChangeSize (GameObject target, LUISEntity entity) {
      string size = GetEntityResolutionValue (entity);
      Debug.Log ("Change size of target: " + target.name + " to size:" + size);
      switch (size) {
        case "bigger":
          target.transform.localScale += new Vector3 (ScaleAmount, ScaleAmount, ScaleAmount);
          break;
        case "smaller":
          if (target.transform.localScale.x - ScaleAmount > 0 || target.transform.localScale.y - ScaleAmount > 0 || target.transform.localScale.z - ScaleAmount > 0) {
            target.transform.localScale -= new Vector3 (ScaleAmount, ScaleAmount, ScaleAmount);
          }
          break;
        default:
          Debug.LogWarning ("Unhandled size entity: " + size);
          break;
      }
    }

    // NB: In this example all entity positions are handled in a 1st person / VR scenario (taken from the main camera's pov).
    public void ChangePosition (GameObject target, LUISEntity entity) {
      string position = GetEntityResolutionValue (entity);
      Debug.Log ("Change position of target: " + target.name + " to position: " + position + " f: " + Camera.main.transform.forward);
      switch (position) {
        case "forward": // move the object towards the user
          target.transform.localPosition -= Camera.main.transform.forward * MoveDistance;
          break;
        case "backward": // move the object away from the user
          target.transform.localPosition += Camera.main.transform.forward * MoveDistance;
          break;
        case "left": // move the object to the user's left
          target.transform.localPosition -= Camera.main.transform.right * MoveDistance;
          break;
        case "right": // move the object to the user's right
          target.transform.localPosition += Camera.main.transform.right * MoveDistance;
          break;
        case "up": // move the object higher
          target.transform.localPosition += Camera.main.transform.up * MoveDistance;
          break;
        case "down": // move the object lower
          target.transform.localPosition -= Camera.main.transform.up * MoveDistance;
          break;
        case "center": // move the object into the center of user's current gaze
          target.transform.localPosition = Camera.main.transform.position + Camera.main.transform.forward * GazeDistance;
          break;
        default:
          Debug.LogWarning ("Unhandled position entity: " + position);
          break;
      }
    }

    // Get the normalized value of an entity's list of synonyms
    private string GetEntityResolutionValue (LUISEntity entity, bool lowerCase = true) {
      if (entity.resolution == null || entity.resolution.values == null || entity.resolution.values[0] == null) {
        Debug.LogWarning ("No resolution value found for entity: " + entity.entity);
        return lowerCase ? entity.entity.ToLower () : entity.entity;
      }
      return lowerCase ? entity.resolution.values[0].ToLower () : entity.resolution.values[0];
    }

    #region Unity Inspector test methods

    [SerializeField]
    private GameObject _testTarget;

    public void TestChangeColour (string entityProperty) {
      if (_testTarget == null) {
        return;
      }
      LUISEntity entity = new LUISEntity ();
      entity.entity = entityProperty; // "red" for example
      ChangeColour (_testTarget, entity);
    }

    public void TestChangeSize (string entityProperty) {
      if (_testTarget == null) {
        return;
      }
      LUISEntity entity = new LUISEntity ();
      entity.entity = entityProperty; // "smaller" for example
      ChangeSize (_testTarget, entity);
    }

    public void TestChangePosition (string entityProperty) {
      if (_testTarget == null) {
        return;
      }
      LUISEntity entity = new LUISEntity ();
      entity.entity = entityProperty; // "forward" for example
      ChangePosition (_testTarget, entity);
    }

    #endregion

  }
}
