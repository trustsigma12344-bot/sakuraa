using SakuraaCastingMod.Core;
using SakuraaCastingMod.Desktop.Ui.Framework.MenuItems;
using SakuraaCastingMod.Features.Overlays;
using SakuraaCastingMod.Shared.Helpers;
using SakuraaCastingMod.Shared.Models;
using SakuraaCastingMod.VR.Interaction;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.XR;

#nullable disable
namespace SakuraaCastingMod.Desktop.Camera;

public static class PlayerSpec
{
  private static Vector3 _cameraPosition;
  private static Quaternion _cR3;
  private static Vector3 _finalPos;
  private static Quaternion _finalcR3;
  [SavedSetting("_cameraLerp", 0.12f)]
  public static float CameraLerp = 0.12f;
  [SavedSetting("_quatLerp", 0.055f)]
  public static float QuatLerp = 0.055f;
  private static float _transformCooldown;
  [SavedSetting("_specPart", "head")]
  public static string SpecPart = "head";
  [SavedSetting("_smoothMode", true)]
  public static bool SmoothMode = true;
  [SavedSetting("FpvSmoothness", 25f)]
  public static float FpvSmoothness = 25f;
  [SavedSetting("FpvClamping", true)]
  public static bool FpvClamping = true;
  [SavedSetting("FpvClampAngle", 30f)]
  public static float FpvClampAngle = 30f;
  [SavedSetting("FpvRollLock", false)]
  public static bool FpvRollLock = false;
  [SavedSetting("FpvOffset", 0.0f)]
  public static float FpvOffset = 0.0f;
  [SavedSetting("SpeedFov", false)]
  public static bool SpeedFov = false;
  [SavedSetting("SpeedFovBoost", 15f)]
  public static float SpeedFovBoost = 15f;
  [SavedSetting("SpeedFovMaxSpeed", 9f)]
  public static float SpeedFovMaxSpeed = 9f;
  private const float TeleportSpeedCutoff = 40f;
  private static float _speedFovMeasuredSpeed;
  private static float _speedFovCurrentBoost;
  private static Vector3 _speedFovLastHeadPos;
  private static string _speedFovUserId;
  [SavedSetting("HideSpectatedCosmetics", false)]
  public static bool HideSpectatedCosmetics = false;
  [SavedSetting("_trackPosForward", 1.75f)]
  public static float TrackPosForward = 1.75f;
  [SavedSetting("_trackPosRight", 0.0f)]
  public static float TrackPosRight;
  [SavedSetting("_trackPosUp", 0.0f)]
  public static float TrackPosUp;
  public static int SpecGorilla;
  [SavedSetting("PinnedPlayerEnabled", false)]
  public static bool PinnedPlayerEnabled = false;
  [SavedSetting("PinnedPlayerId", "")]
  public static string PinnedPlayerId = "";
  [SavedSetting("OrbitMode", false)]
  public static bool OrbitMode = false;
  [SavedSetting("MouseOrbit", false)]
  public static bool MouseOrbit = false;
  [SavedSetting("SmartOrbit", false)]
  public static bool SmartOrbit = false;
  public static float OrbitX = 0.0f;
  public static float OrbitY = 0.0f;
  private static float _smoothOrbitX = 0.0f;
  private static float _smoothOrbitY = 0.0f;
  [SavedSetting("OrbitSmoothing", 15f)]
  public static float OrbitSmoothing = 15f;
  [SavedSetting("OrbitKbSpeed", 75f)]
  public static float OrbitKbSpeed = 75f;
  private const float StickDeadzone = 0.15f;
  [SavedSetting("SmartOrbitMinDist", 1.5f)]
  public static float SmartOrbitMinDist = 1.5f;
  [SavedSetting("SmartOrbitSensitivity", 2.5f)]
  public static float SmartOrbitSensitivity = 2.5f;
  [SavedSetting("SmartOrbitOffsetAngle", 25f)]
  public static float SmartOrbitOffsetAngle = 25f;

  public static void UpdatePlayerSpec()
  {
    if (Networking.InRoom)
    {
      UnityEngine.Camera camera = Plugin.Ins.camera;
      PlayerSpec.HandlePinningLogic();
      if (PlayerSpec.OrbitMode)
      {
        if (PlayerSpec.SmartOrbit)
          PlayerSpec.HandleSmartOrbitLogic();
        else
          PlayerSpec.HandleOrbitInput();
        PlayerSpec._smoothOrbitX = Mathf.Lerp(PlayerSpec._smoothOrbitX, PlayerSpec.OrbitX, PlayerSpec.OrbitSmoothing * Time.deltaTime);
        PlayerSpec._smoothOrbitY = Mathf.Lerp(PlayerSpec._smoothOrbitY, PlayerSpec.OrbitY, PlayerSpec.OrbitSmoothing * Time.deltaTime);
      }
      else
        PlayerSpec.HandleStandardInput();
      PlayerSpec.HandleAutoPilotSwitching();
      if (GorillaDataHandler.GorillaDataList.Count == 0)
      {
        SpectatedCosmeticHider.Restore();
      }
      else
      {
        PlayerSpec.SpecGorilla = Mathf.Clamp(PlayerSpec.SpecGorilla, 0, GorillaDataHandler.GorillaDataList.Count - 1);
        GorillaData gorillaData = GorillaDataHandler.GorillaDataList[PlayerSpec.SpecGorilla];
        if (gorillaData == null)
        {
          SpectatedCosmeticHider.Restore();
        }
        else
        {
          PlayerSpec.ApplyFinalTransform(camera, gorillaData);
          if ((!(PlayerSpec.SpecPart == "eyes") ? 0 : (PlayerSpec.HideSpectatedCosmetics ? 1 : 0)) == 0)
            SpectatedCosmeticHider.Restore();
          else
            SpectatedCosmeticHider.HideFor(gorillaData.Rig);
          camera.nearClipPlane = Plugin.clippingPlaneNear;
          camera.fieldOfView = Mathf.Min(Plugin.fov + PlayerSpec.UpdateSpeedFov(gorillaData), 179f);
        }
      }
    }
    else
    {
      Plugin.Ins.currentCameraMode = (Plugin.Ins.currentCameraMode + 1) % Plugin.Ins.CameraModes.Length;
      Notification.Send("You need to be in a lobby to unlock more modes.", Color.red);
      Plugin.Ins.OnModeChange();
    }
  }

  private static float UpdateSpeedFov(GorillaData gorilla)
  {
    float deltaTime = Time.deltaTime;
    if ((!PlayerSpec.SpeedFov ? 0 : ((double) deltaTime > 0.0 ? 1 : 0)) == 0)
    {
      PlayerSpec._speedFovUserId = (string) null;
      PlayerSpec._speedFovMeasuredSpeed = 0.0f;
    }
    else
    {
      Vector3 position = gorilla.HeadTransform.position;
      if (gorilla.UserId != PlayerSpec._speedFovUserId)
      {
        PlayerSpec._speedFovUserId = gorilla.UserId;
        PlayerSpec._speedFovMeasuredSpeed = 0.0f;
      }
      else
      {
        float num = Vector3.Distance(position, PlayerSpec._speedFovLastHeadPos) / deltaTime;
        if ((double) num < 40.0)
          PlayerSpec._speedFovMeasuredSpeed = Mathf.Lerp(PlayerSpec._speedFovMeasuredSpeed, num, 1f - Mathf.Exp(-6f * deltaTime));
      }
      PlayerSpec._speedFovLastHeadPos = position;
    }
    float num1 = PlayerSpec.SpeedFov ? PlayerSpec.SpeedFovBoost * Mathf.Clamp01(PlayerSpec._speedFovMeasuredSpeed / Mathf.Max(PlayerSpec.SpeedFovMaxSpeed, 0.1f)) : 0.0f;
    PlayerSpec._speedFovCurrentBoost = Mathf.Lerp(PlayerSpec._speedFovCurrentBoost, num1, 1f - Mathf.Exp(-8f * deltaTime));
    return PlayerSpec._speedFovCurrentBoost;
  }

  private static void HandleSmartOrbitLogic()
  {
    if (GorillaDataHandler.GorillaDataList.Count <= PlayerSpec.SpecGorilla)
      return;
    GorillaData gorillaData1 = GorillaDataHandler.GorillaDataList[PlayerSpec.SpecGorilla];
    if (gorillaData1 == null)
      return;
    string userId = NetworkSystem.Instance?.LocalPlayer?.UserId;
    Vector3 position = gorillaData1.BodyTransform.position;
    bool infected = gorillaData1.Infected;
    float num1 = float.MaxValue;
    int index1 = -1;
    for (int index2 = 0; index2 < GorillaDataHandler.GorillaDataList.Count; ++index2)
    {
      GorillaData gorillaData2 = GorillaDataHandler.GorillaDataList[index2];
      if (gorillaData2 != null && (index2 == PlayerSpec.SpecGorilla ? 1 : (gorillaData2.UserId == userId ? 1 : 0)) == 0 && infected != gorillaData2.Infected)
      {
        float num2 = Vector3.Distance(position, gorillaData2.BodyTransform.position);
        if (((double) num2 >= (double) num1 ? 0 : ((double) num2 > (double) PlayerSpec.SmartOrbitMinDist ? 1 : 0)) != 0)
        {
          num1 = num2;
          index1 = index2;
        }
      }
    }
    if (index1 == -1)
      return;
    Vector3 vector3 = (GorillaDataHandler.GorillaDataList[index1].BodyTransform.position - position);
    Vector3 normalized = vector3.normalized;
    float num3 = Mathf.Atan2(normalized.x, normalized.z) * 57.29578f + PlayerSpec.SmartOrbitOffsetAngle;
    float num4 = (float) (-(double) Mathf.Asin(normalized.y) * 57.295780181884766);
    PlayerSpec.OrbitY = Mathf.LerpAngle(PlayerSpec.OrbitY, num3, PlayerSpec.SmartOrbitSensitivity * Time.deltaTime);
    PlayerSpec.OrbitX = Mathf.LerpAngle(PlayerSpec.OrbitX, num4, PlayerSpec.SmartOrbitSensitivity * Time.deltaTime);
    PlayerSpec.OrbitX = Mathf.Clamp(PlayerSpec.OrbitX, -89f, 89f);
  }

  private static void HandleOrbitInput()
  {
    if (TextFieldMenuItem.AnyFocused)
      return;
    float num = PlayerSpec.OrbitKbSpeed * Plugin.moveSpeed;
    if (((ButtonControl) Keyboard.current.aKey).isPressed)
      PlayerSpec.OrbitY -= num * Time.deltaTime;
    if (((ButtonControl) Keyboard.current.dKey).isPressed)
      PlayerSpec.OrbitY += num * Time.deltaTime;
    if (((ButtonControl) Keyboard.current.wKey).isPressed)
      PlayerSpec.OrbitX -= num * Time.deltaTime;
    if (((ButtonControl) Keyboard.current.sKey).isPressed)
      PlayerSpec.OrbitX += num * Time.deltaTime;
    if (((ButtonControl) Keyboard.current.eKey).isPressed)
      PlayerSpec.TrackPosForward += Time.deltaTime * Plugin.moveSpeed;
    if (((ButtonControl) Keyboard.current.qKey).isPressed)
      PlayerSpec.TrackPosForward -= Time.deltaTime * Plugin.moveSpeed;
    if (PlayerSpec.MouseOrbit)
    {
      PlayerSpec.OrbitY += (float) ((double) ((InputControl<float>) ((Vector2Control) ((Pointer) Mouse.current).delta).x).ReadValue() * (double) Plugin.rotationSpeed * 0.10000000149011612);
      PlayerSpec.OrbitX -= (float) ((double) ((InputControl<float>) ((Vector2Control) ((Pointer) Mouse.current).delta).y).ReadValue() * (double) Plugin.rotationSpeed * 0.10000000149011612);
    }
    if ((XRSettings.isDeviceActive ? 0 : (Gamepad.current != null ? 1 : 0)) != 0)
    {
      Vector2 vector2_1 = ((InputControl<Vector2>) Gamepad.current.rightStick).ReadValue();
      Vector2 vector2_2 = ((InputControl<Vector2>) Gamepad.current.leftStick).ReadValue();
      if ((double) Mathf.Abs(vector2_1.x) > 0.15000000596046448)
        PlayerSpec.OrbitY += vector2_1.x * num * Time.deltaTime;
      if ((double) Mathf.Abs(vector2_1.y) > 0.15000000596046448)
        PlayerSpec.OrbitX -= vector2_1.y * num * Time.deltaTime;
      if ((double) Mathf.Abs(vector2_2.y) > 0.15000000596046448)
        PlayerSpec.TrackPosForward += vector2_2.y * Time.deltaTime * Plugin.moveSpeed;
    }
    if ((XRSettings.isDeviceActive ? 0 : (((UnityEngine.Object) ControllerInputPoller.instance != (UnityEngine.Object) null) ? 1 : 0)) != 0)
    {
      UnityEngine.XR.InputDevice controllerDevice1 = ControllerInputPoller.instance.rightControllerDevice;
      UnityEngine.XR.InputDevice controllerDevice2 = ControllerInputPoller.instance.leftControllerDevice;
      Vector2 vector2_3 = default;
      if ((!controllerDevice1.isValid ? 0 : (controllerDevice1.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis, out vector2_3) ? 1 : 0)) != 0)
      {
        if ((double) Mathf.Abs(vector2_3.x) > 0.15000000596046448)
          PlayerSpec.OrbitY += vector2_3.x * num * Time.deltaTime;
        if ((double) Mathf.Abs(vector2_3.y) > 0.15000000596046448)
          PlayerSpec.OrbitX -= vector2_3.y * num * Time.deltaTime;
      }
      Vector2 vector2_4 = default;
      if ((!controllerDevice2.isValid ? 0 : (controllerDevice2.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis, out vector2_4) ? 1 : 0)) != 0 && (double) Mathf.Abs(vector2_4.y) > 0.15000000596046448)
        PlayerSpec.TrackPosForward += vector2_4.y * Time.deltaTime * Plugin.moveSpeed;
    }
    PlayerSpec.OrbitX = Mathf.Clamp(PlayerSpec.OrbitX, -89f, 89f);
  }

  private static void HandlePinningLogic()
  {
    if ((!PlayerSpec.PinnedPlayerEnabled ? 0 : (!string.IsNullOrEmpty(PlayerSpec.PinnedPlayerId) ? 1 : 0)) == 0)
      return;
    for (int index = 0; index < GorillaDataHandler.GorillaDataList.Count; ++index)
    {
      if (GorillaDataHandler.GorillaDataList[index]?.UserId == PlayerSpec.PinnedPlayerId)
      {
        if (PlayerSpec.SpecGorilla == index)
          break;
        PlayerSpec.SpecPlayer(index);
        break;
      }
    }
  }

  private static void HandleStandardInput()
  {
    float moveSpeed = Plugin.moveSpeed;
    if (((ButtonControl) Keyboard.current.sKey).isPressed)
      PlayerSpec.TrackPosForward += Time.deltaTime * moveSpeed;
    if (((ButtonControl) Keyboard.current.wKey).isPressed)
      PlayerSpec.TrackPosForward -= Time.deltaTime * moveSpeed;
    if (((ButtonControl) Keyboard.current.dKey).isPressed)
      PlayerSpec.TrackPosRight += Time.deltaTime * moveSpeed;
    if (((ButtonControl) Keyboard.current.aKey).isPressed)
      PlayerSpec.TrackPosRight -= Time.deltaTime * moveSpeed;
    if (((ButtonControl) Keyboard.current.eKey).isPressed)
      PlayerSpec.TrackPosUp += Time.deltaTime * moveSpeed;
    if (((ButtonControl) Keyboard.current.qKey).isPressed)
      PlayerSpec.TrackPosUp -= Time.deltaTime * moveSpeed;
    if ((XRSettings.isDeviceActive ? 0 : (Gamepad.current != null ? 1 : 0)) != 0)
    {
      Vector2 vector2 = ((InputControl<Vector2>) Gamepad.current.leftStick).ReadValue();
      if ((double) Mathf.Abs(vector2.y) > 0.15000000596046448)
        PlayerSpec.TrackPosForward -= vector2.y * Time.deltaTime * moveSpeed;
      if ((double) Mathf.Abs(vector2.x) > 0.15000000596046448)
        PlayerSpec.TrackPosRight += vector2.x * Time.deltaTime * moveSpeed;
      float num1 = ((InputControl<float>) Gamepad.current.rightTrigger).ReadValue();
      float num2 = ((InputControl<float>) Gamepad.current.leftTrigger).ReadValue();
      if ((double) num1 > 0.10000000149011612)
        PlayerSpec.TrackPosUp += num1 * Time.deltaTime * moveSpeed;
      if ((double) num2 > 0.10000000149011612)
        PlayerSpec.TrackPosUp -= num2 * Time.deltaTime * moveSpeed;
    }
    if ((XRSettings.isDeviceActive ? 0 : (((UnityEngine.Object) ControllerInputPoller.instance != (UnityEngine.Object) null) ? 1 : 0)) == 0)
      return;
    UnityEngine.XR.InputDevice controllerDevice = ControllerInputPoller.instance.leftControllerDevice;
    Vector2 vector2_1 = default;
    if ((!controllerDevice.isValid ? 0 : (controllerDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis, out vector2_1) ? 1 : 0)) != 0)
    {
      if ((double) Mathf.Abs(vector2_1.y) > 0.15000000596046448)
        PlayerSpec.TrackPosForward -= vector2_1.y * Time.deltaTime * moveSpeed;
      if ((double) Mathf.Abs(vector2_1.x) > 0.15000000596046448)
        PlayerSpec.TrackPosRight += vector2_1.x * Time.deltaTime * moveSpeed;
    }
    if (!((UnityEngine.Object) InputManager.Ins != (UnityEngine.Object) null))
      return;
    if (InputManager.Ins.rightGrip)
      PlayerSpec.TrackPosUp += Time.deltaTime * moveSpeed;
    if (!InputManager.Ins.leftGrip)
      return;
    PlayerSpec.TrackPosUp -= Time.deltaTime * moveSpeed;
  }

  private static void HandleAutoPilotSwitching()
  {
    if (AutoPilot.AutoPilotEnabled)
    {
      AutoPilot.APCooldown -= Time.deltaTime;
      if ((double) AutoPilot.APCooldown > 0.0)
        return;
      AutoPilot.APCooldown = AutoPilot.APCooldownInterval;
      PlayerSpec.SpecPlayer(AutoPilot.GetClosest());
    }
    else
    {
      if (((ButtonControl) Keyboard.current.digit0Key).wasPressedThisFrame)
        PlayerSpec.SpecPlayer(0);
      if (((ButtonControl) Keyboard.current.digit1Key).wasPressedThisFrame)
        PlayerSpec.SpecPlayer(1);
      if (((ButtonControl) Keyboard.current.digit2Key).wasPressedThisFrame)
        PlayerSpec.SpecPlayer(2);
      if (((ButtonControl) Keyboard.current.digit3Key).wasPressedThisFrame)
        PlayerSpec.SpecPlayer(3);
      if (((ButtonControl) Keyboard.current.digit4Key).wasPressedThisFrame)
        PlayerSpec.SpecPlayer(4);
      if (((ButtonControl) Keyboard.current.digit5Key).wasPressedThisFrame)
        PlayerSpec.SpecPlayer(5);
      if (((ButtonControl) Keyboard.current.digit6Key).wasPressedThisFrame)
        PlayerSpec.SpecPlayer(6);
      if (((ButtonControl) Keyboard.current.digit7Key).wasPressedThisFrame)
        PlayerSpec.SpecPlayer(7);
      if (((ButtonControl) Keyboard.current.digit8Key).wasPressedThisFrame)
        PlayerSpec.SpecPlayer(8);
      if (!((ButtonControl) Keyboard.current.digit9Key).wasPressedThisFrame)
        return;
      PlayerSpec.SpecPlayer(9);
    }
  }

  private static void ApplyFinalTransform(UnityEngine.Camera camera, GorillaData gorilla)
  {
    if (PlayerSpec.SmoothMode)
    {
      if (PlayerSpec.SpecPart != "eyes")
      {
        if ((double) Time.time >= (double) PlayerSpec._transformCooldown)
        {
          PlayerSpec._transformCooldown = Time.time + 0.025f;
          PlayerSpec._cameraPosition = PlayerSpec.GetPositionBasedOnGorilla(gorilla);
          PlayerSpec._cR3 = PlayerSpec.GetRotationBasedOnGorilla(gorilla, PlayerSpec._cameraPosition);
        }
        PlayerSpec._finalPos = Vector3.Lerp(PlayerSpec._finalPos, PlayerSpec._cameraPosition, PlayerSpec.CameraLerp * Time.deltaTime * Plugin.Ins.lerpMult);
        PlayerSpec._finalcR3 = Quaternion.Slerp(PlayerSpec._finalcR3, PlayerSpec._cR3, PlayerSpec.QuatLerp * Time.deltaTime * Plugin.Ins.lerpMult);
        ((Component) camera).transform.position = PlayerSpec._finalPos;
        ((Component) camera).transform.rotation = PlayerSpec._finalcR3;
        return;
      }
    }
    else if (PlayerSpec.SpecPart != "eyes")
    {
      ((Component) camera).transform.parent = (Transform) null;
      Vector3 positionBasedOnGorilla = PlayerSpec.GetPositionBasedOnGorilla(gorilla);
      ((Component) camera).transform.position = positionBasedOnGorilla;
      ((Component) camera).transform.rotation = PlayerSpec.GetRotationBasedOnGorilla(gorilla, positionBasedOnGorilla);
      return;
    }
    ((Component) camera).transform.parent = (Transform) null;
    ((Component) camera).transform.position = gorilla.HeadTransform.TransformPoint(new Vector3(0.0f, 0.12f, PlayerSpec.FpvOffset));
    if (PlayerSpec.SmoothMode)
    {
      Quaternion rotation = gorilla.HeadTransform.rotation;
      Quaternion quaternion1 = ((Component) camera).transform.rotation;
      if ((!PlayerSpec.FpvClamping ? 0 : ((double) Quaternion.Angle(quaternion1, rotation) > (double) PlayerSpec.FpvClampAngle ? 1 : 0)) != 0)
        quaternion1 = Quaternion.RotateTowards(rotation, quaternion1, PlayerSpec.FpvClampAngle);
      Quaternion quaternion2 = Quaternion.Slerp(quaternion1, rotation, PlayerSpec.FpvSmoothness * Time.deltaTime);
      if (PlayerSpec.FpvRollLock)
        quaternion2 = Quaternion.LookRotation((quaternion2 * Vector3.forward), Vector3.up);
      ((Component) camera).transform.rotation = quaternion2;
    }
    else
      ((Component) camera).transform.rotation = Quaternion.Slerp(((Component) camera).transform.rotation, gorilla.HeadTransform.rotation, 15f * Time.deltaTime);
  }

  private static Vector3 GetPositionBasedOnGorilla(GorillaData gorilla)
  {
    Transform transform = PlayerSpec.SpecPart == "head" ? gorilla.HeadTransform : gorilla.BodyTransform;
    Vector3 positionBasedOnGorilla;
    if (PlayerSpec.OrbitMode)
    {
      Quaternion quaternion = Quaternion.Euler(PlayerSpec._smoothOrbitX, PlayerSpec._smoothOrbitY, 0.0f);
      positionBasedOnGorilla = ((transform.position + ((quaternion * Vector3.back) * PlayerSpec.TrackPosForward)) + (Vector3.up * PlayerSpec.TrackPosUp));
    }
    else
      positionBasedOnGorilla = ((((transform.position + (transform.up * 0.1f)) + (transform.forward * PlayerSpec.TrackPosForward * -1f)) + (transform.up * PlayerSpec.TrackPosUp)) + (transform.right * PlayerSpec.TrackPosRight));
    return positionBasedOnGorilla;
  }

  private static Quaternion GetRotationBasedOnGorilla(GorillaData gorilla, Vector3 fromPosition)
  {
    return Quaternion.LookRotation((PlayerSpec.SpecPart == "head" ? gorilla.HeadTransform.position : gorilla.BodyTransform.position - fromPosition));
  }

  public static void SpecPlayer(int plr)
  {
    Observation.VerticalOffset = 0.0f;
    if (PlayerSpec.SpecGorilla == plr || plr > GorillaDataHandler.GorillaDataList.Count - 1)
      return;
    ((Component) Plugin.Ins.camera).transform.parent = (Transform) null;
    PlayerSpec.SpecGorilla = Networking.InRoom ? ExtraTools.Clamp(plr, 0, GorillaDataHandler.GorillaDataList.Count - 1) : 0;
    LavaDistance.CurrentSpecGorilla = GorillaDataHandler.GorillaDataList[PlayerSpec.SpecGorilla];
    PlayerSpec.PinnedPlayerId = GorillaDataHandler.GorillaDataList[PlayerSpec.SpecGorilla].UserId;
  }

  public static void SwitchSpecPart()
  {
    string str;
    switch (PlayerSpec.SpecPart)
    {
      case "head":
        str = "body";
        break;
      case "body":
        str = "eyes";
        break;
      case "eyes":
        str = "head";
        break;
      default:
        str = "head";
        break;
    }
    PlayerSpec.SpecPart = str;
  }
}
