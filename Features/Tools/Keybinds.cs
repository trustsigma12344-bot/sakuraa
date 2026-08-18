using SakuraaCastingMod.Core;
using SakuraaCastingMod.Desktop.Camera;
using SakuraaCastingMod.Desktop.Ui;
using SakuraaCastingMod.Features.Overlays;
using SakuraaCastingMod.Shared.Helpers;
using SakuraaCastingMod.VR.Interaction;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.XR;

#nullable disable
namespace SakuraaCastingMod.Features.Tools;

public static class Keybinds
{
  public static Keyboard Keyboard;
  private static bool _prevGpStart;
  private static bool _prevGpSelect;
  private static bool _prevGpDPadUp;
  private static bool _prevGpDPadDown;
  private static bool _prevGpDPadLeft;
  private static bool _prevGpDPadRight;
  private static bool _prevGpLeftStickBtn;
  private static bool _prevGpRB;
  private static bool _prevGpLB;
  [SavedSetting("ShowKeybindMenu", false)]
  public static bool ShowKeybindMenu;
  [SavedSetting]
  public static Key ToggleMenuKey = (Key) 3;
  [SavedSetting]
  public static Key ZoomKey = (Key) 40;
  [SavedSetting]
  public static Key SwitchModeKey = (Key) 53;
  [SavedSetting]
  public static Key CreateNestKey = (Key) 28;
  [SavedSetting]
  public static Key ToggleDistanceCardKey = (Key) 16 /*0x10*/;
  [SavedSetting]
  public static Key ToggleLeaderboardKey = (Key) 26;
  [SavedSetting]
  public static Key ResetCameraKey = (Key) 71;
  [SavedSetting]
  public static Key ToggleMicKey = (Key) 34;
  [SavedSetting]
  public static Key SpecPartKey = (Key) 22;
  [SavedSetting]
  public static Key SpecSmoothKey = (Key) 24;
  [SavedSetting]
  public static Key RewindKey = (Key) 54;
  [SavedSetting]
  public static Key SbScoreDownKey = (Key) 13;
  [SavedSetting]
  public static Key SbScoreUpKey = (Key) 14;
  [SavedSetting]
  public static Key SbPauseKey = (Key) 1;
  [SavedSetting]
  public static Key SbResetTimerKey = (Key) 28;
  [SavedSetting]
  public static Key FcForwardKey = (Key) 37;
  [SavedSetting]
  public static Key FcBackKey = (Key) 33;
  [SavedSetting]
  public static Key FcLeftKey = (Key) 15;
  [SavedSetting]
  public static Key FcRightKey = (Key) 18;
  [SavedSetting]
  public static Key FcUpKey = (Key) 19;
  [SavedSetting]
  public static Key FcDownKey = (Key) 31 /*0x1F*/;
  [SavedSetting]
  public static Key FcPitchUpKey = (Key) 63 /*0x3F*/;
  [SavedSetting]
  public static Key FcPitchDownKey = (Key) 64 /*0x40*/;
  [SavedSetting]
  public static Key FcYawLeftKey = (Key) 61;
  [SavedSetting]
  public static Key FcYawRightKey = (Key) 62;
  [SavedSetting]
  public static Key FcRollLeftKey = (Key) 32 /*0x20*/;
  [SavedSetting]
  public static Key FcRollRightKey = (Key) 20;
  [SavedSetting]
  public static Key DrResetKey = (Key) 32 /*0x20*/;
  [SavedSetting]
  public static Key DrThrottleUpKey = (Key) 37;
  [SavedSetting]
  public static Key DrBoostKey = (Key) 19;
  [SavedSetting]
  public static Key DrThrottleDownKey = (Key) 33;
  [SavedSetting]
  public static Key DrDescendKey = (Key) 31 /*0x1F*/;
  [SavedSetting]
  public static Key DrYawLeftKey = (Key) 15;
  [SavedSetting]
  public static Key DrYawRightKey = (Key) 18;
  [SavedSetting]
  public static Key ObUpKey = (Key) 19;
  [SavedSetting]
  public static Key ObDownKey = (Key) 31 /*0x1F*/;
  [SavedSetting]
  public static Key CmForwardKey = (Key) 37;
  [SavedSetting]
  public static Key CmBackKey = (Key) 33;
  [SavedSetting]
  public static Key CmLeftKey = (Key) 15;
  [SavedSetting]
  public static Key CmRightKey = (Key) 18;
  [SavedSetting]
  public static Key CmJumpKey = (Key) 1;
  [SavedSetting]
  public static Key CmCrouchKey = (Key) 55;
  [SavedSetting]
  public static Key CmSprintKey = (Key) 51;
  [SavedSetting]
  public static Key CmToggleFlyKey = (Key) 1;
  [SavedSetting]
  public static Key CmAnimWalkKey = (Key) 41;
  [SavedSetting]
  public static Key CmAnimFlyKey = (Key) 42;
  [SavedSetting]
  public static Key CmAnimHandKey = (Key) 43;
  [SavedSetting]
  public static Key RpPauseKey = (Key) 1;
  [SavedSetting]
  public static Key RpScrubBackKey = (Key) 61;
  [SavedSetting]
  public static Key RpScrubForwardKey = (Key) 62;
  [SavedSetting]
  public static Key RpAddCameraKey = (Key) 17;
  [SavedSetting]
  public static Key RpAddSpeedKey = (Key) 36;
  [SavedSetting]
  public static Key RpToggleCameraKey = (Key) 38;
  [SavedSetting]
  public static Key RpSaveKey = (Key) 25;
  public static string CapturingId;
  public static readonly List<Keybinds.KeybindEntry> All = new List<Keybinds.KeybindEntry>();

  private static void Reg(string label, string page, Func<Key> get, Action<Key> set)
  {
    Keybinds.All.Add(new Keybinds.KeybindEntry(label, page, get, set));
  }

  public static void EnsureRegistry()
  {
    if (Keybinds.All.Count > 0)
      return;
    Keybinds.Reg("Toggle Menu", "General", (Func<Key>) (() => Keybinds.ToggleMenuKey), (Action<Key>) (k => Keybinds.ToggleMenuKey = k));
    Keybinds.Reg("Zoom", "General", (Func<Key>) (() => Keybinds.ZoomKey), (Action<Key>) (k => Keybinds.ZoomKey = k));
    Keybinds.Reg("Switch Mode", "General", (Func<Key>) (() => Keybinds.SwitchModeKey), (Action<Key>) (k => Keybinds.SwitchModeKey = k));
    Keybinds.Reg("Create Nest", "General", (Func<Key>) (() => Keybinds.CreateNestKey), (Action<Key>) (k => Keybinds.CreateNestKey = k));
    Keybinds.Reg("Toggle Distance", "General", (Func<Key>) (() => Keybinds.ToggleDistanceCardKey), (Action<Key>) (k => Keybinds.ToggleDistanceCardKey = k));
    Keybinds.Reg("Toggle Leaderboard", "General", (Func<Key>) (() => Keybinds.ToggleLeaderboardKey), (Action<Key>) (k => Keybinds.ToggleLeaderboardKey = k));
    Keybinds.Reg("Reset Camera", "General", (Func<Key>) (() => Keybinds.ResetCameraKey), (Action<Key>) (k => Keybinds.ResetCameraKey = k));
    Keybinds.Reg("Toggle Mic", "General", (Func<Key>) (() => Keybinds.ToggleMicKey), (Action<Key>) (k => Keybinds.ToggleMicKey = k));
    Keybinds.Reg("Spec Part", "General", (Func<Key>) (() => Keybinds.SpecPartKey), (Action<Key>) (k => Keybinds.SpecPartKey = k));
    Keybinds.Reg("Smooth Part", "General", (Func<Key>) (() => Keybinds.SpecSmoothKey), (Action<Key>) (k => Keybinds.SpecSmoothKey = k));
    Keybinds.Reg("Rewind", "General", (Func<Key>) (() => Keybinds.RewindKey), (Action<Key>) (k => Keybinds.RewindKey = k));
    Keybinds.Reg("Score Down", "Scoreboard", (Func<Key>) (() => Keybinds.SbScoreDownKey), (Action<Key>) (k => Keybinds.SbScoreDownKey = k));
    Keybinds.Reg("Score Up", "Scoreboard", (Func<Key>) (() => Keybinds.SbScoreUpKey), (Action<Key>) (k => Keybinds.SbScoreUpKey = k));
    Keybinds.Reg("Pause Timer", "Scoreboard", (Func<Key>) (() => Keybinds.SbPauseKey), (Action<Key>) (k => Keybinds.SbPauseKey = k));
    Keybinds.Reg("Reset Timer", "Scoreboard", (Func<Key>) (() => Keybinds.SbResetTimerKey), (Action<Key>) (k => Keybinds.SbResetTimerKey = k));
    Keybinds.Reg("Forward", "FreeCam", (Func<Key>) (() => Keybinds.FcForwardKey), (Action<Key>) (k => Keybinds.FcForwardKey = k));
    Keybinds.Reg("Back", "FreeCam", (Func<Key>) (() => Keybinds.FcBackKey), (Action<Key>) (k => Keybinds.FcBackKey = k));
    Keybinds.Reg("Left", "FreeCam", (Func<Key>) (() => Keybinds.FcLeftKey), (Action<Key>) (k => Keybinds.FcLeftKey = k));
    Keybinds.Reg("Right", "FreeCam", (Func<Key>) (() => Keybinds.FcRightKey), (Action<Key>) (k => Keybinds.FcRightKey = k));
    Keybinds.Reg("Up", "FreeCam", (Func<Key>) (() => Keybinds.FcUpKey), (Action<Key>) (k => Keybinds.FcUpKey = k));
    Keybinds.Reg("Down", "FreeCam", (Func<Key>) (() => Keybinds.FcDownKey), (Action<Key>) (k => Keybinds.FcDownKey = k));
    Keybinds.Reg("Pitch Up", "FreeCam", (Func<Key>) (() => Keybinds.FcPitchUpKey), (Action<Key>) (k => Keybinds.FcPitchUpKey = k));
    Keybinds.Reg("Pitch Down", "FreeCam", (Func<Key>) (() => Keybinds.FcPitchDownKey), (Action<Key>) (k => Keybinds.FcPitchDownKey = k));
    Keybinds.Reg("Yaw Left", "FreeCam", (Func<Key>) (() => Keybinds.FcYawLeftKey), (Action<Key>) (k => Keybinds.FcYawLeftKey = k));
    Keybinds.Reg("Yaw Right", "FreeCam", (Func<Key>) (() => Keybinds.FcYawRightKey), (Action<Key>) (k => Keybinds.FcYawRightKey = k));
    Keybinds.Reg("Roll Left", "FreeCam", (Func<Key>) (() => Keybinds.FcRollLeftKey), (Action<Key>) (k => Keybinds.FcRollLeftKey = k));
    Keybinds.Reg("Roll Right", "FreeCam", (Func<Key>) (() => Keybinds.FcRollRightKey), (Action<Key>) (k => Keybinds.FcRollRightKey = k));
    Keybinds.Reg("Reset Drone", "Drone", (Func<Key>) (() => Keybinds.DrResetKey), (Action<Key>) (k => Keybinds.DrResetKey = k));
    Keybinds.Reg("Throttle Up", "Drone", (Func<Key>) (() => Keybinds.DrThrottleUpKey), (Action<Key>) (k => Keybinds.DrThrottleUpKey = k));
    Keybinds.Reg("Boost", "Drone", (Func<Key>) (() => Keybinds.DrBoostKey), (Action<Key>) (k => Keybinds.DrBoostKey = k));
    Keybinds.Reg("Throttle Down", "Drone", (Func<Key>) (() => Keybinds.DrThrottleDownKey), (Action<Key>) (k => Keybinds.DrThrottleDownKey = k));
    Keybinds.Reg("Descend", "Drone", (Func<Key>) (() => Keybinds.DrDescendKey), (Action<Key>) (k => Keybinds.DrDescendKey = k));
    Keybinds.Reg("Yaw Left", "Drone", (Func<Key>) (() => Keybinds.DrYawLeftKey), (Action<Key>) (k => Keybinds.DrYawLeftKey = k));
    Keybinds.Reg("Yaw Right", "Drone", (Func<Key>) (() => Keybinds.DrYawRightKey), (Action<Key>) (k => Keybinds.DrYawRightKey = k));
    Keybinds.Reg("Move Up", "Observe", (Func<Key>) (() => Keybinds.ObUpKey), (Action<Key>) (k => Keybinds.ObUpKey = k));
    Keybinds.Reg("Move Down", "Observe", (Func<Key>) (() => Keybinds.ObDownKey), (Action<Key>) (k => Keybinds.ObDownKey = k));
    Keybinds.Reg("Forward", "Control Mode", (Func<Key>) (() => Keybinds.CmForwardKey), (Action<Key>) (k => Keybinds.CmForwardKey = k));
    Keybinds.Reg("Back", "Control Mode", (Func<Key>) (() => Keybinds.CmBackKey), (Action<Key>) (k => Keybinds.CmBackKey = k));
    Keybinds.Reg("Left", "Control Mode", (Func<Key>) (() => Keybinds.CmLeftKey), (Action<Key>) (k => Keybinds.CmLeftKey = k));
    Keybinds.Reg("Right", "Control Mode", (Func<Key>) (() => Keybinds.CmRightKey), (Action<Key>) (k => Keybinds.CmRightKey = k));
    Keybinds.Reg("Jump / Up", "Control Mode", (Func<Key>) (() => Keybinds.CmJumpKey), (Action<Key>) (k => Keybinds.CmJumpKey = k));
    Keybinds.Reg("Crouch / Down", "Control Mode", (Func<Key>) (() => Keybinds.CmCrouchKey), (Action<Key>) (k => Keybinds.CmCrouchKey = k));
    Keybinds.Reg("Sprint", "Control Mode", (Func<Key>) (() => Keybinds.CmSprintKey), (Action<Key>) (k => Keybinds.CmSprintKey = k));
    Keybinds.Reg("Toggle Walk/Fly", "Control Mode", (Func<Key>) (() => Keybinds.CmToggleFlyKey), (Action<Key>) (k => Keybinds.CmToggleFlyKey = k));
    Keybinds.Reg("Animator: Walk", "Control Mode", (Func<Key>) (() => Keybinds.CmAnimWalkKey), (Action<Key>) (k => Keybinds.CmAnimWalkKey = k));
    Keybinds.Reg("Animator: Fly", "Control Mode", (Func<Key>) (() => Keybinds.CmAnimFlyKey), (Action<Key>) (k => Keybinds.CmAnimFlyKey = k));
    Keybinds.Reg("Animator: Hand", "Control Mode", (Func<Key>) (() => Keybinds.CmAnimHandKey), (Action<Key>) (k => Keybinds.CmAnimHandKey = k));
    Keybinds.Reg("Pause / Play", "Replay", (Func<Key>) (() => Keybinds.RpPauseKey), (Action<Key>) (k => Keybinds.RpPauseKey = k));
    Keybinds.Reg("Scrub Back", "Replay", (Func<Key>) (() => Keybinds.RpScrubBackKey), (Action<Key>) (k => Keybinds.RpScrubBackKey = k));
    Keybinds.Reg("Scrub Forward", "Replay", (Func<Key>) (() => Keybinds.RpScrubForwardKey), (Action<Key>) (k => Keybinds.RpScrubForwardKey = k));
    Keybinds.Reg("Add Camera Keyframe", "Replay", (Func<Key>) (() => Keybinds.RpAddCameraKey), (Action<Key>) (k => Keybinds.RpAddCameraKey = k));
    Keybinds.Reg("Add Speed Keyframe", "Replay", (Func<Key>) (() => Keybinds.RpAddSpeedKey), (Action<Key>) (k => Keybinds.RpAddSpeedKey = k));
    Keybinds.Reg("Toggle Camera", "Replay", (Func<Key>) (() => Keybinds.RpToggleCameraKey), (Action<Key>) (k => Keybinds.RpToggleCameraKey = k));
    Keybinds.Reg("Save Replay", "Replay", (Func<Key>) (() => Keybinds.RpSaveKey), (Action<Key>) (k => Keybinds.RpSaveKey = k));
  }

  public static void ApplyCapture(Key key)
  {
    if (Keybinds.CapturingId == null)
      return;
    foreach (Keybinds.KeybindEntry keybindEntry in Keybinds.All)
    {
      if (keybindEntry.Id == Keybinds.CapturingId)
      {
        keybindEntry.Set(key);
        break;
      }
    }
    Keybinds.CapturingId = (string) null;
  }

  public static void Initialize()
  {
    Keybinds.Keyboard = Keyboard.current;
    Keybinds.EnsureRegistry();
  }

  public static void UpdateCheck()
  {
    if (Keybinds.Keyboard != null)
    {
      if (Keybinds.CapturingId != null)
      {
        if (Keybinds.ShowKeybindMenu)
          return;
        Keybinds.CapturingId = (string) null;
      }
      else
      {
        bool flag1 = false;
        bool flag2 = false;
        bool flag3 = false;
        bool flag4 = false;
        bool flag5 = false;
        bool flag6 = false;
        bool flag7 = false;
        bool flag8 = false;
        bool flag9 = false;
        if ((XRSettings.isDeviceActive ? 0 : (Gamepad.current != null ? 1 : 0)) != 0)
        {
          bool isPressed1;
          flag1 = (isPressed1 = Gamepad.current.startButton.isPressed) && !Keybinds._prevGpStart;
          Keybinds._prevGpStart = isPressed1;
          bool isPressed2;
          flag2 = (isPressed2 = Gamepad.current.selectButton.isPressed) && !Keybinds._prevGpSelect;
          Keybinds._prevGpSelect = isPressed2;
          bool isPressed3;
          flag3 = (isPressed3 = Gamepad.current.dpad.up.isPressed) && !Keybinds._prevGpDPadUp;
          Keybinds._prevGpDPadUp = isPressed3;
          bool isPressed4;
          flag4 = (isPressed4 = Gamepad.current.dpad.down.isPressed) && !Keybinds._prevGpDPadDown;
          Keybinds._prevGpDPadDown = isPressed4;
          bool isPressed5;
          flag5 = (isPressed5 = Gamepad.current.dpad.left.isPressed) && !Keybinds._prevGpDPadLeft;
          Keybinds._prevGpDPadLeft = isPressed5;
          bool isPressed6;
          flag6 = (isPressed6 = Gamepad.current.dpad.right.isPressed) && !Keybinds._prevGpDPadRight;
          Keybinds._prevGpDPadRight = isPressed6;
          bool isPressed7;
          flag7 = (isPressed7 = Gamepad.current.leftStickButton.isPressed) && !Keybinds._prevGpLeftStickBtn;
          Keybinds._prevGpLeftStickBtn = isPressed7;
          bool isPressed8;
          flag8 = (isPressed8 = Gamepad.current.rightShoulder.isPressed) && !Keybinds._prevGpRB;
          Keybinds._prevGpRB = isPressed8;
          bool isPressed9;
          flag9 = (isPressed9 = Gamepad.current.leftShoulder.isPressed) && !Keybinds._prevGpLB;
          Keybinds._prevGpLB = isPressed9;
        }
        bool flag10 = false;
        bool flag11 = false;
        bool flag12 = false;
        bool flag13 = false;
        bool flag14 = false;
        if ((XRSettings.isDeviceActive ? 0 : (((UnityEngine.Object) InputManager.Ins != (UnityEngine.Object) null) ? 1 : 0)) != 0)
        {
          flag10 = InputManager.Ins.leftSecondaryBtnDouble;
          flag11 = InputManager.Ins.rightSecondaryBtnDouble;
          flag12 = InputManager.Ins.leftPrimaryBtnSingle;
          flag13 = InputManager.Ins.rightSecondaryBtnSingle;
          flag14 = InputManager.Ins.leftSecondaryBtnSingle;
        }
        if (((ButtonControl) Keybinds.Keyboard[Keybinds.ToggleMenuKey]).wasPressedThisFrame | flag1 | flag10 && Plugin.Ins.XPosition != 0)
        {
          if (LayoutEditor.IsEditing)
            LayoutEditor.Toggle();
          MainMenus.ToggleMenuKeyPressed();
        }
        if (((ButtonControl) Keybinds.Keyboard[Keybinds.ToggleMicKey]).wasPressedThisFrame | flag7 | flag12)
        {
          Plugin.Ins.isPttTypeUnmuted = !Plugin.Ins.isPttTypeUnmuted;
          if ((!((UnityEngine.Object) GorillaTagger.Instance != (UnityEngine.Object) null) ? 0 : (((UnityEngine.Object) GorillaTagger.Instance.myRecorder != (UnityEngine.Object) null) ? 1 : 0)) != 0)
          {
            if (Plugin.Ins.isPttTypeUnmuted)
              GorillaTagger.Instance.myRecorder.StartRecording();
            else
              GorillaTagger.Instance.myRecorder.StopRecording();
          }
        }
        if (((ButtonControl) Keybinds.Keyboard[Keybinds.SpecPartKey]).wasPressedThisFrame | flag3)
          PlayerSpec.SwitchSpecPart();
        if (((ButtonControl) Keybinds.Keyboard[Keybinds.SpecSmoothKey]).wasPressedThisFrame | flag4)
        {
          PlayerSpec.SmoothMode = !PlayerSpec.SmoothMode;
          AutoPilot.AutoPilotEnabled = false;
          PlayerSpec.SpecPlayer(0);
        }
        if (((ButtonControl) Keybinds.Keyboard[Keybinds.SwitchModeKey]).wasPressedThisFrame | flag2 | flag11 && Plugin.Ins.XPosition != 0 && (!((UnityEngine.Object) Plugin.Ins != (UnityEngine.Object) null) || Plugin.Ins.CameraModes == null ? 0 : (Plugin.Ins.CameraModes.Length != 0 ? 1 : 0)) != 0)
        {
          Plugin.Ins.currentCameraMode = (Plugin.Ins.currentCameraMode + 1) % Plugin.Ins.CameraModes.Length;
          Plugin.Ins.OnModeChange();
        }
        else
        {
          if (((ButtonControl) Keybinds.Keyboard[Keybinds.ResetCameraKey]).wasPressedThisFrame | flag5)
          {
            PlayerSpec.TrackPosForward = 1.75f;
            PlayerSpec.TrackPosRight = 0.0f;
            PlayerSpec.TrackPosUp = 0.0f;
          }
          if (((ButtonControl) Keybinds.Keyboard[Keybinds.ToggleDistanceCardKey]).wasPressedThisFrame)
            LavaDistance.ToggleDistanceCard();
          if (((ButtonControl) Keybinds.Keyboard[Keybinds.ToggleLeaderboardKey]).wasPressedThisFrame)
            Leaderboard.ToggleLeaderboard();
          if ((!((ButtonControl) Keybinds.Keyboard[Keybinds.RewindKey]).wasPressedThisFrame ? 0 : (RewindViewer.ShowEditorMenu ? 1 : 0)) != 0)
            RewindViewer.Toggle();
          if ((Plugin.Ins.currentCameraMode == 2 || Plugin.Ins.currentCameraMode == 1 ? (((ButtonControl) Keybinds.Keyboard[Keybinds.CreateNestKey]).wasPressedThisFrame | flag6 ? 1 : 0) : 0) != 0)
            Observation.CreateNest();
          if ((Plugin.Ins.currentCameraMode == 5 ? 1 : (Plugin.Ins.currentCameraMode == 6 ? 1 : 0)) != 0 && !AutoPilot.AutoPilotEnabled)
          {
            int count = GorillaDataHandler.GorillaDataList.Count;
            if (count > 0)
            {
              if (flag8 | flag13)
                PlayerSpec.SpecPlayer((PlayerSpec.SpecGorilla + 1) % count);
              if (flag9 | flag14)
                PlayerSpec.SpecPlayer((PlayerSpec.SpecGorilla - 1 + count) % count);
            }
          }
          if ((!MainMenus.ShowMainMenu ? 0 : (ToggleMicOverlay.ShowToggleMicIcon ? 1 : 0)) == 0)
          {
            if ((!((UnityEngine.Object) Plugin.Ins != (UnityEngine.Object) null) ? 0 : (((UnityEngine.Object) Plugin.Ins.toggleMicObj != (UnityEngine.Object) null) ? 1 : 0)) == 0)
              return;
            Plugin.Ins.toggleMicObj.SetActive(false);
          }
          else
          {
            if ((!((UnityEngine.Object) Plugin.Ins != (UnityEngine.Object) null) ? 0 : (((UnityEngine.Object) Plugin.Ins.toggleMicObj != (UnityEngine.Object) null) ? 1 : 0)) == 0)
              return;
            if (((UnityEngine.Object) Plugin.Ins.toggleMicNormalObj != (UnityEngine.Object) null))
              Plugin.Ins.toggleMicNormalObj.SetActive(Plugin.Ins.isPttTypeUnmuted);
            if (((UnityEngine.Object) Plugin.Ins.toggleMicMutedObj != (UnityEngine.Object) null))
              Plugin.Ins.toggleMicMutedObj.SetActive(!Plugin.Ins.isPttTypeUnmuted);
            Plugin.Ins.toggleMicObj.SetActive(true);
          }
        }
      }
    }
    else
      UnityEngine.Debug.Log((object) "Keyboard not initialized in Keybinds.UpdateCheck");
  }

  public class KeybindEntry
  {
    public readonly string Label;
    public readonly string Page;
    public readonly Func<Key> Get;
    public readonly Action<Key> Set;

    public string Id => $"{this.Page}/{this.Label}";

    public KeybindEntry(string label, string page, Func<Key> get, Action<Key> set)
    {
      this.Label = label;
      this.Page = page;
      this.Get = get;
      this.Set = set;
    }
  }
}
