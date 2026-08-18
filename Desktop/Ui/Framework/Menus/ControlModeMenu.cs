using SakuraaCastingMod.Core;
using SakuraaCastingMod.Desktop.ControlMode;
using SakuraaCastingMod.Desktop.ControlMode.Menus;
using System;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Desktop.Ui.Framework.Menus;

public static class ControlModeMenu
{
  public static MenuBuilder ControlOptionsMenu;

  public static bool IsInitialized => ControlModeMenu.ControlOptionsMenu != null;

  public static void Draw()
  {
    if (!ControlModeMenu.IsInitialized)
      ControlModeMenu.Initialize();
    if ((!ControlModeMenu.IsInitialized ? 0 : (Plugin.Ins.currentCameraMode == 3 ? 1 : 0)) == 0)
      return;
    ControlModeMenu.ControlOptionsMenu.Draw();
  }

  public static void Initialize()
  {
    ControlModeMenu.ControlOptionsMenu = new MenuBuilder("Control Options", 250f).AddSpace(25f).AddDynamicButton((Func<string>) (() => ((UnityEngine.Object) ControlModeGUI.Instance != (UnityEngine.Object) null) && ControlModeGUI.Instance.isInUse ? "Exit Computer [Escape]" : "Use Computer"), (Action) (() =>
    {
      if (!((UnityEngine.Object) ControlModeGUI.Instance != (UnityEngine.Object) null))
        return;
      ControlModeGUI.Instance.ToggleComputer();
    }), "Toggle using the in-game computer terminal you're standing near", (Func<bool>) (() => ((UnityEngine.Object) ControlModeGUI.Instance != (UnityEngine.Object) null) && ControlModeGUI.Instance.InRange)).AddLabel("Controls", true).AddLabel("Move: WASD\nFly Up/Down: Space / Ctrl\nLook: Mouse\nTurn Player: Right-Click + Mouse\nToggle Walk/Fly: Double Space").AddLabel("Modes (Num Keys)", true).AddLabel("1: Walk    2: Fly    3: Hand Pose").AddSection("Settings").AddSlider("FOV:", ControlModeSettings.Fov, 30f, 160f, (Action<float>) (value => ControlModeSettings.Fov = value), description: "Camera field of view").AddSlider("Clipping Plane:", ControlModeSettings.Clipping, 0.01f, 1f, (Action<float>) (value => ControlModeSettings.Clipping = value), 2, "Near clipping plane distance").AddSlider("Walk Speed:", ControlModeSettings.WalkSpeedMultiplier, 0.1f, 5f, (Action<float>) (value => ControlModeSettings.WalkSpeedMultiplier = value), 1, "Walk movement speed multiplier").AddSlider("Sprint Mult:", ControlModeSettings.SprintMultiplier, 1f, 5f, (Action<float>) (value => ControlModeSettings.SprintMultiplier = value), 1, "Extra speed multiplier while holding the sprint key").AddSlider("Fly Speed:", ControlModeSettings.FlySpeedMultiplier, 0.1f, 5f, (Action<float>) (value => ControlModeSettings.FlySpeedMultiplier = value), 1, "Fly movement speed multiplier (on top of scroll-wheel speed)").AddSlider("Look Sens:", ControlModeSettings.TurnSpeed, 0.1f, 5f, (Action<float>) (value => ControlModeSettings.TurnSpeed = value), 1, "Mouse / gamepad look sensitivity multiplier");
  }

  public static void Reset() => ControlModeMenu.ControlOptionsMenu = (MenuBuilder) null;
}
