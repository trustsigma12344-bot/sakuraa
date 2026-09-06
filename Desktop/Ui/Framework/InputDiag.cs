using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Desktop.Ui.Framework;

/// <summary>
/// Temporary click diagnostics. Traces a click from the raw IMGUI event through to the
/// control that claims it, so a single session shows exactly where a click is lost.
/// Draws nothing. Drags are ignored so they cannot flood the budget.
/// </summary>
public static class InputDiag
{
  public static bool Enabled = true;
  private const int MaxLines = 200;
  private static int _lines;
  private static bool _headerLogged;

  private static bool Budget()
  {
    if (!InputDiag.Enabled || InputDiag._lines >= 200)
      return false;
    ++InputDiag._lines;
    return true;
  }

  private static void Line(string text)
  {
    if (InputDiag.Budget())
      UnityEngine.Debug.Log((object) ("[SakDiag] " + text));
  }

  public static void NoteGlobal(Event e)
  {
    if (e == null || !InputDiag.Enabled)
      return;
    int type = (int) e.type;
    if (type != 0 && type != 1)
      return;
    if (!InputDiag._headerLogged)
    {
      InputDiag._headerLogged = true;
      InputDiag.Line("screen=" + Screen.width + "x" + Screen.height + " cursorLock=" + Cursor.lockState + " visible=" + Cursor.visible);
    }
    InputDiag.Line((type == 0 ? "-- MouseDown" : "-- MouseUp") + " btn=" + e.button + " at=" + e.mousePosition);
  }

  public static void NoteMenu(string title, Rect menuRect, Event e)
  {
    if (e == null || !InputDiag.Enabled || (int) e.type != 0)
      return;
    InputDiag.Line("   menu '" + title + "' rect=" + menuRect + " hit=" + menuRect.Contains(e.mousePosition));
  }

  public static void NoteHeaderDrag(string title)
  {
    InputDiag.Line("   HEADER DRAG started on '" + title + "' (consumes the event)");
  }

  public static void NoteArm(Rect rect)
  {
    InputDiag.Line("   ARM control rect=" + rect);
  }

  public static void NoteClick(Rect rect)
  {
    InputDiag.Line("   *** CLICK fired on control rect=" + rect);
  }

  public static void NoteLayout(string title, int visibleItems, bool minimized, int pages, float pagerHeight, Rect contentRect)
  {
    InputDiag.Line("   layout '" + title + "' items=" + visibleItems + " minimized=" + minimized + " pages=" + pages + " pagerH=" + pagerHeight + " contentRect=" + contentRect);
  }

  public static void NoteItem(int index, string type, Rect rect, Vector2 mouse)
  {
    InputDiag.Line("     item[" + index + "] " + type + " rect=" + rect + " contains=" + rect.Contains(mouse));
  }

  public static void NoteUpNoOwner()
  {
    InputDiag.Line("   MouseUp reached controls but no control was armed");
  }
}
