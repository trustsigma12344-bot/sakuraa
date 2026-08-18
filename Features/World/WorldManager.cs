using SakuraaCastingMod.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Features.World;

public static class WorldManager
{
  [SavedSetting("ShowTimeChangerMenu", false)]
  public static bool ShowTimeChangerMenu;
  [SavedSetting("ShowMapLoader", false)]
  public static bool ShowMapLoader;
  public static float CurrentDayTime;
  public static bool RainingWeather;
  public static List<GorillaSetZoneTrigger> RegionList;
  public static BetterDayNightManager Bdnm;

  public static void ToggleWeather()
  {
    if (((UnityEngine.Object) BetterDayNightManager.instance == (UnityEngine.Object) null))
      return;
    for (int index = 1; index < BetterDayNightManager.instance.weatherCycle.Length; ++index)
      BetterDayNightManager.instance.weatherCycle[index] = WorldManager.RainingWeather ? (BetterDayNightManager.WeatherType) 0 : (BetterDayNightManager.WeatherType) 1;
    WorldManager.RainingWeather = !WorldManager.RainingWeather;
  }

  public static void CheckMaps()
  {
    if (WorldManager.RegionList == null)
      WorldManager.RegionList = new List<GorillaSetZoneTrigger>();
    GameObject gameObject = GameObject.Find("Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab");
    if (((UnityEngine.Object) gameObject == (UnityEngine.Object) null))
      return;
    GorillaSetZoneTrigger[] componentsInChildren = gameObject.GetComponentsInChildren<GorillaSetZoneTrigger>();
    if (componentsInChildren == null)
      return;
    HashSet<string> stringSet = new HashSet<string>(WorldManager.RegionList.Select<GorillaSetZoneTrigger, string>((Func<GorillaSetZoneTrigger, string>) (r => ExtraTools.CustomSplit(((UnityEngine.Object) ((Component) r).gameObject).name, "To")[1])));
    foreach (GorillaSetZoneTrigger gorillaSetZoneTrigger in componentsInChildren)
    {
      if ((((UnityEngine.Object) gorillaSetZoneTrigger == (UnityEngine.Object) null) || ((UnityEngine.Object) ((Component) gorillaSetZoneTrigger).gameObject == (UnityEngine.Object) null) ? 1 : (!((UnityEngine.Object) ((Component) gorillaSetZoneTrigger).gameObject).name.Contains("To") ? 1 : 0)) == 0)
      {
        string[] strArray = ExtraTools.CustomSplit(((UnityEngine.Object) ((Component) gorillaSetZoneTrigger).gameObject).name, "To");
        if (strArray.Length >= 2)
        {
          string str = strArray[1];
          if (!stringSet.Contains(str))
          {
            stringSet.Add(str);
            WorldManager.RegionList.Add(gorillaSetZoneTrigger);
          }
        }
      }
    }
  }
}
