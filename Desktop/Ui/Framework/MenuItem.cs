using System;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Desktop.Ui.Framework;

public abstract class MenuItem
{
  public string Label { get; set; }

  public string Description { get; set; }

  public Func<bool> IsVisible { get; set; } = (Func<bool>) (() => true);

  public float Height { get; set; } = MenuConfig.DefaultItemHeight;

  public bool Enabled { get; set; } = true;

  public abstract bool Draw(Rect rect);

  public virtual float Measure(float width) => this.Height;

  protected void DrawDescription(Rect rect, bool below = false)
  {
    if ((!MainMenus.ToolTipsEnabled ? 1 : (string.IsNullOrEmpty(this.Description) ? 1 : 0)) != 0 || !rect.Contains(Event.current.mousePosition))
      return;
    TooltipOverlay.Request(this.Description, GUIUtility.GUIToScreenPoint(Event.current.mousePosition), below);
  }
}
