using SakuraaCastingMod.Core;
using SakuraaCastingMod.Desktop.Camera;
using SakuraaCastingMod.Features.Overlays;
using SakuraaCastingMod.Shared.Helpers;
using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Desktop.Ui.Framework.Menus;

public static class PlayerSpecMenu
{
  public static MenuBuilder _playerSpecMenu;

  public static bool IsInitialized => PlayerSpecMenu._playerSpecMenu != null;

  public static void Draw()
  {
    if (PlayerSpecMenu._playerSpecMenu == null)
      PlayerSpecMenu.Initialize();
    PlayerSpecMenu._playerSpecMenu.Draw();
  }

  public static void Initialize()
  {
    PlayerSpecMenu._playerSpecMenu = new MenuBuilder("Player Spec Options", 250f).AddSpace(25f).AddSlider("FOV:", Plugin.fov, 30f, 160f, (Action<float>) (value => Plugin.fov = value), 1, "Adjust field of view").AddDynamicToggle("Speed FOV", (Func<bool>) (() => PlayerSpec.SpeedFov), (Action<bool>) (v => PlayerSpec.SpeedFov = v), "Smoothly widen the FOV as the spectated player moves faster").AddSlider("Speed Boost:", PlayerSpec.SpeedFovBoost, 0.0f, 40f, (Action<float>) (v => PlayerSpec.SpeedFovBoost = v), description: "Extra FOV degrees added at full speed", visibilityCondition: (Func<bool>) (() => PlayerSpec.SpeedFov)).AddSlider("Full At:", PlayerSpec.SpeedFovMaxSpeed, 2f, 15f, (Action<float>) (v => PlayerSpec.SpeedFovMaxSpeed = v), 1, "Speed (m/s) at which the full FOV boost is reached", (Func<bool>) (() => PlayerSpec.SpeedFov)).AddSlider("Clipping Plane:", Plugin.clippingPlaneNear, 0.01f, 1f, (Action<float>) (value => Plugin.clippingPlaneNear = value), 2, "Adjust near clipping plane distance").AddDynamicToggle("Move Audio", (Func<bool>) (() => Plugin.listenerBool), (Action<bool>) (v => Plugin.listenerBool = v), "Move audio listener with camera").AddSlider("Move Speed:", Plugin.moveSpeed, 0.1f, 30f, (Action<float>) (value => Plugin.moveSpeed = value), 1, "Adjust camera movement speed").AddDynamicToggle("Orbit Mode", (Func<bool>) (() => PlayerSpec.OrbitMode), (Action<bool>) (v => PlayerSpec.OrbitMode = v), "Enable orbital camera path").AddDynamicToggle("Smart Orbit", (Func<bool>) (() => PlayerSpec.SmartOrbit), (Action<bool>) (v => PlayerSpec.SmartOrbit = v), "Auto-track nearest interaction (Blocks Input)", (Func<bool>) (() => PlayerSpec.OrbitMode)).AddDynamicToggle("Mouse Orbit", (Func<bool>) (() => PlayerSpec.MouseOrbit), (Action<bool>) (v => PlayerSpec.MouseOrbit = v), "Use mouse to control orbit", (Func<bool>) (() => PlayerSpec.OrbitMode && !PlayerSpec.SmartOrbit)).AddSpace().AddSlider("Orbit Smooth:", PlayerSpec.OrbitSmoothing, 1f, 30f, (Action<float>) (v => PlayerSpec.OrbitSmoothing = v), 1, "Smoothing of the orbital path", (Func<bool>) (() => PlayerSpec.OrbitMode)).AddSlider("Kb Speed:", PlayerSpec.OrbitKbSpeed, 10f, 200f, (Action<float>) (v => PlayerSpec.OrbitKbSpeed = v), 1, "Speed of manual orbit movement", (Func<bool>) (() => PlayerSpec.OrbitMode && !PlayerSpec.SmartOrbit)).AddSlider("Smart Sens:", PlayerSpec.SmartOrbitSensitivity, 0.1f, 10f, (Action<float>) (v => PlayerSpec.SmartOrbitSensitivity = v), 1, "How fast smart orbit locks targets", (Func<bool>) (() => PlayerSpec.SmartOrbit)).AddSlider("Smart Offset:", PlayerSpec.SmartOrbitOffsetAngle, 0.0f, 60f, (Action<float>) (v => PlayerSpec.SmartOrbitOffsetAngle = v), description: "Angle the camera sits off the direct line so the chaser behind your target stays visible", visibilityCondition: (Func<bool>) (() => PlayerSpec.SmartOrbit)).AddSpace().AddDynamicToggle("AutoPilot", (Func<bool>) (() => AutoPilot.AutoPilotEnabled), (Action<bool>) (v => AutoPilot.AutoPilotEnabled = v), "Toggle automated player switching").AddDynamicButton((Func<string>) (() => "Target: " + (AutoPilot.PickTaggers ? "Taggers" : "Runners")), (Action) (() => AutoPilot.PickTaggers = !AutoPilot.PickTaggers), "Toggle between targeting taggers or runners", (Func<bool>) (() => AutoPilot.AutoPilotEnabled)).AddSlider("Cooldown:", AutoPilot.APCooldownInterval, 0.0f, 15f, (Action<float>) (value => AutoPilot.APCooldownInterval = value), 1, "Adjust autopilot cooldown interval", (Func<bool>) (() => AutoPilot.AutoPilotEnabled)).AddDynamicToggle("Smooth", (Func<bool>) (() => PlayerSpec.SmoothMode), (Action<bool>) (v =>
    {
      PlayerSpec.SmoothMode = v;
      AutoPilot.AutoPilotEnabled = false;
      PlayerSpec.SpecPlayer(PlayerSpec.SpecGorilla);
    }), "Camera smoothing (Head/Body lerp; Eyes = tablet-style first-person)").AddSlider("Pos Smooth:", PlayerSpec.CameraLerp, 0.3f, 0.02f, (Action<float>) (v => PlayerSpec.CameraLerp = v), 2, "Position interpolation", (Func<bool>) (() => PlayerSpec.SmoothMode && PlayerSpec.SpecPart != "eyes")).AddSlider("Rot Smooth:", PlayerSpec.QuatLerp, 0.3f, 0.02f, (Action<float>) (v => PlayerSpec.QuatLerp = v), 2, "Rotation interpolation", (Func<bool>) (() => PlayerSpec.SmoothMode && PlayerSpec.SpecPart != "eyes")).AddDropdown("Tracking", (Func<IList<string>>) (() => (IList<string>) new List<string>()
    {
      "Head",
      "Body",
      "Eyes"
    }), (Func<int>) (() =>
    {
      int num;
      switch (PlayerSpec.SpecPart)
      {
        case "body":
          num = 1;
          break;
        case "eyes":
          num = 2;
          break;
        default:
          num = 0;
          break;
      }
      return num;
    }), (Action<int>) (idx =>
    {
      string str;
      switch (idx)
      {
        case 1:
          str = "body";
          break;
        case 2:
          str = "eyes";
          break;
        default:
          str = "head";
          break;
      }
      PlayerSpec.SpecPart = str;
    }), "Change tracking focus (Head / Body / Eyes)").AddSlider("Smooth Amt:", PlayerSpec.FpvSmoothness, 40f, 3f, (Action<float>) (v => PlayerSpec.FpvSmoothness = v), 1, "First-person smoothing (right = smoother / floatier)", (Func<bool>) (() => PlayerSpec.SpecPart == "eyes" && PlayerSpec.SmoothMode)).AddDynamicToggle("FPV Clamp", (Func<bool>) (() => PlayerSpec.FpvClamping), (Action<bool>) (v => PlayerSpec.FpvClamping = v), "Stop the view lagging more than the clamp angle behind fast head turns", (Func<bool>) (() => PlayerSpec.SpecPart == "eyes" && PlayerSpec.SmoothMode)).AddSlider("Clamp Angle:", PlayerSpec.FpvClampAngle, 5f, 90f, (Action<float>) (v => PlayerSpec.FpvClampAngle = v), description: "Max degrees the smoothed view may trail the head", visibilityCondition: (Func<bool>) (() => PlayerSpec.SpecPart == "eyes" && PlayerSpec.SmoothMode && PlayerSpec.FpvClamping)).AddDynamicToggle("Roll Lock", (Func<bool>) (() => PlayerSpec.FpvRollLock), (Action<bool>) (v => PlayerSpec.FpvRollLock = v), "Keep the horizon level (ignore the head's roll/tilt)", (Func<bool>) (() => PlayerSpec.SpecPart == "eyes" && PlayerSpec.SmoothMode)).AddSlider("FPV Offset:", PlayerSpec.FpvOffset, -0.5f, 0.5f, (Action<float>) (v => PlayerSpec.FpvOffset = v), 2, "Move the first-person camera fore/aft from the eyes", (Func<bool>) (() => PlayerSpec.SpecPart == "eyes")).AddDynamicToggle("Hide Their Cosmetics", (Func<bool>) (() => PlayerSpec.HideSpectatedCosmetics), (Action<bool>) (v => PlayerSpec.HideSpectatedCosmetics = v), "Hide the spectated player's own hat/face/etc. so they don't block the first-person view (only you see this)", (Func<bool>) (() => PlayerSpec.SpecPart == "eyes")).AddDynamicToggle("Pin player", (Func<bool>) (() => PlayerSpec.PinnedPlayerEnabled), (Action<bool>) (v =>
    {
      if (v)
      {
        if ((GorillaDataHandler.GorillaDataList.Count <= PlayerSpec.SpecGorilla ? 0 : (GorillaDataHandler.GorillaDataList[PlayerSpec.SpecGorilla] != null ? 1 : 0)) == 0)
        {
          Notification.Send("Could not find current player to pin", Color.red);
        }
        else
        {
          PlayerSpec.PinnedPlayerId = GorillaDataHandler.GorillaDataList[PlayerSpec.SpecGorilla].UserId;
          PlayerSpec.PinnedPlayerEnabled = true;
          int plr = -1;
          for (int index = 0; index < GorillaDataHandler.GorillaDataList.Count; ++index)
          {
            if (GorillaDataHandler.GorillaDataList[index]?.UserId == PlayerSpec.PinnedPlayerId)
            {
              plr = index;
              break;
            }
          }
          if (plr != -1)
          {
            PlayerSpec.SpecPlayer(plr);
          }
          else
          {
            Notification.Send("Pinned player left the room", Color.yellow);
            PlayerSpec.SpecPlayer(0);
          }
        }
      }
      else
      {
        PlayerSpec.PinnedPlayerEnabled = false;
        PlayerSpec.PinnedPlayerId = "";
      }
    }), "Pin camera to the currently spectated player");
  }
}
