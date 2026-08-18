using GorillaNetworking;
using SakuraaCastingMod.Desktop.ControlMode.Rigging;
using SakuraaCastingMod.Shared.Helpers;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

#nullable disable
namespace SakuraaCastingMod.Desktop.ControlMode.Animators;

public class InteractAnimator : AnimatorBase
{
  private InteractAnimator.State _state;

  protected override void Start() => base.Start();

  private void Update()
  {
    if (this._state > InteractAnimator.State.Idle)
      return;
    if (Mouse.current.leftButton.wasPressedThisFrame)
    {
      this._state = InteractAnimator.State.Wait;
      this.StartCoroutine(this.Raycast(this.LeftHandWalking));
    }
    else
    {
      if (!Mouse.current.rightButton.wasPressedThisFrame)
        return;
      this._state = InteractAnimator.State.Wait;
      this.StartCoroutine(this.Raycast(this.RightHandWalking));
    }
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
    this.WalkingModeRig.targetPosition = this.Body.position;
  }

  private IEnumerator Raycast(HandWalkingDriver main)
  {
    Ray ray = UnityEngine.Camera.main.ScreenPointToRay((((InputControl<Vector2>) ((Pointer) Mouse.current).position).ReadValue()));
    int buttonLayer = LayerMask.GetMask(new string[2]
    {
      "GorillaInteractable",
      "TransparentFX"
    });
    RaycastHit[] raycastHitArray = Physics.RaycastAll(ray, 100f, buttonLayer);
    for (int index = 0; index < raycastHitArray.Length; ++index)
    {
      RaycastHit hit = raycastHitArray[index];
      bool flag2 = ((UnityEngine.Object) ((Component) hit.transform).GetComponent<GorillaPressableButton>()) || ((UnityEngine.Object) ((Component) hit.transform).GetComponent<GorillaKeyboardButton>()) || ((UnityEngine.Object) ((Component) hit.transform).GetComponent<GorillaPlayerLineButton>()) || ((UnityEngine.Object) hit.transform).name.ToLower().Contains("button");
      if (!flag2)
        continue;
      yield return (object) this.PressButton(main, (hit.point - (ray.direction * 0.05f)));
    }
    raycastHitArray = (RaycastHit[]) null;
    this._state = InteractAnimator.State.Idle;
  }

  private IEnumerator PressButton(HandWalkingDriver handWalking, Vector3 targetPosition)
  {
    this._state = InteractAnimator.State.Button;
    handWalking.grip = true;
    handWalking.targetPosition = ((Component) UnityEngine.Camera.main).transform.TransformPoint((Vector3.forward * 0.1f));
    handWalking.lookAt = targetPosition;
    handWalking.up = handWalking.isLeft ? this.Head.right : (-this.Head.right);
    yield return (object) new WaitForSeconds(0.1f);
    handWalking.targetPosition = targetPosition;
    while ((double) ((Component) handWalking).transform.position.Distance(targetPosition) > 0.05000000074505806)
      yield return (object) new WaitForFixedUpdate();
    handWalking.targetPosition = ((Component) UnityEngine.Camera.main).transform.TransformPoint((Vector3.forward * 0.15f));
    yield return (object) new WaitForSeconds(0.1f);
    handWalking.targetPosition = handWalking.DefaultPosition;
    this._state = InteractAnimator.State.Idle;
  }

  private void AnimateHands()
  {
    if (this._state != InteractAnimator.State.Idle)
      return;
    this.LeftHandWalking.targetPosition = this.LeftHandWalking.DefaultPosition;
    this.RightHandWalking.targetPosition = this.RightHandWalking.DefaultPosition;
    this.LeftHandWalking.lookAt = (this.LeftHandWalking.targetPosition + this.Head.forward);
    this.RightHandWalking.lookAt = (this.RightHandWalking.targetPosition + this.Head.forward);
    this.LeftHandWalking.up = this.Head.right;
    this.RightHandWalking.up = (-this.Head.right);
  }

  public override void Setup()
  {
  }

  public override void Cleanup()
  {
    // ISSUE: explicit non-virtual call
    base.Cleanup();
    this._state = InteractAnimator.State.Idle;
    this.StopAllCoroutines();
  }

  private void OnDestory()
  {
  }

  private enum State
  {
    Idle,
    Wait,
    Button,
  }
}
