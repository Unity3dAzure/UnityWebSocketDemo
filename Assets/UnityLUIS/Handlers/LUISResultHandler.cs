using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Unity3dAzure.LUIS {
  public class LUISResultHandler : MonoBehaviour {

    [SerializeField, Tooltip ("Entity type used to match GameObject target names")]
    private string entityTargetType;

    [SerializeField, Tooltip ("GameObjects to be effected by LUIS")]
    private List<LUISTarget> targets;

    [Tooltip ("Enable this to allow LUIS actions to target any gazed GameObject")]
    public bool GlobalScope = false;
    //[SerializeField, Tooltip ("Entity value and synonyms for targeting the current object")]
    private string[] thisTargetSynonyms = { "this", "that", "it", "target", "object", "thing" };

    [SerializeField, Tooltip ("Actions to be triggered by LUIS result")]
    private List<LUISAction> actions;

    private StringComparison ignoreCase = StringComparison.OrdinalIgnoreCase;

    void OnEnable () {
      LUISApp.OnLUISQueryResult += OnLUISQueryResult;
    }

    void OnDisable () {
      LUISApp.OnLUISQueryResult -= OnLUISQueryResult;
    }

    #region LUIS result handler

    private void OnLUISQueryResult (LUISResult result, GameObject gazedObject) {
      if (targets == null || actions == null) {
        Debug.LogWarning ("Add some Unity game object targets and actions");
        return;
      }

      LUISAction action = null;

      foreach (LUISAction possibleAction in actions) {
        if (possibleAction.topScoringIntent.Equals (result.topScoringIntent.intent, ignoreCase)) {
          action = possibleAction;
          break;
        }
      }

      if (action == null) {
        Debug.LogWarning ("No Unity action matches LUIS intent:" + result.topScoringIntent.intent);
        return;
      }

      // Find target's entity name
      LUISEntity targetEntity = GetEntityFromLUISResult (result, entityTargetType);

      if (targetEntity == null) {
        Debug.LogWarning ("Couldn't find any targets for entity type: " + entityTargetType);
        return;
      }

      // Get action entity property
      LUISEntity entityProperty = GetEntityFromLUISResult (result, action.entityType);
      if (entityProperty == null) {
        Debug.LogWarning ("No result matches for the action entity type: " + action.entityType);
        return;
      }

      // Only target "this" gameobject
      if (thisTargetSynonyms.Any (s => string.Equals (s, targetEntity.entity, ignoreCase))) {
        if (gazedObject == null) {
          Debug.Log ("No gazed object detected for target entity: " + targetEntity.entity);
          return;
        }
        if (GlobalScope) {
          PerformAction (action, gazedObject, entityProperty);
          return;
        }
        // Check if the gazed object is registered target for LUIS
        bool isThisALUISTarget = false;
        foreach (LUISTarget possibleTarget in targets) {
          if (gazedObject == possibleTarget.target) {
            isThisALUISTarget = true;
            Debug.Log ("This gazed object is a LUIS target: " + gazedObject.name + " entity:" + entityProperty.entity);
            PerformAction (action, possibleTarget.target, entityProperty);
            break;
          }
        }
        if (!isThisALUISTarget) {
          Debug.LogWarning ("This gazed gameObject is not a LUIS target: " + gazedObject.name + " entity:" + entityProperty.entity + "\nTip: Enable GlobalScope to allow LUIS actions to target any gazed GameObject.");
        }
        return;
      }

      // Target the named gameobject (could be modified to invoke action in multiple matched targets)
      foreach (LUISTarget possibleTarget in targets) {
        if (String.Equals (possibleTarget.entityName, targetEntity.entity, ignoreCase) || (targetEntity.resolution != null && targetEntity.resolution.values.Any (s => string.Equals (s, possibleTarget.entityName, ignoreCase)))) {
          PerformAction (action, possibleTarget.target, entityProperty);
          break;
        } else {
          Debug.Log ("Skipped entity name: " + possibleTarget.entityName + " LUIS entity: " + targetEntity.entity);
        }
      }
    }

    // Invoke LUIS Unity Event with <GameObject, LUISEntity>
    private void PerformAction (LUISAction action, GameObject target, LUISEntity entity) {
      if (action.Response != null) {
        action.Response.Invoke (target, entity);
      }
    }

    private LUISEntity GetEntityFromLUISResult (LUISResult result, string entityType) {
      LUISEntity matchedEntity = null;
      foreach (LUISEntity entity in result.entities) {
        if (entity.type.Equals (entityType, ignoreCase) || (entity.resolution != null && entity.resolution.values.Any (s => string.Equals (s, entityType, ignoreCase)))) {
          matchedEntity = entity;
          break;
        }
      }
      return matchedEntity;
    }

    #endregion

  }
}
