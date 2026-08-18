using Newtonsoft.Json;
using SakuraaCastingMod.Core;
using SakuraaCastingMod.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.VR.UtilMenu.Utility;

public static class ThemeManager
{
  [SavedSetting("ThemeManagerIsCustomThemeEnabled", false)]
  public static bool IsCustomThemeEnabled = false;
  private static Dictionary<string, Material> _materials = new Dictionary<string, Material>();
  private static Dictionary<string, Color> _defaultColors = new Dictionary<string, Color>();
  private static Dictionary<string, Color> _customColors = new Dictionary<string, Color>();
  public static List<string> MaterialNames = new List<string>();
  public static List<ThemePreset> AvailableThemes = new List<ThemePreset>();
  [SavedSetting("ThemeManagerCurrentThemeIndex", 0)]
  public static int CurrentThemeIndex = 0;
  public const int CustomSlotCount = 3;
  private static readonly Dictionary<string, Color>[] _customSlots = new Dictionary<string, Color>[3];
  [SavedSetting("ThemeManagerActiveCustomSlot", 0)]
  public static int ActiveCustomSlot = 0;
  [SavedSetting("ThemeManagerCustomSlot1", null)]
  public static Dictionary<string, List<float>> CustomSlot1Saved = new Dictionary<string, List<float>>();
  [SavedSetting("ThemeManagerCustomSlot2", null)]
  public static Dictionary<string, List<float>> CustomSlot2Saved = new Dictionary<string, List<float>>();
  [SavedSetting("ThemeManagerCustomSlot3", null)]
  public static Dictionary<string, List<float>> CustomSlot3Saved = new Dictionary<string, List<float>>();
  private const string ThemeFileName = "CustomTheme.json";

  public static event Action OnThemeApplied;

  public static void Initialize()
  {
    ThemeManager._materials.Clear();
    ThemeManager._defaultColors.Clear();
    ThemeManager._customColors.Clear();
    ThemeManager.MaterialNames.Clear();
    ThemeManager.AvailableThemes.Clear();
    ThemeManager.AddMaterial("BUTTON", UtilMenuMain.Instance?.buttonMat);
    ThemeManager.AddMaterial("PRESSED", UtilMenuMain.Instance?.pressedButtonMat);
    ThemeManager.AddMaterial("INNER", UtilMenuMain.Instance?.innerBtnMat);
    ThemeManager.AddMaterial("SELECTED", UtilMenuMain.Instance?.selectedBtnMat);
    ThemeManager.AddMaterial("PANEL", UtilMenuMain.Instance?.panelMat);
    ThemeManager.AddColorDef("TEXT 1", Color.white);
    ThemeManager.AddColorDef("TEXT 2", new Color(1f, 0.4f, 0.4f, 1f));
    for (int index = 0; index < 3; ++index)
      ThemeManager._customSlots[index] = new Dictionary<string, Color>((IDictionary<string, Color>) ThemeManager._defaultColors);
    ThemeManager.CreatePresets();
    ThemeManager.LoadTheme();
    if ((!ThemeManager.SavedSlotsEmpty() ? 0 : (ThemeManager.TryLoadLegacyFile() ? 1 : 0)) != 0)
      ThemeManager.SaveTheme();
    ThemeManager.ActiveCustomSlot = Mathf.Clamp(ThemeManager.ActiveCustomSlot, 0, 2);
    ThemeManager.CopySlotToLive(ThemeManager.ActiveCustomSlot);
    ThemeManager.ReapplyCurrentTheme();
  }

  private static void CreatePresets()
  {
    ThemeManager.AvailableThemes.Add(new ThemePreset()
    {
      Name = "DEFAULT",
      Colors = {
        ["BUTTON"] = new Color(1f, 0.553f, 0.773f),
        ["PRESSED"] = new Color(0.873f, 0.41f, 1f),
        ["INNER"] = new Color(0.525f, 0.553f, 1f),
        ["SELECTED"] = new Color(0.873f, 0.41f, 1f),
        ["PANEL"] = new Color(0.165f, 0.082f, 0.051f),
        ["TEXT 1"] = new Color(1f, 1f, 1f),
        ["TEXT 2"] = new Color(1f, 0.4f, 0.4f)
      }
    });
    ThemePreset themePreset = new ThemePreset()
    {
      Name = "DARK",
      Colors = {
        ["BUTTON"] = new Color(0.1f, 0.1f, 0.1f),
        ["PRESSED"] = new Color(0.2f, 0.2f, 0.2f),
        ["INNER"] = new Color(0.05f, 0.05f, 0.05f),
        ["SELECTED"] = new Color(0.4f, 0.4f, 0.4f),
        ["PANEL"] = new Color(0.0f, 0.0f, 0.0f),
        ["TEXT 1"] = new Color(0.9f, 0.9f, 0.9f),
        ["TEXT 2"] = new Color(0.6f, 0.6f, 0.6f)
      }
    };
    ThemeManager.AvailableThemes.Add(themePreset);
    ThemeManager.AvailableThemes.Add(new ThemePreset()
    {
      Name = "NEBULA",
      Colors = {
        ["PANEL"] = new Color(0.1f, 0.05f, 0.15f),
        ["INNER"] = new Color(0.2f, 0.0f, 0.3f),
        ["BUTTON"] = new Color(0.35f, 0.1f, 0.5f),
        ["PRESSED"] = new Color(0.5f, 0.2f, 0.7f),
        ["SELECTED"] = new Color(0.0f, 0.9f, 1f),
        ["TEXT 1"] = new Color(1f, 1f, 1f),
        ["TEXT 2"] = new Color(0.8f, 0.6f, 1f)
      }
    });
    ThemeManager.AvailableThemes.Add(new ThemePreset()
    {
      Name = "CYBERPUNK",
      Colors = {
        ["PANEL"] = new Color(0.05f, 0.05f, 0.08f),
        ["INNER"] = new Color(0.0f, 0.0f, 0.0f),
        ["BUTTON"] = new Color(0.0f, 0.6f, 0.8f),
        ["PRESSED"] = new Color(0.0f, 0.8f, 1f),
        ["SELECTED"] = new Color(1f, 0.0f, 0.5f),
        ["TEXT 1"] = new Color(0.9f, 1f, 0.0f),
        ["TEXT 2"] = new Color(0.0f, 0.9f, 0.9f)
      }
    });
    ThemeManager.AvailableThemes.Add(new ThemePreset()
    {
      Name = "FOREST",
      Colors = {
        ["PANEL"] = new Color(0.1f, 0.15f, 0.1f),
        ["INNER"] = new Color(0.15f, 0.2f, 0.15f),
        ["BUTTON"] = new Color(0.3f, 0.25f, 0.2f),
        ["PRESSED"] = new Color(0.4f, 0.35f, 0.25f),
        ["SELECTED"] = new Color(0.4f, 0.6f, 0.2f),
        ["TEXT 1"] = new Color(0.9f, 0.9f, 0.8f),
        ["TEXT 2"] = new Color(0.6f, 0.7f, 0.5f)
      }
    });
    ThemeManager.AvailableThemes.Add(new ThemePreset()
    {
      Name = "OCEAN",
      Colors = {
        ["PANEL"] = new Color(0.0f, 0.1f, 0.2f),
        ["INNER"] = new Color(0.0f, 0.2f, 0.3f),
        ["BUTTON"] = new Color(0.0f, 0.4f, 0.5f),
        ["PRESSED"] = new Color(0.2f, 0.6f, 0.7f),
        ["SELECTED"] = new Color(0.0f, 0.8f, 0.7f),
        ["TEXT 1"] = new Color(1f, 1f, 1f),
        ["TEXT 2"] = new Color(0.5f, 0.8f, 0.9f)
      }
    });
    ThemeManager.AvailableThemes.Add(new ThemePreset()
    {
      Name = "RETRO GOLD",
      Colors = {
        ["PANEL"] = new Color(0.15f, 0.15f, 0.15f),
        ["INNER"] = new Color(0.1f, 0.1f, 0.1f),
        ["BUTTON"] = new Color(0.3f, 0.3f, 0.3f),
        ["PRESSED"] = new Color(0.5f, 0.5f, 0.5f),
        ["SELECTED"] = new Color(1f, 0.84f, 0.0f),
        ["TEXT 1"] = new Color(1f, 0.95f, 0.8f),
        ["TEXT 2"] = new Color(0.7f, 0.7f, 0.7f)
      }
    });
    ThemeManager.AvailableThemes.Add(new ThemePreset()
    {
      Name = "CRIMSON",
      Colors = {
        ["PANEL"] = new Color(0.1f, 0.02f, 0.04f),
        ["INNER"] = new Color(0.18f, 0.05f, 0.08f),
        ["BUTTON"] = new Color(0.75f, 0.1f, 0.15f),
        ["PRESSED"] = new Color(0.95f, 0.2f, 0.25f),
        ["SELECTED"] = new Color(1f, 0.45f, 0.45f),
        ["TEXT 1"] = new Color(0.98f, 0.92f, 0.88f),
        ["TEXT 2"] = new Color(0.85f, 0.6f, 0.65f)
      }
    });
    ThemeManager.AvailableThemes.Add(new ThemePreset()
    {
      Name = "VAPORWAVE",
      Colors = {
        ["PANEL"] = new Color(0.06f, 0.02f, 0.1f),
        ["INNER"] = new Color(0.18f, 0.06f, 0.22f),
        ["BUTTON"] = new Color(0.95f, 0.2f, 0.85f),
        ["PRESSED"] = new Color(1f, 0.4f, 0.95f),
        ["SELECTED"] = new Color(0.3f, 0.95f, 1f),
        ["TEXT 1"] = new Color(1f, 1f, 1f),
        ["TEXT 2"] = new Color(0.85f, 0.7f, 1f)
      }
    });
    ThemeManager.AvailableThemes.Add(new ThemePreset()
    {
      Name = "TOXIC",
      Colors = {
        ["PANEL"] = new Color(0.05f, 0.1f, 0.05f),
        ["INNER"] = new Color(0.1f, 0.18f, 0.08f),
        ["BUTTON"] = new Color(0.5f, 0.95f, 0.1f),
        ["PRESSED"] = new Color(0.85f, 1f, 0.2f),
        ["SELECTED"] = new Color(0.1f, 0.65f, 0.2f),
        ["TEXT 1"] = new Color(0.9f, 1f, 0.85f),
        ["TEXT 2"] = new Color(0.6f, 0.8f, 0.4f)
      }
    });
    ThemeManager.AvailableThemes.Add(new ThemePreset()
    {
      Name = "AZTEC",
      Colors = {
        ["PANEL"] = new Color(0.725f, 0.933f, 1f),
        ["INNER"] = new Color(0.0f, 0.749f, 1f),
        ["BUTTON"] = new Color(0.294f, 0.808f, 0.976f),
        ["PRESSED"] = new Color(1f, 1f, 1f),
        ["SELECTED"] = new Color(0.0f, 0.643f, 0.906f),
        ["TEXT 1"] = new Color(0.0f, 0.0f, 0.0f),
        ["TEXT 2"] = new Color(0.0f, 0.0f, 0.0f)
      }
    });
    for (int index = 0; index < 3; ++index)
      ThemeManager.AvailableThemes.Add(new ThemePreset()
      {
        Name = $"CUSTOM {index + 1}"
      });
  }

  public static void CycleTheme(int direction)
  {
    if (ThemeManager.AvailableThemes.Count == 0)
      return;
    ThemeManager.CurrentThemeIndex += direction;
    if (ThemeManager.CurrentThemeIndex >= ThemeManager.AvailableThemes.Count)
      ThemeManager.CurrentThemeIndex = 0;
    if (ThemeManager.CurrentThemeIndex < 0)
      ThemeManager.CurrentThemeIndex = ThemeManager.AvailableThemes.Count - 1;
    ThemeManager.ApplyThemeAtIndex(ThemeManager.CurrentThemeIndex);
    Configuration.SaveSettings();
  }

  public static void SetThemeIndex(int idx)
  {
    if (ThemeManager.AvailableThemes.Count == 0 || (idx < 0 ? 1 : (idx >= ThemeManager.AvailableThemes.Count ? 1 : 0)) != 0)
      return;
    ThemeManager.CurrentThemeIndex = idx;
    ThemeManager.ApplyThemeAtIndex(idx);
    Configuration.SaveSettings();
  }

  public static void ReapplyCurrentTheme()
  {
    if (ThemeManager.AvailableThemes.Count == 0)
      return;
    ThemeManager.LoadTheme();
    if (ThemeManager.IsCustomThemeEnabled)
      ThemeManager.ApplyThemeAtIndex(Mathf.Clamp(ThemeManager.CurrentThemeIndex, 0, ThemeManager.AvailableThemes.Count - 1));
    else
      ThemeManager.RefreshTheme();
  }

  private static void ApplyThemeAtIndex(int idx)
  {
    int slot = ThemeManager.CustomSlotForIndex(idx);
    if (slot >= 0)
      ThemeManager.ActivateCustomSlot(slot);
    else
      ThemeManager.ApplyTheme(ThemeManager.AvailableThemes[idx]);
  }

  private static void ApplyTheme(ThemePreset preset)
  {
    ThemeManager.IsCustomThemeEnabled = true;
    foreach (KeyValuePair<string, Color> color in preset.Colors)
    {
      if (ThemeManager._customColors.ContainsKey(color.Key))
        ThemeManager._customColors[color.Key] = color.Value;
      if (ThemeManager._materials.ContainsKey(color.Key))
        ThemeManager._materials[color.Key].color = color.Value;
    }
    Action onThemeApplied = ThemeManager.OnThemeApplied;
    if (onThemeApplied == null)
      return;
    onThemeApplied();
  }

  public static void ActivateCustomSlot(int slot)
  {
    slot = Mathf.Clamp(slot, 0, 2);
    ThemeManager.ActiveCustomSlot = slot;
    ThemeManager.IsCustomThemeEnabled = true;
    ThemeManager.CopySlotToLive(slot);
    foreach (KeyValuePair<string, Color> customColor in ThemeManager._customColors)
    {
      if (ThemeManager._materials.ContainsKey(customColor.Key))
        ThemeManager._materials[customColor.Key].color = customColor.Value;
    }
    Action onThemeApplied = ThemeManager.OnThemeApplied;
    if (onThemeApplied != null)
      onThemeApplied();
    ThemeManager.SaveTheme();
  }

  private static void CopySlotToLive(int slot)
  {
    if ((slot < 0 || slot >= 3 ? 1 : (ThemeManager._customSlots[slot] == null ? 1 : 0)) != 0)
      return;
    foreach (KeyValuePair<string, Color> keyValuePair in ThemeManager._customSlots[slot])
      ThemeManager._customColors[keyValuePair.Key] = keyValuePair.Value;
  }

  private static int CustomSlotForIndex(int idx)
  {
    int num;
    if ((idx < 0 ? 1 : (idx >= ThemeManager.AvailableThemes.Count ? 1 : 0)) != 0)
    {
      num = -1;
    }
    else
    {
      string name = ThemeManager.AvailableThemes[idx].Name;
      int result = default;
      num = (name == null || !name.StartsWith("CUSTOM ") || !int.TryParse(name.Substring(7), out result) || result < 1 ? 0 : (result <= 3 ? 1 : 0)) != 0 ? result - 1 : -1;
    }
    return num;
  }

  public static int CustomSlotThemeIndex(int slot)
  {
    return ThemeManager.AvailableThemes.FindIndex((Predicate<ThemePreset>) (t => t.Name == $"CUSTOM {slot + 1}"));
  }

  private static void AddMaterial(string name, Material mat)
  {
    if (((UnityEngine.Object) mat == (UnityEngine.Object) null))
    {
      Color materialFallback = ThemeManager.GetMaterialFallback(name);
      ThemeManager._defaultColors[name] = materialFallback;
      ThemeManager._customColors[name] = materialFallback;
      ThemeManager.MaterialNames.Add(name);
    }
    else
    {
      ThemeManager._materials[name] = mat;
      ThemeManager._defaultColors[name] = mat.color;
      ThemeManager._customColors[name] = mat.color;
      ThemeManager.MaterialNames.Add(name);
    }
  }

  private static Color GetMaterialFallback(string name)
  {
    Color materialFallback;
    switch (name)
    {
      case "BUTTON":
        materialFallback = new Color(0.78f, 0.65f, 0.55f);
        break;
      case "PRESSED":
        materialFallback = new Color(0.55f, 0.45f, 0.35f);
        break;
      case "INNER":
        materialFallback = new Color(0.3f, 0.3f, 0.3f);
        break;
      case "SELECTED":
        materialFallback = new Color(1f, 0.72f, 0.77f);
        break;
      case "PANEL":
        materialFallback = new Color(0.07f, 0.07f, 0.07f);
        break;
      default:
        materialFallback = Color.white;
        break;
    }
    return materialFallback;
  }

  private static void AddColorDef(string name, Color defaultCol)
  {
    ThemeManager._defaultColors[name] = defaultCol;
    ThemeManager._customColors[name] = defaultCol;
    ThemeManager.MaterialNames.Add(name);
  }

  public static void RefreshTheme()
  {
    foreach (KeyValuePair<string, Material> pair in ThemeManager._materials)
    {
      string key = pair.Key;
      Material material = pair.Value;
      if (!ThemeManager.IsCustomThemeEnabled)
      {
        Color color;
        if (ThemeManager._defaultColors.TryGetValue(key, out color))
          material.color = color;
      }
      else
      {
        Color color;
        if (ThemeManager._customColors.TryGetValue(key, out color))
          material.color = color;
      }
    }
    Action onThemeApplied = ThemeManager.OnThemeApplied;
    if (onThemeApplied == null)
      return;
    onThemeApplied();
  }

  public static void UpdateColor(string name, Color col)
  {
    if (!ThemeManager._customColors.ContainsKey(name))
      return;
    ThemeManager._customColors[name] = col;
    if (ThemeManager._customSlots[ThemeManager.ActiveCustomSlot] != null)
      ThemeManager._customSlots[ThemeManager.ActiveCustomSlot][name] = col;
    if ((!ThemeManager.IsCustomThemeEnabled ? 0 : (ThemeManager._materials.ContainsKey(name) ? 1 : 0)) != 0)
      ThemeManager._materials[name].color = col;
    ThemeManager.SaveTheme();
  }

  public static Color GetCustomColor(string name)
  {
    Color color;
    return (ThemeManager.IsCustomThemeEnabled ? 0 : (ThemeManager._defaultColors.ContainsKey(name) ? 1 : 0)) != 0 ? ThemeManager._defaultColors[name] : (ThemeManager._customColors.TryGetValue(name, out color) ? color : Color.white);
  }

  public static Color GetDefaultColor(string name)
  {
    Color color;
    return !ThemeManager._defaultColors.TryGetValue(name, out color) ? Color.white : color;
  }

  public static void UpdateColorLive(string name, Color col)
  {
    if (!ThemeManager._customColors.ContainsKey(name))
      return;
    ThemeManager._customColors[name] = col;
    if (ThemeManager._customSlots[ThemeManager.ActiveCustomSlot] != null)
      ThemeManager._customSlots[ThemeManager.ActiveCustomSlot][name] = col;
    if ((!ThemeManager.IsCustomThemeEnabled ? 0 : (ThemeManager._materials.ContainsKey(name) ? 1 : 0)) == 0)
      return;
    ThemeManager._materials[name].color = col;
  }

  public static void ResetColorToDefault(string name)
  {
    if (!ThemeManager._defaultColors.ContainsKey(name))
      return;
    ThemeManager.UpdateColor(name, ThemeManager._defaultColors[name]);
  }

  public static void SaveCurrentTheme() => ThemeManager.SaveTheme();

  public static int CustomThemeIndex
  {
    get => ThemeManager.CustomSlotThemeIndex(ThemeManager.ActiveCustomSlot);
  }

  public static void ResetToDefaults()
  {
    foreach (string key in ThemeManager.MaterialNames.Where<string>((Func<string, bool>) (name => ThemeManager._defaultColors.ContainsKey(name))))
    {
      ThemeManager._customColors[key] = ThemeManager._defaultColors[key];
      if (ThemeManager._customSlots[ThemeManager.ActiveCustomSlot] != null)
        ThemeManager._customSlots[ThemeManager.ActiveCustomSlot][key] = ThemeManager._defaultColors[key];
    }
    ThemeManager.RefreshTheme();
    ThemeManager.SaveTheme();
  }

  private static void SaveTheme()
  {
    ThemeManager.CustomSlot1Saved = ThemeManager.SerializeSlot(0);
    ThemeManager.CustomSlot2Saved = ThemeManager.SerializeSlot(1);
    ThemeManager.CustomSlot3Saved = ThemeManager.SerializeSlot(2);
    Configuration.SaveSettings();
  }

  private static void LoadTheme()
  {
    ThemeManager.DeserializeSlot(0, ThemeManager.CustomSlot1Saved);
    ThemeManager.DeserializeSlot(1, ThemeManager.CustomSlot2Saved);
    ThemeManager.DeserializeSlot(2, ThemeManager.CustomSlot3Saved);
  }

  private static Dictionary<string, List<float>> SerializeSlot(int slot)
  {
    Dictionary<string, List<float>> dictionary = new Dictionary<string, List<float>>();
    if ((slot < 0 || slot >= 3 ? 0 : (ThemeManager._customSlots[slot] != null ? 1 : 0)) != 0)
    {
      foreach (KeyValuePair<string, Color> keyValuePair in ThemeManager._customSlots[slot])
        dictionary[keyValuePair.Key] = new List<float>()
        {
          keyValuePair.Value.r,
          keyValuePair.Value.g,
          keyValuePair.Value.b,
          keyValuePair.Value.a
        };
    }
    return dictionary;
  }

  private static void DeserializeSlot(int slot, Dictionary<string, List<float>> saved)
  {
    if ((slot < 0 || slot >= 3 ? 1 : (ThemeManager._customSlots[slot] == null ? 1 : 0)) != 0)
      return;
    ThemeManager.LoadSlotColors(ThemeManager._customSlots[slot], saved);
  }

  private static bool SavedSlotsEmpty()
  {
    if (ThemeManager.CustomSlot1Saved != null && ThemeManager.CustomSlot1Saved.Count != 0 || ThemeManager.CustomSlot2Saved != null && ThemeManager.CustomSlot2Saved.Count != 0)
      return false;
    return ThemeManager.CustomSlot3Saved == null || ThemeManager.CustomSlot3Saved.Count == 0;
  }

  private static bool TryLoadLegacyFile()
  {
    try
    {
      string path = Path.Combine(Application.persistentDataPath, "CustomTheme.json");
      if (!File.Exists(path))
        return false;
      ThemeManager.ThemeSaveData themeSaveData = JsonConvert.DeserializeObject<ThemeManager.ThemeSaveData>(File.ReadAllText(path));
      if (themeSaveData == null)
        return false;
      bool flag = false;
      if ((themeSaveData.Slots == null ? 0 : (themeSaveData.Slots.Count > 0 ? 1 : 0)) == 0)
      {
        if (themeSaveData.Colors != null)
          flag |= ThemeManager.LoadSlotColors(ThemeManager._customSlots[0], themeSaveData.Colors);
      }
      else
      {
        for (int index = 0; (index >= 3 ? 0 : (index < themeSaveData.Slots.Count ? 1 : 0)) != 0; ++index)
          flag |= ThemeManager.LoadSlotColors(ThemeManager._customSlots[index], themeSaveData.Slots[index]);
      }
      return flag;
    }
    catch (Exception ex)
    {
      UnityEngine.Debug.LogError((object) ("[ThemeManager] Legacy theme migration failed: " + ex.Message));
      return false;
    }
  }

  private static bool LoadSlotColors(
    Dictionary<string, Color> target,
    Dictionary<string, List<float>> src)
  {
    bool flag1;
    if ((target == null ? 1 : (src == null ? 1 : 0)) != 0)
    {
      flag1 = false;
    }
    else
    {
      bool flag2 = false;
      foreach (KeyValuePair<string, List<float>> keyValuePair in src)
      {
        if ((!target.ContainsKey(keyValuePair.Key) || keyValuePair.Value == null ? 1 : (keyValuePair.Value.Count < 3 ? 1 : 0)) == 0)
        {
          float num = keyValuePair.Value.Count > 3 ? keyValuePair.Value[3] : 1f;
          target[keyValuePair.Key] = new Color(keyValuePair.Value[0], keyValuePair.Value[1], keyValuePair.Value[2], num);
          flag2 = true;
        }
      }
      flag1 = flag2;
    }
    return flag1;
  }

  private class ThemeSaveData
  {
    public List<Dictionary<string, List<float>>> Slots;
    public Dictionary<string, List<float>> Colors;
  }
}
