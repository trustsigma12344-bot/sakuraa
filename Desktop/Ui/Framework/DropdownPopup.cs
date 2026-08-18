using SakuraaCastingMod.Desktop.Ui.Framework.MenuItems;
using SakuraaCastingMod.Shared.Helpers;
using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Desktop.Ui.Framework;

public static class DropdownPopup
{
  private static DropdownMenuItem _active;
  private static Rect _anchor;
  private static readonly List<AnimState> _rowAnims = new List<AnimState>();

  public static bool IsOpen => DropdownPopup._active != null;

  public static bool IsOpenFor(DropdownMenuItem dd) => DropdownPopup._active == dd;

  public static void Open(DropdownMenuItem dd, Rect anchor)
  {
    DropdownPopup._active = dd;
    DropdownPopup._anchor = anchor;
    DropdownPopup._rowAnims.Clear();
  }

  public static void Close() => DropdownPopup._active = (DropdownMenuItem) null;

  public static void HandleInput()
  {
    if (DropdownPopup._active == null)
      return;
    Event current = Event.current;
    if (current.type > 0)
      return;
    Func<IList<string>> getOptions = DropdownPopup._active.GetOptions;
    IList<string> stringList = getOptions != null ? getOptions() : (IList<string>) null;
    if ((stringList == null ? 1 : (stringList.Count == 0 ? 1 : 0)) == 0)
    {
      Rect panelRect = DropdownPopup.ComputePanelRect(stringList.Count);
      if (panelRect.Contains(current.mousePosition))
      {
        float defaultItemHeight = MenuConfig.DefaultItemHeight;
        float verticalSpacing = MenuConfig.VerticalSpacing;
        float num = panelRect.y + 4f;
        for (int index = 0; index < stringList.Count; ++index)
        {
          Rect rect;
          // ISSUE: explicit constructor call
          rect = new Rect(panelRect.x + 4f, num, panelRect.width - 8f, defaultItemHeight);
          if (!rect.Contains(current.mousePosition))
          {
            num += defaultItemHeight + verticalSpacing;
          }
          else
          {
            Action<int> onSelected = DropdownPopup._active.OnSelected;
            if (onSelected != null)
              onSelected(index);
            Sounds.PlayCasterClick(Sounds.boingSfx);
            DropdownPopup.Close();
            current.Use();
            return;
          }
        }
        current.Use();
      }
      else
      {
        DropdownPopup.Close();
        current.Use();
      }
    }
    else
      DropdownPopup.Close();
  }

  public static void DrawVisuals()
  {
    if (DropdownPopup._active == null)
      return;
    Func<IList<string>> getOptions = DropdownPopup._active.GetOptions;
    IList<string> stringList = getOptions != null ? getOptions() : (IList<string>) null;
    if ((stringList == null ? 1 : (stringList.Count == 0 ? 1 : 0)) != 0)
    {
      DropdownPopup.Close();
    }
    else
    {
      Rect panelRect = DropdownPopup.ComputePanelRect(stringList.Count);
      GUI.Box(panelRect, "", MenuConfig.GetMenuBoxStyle());
      Func<int> getSelectedIndex = DropdownPopup._active.GetSelectedIndex;
      int num1 = getSelectedIndex != null ? getSelectedIndex() : -1;
      float defaultItemHeight = MenuConfig.DefaultItemHeight;
      float verticalSpacing = MenuConfig.VerticalSpacing;
      Vector2 mousePosition = Event.current.mousePosition;
      float num2 = panelRect.y + 4f;
      for (int index = 0; index < stringList.Count; ++index)
      {
        while (DropdownPopup._rowAnims.Count <= index)
          DropdownPopup._rowAnims.Add(new AnimState());
        Rect rect;
        // ISSUE: explicit constructor call
        rect = new Rect(panelRect.x + 4f, num2, panelRect.width - 8f, defaultItemHeight);
        string str = index == num1 ? "● " + stringList[index] : stringList[index];
        ControlChrome.DrawSurface(rect, DropdownPopup._rowAnims[index], rect.Contains(mousePosition), false);
        GUI.Label(rect, str, MenuConfig.GetCenterTextStyle());
        num2 += defaultItemHeight + verticalSpacing;
      }
    }
  }

  private static Rect ComputePanelRect(int optionCount)
  {
    float defaultItemHeight = MenuConfig.DefaultItemHeight;
    float verticalSpacing = MenuConfig.VerticalSpacing;
    float num1 = (float) (8.0 + (double) optionCount * (double) defaultItemHeight + (double) (optionCount - 1) * (double) verticalSpacing);
    float width = DropdownPopup._anchor.width;
    float x = DropdownPopup._anchor.x;
    float num2 = DropdownPopup._anchor.yMax + 2f;
    if ((double) num2 + (double) num1 > (double) Screen.height)
      num2 = (float) ((double) DropdownPopup._anchor.y - (double) num1 - 2.0);
    if ((double) num2 < 0.0)
      num2 = 0.0f;
    return new Rect(x, num2, width, num1);
  }
}
