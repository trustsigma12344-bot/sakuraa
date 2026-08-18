using SakuraaCastingMod.Core;
using SakuraaCastingMod.Features.Visuals;
using System;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Desktop.Ui.Framework.Menus;

public static class NameTagsMenu
{
  public static MenuBuilder Menu;
  private const float MenuWidth = 240f;

  public static void Draw()
  {
    if (NameTagsMenu.Menu == null)
      NameTagsMenu.Initialize();
    NameTagsMenu.Menu?.Draw();
  }

  public static void Initialize()
  {
    if (((UnityEngine.Object) Plugin.Ins == (UnityEngine.Object) null))
      UnityEngine.Debug.LogError((object) "Cannot initialize NameTagsMenu: Plugin.Ins is null.");
    else
      NameTagsMenu.Menu = new MenuBuilder("NameTag Options", 240f).SetPositionRef(250f, 100f).AddSpace(25f).AddDynamicButton((Func<string>) (() => "Font: " + NameTags.CurrentNameFont), (Action) (() => NameTags.SwitchNameFont()), "Cycle through available name tag fonts (Default, GTag, Pixel, etc.)").AddDynamicToggle("Self Tags", (Func<bool>) (() => NameTags.SelfNameTagVisible), (Action<bool>) (v =>
      {
        NameTags.SelfNameTagVisible = v;
        NameTags.ClearNameTags();
      }), "Toggle the visibility of your own nametag").AddDynamicToggle("Hide Spec Tag", (Func<bool>) (() => NameTags.HideSpecTagEnabled), (Action<bool>) (v =>
      {
        NameTags.HideSpecTagEnabled = v;
        if (!NameTags.HideSpecTagEnabled)
          NameTags.HideSpecTagFirstPersonOnly = false;
        NameTags.ClearNameTags();
      }), "Hide the nametag of the player you are spectating").AddDynamicToggle("FP Only", (Func<bool>) (() => NameTags.HideSpecTagFirstPersonOnly), (Action<bool>) (v =>
      {
        NameTags.HideSpecTagFirstPersonOnly = v;
        NameTags.ClearNameTags();
      }), "Only hide the spectated player's nametag in first person (eyes) view", (Func<bool>) (() => NameTags.HideSpecTagEnabled)).AddDynamicToggle("Ranks", (Func<bool>) (() => RankVisuals.RankCheckOnNamesEnabled), (Action<bool>) (v => RankVisuals.RankCheckOnNamesEnabled = v), "Show or hide pack-tier rank icons above name tags. Only appears for players also running the mod.").AddDynamicToggle("Platforms", (Func<bool>) (() => RankVisuals.PlatCheckOnNamesEnabled), (Action<bool>) (v => RankVisuals.PlatCheckOnNamesEnabled = v), "Show a Steam/Meta platform icon next to each name tag.").AddDynamicToggle("Show Hz", (Func<bool>) (() => RankVisuals.ShowHzOnName), (Action<bool>) (v => RankVisuals.ShowHzOnName = v), "Show each player's estimated refresh rate (Hz) on their name tag.").AddDynamicToggle("Auto Color Hz", (Func<bool>) (() => RankVisuals.AutoColorHzTag), (Action<bool>) (v => RankVisuals.AutoColorHzTag = v), "Color the Hz tag automatically based on the value.", (Func<bool>) (() => RankVisuals.ShowHzOnName)).AddSlider("Scale:", NameTags.NameTagScale, 0.1f, 2f, (Action<float>) (value => NameTags.NameTagScale = value), 1, "Adjust the size of the name tags. Requires 'Apply'.").AddSlider("Position:", NameTags.NameTagPosition, 0.1f, 3f, (Action<float>) (value => NameTags.NameTagPosition = value), 1, "Adjust the vertical offset of name tags above players. Requires 'Apply'.").AddButton("Apply", (Action) (() => NameTags.ClearNameTags()), "Apply scale and position changes by recreating all name tags.");
  }

  public static void ResetMenu() => NameTagsMenu.Menu = (MenuBuilder) null;
}
