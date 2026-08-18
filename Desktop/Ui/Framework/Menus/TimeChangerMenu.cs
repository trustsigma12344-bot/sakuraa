using SakMerge.Api;
using SakuraaCastingMod.Features.World;
using System;

#nullable disable
namespace SakuraaCastingMod.Desktop.Ui.Framework.Menus;

public static class TimeChangerMenu
{
  public static MenuBuilder _timeChangerMenu;

  public static void Draw()
  {
    if (TimeChangerMenu._timeChangerMenu == null)
      TimeChangerMenu.Initialize();
    TimeChangerMenu._timeChangerMenu?.Draw();
  }

  public static void Initialize()
  {
    TimeChangerMenu._timeChangerMenu = new MenuBuilder("Environment", 150f).SetPositionRef(300f, 100f).AddSlider("Time:", WorldManager.CurrentDayTime, 0.0f, 10000f, (Action<float>) (value => WorldManager.CurrentDayTime = value), description: "Adjust the time of day").AddButton("Apply", (Action) (() => TimeOfDay.Set(WorldManager.CurrentDayTime)), "Apply the selected time").AddDynamicToggle("Rain", (Func<bool>) (() => WorldManager.RainingWeather), (Action<bool>) (_ => WorldManager.ToggleWeather()), "Toggle rainy weather");
  }
}
