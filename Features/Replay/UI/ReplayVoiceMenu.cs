using SakuraaCastingMod.Desktop.Ui.Framework;
using SakuraaCastingMod.Features.Replay.ReplayJson;
using System;

#nullable disable
namespace SakuraaCastingMod.Features.Replay.UI;

internal static class ReplayVoiceMenu
{
  public static bool ShowMenu = false;
  private static MenuBuilder _menu;
  private static GorillaController _controller;
  private static bool _dirty = true;

  public static void Init(GorillaController controller) => ReplayVoiceMenu._controller = controller;

  public static void MarkDirty() => ReplayVoiceMenu._dirty = true;

  public static void Draw()
  {
    if (!ReplayVoiceMenu.ShowMenu)
      return;
    if (ReplayVoiceMenu._menu == null)
      ReplayVoiceMenu._menu = new MenuBuilder("Voice", 240f).SetPositionRef(1400f, 120f);
    if (ReplayVoiceMenu._dirty)
    {
      ReplayVoiceMenu.Rebuild();
      ReplayVoiceMenu._dirty = false;
    }
    ReplayVoiceMenu._menu.Draw();
  }

  private static void Rebuild()
  {
    ReplayVoiceMenu._menu.Items.Clear();
    ReplayVoiceMenu._menu.AddButton("Mute All", (Action) (() => VoiceEditManager.SetAllAt(true, ReplayVoiceMenu._controller.GetCurrentRealTime())), "Mute everyone from the current time");
    ReplayVoiceMenu._menu.AddButton("Unmute All", (Action) (() => VoiceEditManager.SetAllAt(false, ReplayVoiceMenu._controller.GetCurrentRealTime())), "Unmute everyone from the current time");
    ReplayVoiceMenu._menu.AddSpace(8f);
    foreach (Player player in VoiceEditManager.GetPlayers())
    {
      int actor = player.actornumber;
      string name = player.Name;
      ReplayVoiceMenu._menu.AddDynamicToggle(name, (Func<bool>) (() => !VoiceEditManager.IsActorMutedAt(actor, ReplayVoiceMenu._controller.GetCurrentRealTime())), (Action<bool>) (_ => VoiceEditManager.ToggleAt(actor, ReplayVoiceMenu._controller.GetCurrentRealTime())), $"Toggle {name}'s voice from the current time");
    }
  }
}
