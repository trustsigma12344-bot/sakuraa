using SakuraaCastingMod.Desktop.Camera;
using System;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Desktop.Ui.Framework.Menus;

public static class NestsMenu
{
  public static MenuBuilder _nestsMenu;
  private static readonly Vector2 NestsMenuPosition = new Vector2(430f, 100f);

  public static bool IsInitialized => NestsMenu._nestsMenu != null;

  public static void Draw()
  {
    if (!NestsMenu.IsInitialized)
      NestsMenu.Initialize();
    if (!NestsMenu.IsInitialized)
      return;
    NestsMenu._nestsMenu.Draw();
  }

  public static void Initialize()
  {
    NestsMenu._nestsMenu = new MenuBuilder("Nests Options", 150f).SetPositionRef(NestsMenu.NestsMenuPosition.x, NestsMenu.NestsMenuPosition.y).AddSpace(25f).AddButton("Place Nest", (Action) (() => Observation.CreateNest()), "Create a new observation nest at the current camera position").AddButton("Save Nests", (Action) (() => Observation.SaveGameObjects(Observation.NestList)), "Save current nest positions to a file").AddButton("Load Nests", (Action) (() => Observation.LoadGameObjects()), "Load saved nest positions from a file");
  }

  public static void Reset() => NestsMenu._nestsMenu = (MenuBuilder) null;
}
