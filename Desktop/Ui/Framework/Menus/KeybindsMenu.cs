using SakuraaCastingMod.Features.Tools;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

#nullable disable
namespace SakuraaCastingMod.Desktop.Ui.Framework.Menus;

public static class KeybindsMenu
{
  public static MenuBuilder _keybindsMenu;

  public static void Draw()
  {
    if (!Keybinds.ShowKeybindMenu)
      return;
    if (KeybindsMenu._keybindsMenu == null)
      KeybindsMenu.Initialize();
    KeybindsMenu._keybindsMenu.Draw();
    if (Keybinds.CapturingId == null)
      return;
    KeybindsMenu.HandleKeyCapture();
  }

  public static void Initialize()
  {
    Keybinds.EnsureRegistry();
    MenuBuilder menuBuilder = new MenuBuilder("Keybind Menu", 240f).SetPositionRef(400f, 100f);
    string title = (string) null;
    foreach (Keybinds.KeybindEntry keybindEntry in Keybinds.All)
    {
      if (keybindEntry.Page != title)
      {
        title = keybindEntry.Page;
        menuBuilder.AddPage(title);
      }
      Keybinds.KeybindEntry e = keybindEntry;
      menuBuilder.AddDynamicButton((Func<string>) (() => !(Keybinds.CapturingId == e.Id) ? $"{e.Label}: {e.Get()}" : e.Label + ": <press a key>"), (Action) (() => Keybinds.CapturingId = e.Id), $"Click, then press a key to rebind \"{e.Label}\".");
    }
    KeybindsMenu._keybindsMenu = menuBuilder;
  }

  private static void HandleKeyCapture()
  {
    Event current = Event.current;
    if ((!current.isKey || current.keyCode == null ? 1 : (current.type != (EventType) 4 ? 1 : 0)) != 0)
      return;
    Key key;
    try
    {
      string str = current.keyCode.ToString();
      if ((!str.StartsWith("Alpha") ? 0 : (str.Length == 6 ? 1 : 0)) != 0)
        str = "Digit" + str.Substring(5);
      key = (Key) Enum.Parse(typeof (Key), str);
    }
    catch (ArgumentException ex)
    {
      UnityEngine.Debug.LogError((object) $"[KeybindsMenu] Could not map KeyCode '{current.keyCode}' to an InputSystem.Key. Binding cancelled.");
      Keybinds.CapturingId = (string) null;
      return;
    }
    Keybinds.ApplyCapture(key);
    current.Use();
  }

  public static void ToggleMenu()
  {
    Keybinds.ShowKeybindMenu = !Keybinds.ShowKeybindMenu;
    if (Keybinds.ShowKeybindMenu)
      return;
    Keybinds.CapturingId = (string) null;
  }
}
