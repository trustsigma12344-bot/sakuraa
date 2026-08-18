using SakuraaCastingMod.Desktop.Ui.Framework;
using SakuraaCastingMod.Features.Replay.CustomCamera;
using SakuraaCastingMod.Features.Replay.ReplayManagers;
using SakuraaCastingMod.Features.Tools;
using System;

#nullable disable
namespace SakuraaCastingMod.Features.Replay.UI;

internal static class ReplayToolsMenu
{
  public static bool ShowMenu;
  private static MenuBuilder _menu;
  private static GorillaController _controller;

  public static void Init(GorillaController controller) => ReplayToolsMenu._controller = controller;

  public static void Draw()
  {
    if (!ReplayToolsMenu.ShowMenu)
      return;
    if (ReplayToolsMenu._menu == null)
    {
      ReplayToolsMenu._menu = new MenuBuilder("Replay Tools", 250f).SetPositionRef(1060f, 120f);
      ReplayToolsMenu.Build();
    }
    ReplayToolsMenu._menu.Draw();
  }

  private static void Build()
  {
    ReplayToolsMenu._menu.AddLabel("Playback", true);
    ReplayToolsMenu._menu.AddDynamicButton((Func<string>) (() => (GorillaController.Paused ? "Play" : "Pause") + $"  [{Keybinds.RpPauseKey}]"), (Action) (() => GorillaController.Paused = !GorillaController.Paused), "Play or pause the replay (scrub with the arrow keys)");
    ReplayToolsMenu._menu.AddDynamicLabel((Func<string>) (() => $"Time: {ReplayToolsMenu._controller.GetCurrentRealTime():0.0}s / {ReplayManager.FindGreatestEndTime():0.0}s"));
    ReplayToolsMenu._menu.AddSlider("Speed:", ReplayToolsMenu._controller.SlowValue, 0.1f, 3f, (Action<float>) (v => ReplayToolsMenu._controller.SlowValue = v), 1, "Playback speed multiplier");
    ReplayToolsMenu._menu.AddSection("History");
    ReplayToolsMenu._menu.AddButton("Undo  [Ctrl+Z]", (Action) (() => ReplayHistory.Undo()), "Undo the last keyframe change");
    ReplayToolsMenu._menu.AddButton("Redo  [Ctrl+Shift+Z]", (Action) (() => ReplayHistory.Redo()), "Redo the last undone change");
    ReplayToolsMenu._menu.AddSection("Camera");
    ReplayToolsMenu._menu.AddDynamicButton((Func<string>) (() => (CameraController.UseKeyCustomCamera ? "Exit Camera" : "Enter Camera") + $"  [{Keybinds.RpToggleCameraKey}]"), (Action) (() => CameraController.UseKeyCustomCamera = !CameraController.UseKeyCustomCamera), "Toggle the cinematic replay camera that follows your camera keyframes");
    ReplayToolsMenu._menu.AddDynamicButton((Func<string>) (() => $"Add Camera Keyframe  [{Keybinds.RpAddCameraKey}]"), (Action) (() =>
    {
      if (CameraController.instance == null)
        return;
      CameraController.instance.AddCamera();
    }), "Snapshot the current camera position/rotation as a keyframe at the current time");
    ReplayToolsMenu._menu.AddSection("Speed Keyframes");
    ReplayToolsMenu._menu.AddDynamicButton((Func<string>) (() => $"Add Speed Keyframe  [{Keybinds.RpAddSpeedKey}]"), (Action) (() => SpeedManager.AddSpeedFrame(ReplayToolsMenu._controller.GetCurrentRealTime())), "Add a slow-motion / speed-up keyframe at the current time");
    ReplayToolsMenu._menu.AddSection("Project");
    ReplayToolsMenu._menu.AddDynamicButton((Func<string>) (() => $"Save Replay  [{Keybinds.RpSaveKey}]"), (Action) (() => ReplayManager.SaveProject()), "Save your camera, speed and voice edits to this replay");
  }
}
