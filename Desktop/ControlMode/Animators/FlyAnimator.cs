using GorillaLocomotion;
using SakuraaCastingMod.Desktop.ControlMode.Main;
using SakuraaCastingMod.Desktop.ControlMode.Rigging;
using SakuraaCastingMod.Shared.Helpers;
using UnityEngine;
using UnityEngine.InputSystem;

#nullable disable
namespace SakuraaCastingMod.Desktop.ControlMode.Animators;

public class FlyAnimator : AnimatorBase
{
  private const float MaxSpeed = 5f;
  private const float MinSpeed = 0.0f;
  private int _layersBackup;
  private float _speed = 1f;

  private void Awake()
  {
    this._layersBackup = (GTPlayer.Instance.locomotionEnabledLayers);
  }

  private void Update()
  {
    this._speed += ((InputControl<Vector2>) Mouse.current.scroll).ReadValue().y / 50f;
    this._speed = Mathf.Clamp(this._speed, 0.0f, 5f);
  }

  public override void Animate()
  {
    this.AnimateBody();
    this.AnimateHands();
  }

  private void AnimateBody()
  {
    this.WalkingModeRig.active = true;
    this.WalkingModeRig.useGravity = false;
    this.WalkingModeRig.targetPosition = this.Body.TransformPoint((ControlModeInputHandler.InputDirection * this._speed * ControlModeSettings.FlySpeedMultiplier * GTPlayer.Instance.maxJumpSpeed));
  }

  private void AnimateHands()
  {
    this.LeftHandWalking.followRate = this.RightHandWalking.followRate = Extensions.Map(this._speed, 0.0f, 5f, 0.0f, 1f);
    this.LeftHandWalking.targetPosition = this.LeftHandWalking.DefaultPosition;
    this.RightHandWalking.targetPosition = this.RightHandWalking.DefaultPosition;
    this.LeftHandWalking.lookAt = (this.LeftHandWalking.targetPosition + this.Body.forward);
    this.RightHandWalking.lookAt = (this.RightHandWalking.targetPosition + this.Body.forward);
    this.LeftHandWalking.up = this.Body.right;
    this.RightHandWalking.up = (-this.Body.right);
  }

  public override void Cleanup()
  {
    // ISSUE: explicit non-virtual call
    base.Cleanup();
    HandWalkingDriver leftHandWalking = this.LeftHandWalking;
    HandWalkingDriver rightHandWalking = this.RightHandWalking;
    float num1 = 0.1f;
    rightHandWalking.followRate = 0.1f;
    double num2 = (double) num1;
    leftHandWalking.followRate = (float) num2;
    GTPlayer.Instance.locomotionEnabledLayers = (this._layersBackup);
  }

  public override void Setup()
  {
  }
}
