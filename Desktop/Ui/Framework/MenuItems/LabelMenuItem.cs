using System;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Desktop.Ui.Framework.MenuItems;

public class LabelMenuItem : MenuItem
{
  private string _staticLabel;
  private Func<string> _dynamicLabelGetter;

  public new string Label
  {
    get => this._dynamicLabelGetter == null ? this._staticLabel : this._dynamicLabelGetter();
    set
    {
      this._staticLabel = value;
      this._dynamicLabelGetter = (Func<string>) null;
    }
  }

  public bool IsHeader { get; set; } = false;

  public void SetDynamicLabel(Func<string> labelGetter)
  {
    this._dynamicLabelGetter = labelGetter;
    this._staticLabel = (string) null;
  }

  public override float Measure(float width)
  {
    string label = this.Label;
    return !string.IsNullOrEmpty(label) ? Mathf.Max(this.Height, (this.IsHeader ? MenuConfig.GetHeaderStyle() : MenuConfig.GetWrapLabelStyle()).CalcHeight(new GUIContent(label), width)) : this.Height;
  }

  public override bool Draw(Rect rect)
  {
    GUIStyle guiStyle = this.IsHeader ? MenuConfig.GetHeaderStyle() : MenuConfig.GetWrapLabelStyle();
    GUI.Label(rect, this.Label, guiStyle);
    this.DrawDescription(rect);
    return false;
  }
}
