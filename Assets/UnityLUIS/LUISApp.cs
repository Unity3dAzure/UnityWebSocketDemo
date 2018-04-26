using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Unity3dAzure.LUIS {
  public class LUISApp : MonoBehaviour {
    // Create and publish a LUIS app on https://www.luis.ai/applications
    // In the 'Publish' app section generate a resource key to generate the endpoint
    [SerializeField, Tooltip ("LUIS app resource endpoint (ending in 'q=')")]
    private string endpoint; // NB: the endpoint should end in 'q='

    // LUIS query result data delegate
    public delegate void LUISQueryResult (LUISResult result, GameObject gazedObject);
    public static LUISQueryResult OnLUISQueryResult;

    private List<string> queue = new List<string> ();
    [SerializeField, Range (1, 5), Tooltip ("If you get 'too many requests' HTTP error try setting this to 1.")]
    public uint MaxConcurrentRequests = 1;
    private uint requestsCount = 0;
    [SerializeField, Range (0.0f, 2.0f), Tooltip ("If you get 'too many requests' HTTP error you can try adding a delay.")]
    private float transactionDelay = 0.0f; // Basic tier requests can be throttled to 1.2 seconds to keep it under 50 transactions per sec
    private float timer = 0;

    public const float TRANSACTION_DELAY_BASIC_TIER = 1.2f;

    public float gazeMaxDistance = 100.0f;

    // Use this for initialization
    void Start () {
      if (string.IsNullOrEmpty (endpoint)) {
        Debug.LogError ("Requires LUIS app resource endpoint");
        this.enabled = false;
        return;
      }

      // Check endpoint ends in 'q='
      if (!endpoint.Substring (endpoint.Length - 2).Equals ("q=")) {
        Debug.LogError ("Expected LUIS app resource endpoint ending in 'q=' but got: " + endpoint.Substring (endpoint.Length - 2));
        this.enabled = false;
        return;
      }

      // No delay for first request
      timer = transactionDelay;

      // Uncomment to try some example LUIS utterances
      // RunSampleQueries ();
    }

    void Update () {
      timer += Time.deltaTime;
      if (requestsCount >= MaxConcurrentRequests) {
        return;
      }
      if (queue.Count >= 1 && timer > transactionDelay) {
        Query (queue[0]);
        queue.RemoveAt (0);
        timer = 0;
      }
    }

    public void AddQueryToQueue (string query) {
      if (string.IsNullOrEmpty (query)) {
        Debug.LogWarning ("Requires query string");
        return;
      }
      // don't add duplicate queries
      if (!queue.Any (query.Contains)) {
        Debug.Log ("Add query:" + query);
        queue.Add (query);
      } else {
        Debug.LogWarning ("Skipping duplicate LUIS query:" + query);
      }
    }

    // Route all requests via 'AddQueryToQueue' instead to prevent reaching LUIS transactions limit.
    protected void Query (string query) {
      if (string.IsNullOrEmpty (query)) {
        Debug.LogError ("Error: Requires query string");
        return;
      }
      string urlQueryString = Uri.EscapeDataString (query);
      requestsCount++;
      Debug.Log ("Query string: " + urlQueryString + " no. requests: " + requestsCount + " timer: " + timer);
      StartCoroutine (GetQuery (urlQueryString, GetQueryComplete)); // NB: StartCoroutine can only be called from the main thread
    }

    private IEnumerator GetQuery (string query, Action<LUISResult> callback) {
      string endpointQuery = endpoint + query;
      Debug.Log ("LUIS query endpoint:" + endpointQuery);
      using (UnityWebRequest www = UnityWebRequest.Get (endpointQuery)) {
        yield return www.SendWebRequest ();
        if (www.isNetworkError || www.isHttpError) {
          // Check if LUIS gets too many requests
          if (www.responseCode.Equals (429)) {
            Debug.LogError ("Too many LUIS requests. Try upgrading the service tier. \nQuery: " + query + "\nConcurrent requests will be limited to 1 and a transaction delay has now been set to " + TRANSACTION_DELAY_BASIC_TIER + " secs for future requests.");
            MaxConcurrentRequests = 1;
            transactionDelay = TRANSACTION_DELAY_BASIC_TIER;
          } else {
            Debug.LogError ("LUIS query error: " + www.error + " status:" + www.responseCode.ToString () + " no. requests:" + requestsCount + "\nQuery: " + query);
          }
        } else {
          // Save token as result
          string json = www.downloadHandler.text;
          try {
            LUISResult result = JsonUtility.FromJson<LUISResult> (json);
            GetQueryComplete (result);
          } catch (ArgumentException exception) {
            Debug.LogError ("Failed to parse LUIS result: " + exception.Message + " \nresult:\n" + json);
          }
        }
        requestsCount--;
      }
    }

    private void GetQueryComplete (LUISResult result) {
      Debug.Log ("LUIS result: \n" + result);

      // Raise LUIS result handler event
      if (OnLUISQueryResult != null) {
        OnLUISQueryResult (result, FindCurrentGazedTarget (gazeMaxDistance));
      }
    }

    #region Gaze

    // Provide current target for LUIS using gaze
    public static GameObject FindCurrentGazedTarget (float gazeDistance = 100.0f) {
      RaycastHit hitInfo;
      if (Physics.Raycast (
          Camera.main.transform.position,
          Camera.main.transform.forward,
          out hitInfo,
          gazeDistance,
          Physics.DefaultRaycastLayers)) {
        return hitInfo.transform.gameObject;
      }
      return null;
    }

    #endregion

    #region Sample custom queries

    public void RunSampleQueries () {
      // Sphere: "ball" entity
      AddQueryToQueue ("change ball to red");
      AddQueryToQueue ("raise the sphere");
      AddQueryToQueue ("drop the ball");
      // Cube: "box" entity (also responds to 'this' entity as it is the current gazed object at start)
      AddQueryToQueue ("color this green");
      AddQueryToQueue ("move it toward me");
      AddQueryToQueue ("shrink the box");
      // Cylinder: "bar" entity
      AddQueryToQueue ("paint the cylinder blue");
      AddQueryToQueue ("scale up the pipe");
      AddQueryToQueue ("shift the tube off to the right");
    }

    #endregion

  }
}
