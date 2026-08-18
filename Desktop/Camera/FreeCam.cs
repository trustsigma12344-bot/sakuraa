using SakuraaCastingMod.Core;
using SakuraaCastingMod.Desktop.Ui.Framework.MenuItems;
using SakuraaCastingMod.Features.Tools;
using SakuraaCastingMod.Shared.Helpers;
using SakuraaCastingMod.VR.Interaction;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.XR;

#nullable disable
namespace SakuraaCastingMod.Desktop.Camera;

public static class FreeCam
{
  public static Vector3 Pos;
  public static Vector3 Rot;
  [SavedSetting("FreeCamRotationMultiplier", 0.1f)]
  public static float RotationMultiplier = 0.1f;
  [SavedSetting("FreeCamMovementMultiplier", 10f)]
  public static float MovementMultiplier = 10f;
  private const float StickDeadzone = 0.15f;
  private const float DoublePressThreshold = 0.4f;
  private static float _lastBothRollKbTime;
  private static int _bothRollKbCount;
  private static bool _prevBothRollKb;
  private static float _lastBothBumperTime;
  private static int _bothBumperCount;
  private static bool _prevBothBumper;

  public static void FUpdateFreecam()
  {
    UnityEngine.Camera camera = Plugin.Ins.camera;
    Transform transform = ((Component) camera).transform;
    Vector3 forward = transform.forward;
    Vector3 normalized1 = forward.normalized;
    Vector3 right = transform.right;
    Vector3 normalized2 = right.normalized;
    Vector3 vector3_1 = Vector3.zero;
    Vector3 vector3_2 = Vector3.zero;
    bool flag1 = false;
    if ((Keyboard.current == null ? 0 : (!TextFieldMenuItem.AnyFocused ? 1 : 0)) != 0)
    {
      if (((ButtonControl) Keyboard.current[Keybinds.FcForwardKey]).isPressed)
        vector3_1 = (vector3_1 + normalized1);
      if (((ButtonControl) Keyboard.current[Keybinds.FcBackKey]).isPressed)
        vector3_1 = (vector3_1 - normalized1);
      if (((ButtonControl) Keyboard.current[Keybinds.FcRightKey]).isPressed)
        vector3_1 = (vector3_1 + normalized2);
      if (((ButtonControl) Keyboard.current[Keybinds.FcLeftKey]).isPressed)
        vector3_1 = (vector3_1 - normalized2);
      if (((ButtonControl) Keyboard.current[Keybinds.FcUpKey]).isPressed)
        vector3_1 = (vector3_1 + Vector3.up);
      if (((ButtonControl) Keyboard.current[Keybinds.FcDownKey]).isPressed)
        vector3_1 = (vector3_1 - Vector3.up);
      if (((ButtonControl) Keyboard.current[Keybinds.FcPitchDownKey]).isPressed)
        vector3_2 = (vector3_2 + new Vector3(Plugin.rotationSpeed * FreeCam.RotationMultiplier, 0.0f, 0.0f));
      if (((ButtonControl) Keyboard.current[Keybinds.FcPitchUpKey]).isPressed)
        vector3_2 = (vector3_2 - new Vector3(Plugin.rotationSpeed * FreeCam.RotationMultiplier, 0.0f, 0.0f));
      if (((ButtonControl) Keyboard.current[Keybinds.FcYawRightKey]).isPressed)
        vector3_2 = (vector3_2 + new Vector3(0.0f, Plugin.rotationSpeed * FreeCam.RotationMultiplier, 0.0f));
      if (((ButtonControl) Keyboard.current[Keybinds.FcYawLeftKey]).isPressed)
        vector3_2 = (vector3_2 - new Vector3(0.0f, Plugin.rotationSpeed * FreeCam.RotationMultiplier, 0.0f));
      bool flag2;
      if ((!(flag2 = ((ButtonControl) Keyboard.current[Keybinds.FcRollLeftKey]).isPressed && ((ButtonControl) Keyboard.current[Keybinds.FcRollRightKey]).isPressed) ? 0 : (!FreeCam._prevBothRollKb ? 1 : 0)) != 0)
      {
        if ((double) Time.time - (double) FreeCam._lastBothRollKbTime <= 0.40000000596046448)
          ++FreeCam._bothRollKbCount;
        else
          FreeCam._bothRollKbCount = 1;
        FreeCam._lastBothRollKbTime = Time.time;
        if (FreeCam._bothRollKbCount >= 2)
        {
          FreeCam.Rot = new Vector3(FreeCam.Rot.x, FreeCam.Rot.y, 0.0f);
          FreeCam._bothRollKbCount = 0;
        }
      }
      FreeCam._prevBothRollKb = flag2;
      if (!flag2)
      {
        if (((ButtonControl) Keyboard.current[Keybinds.FcRollLeftKey]).isPressed)
          vector3_2 = (vector3_2 - new Vector3(0.0f, 0.0f, Plugin.rotationSpeed * FreeCam.RotationMultiplier));
        if (((ButtonControl) Keyboard.current[Keybinds.FcRollRightKey]).isPressed)
          vector3_2 = (vector3_2 + new Vector3(0.0f, 0.0f, Plugin.rotationSpeed * FreeCam.RotationMultiplier));
      }
    }
    if ((Mouse.current == null ? 0 : (Mouse.current.rightButton.isPressed ? 1 : 0)) != 0)
      vector3_2 = (vector3_2 + new Vector3((float) (-(double) ((InputControl<float>) ((Vector2Control) ((Pointer) Mouse.current).delta).y).ReadValue() * ((double) Plugin.rotationSpeed / 10.0)) * FreeCam.RotationMultiplier, ((InputControl<float>) ((Vector2Control) ((Pointer) Mouse.current).delta).x).ReadValue() * (Plugin.rotationSpeed / 10f) * FreeCam.RotationMultiplier, 0.0f));
    if ((XRSettings.isDeviceActive ? 0 : (Gamepad.current != null ? 1 : 0)) != 0)
    {
      Vector2 vector2_1 = ((InputControl<Vector2>) Gamepad.current.leftStick).ReadValue();
      Vector2 vector2_2 = ((InputControl<Vector2>) Gamepad.current.rightStick).ReadValue();
      if ((double) vector2_1.magnitude > 0.15000000596046448)
        vector3_1 = ((vector3_1 + (normalized1 * vector2_1.y)) + (normalized2 * vector2_1.x));
      float num1 = ((InputControl<float>) Gamepad.current.leftTrigger).ReadValue();
      float num2 = ((InputControl<float>) Gamepad.current.rightTrigger).ReadValue();
      if ((double) num2 > 0.10000000149011612)
        vector3_1 = (vector3_1 + (Vector3.up * num2));
      if ((double) num1 > 0.10000000149011612)
        vector3_1 = (vector3_1 - (Vector3.up * num1));
      if ((double) vector2_2.magnitude > 0.15000000596046448)
        vector3_2 = (vector3_2 + new Vector3(-vector2_2.y * Plugin.rotationSpeed * FreeCam.RotationMultiplier, vector2_2.x * Plugin.rotationSpeed * FreeCam.RotationMultiplier, 0.0f));
      bool flag3;
      if ((!(flag3 = Gamepad.current.leftShoulder.isPressed && Gamepad.current.rightShoulder.isPressed) ? 0 : (!FreeCam._prevBothBumper ? 1 : 0)) != 0)
      {
        if ((double) Time.time - (double) FreeCam._lastBothBumperTime <= 0.40000000596046448)
          ++FreeCam._bothBumperCount;
        else
          FreeCam._bothBumperCount = 1;
        FreeCam._lastBothBumperTime = Time.time;
        if (FreeCam._bothBumperCount >= 2)
        {
          FreeCam.Rot = new Vector3(FreeCam.Rot.x, FreeCam.Rot.y, 0.0f);
          FreeCam._bothBumperCount = 0;
        }
      }
      FreeCam._prevBothBumper = flag3;
      if (!flag3)
      {
        if (Gamepad.current.leftShoulder.isPressed)
          vector3_2 = (vector3_2 - new Vector3(0.0f, 0.0f, Plugin.rotationSpeed * FreeCam.RotationMultiplier));
        if (Gamepad.current.rightShoulder.isPressed)
          vector3_2 = (vector3_2 + new Vector3(0.0f, 0.0f, Plugin.rotationSpeed * FreeCam.RotationMultiplier));
      }
      if ((Gamepad.current.leftStickButton.isPressed ? 1 : (Gamepad.current.rightStickButton.isPressed ? 1 : 0)) != 0)
        FreeCam.MovementMultiplier = 15f;
      flag1 = Gamepad.current.buttonNorth.isPressed;
    }
    if ((XRSettings.isDeviceActive ? 0 : (((UnityEngine.Object) ControllerInputPoller.instance != (UnityEngine.Object) null) ? 1 : 0)) != 0)
    {
      UnityEngine.XR.InputDevice controllerDevice1 = ControllerInputPoller.instance.leftControllerDevice;
      UnityEngine.XR.InputDevice controllerDevice2 = ControllerInputPoller.instance.rightControllerDevice;
      Vector2 vector2_3 = default;
      if ((!controllerDevice1.isValid ? 0 : (controllerDevice1.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis, out vector2_3) ? 1 : 0)) != 0 && (double) vector2_3.magnitude > 0.15000000596046448)
        vector3_1 = ((vector3_1 + (normalized1 * vector2_3.y)) + (normalized2 * vector2_3.x));
      Vector2 vector2_4 = default;
      if ((!controllerDevice2.isValid ? 0 : (controllerDevice2.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis, out vector2_4) ? 1 : 0)) != 0 && (double) vector2_4.magnitude > 0.15000000596046448)
        vector3_2 = (vector3_2 + new Vector3(-vector2_4.y * Plugin.rotationSpeed * FreeCam.RotationMultiplier, vector2_4.x * Plugin.rotationSpeed * FreeCam.RotationMultiplier, 0.0f));
      if (((UnityEngine.Object) InputManager.Ins != (UnityEngine.Object) null))
      {
        if (InputManager.Ins.rightGrip)
          vector3_1 = (vector3_1 + Vector3.up);
        if (InputManager.Ins.leftGrip)
          vector3_1 = (vector3_1 - Vector3.up);
        if (InputManager.Ins.rightPrimaryButton)
          flag1 = true;
      }
    }
    vector3_1.Normalize();
    FreeCam.Pos = (FreeCam.Pos + (vector3_1 * (float) ((double) Plugin.moveSpeed * (double) FreeCam.MovementMultiplier * 0.0099999997764825821)));
    if (Plugin.freeCamSmoothing)
      ((Component) camera).transform.localPosition = Vector3.Lerp(((Component) camera).transform.localPosition, FreeCam.Pos, Plugin.freeCamSmoothingSpeed * Time.deltaTime);
    else
      ((Component) camera).transform.localPosition = FreeCam.Pos;
    FreeCam.Rot = (FreeCam.Rot + vector3_2);
    if (Plugin.freeCamSmoothing)
      ((Component) camera).transform.localRotation = Quaternion.Slerp(((Component) camera).transform.localRotation, Quaternion.Euler(FreeCam.Rot), Plugin.freeCamSmoothingSpeed * Time.deltaTime);
    else
      ((Component) camera).transform.localEulerAngles = FreeCam.Rot;
    camera.nearClipPlane = Plugin.clippingPlaneNear;
    if (((Keyboard.current == null ? 0 : (((ButtonControl) Keybinds.Keyboard[Keybinds.ZoomKey]).isPressed ? 1 : 0)) | (flag1 ? 1 : 0)) == 0)
      camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, Plugin.fov, 0.1f);
    else
      camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, Plugin.zoomFov, 0.1f);
  }
}
