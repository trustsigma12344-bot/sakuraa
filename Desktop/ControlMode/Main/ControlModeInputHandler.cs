using GorillaLocomotion;
using SakuraaCastingMod.Desktop.ControlMode.Animators;
using SakuraaCastingMod.Desktop.ControlMode.Menus;
using SakuraaCastingMod.Desktop.ControlMode.Rigging;
using SakuraaCastingMod.Desktop.Ui;
using SakuraaCastingMod.Features.Tools;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.XR;

#nullable disable
namespace SakuraaCastingMod.Desktop.ControlMode.Main;

public class ControlModeInputHandler : MonoBehaviour
{
  public static ControlModeInputHandler Instance;
  public static Vector3 InputDirection;
  public static Vector3 InputDirectionNoY;
  public static float Pitch;
  private const float MaxPitch = 80f;
  private float _lastSpacePressTime = -1f;
  private const float DoublePressThreshold = 1f;
  private const float StickDeadzone = 0.15f;
  private bool _prevGpSouth;
  private float _lastGpSouthPressTime = -1f;
  private bool _prevGpDPadLeft;
  private bool _prevGpDPadUp;
  private bool _prevGpDPadRight;

  private void Awake() => ControlModeInputHandler.Instance = this;

  private void Update()
  {
    try
    {
      if (!SakuraaCastingMod.Desktop.ControlMode.Main.ControlMode.Instance.Enabled)
        return;
      bool flag1 = ((UnityEngine.Object) ControlModeGUI.Instance != (UnityEngine.Object) null) && ControlModeGUI.Instance.isInUse;
      AnimatorBase animator1 = WalkingModeRig.Instance.Animator;
      if ((flag1 ? 1 : (MainMenus.ShowMainMenu ? 1 : 0)) != 0)
      {
        Cursor.lockState = (CursorLockMode) 0;
        Cursor.visible = true;
      }
      else
      {
        Cursor.lockState = (CursorLockMode) 1;
        Cursor.visible = false;
      }
      bool flag2 = ((UnityEngine.Object) animator1 == (UnityEngine.Object) null) || ((UnityEngine.Object) animator1 != (UnityEngine.Object) SakuraaCastingMod.Desktop.ControlMode.Main.ControlMode.Instance.handAnimator);
      if ((!(Mouse.current.rightButton.isPressed & flag2) ? 0 : (!flag1 ? 1 : 0)) != 0)
      {
        GTPlayer.Instance.Turn(((InputControl<Vector2>) ((Pointer) Mouse.current).delta).value.x / 10f * ControlModeSettings.TurnSpeed);
        ControlModeInputHandler.Pitch = Mathf.Clamp(ControlModeInputHandler.Pitch - ((InputControl<Vector2>) ((Pointer) Mouse.current).delta).value.y / 10f * ControlModeSettings.TurnSpeed, -80f, 80f);
      }
      if ((((XRSettings.isDeviceActive ? 0 : (Gamepad.current != null ? 1 : 0)) & (flag2 ? 1 : 0)) == 0 ? 0 : (!flag1 ? 1 : 0)) != 0)
      {
        Vector2 vector2 = ((InputControl<Vector2>) Gamepad.current.rightStick).ReadValue();
        if ((double) Mathf.Abs(vector2.x) > 0.15000000596046448)
          GTPlayer.Instance.Turn(vector2.x * 2f * ControlModeSettings.TurnSpeed);
      }
      if (flag1)
        return;
      this.GetInputDirection();
      bool pressedThisFrame = ((ButtonControl) Keyboard.current[Keybinds.CmToggleFlyKey]).wasPressedThisFrame;
      bool flag3 = false;
      if ((XRSettings.isDeviceActive ? 0 : (Gamepad.current != null ? 1 : 0)) != 0)
      {
        bool isPressed;
        flag3 = (isPressed = Gamepad.current.buttonSouth.isPressed) && !this._prevGpSouth;
        this._prevGpSouth = isPressed;
      }
      if (pressedThisFrame | flag3)
      {
        if ((double) Time.time - (pressedThisFrame ? (double) this._lastSpacePressTime : (double) this._lastGpSouthPressTime) <= 1.0)
        {
          AnimatorBase animator2 = WalkingModeRig.Instance.Animator;
          if (!((UnityEngine.Object) animator2 == (UnityEngine.Object) SakuraaCastingMod.Desktop.ControlMode.Main.ControlMode.Instance.walkAnimator))
          {
            if (((UnityEngine.Object) animator2 == (UnityEngine.Object) SakuraaCastingMod.Desktop.ControlMode.Main.ControlMode.Instance.flyAnimator))
              WalkingModeRig.Instance.Animator = SakuraaCastingMod.Desktop.ControlMode.Main.ControlMode.Instance.walkAnimator;
          }
          else
            WalkingModeRig.Instance.Animator = SakuraaCastingMod.Desktop.ControlMode.Main.ControlMode.Instance.flyAnimator;
          this._lastSpacePressTime = -1f;
          this._lastGpSouthPressTime = -1f;
        }
        else
        {
          if (pressedThisFrame)
            this._lastSpacePressTime = Time.time;
          if (flag3)
            this._lastGpSouthPressTime = Time.time;
        }
      }
      bool flag4 = false;
      bool flag5 = false;
      bool flag6 = false;
      if ((XRSettings.isDeviceActive ? 0 : (Gamepad.current != null ? 1 : 0)) != 0)
      {
        bool isPressed1;
        flag4 = (isPressed1 = Gamepad.current.dpad.left.isPressed) && !this._prevGpDPadLeft;
        this._prevGpDPadLeft = isPressed1;
        bool isPressed2;
        flag5 = (isPressed2 = Gamepad.current.dpad.up.isPressed) && !this._prevGpDPadUp;
        this._prevGpDPadUp = isPressed2;
        bool isPressed3;
        flag6 = (isPressed3 = Gamepad.current.dpad.right.isPressed) && !this._prevGpDPadRight;
        this._prevGpDPadRight = isPressed3;
      }
      if (((ButtonControl) Keyboard.current[Keybinds.CmAnimWalkKey]).wasPressedThisFrame | flag4)
        WalkingModeRig.Instance.Animator = SakuraaCastingMod.Desktop.ControlMode.Main.ControlMode.Instance.walkAnimator;
      if (((ButtonControl) Keyboard.current[Keybinds.CmAnimFlyKey]).wasPressedThisFrame | flag5)
        WalkingModeRig.Instance.Animator = SakuraaCastingMod.Desktop.ControlMode.Main.ControlMode.Instance.flyAnimator;
      if (!(((ButtonControl) Keyboard.current[Keybinds.CmAnimHandKey]).wasPressedThisFrame | flag6))
        return;
      WalkingModeRig.Instance.Animator = SakuraaCastingMod.Desktop.ControlMode.Main.ControlMode.Instance.handAnimator;
    }
    catch
    {
    }
  }

  private void GetInputDirection()
  {
    Vector3 vector3_1 = this.KeyboardAndGamepadInput();
    if ((double) vector3_1.magnitude > 0.0)
    {
      ControlModeInputHandler.InputDirection = vector3_1.normalized;
      ControlModeInputHandler.InputDirectionNoY = vector3_1;
      ControlModeInputHandler.InputDirectionNoY.y = 0.0f;
    }
    else if (!XRSettings.isDeviceActive)
    {
      UnityEngine.XR.InputDevice controllerDevice1 = ControllerInputPoller.instance.leftControllerDevice;
      UnityEngine.XR.InputDevice controllerDevice2 = ControllerInputPoller.instance.rightControllerDevice;
      Vector2 vector2_1 = default;
      controllerDevice1.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis, out vector2_1);
      Vector2 vector2_2 = default;
      controllerDevice2.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis, out vector2_2);
      Vector3 vector3_2 = new Vector3(vector2_1.x, vector2_2.y, vector2_1.y);
      ControlModeInputHandler.InputDirection = vector3_2.normalized;
      ControlModeInputHandler.InputDirectionNoY = new Vector3(vector2_1.x, 0.0f, vector2_1.y);
    }
    ControlModeInputHandler.InputDirectionNoY.Normalize();
  }

  private Vector3 KeyboardAndGamepadInput()
  {
    float num1 = 0.0f;
    float num2 = 0.0f;
    float num3 = 0.0f;
    if (((ButtonControl) Keyboard.current[Keybinds.CmLeftKey]).isPressed)
      --num1;
    if (((ButtonControl) Keyboard.current[Keybinds.CmRightKey]).isPressed)
      ++num1;
    if (((ButtonControl) Keyboard.current[Keybinds.CmBackKey]).isPressed)
      --num2;
    if (((ButtonControl) Keyboard.current[Keybinds.CmForwardKey]).isPressed)
      ++num2;
    if (((ButtonControl) Keyboard.current[Keybinds.CmCrouchKey]).isPressed)
      --num3;
    if (((ButtonControl) Keyboard.current[Keybinds.CmJumpKey]).isPressed)
      ++num3;
    if ((XRSettings.isDeviceActive ? 0 : (Gamepad.current != null ? 1 : 0)) != 0)
    {
      Vector2 vector2 = ((InputControl<Vector2>) Gamepad.current.leftStick).ReadValue();
      if ((double) Mathf.Abs(vector2.x) > 0.15000000596046448)
        num1 += vector2.x;
      if ((double) Mathf.Abs(vector2.y) > 0.15000000596046448)
        num2 += vector2.y;
      float num4 = ((InputControl<float>) Gamepad.current.rightTrigger).ReadValue();
      float num5 = ((InputControl<float>) Gamepad.current.leftTrigger).ReadValue();
      if ((double) num4 > 0.10000000149011612)
        num3 += num4;
      if ((double) num5 > 0.10000000149011612)
        num3 -= num5;
    }
    return new Vector3(num1, num3, num2);
  }
}
