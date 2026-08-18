using SakuraaCastingMod.Features.World;
using System;
using System.Linq;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Desktop.Ui.Framework.Menus;

public static class MapLoaderMenu
{
  public static MenuBuilder _mapLoaderMenu;

  public static void Draw()
  {
    WorldManager.CheckMaps();
    if (MapLoaderMenu._mapLoaderMenu == null)
      MapLoaderMenu.Initialize();
    MapLoaderMenu._mapLoaderMenu?.Draw();
  }

  public static void Initialize()
  {
    MapLoaderMenu._mapLoaderMenu = new MenuBuilder("Map Loader", 150f).SetPositionRef(100f, 300f).AddSpace();
    if ((WorldManager.RegionList == null ? 0 : (WorldManager.RegionList.Any<GorillaSetZoneTrigger>() ? 1 : 0)) != 0)
    {
      foreach (GorillaSetZoneTrigger region in WorldManager.RegionList)
      {
        GorillaSetZoneTrigger currentRegion = region;
        string[] strArray = ((UnityEngine.Object) ((Component) currentRegion).gameObject).name.Split(new string[1]
        {
          "To"
        }, StringSplitOptions.None);
        if (strArray.Length >= 2)
        {
          string label = strArray[1];
          MapLoaderMenu._mapLoaderMenu.AddButton(label, (Action) (() =>
          {
            ((GorillaTriggerBox) currentRegion).OnBoxTriggered();
            GameObject gameObject = GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom");
            if (!((UnityEngine.Object) gameObject != (UnityEngine.Object) null))
              return;
            gameObject.SetActive(true);
          }), $"Load the {label} map");
        }
      }
    }
    else
      MapLoaderMenu._mapLoaderMenu.AddLabel("No regions found", description: "Check map configuration");
  }
}
