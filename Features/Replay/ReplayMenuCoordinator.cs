using SakuraaCastingMod.Core;
using SakuraaCastingMod.Desktop.Ui;
using SakuraaCastingMod.Features.Replay.ReplayManagers;
using SakuraaCastingMod.Features.Replay.UI;

#nullable disable
namespace SakuraaCastingMod.Features.Replay;

internal static class ReplayMenuCoordinator
{
  public static void Cycle()
  {
    UiManager ui = ReplaySystemBase.Ui;
    if (ReplayManager.replayProject != null)
    {
      bool flag = ui != null && ui.UiON;
      bool showMainMenu = MainMenus.ShowMainMenu;
      if (!flag)
      {
        if (showMainMenu)
        {
          MainMenus.ShowMainMenu = false;
          if (ui == null)
            return;
          ui.UiON = true;
        }
        else
        {
          MainMenus.ShowMainMenu = true;
          if (ui == null)
            return;
          ui.UiON = false;
        }
      }
      else
      {
        if (ui != null)
          ui.UiON = false;
        MainMenus.ShowMainMenu = false;
      }
    }
    else
    {
      if (ui != null)
        ui.UiON = false;
      MainMenus.ShowMainMenu = !MainMenus.ShowMainMenu;
    }
  }

  public static void OpenReplayViewer()
  {
    if (!FeatureToggles.ShowReplaySystem || ReplaySystemBase.Ui == null)
      return;
    MainMenus.ShowMainMenu = false;
    ReplaySystemBase.Ui.UiON = true;
    ReplayBrowserMenu.ShowMenu = true;
  }
}
