using SakuraaCastingMod.Core;
using SakuraaCastingMod.Desktop.Ui.Framework;
using SakuraaCastingMod.Shared.Helpers;
using System;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Features.Overlays;

public static class RewindViewer
{
  [SavedSetting("RewindSpeed", 0.5f)]
  public static float PlaybackSpeed = 0.5f;
  [SavedSetting("RewindLength", 5f)]
  public static float BufferLengthSeconds = 5f;
  [SavedSetting("RewindEditorEnabled", false)]
  public static bool ShowEditorMenu;
  [SavedSetting("RewindScale", 1f)]
  public static float RewindScale = 1f;
  [SavedSetting("RewindPosX", 100f)]
  public static float RewindPosX = 100f;
  [SavedSetting("RewindPosY", 100f)]
  public static float RewindPosY = 100f;
  private const float BaseWidth = 384f;
  private const float BaseHeight = 216f;
  private const int CaptureWidth = 384;
  private const int CaptureHeight = 216;
  private const int CaptureFrameRate = 30;
  private static RenderTexture[] _frameBuffer;
  private static int _bufferSize;
  private static int _writeIndex;
  private static int _frameCount;
  private static float _captureTimer;
  private static float _captureInterval;
  private static bool _isPlaying;
  private static float _playbackTimer;
  private static int _snapFrameCount;
  private static int _snapWriteIndex;
  private static float _snapDuration;
  private static bool _initialized;
  private static bool _initFailed;
  private static GameObject _captureCamObj;
  private static Camera _captureCam;
  private static RenderTexture _liveRT;
  private const float HeaderHeight = 22f;
  private const float BorderThickness = 2f;
  private const int CornerRadius = 10;
  private const float ProgressBarHeight = 15f;
  private static Texture2D _frameTex;
  private static Texture2D _videoMaskTex;
  private static Texture2D _progressBgTex;
  private static Texture2D _progressFillTex;
  private static GUIStyle _headerStyle;
  private static GUIStyle _frameStyle;
  private static GUIStyle _videoStyle;

  public static void Initialize()
  {
    try
    {
      RewindViewer.CreateCaptureCamera();
      RewindViewer.RebuildBuffer();
      RewindViewer._initialized = true;
    }
    catch (Exception ex)
    {
      UnityEngine.Debug.LogError((object) $"[RewindViewer] Initialize failed: {ex}");
      RewindViewer._initFailed = true;
    }
  }

  private static void CreateCaptureCamera()
  {
    if (((UnityEngine.Object) RewindViewer._captureCam != (UnityEngine.Object) null))
      return;
    Camera camera = Plugin.Ins.camera;
    if (((UnityEngine.Object) camera == (UnityEngine.Object) null))
      return;
    RewindViewer._liveRT = new RenderTexture(384, 216, 16 /*0x10*/, (RenderTextureFormat) 0);
    RewindViewer._liveRT.Create();
    RewindViewer._captureCamObj = new GameObject("RewindCaptureCam");
    RewindViewer._captureCamObj.transform.parent = ((Component) camera).transform;
    RewindViewer._captureCamObj.transform.localPosition = Vector3.zero;
    RewindViewer._captureCamObj.transform.localRotation = Quaternion.identity;
    RewindViewer._captureCam = RewindViewer._captureCamObj.AddComponent<Camera>();
    RewindViewer._captureCam.targetTexture = RewindViewer._liveRT;
    ((Behaviour) RewindViewer._captureCam).enabled = false;
  }

  private static void SyncCameraSettings()
  {
    Camera camera = Plugin.Ins.camera;
    if ((((UnityEngine.Object) camera == (UnityEngine.Object) null) ? 1 : (((UnityEngine.Object) RewindViewer._captureCam == (UnityEngine.Object) null) ? 1 : 0)) != 0)
      return;
    RewindViewer._captureCam.fieldOfView = camera.fieldOfView;
    RewindViewer._captureCam.nearClipPlane = camera.nearClipPlane;
    RewindViewer._captureCam.farClipPlane = camera.farClipPlane;
    RewindViewer._captureCam.cullingMask = camera.cullingMask & -524289;
    RewindViewer._captureCam.clearFlags = camera.clearFlags;
    RewindViewer._captureCam.backgroundColor = camera.backgroundColor;
    RewindViewer._captureCam.renderingPath = camera.renderingPath;
    RewindViewer._captureCam.allowHDR = camera.allowHDR;
    RewindViewer._captureCam.allowMSAA = camera.allowMSAA;
    RewindViewer._captureCam.depth = camera.depth - 1f;
  }

  public static void RebuildBuffer()
  {
    if (RewindViewer._frameBuffer != null)
    {
      foreach (RenderTexture renderTexture in RewindViewer._frameBuffer)
      {
        if (((UnityEngine.Object) renderTexture != (UnityEngine.Object) null))
        {
          renderTexture.Release();
          UnityEngine.Object.Destroy((UnityEngine.Object) renderTexture);
        }
      }
    }
    RewindViewer._captureInterval = 0.0333333351f;
    RewindViewer._bufferSize = Mathf.CeilToInt(RewindViewer.BufferLengthSeconds * 30f);
    RewindViewer._frameBuffer = new RenderTexture[RewindViewer._bufferSize];
    for (int index = 0; index < RewindViewer._bufferSize; ++index)
    {
      RewindViewer._frameBuffer[index] = new RenderTexture(384, 216, 0, (RenderTextureFormat) 0);
      RewindViewer._frameBuffer[index].Create();
    }
    RewindViewer._writeIndex = 0;
    RewindViewer._frameCount = 0;
    RewindViewer._captureTimer = 0.0f;
    RewindViewer._isPlaying = false;
    RewindViewer._playbackTimer = 0.0f;
  }

  public static void Toggle()
  {
    if (!RewindViewer.ShowEditorMenu)
      return;
    RewindViewer._isPlaying = !RewindViewer._isPlaying;
    if (!RewindViewer._initialized)
      RewindViewer.Initialize();
    if (!RewindViewer._isPlaying)
      return;
    RewindViewer._snapFrameCount = RewindViewer._frameCount;
    RewindViewer._snapWriteIndex = RewindViewer._writeIndex;
    RewindViewer._snapDuration = (float) RewindViewer._snapFrameCount * RewindViewer._captureInterval;
    RewindViewer._playbackTimer = 0.0f;
  }

  public static void CaptureFrame()
  {
    if (RewindViewer._initFailed)
      return;
    if (RewindViewer.ShowEditorMenu)
    {
      if (!RewindViewer._initialized)
      {
        RewindViewer.Initialize();
      }
      else
      {
        if ((!((UnityEngine.Object) RewindViewer._captureCam != (UnityEngine.Object) null) ? 0 : (!((Behaviour) RewindViewer._captureCam).enabled ? 1 : 0)) != 0)
          ((Behaviour) RewindViewer._captureCam).enabled = true;
        if ((RewindViewer._frameBuffer == null ? 1 : (((UnityEngine.Object) RewindViewer._liveRT == (UnityEngine.Object) null) ? 1 : 0)) != 0 || RewindViewer._isPlaying)
          return;
        RewindViewer.SyncCameraSettings();
        RewindViewer._captureTimer += Time.unscaledDeltaTime;
        if ((double) RewindViewer._captureTimer < (double) RewindViewer._captureInterval)
          return;
        RewindViewer._captureTimer -= RewindViewer._captureInterval;
        Graphics.Blit((Texture) RewindViewer._liveRT, RewindViewer._frameBuffer[RewindViewer._writeIndex]);
        RewindViewer._writeIndex = (RewindViewer._writeIndex + 1) % RewindViewer._bufferSize;
        RewindViewer._frameCount = Mathf.Min(RewindViewer._frameCount + 1, RewindViewer._bufferSize);
      }
    }
    else
    {
      if ((!((UnityEngine.Object) RewindViewer._captureCam != (UnityEngine.Object) null) ? 0 : (((Behaviour) RewindViewer._captureCam).enabled ? 1 : 0)) == 0)
        return;
      ((Behaviour) RewindViewer._captureCam).enabled = false;
    }
  }

  public static void UpdatePlayback()
  {
    if ((!RewindViewer._isPlaying ? 1 : (RewindViewer._snapFrameCount == 0 ? 1 : 0)) != 0)
      return;
    RewindViewer._playbackTimer += Time.unscaledDeltaTime * RewindViewer.PlaybackSpeed;
    if ((double) RewindViewer._playbackTimer < (double) RewindViewer._snapDuration)
      return;
    RewindViewer._isPlaying = false;
  }

  private static RenderTexture GetPlaybackFrame()
  {
    RenderTexture playbackFrame;
    if ((RewindViewer._snapFrameCount == 0 ? 1 : (RewindViewer._frameBuffer == null ? 1 : 0)) != 0)
    {
      playbackFrame = (RenderTexture) null;
    }
    else
    {
      int num = Mathf.Clamp(Mathf.FloorToInt(Mathf.Clamp01(RewindViewer._playbackTimer / RewindViewer._snapDuration) * (float) (RewindViewer._snapFrameCount - 1)), 0, RewindViewer._snapFrameCount - 1);
      int index = ((RewindViewer._snapFrameCount < RewindViewer._bufferSize ? 0 : RewindViewer._snapWriteIndex) + num) % RewindViewer._bufferSize;
      playbackFrame = RewindViewer._frameBuffer[index];
    }
    return playbackFrame;
  }

  public static Rect GetDisplayRect()
  {
    float num1 = 384f * RewindViewer.RewindScale;
    float num2 = 216f * RewindViewer.RewindScale;
    return new Rect(RewindViewer.RewindPosX, RewindViewer.RewindPosY, num1, num2);
  }

  private static Texture2D CreateRoundedRect(
    int w,
    int h,
    Color fill,
    Color outline,
    int radius,
    float border,
    bool roundTop = true,
    bool roundBottom = true)
  {
    Texture2D roundedRect = new Texture2D(w, h, (TextureFormat) 5, false);
    Color color;
    // ISSUE: explicit constructor call
    color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
    for (int index1 = 0; index1 < h; ++index1)
    {
      for (int index2 = 0; index2 < w; ++index2)
      {
        float num1 = 0.0f;
        float num2 = 0.0f;
        bool flag1 = false;
        bool flag2 = false;
        bool flag3 = false;
        if ((index2 >= radius ? 0 : (index1 > h - radius - 1 ? 1 : 0)) == 0)
        {
          if ((index2 <= w - radius - 1 ? 0 : (index1 > h - radius - 1 ? 1 : 0)) == 0)
          {
            if ((index2 >= radius ? 0 : (index1 < radius ? 1 : 0)) == 0)
            {
              if ((index2 <= w - radius - 1 ? 0 : (index1 < radius ? 1 : 0)) != 0)
              {
                num1 = (float) (index2 - (w - radius - 1));
                num2 = (float) (index1 - radius);
                flag1 = true;
                flag3 = true;
              }
            }
            else
            {
              num1 = (float) (index2 - radius);
              num2 = (float) (index1 - radius);
              flag1 = true;
              flag3 = true;
            }
          }
          else
          {
            num1 = (float) (index2 - (w - radius - 1));
            num2 = (float) (index1 - (h - radius - 1));
            flag1 = true;
            flag2 = true;
          }
        }
        else
        {
          num1 = (float) (index2 - radius);
          num2 = (float) (index1 - (h - radius - 1));
          flag1 = true;
          flag2 = true;
        }
        if (!flag1)
        {
          if (((double) index2 < (double) border || (double) index2 >= (double) w - (double) border || (double) index1 < (double) border ? 1 : ((double) index1 >= (double) h - (double) border ? 1 : 0)) == 0)
            roundedRect.SetPixel(index2, index1, fill);
          else
            roundedRect.SetPixel(index2, index1, outline);
        }
        else if ((flag2 & roundTop ? 1 : (flag3 & roundBottom ? 1 : 0)) == 0)
        {
          if (((double) index2 < (double) border || (double) index2 >= (double) w - (double) border || (double) index1 < (double) border ? 1 : ((double) index1 >= (double) h - (double) border ? 1 : 0)) == 0)
            roundedRect.SetPixel(index2, index1, fill);
          else
            roundedRect.SetPixel(index2, index1, outline);
        }
        else
        {
          float num3 = Mathf.Sqrt((float) ((double) num1 * (double) num1 + (double) num2 * (double) num2));
          if ((double) num3 <= (double) radius)
          {
            if ((double) num3 > (double) radius - (double) border)
              roundedRect.SetPixel(index2, index1, outline);
            else
              roundedRect.SetPixel(index2, index1, fill);
          }
          else
            roundedRect.SetPixel(index2, index1, color);
        }
      }
    }
    roundedRect.Apply();
    return roundedRect;
  }

  private static void EnsureStyles()
  {
    Color menuOutlineColor = MenuConfig.MenuOutlineColor;
    Color fill;
    // ISSUE: explicit constructor call
    fill = new Color(0.0470588244f, 0.0470588244f, 0.05490196f, 0.92f);
    if (((UnityEngine.Object) RewindViewer._frameTex == (UnityEngine.Object) null))
      RewindViewer._frameTex = RewindViewer.CreateRoundedRect(64 /*0x40*/, 64 /*0x40*/, fill, new Color(menuOutlineColor.r, menuOutlineColor.g, menuOutlineColor.b, 0.6f), 10, 2f);
    if (((UnityEngine.Object) RewindViewer._videoMaskTex == (UnityEngine.Object) null))
      RewindViewer._videoMaskTex = RewindViewer.CreateRoundedRect(64 /*0x40*/, 64 /*0x40*/, Color.black, Color.black, 8, 0.0f, false);
    if (((UnityEngine.Object) RewindViewer._progressBgTex == (UnityEngine.Object) null))
    {
      RewindViewer._progressBgTex = new Texture2D(1, 1);
      RewindViewer._progressBgTex.SetPixel(0, 0, new Color(1f, 1f, 1f, 0.15f));
      RewindViewer._progressBgTex.Apply();
    }
    if (((UnityEngine.Object) RewindViewer._progressFillTex == (UnityEngine.Object) null))
    {
      RewindViewer._progressFillTex = new Texture2D(1, 1);
      RewindViewer._progressFillTex.SetPixel(0, 0, menuOutlineColor);
      RewindViewer._progressFillTex.Apply();
    }
    if (RewindViewer._frameStyle == null)
      RewindViewer._frameStyle = new GUIStyle()
      {
        normal = {
          background = RewindViewer._frameTex
        },
        border = new RectOffset(10, 10, 10, 10)
      };
    if (RewindViewer._videoStyle == null)
      RewindViewer._videoStyle = new GUIStyle()
      {
        normal = {
          background = RewindViewer._videoMaskTex
        },
        border = new RectOffset(10, 10, 10, 10)
      };
    if (RewindViewer._headerStyle != null)
      return;
    RewindViewer._headerStyle = new GUIStyle(GUI.skin.label)
    {
      fontSize = 11,
      fontStyle = (FontStyle) 1,
      alignment = (TextAnchor) 4,
      normal = {
        textColor = new Color(menuOutlineColor.r, menuOutlineColor.g, menuOutlineColor.b, 0.9f)
      },
      padding = new RectOffset(0, 0, 2, 0)
    };
  }

  public static void DrawRewindWindow()
  {
    if ((!RewindViewer._isPlaying ? 1 : (RewindViewer._frameBuffer == null ? 1 : 0)) != 0)
      return;
    RewindViewer.EnsureStyles();
    Rect displayRect = RewindViewer.GetDisplayRect();
    float num1 = displayRect.height + 15f;
    Rect rect1;
    // ISSUE: explicit constructor call
    rect1 = new Rect(displayRect.x - 2f, displayRect.y - 2f, displayRect.width + 4f, num1 + 4f);
    GUI.Box(rect1, GUIContent.none, RewindViewer._frameStyle);
    RenderTexture playbackFrame = RewindViewer.GetPlaybackFrame();
    if (((UnityEngine.Object) playbackFrame != (UnityEngine.Object) null))
      GUI.DrawTexture(displayRect, (Texture) playbackFrame, (ScaleMode) 2);
    else
      GUI.Box(displayRect, "No frames captured?? Erm?");
    Rect rect2;
    // ISSUE: explicit constructor call
    rect2 = new Rect(displayRect.x, displayRect.y, displayRect.width, 22f);
    GUI.Label(rect2, "INSTANT REWIND", RewindViewer._headerStyle);
    float num2 = (double) RewindViewer._snapDuration > 0.0 ? Mathf.Clamp01(RewindViewer._playbackTimer / RewindViewer._snapDuration) : 0.0f;
    Rect rect3;
    // ISSUE: explicit constructor call
    rect3 = new Rect(displayRect.x, displayRect.y + displayRect.height, displayRect.width, 15f);
    GUI.DrawTexture(rect3, (Texture) RewindViewer._progressBgTex);
    if ((double) num2 <= 0.0)
      return;
    Rect rect4;
    // ISSUE: explicit constructor call
    rect4 = new Rect(rect3.x, rect3.y, rect3.width * num2, 15f);
    GUI.DrawTexture(rect4, (Texture) RewindViewer._progressFillTex);
  }
}
