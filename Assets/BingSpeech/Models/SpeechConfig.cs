using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Unity3dAzure.BingSpeech {
  // NB: get_operatingSystem can only be called from the main thread.
  [Serializable]
  public class SpeechConfig {
    public ContextSystem system;
    public ContextOS os;
    public ContextDevice device;

    public SpeechConfig (string version = "2.0.12341") {
      this.system = new ContextSystem ();
      this.system.version = version;

      this.os = new ContextOS ();
      this.os.platform = GetPlatform ();
      this.os.name = GetOS ();
      this.os.version = GetOSVersion ();

      this.device = new ContextDevice ();
      this.device.model = SystemInfo.deviceModel;
      this.device.manufacturer = GetDeviceManufacturer ();
      this.device.version = GetDeviceVersion ();
    }

    public override string ToString () {
      return JsonUtility.ToJson (this);
    }

    public string ToJSON (Boolean prettyPrint = true) {
      return JsonUtility.ToJson (this, prettyPrint);
    }

    public static string GetPlatform () {
      string platform;
      switch (Application.platform) {
        case RuntimePlatform.WindowsPlayer:
        case RuntimePlatform.WindowsEditor:
        case RuntimePlatform.WSAPlayerX86:
        case RuntimePlatform.WSAPlayerX64:
        case RuntimePlatform.WSAPlayerARM:
          platform = "Windows";
          break;
        case RuntimePlatform.OSXEditor:
        case RuntimePlatform.OSXPlayer:
          platform = "Mac";
          break;
        case RuntimePlatform.LinuxPlayer:
        case RuntimePlatform.LinuxEditor:
          platform = "Linux";
          break;
        case RuntimePlatform.IPhonePlayer:
          platform = "iOS";
          break;
        default:
          platform = Application.platform.ToString ();
          break;
      }
      return platform;
    }

    public static string GetOS () {
      string os = SystemInfo.operatingSystem;
      if (os.Contains ("Mac OS X")) {
        return "Mac OS X";
      }
      Match match = Regex.Match (os, @"on\s([0-9]{2}\sbit\s)?([A-z0-9\s]*)$", RegexOptions.IgnoreCase);
      if (match.Success) {
        os = match.Groups[2].Value;
      }
      return os;
    }

    public static string GetOSVersion () {
      string os = SystemInfo.operatingSystem;
      Match match = Regex.Match (os, @"[0-9]+\.[0-9]+(\.[0-9]+)?(\.[0-9]+)?", RegexOptions.IgnoreCase);
      if (match.Success) {
        os = match.Value;
      }
      return os;
    }

    public static string GetDeviceManufacturer () {
      string device = SystemInfo.deviceModel;
      Match match = Regex.Match (device, @"[A-z]+", RegexOptions.IgnoreCase);
      if (match.Success) {
        device = match.Value;
      }
      return device;
    }

    public static string GetDeviceVersion () {
      string device = SystemInfo.deviceModel;
      Match match = Regex.Match (device, @"[0-9\.,]+", RegexOptions.IgnoreCase);
      if (match.Success) {
        device = match.Value;
      }
      return device;
    }

  }

  [Serializable]
  public class ContextSystem {
    public string version;
  }

  [Serializable]
  public class ContextOS {
    public string platform;
    public string name;
    public string version;
  }

  [Serializable]
  public class ContextDevice {
    public string manufacturer;
    public string model;
    public string version;
  }
}
