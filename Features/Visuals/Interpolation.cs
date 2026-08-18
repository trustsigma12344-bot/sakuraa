using SakuraaCastingMod.Shared.Helpers;
using SakuraaCastingMod.Shared.Models;
using UnityEngine;
using UnityEngine.XR;

#nullable disable
namespace SakuraaCastingMod.Features.Visuals;

public static class Interpolation
{
  [SavedSetting("InterpolationRigSmoothEnabled", true)]
  public static bool RigSmoothEnabled = true;
  [SavedSetting("InterpolationRigSmooth", 1f)]
  public static float RigSmooth = 1f;
  public static float LastRigSmooth = 1f;

  public static void ApplyInterpolationSettings()
  {
    if (XRSettings.isDeviceActive || (((UnityEngine.Object) GorillaParent.instance == (UnityEngine.Object) null) || ((UnityEngine.Object) GorillaTagger.Instance == (UnityEngine.Object) null) ? 1 : (((UnityEngine.Object) GorillaTagger.Instance.offlineVRRig == (UnityEngine.Object) null) ? 1 : 0)) != 0)
      return;
    VRRig offlineVrRig = GorillaTagger.Instance.offlineVRRig;
    float lerpValueBody = offlineVrRig.lerpValueBody;
    float lerpValueFingers = offlineVrRig.lerpValueFingers;
    float num = Interpolation.RigSmoothEnabled ? Interpolation.RigSmooth : 1f;
    if ((double) Mathf.Abs(num - (Interpolation.RigSmoothEnabled ? Interpolation.LastRigSmooth : Interpolation.RigSmooth)) <= 1.0 / 1000.0)
      return;
    foreach (GorillaData gorillaData in GorillaDataHandler.GorillaDataList)
    {
      VRRig rigByGorilla = PlayerTranslator.GetRigByGorilla(gorillaData);
      if (!((UnityEngine.Object) rigByGorilla == (UnityEngine.Object) offlineVrRig))
      {
        rigByGorilla.lerpValueBody = lerpValueBody * num;
        rigByGorilla.lerpValueFingers = lerpValueFingers * num;
      }
    }
    if (!Interpolation.RigSmoothEnabled)
      Interpolation.RigSmooth = 1f;
    Interpolation.LastRigSmooth = num;
  }
}
