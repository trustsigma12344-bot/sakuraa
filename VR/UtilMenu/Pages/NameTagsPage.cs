using SakuraaCastingMod.Core;
using SakuraaCastingMod.Features.Overlays;
using SakuraaCastingMod.Features.Visuals;
using SakuraaCastingMod.Shared.Models;
using System;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.VR.UtilMenu.Pages;

public class NameTagsPage : BasePage
{
  public override string PageName => "NAMETAGS";

  public override Material PageIcon => UtilMenuMain.Instance.Icons.LightningShockHead;

  public override void BuildTabs()
  {
    this.AddTab(UtilMenuMain.Instance.Icons.Paintbrush, new MenuElement("NAMETAGS: " + (NameTags.NameTagsEnabled ? "ON" : "OFF"), (Action) (() =>
    {
      NameTags.ToggleNameTagVisibility();
      if ((this.Tabs.Count <= 0 ? 0 : (this.Tabs[0].Elements.Count > 0 ? 1 : 0)) != 0)
        this.Tabs[0].Elements[0].Text = "NAMETAGS: " + (NameTags.NameTagsEnabled ? "ON" : "OFF");
      if ((this.Tabs.Count <= 1 ? 0 : (this.Tabs[1].Elements.Count > 0 ? 1 : 0)) != 0)
      {
        this.Tabs[1].Elements[0].Text = this.Tabs[0].Elements[0].Text;
        this.Tabs[1].Elements[0].IsToggled = NameTags.NameTagsEnabled;
      }
      UtilMenuController.Instance.RefreshUI();
    }), NameTags.NameTagsEnabled)
    {
      Type = ElementType.Toggle
    }, new MenuElement("FONT: " + NameTags.CurrentNameFont.ToUpper(), (Action) (() =>
    {
      NameTags.SwitchNameFont();
      if ((this.Tabs.Count <= 0 ? 0 : (this.Tabs[0].Elements.Count > 1 ? 1 : 0)) != 0)
        this.Tabs[0].Elements[1].Text = "FONT: " + NameTags.CurrentNameFont.ToUpper();
      UtilMenuController.Instance.RefreshUI();
    })), new MenuElement("SELF TAGS", (Action) (() =>
    {
      NameTags.SelfNameTagVisible = !NameTags.SelfNameTagVisible;
      NameTags.ClearNameTags();
    }), NameTags.SelfNameTagVisible)
    {
      Type = ElementType.Toggle
    }, new MenuElement("HIDE SPEC TAG", (Action) (() =>
    {
      NameTags.HideSpecTagEnabled = !NameTags.HideSpecTagEnabled;
      if (!NameTags.HideSpecTagEnabled)
        NameTags.HideSpecTagFirstPersonOnly = false;
      NameTags.ClearNameTags();
      UtilMenuController.Instance.RefreshUI();
    }), NameTags.HideSpecTagEnabled)
    {
      Type = ElementType.Toggle
    }, new MenuElement("PLATFORMS", (Action) (() =>
    {
      if (Plugin.Ins.XPosition != 0)
      {
        RankVisuals.PlatCheckOnNamesEnabled = !RankVisuals.PlatCheckOnNamesEnabled;
      }
      else
      {
        Notification.Send("You need a higher subscription tier to use this!", Color.red);
        if ((this.Tabs.Count <= 0 ? 0 : (this.Tabs[0].Elements.Count > 4 ? 1 : 0)) != 0)
          this.Tabs[0].Elements[4].IsToggled = false;
      }
      UtilMenuController.Instance.RefreshUI();
    }), RankVisuals.PlatCheckOnNamesEnabled)
    {
      Type = ElementType.Toggle
    }, new MenuElement("FPS / HZ", (Action) null, RankVisuals.ShowHzOnName)
    {
      Type = ElementType.Toggle,
      OnToggle = (Action) (() =>
      {
        RankVisuals.ShowHzOnName = !RankVisuals.ShowHzOnName;
        UtilMenuController.Instance.RefreshUI();
      })
    });
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    this.AddTab(UtilMenuMain.Instance.Icons.Options, new MenuElement("NAMETAGS: " + (NameTags.NameTagsEnabled ? "ON" : "OFF"), (Action) (() =>
    {
      NameTags.ToggleNameTagVisibility();
      if ((this.Tabs.Count <= 1 ? 0 : (this.Tabs[1].Elements.Count > 0 ? 1 : 0)) != 0)
        this.Tabs[1].Elements[0].Text = "NAMETAGS: " + (NameTags.NameTagsEnabled ? "ON" : "OFF");
      if ((this.Tabs.Count <= 0 ? 0 : (this.Tabs[0].Elements.Count > 0 ? 1 : 0)) != 0)
      {
        this.Tabs[0].Elements[0].Text = this.Tabs[1].Elements[0].Text;
        this.Tabs[0].Elements[0].IsToggled = NameTags.NameTagsEnabled;
      }
      UtilMenuController.Instance.RefreshUI();
    }), NameTags.NameTagsEnabled)
    {
      Type = ElementType.Toggle
    }, new MenuElement("SCALE", NameTags.NameTagScale.ToString("F1"), (Action) (() =>
    {
      NameTags.NameTagScale = Mathf.Clamp(NameTags.NameTagScale - 0.1f, 0.1f, 2f);
      this.UpdateSliderText(1, 1, NameTags.NameTagScale);
    }), (Action) (() =>
    {
      NameTags.NameTagScale = Mathf.Clamp(NameTags.NameTagScale + 0.1f, 0.1f, 2f);
      this.UpdateSliderText(1, 1, NameTags.NameTagScale);
    })), new MenuElement("POSITION", NameTags.NameTagPosition.ToString("F1"), (Action) (() =>
    {
      NameTags.NameTagPosition = Mathf.Clamp(NameTags.NameTagPosition - 0.1f, 0.1f, 3f);
      this.UpdateSliderText(1, 2, NameTags.NameTagPosition);
    }), (Action) (() =>
    {
      NameTags.NameTagPosition = Mathf.Clamp(NameTags.NameTagPosition + 0.1f, 0.1f, 3f);
      this.UpdateSliderText(1, 2, NameTags.NameTagPosition);
    })), new MenuElement("APPLY CHANGES", new Action(NameTags.ClearNameTags)));
  }

  private void UpdateSliderText(int tabIndex, int elementIndex, float val)
  {
    if ((this.Tabs.Count <= tabIndex ? 0 : (this.Tabs[tabIndex].Elements.Count > elementIndex ? 1 : 0)) == 0)
      return;
    this.Tabs[tabIndex].Elements[elementIndex].ValueText = val.ToString("F1");
    UtilMenuController.Instance.RefreshUI();
  }
}
