using SakuraaCastingMod.Shared.Helpers;
using System;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Desktop.Ui.Framework.MenuItems;

public class HoldButtonMenuItem : MenuItem
{
  private bool _holding;
  private float _elapsed;
  private double _lastFrameTime;
  private readonly AnimState _anim = new AnimState();

  public string BaseLabel { get; set; }

  public float HoldDuration { get; set; } = 3f;

  public Action OnHoldComplete { get; set; }

  public override bool Draw(Rect rect)
  {
    Event current = Event.current;
    bool flag1 = rect.Contains(current.mousePosition);
    if (((((int) current.type != 0 ? 0 : (current.button == 0 ? 1 : 0)) & (flag1 ? 1 : 0)) == 0 ? 0 : (this.Enabled ? 1 : 0)) != 0)
    {
      this._holding = true;
      this._elapsed = 0.0f;
      this._lastFrameTime = Time.realtimeSinceStartupAsDouble;
      current.Use();
    }
    else if (this._holding && (current.type == (EventType) 1 ? 1 : (flag1 ? 0 : (current.type == (EventType) 3 ? 1 : 0))) != 0)
    {
      this._holding = false;
      this._elapsed = 0.0f;
    }
    bool flag2 = false;
    if ((!this._holding ? 0 : (current.type == (EventType) 7 ? 1 : 0)) != 0)
    {
      double sinceStartupAsDouble = Time.realtimeSinceStartupAsDouble;
      this._elapsed += (float) (sinceStartupAsDouble - this._lastFrameTime);
      this._lastFrameTime = sinceStartupAsDouble;
      if ((double) this._elapsed >= (double) this.HoldDuration)
      {
        this._holding = false;
        this._elapsed = 0.0f;
        flag2 = true;
        Sounds.PlayCasterClick(Sounds.boingSfx);
        Action onHoldComplete = this.OnHoldComplete;
        if (onHoldComplete != null)
          onHoldComplete();
      }
    }
    ControlChrome.DrawSurface(rect, this._anim, flag1 && this.Enabled, this._holding, this.Enabled);
    if (current.type == (EventType) 7)
    {
      float num = this._holding ? Mathf.Clamp01(this._elapsed / this.HoldDuration) : 0.0f;
      if ((double) num > 0.0)
      {
        Color activeColorNow = MenuConfig.ActiveColorNow;
        activeColorNow.a = num;
        ControlChrome.FillRect(rect, activeColorNow);
      }
    }
    string str = this._holding ? $"{this.BaseLabel} [{Mathf.CeilToInt(this.HoldDuration - this._elapsed)}]" : this.BaseLabel + " [HOLD]";
    GUI.Label(rect, str, MenuConfig.GetCenterTextStyle());
    this.DrawDescription(rect);
    return flag2;
  }
}
