using SakuraaCastingMod.Features.Overlays;
using System;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Desktop.Ui.Framework.Menus;

public static class MiniMapMenu
{
  public static MenuBuilder _miniMapMenu;
  private static readonly Vector2 DefaultMenuPosition = new Vector2(100f, 400f);

  public static void Draw()
  {
    if (MiniMapMenu._miniMapMenu == null)
      MiniMapMenu.Initialize();
    if ((MiniMapMenu._miniMapMenu == null ? 0 : (MiniMap.MiniMapEnabled ? 1 : 0)) == 0)
      return;
    MiniMapMenu._miniMapMenu.Draw();
  }

  public static void Initialize()
  {
    MiniMapMenu._miniMapMenu = new MenuBuilder("MiniMap Options", 200f).SetPositionRef(MiniMapMenu.DefaultMenuPosition.x, MiniMapMenu.DefaultMenuPosition.y).AddSpace(15f).AddSlider("Scale:", MiniMap.MiniMapScale, 0.5f, 6f, (Action<float>) (value => MiniMap.MiniMapScale = value), 1, "Adjust minimap scale").AddSlider("Y Offset:", MiniMap.MiniMapYOffset, 0.0f, 50f, (Action<float>) (value => MiniMap.MiniMapYOffset = value), 1, "Adjust minimap camera vertical offset").AddSlider("Camera Size:", MiniMap.MiniMapOrthographicSize, 1f, 20f, (Action<float>) (value => MiniMap.MiniMapOrthographicSize = value), 1, "Adjust minimap camera orthographic size").AddDynamicToggle("Avoid Forest Top", (Func<bool>) (() => MiniMap.AvoidForestTop), (Action<bool>) (v => MiniMap.AvoidForestTop = v), "Toggle preventing the minimap camera from going below forest canopy level").AddButton("Reset Position", (Action) (() =>
    {
      MiniMap.MiniMapPosition = new Vector2(150f, (float) (Screen.height - 150));
      MiniMap.MiniMapScale = 3f;
      MiniMap.MiniMapYOffset = 20f;
    }), "Reset minimap position, scale, and offset to defaults");
  }

  public static void DrawMiniMapMenu()
  {
    if (MiniMapMenu._miniMapMenu == null)
      MiniMapMenu.Initialize();
    MiniMapMenu.Draw();
  }
}
