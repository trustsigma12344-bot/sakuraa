using SakuraaCastingMod.Shared.Helpers;
using System;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Desktop.Ui.Framework.MenuItems;

public class ButtonMenuItem : MenuItem
{
  private string _staticLabel;
  private Func<string> _dynamicLabelGetter;
  private readonly AnimState _anim = new AnimState();

  public Action OnClick { get; set; }

  public new string Label
  {
    get => this._dynamicLabelGetter == null ? this._staticLabel : this._dynamicLabelGetter();
    set
    {
      this._staticLabel = value;
      this._dynamicLabelGetter = (Func<string>) null;
    }
  }

  public void SetDynamicLabel(Func<string> labelGetter)
  {
    this._dynamicLabelGetter = labelGetter;
    this._staticLabel = (string) null;
  }

  public override bool Draw(Rect rect)
  {
    ChromeResult chromeResult = ControlChrome.Draw(rect, this._anim, enabled: this.Enabled, clickSound: Sounds.boingSfx);
    GUI.Label(chromeResult.ContentRect, this.Label, MenuConfig.GetCenterTextStyle());
    if (chromeResult.Clicked)
    {
      Action onClick = this.OnClick;
      if (onClick != null)
        onClick();
    }
    this.DrawDescription(rect);
    return chromeResult.Clicked;
  }
}
