using SakuraaCastingMod.Core;
using SakuraaCastingMod.Desktop.Camera;
using SakuraaCastingMod.Shared.Helpers;
using System;

#nullable disable
namespace SakuraaCastingMod.Desktop.Ui.Framework.Menus;

public static class ObservationMenu
{
  public static MenuBuilder _observationMenu;

  public static bool IsInitialized => ObservationMenu._observationMenu != null;

  public static void Draw()
  {
    if (!ObservationMenu.IsInitialized)
      ObservationMenu.Initialize();
    if ((!ObservationMenu.IsInitialized ? 0 : (Plugin.Ins.currentCameraMode == 6 ? 1 : 0)) == 0)
      return;
    ObservationMenu._observationMenu.Draw();
  }

  public static void Initialize()
  {
    ObservationMenu._observationMenu = new MenuBuilder("Observation Options", 250f).AddSpace(25f).AddSlider("FOV:", Plugin.fov, 30f, 160f, (Action<float>) (value => Plugin.fov = value), description: "Adjust Field of View").AddSlider("Zoom:", Plugin.zoomFov, 2f, 30f, (Action<float>) (value => Plugin.zoomFov = value), description: "Adjust Zoom Level (when holding zoom key)").AddSlider("Clipping Plane:", Plugin.clippingPlaneNear, 0.01f, 1f, (Action<float>) (value => Plugin.clippingPlaneNear = value), 2, "Adjust Near Clipping Plane (ft)").AddSlider("Move Speed:", Observation.VerticalMoveSpeed, 0.1f, 15f, (Action<float>) (value => Observation.VerticalMoveSpeed = value), 1, "Adjust Vertical Movement Speed").AddDynamicToggle("Move Audio", (Func<bool>) (() => Plugin.listenerBool), (Action<bool>) (v => Plugin.listenerBool = v), "Toggle moving audio listener with the camera").AddSlider("Switch Cooldown:", Observation.MinSwitchInterval, 0.0f, 15f, (Action<float>) (value => Observation.MinSwitchInterval = value), description: "Minimum time between automatic player switches (sec)").AddDynamicToggle("Smooth Mode", (Func<bool>) (() => Observation.SmoothObservation), (Action<bool>) (v => Observation.SmoothObservation = v), "Toggle smooth camera movement interpolation").AddSlider("Position Lerp:", Observation.ObservationLerp, 0.1f, 5f, (Action<float>) (value => Observation.ObservationLerp = value), 1, "Adjust position smoothing factor (higher = slower)", (Func<bool>) (() => Observation.SmoothObservation)).AddSlider("Rotation Slerp:", Observation.ObservationSlerp, 0.1f, 5f, (Action<float>) (value => Observation.ObservationSlerp = value), 1, "Adjust rotation smoothing factor (higher = slower)", (Func<bool>) (() => Observation.SmoothObservation)).AddDynamicToggle("AutoPilot", (Func<bool>) (() => AutoPilot.AutoPilotEnabled), (Action<bool>) (v => AutoPilot.AutoPilotEnabled = v), "Toggle automatic camera switching between players").AddDynamicButton((Func<string>) (() => "Target: " + (AutoPilot.PickTaggers ? "Taggers" : "Runners")), (Action) (() => AutoPilot.PickTaggers = !AutoPilot.PickTaggers), "Choose whether AutoPilot targets tagged or non-tagged players", (Func<bool>) (() => AutoPilot.AutoPilotEnabled)).AddSlider("AP Cooldown:", AutoPilot.APCooldownInterval, 0.0f, 15f, (Action<float>) (value => AutoPilot.APCooldownInterval = value), description: "Time AutoPilot waits before switching targets (sec)", visibilityCondition: (Func<bool>) (() => AutoPilot.AutoPilotEnabled));
  }

  public static void Reset() => ObservationMenu._observationMenu = (MenuBuilder) null;
}
