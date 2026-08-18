using SakuraaCastingMod.Features.World;
using SakuraaCastingMod.Shared.Helpers;
using System;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Desktop.Ui.Framework.Menus;

public static class TeleporterMenu
{
  public static MenuBuilder _teleporterMenu;
  private static Vector2 _menuPosition = new Vector2(400f, 300f);

  public static void Draw()
  {
    if (SakTeleporter.ShowMenu)
    {
      if (TeleporterMenu._teleporterMenu == null)
        TeleporterMenu.Initialize();
      if (TeleporterMenu._teleporterMenu == null)
        return;
      TeleporterMenu._teleporterMenu.Draw();
      TeleporterMenu._menuPosition = TeleporterMenu._teleporterMenu.MenuRect.position;
    }
    else
    {
      if (TeleporterMenu._teleporterMenu == null)
        return;
      TeleporterMenu.Reset();
    }
  }

  public static void Initialize() => TeleporterMenu.RebuildMenu();

  private static void RebuildMenu()
  {
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
    TeleporterMenu._teleporterMenu = new MenuBuilder("Teleporter", 250f).SetPosition(TeleporterMenu._menuPosition.x, TeleporterMenu._menuPosition.y).AddSpace(8f).AddLabel("Only works outside a lobby", description: "Teleport buttons are disabled while you're in a lobby. Return to single-player to use them.", visibilityCondition: new Func<bool>(Networking.CheckNotInRoom)).AddLabel("In a lobby, teleporting disabled.\nLeave the lobby to teleport.", true, visibilityCondition: new Func<bool>(Networking.CheckInRoom)).AddSpace(7f, new Func<bool>(Networking.CheckNotInRoom)).AddButton("Tp in Stump", (Action) (() => SakTeleporter.AttemptToTeleportTo(new Vector3(-68.5886f, 12.0883f, -83.9574f), Quaternion.Euler(0.0584f, -0.3177f, -0.259f))), "Teleport inside the Tree Stump", new Func<bool>(Networking.CheckNotInRoom)).AddButton("Tp on Stairs", (Action) (() => SakTeleporter.AttemptToTeleportTo(new Vector3(-60.8484f, 4.0272f, -62.0698f), new Quaternion())), "Teleport onto the Tree Stump stairs", new Func<bool>(Networking.CheckNotInRoom)).AddButton("Tp under table", (Action) (() => SakTeleporter.AttemptToTeleportTo(new Vector3(-63.3356f, 2.3749f, -63.3645f), new Quaternion())), "Teleport under the table in the Tree Stump", new Func<bool>(Networking.CheckNotInRoom));
  }

  public static void Reset() => TeleporterMenu._teleporterMenu = (MenuBuilder) null;
}
