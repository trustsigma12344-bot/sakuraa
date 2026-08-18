using GorillaLocomotion;
using SakuraaCastingMod.Desktop.ControlMode.Rigging;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

#nullable disable
namespace SakuraaCastingMod.Desktop.ControlMode.Animators;

public class PoseAnimator : AnimatorBase
{
  private Vector3 _eulerAngles;
  private Vector3 _lookAtLeft = Vector3.forward;
  private Vector3 _lookAtRight = Vector3.forward;
  private HandWalkingDriver _main;
  private Vector3 _offsetLeft;
  private Vector3 _offsetRight;
  private HandWalkingDriver _secondary;
  private float _zRotationLeft;
  private float _zRotationRight;

  private void Update()
  {
    if ((((UnityEngine.Object) this._main == (UnityEngine.Object) null) || ((UnityEngine.Object) this._secondary == (UnityEngine.Object) null) || Mouse.current == null ? 1 : (Keyboard.current == null ? 1 : 0)) != 0)
      return;
    if (((ButtonControl) Keyboard.current.qKey).wasPressedThisFrame)
    {
      HandWalkingDriver secondary = this._secondary;
      HandWalkingDriver main = this._main;
      this._main = secondary;
      this._secondary = main;
    }
    if (((ButtonControl) Keyboard.current.rKey).isPressed)
      this.RotateHand();
    else
      this.PositionHand();
    Vector3 vector3 = this._main.isLeft ? this._lookAtLeft : this._lookAtRight;
    this._main.up = (Quaternion.AngleAxis((this._main.isLeft ? this._zRotationLeft : this._zRotationRight) * (this._main.isLeft ? -1f : 1f), vector3) * this.Head.up);
    this._main.trigger = Mouse.current.leftButton.isPressed;
    this._main.grip = Mouse.current.rightButton.isPressed;
    this._main.primary = Mouse.current.backButton.isPressed || ((ButtonControl) Keyboard.current.leftBracketKey).isPressed;
    this._main.secondary = Mouse.current.forwardButton.isPressed || ((ButtonControl) Keyboard.current.rightBracketKey).isPressed;
  }

  public override void Animate()
  {
    this.AnimateBody();
    this.AnimateHands();
  }

  private void RotateHand()
  {
    this._eulerAngles.x -= ((InputControl<Vector2>) ((Pointer) Mouse.current).delta).value.y / 10f;
    if ((double) this._eulerAngles.x > 180.0)
      this._eulerAngles.x -= 360f;
    this._eulerAngles.x = Mathf.Clamp(this._eulerAngles.x, -85f, 85f);
    this._eulerAngles.y += ((InputControl<Vector2>) ((Pointer) Mouse.current).delta).value.x / 10f;
    if ((double) this._eulerAngles.y > 180.0)
      this._eulerAngles.y -= 360f;
    this._eulerAngles.y = Mathf.Clamp(this._eulerAngles.y, -85f, 85f);
    if (!this._main.isLeft)
    {
      this._lookAtRight = (Quaternion.Euler(this._eulerAngles) * this.Head.forward);
      this._zRotationRight += ((InputControl<Vector2>) Mouse.current.scroll).ReadValue().y / 5f;
    }
    else
    {
      this._lookAtLeft = (Quaternion.Euler(this._eulerAngles) * this.Head.forward);
      this._zRotationLeft += ((InputControl<Vector2>) Mouse.current.scroll).ReadValue().y / 5f;
    }
  }

  private void PositionHand()
  {
    Vector3 vector3 = this._main.isLeft ? this._offsetLeft : this._offsetRight;
    vector3.z += ((InputControl<Vector2>) Mouse.current.scroll).ReadValue().y / 50f;
    if (((ButtonControl) Keyboard.current.upArrowKey).wasPressedThisFrame)
      vector3.z += 0.1f;
    if (((ButtonControl) Keyboard.current.downArrowKey).wasPressedThisFrame)
      vector3.z -= 0.1f;
    vector3.z = Mathf.Clamp(vector3.z, -0.25f, 0.75f);
    vector3.x += ((InputControl<Vector2>) ((Pointer) Mouse.current).delta).ReadValue().x / 1000f;
    vector3.x = Mathf.Clamp(vector3.x, -0.5f, 0.5f);
    vector3.y += ((InputControl<Vector2>) ((Pointer) Mouse.current).delta).ReadValue().y / 1000f;
    vector3.y = Mathf.Clamp(vector3.y, -0.5f, 0.5f);
    if (!this._main.isLeft)
      this._offsetRight = vector3;
    else
      this._offsetLeft = vector3;
  }

  private void AnimateBody()
  {
    this.WalkingModeRig.active = true;
    this.WalkingModeRig.useGravity = false;
    this.WalkingModeRig.targetPosition = this.Body.position;
  }

  private void AnimateHands()
  {
    this._main.targetPosition = this.Body.TransformPoint(new Vector3((this._main.isLeft ? -1f : 1f) * 0.2f, 0.1f, 0.3f) + (this._main.isLeft ? this._offsetLeft : this._offsetRight));
    this._main.lookAt = this._main.targetPosition + (this._main.isLeft ? this._lookAtLeft : this._lookAtRight);
    this._main.hideControllerTransform = false;
  }

  public override void Setup()
  {
    this.Start();
    this._main = this.RightHandWalking;
    this._secondary = this.LeftHandWalking;
    this._offsetLeft = Vector3.zero;
    this._lookAtLeft = this.Head.forward;
    this._offsetRight = Vector3.zero;
    this._lookAtRight = this.Head.forward;
    this._secondary.targetPosition = (this._secondary.DefaultPosition + (Vector3.up * 0.2f * GTPlayer.Instance.NativeScale));
    this._secondary.lookAt = (this._secondary.targetPosition + this.Head.forward);
    this._secondary.up = this.Body.right * (this._main.isLeft ? -1f : 1f);
  }
}
