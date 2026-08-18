using SakuraaCastingMod.Features.Visuals;
using SakuraaCastingMod.Shared.Helpers;
using SakuraaCastingMod.Shared.Models;
using System;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Desktop.Ui.Framework.Menus;

public static class InterpolationMenu
{
  public static MenuBuilder _interpolationMenu;

  public static void ApplyInterpolationSettings()
  {
    if ((((UnityEngine.Object) GorillaParent.instance == (UnityEngine.Object) null) || ((UnityEngine.Object) GorillaTagger.Instance == (UnityEngine.Object) null) ? 1 : (((UnityEngine.Object) GorillaTagger.Instance.offlineVRRig == (UnityEngine.Object) null) ? 1 : 0)) != 0)
      return;
    VRRig offlineVrRig = GorillaTagger.Instance.offlineVRRig;
    float lerpValueBody = offlineVrRig.lerpValueBody;
    float lerpValueFingers = offlineVrRig.lerpValueFingers;
    float num = Interpolation.RigSmoothEnabled ? Interpolation.RigSmooth : 1f;
    if ((double) Mathf.Abs(num - Interpolation.LastRigSmooth) <= 1.0 / 1000.0)
      return;
    foreach (GorillaData gorillaData in GorillaDataHandler.GorillaDataList)
    {
      VRRig rigByGorilla = PlayerTranslator.GetRigByGorilla(gorillaData);
      if ((((UnityEngine.Object) rigByGorilla == (UnityEngine.Object) null) ? 1 : (((UnityEngine.Object) rigByGorilla == (UnityEngine.Object) offlineVrRig) ? 1 : 0)) == 0)
      {
        rigByGorilla.lerpValueBody = lerpValueBody * num;
        rigByGorilla.lerpValueFingers = lerpValueFingers * num;
      }
    }
    if (!Interpolation.RigSmoothEnabled)
      Interpolation.RigSmooth = 1f;
    Interpolation.LastRigSmooth = num;
  }

  public static void Draw()
  {
    if (InterpolationMenu._interpolationMenu == null)
      InterpolationMenu.Initialize();
    InterpolationMenu._interpolationMenu?.Draw();
  }

  public static void Initialize()
  {
    InterpolationMenu._interpolationMenu = new MenuBuilder("Rig Smoothing Menu", 300f).SetPositionRef(400f, 100f).AddSpace(25f).AddSlider("Rig Smoothing:", Interpolation.RigSmooth, 0.1f, 5f, (Action<float>) (value => Interpolation.RigSmooth = value), 2, "Adjusts the interpolation smoothness for other player rigs.").AddDynamicButton((Func<string>) (() => "Reset"), (Action) (() => Interpolation.RigSmooth = 1f), "Reset smoothing to default (1.0).");
  }
}
