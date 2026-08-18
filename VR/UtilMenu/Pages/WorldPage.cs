using SakMerge.Api;
using SakuraaCastingMod.Features.World;
using SakuraaCastingMod.Shared.Models;
using System;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.VR.UtilMenu.Pages;

public class WorldPage : BasePage
{
  public override string PageName => "WORLD SETTINGS";

  public override Material PageIcon => UtilMenuMain.Instance.Icons.SnowCloud;

  public override void BuildTabs()
  {
    this.AddTab(UtilMenuMain.Instance.Icons.Options);
    UtilTab tab = this.Tabs[0];
    tab.Elements.Add(new MenuElement("TOGGLE RAIN", (Action) (() => WorldManager.ToggleWeather())));
    tab.Elements.Add(new MenuElement("TIME", "", (Action) (() =>
    {
      WorldManager.CurrentDayTime -= 250f;
      if ((double) WorldManager.CurrentDayTime < 0.0)
        WorldManager.CurrentDayTime = 0.0f;
      this.UpdateSlider();
      this.ApplyTime();
    }), (Action) (() =>
    {
      WorldManager.CurrentDayTime += 250f;
      if ((double) WorldManager.CurrentDayTime > 10000.0)
        WorldManager.CurrentDayTime = 10000f;
      this.UpdateSlider();
      this.ApplyTime();
    }))
    {
      Type = ElementType.Slider
    });
    tab.Elements.Add(new MenuElement("MORNING", (Action) (() => this.SetTime(2500f))));
    tab.Elements.Add(new MenuElement("NOON", (Action) (() => this.SetTime(5000f))));
    tab.Elements.Add(new MenuElement("EVENING", (Action) (() => this.SetTime(7500f))));
    tab.Elements.Add(new MenuElement("MIDNIGHT", (Action) (() => this.SetTime(0.0f))));
  }

  private void SetTime(float time)
  {
    WorldManager.CurrentDayTime = time;
    this.UpdateSlider();
    this.ApplyTime();
  }

  private void UpdateSlider()
  {
    if ((this.Tabs.Count <= 0 ? 0 : (this.Tabs[0].Elements.Count > 1 ? 1 : 0)) != 0)
      this.Tabs[0].Elements[1].ValueText = WorldManager.CurrentDayTime.ToString("F0");
    if (!((UnityEngine.Object) UtilMenuController.Instance != (UnityEngine.Object) null))
      return;
    UtilMenuController.Instance.RefreshUI();
  }

  private void ApplyTime() => TimeOfDay.Set(WorldManager.CurrentDayTime);
}
