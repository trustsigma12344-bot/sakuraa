using SakuraaCastingMod.Desktop.ControlMode.Rigging;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Desktop.ControlMode.Animators;

public abstract class AnimatorBase : MonoBehaviour
{
  protected Transform Body;
  protected Transform Head;
  protected HandWalkingDriver LeftHandWalking;
  protected WalkingModeRig WalkingModeRig;
  protected HandWalkingDriver RightHandWalking;
  protected Rigidbody Rigidbody;

  protected virtual void Start()
  {
    UnityEngine.Debug.Log((object) "==START==");
    this.WalkingModeRig = WalkingModeRig.Instance;
    this.Body = this.WalkingModeRig.body;
    this.Head = this.WalkingModeRig.head;
    this.Rigidbody = this.WalkingModeRig.rigidbody;
    this.LeftHandWalking = this.WalkingModeRig.leftHandWalking;
    this.RightHandWalking = this.WalkingModeRig.rightHandWalking;
  }

  public abstract void Setup();

  public virtual void Cleanup()
  {
    ((Behaviour) this).enabled = false;
    this.WalkingModeRig.active = false;
    this.WalkingModeRig.useGravity = true;
    this.LeftHandWalking.Reset();
    this.RightHandWalking.Reset();
  }

  public abstract void Animate();
}
