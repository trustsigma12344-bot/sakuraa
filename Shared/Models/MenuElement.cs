using System;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Shared.Models;

public class MenuElement
{
  public string Text;
  public ElementType Type;
  public Action OnClick;
  public Action OnToggle;
  public Action OnSliderLeft;
  public Action OnSliderRight;
  public Material CustomLeftIcon;
  public Material CustomRightIcon;
  public bool IsToggled;
  public string ValueText;
  public float HoldTime = 2f;

  public MenuElement(string text, Action onClick)
  {
    this.Text = text;
    this.Type = ElementType.Button;
    this.OnClick = onClick;
  }

  public MenuElement(string text, Action onClick, ElementType type)
  {
    this.Text = text;
    this.Type = type;
    this.OnClick = onClick;
  }

  public MenuElement(string text, Action onClick, float holdTime)
  {
    this.Text = text;
    this.Type = ElementType.HoldableButton;
    this.OnClick = onClick;
    this.HoldTime = holdTime;
  }

  public MenuElement(string text, Action onToggle, bool initialValue)
  {
    this.Text = text;
    this.Type = ElementType.Toggle;
    this.OnToggle = onToggle;
    this.IsToggled = initialValue;
  }

  public MenuElement(string text, string initialValue, Action onLeft, Action onRight)
  {
    this.Text = text;
    this.ValueText = initialValue;
    this.Type = ElementType.Slider;
    this.OnSliderLeft = onLeft;
    this.OnSliderRight = onRight;
  }
}
