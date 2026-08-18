using SakuraaCastingMod.Features.Overlays;
using SakuraaCastingMod.Features.Tools;
using System;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Desktop.Ui.Framework.Menus;

public static class RewindMenu
{
  private static MenuBuilder _rewindMenu;

  public static void Draw()
  {
    if (!RewindViewer.ShowEditorMenu)
      return;
    if (RewindMenu._rewindMenu == null)
      RewindMenu.Initialize();
    RewindMenu._rewindMenu?.Draw();
  }

  private static void Initialize()
  {
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    RewindMenu._rewindMenu = new MenuBuilder("Rewind", 200f).SetPositionRef(540f, 300f).AddSpace(15f).AddSlider("Speed:", RewindViewer.PlaybackSpeed, 0.1f, 2f, (Action<float>) (value => RewindViewer.PlaybackSpeed = value), 1, "Playback speed multiplier (0.1x - 2.0x)").AddSlider("Rewind Length:", RewindViewer.BufferLengthSeconds, 1f, 15f, (Action<float>) (value =>
    {
      float num = Mathf.Round(value);
      if ((double) Mathf.Abs(num - RewindViewer.BufferLengthSeconds) <= 0.5)
      {
        RewindViewer.BufferLengthSeconds = num;
      }
      else
      {
        RewindViewer.BufferLengthSeconds = num;
        RewindViewer.RebuildBuffer();
      }
    }), description: "Seconds of footage to keep in the buffer (1 - 15s)").AddDynamicButton((Func<string>) (() => $"Trigger Rewind [{Keybinds.RewindKey}]"), new Action(RewindViewer.Toggle), "Manually trigger the rewind playback");
  }

  public static void ToggleMenu() => RewindViewer.ShowEditorMenu = !RewindViewer.ShowEditorMenu;
}
