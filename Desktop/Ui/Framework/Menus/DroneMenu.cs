using SakuraaCastingMod.Core;
using SakuraaCastingMod.Desktop.Camera;
using System;

#nullable disable
namespace SakuraaCastingMod.Desktop.Ui.Framework.Menus;

public static class DroneMenu
{
  public static MenuBuilder DroneOptionsMenu;

  public static bool IsInitialized => DroneMenu.DroneOptionsMenu != null;

  public static void Draw()
  {
    if (!DroneMenu.IsInitialized)
      DroneMenu.Initialize();
    if ((!DroneMenu.IsInitialized ? 0 : (Plugin.Ins.currentCameraMode == 4 ? 1 : 0)) == 0)
      return;
    DroneMenu.DroneOptionsMenu.Draw();
  }

  public static void Initialize()
  {
    DroneMenu.DroneOptionsMenu = new MenuBuilder("Drone Options", 250f).AddSpace(25f).AddSlider("Pitch Rate:", Drone.PitchRate, 0.2f, 3f, (Action<float>) (value => Drone.PitchRate = value), 1, "Pitch rotation speed multiplier").AddSlider("Roll Rate:", Drone.RollRate, 0.2f, 3f, (Action<float>) (value => Drone.RollRate = value), 1, "Roll rotation speed multiplier").AddSlider("Yaw Rate:", Drone.YawRate, 0.2f, 3f, (Action<float>) (value => Drone.YawRate = value), 1, "Yaw rotation speed multiplier").AddSlider("Expo:", Drone.ExpoAmount, 0.0f, 1f, (Action<float>) (value => Drone.ExpoAmount = value), 2, "Stick expo curve (0 = linear, 1 = full expo)").AddSlider("Stick Sens:", Drone.StickSensitivity, 0.2f, 2f, (Action<float>) (value => Drone.StickSensitivity = value), 1, "Controller stick sensitivity multiplier").AddSlider("Mouse Sens:", Drone.MouseSensitivity, 0.2f, 3f, (Action<float>) (value => Drone.MouseSensitivity = value), 1, "Mouse sensitivity multiplier").AddSlider("Cam Angle:", Drone.CameraAngle, 0.0f, 45f, (Action<float>) (value => Drone.CameraAngle = value), description: "Camera tilt angle in degrees (real drones use ~20)").AddSlider("FOV:", Drone.DroneFov, 60f, 140f, (Action<float>) (value => Drone.DroneFov = value), description: "Camera field of view").AddSlider("Volume:", Drone.DroneVolume, 0.0f, 1f, (Action<float>) (value => Drone.DroneVolume = value), 2, "Motor audio volume").AddDynamicButton((Func<string>) (() => "Respawn Drone"), (Action) (() =>
    {
      if (!Drone.DroneStarted)
        return;
      Plugin.Ins.ResetCameraObject();
      Drone.DroneStarted = false;
    }), "Reset and respawn the drone");
  }

  public static void Reset() => DroneMenu.DroneOptionsMenu = (MenuBuilder) null;
}
