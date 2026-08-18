using GorillaLocomotion;
using SakuraaCastingMod.Desktop.ControlMode.Main;
using SakuraaCastingMod.Desktop.ControlMode.Rigging;
using SakuraaCastingMod.Features.Tools;
using SakuraaCastingMod.Shared.Helpers;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

#nullable disable
namespace SakuraaCastingMod.Desktop.ControlMode.Animators;

public class WalkAnimator : AnimatorBase
{
  private bool _hasJumped;
  private float _height = 0.2f;
  private float _heightModifier = 0.0f;
  private float _jumpTime;
  private bool _onJumpCooldown;
  private float _targetHeight;
  private float _walkCycleTime;

  private bool IsSprinting => ((ButtonControl) Keyboard.current[Keybinds.CmSprintKey]).isPressed;

  private bool NotMoving
  {
    get => (ControlModeInputHandler.InputDirectionNoY == Vector3.zero);
  }

  private void Update()
  {
    if (!SakuraaCastingMod.Desktop.ControlMode.Main.ControlMode.Instance.Enabled)
      return;
    if ((this._hasJumped || !this.WalkingModeRig.onGround ? 0 : (((ButtonControl) Keyboard.current[Keybinds.CmJumpKey]).wasPressedThisFrame ? 1 : 0)) != 0)
    {
      this._hasJumped = true;
      this._onJumpCooldown = true;
      this._jumpTime = Time.time;
      this.WalkingModeRig.active = false;
      this.Rigidbody.AddForce((Vector3.up * 6.5f), (ForceMode) 1);
    }
    if ((!this._hasJumped || this.WalkingModeRig.onGround ? ((double) Time.time - (double) this._jumpTime > 1.0 ? 1 : 0) : 1) != 0)
      this._onJumpCooldown = false;
    if ((!this.WalkingModeRig.onGround ? 0 : (!this._onJumpCooldown ? 1 : 0)) == 0)
      return;
    this._hasJumped = false;
  }

  public override void Animate()
  {
    this.MoveBody();
    this.AnimateHands();
  }

  public void MoveBody()
  {
    this.WalkingModeRig.active = this.WalkingModeRig.onGround && !this._hasJumped;
    this.WalkingModeRig.useGravity = !this.WalkingModeRig.onGround;
    if (Keyboard.current.altKey.isPressed)
      this._heightModifier += ((InputControl<Vector2>) Mouse.current.scroll).ReadValue().y / 50f;
    this._heightModifier = Mathf.Clamp(this._heightModifier, 0.0f, 1f);
    if (!this.WalkingModeRig.onGround)
      return;
    float num1;
    float num2;
    float num3;
    if (this.NotMoving)
    {
      num1 = 0.5f;
      num2 = 0.55f;
      num3 = (float) ((double) Time.time * 3.1415927410125732 * 2.0);
    }
    else
    {
      num1 = 0.3f;
      num2 = 0.8f;
      num3 = (float) ((double) this._walkCycleTime * 3.1415927410125732 * 2.0);
    }
    if (((ButtonControl) Keyboard.current[Keybinds.CmCrouchKey]).isPressed)
    {
      num1 -= 0.3f;
      num2 -= 0.3f;
    }
    float b1 = num1 + this._heightModifier;
    float b2 = num2 + this._heightModifier;
    this._targetHeight = Extensions.Map(Mathf.Sin(num3), -1f, 1f, b1, b2);
    this._height = this._targetHeight;
    Vector3 vector3_1 = (this.WalkingModeRig.lastGroundPosition + (Vector3.up * this._height * GTPlayer.Instance.NativeScale));
    Vector3 vector3_2 = this.Body.TransformDirection(ControlModeInputHandler.InputDirectionNoY);
    vector3_2.y = 0.0f;
    if ((double) Vector3.Dot(this.WalkingModeRig.lastNormal, Vector3.up) > 0.30000001192092896)
      vector3_2 = Vector3.ProjectOnPlane(vector3_2, this.WalkingModeRig.lastNormal);
    Vector3 vector3_3 = (vector3_2 * GTPlayer.Instance.NativeScale);
    float num4 = GTPlayer.Instance.maxJumpSpeed * ControlModeSettings.WalkSpeedMultiplier;
    float num5 = this.IsSprinting ? num4 * ControlModeSettings.SprintMultiplier : num4;
    this.WalkingModeRig.targetPosition = (vector3_1 + ((vector3_3 * num5) / 10f));
  }

  private void AnimateHands()
  {
    this.LeftHandWalking.lookAt = (this.LeftHandWalking.targetPosition + this.Body.forward);
    this.RightHandWalking.lookAt = (this.RightHandWalking.targetPosition + this.Body.forward);
    this.LeftHandWalking.up = this.Body.right;
    this.RightHandWalking.up = (-this.Body.right);
    if (!this.WalkingModeRig.onGround)
    {
      this.LeftHandWalking.grounded = false;
      this.RightHandWalking.grounded = false;
      Vector3 vector3 = (Vector3.up * 0.2f * GTPlayer.Instance.NativeScale);
      this.LeftHandWalking.targetPosition = this.LeftHandWalking.DefaultPosition;
      this.RightHandWalking.targetPosition = (this.RightHandWalking.DefaultPosition + vector3);
    }
    else
    {
      this.UpdateHitInfo(this.LeftHandWalking);
      this.UpdateHitInfo(this.RightHandWalking);
      if (!this.NotMoving)
      {
        if ((this.LeftHandWalking.grounded ? 0 : (!this.RightHandWalking.grounded ? 1 : 0)) != 0)
        {
          this.LeftHandWalking.grounded = true;
          this.LeftHandWalking.lastSnap = this.LeftHandWalking.hit;
          this.LeftHandWalking.targetPosition = this.LeftHandWalking.hit;
          this.RightHandWalking.lastSnap = this.RightHandWalking.hit;
          this.RightHandWalking.targetPosition = this.RightHandWalking.hit;
        }
        this.AnimateHand(this.LeftHandWalking, this.RightHandWalking);
        this.AnimateHand(this.RightHandWalking, this.LeftHandWalking);
      }
      else
      {
        this.LeftHandWalking.targetPosition = this.LeftHandWalking.hit;
        this.RightHandWalking.targetPosition = this.RightHandWalking.hit;
      }
    }
  }

  private void UpdateHitInfo(HandWalkingDriver handWalking)
  {
    Vector3 smoothedGroundPosition = this.WalkingModeRig.SmoothedGroundPosition;
    Vector3 lastNormal = this.WalkingModeRig.lastNormal;
    Vector3 vector3_1 = this.Body.TransformDirection((ControlModeInputHandler.InputDirectionNoY * Extensions.Map(Mathf.Abs(Vector3.Dot(ControlModeInputHandler.InputDirectionNoY, Vector3.forward)), 0.0f, 1f, 0.4f, 0.5f)));
    vector3_1.y = 0.0f;
    Vector3 vector3_2 = (vector3_1 * GTPlayer.Instance.NativeScale);
    RaycastHit raycastHit = default;
    if (Physics.Raycast((Vector3.ProjectOnPlane(((handWalking.DefaultPosition - smoothedGroundPosition) + vector3_2), lastNormal) + (smoothedGroundPosition + (lastNormal * 0.3f * GTPlayer.Instance.NativeScale))), (-lastNormal), out raycastHit, 0.5f * GTPlayer.Instance.NativeScale, (GTPlayer.Instance.locomotionEnabledLayers)))
    {
      handWalking.hit = raycastHit.point;
      handWalking.normal = raycastHit.normal;
      handWalking.lookAt = (((Component) handWalking).transform.position + this.Body.forward);
    }
    else
    {
      if (!this.NotMoving)
        return;
      handWalking.targetPosition = handWalking.DefaultPosition;
    }
  }

  private void AnimateHand(HandWalkingDriver handWalking, HandWalkingDriver otherHandWalking)
  {
    float num1 = Extensions.Map(Mathf.Abs(Vector3.Dot(ControlModeInputHandler.InputDirectionNoY, Vector3.forward)), 0.0f, 1f, 0.5f, 1.25f) * (Extensions.Map(Vector3.Dot(this.WalkingModeRig.lastNormal, Vector3.up), 0.0f, 1f, 0.1f, 0.6f) * GTPlayer.Instance.NativeScale);
    float num2 = otherHandWalking.hit.Distance(otherHandWalking.lastSnap) / num1;
    if ((!otherHandWalking.grounded ? 0 : ((double) num2 >= 1.0 ? 1 : 0)) != 0)
    {
      handWalking.targetPosition = handWalking.hit;
      handWalking.lastSnap = handWalking.hit;
      handWalking.grounded = true;
      otherHandWalking.grounded = false;
    }
    else if (otherHandWalking.grounded)
    {
      this._walkCycleTime = num2;
      handWalking.targetPosition = Vector3.Slerp(handWalking.lastSnap, handWalking.hit, this._walkCycleTime);
      HandWalkingDriver handWalkingDriver = handWalking;
      handWalkingDriver.targetPosition = (handWalkingDriver.targetPosition + (handWalking.normal * 0.2f * GTPlayer.Instance.NativeScale * Mathf.Sin(this._walkCycleTime)));
      handWalking.grounded = false;
    }
    if ((double) handWalking.targetPosition.Distance(handWalking.DefaultPosition) <= 1.0)
      return;
    handWalking.targetPosition = handWalking.DefaultPosition;
  }

  public override void Setup()
  {
  }
}
