using SakuraaCastingMod.Shared.Helpers;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Desktop.Ui.Framework;

public static class ControlChrome
{
  private static AnimState _armed;
  public static ChromeResult Draw(
    Rect rect,
    AnimState anim,
    bool interactable = true,
    bool enabled = true,
    AudioClip clickSound = null,
    bool? toggledOn = null)
  {
    Event current = Event.current;
    bool hovered = enabled && rect.Contains(current.mousePosition);
    bool flag = false;
    if (interactable & enabled)
    {
      if ((((int) current.type != 0 ? 0 : (current.button == 0 ? 1 : 0)) & (hovered ? 1 : 0)) != 0)
      {
        ControlChrome._armed = anim;
        InputDiag.NoteArm(rect);
        current.Use();
      }
      else if ((current.type != (EventType) 1 || current.button != 0 ? 0 : (ControlChrome._armed == anim ? 1 : 0)) != 0)
      {
        if (hovered)
          flag = true;
        if (flag)
          InputDiag.NoteClick(rect);
        ControlChrome._armed = (AnimState) null;
        current.Use();
      }
    }
    bool pressed = ((!(interactable & enabled) ? 0 : (ControlChrome._armed == anim ? 1 : 0)) & (hovered ? 1 : 0)) != 0;
    anim.Advance(hovered, pressed, toggledOn);
    if (current.type == (EventType) 7)
    {
      if (((!hovered ? 0 : (!anim.HoverLatch ? 1 : 0)) & (enabled ? 1 : 0)) != 0)
        Sounds.PlayCasterClick(Sounds.subtleClickSfx, 0.04f);
      anim.HoverLatch = hovered;
    }
    if ((!flag ? 0 : (((UnityEngine.Object) clickSound != (UnityEngine.Object) null) ? 1 : 0)) != 0)
      Sounds.PlayCasterClick(clickSound);
    Rect r = UiAnim.ScaleRectCentered(rect, Mathf.Lerp(1f, 0.97f, anim.Press));
    if (current.type == (EventType) 7)
      ControlChrome.DrawLayers(r, anim.Hover, anim.Press, enabled);
    return new ChromeResult()
    {
      Hovered = hovered,
      Pressed = pressed,
      Clicked = flag,
      Hover01 = anim.Hover,
      Press01 = anim.Press,
      ContentRect = r
    };
  }

  public static void DrawSurface(
    Rect rect,
    AnimState anim,
    bool hovered,
    bool pressed,
    bool enabled = true)
  {
    anim.Advance(hovered, pressed);
    if (Event.current.type != (EventType) 7)
      return;
    ControlChrome.DrawLayers(UiAnim.ScaleRectCentered(rect, Mathf.Lerp(1f, 0.97f, anim.Press)), anim.Hover, anim.Press, enabled);
  }

  private static void DrawLayers(Rect r, float hover, float press, bool enabled)
  {
    Color color = GUI.color;
    GUIStyle fillBoxStyle = MenuConfig.GetFillBoxStyle();
    Color fillColorNow = MenuConfig.FillColorNow;
    if (!enabled)
      fillColorNow.a *= 0.5f;
    GUI.color = fillColorNow;
    GUI.Box(r, GUIContent.none, fillBoxStyle);
    GUI.color = Color.white;
    GUI.DrawTexture(r, (Texture) MenuConfig.GetGradientTexture());
    if ((double) hover > 1.0 / 1000.0)
    {
      Color hoverColorNow = MenuConfig.HoverColorNow;
      hoverColorNow.a *= hover * 0.9f;
      GUI.color = hoverColorNow;
      GUI.Box(r, GUIContent.none, fillBoxStyle);
    }
    if ((double) press > 1.0 / 1000.0)
    {
      Color activeColorNow = MenuConfig.ActiveColorNow;
      activeColorNow.a *= press;
      GUI.color = activeColorNow;
      GUI.Box(r, GUIContent.none, fillBoxStyle);
    }
    GUI.color = MenuConfig.OutlineColorNow;
    GUI.Box(r, GUIContent.none, MenuConfig.GetOutlineBoxStyle());
    GUI.color = color;
  }

  public static void FillRect(Rect r, Color color)
  {
    Color color1 = GUI.color;
    GUI.color = color;
    GUI.Box(r, GUIContent.none, MenuConfig.GetFillBoxStyle());
    GUI.color = color1;
  }
}
