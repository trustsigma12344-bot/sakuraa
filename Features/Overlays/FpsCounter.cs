using SakuraaCastingMod.Shared.Helpers;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Features.Overlays;

public static class FpsCounter
{
  [SavedSetting("LocalFpsCounterDisplayFps", false)]
  private static bool _showFps;
  [SavedSetting("LocalFpsCapped", true)]
  public static bool FPSCapped = true;
  public static int _currentFps;
  private static int _fpsFrames;
  private static float _fpsAccumulatedTime;

  public static bool ShowFps
  {
    get => FpsCounter._showFps;
    set
    {
      FpsCounter._showFps = value;
      if (!FpsCounter._showFps)
        return;
      FpsCounter._fpsFrames = 0;
      FpsCounter._fpsAccumulatedTime = 0.0f;
    }
  }

  public static void UpdateCheck()
  {
    if (!FpsCounter.ShowFps)
      return;
    FpsCounter._fpsAccumulatedTime += Time.unscaledDeltaTime;
    ++FpsCounter._fpsFrames;
    if ((double) FpsCounter._fpsAccumulatedTime < 0.5)
      return;
    FpsCounter._currentFps = Mathf.RoundToInt((float) FpsCounter._fpsFrames / FpsCounter._fpsAccumulatedTime);
    FpsCounter._fpsFrames = 0;
    FpsCounter._fpsAccumulatedTime = 0.0f;
  }

  public static void FixedUpdateCheck()
  {
    if (FpsCounter.FPSCapped)
    {
      Application.targetFrameRate = 144 /*0x90*/;
    }
    else
    {
      QualitySettings.vSyncCount = 0;
      Application.targetFrameRate = int.MaxValue;
    }
  }

  public static void Draw()
  {
    GUIStyle guiStyle1 = new GUIStyle(GUI.skin.label)
    {
      fontSize = 20,
      alignment = (TextAnchor) 2,
      normal = {
        textColor = Color.white
      }
    };
    GUIStyle guiStyle2 = new GUIStyle(guiStyle1)
    {
      normal = {
        textColor = Color.black
      }
    };
    string str = $"FPS: {FpsCounter._currentFps}";
    float num1 = 10f;
    Vector2 vector2 = guiStyle1.CalcSize(new GUIContent(str));
    float num2 = vector2.x + 5f;
    Rect rect1;
    // ISSUE: explicit constructor call
    rect1 = new Rect((float) ((double) Screen.width - (double) num2 - (double) num1 + (double) LayoutEditor.FpsOffsetX + 2.0), (float) ((double) num1 + (double) LayoutEditor.FpsOffsetY + 2.0), num2, vector2.y);
    Rect rect2;
    // ISSUE: explicit constructor call
    rect2 = new Rect((float) Screen.width - num2 - num1 + LayoutEditor.FpsOffsetX, num1 + LayoutEditor.FpsOffsetY, num2, vector2.y);
    GUI.Label(rect1, str, guiStyle2);
    GUI.Label(rect2, str, guiStyle1);
  }
}
