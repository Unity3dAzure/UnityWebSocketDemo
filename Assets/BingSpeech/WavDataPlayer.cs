using System;
using UnityEngine;

namespace Unity3dAzure.BingSpeech {
  [RequireComponent (typeof (AudioSource))]
  public class WavDataPlayer : MonoBehaviour {
    AudioSource audioSource;

    void Start () {
      audioSource = GetComponent<AudioSource> ();
    }

    void OnEnable () {
      MicRecorder.OnRecordedData += ReceivedWavBytes;
    }

    void OnDisable () {
      MicRecorder.OnRecordedData -= ReceivedWavBytes;
    }

    #region Mic Recorder handlers

    private void ReceivedWavBytes (byte[] data) {
      // Debug.Log("Player ReceivedWavBytes!");
      AudioClip audioClip = WavDataUtility.ToAudioClip (data);

      audioSource.clip = audioClip;
      audioSource.Play ();
    }

    #endregion

  }
}
