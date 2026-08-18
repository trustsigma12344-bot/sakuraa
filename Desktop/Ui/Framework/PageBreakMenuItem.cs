using System;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Desktop.Ui.Framework;

internal class PageBreakMenuItem : MenuItem
{
  public readonly string PageTitle;

  public PageBreakMenuItem(string title)
  {
    this.PageTitle = title;
    this.Height = 0.0f;
    this.IsVisible = (Func<bool>) (() => false);
  }

  public override bool Draw(Rect rect) => false;
}
