using SakuraaCastingMod.Core;
using SakuraaCastingMod.Desktop.Ui.Framework.MenuItems;
using SakuraaCastingMod.Features.Overlays;
using SakuraaCastingMod.Shared.Helpers;
using SakuraaOfflinePresets;
using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Desktop.Ui.Framework.Menus;

public static class PresetsMenu
{
  [SavedSetting("ShowPresetsMenu", true)]
  public static bool ShowMenu = true;
  public static MenuBuilder Menu;
  private static TextFieldMenuItem _nameField;
  private static int _seenVersion = -1;
  private static readonly Vector2 DefaultPosition = new Vector2(540f, 320f);
  private const int MenuWidth = 280;

  public static void Draw()
  {
    if (PresetsMenu.Menu == null)
      PresetsMenu.Initialize();
    if (Configuration.PresetListVersion != PresetsMenu._seenVersion)
    {
      PresetsMenu.Rebuild();
      PresetsMenu._seenVersion = Configuration.PresetListVersion;
    }
    if (!PresetsMenu.ShowMenu)
      return;
    PresetsMenu.Menu.Draw();
  }

  public static void Initialize()
  {
    OfflinePresetStore.LoadLocalPresets();
    TextFieldMenuItem textFieldMenuItem = new TextFieldMenuItem();
    textFieldMenuItem.Label = "New preset name";
    textFieldMenuItem.Text = "";
    textFieldMenuItem.MaxLength = 64 /*0x40*/;
    textFieldMenuItem.ForceUpperCase = false;
    textFieldMenuItem.DisallowSpaces = false;
    textFieldMenuItem.Description = "Type a name then click Create";
    PresetsMenu._nameField = textFieldMenuItem;
    PresetsMenu.Menu = new MenuBuilder("Presets", 280f).SetPositionRef(PresetsMenu.DefaultPosition.x, PresetsMenu.DefaultPosition.y);
    PresetsMenu.Rebuild();
  }

  private static void Rebuild()
  {
    OfflinePresetStore.LoadLocalPresets();
    if (PresetsMenu.Menu == null)
      return;
    PresetsMenu.Menu.Items.Clear();
    PresetsMenu.Menu.AddDynamicLabel((Func<string>) (() =>
    {
      string str;
      if (!Configuration.InitialLoadComplete)
      {
        str = "Loading…";
      }
      else
      {
        string activePresetName = Configuration.ActivePresetName;
        str = string.IsNullOrEmpty(activePresetName) ? "Current: Custom" : "Current: " + activePresetName;
      }
      return str;
    }), true);
    PresetsMenu.Menu.AddSpace(4f);
    PresetsMenu.Menu.AddItem((MenuItem) PresetsMenu._nameField);
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    PresetsMenu.Menu.AddButton("Create Preset", new Action(PresetsMenu.OnCreateClicked), "Snapshot the current state into a new named preset");
    PresetsMenu.Menu.AddSpace(8f);
    PresetsMenu.Menu.AddLabel("Saved presets", true);
    List<PresetMeta> availablePresets = Configuration.AvailablePresets;
    if (Configuration.InitialLoadComplete)
    {
      if ((availablePresets == null ? 1 : (availablePresets.Count == 0 ? 1 : 0)) != 0)
      {
        PresetsMenu.Menu.AddLabel("(none yet)");
      }
      else
      {
        foreach (PresetMeta presetMeta in availablePresets)
        {
          string id = presetMeta.Id;
          string name = presetMeta.Name ?? "?";
          if (!presetMeta.IsPending)
          {
            PresetsMenu.Menu.AddDynamicButton((Func<string>) (() => id == Configuration.ActivePresetId ? "★ " + name : name), (Action) (() => PresetsMenu.OnLoadClicked(id, name)), "Load this preset (replaces current state)");
            PresetsMenu.Menu.AddHoldButton("  Delete", 3f, (Action) (() => PresetsMenu.OnDeleteClicked(id, name)), "Hold to delete this preset");
            PresetsMenu.Menu.AddSpace(4f);
          }
          else
            PresetsMenu.Menu.AddDynamicLabel((Func<string>) (() => $"  {name} (saving…)"));
        }
      }
    }
    else
      PresetsMenu.Menu.AddLabel("Loading…");
    PresetsMenu.Menu.AddSpace(8f);
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    PresetsMenu.Menu.AddButton("Reload Config", new Action(Configuration.LoadSettings), "Re-fetch the current state from the server");
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    PresetsMenu.Menu.AddButton("Reset to Defaults", new Action(Configuration.ResetToDefaults), "Restore every setting across every mod to its default");
    PresetsMenu.Menu.AddSpace(4f);
    PresetsMenu.Menu.AddButton("Close", (Action) (() => PresetsMenu.ShowMenu = false), "Hide this menu (toggle from Configuration in UI Manager)");
  }

  private static void OnCreateClicked()
  {
    TextFieldMenuItem nameField = PresetsMenu._nameField;
    string str1;
    if (nameField == null)
    {
      str1 = (string) null;
    }
    else
    {
      str1 = nameField.Text;
      if (str1 != null)
        goto label_4;
    }
    str1 = "";
label_4:
    string str2 = str1.Trim();
    if (string.IsNullOrEmpty(str2))
    {
      Notification.Send("Type a preset name first", Color.yellow);
    }
    else
    {
      foreach (PresetMeta availablePreset in Configuration.AvailablePresets)
      {
        if (string.Equals(availablePreset.Name, str2, StringComparison.OrdinalIgnoreCase))
        {
          Notification.Send($"Preset name \"{str2}\" already exists", Color.red);
          return;
        }
      }
      if (!Configuration.CreatePresetFromCurrent(str2))
        return;
      if (PresetsMenu._nameField != null)
        PresetsMenu._nameField.Text = "";
      Notification.Send($"Saving preset \"{str2}\"…", Color.green);
    }
  }

  private static void OnLoadClicked(string id, string name) => Configuration.LoadProfile(name);

  private static void OnDeleteClicked(string id, string name)
  {
    Configuration.DeleteProfile(name);
    Notification.Send($"Deleted \"{name}\"", Color.yellow);
  }
}
