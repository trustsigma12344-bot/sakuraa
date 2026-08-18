using SakuraaCastingMod.Shared.Helpers;
using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Desktop.Ui.Framework.MenuItems;

public class DropdownMenuItem : MenuItem
{
  private readonly AnimState _anim = new AnimState();

  public string LabelPrefix { get; set; }

  public Func<IList<string>> GetOptions { get; set; }

  public Func<int> GetSelectedIndex { get; set; }

  public Action<int> OnSelected { get; set; }

  public override bool Draw(Rect rect)
  {
    Func<IList<string>> getOptions = this.GetOptions;
    IList<string> stringList = getOptions != null ? getOptions() : (IList<string>) null;
    Func<int> getSelectedIndex = this.GetSelectedIndex;
    int index = getSelectedIndex != null ? getSelectedIndex() : -1;
    string str1 = stringList == null || index < 0 || index >= stringList.Count ? "-" : stringList[index];
    string str2 = string.IsNullOrEmpty(this.LabelPrefix) ? str1 + " ▼" : $"{this.LabelPrefix}: {str1} ▼";
    ChromeResult chromeResult = ControlChrome.Draw(rect, this._anim, enabled: this.Enabled, clickSound: Sounds.subtleClickSfx);
    GUI.Label(chromeResult.ContentRect, str2, MenuConfig.GetCenterTextStyle());
    if (chromeResult.Clicked)
    {
      if (!DropdownPopup.IsOpenFor(this))
      {
        Vector2 screenPoint = GUIUtility.GUIToScreenPoint(new Vector2(rect.x, rect.y));
        DropdownPopup.Open(this, new Rect(screenPoint.x, screenPoint.y, rect.width, rect.height));
      }
      else
        DropdownPopup.Close();
    }
    this.DrawDescription(rect);
    return chromeResult.Clicked;
  }
}
