using SakuraaCastingMod.Desktop.Ui.Framework;
using SakuraaCastingMod.Features.Replay.EditJson;
using System;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Features.Replay.UI;

internal class TimelineRenderer
{
  private GorillaController gorillaController;
  private ReplayProject replayProject;
  private UiManager uiManager;
  private Action<CameraKeyFrame> onCameraKeyFrameSelected;
  private Action<SpeedKeyframe> onSpeedKeyframeSelected;
  private Action<float> onTimeChanged;
  private Action<IKeyFrame, float> onKeyFrameTimeMoved;
  private Action<VoiceKeyFrame> onVoiceKeyFrameSelected;
  private bool _resizingZoom;

  public Vector2 ScrollPosition { get; set; } = Vector2.zero;

  public TimelineRenderer(
    GorillaController gorillaController,
    ReplayProject replayProject,
    UiManager uiManager,
    Action<CameraKeyFrame> onCameraKeyFrameSelected,
    Action<SpeedKeyframe> onSpeedKeyframeSelected,
    Action<float> onTimeChanged,
    Action<IKeyFrame, float> onKeyFrameTimeMoved,
    Action<VoiceKeyFrame> onVoiceKeyFrameSelected)
  {
    this.gorillaController = gorillaController;
    this.replayProject = replayProject;
    this.uiManager = uiManager;
    this.onCameraKeyFrameSelected = onCameraKeyFrameSelected;
    this.onSpeedKeyframeSelected = onSpeedKeyframeSelected;
    this.onTimeChanged = onTimeChanged;
    this.onKeyFrameTimeMoved = onKeyFrameTimeMoved;
    this.onVoiceKeyFrameSelected = onVoiceKeyFrameSelected;
  }

  public float Render(float tenthSecondPxSpacing, float currentTime)
  {
    float projectTimeWithSpeed = SpeedManager.GetTotalProjectTimeWithSpeed();
    float timelineLength = UIConstants.CalculateTimelineLength(tenthSecondPxSpacing, projectTimeWithSpeed);
    float scrollBoxHeight = 50f * (float) (this.gorillaController.Gorillas.Count + 5);
    float num1 = (float) Screen.height - this.uiManager.VH * 30f;
    float num2 = this.uiManager.VH * 20f;
    float num3 = Mathf.Min(timelineLength, (float) Screen.width);
    Rect box;
    // ISSUE: explicit constructor call
    box = new Rect(0.0f, num1, num3, num2);
    tenthSecondPxSpacing = this.HandleZoomDrag(box, projectTimeWithSpeed, tenthSecondPxSpacing);
    this.ScrollPosition = GUI.BeginScrollView(box, this.ScrollPosition, new Rect(0.0f, 0.0f, timelineLength, scrollBoxHeight), GUI.skin.horizontalScrollbar, GUIStyle.none);
    GUI.Box(new Rect(0.0f, 0.0f, timelineLength, scrollBoxHeight), "", MenuConfig.GetMenuBoxStyle());
    this.RenderCameraKeyFrames(tenthSecondPxSpacing);
    this.RenderSpeedKeyFrames(tenthSecondPxSpacing);
    this.RenderTimeMarkers(tenthSecondPxSpacing);
    this.RenderGorillaTracks(tenthSecondPxSpacing);
    this.RenderCurrentTimeBar(tenthSecondPxSpacing, currentTime, scrollBoxHeight);
    this.HandleTimelineInput(tenthSecondPxSpacing, projectTimeWithSpeed);
    GUI.EndScrollView();
    this.DrawZoomHandle(box);
    return tenthSecondPxSpacing;
  }

  private float HandleZoomDrag(Rect box, float total, float tenthSecondPxSpacing)
  {
    float num;
    if ((double) total <= 0.0)
    {
      num = tenthSecondPxSpacing;
    }
    else
    {
      Rect rect;
      // ISSUE: explicit constructor call
      rect = new Rect(box.xMax - 8f, box.y, 16f, box.height);
      Event current = Event.current;
      switch ((int) current.type)
      {
        case 0:
          if (current.button == 0 && rect.Contains(current.mousePosition))
          {
            this._resizingZoom = true;
            current.Use();
            break;
          }
          break;
        case 1:
          if (this._resizingZoom && current.button == 0)
          {
            this._resizingZoom = false;
            current.Use();
            break;
          }
          break;
        case 3:
          if (this._resizingZoom)
          {
            tenthSecondPxSpacing = Mathf.Clamp(current.mousePosition.x / (total * 100f), 0.02f, 5f);
            current.Use();
            break;
          }
          break;
      }
      num = tenthSecondPxSpacing;
    }
    return num;
  }

  private void DrawZoomHandle(Rect box)
  {
    if (Event.current.type != (EventType) 7)
      return;
    Color color = GUI.color;
    GUI.color = MenuConfig.HoverColorNow;
    float num = 46f;
    GUI.DrawTexture(new Rect(box.xMax - 4f, (float) ((double) box.y + (double) box.height * 0.5 - 23.0), 5f, num), (Texture) Texture2D.whiteTexture);
    GUI.color = color;
  }

  private void RenderCameraKeyFrames(float tenthSecondPxSpacing)
  {
    for (int index = 0; index < this.replayProject.CameraKeyFrames.Count; ++index)
    {
      CameraKeyFrame cameraKeyFrame = this.replayProject.CameraKeyFrames[index];
      float num1 = SpeedManager.RealTimeToSpeedTime(cameraKeyFrame.Time) * 100f * tenthSecondPxSpacing;
      Rect rect;
      // ISSUE: explicit constructor call
      rect = new Rect(num1 - 8f, 50f, 16f, 50f);
      if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
      {
        Action<CameraKeyFrame> keyFrameSelected = this.onCameraKeyFrameSelected;
        if (keyFrameSelected != null)
          keyFrameSelected(cameraKeyFrame);
      }
      float num2 = 14f;
      TimelineShapes.Square(new Rect(num1 - 7f, 68f, num2, num2), MenuConfig.HoverColorNow);
    }
  }

  private void RenderSpeedKeyFrames(float tenthSecondPxSpacing)
  {
    float num1 = 103f;
    foreach (SpeedKeyframe speedKeyframe in this.replayProject.SpeedKeyframes)
    {
      float num2 = SpeedManager.RealTimeToSpeedTime(speedKeyframe.Time) * 100f * tenthSecondPxSpacing;
      Rect rect;
      // ISSUE: explicit constructor call
      rect = new Rect(num2 - 8f, num1, 16f, 50f);
      if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
      {
        Action<SpeedKeyframe> keyframeSelected = this.onSpeedKeyframeSelected;
        if (keyframeSelected != null)
          keyframeSelected(speedKeyframe);
      }
      float num3 = 14f;
      TimelineShapes.Dot(new Rect(num2 - 7f, num1 + 18f, num3, num3), MenuConfig.ActiveColorNow);
    }
  }

  private void RenderTimeMarkers(float tenthSecondPxSpacing)
  {
    float secondSpacing = UIConstants.GetSecondSpacing(tenthSecondPxSpacing);
    this.RenderMajorTimeLabels(secondSpacing);
    this.RenderMinorTicks(secondSpacing);
  }

  private void RenderMajorTimeLabels(float secondSpacing)
  {
    float projectTimeWithSpeed = SpeedManager.GetTotalProjectTimeWithSpeed();
    float num1 = 10f;
    for (float num2 = 0.0f; (double) num2 <= (double) projectTimeWithSpeed; num2 += num1)
      GUI.Label(new Rect(num2 * secondSpacing, 5f, 50f, 20f), num2.ToString("F0"), MenuConfig.GetLabelStyle());
  }

  private void RenderMinorTicks(float secondSpacing)
  {
    float projectTimeWithSpeed = SpeedManager.GetTotalProjectTimeWithSpeed();
    float num1 = 1f;
    for (float num2 = 0.0f; (double) num2 <= (double) projectTimeWithSpeed; num2 += num1)
      GUI.Label(new Rect(num2 * secondSpacing, 25f, 10f, 15f), "|", MenuConfig.GetLabelStyle());
  }

  private void RenderGorillaTracks(float tenthSecondPxSpacing)
  {
    int numberOfGorillaGoneThrough = 0;
    float trackSpaceFromTop = 156f;
    for (int groupIndex = 0; groupIndex < this.gorillaController.GorillasByReplay.Count; ++groupIndex)
      this.RenderGorillaGroup(groupIndex, ref numberOfGorillaGoneThrough, trackSpaceFromTop, tenthSecondPxSpacing);
  }

  private void RenderGorillaGroup(
    int groupIndex,
    ref int numberOfGorillaGoneThrough,
    float trackSpaceFromTop,
    float tenthSecondPxSpacing)
  {
    float num1 = 0.0f;
    float num2 = (float) (53.0 * (double) numberOfGorillaGoneThrough + (double) trackSpaceFromTop + (double) groupIndex * 20.0 * 2.0);
    GUIStyle guiStyle = new GUIStyle();
    guiStyle.normal.background = this.uiManager.MakeTex(2, 2, UIConstants.GORILLA_GROUP_BG_COLOR);
    float num3 = (float) ((double) this.gorillaController.GorillasByReplay[groupIndex].Count * 53.0 + (double) (groupIndex + 1) * 20.0);
    GUI.Box(new Rect(num1, num2, SpeedManager.GetTotalProjectTimeWithSpeed() * 100f * tenthSecondPxSpacing, num3), "", guiStyle);
    int index = 0;
    while (index < this.gorillaController.GorillasByReplay[groupIndex].Count)
    {
      this.RenderGorillaTrack(this.gorillaController.GorillasByReplay[groupIndex][index], numberOfGorillaGoneThrough, groupIndex, trackSpaceFromTop, tenthSecondPxSpacing);
      ++index;
      ++numberOfGorillaGoneThrough;
    }
  }

  private void RenderGorillaTrack(
    Gorilla gorilla,
    int trackIndex,
    int groupIndex,
    float trackSpaceFromTop,
    float tenthSecondPxSpacing)
  {
    float num1 = SpeedManager.RealTimeToSpeedTime(gorilla.StartTime / 100f + gorilla.offsetStartTime) * 100f * tenthSecondPxSpacing;
    float num2 = SpeedManager.GetGorillaFinalTimeWithSpeed(gorilla) * 100f * tenthSecondPxSpacing;
    float trackYPos = (float) (53.0 * (double) trackIndex + (double) trackSpaceFromTop + (double) (groupIndex + 1) * 20.0);
    GUI.Box(new Rect(num1, trackYPos, num2, 50f), $"{gorilla.userName} - {gorilla.ID}", this.uiManager.MakeGuiForGorillaBar(gorilla.gorillaColor));
    this.RenderVoiceMarkers(gorilla, trackYPos, tenthSecondPxSpacing);
  }

  private void RenderVoiceMarkers(Gorilla gorilla, float trackYPos, float tenthSecondPxSpacing)
  {
    if (this.replayProject.VoiceKeyFrames == null)
      return;
    foreach (VoiceKeyFrame voiceKeyFrame in this.replayProject.VoiceKeyFrames)
    {
      if (voiceKeyFrame.ActorNumber == gorilla.actorNumber)
      {
        float num = SpeedManager.RealTimeToSpeedTime(voiceKeyFrame.Time) * 100f * tenthSecondPxSpacing;
        Rect rect;
        // ISSUE: explicit constructor call
        rect = new Rect(num - 4f, trackYPos, 9f, 50f);
        if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
        {
          Action<VoiceKeyFrame> keyFrameSelected = this.onVoiceKeyFrameSelected;
          if (keyFrameSelected != null)
            keyFrameSelected(voiceKeyFrame);
        }
        Color outlineColorNow = MenuConfig.OutlineColorNow;
        outlineColorNow.a = voiceKeyFrame.Muted ? 0.8f : 0.4f;
        TimelineShapes.Line(new Rect(num - 0.75f, trackYPos, 1.5f, 50f), outlineColorNow);
      }
    }
  }

  private void RenderCurrentTimeBar(
    float tenthSecondPxSpacing,
    float currentTime,
    float scrollBoxHeight)
  {
    GUIStyle guiStyle = new GUIStyle();
    guiStyle.normal.background = this.uiManager.MakeTex(2, 2, UIConstants.TIME_BAR_COLOR);
    Rect rect;
    // ISSUE: explicit constructor call
    rect = new Rect(tenthSecondPxSpacing * (SpeedManager.RealTimeToSpeedTime(currentTime) * 100f), 0.0f, 2f, scrollBoxHeight);
    GUI.Box(rect, "", guiStyle);
  }

  private void HandleTimelineInput(float tenthSecondPxSpacing, float total)
  {
    Event current = Event.current;
    if (current.type != 0)
      return;
    float num = tenthSecondPxSpacing * 100f;
    float realTime = SpeedManager.SpeedTimeToRealTime(Mathf.Clamp(current.mousePosition.x / num, 0.0f, total));
    if (current.button == 0)
    {
      Action<float> onTimeChanged = this.onTimeChanged;
      if (onTimeChanged != null)
        onTimeChanged(realTime);
    }
    else if (current.button == 1)
    {
      Action<IKeyFrame, float> keyFrameTimeMoved = this.onKeyFrameTimeMoved;
      if (keyFrameTimeMoved != null)
        keyFrameTimeMoved((IKeyFrame) null, realTime);
    }
    current.Use();
  }
}
