using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Features.Replay.UI;

internal static class UIConstants
{
  public const float CAMERA_KEY_SPACING = 7f;
  public const float TRACK_HEIGHT = 50f;
  public const float TRACK_SPACING = 3f;
  public const float GROUPING_BAR_HEIGHT = 20f;
  public const int SUB_POINTS_PER_LINE = 25;
  public const float DEFAULT_SPACE_FROM_TOP = 50f;
  public const float SETTINGS_PANEL_X = 22f;
  public const float SETTINGS_PANEL_WIDTH = 18f;
  public const float SETTINGS_PANEL_HEIGHT = 60f;
  public const float BUTTON_WIDTH = 14f;
  public const float BUTTON_HEIGHT = 2f;
  public const float FIELD_WIDTH = 6f;
  public static readonly Color CAMERA_KEY_COLOR = new Color(0.95f, 0.71f, 0.27f, 1f);
  public static readonly Color SPEED_KEY_COLOR = new Color(0.56f, 0.55f, 1f, 1f);
  public static readonly Color VOICE_MUTE_COLOR = new Color(1f, 0.44f, 0.61f, 1f);
  public static readonly Color VOICE_UNMUTE_COLOR = new Color(1f, 0.44f, 0.61f, 0.42f);
  public static readonly Color TIME_BAR_COLOR = new Color(1f, 1f, 1f, 1f);
  public static readonly Color GORILLA_GROUP_BG_COLOR = new Color(0.5f, 0.5f, 0.5f, 0.4f);
  public static readonly Color CAMERA_VISUAL_COLOR = Color.yellow;
  public static readonly Color CAMERA_DIRECTION_COLOR = Color.white;
  public static readonly Color CAMERA_ANCHOR_COLOR = Color.red;

  public static float GetTenSecondSpacing(float tenthSecondPxSpacing)
  {
    return tenthSecondPxSpacing * 1000f;
  }

  public static float GetSecondSpacing(float tenthSecondPxSpacing) => tenthSecondPxSpacing * 100f;

  public static float CalculateTimelineLength(float tenthSecondPxSpacing, float totalProjectTime)
  {
    return (float) ((double) tenthSecondPxSpacing * (double) totalProjectTime * 100.0);
  }
}
