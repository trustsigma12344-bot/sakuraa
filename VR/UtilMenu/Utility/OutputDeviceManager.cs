using System;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.VR.UtilMenu.Utility;

public static class OutputDeviceManager
{
  public static void RestartAudioSystem()
  {
    try
    {
      UnityEngine.Debug.Log((object) "[SakuraaUtil] Attempting to restart Audio System...");
      if (!AudioSettings.Reset(AudioSettings.GetConfiguration()))
        UnityEngine.Debug.LogWarning((object) "[SakuraaUtil] Audio System Restart Failed (Unity returned false).");
      else
        UnityEngine.Debug.Log((object) "[SakuraaUtil] Audio System Restarted Successfully.");
    }
    catch (Exception ex)
    {
      UnityEngine.Debug.LogError((object) ("[SakuraaUtil] Error restarting audio: " + ex.Message));
    }
  }
}
