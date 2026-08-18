using HarmonyLib;
using SakuraaCastingMod.Shared.Helpers;
using System;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Features.Visuals;

public static class HzSp
{
  [SavedSetting("HzSpEnabled", false)]
  public static bool Enabled;
  public const int MinHz = 10;
  public const int MaxHz = 500;
  [SavedSetting("HzSpValue", 72)]
  public static int TargetHz = 72;
  private static readonly FieldInfo FpsField = typeof (VRRig).GetField("fps", BindingFlags.Instance | BindingFlags.NonPublic);
  private static bool _patched;
  private static int _currentHz;
  private static float _nextJitterAt;
  private static float _lastAdjustTime;
  private static int _consecutivePresses;

  public static void Init()
  {
    if (HzSp._patched)
      return;
    try
    {
      Harmony harmony = new Harmony("sakuraa.castingmod.hzspoofer");
      MethodInfo methodInfo = AccessTools.Method(typeof (VRRig), "PackCompetitiveData", (Type[]) null, (Type[]) null);
      if (methodInfo == (MethodInfo) null)
      {
        UnityEngine.Debug.LogWarning((object) "[SCM] HzSpoofer: VRRig.PackCompetitiveData not found - spoof disabled.");
      }
      else
      {
        harmony.Patch((MethodBase) methodInfo, (HarmonyMethod) null, new HarmonyMethod(typeof (HzSp), "Postfix", (Type[]) null), (HarmonyMethod) null, (HarmonyMethod) null, (HarmonyMethod) null);
        HzSp._patched = true;
      }
    }
    catch (Exception ex)
    {
      UnityEngine.Debug.LogError((object) ("[SCM] HzSpoofer init failed: " + ex.Message));
    }
  }

  private static void Postfix(VRRig __instance, ref short __result)
  {
    if ((!HzSp.Enabled ? 1 : (((UnityEngine.Object) __instance == (UnityEngine.Object) null) ? 1 : 0)) != 0 || (((UnityEngine.Object) GorillaTagger.Instance == (UnityEngine.Object) null) ? 1 : (((UnityEngine.Object) __instance != (UnityEngine.Object) GorillaTagger.Instance.offlineVRRig) ? 1 : 0)) != 0)
      return;
    if ((HzSp._currentHz == 0 ? 1 : ((double) Time.unscaledTime >= (double) HzSp._nextJitterAt ? 1 : 0)) != 0)
    {
      HzSp._currentHz = HzSp.RollJitteredHz();
      HzSp._nextJitterAt = Time.unscaledTime + UnityEngine.Random.Range(0.2f, 0.7f);
    }
    int num1 = Mathf.Clamp(HzSp._currentHz, 1, (int) byte.MaxValue);
    int num2 = (int) (ushort) __result & 65280 | num1 & (int) byte.MaxValue;
    __result = (short) num2;
    if ((!(HzSp.FpsField != (FieldInfo) null) ? 0 : (HzSp.FpsField.FieldType == typeof (int) ? 1 : 0)) == 0)
      return;
    HzSp.FpsField.SetValue((object) __instance, (object) num1);
  }

  private static int RollJitteredHz()
  {
    float num1 = UnityEngine.Random.value;
    int num2 = (double) num1 < 0.40000000596046448 ? 0 : ((double) num1 < 0.699999988079071 ? 1 : ((double) num1 < 0.85000002384185791 ? 2 : UnityEngine.Random.Range(3, 6)));
    return Mathf.Clamp(HzSp.TargetHz - num2, 1, 500);
  }

  public static void Adjust(int dir)
  {
    float unscaledTime = Time.unscaledTime;
    if ((double) unscaledTime - (double) HzSp._lastAdjustTime > 0.44999998807907104)
      HzSp._consecutivePresses = 0;
    HzSp._lastAdjustTime = unscaledTime;
    ++HzSp._consecutivePresses;
    int num = HzSp._consecutivePresses >= 16 /*0x10*/ ? 25 : (HzSp._consecutivePresses >= 10 ? 10 : (HzSp._consecutivePresses >= 4 ? 5 : 1));
    int targetHz = HzSp.TargetHz;
    HzSp.TargetHz = Mathf.Clamp(num != 1 ? (dir > 0 ? (Mathf.FloorToInt((float) targetHz / (float) num) + 1) * num : (Mathf.CeilToInt((float) targetHz / (float) num) - 1) * num) : targetHz + dir, 10, 500);
    HzSp._currentHz = 0;
  }
}
