using SakuraaCastingMod.Shared.Helpers;
using SakuraaCastingMod.VR.UtilMenu.Utility;
using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Desktop.Ui.Framework.Menus;

internal static class ThemeEditorMenu
{
  public static bool ShowMenu;
  private static readonly (string Key, string Label, string Desc)[] Slots = new (string, string, string)[7]
  {
    ("PANEL", "Panel", "Window background"),
    ("INNER", "Inset", "Slider tracks, tooltip + description boxes"),
    ("BUTTON", "Hover", "Button hover tint"),
    ("PRESSED", "Pressed", "Button pressed tint"),
    ("SELECTED", "Accent", "Outlines and highlights"),
    ("TEXT 1", "Text", "Primary text"),
    ("TEXT 2", "Text 2", "Secondary text")
  };
  private static MenuBuilder _menu;
  private static bool _initialized;
  private static string _selectedKey = "PANEL";
  private static float _h;
  private static float _s;
  private static float _v;
  private static int _dragging;
  private static bool _pendingSave;
  private static bool _colorsDirty;
  private static Texture2D _svTex;
  private static float _svTexHue = -1f;
  private static Texture2D _hueTex;
  private const int SvRes = 96 /*0x60*/;
  private const int HueRes = 128 /*0x80*/;
  private static readonly List<int> _presetIdx = new List<int>();

  public static void Open()
  {
    ThemeEditorMenu.EnsureInit();
    ThemeEditorMenu.ShowMenu = true;
    ThemeEditorMenu._dragging = 0;
    ThemeEditorMenu._pendingSave = false;
    ThemeEditorMenu.EnsureEditingCustom();
    ThemeEditorMenu.SyncFromSlot();
  }

  public static void Toggle()
  {
    if (!ThemeEditorMenu.ShowMenu)
      ThemeEditorMenu.Open();
    else
      ThemeEditorMenu.ShowMenu = false;
  }

  public static void Draw()
  {
    if (!ThemeEditorMenu.ShowMenu)
      return;
    ThemeEditorMenu.EnsureInit();
    ThemeEditorMenu._menu?.Draw();
  }

  public static bool ConsumeColorsDirty()
  {
    bool flag;
    if (!ThemeEditorMenu._colorsDirty)
    {
      flag = false;
    }
    else
    {
      ThemeEditorMenu._colorsDirty = false;
      flag = true;
    }
    return flag;
  }

  private static void EnsureInit()
  {
    if (ThemeEditorMenu._initialized)
      return;
    if (ThemeManager.AvailableThemes.Count == 0)
      ThemeManager.Initialize();
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    ThemeEditorMenu._menu = new MenuBuilder("Theme Editor", 300f).SetPositionRef(560f, 110f).AddSpace(4f).AddLabel("Three saved custom themes - pick a slot and recolor it.").AddDropdown("Editing", (Func<IList<string>>) (() => (IList<string>) ThemeEditorMenu.CustomSlotNames()), (Func<int>) (() => Mathf.Clamp(ThemeManager.ActiveCustomSlot, 0, 2)), new Action<int>(ThemeEditorMenu.SelectCustomSlot), "Which of your 3 custom themes to edit - each one is saved separately").AddDropdown("Start from", (Func<IList<string>>) (() => (IList<string>) ThemeEditorMenu.PresetNames()), (Func<int>) (() => 0), new Action<int>(ThemeEditorMenu.SeedFromPresetOption), "Copy a built-in theme's colors into this slot as a starting point").AddSpace(6f).AddItem((MenuItem) new ThemeEditorMenu.SwatchStripItem()).AddDynamicLabel((Func<string>) (() => "Editing  -  " + ThemeEditorMenu.CurrentSlotLabel())).AddItem((MenuItem) new ThemeEditorMenu.PickerItem()).AddDynamicLabel(new Func<string>(ThemeEditorMenu.HexAndRgbReadout)).AddButton("Copy Hex", new Action(ThemeEditorMenu.CopyHex), "Copy this color's #RRGGBB to the clipboard").AddButton("Paste Hex", new Action(ThemeEditorMenu.PasteHex), "Apply a #RRGGBB color from the clipboard").AddSpace(6f).AddButton("Reset This Slot", new Action(ThemeEditorMenu.ResetSlot), "Restore the selected color to its default").AddHoldButton("Reset Whole Theme", 1.2f, new Action(ThemeEditorMenu.ResetAll), "Hold to reset every color to defaults").AddSpace(6f).AddButton("Close", (Action) (() => ThemeEditorMenu.ShowMenu = false), "Hide the theme editor");
    ThemeEditorMenu._initialized = true;
  }

  private static Color CurrentColor()
  {
    Color rgb = Color.HSVToRGB(Mathf.Clamp01(ThemeEditorMenu._h), Mathf.Clamp01(ThemeEditorMenu._s), Mathf.Clamp01(ThemeEditorMenu._v));
    rgb.a = 1f;
    return rgb;
  }

  private static string CurrentSlotLabel()
  {
    string str;
    foreach ((string Key, string Label, string Desc) slot in ThemeEditorMenu.Slots)
    {
      if (slot.Key == ThemeEditorMenu._selectedKey)
      {
        str = slot.Label;
        goto label_6;
      }
    }
    str = ThemeEditorMenu._selectedKey;
label_6:
    return str;
  }

  private static void SyncFromSlot()
  {
    float num1 = default;
    float num2 = default;
    float num3 = default;
    Color.RGBToHSV(ThemeManager.GetCustomColor(ThemeEditorMenu._selectedKey), out num1, out num2, out num3);
    if (((double) num2 <= 9.9999997473787516E-05 ? 0 : ((double) num3 > 9.9999997473787516E-05 ? 1 : 0)) != 0)
      ThemeEditorMenu._h = num1;
    ThemeEditorMenu._s = num2;
    ThemeEditorMenu._v = num3;
  }

  private static void SelectSlot(string key)
  {
    if (ThemeEditorMenu._selectedKey == key)
      return;
    ThemeEditorMenu._selectedKey = key;
    ThemeEditorMenu.SyncFromSlot();
  }

  private static void EnsureEditingCustom()
  {
    int customThemeIndex = ThemeManager.CustomThemeIndex;
    if ((customThemeIndex < 0 ? 0 : (ThemeManager.CurrentThemeIndex != customThemeIndex ? 1 : 0)) == 0)
      return;
    ThemeManager.SetThemeIndex(customThemeIndex);
  }

  private static void ApplyLive()
  {
    ThemeEditorMenu.EnsureEditingCustom();
    ThemeManager.UpdateColorLive(ThemeEditorMenu._selectedKey, ThemeEditorMenu.CurrentColor());
    ThemeEditorMenu._colorsDirty = true;
    ThemeEditorMenu._pendingSave = true;
  }

  private static void CommitSave()
  {
    if (!ThemeEditorMenu._pendingSave)
      return;
    ThemeManager.SaveCurrentTheme();
    ThemeEditorMenu._pendingSave = false;
  }

  private static void SetSlotColor(Color c, bool save)
  {
    float num1;
    float num2;
    float num3;
    Color.RGBToHSV(c, out num1, out num2, out num3);
    if (((double) num2 <= 9.9999997473787516E-05 ? 0 : ((double) num3 > 9.9999997473787516E-05 ? 1 : 0)) != 0)
      ThemeEditorMenu._h = num1;
    ThemeEditorMenu._s = num2;
    ThemeEditorMenu._v = num3;
    ThemeEditorMenu.EnsureEditingCustom();
    ThemeManager.UpdateColorLive(ThemeEditorMenu._selectedKey, ThemeEditorMenu.CurrentColor());
    ThemeEditorMenu._colorsDirty = true;
    ThemeEditorMenu._pendingSave = true;
    if (!save)
      return;
    ThemeEditorMenu.CommitSave();
  }

  private static List<string> CustomSlotNames()
  {
    List<string> stringList = new List<string>();
    for (int index = 0; index < 3; ++index)
      stringList.Add($"Custom {index + 1}");
    return stringList;
  }

  private static List<string> PresetNames()
  {
    ThemeEditorMenu._presetIdx.Clear();
    List<string> stringList = new List<string>()
    {
      "Pick a preset..."
    };
    List<ThemePreset> availableThemes = ThemeManager.AvailableThemes;
    for (int index = 0; index < availableThemes.Count; ++index)
    {
      if ((availableThemes[index].Name == null ? 0 : (availableThemes[index].Name.StartsWith("CUSTOM ") ? 1 : 0)) == 0)
      {
        ThemeEditorMenu._presetIdx.Add(index);
        stringList.Add(availableThemes[index].Name);
      }
    }
    return stringList;
  }

  private static void SelectCustomSlot(int slot)
  {
    int idx = ThemeManager.CustomSlotThemeIndex(slot);
    if (idx >= 0)
      ThemeManager.SetThemeIndex(idx);
    ThemeEditorMenu.SyncFromSlot();
    ThemeEditorMenu._colorsDirty = true;
  }

  private static void SeedFromPresetOption(int optionIndex)
  {
    int index = optionIndex - 1;
    if ((index < 0 ? 1 : (index >= ThemeEditorMenu._presetIdx.Count ? 1 : 0)) != 0)
      return;
    ThemePreset availableTheme = ThemeManager.AvailableThemes[ThemeEditorMenu._presetIdx[index]];
    ThemeEditorMenu.EnsureEditingCustom();
    foreach (KeyValuePair<string, Color> color in availableTheme.Colors)
      ThemeManager.UpdateColorLive(color.Key, color.Value);
    ThemeManager.SaveCurrentTheme();
    ThemeEditorMenu.SyncFromSlot();
    ThemeEditorMenu._colorsDirty = true;
  }

  private static void ResetSlot()
  {
    ThemeEditorMenu.EnsureEditingCustom();
    ThemeManager.ResetColorToDefault(ThemeEditorMenu._selectedKey);
    ThemeEditorMenu.SyncFromSlot();
    ThemeEditorMenu._colorsDirty = true;
  }

  private static void ResetAll()
  {
    ThemeEditorMenu.EnsureEditingCustom();
    ThemeManager.ResetToDefaults();
    ThemeEditorMenu.SyncFromSlot();
    ThemeEditorMenu._colorsDirty = true;
  }

  private static void CopyHex()
  {
    GUIUtility.systemCopyBuffer = "#" + ColorUtility.ToHtmlStringRGB(ThemeEditorMenu.CurrentColor());
  }

  private static void PasteHex()
  {
    string systemCopyBuffer = GUIUtility.systemCopyBuffer;
    if (string.IsNullOrWhiteSpace(systemCopyBuffer))
      return;
    string str = systemCopyBuffer.Trim();
    if (!str.StartsWith("#"))
      str = "#" + str;
    Color c = default;
    if (!ColorUtility.TryParseHtmlString(str, out c))
      return;
    ThemeEditorMenu.SetSlotColor(c, true);
  }

  private static string HexAndRgbReadout()
  {
    Color color = ThemeEditorMenu.CurrentColor();
    return $"#{ColorUtility.ToHtmlStringRGB(color)}      R {Mathf.RoundToInt(color.r * (float) byte.MaxValue)}   G {Mathf.RoundToInt(color.g * (float) byte.MaxValue)}   B {Mathf.RoundToInt(color.b * (float) byte.MaxValue)}";
  }

  private static void EnsureHueTexture()
  {
    if (((UnityEngine.Object) ThemeEditorMenu._hueTex != (UnityEngine.Object) null))
      return;
    Texture2D texture2D = new Texture2D(1, 128 /*0x80*/, (TextureFormat) 4, false);
    ((Texture) texture2D).wrapMode = (TextureWrapMode) 1;
    ((Texture) texture2D).filterMode = (FilterMode) 1;
    ThemeEditorMenu._hueTex = texture2D;
    Color[] colorArray = new Color[128 /*0x80*/];
    for (int index = 0; index < 128 /*0x80*/; ++index)
      colorArray[index] = Color.HSVToRGB((float) (1.0 - (double) index / (double) sbyte.MaxValue), 1f, 1f);
    ThemeEditorMenu._hueTex.SetPixels(colorArray);
    ThemeEditorMenu._hueTex.Apply();
  }

  private static void EnsureSvTexture()
  {
    if ((!((UnityEngine.Object) ThemeEditorMenu._svTex != (UnityEngine.Object) null) ? 0 : (Mathf.Approximately(ThemeEditorMenu._svTexHue, ThemeEditorMenu._h) ? 1 : 0)) != 0)
      return;
    if (((UnityEngine.Object) ThemeEditorMenu._svTex == (UnityEngine.Object) null))
    {
      Texture2D texture2D = new Texture2D(96 /*0x60*/, 96 /*0x60*/, (TextureFormat) 4, false);
      ((Texture) texture2D).wrapMode = (TextureWrapMode) 1;
      ((Texture) texture2D).filterMode = (FilterMode) 1;
      ThemeEditorMenu._svTex = texture2D;
    }
    Color[] colorArray = new Color[9216];
    for (int index1 = 0; index1 < 96 /*0x60*/; ++index1)
    {
      float num1 = (float) index1 / 95f;
      for (int index2 = 0; index2 < 96 /*0x60*/; ++index2)
      {
        float num2 = (float) index2 / 95f;
        colorArray[index1 * 96 /*0x60*/ + index2] = Color.HSVToRGB(ThemeEditorMenu._h, num2, num1);
      }
    }
    ThemeEditorMenu._svTex.SetPixels(colorArray);
    ThemeEditorMenu._svTex.Apply();
    ThemeEditorMenu._svTexHue = ThemeEditorMenu._h;
  }

  private sealed class SwatchStripItem : MenuItem
  {
    public SwatchStripItem() => this.Height = 40f;

    public override bool Draw(Rect rect)
    {
      Event current = Event.current;
      int length = ThemeEditorMenu.Slots.Length;
      float num1 = (rect.width - 5f * (float) (length - 1)) / (float) length;
      float num2 = Mathf.Min(rect.height, 30f);
      for (int index = 0; index < length; ++index)
      {
        (string Key, string Label, string Desc) slot = ThemeEditorMenu.Slots[index];
        Rect r;
        // ISSUE: explicit constructor call
        r = new Rect(rect.x + (float) index * (num1 + 5f), rect.y + (float) (((double) rect.height - (double) num2) * 0.5), num1, num2);
        if (((int) current.type != 0 || current.button != 0 ? 0 : (r.Contains(current.mousePosition) ? 1 : 0)) != 0)
        {
          ThemeEditorMenu.SelectSlot(slot.Key);
          Sounds.PlayCasterClick(Sounds.subtleClickSfx);
          current.Use();
        }
        if (current.type == (EventType) 7)
        {
          bool flag1 = slot.Key == ThemeEditorMenu._selectedKey;
          bool flag2 = r.Contains(current.mousePosition);
          if (flag1 | flag2)
          {
            Color outlineColorNow = MenuConfig.OutlineColorNow;
            outlineColorNow.a = flag1 ? 1f : 0.5f;
            ControlChrome.FillRect(new Rect(r.x - 2f, r.y - 2f, r.width + 4f, r.height + 4f), outlineColorNow);
          }
          Color customColor = ThemeManager.GetCustomColor(slot.Key);
          customColor.a = 1f;
          ControlChrome.FillRect(r, customColor);
        }
      }
      if ((current.type != (EventType) 7 ? 0 : (rect.Contains(current.mousePosition) ? 1 : 0)) != 0)
      {
        float num3 = (rect.width - 5f * (float) (length - 1)) / (float) length;
        int index = Mathf.Clamp((int) (((double) current.mousePosition.x - (double) rect.x) / ((double) num3 + 5.0)), 0, length - 1);
        this.Description = $"{ThemeEditorMenu.Slots[index].Label} - {ThemeEditorMenu.Slots[index].Desc}";
      }
      this.DrawDescription(rect, true);
      return false;
    }
  }

  private sealed class PickerItem : MenuItem
  {
    public PickerItem() => this.Height = 150f;

    public override bool Draw(Rect rect)
    {
      Event current = Event.current;
      float num = Mathf.Min(rect.height, rect.width - 120f);
      Rect rect1;
      // ISSUE: explicit constructor call
      rect1 = new Rect(rect.x, rect.y, num, num);
      Rect rect2;
      // ISSUE: explicit constructor call
      rect2 = new Rect(rect1.xMax + 10f, rect.y, 22f, num);
      Rect r;
      // ISSUE: explicit constructor call
      r = new Rect(rect2.xMax + 10f, rect.y, rect.xMax - (rect2.xMax + 10f), num);
      ThemeEditorMenu.PickerItem.HandleInput(current, rect1, rect2);
      if (current.type == (EventType) 7)
      {
        ThemeEditorMenu.EnsureSvTexture();
        ThemeEditorMenu.EnsureHueTexture();
        GUI.DrawTexture(rect1, (Texture) ThemeEditorMenu._svTex, (ScaleMode) 0, false);
        ThemeEditorMenu.PickerItem.DrawBorder(rect1, MenuConfig.OutlineColorNow);
        Vector2 c;
        // ISSUE: explicit constructor call
        c = new Vector2(rect1.x + Mathf.Clamp01(ThemeEditorMenu._s) * rect1.width, rect1.y + (1f - Mathf.Clamp01(ThemeEditorMenu._v)) * rect1.height);
        ThemeEditorMenu.PickerItem.DrawRingCursor(c);
        GUI.DrawTexture(rect2, (Texture) ThemeEditorMenu._hueTex, (ScaleMode) 0, false);
        ThemeEditorMenu.PickerItem.DrawBorder(rect2, MenuConfig.OutlineColorNow);
        float y = rect2.y + Mathf.Clamp01(ThemeEditorMenu._h) * rect2.height;
        ThemeEditorMenu.PickerItem.DrawHueMarker(rect2, y);
        ControlChrome.FillRect(r, ThemeEditorMenu.CurrentColor());
        ThemeEditorMenu.PickerItem.DrawBorder(r, MenuConfig.OutlineColorNow);
      }
      return false;
    }

    private static void HandleInput(Event e, Rect sv, Rect hue)
    {
      if (((int) e.type != 0 ? 0 : (e.button == 0 ? 1 : 0)) == 0)
      {
        if ((e.type != (EventType) 3 ? 0 : (ThemeEditorMenu._dragging != 0 ? 1 : 0)) == 0)
        {
          if ((e.type != (EventType) 1 || e.button != 0 ? 0 : (ThemeEditorMenu._dragging != 0 ? 1 : 0)) == 0)
            return;
          ThemeEditorMenu._dragging = 0;
          ThemeEditorMenu.CommitSave();
          e.Use();
        }
        else
        {
          if (ThemeEditorMenu._dragging != 1)
            ThemeEditorMenu.PickerItem.UpdateHue(e, hue);
          else
            ThemeEditorMenu.PickerItem.UpdateSv(e, sv);
          e.Use();
        }
      }
      else if (sv.Contains(e.mousePosition))
      {
        ThemeEditorMenu._dragging = 1;
        ThemeEditorMenu.PickerItem.UpdateSv(e, sv);
        e.Use();
      }
      else
      {
        if (!hue.Contains(e.mousePosition))
          return;
        ThemeEditorMenu._dragging = 2;
        ThemeEditorMenu.PickerItem.UpdateHue(e, hue);
        e.Use();
      }
    }

    private static void UpdateSv(Event e, Rect sv)
    {
      ThemeEditorMenu._s = Mathf.Clamp01((e.mousePosition.x - sv.x) / sv.width);
      ThemeEditorMenu._v = Mathf.Clamp01((float) (1.0 - ((double) e.mousePosition.y - (double) sv.y) / (double) sv.height));
      ThemeEditorMenu.ApplyLive();
    }

    private static void UpdateHue(Event e, Rect hue)
    {
      ThemeEditorMenu._h = Mathf.Clamp01((e.mousePosition.y - hue.y) / hue.height);
      ThemeEditorMenu.ApplyLive();
    }

    private static void DrawRingCursor(Vector2 c)
    {
      float num = 6f;
      ControlChrome.FillRect(new Rect(c.x - num, c.y - num, 12f, 12f), new Color(1f, 1f, 1f, 0.95f));
      ControlChrome.FillRect(new Rect((float) ((double) c.x - (double) num + 2.0), (float) ((double) c.y - (double) num + 2.0), 8f, 8f), ThemeEditorMenu.CurrentColor());
    }

    private static void DrawHueMarker(Rect hue, float y)
    {
      float num = 3f;
      ControlChrome.FillRect(new Rect(hue.x - 2f, y - num, hue.width + 4f, 6f), new Color(1f, 1f, 1f, 0.95f));
    }

    private static void DrawBorder(Rect r, Color c)
    {
      c.a = Mathf.Max(c.a, 0.8f);
      ControlChrome.FillRect(new Rect(r.x - 1.5f, r.y - 1.5f, r.width + 3f, 1.5f), c);
      ControlChrome.FillRect(new Rect(r.x - 1.5f, r.yMax, r.width + 3f, 1.5f), c);
      ControlChrome.FillRect(new Rect(r.x - 1.5f, r.y - 1.5f, 1.5f, r.height + 3f), c);
      ControlChrome.FillRect(new Rect(r.xMax, r.y - 1.5f, 1.5f, r.height + 3f), c);
    }
  }
}
