using SakuraaCastingMod.Features.Visuals;
using System;

#nullable disable
namespace SakuraaCastingMod.Desktop.Ui.Framework.Menus;

public static class LeaderboardMenu
{
  public static MenuBuilder Menu;
  private const float MenuWidth = 220f;

  public static void Draw()
  {
    if (LeaderboardMenu.Menu == null)
      LeaderboardMenu.Initialize();
    LeaderboardMenu.Menu?.Draw();
  }

  public static void Initialize()
  {
    LeaderboardMenu.Menu = new MenuBuilder("Leaderboard Options", 220f).SetPositionRef(250f, 260f).AddSpace(15f).AddDynamicToggle("Ranks", (Func<bool>) (() => RankVisuals.RankCheckOnLeaderboardEnabled), (Action<bool>) (v => RankVisuals.RankCheckOnLeaderboardEnabled = v), "Show or hide pack-tier rank icons on the leaderboard cards. Only appears for players also running the mod.").AddDynamicToggle("Platforms", (Func<bool>) (() => RankVisuals.PlatCheckOnLeaderboardEnabled), (Action<bool>) (v => RankVisuals.PlatCheckOnLeaderboardEnabled = v), "Show a Steam/Meta platform icon on each leaderboard card.");
  }

  public static void ResetMenu() => LeaderboardMenu.Menu = (MenuBuilder) null;
}
