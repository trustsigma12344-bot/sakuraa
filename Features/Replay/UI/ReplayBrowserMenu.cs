using SakuraaCastingMod.Desktop.Ui.Framework;
using SakuraaCastingMod.Features.Replay.ReplayManagers;
using System;
using System.Collections.Generic;

#nullable disable
namespace SakuraaCastingMod.Features.Replay.UI;

internal static class ReplayBrowserMenu
{
  public static bool ShowMenu = false;
  public static bool OverrideLimit = false;
  private static MenuBuilder _menu;
  private static GorillaController _controller;
  private static List<ReplayEntry> _entries = new List<ReplayEntry>();
  private static bool _dirty = true;
  private static bool _wasOpen = false;

  public static void Init(GorillaController controller)
  {
    ReplayBrowserMenu._controller = controller;
  }

  public static void MarkDirty() => ReplayBrowserMenu._dirty = true;

  public static void Draw()
  {
    if (ReplayBrowserMenu.ShowMenu)
    {
      if (ReplayBrowserMenu._menu == null)
        ReplayBrowserMenu._menu = new MenuBuilder("Replays", 280f).SetPositionRef(680f, 200f);
      if (!ReplayBrowserMenu._wasOpen)
      {
        ReplayBrowserMenu._dirty = true;
        ReplayBrowserMenu._wasOpen = true;
      }
      if (ReplayBrowserMenu._dirty)
      {
        ReplayBrowserMenu.RefreshAndRebuild();
        ReplayBrowserMenu._dirty = false;
      }
      ReplayBrowserMenu._menu.Draw();
    }
    else
      ReplayBrowserMenu._wasOpen = false;
  }

  private static void RefreshAndRebuild()
  {
    ReplayBrowserMenu._entries = ReplayLibrary.Scan();
    bool flag1;
    bool flag2 = !(flag1 = ReplayManager.replayProject != null) || ReplayBrowserMenu.OverrideLimit;
    ReplayBrowserMenu._menu.Items.Clear();
    if (flag2)
      ReplayBrowserMenu._menu.AddButton("Load From File", (Action) (() =>
      {
        ReplayBrowserMenu._controller.ReloadFile();
        if (ReplayManager.replayProject == null)
          return;
        ReplayBrowserMenu.ShowMenu = false;
      }), "Pick a .rawReplay from disk");
    ReplayBrowserMenu._menu.AddButton("Refresh", (Action) (() => ReplayBrowserMenu._dirty = true), "Re-scan the replays folder");
    if (flag1)
      ReplayBrowserMenu._menu.AddDynamicToggle("Override Replay Limit", (Func<bool>) (() => ReplayBrowserMenu.OverrideLimit), (Action<bool>) (v =>
      {
        ReplayBrowserMenu.OverrideLimit = v;
        ReplayBrowserMenu._dirty = true;
      }), "Allow loading another replay (only one replay is fully supported per session)");
    ReplayBrowserMenu._menu.AddSpace(8f);
    if (ReplayBrowserMenu._entries.Count == 0)
    {
      ReplayBrowserMenu._menu.AddLabel("No replays found, record a round first.");
    }
    else
    {
      foreach (ReplayEntry entry1 in ReplayBrowserMenu._entries)
      {
        ReplayEntry entry = entry1;
        ReplayLibrary.LoadDetails(entry);
        int durationSeconds = (int) entry.DurationSeconds;
        string str = $"{durationSeconds / 60}:{durationSeconds % 60:00}";
        ReplayBrowserMenu._menu.AddLabel(entry.DisplayName, true);
        ReplayBrowserMenu._menu.AddLabel($"{entry.DateText}    {entry.PlayerCount} players    {str}");
        if (flag2)
          ReplayBrowserMenu._menu.AddButton("Load", (Action) (() =>
          {
            ReplayBrowserMenu._controller.LoadReplayFromPath(entry.RawReplayPath);
            ReplayBrowserMenu.ShowMenu = false;
          }), "Play this replay");
        ReplayBrowserMenu._menu.AddHoldButton("Delete", 3f, (Action) (() =>
        {
          ReplayLibrary.Delete(entry);
          ReplayBrowserMenu._dirty = true;
        }), "Hold to permanently delete this replay");
        ReplayBrowserMenu._menu.AddSpace(8f);
      }
    }
  }
}
