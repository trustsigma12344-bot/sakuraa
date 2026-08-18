using SakuraaCastingMod.Core;
using SakuraaCastingMod.Desktop.Camera;
using SakuraaCastingMod.Features.Overlays;
using System;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Desktop.Ui.Framework.Menus;

public static class FreeCamMenu
{
  public static MenuBuilder FreecamMenu;

  public static bool IsInitialized => FreeCamMenu.FreecamMenu != null;

  public static void Draw()
  {
    if (!FreeCamMenu.IsInitialized)
      FreeCamMenu.Initialize();
    if ((!FreeCamMenu.IsInitialized ? 0 : (Plugin.Ins.currentCameraMode == 2 ? 1 : 0)) == 0)
      return;
    FreeCamMenu.FreecamMenu.Draw();
    if (!Observation.ShowNests)
      return;
    NestsMenu.Draw();
  }

  public static void Initialize()
  {
    FreeCamMenu.FreecamMenu = new MenuBuilder("Freecam Options", 250f).AddSpace(25f).AddSlider("FOV:", Plugin.fov, 30f, 160f, (Action<float>) (value => Plugin.fov = value), description: "Adjust field of view").AddSlider("Zoom:", Plugin.zoomFov, 2f, 30f, (Action<float>) (value => Plugin.zoomFov = value), description: "Adjust zoom level when holding zoom key").AddSlider("Clipping Plane:", Plugin.clippingPlaneNear, 0.01f, 1f, (Action<float>) (value => Plugin.clippingPlaneNear = value), 2, "Adjust near clipping plane distance").AddSlider("Move Speed:", Plugin.moveSpeed, 0.1f, 30f, (Action<float>) (value => Plugin.moveSpeed = value), 1, "Adjust camera movement speed").AddSlider("Rotation Speed:", Plugin.rotationSpeed, 1f, 30f, (Action<float>) (value => Plugin.rotationSpeed = value), description: "Adjust camera rotation speed").AddDynamicToggle("Smoothing", (Func<bool>) (() => Plugin.freeCamSmoothing), (Action<bool>) (v =>
    {
      if ((!v ? 0 : ((double) Plugin.freeCamSmoothingSpeed < 14.0 ? 1 : 0)) != 0)
        Notification.Send("Mouse rotations may be too fast for your smoothing settings", Color.yellow);
      Plugin.freeCamSmoothing = v;
      FreeCamMenu.Reset();
    }), "Toggle camera movement smoothing");
    if (Plugin.freeCamSmoothing)
      FreeCamMenu.FreecamMenu.AddSlider("Smooth Speed:", Plugin.freeCamSmoothingSpeed, 0.1f, 20f, (Action<float>) (value => Plugin.freeCamSmoothingSpeed = value), 1, "Adjust camera interpolation speed");
    FreeCamMenu.FreecamMenu.AddDynamicToggle("Move audio", (Func<bool>) (() => Plugin.listenerBool), (Action<bool>) (v => Plugin.listenerBool = v), "Move audio listener with camera").AddDynamicToggle("Nests", (Func<bool>) (() => Observation.ShowNests), (Action<bool>) (_ => Observation.ToggleNestVisibility()), "Toggle visibility of observation nests and the Nests Options menu");
  }

  public static void Reset() => FreeCamMenu.FreecamMenu = (MenuBuilder) null;
}
