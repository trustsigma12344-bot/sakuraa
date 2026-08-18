using SakuraaCastingMod.Desktop.Ui.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Features.Replay.UI;

public class ButtonRowFlexContainer
{
  private float yPos;
  private float xPos;
  private float buttonWidth;
  private float buttonPadding;
  private float buttonHight;
  private List<ButtonRowFlexContainer.ButtonToAdd> buttonRenderList = new List<ButtonRowFlexContainer.ButtonToAdd>();

  public ButtonRowFlexContainer(
    float xPos,
    float yPos,
    float bWidth,
    float bPadding,
    float bHight)
  {
    this.xPos = xPos;
    this.yPos = yPos;
    this.buttonWidth = bWidth;
    this.buttonPadding = bPadding;
    this.buttonHight = bHight;
  }

  public void Add(string text, Action callback)
  {
    this.buttonRenderList.Add(new ButtonRowFlexContainer.ButtonToAdd()
    {
      buttonText = text,
      callback = callback
    });
  }

  public void RenderButtons()
  {
    float num1 = this.xPos - (0.0f + (float) this.buttonRenderList.Count * this.buttonWidth + (float) (this.buttonRenderList.Count - 1) * this.buttonPadding) / 2f;
    float num2 = this.buttonWidth + this.buttonPadding;
    for (int index = 0; index < this.buttonRenderList.Count; ++index)
    {
      if (GUI.Button(new Rect(num1 + num2 * (float) index, this.yPos, this.buttonWidth, this.buttonHight), this.buttonRenderList[index].buttonText, MenuConfig.GetButtonStyle()))
        this.buttonRenderList[index].callback();
    }
    this.buttonRenderList.Clear();
  }

  internal class ButtonToAdd
  {
    public string buttonText;
    public Action callback;
  }
}
