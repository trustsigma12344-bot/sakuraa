using System;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Desktop.Ui.Framework.MenuItems;

public class SliderMenuItem : MenuItem
{
  private readonly AnimState _anim = new AnimState();
  private bool _dragging;

  public float Value { get; set; }

  public float MinValue { get; set; }

  public float MaxValue { get; set; }

  public int Decimals { get; set; } = 0;

  public bool ShowValue { get; set; } = true;

  public Action<float> OnValueChanged { get; set; }

  public SliderMenuItem() => this.Height = MenuConfig.DefaultItemHeight * 1.8f;

  public override bool Draw(Rect rect)
  {
    GUI.Label(new Rect(rect.x + 8f, rect.y + 4f, rect.width - 64f, MenuConfig.DefaultItemHeight), this.Label, MenuConfig.GetLabelStyle());
    if (this.ShowValue)
    {
      string str = this.Decimals == 0 ? Mathf.RoundToInt(this.Value).ToString() : this.Value.ToString($"F{this.Decimals}");
      GUI.Label(new Rect((float) ((double) rect.x + (double) rect.width - 56.0), rect.y + 4f, 48f, MenuConfig.DefaultItemHeight), str, MenuConfig.GetValueStyle());
    }
    float num1 = (float) ((double) rect.y + (double) MenuConfig.DefaultItemHeight + 4.0);
    Rect track;
    // ISSUE: explicit constructor call
    track = new Rect(rect.x + 8f, num1 + 4f, rect.width - 16f, 6f);
    Rect rect1;
    // ISSUE: explicit constructor call
    rect1 = new Rect(rect.x + 4f, num1, rect.width - 8f, 18f);
    Event current = Event.current;
    bool hovered = rect1.Contains(current.mousePosition);
    bool flag = false;
    if (this.Enabled)
    {
      if (((current.type != null ? 0 : (current.button == 0 ? 1 : 0)) & (hovered ? 1 : 0)) != 0)
      {
        this._dragging = true;
        MenuBuilder.ActiveDragOwner = (object) this;
        this.ApplyFromMouse(track, current.mousePosition.x);
        flag = true;
        current.Use();
      }
      else if ((!this._dragging ? 0 : (current.type == (EventType) 3 ? 1 : 0)) != 0)
      {
        this.ApplyFromMouse(track, current.mousePosition.x);
        flag = true;
        current.Use();
      }
      else if ((!this._dragging ? 0 : (current.type == (EventType) 1 ? 1 : 0)) != 0)
      {
        this._dragging = false;
        if (MenuBuilder.ActiveDragOwner == this)
          MenuBuilder.ActiveDragOwner = (object) null;
        current.Use();
      }
    }
    this._anim.Advance(hovered, this._dragging);
    if (current.type == (EventType) 7)
    {
      float f = Mathf.Approximately(this.MaxValue, this.MinValue) ? 0.0f : Mathf.InverseLerp(this.MinValue, this.MaxValue, this.Value);
      if (float.IsNaN(f))
        f = 0.0f;
      float num2 = track.height * 0.5f;
      GUI.DrawTexture(track, (Texture) Texture2D.whiteTexture, (ScaleMode) 0, true, 0.0f, MenuConfig.SliderBgColor, 0.0f, num2);
      if ((double) f > 0.0)
        GUI.DrawTexture(new Rect(track.x, track.y, track.width * f, track.height), (Texture) Texture2D.whiteTexture, (ScaleMode) 0, true, 0.0f, MenuConfig.HoverColorNow, 0.0f, num2);
      float num3 = 14f * (float) (1.0 + 0.34999999403953552 * (double) Mathf.Max(this._anim.Hover, this._anim.Press));
      float num4 = Mathf.Lerp(track.x, track.xMax - num3, f);
      Rect rect2;
      // ISSUE: explicit constructor call
      rect2 = new Rect(num4, (float) ((double) track.y + (double) track.height * 0.5 - (double) num3 * 0.5), num3, num3);
      GUI.DrawTexture(rect2, (Texture) Texture2D.whiteTexture, (ScaleMode) 0, true, 0.0f, Color.white, 0.0f, num3 * 0.5f);
    }
    if (flag)
    {
      Action<float> onValueChanged = this.OnValueChanged;
      if (onValueChanged != null)
        onValueChanged(this.Value);
    }
    this.DrawDescription(rect, true);
    return flag;
  }

  private void ApplyFromMouse(Rect track, float mouseX)
  {
    float num1 = Mathf.Lerp(this.MinValue, this.MaxValue, Mathf.Clamp01(Mathf.InverseLerp(track.x, track.xMax, mouseX)));
    float num2 = Mathf.Pow(10f, (float) -this.Decimals);
    float num3 = Mathf.Min(this.MinValue, this.MaxValue);
    float num4 = Mathf.Max(this.MinValue, this.MaxValue);
    this.Value = Mathf.Clamp(Mathf.Round(num1 / num2) * num2, num3, num4);
  }
}
