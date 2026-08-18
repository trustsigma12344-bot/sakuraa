using GorillaLocomotion;
using SakuraaCastingMod.Core.Patches;
using System;
using System.Collections;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Desktop.ControlMode.Rigging;

public class HandWalkingDriver : MonoBehaviour
{
  public bool grip;
  public bool trigger;
  public bool primary;
  public bool secondary;
  public bool isLeft;
  public bool grounded;
  public bool hideControllerTransform = true;
  public Vector3 targetPosition;
  public Vector3 lookAt;
  public Vector3 up;
  public float followRate = 0.1f;
  public Vector3 hit;
  public Vector3 lastSnap;
  public Vector3 normal;
  private Transform _body;
  private Transform _controller;
  private Vector3 _defaultOffset;
  private VRMap _handMap;

  public Vector3 DefaultPosition => this._body.TransformPoint(this._defaultOffset);

  public void Reset()
  {
    this.primary = false;
    this.trigger = false;
    this.grip = false;
    this.targetPosition = this.DefaultPosition;
    this.lookAt = (this.targetPosition + this._body.forward);
    this.up = this.isLeft ? this._body.right : (-this._body.right);
    this.hideControllerTransform = true;
    this.grounded = false;
  }

  private void FixedUpdate()
  {
    ((Component) this).transform.position = Vector3.Lerp(((Component) this).transform.position, this.targetPosition, this.followRate);
    ((Component) this).transform.LookAt(this.lookAt, this.up);
    this._controller.position = this.hideControllerTransform ? this._body.position : ((Component) this).transform.position;
    if (!this.isLeft)
    {
      FingerPatch.forceRightGrip = this.grip;
      FingerPatch.forceRightPrimary = this.primary;
      FingerPatch.forceRightSecondary = this.secondary;
      FingerPatch.forceRightTrigger = this.trigger;
    }
    else
    {
      FingerPatch.forceLeftGrip = this.grip;
      FingerPatch.forceLeftPrimary = this.primary;
      FingerPatch.forceLeftSecondary = this.secondary;
      FingerPatch.forceLeftTrigger = this.trigger;
    }
  }

  private void OnEnable()
  {
    if ((!((UnityEngine.Object) this._body) ? 1 : (((UnityEngine.Object) WalkingModeRig.Instance.Animator == (UnityEngine.Object) null) ? 1 : 0)) != 0)
      return;
    ((Component) this).transform.position = this.DefaultPosition;
    this.targetPosition = this.DefaultPosition;
    try
    {
      this._handMap.overrideTarget = ((Component) this).transform;
    }
    catch (Exception ex)
    {
      UnityEngine.Debug.LogException(ex);
    }
  }

  public void Init(bool initIsLeft)
  {
    this.isLeft = initIsLeft;
    this._defaultOffset = new Vector3(initIsLeft ? -0.25f : 0.25f, -0.45f, 0.2f);
    this._handMap = initIsLeft ? GorillaTagger.Instance.offlineVRRig.leftHand : GorillaTagger.Instance.offlineVRRig.rightHand;
    this._body = WalkingModeRig.Instance.body;
    this._controller = initIsLeft ? GTPlayer.Instance.GetControllerTransform(true) : GTPlayer.Instance.GetControllerTransform(false);
    this._handMap = initIsLeft ? GorillaTagger.Instance.offlineVRRig.leftHand : GorillaTagger.Instance.offlineVRRig.rightHand;
    ((Component) this).transform.position = this.DefaultPosition;
    this.targetPosition = this.DefaultPosition;
    this.lastSnap = this.DefaultPosition;
    this.hit = this.DefaultPosition;
    this.up = Vector3.up;
  }

  private IEnumerator Disable(Action<HandWalkingDriver> onDisable)
  {
    ((Component) this).transform.position = this.DefaultPosition;
    yield return (object) new WaitForSeconds(0.1f);
    this._handMap.overrideTarget = (Transform) null;
    ((Behaviour) this).enabled = false;
    Action<HandWalkingDriver> action = onDisable;
    if (action != null)
      action(this);
  }
}
