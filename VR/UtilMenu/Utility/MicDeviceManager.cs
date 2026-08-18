using Photon.Voice.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.VR.UtilMenu.Utility;

public static class MicDeviceManager
{
  private static Recorder _photonRecorder;
  private const string PrefsKey = "SelectedMicrophoneDeviceName";
  private static bool _isInitialized = false;

  public static List<string> AvailableDevices { get; private set; } = new List<string>();

  public static string SelectedDevice { get; private set; }

  public static bool TryInitialize()
  {
    bool flag;
    if (!MicDeviceManager._isInitialized)
    {
      MicDeviceManager._photonRecorder = UnityEngine.Object.FindFirstObjectByType<Recorder>();
      if (((UnityEngine.Object) MicDeviceManager._photonRecorder == (UnityEngine.Object) null))
      {
        flag = false;
      }
      else
      {
        MicDeviceManager.RefreshDeviceList();
        string savedDeviceName = PlayerPrefs.GetString("SelectedMicrophoneDeviceName", MicDeviceManager.AvailableDevices.FirstOrDefault<string>());
        MicDeviceManager.SelectedDevice = MicDeviceManager.AvailableDevices.FirstOrDefault<string>((Func<string, bool>) (d => d == savedDeviceName));
        if ((!string.IsNullOrEmpty(MicDeviceManager.SelectedDevice) ? 0 : (MicDeviceManager.AvailableDevices.Count > 0 ? 1 : 0)) != 0)
          MicDeviceManager.SelectedDevice = MicDeviceManager.AvailableDevices[0];
        MicDeviceManager.ApplyMicrophoneSetting(MicDeviceManager.SelectedDevice, false);
        MicDeviceManager._isInitialized = true;
        flag = true;
      }
    }
    else
      flag = true;
    return flag;
  }

  public static bool PollForChanges()
  {
    bool flag;
    if ((((UnityEngine.Object) UtilMenuController.Instance == (UnityEngine.Object) null) ? 1 : (!UtilMenuController.Instance.isMenuEnabled ? 1 : 0)) != 0)
      flag = false;
    else if (!MicDeviceManager._isInitialized)
    {
      flag = MicDeviceManager.TryInitialize();
    }
    else
    {
      string[] devices = UnityMicrophone.devices;
      if (devices.Length == MicDeviceManager.AvailableDevices.Count)
      {
        if (((IEnumerable<string>) devices).Where<string>((Func<string, int, bool>) ((t, i) => t != MicDeviceManager.AvailableDevices[i])).Any<string>())
        {
          MicDeviceManager.RefreshDeviceList();
          flag = true;
        }
        else
          flag = false;
      }
      else
      {
        MicDeviceManager.RefreshDeviceList();
        flag = true;
      }
    }
    return flag;
  }

  private static void RefreshDeviceList()
  {
    MicDeviceManager.AvailableDevices = ((IEnumerable<string>) UnityMicrophone.devices).ToList<string>();
    if (MicDeviceManager.AvailableDevices.Count != 0)
      return;
    MicDeviceManager.AvailableDevices.Add("Default");
  }

  public static void SetMicrophone(string newDevice)
  {
    if (!MicDeviceManager._isInitialized)
      MicDeviceManager.TryInitialize();
    if (!MicDeviceManager.AvailableDevices.Contains(newDevice) || newDevice == MicDeviceManager.SelectedDevice)
      return;
    MicDeviceManager.SelectedDevice = newDevice;
    PlayerPrefs.SetString("SelectedMicrophoneDeviceName", MicDeviceManager.SelectedDevice);
    PlayerPrefs.Save();
    MicDeviceManager.ApplyMicrophoneSetting(MicDeviceManager.SelectedDevice, true);
  }

  private static void ApplyMicrophoneSetting(string device, bool restart)
  {
    if (((UnityEngine.Object) MicDeviceManager._photonRecorder == (UnityEngine.Object) null))
      MicDeviceManager._photonRecorder = UnityEngine.Object.FindFirstObjectByType<Recorder>();
    if (((UnityEngine.Object) MicDeviceManager._photonRecorder == (UnityEngine.Object) null))
      return;
    MicDeviceManager._photonRecorder.UnityMicrophoneDevice = device;
    if ((!restart ? 0 : (MicDeviceManager._photonRecorder.IsRecording ? 1 : 0)) == 0)
      return;
    MicDeviceManager._photonRecorder.RestartRecording(false);
  }
}
