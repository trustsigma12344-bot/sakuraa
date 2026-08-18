using SakuraaCastingMod.Desktop.Ui.Framework;
using SakuraaCastingMod.Desktop.Ui.Framework.MenuItems;
using SakuraaCastingMod.Features.Replay.CustomCamera;
using SakuraaCastingMod.Features.Replay.EditJson;
using SakuraaCastingMod.Features.Replay.ReplayManagers;
using SakuraaCastingMod.Features.Tools;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

#nullable disable
namespace SakuraaCastingMod.Features.Replay.UI;

internal class ContentUI : UiManager
{
  private IKeyFrame currentSelectedKeyFrame;
  private ContentUI.KeyFrameType currentKeyFrameType = ContentUI.KeyFrameType.None;
  private CameraVisualManager cameraVisualManager;
  private TimelineRenderer timelineRenderer;
  private SliderMenuItem _zoomSlider;

  public ContentUI(GorillaController gorillaController)
    : base(gorillaController)
  {
    ReplayManager.OnReplayProjectChanged += new Action<ReplayProject>(this.OnReplayProjectChanged);
    ReplayBrowserMenu.Init(gorillaController);
    ReplayVoiceMenu.Init(gorillaController);
    ReplayToolsMenu.Init(gorillaController);
    KeyframeEditMenu.Init(new Action(this.ClearSelection));
    ReplayHistory.OnRestored = new Action(this.OnHistoryRestored);
    this.OnReplayProjectChanged(this.replayProject);
  }

  private void OnHistoryRestored()
  {
    this.ClearSelection();
    KeyframeEditMenu.Hide();
    this.cameraVisualManager?.ClearAllVisuals();
    if (this.replayProject == null)
      return;
    this.cameraVisualManager = new CameraVisualManager(this.replayProject);
  }

  private void OnReplayProjectChanged(ReplayProject newProject)
  {
    this.cameraVisualManager?.ClearAllVisuals();
    if (newProject == null)
    {
      this.cameraVisualManager = (CameraVisualManager) null;
      this.timelineRenderer = (TimelineRenderer) null;
    }
    else
    {
      this.cameraVisualManager = new CameraVisualManager(newProject);
      this.timelineRenderer = new TimelineRenderer(this.gorillaController, newProject, (UiManager) this, new Action<CameraKeyFrame>(this.OnCameraKeyFrameSelected), new Action<SpeedKeyframe>(this.OnSpeedKeyframeSelected), new Action<float>(this.OnTimeChanged), new Action<IKeyFrame, float>(this.OnKeyFrameTimeMoved), new Action<VoiceKeyFrame>(this.OnVoiceKeyFrameSelected));
    }
    ReplayToolsMenu.ShowMenu = newProject != null;
    ReplayVoiceMenu.ShowMenu = newProject != null;
    ReplayBrowserMenu.MarkDirty();
    ReplayVoiceMenu.MarkDirty();
    KeyframeEditMenu.Hide();
    ReplayHistory.Clear();
    this.ClearSelection();
  }

  private void ClearSelection()
  {
    this.currentSelectedKeyFrame = (IKeyFrame) null;
    this.currentKeyFrameType = ContentUI.KeyFrameType.None;
  }

  private void OnCameraKeyFrameSelected(CameraKeyFrame cameraFrame)
  {
    this.currentSelectedKeyFrame = (IKeyFrame) cameraFrame;
    this.currentKeyFrameType = ContentUI.KeyFrameType.Camera;
    KeyframeEditMenu.Show((IKeyFrame) cameraFrame, ContentUI.KeyFrameType.Camera);
  }

  private void OnSpeedKeyframeSelected(SpeedKeyframe speedFrame)
  {
    this.currentSelectedKeyFrame = (IKeyFrame) speedFrame;
    this.currentKeyFrameType = ContentUI.KeyFrameType.Speed;
    KeyframeEditMenu.Show((IKeyFrame) speedFrame, ContentUI.KeyFrameType.Speed);
  }

  private void OnVoiceKeyFrameSelected(VoiceKeyFrame voiceFrame)
  {
    this.currentSelectedKeyFrame = (IKeyFrame) voiceFrame;
    this.currentKeyFrameType = ContentUI.KeyFrameType.Voice;
    KeyframeEditMenu.Show((IKeyFrame) voiceFrame, ContentUI.KeyFrameType.Voice);
  }

  private void OnTimeChanged(float newTime) => this.currentTime = newTime;

  private void OnKeyFrameTimeMoved(IKeyFrame keyFrame, float newTime)
  {
    if (this.currentSelectedKeyFrame == null)
      return;
    this.currentSelectedKeyFrame.Time = newTime;
  }

  public override void OnGUI()
  {
    // ISSUE: explicit non-virtual call
    base.OnGUI();
    if (!this.UiON || (this.replayProject == null ? 0 : (this.timelineRenderer != null ? 1 : 0)) == 0)
      return;
    this.HandleZoomScroll();
    this.RenderZoomSlider();
    this.TenthSecondPxSpacing = this.timelineRenderer.Render(this.TenthSecondPxSpacing, this.currentTime);
  }

  private void HandleZoomScroll()
  {
    Event current = Event.current;
    if ((current.type != (EventType) 6 ? 1 : (!current.shift ? 1 : 0)) != 0 || (double) current.mousePosition.y < (double) Screen.height * 0.40000000596046448)
      return;
    this.TenthSecondPxSpacing = Mathf.Clamp(this.TenthSecondPxSpacing * ((double) current.delta.y > 0.0 ? 0.9f : 1.1f), 0.02f, 5f);
    current.Use();
  }

  private void RenderZoomSlider()
  {
    if (this._zoomSlider == null)
    {
      SliderMenuItem sliderMenuItem = new SliderMenuItem();
      sliderMenuItem.Label = "Zoom";
      sliderMenuItem.MinValue = 0.02f;
      sliderMenuItem.MaxValue = 5f;
      sliderMenuItem.Decimals = 2;
      sliderMenuItem.Enabled = true;
      sliderMenuItem.Description = "Zoom the timeline in/out";
      this._zoomSlider = sliderMenuItem;
      this._zoomSlider.OnValueChanged = (Action<float>) (v => this.TenthSecondPxSpacing = v);
    }
    this._zoomSlider.Value = this.TenthSecondPxSpacing;
    float height = this._zoomSlider.Height;
    ((MenuItem) this._zoomSlider).Draw(new Rect(this.VW * 1.5f, (float) ((double) Screen.height - (double) this.VH * 30.0 - (double) height - (double) this.VH * 1.0), this.VW * 26f, height));
  }

  internal override void BaseUI() => this.BottomButtonRow.RenderButtons();

  public override void Update()
  {
    // ISSUE: explicit non-virtual call
    base.Update();
    this.cameraVisualManager?.Update(this.UiON);
    if ((!this.UiON ? 0 : (this.replayProject != null ? 1 : 0)) == 0)
      return;
    this.HandlePlaybackKeys();
  }

  private void HandlePlaybackKeys()
  {
    if (GUIUtility.keyboardControl != 0)
      return;
    Keyboard current = Keyboard.current;
    if (current == null)
      return;
    bool flag;
    if ((!(flag = ((ButtonControl) current.leftCtrlKey).isPressed || ((ButtonControl) current.rightCtrlKey).isPressed) ? 0 : (((ButtonControl) current.zKey).wasPressedThisFrame ? 1 : 0)) != 0)
    {
      if ((((ButtonControl) current.leftShiftKey).isPressed ? 1 : (((ButtonControl) current.rightShiftKey).isPressed ? 1 : 0)) != 0)
        ReplayHistory.Redo();
      else
        ReplayHistory.Undo();
    }
    else if ((!flag ? 0 : (((ButtonControl) current.yKey).wasPressedThisFrame ? 1 : 0)) != 0)
      ReplayHistory.Redo();
    if (((ButtonControl) current[Keybinds.RpPauseKey]).wasPressedThisFrame)
      GorillaController.Paused = !GorillaController.Paused;
    if ((!((ButtonControl) current[Keybinds.RpAddCameraKey]).wasPressedThisFrame ? 0 : (CameraController.instance != null ? 1 : 0)) != 0)
      CameraController.instance.AddCamera();
    if (((ButtonControl) current[Keybinds.RpAddSpeedKey]).wasPressedThisFrame)
      SpeedManager.AddSpeedFrame(this.gorillaController.GetCurrentRealTime());
    if (((ButtonControl) current[Keybinds.RpToggleCameraKey]).wasPressedThisFrame)
      CameraController.UseKeyCustomCamera = !CameraController.UseKeyCustomCamera;
    if (((ButtonControl) current[Keybinds.RpSaveKey]).wasPressedThisFrame)
      ReplayManager.SaveProject();
    float num1 = 0.0f;
    if (((ButtonControl) current[Keybinds.RpScrubBackKey]).isPressed)
      --num1;
    if (((ButtonControl) current[Keybinds.RpScrubForwardKey]).isPressed)
      ++num1;
    if ((double) num1 == 0.0)
      return;
    float num2 = ((ButtonControl) current.leftShiftKey).isPressed ? 8f : 2.5f;
    float greatestEndTime = ReplayManager.FindGreatestEndTime();
    this.currentTime = Mathf.Clamp(this.gorillaController.GetCurrentRealTime() + num1 * num2 * Time.unscaledDeltaTime, 0.0f, greatestEndTime);
  }

  public enum KeyFrameType
  {
    None,
    Speed,
    Camera,
    Tag,
    Cosmetics,
    Voice,
  }
}
