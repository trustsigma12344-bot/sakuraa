using SakuraaCastingMod.Shared.Helpers;
using System;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Desktop.Ui.Framework.MenuItems;

public class SwitchMenuItem : MenuItem
{
  private readonly AnimState _anim = new AnimState();

  public bool IsToggled { get; set; }

  public Action<bool> OnToggled { get; set; }

  public Func<bool> StateGetter { get; set; }

  public override bool Draw(Rect rect)
  {
    if (this.StateGetter != null)
      this.IsToggled = this.StateGetter();
    ChromeResult chromeResult = ControlChrome.Draw(rect, this._anim, enabled: this.Enabled, clickSound: this.IsToggled ? Sounds.subtleClickSfx : Sounds.boingSfx, toggledOn: new bool?(this.IsToggled));
    if (chromeResult.Clicked)
    {
      this.IsToggled = !this.IsToggled;
      Action<bool> onToggled = this.OnToggled;
      if (onToggled != null)
        onToggled(this.IsToggled);
    }
    Rect contentRect = chromeResult.ContentRect;
    Rect rect1;
    // ISSUE: explicit constructor call
    rect1 = new Rect((float) ((double) contentRect.xMax - 34.0 - 8.0), contentRect.y + (float) (((double) contentRect.height - 16.0) * 0.5), 34f, 16f);
    GUI.Label(new Rect(contentRect.x + 8f, contentRect.y, (float) ((double) contentRect.width - 34.0 - 20.0), contentRect.height), this.Label, MenuConfig.GetLabelStyle());
    if (Event.current.type == (EventType) 7)
    {
      float toggle = this._anim.Toggle;
      Color color = UiAnim.LerpColor(MenuConfig.SliderBgColor, MenuConfig.HoverColorNow, toggle);
      GUI.DrawTexture(rect1, (Texture) Texture2D.whiteTexture, (ScaleMode) 0, true, 0.0f, color, 0.0f, rect1.height * 0.5f);
      float num = 12f;
      GUI.DrawTexture(new Rect(Mathf.Lerp(rect1.x + 2f, (float) ((double) rect1.xMax - (double) num - 2.0), toggle), rect1.y + 2f, num, num), (Texture) Texture2D.whiteTexture, (ScaleMode) 0, true, 0.0f, Color.white, 0.0f, 6f);
    }
    this.DrawDescription(rect);
    return chromeResult.Clicked;
  }
}
