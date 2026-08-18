using HarmonyLib;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Core.Patches;

[HarmonyPatch(typeof (ControllerInputPoller), "LateUpdate")]
public class FingerPatch
{
  public static bool forceLeftGrip;
  public static bool forceRightGrip;
  public static bool forceLeftTrigger;
  public static bool forceRightTrigger;
  public static bool forceLeftPrimary;
  public static bool forceRightPrimary;
  public static bool forceLeftSecondary;
  public static bool forceRightSecondary;

  private static void Postfix(ControllerInputPoller __instance)
  {
    if ((!((UnityEngine.Object) SakuraaCastingMod.Desktop.ControlMode.Main.ControlMode.Instance != (UnityEngine.Object) null) ? 0 : (SakuraaCastingMod.Desktop.ControlMode.Main.ControlMode.Instance.Enabled ? 1 : 0)) == 0)
      return;
    if (FingerPatch.forceLeftGrip)
    {
      __instance.leftControllerGripFloat = 1f;
      __instance.leftGrab = true;
      __instance.leftGrabRelease = false;
    }
    if (FingerPatch.forceRightGrip)
    {
      __instance.rightControllerGripFloat = 1f;
      __instance.rightGrab = true;
      __instance.rightGrabRelease = false;
    }
    if (FingerPatch.forceLeftTrigger)
      __instance.leftControllerIndexFloat = 1f;
    if (FingerPatch.forceRightTrigger)
      __instance.rightControllerIndexFloat = 1f;
    if (FingerPatch.forceLeftPrimary)
      __instance.leftControllerPrimaryButton = true;
    if (FingerPatch.forceRightPrimary)
      __instance.rightControllerPrimaryButton = true;
    if (FingerPatch.forceLeftSecondary)
      __instance.leftControllerSecondaryButton = true;
    if (!FingerPatch.forceRightSecondary)
      return;
    __instance.rightControllerSecondaryButton = true;
  }
}
