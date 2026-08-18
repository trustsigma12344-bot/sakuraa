using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Desktop.Ui.Framework;

public static class TooltipOverlay
{
  private static string _text;
  private static Vector2 _mouse;
  private static bool _below;
  private static bool _pending;

  public static void Request(string text, Vector2 screenMouse, bool below)
  {
    TooltipOverlay._text = text;
    TooltipOverlay._mouse = screenMouse;
    TooltipOverlay._below = below;
    TooltipOverlay._pending = true;
  }

  public static void Draw()
  {
    if (!TooltipOverlay._pending)
      return;
    TooltipOverlay._pending = false;
    float descriptionBoxWidth = MenuConfig.DescriptionBoxWidth;
    float descriptionBoxHeight = MenuConfig.DescriptionBoxHeight;
    float num1 = TooltipOverlay._mouse.x;
    float num2 = TooltipOverlay._below ? TooltipOverlay._mouse.y + 20f : (float) ((double) TooltipOverlay._mouse.y - (double) descriptionBoxHeight - 10.0);
    if ((!TooltipOverlay._below ? 0 : ((double) num2 + (double) descriptionBoxHeight > (double) Screen.height ? 1 : 0)) != 0)
      num2 = (float) ((double) TooltipOverlay._mouse.y - (double) descriptionBoxHeight - 10.0);
    else if ((TooltipOverlay._below ? 0 : ((double) num2 < 0.0 ? 1 : 0)) != 0)
      num2 = TooltipOverlay._mouse.y + 20f;
    if ((double) num1 + (double) descriptionBoxWidth > (double) Screen.width)
      num1 = (float) Screen.width - descriptionBoxWidth;
    if ((double) num1 < 0.0)
      num1 = 0.0f;
    GUI.Box(new Rect(num1, num2, descriptionBoxWidth, descriptionBoxHeight), TooltipOverlay._text, MenuConfig.GetDescriptionStyle());
  }
}
