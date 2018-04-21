using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace Unity3dAzure.BingSpeech {
  public class MicRecorder : MonoBehaviour {
    // Mic recording wav data delegate
    public delegate void RecordedData (byte[] data);
    public static RecordedData OnRecordedData;

    public delegate void RecordingStopped ();
    public static RecordingStopped OnRecordingStopped;

    [SerializeField, Range (1, 600), Tooltip ("Recording Duration (seconds)")]
    private int recordingDuration = 180; // should not be greater than the speech recognition interactive time duration limit
    private float recordingTimer = 0;
    private bool isStarted = false;

    private string mic;
    private AudioClip audioClip;

    private const int SAMPLE_RATE = 16000;
    private Boolean includeWavFileHeader = true;

    [SerializeField, Range (0.1f, 1.0f)]
    private float updateInterval = 0.2f;
    private float intervalTimer = 0;
    private int lastPosition = 0;

    [SerializeField]
    private Boolean autoRecord = false;

    void Start () {
      if (autoRecord) {
        StartRecording ();
      }
    }

    void Update () {
      if (!isStarted) {
        return;
      }
      recordingTimer += Time.deltaTime;
      if (recordingTimer > recordingDuration) {
        Stop ();
        return;
      }
      // Send data as we are recording
      intervalTimer += Time.deltaTime;
      if (intervalTimer > updateInterval) {
        intervalTimer = 0;
        FireWavBytes ();
      }
    }

    //
    // Summary:
    // Starts voice recording
    public void StartRecording () {
      if (Microphone.devices.Length == 0) {
        Debug.LogError ("No microphone found.");
        return;
      }

      if (isStarted) {
        Debug.LogWarning ("Already recording");
        return;
      }

      if (mic == null) {
        mic = Microphone.devices[0];
      }

      Debug.Log ("Start Recording: " + mic);
      audioClip = Microphone.Start (mic, false, recordingDuration, SAMPLE_RATE);
      isStarted = true;
    }

    private void FireWavBytes () {
      byte[] wavBytes = GetAudioClipDataAsWavBytes ();
      //Debug.Log ("Fire AudioClip bytes: " + wavBytes.Length);
      // Fire recorded wav bytes
      if (OnRecordedData != null && wavBytes != null) {
        OnRecordedData (wavBytes);
      }
    }

    private byte[] GetAudioClipDataAsWavBytes () {
      int currentPosition = Microphone.GetPosition (mic);
      if (currentPosition == 0) {
        Debug.LogWarning ("No audio recording");
        return null;
      }
      byte[] wavBytes = WavDataUtility.FromAudioClip (audioClip, currentPosition, lastPosition, includeWavFileHeader);
      includeWavFileHeader = false; // wav file header is required only once
      lastPosition = currentPosition;
      return wavBytes;
    }

    //
    // Summary:
    // Stops the voice recording and fires wav bytes
    public void Stop () {
      if (!isStarted) {
        Debug.LogWarning ("Already stopped");
        return;
      }

      // Get Audio Clip, stop recording and reset state
      byte[] wavBytes = GetAudioClipDataAsWavBytes ();
      Microphone.End (mic);
      resetState ();

      Debug.Log ("Stopped Recording! AudioClip bytes: " + wavBytes.Length);

      // Fire recorded wav bytes
      if (OnRecordedData != null) {
        OnRecordedData (wavBytes);
      }

      // Fire finished recording status
      if (OnRecordingStopped != null) {
        OnRecordingStopped ();
      }
    }

    private void resetState () {
      audioClip = null;
      recordingTimer = 0;
      intervalTimer = 0;
      lastPosition = 0;
      isStarted = false;
    }

  }
}
