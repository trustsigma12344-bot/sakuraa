using SakuraaCastingMod.Core;
using SakuraaCastingMod.Desktop.Camera;
using SakuraaCastingMod.Shared.Helpers;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Desktop.Ui.Framework.Menus;

public static class ModeMenus
{
  [SavedSetting("ModeMenuPosX", 100f)]
  public static float ModeMenuPosX = 100f;
  [SavedSetting("ModeMenuPosY", 100f)]
  public static float ModeMenuPosY = 100f;
  public const float SharedWidth = 250f;
  private static int _prevMode = -1;
  private static MenuBuilder _prevActive;

  public static void Init()
  {
    ModeMenus._prevMode = ((UnityEngine.Object) Plugin.Ins != (UnityEngine.Object) null) ? Plugin.Ins.currentCameraMode : -1;
  }

  public static void Draw()
  {
    int currentCameraMode = Plugin.Ins.currentCameraMode;
    MenuBuilder activeBuilder = ModeMenus.GetActiveBuilder(currentCameraMode);
    if (currentCameraMode != ModeMenus._prevMode)
    {
      if (activeBuilder != null)
      {
        if ((ModeMenus._prevActive == null ? 0 : (ModeMenus._prevActive != activeBuilder ? 1 : 0)) != 0)
          activeBuilder.SetAnimHeight(ModeMenus._prevActive.MenuRect.height);
        else
          activeBuilder.BeginIntro();
      }
      ModeMenus._prevMode = currentCameraMode;
      ModeMenus._prevActive = activeBuilder;
    }
    if (activeBuilder == null)
      return;
    activeBuilder.MenuRect.x = ModeMenus.ModeMenuPosX;
    activeBuilder.MenuRect.y = ModeMenus.ModeMenuPosY;
    activeBuilder.Draw();
    ModeMenus.ModeMenuPosX = activeBuilder.MenuRect.x;
    ModeMenus.ModeMenuPosY = activeBuilder.MenuRect.y;
    if ((currentCameraMode != 2 ? 0 : (Observation.ShowNests ? 1 : 0)) == 0)
      return;
    NestsMenu.Draw();
  }

  private static MenuBuilder GetActiveBuilder(int mode)
  {
    MenuBuilder activeBuilder;
    switch (mode)
    {
      case 2:
        if (!FreeCamMenu.IsInitialized)
          FreeCamMenu.Initialize();
        activeBuilder = FreeCamMenu.FreecamMenu;
        break;
      case 3:
        if (!ControlModeMenu.IsInitialized)
          ControlModeMenu.Initialize();
        activeBuilder = ControlModeMenu.ControlOptionsMenu;
        break;
      case 4:
        if (!DroneMenu.IsInitialized)
          DroneMenu.Initialize();
        activeBuilder = DroneMenu.DroneOptionsMenu;
        break;
      case 5:
        if (!PlayerSpecMenu.IsInitialized)
          PlayerSpecMenu.Initialize();
        activeBuilder = PlayerSpecMenu._playerSpecMenu;
        break;
      case 6:
        if (!ObservationMenu.IsInitialized)
          ObservationMenu.Initialize();
        activeBuilder = ObservationMenu._observationMenu;
        break;
      case 7:
        if (!DirectorMenu.IsInitialized)
          DirectorMenu.Initialize();
        activeBuilder = DirectorMenu._directorMenu;
        break;
      default:
        activeBuilder = (MenuBuilder) null;
        break;
    }
    return activeBuilder;
  }
}
