using SakuraaCastingMod.Shared.Helpers;

#nullable disable
namespace SakuraaCastingMod.Desktop.ControlMode;

public static class ControlModeSettings
{
  [SavedSetting("CmFov", 90f)]
  public static float Fov = 90f;
  [SavedSetting("CmClipping", 0.01f)]
  public static float Clipping = 0.01f;
  [SavedSetting("CmWalkSpeed", 1f)]
  public static float WalkSpeedMultiplier = 1f;
  [SavedSetting("CmSprintMultiplier", 2f)]
  public static float SprintMultiplier = 2f;
  [SavedSetting("CmFlySpeed", 1f)]
  public static float FlySpeedMultiplier = 1f;
  [SavedSetting("CmTurnSpeed", 1f)]
  public static float TurnSpeed = 1f;
}
