using SakuraaCastingMod.Core;
using SakuraaCastingMod.Shared.Helpers;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Features.Overlays;

public static class MiniMap
{
  [SavedSetting("MiniMapEnabled", false)]
  public static bool MiniMapEnabled;
  public static GameObject MiniMapCanvasObj;
  public static Canvas MiniMapCanvas;
  public static GameObject MiniMapObj;
  public static GameObject MiniMapCameraObj;
  public static Camera MiniMapCamera;
  [SavedSetting("MiniMapX", 150f)]
  private static float _miniMapX = 150f;
  [SavedSetting("MiniMapY", 500f)]
  private static float _miniMapY = 500f;
  [SavedSetting("MiniMapScale", 3f)]
  public static float MiniMapScale = 3f;
  [SavedSetting("MiniMapYOffset", 20f)]
  public static float MiniMapYOffset = 20f;
  [SavedSetting("MiniMapAvoidForestTop", true)]
  public static bool AvoidForestTop = true;
  [SavedSetting("MiniMapSize", 8f)]
  public static float MiniMapOrthographicSize = 8f;

  public static Vector2 MiniMapPosition
  {
    get => new Vector2(MiniMap._miniMapX, MiniMap._miniMapY);
    set
    {
      MiniMap._miniMapX = value.x;
      MiniMap._miniMapY = value.y;
    }
  }

  public static void ToggleMiniMap()
  {
    MiniMap.MiniMapEnabled = !MiniMap.MiniMapEnabled;
    MiniMap.MiniMapCanvasObj.SetActive(MiniMap.MiniMapEnabled);
  }

  public static void UpdateMap()
  {
    Vector3 position = ((Component) Plugin.Ins.camera).transform.position;
    int num = !MiniMap.AvoidForestTop ? 0 : ((double) position.y <= 34.0 ? 1 : 0);
    MiniMap.MiniMapCameraObj.transform.localPosition = num == 0 ? new Vector3(position.x, position.y + MiniMap.MiniMapYOffset, position.z) : new Vector3(position.x, 36f, position.z);
    MiniMap.MiniMapCamera.orthographicSize = MiniMap.MiniMapOrthographicSize;
    MiniMap.MiniMapObj.transform.position = (MiniMap.MiniMapPosition);
    MiniMap.MiniMapObj.transform.localScale = (Vector3.one * MiniMap.MiniMapScale);
  }
}
