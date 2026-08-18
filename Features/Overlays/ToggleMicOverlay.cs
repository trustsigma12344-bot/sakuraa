using SakuraaCastingMod.Core;
using SakuraaCastingMod.Shared.Helpers;
using SakuraaOfflinePresets;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Features.Overlays;

public static class ToggleMicOverlay
{
  [SavedSetting("ShowToggleMicIcon", true)]
  public static bool ShowToggleMicIcon = true;
  [SavedSetting("ToggleMicOffsetX", 0.0f)]
  public static float ToggleMicOffsetX;
  [SavedSetting("ToggleMicOffsetY", 0.0f)]
  public static float ToggleMicOffsetY;
  private const float IconHalfSize = 32f;
  private const float AnchorX = 40f;
  private const float AnchorY = 40f;

  public static Vector2 GetScreenPosition()
  {
    return new Vector2((float) Screen.width - 40f + ToggleMicOverlay.ToggleMicOffsetX, 40f + ToggleMicOverlay.ToggleMicOffsetY);
  }

  public static void UpdatePosition()
  {
    OfflinePresetStore.ImproveMicIcons();
    if ((((UnityEngine.Object) Plugin.Ins == (UnityEngine.Object) null) ? 1 : (((UnityEngine.Object) Plugin.Ins.toggleMicObj == (UnityEngine.Object) null) ? 1 : 0)) != 0)
      return;
    Plugin.Ins.toggleMicObj.transform.position = (ToggleMicOverlay.GetScreenPosition());
  }

  public static Rect GetDisplayRect()
  {
    Vector2 screenPosition = ToggleMicOverlay.GetScreenPosition();
    return new Rect(screenPosition.x - 32f, (float) ((double) Screen.height - (double) screenPosition.y - 32.0), 64f, 64f);
  }
}
