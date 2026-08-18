using SakuraaCastingMod.Desktop.Ui.Framework;
using SakuraaCastingMod.Features.Replay.CustomCamera;
using SakuraaCastingMod.Features.Replay.EditJson;
using System;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Features.Replay.UI;

internal static class KeyframeEditMenu
{
  public static bool ShowMenu = false;
  private static MenuBuilder _menu;
  private static IKeyFrame _selected;
  private static ContentUI.KeyFrameType _type;
  private static bool _dirty;
  private static Action _onClosed;
  private static readonly string[] MovementTypes = new string[4]
  {
    "Static",
    "Linear",
    "Curve",
    "1st Person"
  };
  private static readonly string[] RotationTypes = new string[3]
  {
    "Linear",
    "Look At",
    "1st Person"
  };

  public static void Init(Action onClosed) => KeyframeEditMenu._onClosed = onClosed;

  public static void Show(IKeyFrame frame, ContentUI.KeyFrameType type)
  {
    KeyframeEditMenu._selected = frame;
    KeyframeEditMenu._type = type;
    KeyframeEditMenu.ShowMenu = frame != null && type != 0;
    KeyframeEditMenu._dirty = true;
  }

  public static void Hide()
  {
    KeyframeEditMenu.ShowMenu = false;
    KeyframeEditMenu._selected = (IKeyFrame) null;
  }

  public static void Draw()
  {
    if ((!KeyframeEditMenu.ShowMenu ? 1 : (KeyframeEditMenu._selected == null ? 1 : 0)) != 0)
      return;
    if (KeyframeEditMenu._menu == null)
      KeyframeEditMenu._menu = new MenuBuilder("Keyframe", 270f).SetPositionRef(70f, 90f);
    if (KeyframeEditMenu._dirty)
    {
      KeyframeEditMenu.Rebuild();
      KeyframeEditMenu._dirty = false;
    }
    KeyframeEditMenu._menu.Draw();
  }

  private static void Rebuild()
  {
    KeyframeEditMenu._menu.Items.Clear();
    switch (KeyframeEditMenu._type)
    {
      case ContentUI.KeyFrameType.Speed:
        KeyframeEditMenu.BuildSpeed((SpeedKeyframe) KeyframeEditMenu._selected);
        break;
      case ContentUI.KeyFrameType.Camera:
        KeyframeEditMenu.BuildCamera((CameraKeyFrame) KeyframeEditMenu._selected);
        break;
      case ContentUI.KeyFrameType.Voice:
        KeyframeEditMenu.BuildVoice((VoiceKeyFrame) KeyframeEditMenu._selected);
        break;
    }
  }

  private static void BuildCamera(CameraKeyFrame key)
  {
    KeyframeEditMenu._menu.AddHoldButton("Delete Keyframe", 2f, (Action) (() =>
    {
      CameraController.instance.DeleteCamera(key);
      KeyframeEditMenu.Close();
    }), "Hold to delete this camera keyframe");
    KeyframeEditMenu._menu.AddDynamicLabel((Func<string>) (() => $"Time: {SpeedManager.RealTimeToSpeedTime(key.Time):0.0}s"));
    KeyframeEditMenu._menu.AddSection("Movement");
    KeyframeEditMenu._menu.AddDynamicButton((Func<string>) (() => "Move: " + KeyframeEditMenu.MovementTypes[Mathf.Clamp(key.CameraMovmentType, 0, 3)]), (Action) (() =>
    {
      ReplayHistory.Record();
      key.CameraMovmentType = (key.CameraMovmentType + 1) % 4;
      KeyframeEditMenu._dirty = true;
    }), "Click to cycle how the camera travels to the next keyframe");
    if (key.CameraMovmentType == 2)
      KeyframeEditMenu._menu.AddDynamicToggle("Main Curve", (Func<bool>) (() => key.cameraMoventCurveSettings.UseMainCurve), (Action<bool>) (v =>
      {
        ReplayHistory.Record();
        key.cameraMoventCurveSettings.UseMainCurve = v;
      }), "Use the curved path between keyframes");
    if (key.CameraMovmentType != 0)
    {
      KeyframeEditMenu._menu.AddDynamicToggle("Smoothing", (Func<bool>) (() => key.cameraSmoothingSettings.UseSmoothing), (Action<bool>) (v =>
      {
        ReplayHistory.Record();
        key.cameraSmoothingSettings.UseSmoothing = v;
        KeyframeEditMenu._dirty = true;
      }), "Ease the camera's motion");
      if (key.cameraSmoothingSettings.UseSmoothing)
        KeyframeEditMenu._menu.AddSlider("Amount:", key.cameraSmoothingSettings.SmoothingLevel, 1f, 50f, (Action<float>) (v => key.cameraSmoothingSettings.SmoothingLevel = v), description: "Higher = smoother / slower to settle");
    }
    if ((key.CameraMovmentType == 1 ? 1 : (key.CameraMovmentType == 2 ? 1 : 0)) != 0)
      KeyframeEditMenu._menu.AddDynamicLabel((Func<string>) (() => KeyframeEditMenu.PathInfo(key)));
    KeyframeEditMenu._menu.AddSection("Rotation");
    KeyframeEditMenu._menu.AddDynamicButton((Func<string>) (() => "Aim: " + KeyframeEditMenu.RotationTypes[Mathf.Clamp(key.CameraRotationType - 1, 0, 2)]), (Action) (() =>
    {
      ReplayHistory.Record();
      key.CameraRotationType = key.CameraRotationType % 3 + 1;
      KeyframeEditMenu._dirty = true;
    }), "Click to cycle how the camera aims");
    if (KeyframeEditMenu.RequiresPlayerTarget(key))
    {
      KeyframeEditMenu._menu.AddSection("Player Target");
      KeyframeEditMenu._menu.AddSlider("Player ID:", (float) key.TargetPlayerID, 1f, 30f, (Action<float>) (v => key.TargetPlayerID = Mathf.RoundToInt(v)), description: "Which player the camera tracks");
      if (KeyframeEditMenu.IsFirstPerson(key))
      {
        KeyframeEditMenu._menu.AddSlider("Offset X:", key.firstPersonSettings.OffsetX, -1f, 1f, (Action<float>) (v => key.firstPersonSettings.OffsetX = v), 2, "View offset from the head");
        KeyframeEditMenu._menu.AddSlider("Offset Y:", key.firstPersonSettings.OffsetY, -1f, 1f, (Action<float>) (v => key.firstPersonSettings.OffsetY = v), 2, "View offset from the head");
        KeyframeEditMenu._menu.AddSlider("Offset Z:", key.firstPersonSettings.OffsetZ, -1f, 1f, (Action<float>) (v => key.firstPersonSettings.OffsetZ = v), 2, "View offset from the head");
      }
    }
    KeyframeEditMenu._menu.AddSection("Camera");
    KeyframeEditMenu._menu.AddSlider("FOV:", key.FOV, 20f, 150f, (Action<float>) (v => key.FOV = v), description: "Field of view");
    KeyframeEditMenu._menu.AddSlider("Near Clip:", key.NearClip, 0.01f, 1f, (Action<float>) (v => key.NearClip = v), 2, "Near clipping plane");
    KeyframeEditMenu._menu.AddSlider("Far Clip:", key.FarClip, 100f, 10000f, (Action<float>) (v => key.FarClip = v), description: "Far clipping plane");
  }

  private static void BuildSpeed(SpeedKeyframe frame)
  {
    KeyframeEditMenu._menu.AddHoldButton("Delete Keyframe", 2f, (Action) (() =>
    {
      SpeedManager.DeleteFrame(frame);
      KeyframeEditMenu.Close();
    }), "Hold to delete this speed keyframe");
    KeyframeEditMenu._menu.AddDynamicLabel((Func<string>) (() => $"Time: {SpeedManager.RealTimeToSpeedTime(frame.Time):0.0}s"));
    KeyframeEditMenu._menu.AddSlider("Speed:", frame.SpeedValue, 0.05f, 4f, (Action<float>) (v => frame.SpeedValue = v), 2, "Playback speed from this point (under 1 = slow-motion)");
  }

  private static void BuildVoice(VoiceKeyFrame frame)
  {
    KeyframeEditMenu._menu.AddHoldButton("Delete Keyframe", 2f, (Action) (() =>
    {
      VoiceEditManager.DeleteFrame(frame);
      KeyframeEditMenu.Close();
    }), "Hold to delete this voice keyframe");
    KeyframeEditMenu._menu.AddLabel($"Actor: {frame.ActorNumber}");
    KeyframeEditMenu._menu.AddDynamicLabel((Func<string>) (() => $"Time: {frame.Time:0.0}s"));
    KeyframeEditMenu._menu.AddDynamicToggle("Voice", (Func<bool>) (() => !frame.Muted), (Action<bool>) (v =>
    {
      ReplayHistory.Record();
      frame.Muted = !v;
    }), "Toggle whether this keyframe mutes or unmutes the player");
  }

  private static void Close()
  {
    KeyframeEditMenu.Hide();
    Action onClosed = KeyframeEditMenu._onClosed;
    if (onClosed == null)
      return;
    onClosed();
  }

  private static string PathInfo(CameraKeyFrame key)
  {
    CameraKeyFrame nextKeyFrame = CameraController.instance.GetNextKeyFrame(key.Time);
    string str;
    if (nextKeyFrame != null)
    {
      float cameraPathDistance = CameraController.instance.GetCameraPathDistance(key, nextKeyFrame);
      float num1 = SpeedManager.RealTimeToSpeedTime(nextKeyFrame.Time) - SpeedManager.RealTimeToSpeedTime(key.Time);
      float num2 = (double) num1 != 0.0 ? cameraPathDistance / num1 : 0.0f;
      str = $"Path: {cameraPathDistance:0.0}u  ·  {num2:0.00}/s";
    }
    else
      str = "Path: last keyframe";
    return str;
  }

  private static bool RequiresPlayerTarget(CameraKeyFrame key)
  {
    return key.CameraMovmentType == 3 || key.CameraRotationType == 2 || key.CameraRotationType == 3;
  }

  private static bool IsFirstPerson(CameraKeyFrame key)
  {
    return key.CameraMovmentType == 3 || key.CameraRotationType == 3;
  }
}
