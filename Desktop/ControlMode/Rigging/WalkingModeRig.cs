using GorillaLocomotion;
using SakuraaCastingMod.Desktop.ControlMode.Animators;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Desktop.ControlMode.Rigging;

public class WalkingModeRig : MonoBehaviour
{
  private const float RaycastLength = 1.3f;
  private const float RaycastRadius = 0.3f;
  public Transform head;
  public Transform body;
  public HandWalkingDriver leftHandWalking;
  public HandWalkingDriver rightHandWalking;
  public Rigidbody rigidbody;
  public Vector3 targetPosition;
  public Vector3 lastNormal;
  public Vector3 lastGroundPosition;
  public bool onGround;
  public bool active;
  public bool useGravity = true;
  private readonly Vector3 _raycastOffset = new Vector3(0.0f, 0.4f, 0.0f);
  private AnimatorBase _animator;
  private float _scale = 1f;

  public static WalkingModeRig Instance { get; private set; }

  public Vector3 SmoothedGroundPosition { get; set; }

  public AnimatorBase Animator
  {
    get => this._animator;
    set
    {
      if ((!((UnityEngine.Object) this._animator) ? 0 : (((UnityEngine.Object) value != (UnityEngine.Object) this._animator) ? 1 : 0)) != 0)
        this._animator.Cleanup();
      this._animator = value;
      if (((UnityEngine.Object) this._animator))
      {
        ((Behaviour) this._animator).enabled = true;
        this._animator.Setup();
      }
      ((Behaviour) this.leftHandWalking).enabled = ((UnityEngine.Object) this._animator);
      ((Behaviour) this.rightHandWalking).enabled = ((UnityEngine.Object) this._animator);
      if (((UnityEngine.Object) this._animator))
        return;
      this.leftHandWalking.Reset();
      this.rightHandWalking.Reset();
    }
  }

  private void Awake()
  {
    WalkingModeRig.Instance = this;
    this.head = ((Component) GTPlayer.Instance.headCollider).transform;
    this.body = ((Component) GTPlayer.Instance.bodyCollider).transform;
    this.rigidbody = ((Collider) GTPlayer.Instance.bodyCollider).attachedRigidbody;
    this.leftHandWalking = new GameObject("Left Hand Driver").AddComponent<HandWalkingDriver>();
    this.leftHandWalking.Init(true);
    ((Behaviour) this.leftHandWalking).enabled = false;
    this.rightHandWalking = new GameObject("Right Hand Driver").AddComponent<HandWalkingDriver>();
    this.rightHandWalking.Init(false);
    ((Behaviour) this.rightHandWalking).enabled = false;
  }

  private void FixedUpdate()
  {
    this._scale = GTPlayer.Instance.NativeScale;
    this.SmoothedGroundPosition = Vector3.Lerp(this.SmoothedGroundPosition, this.lastGroundPosition, 0.6f);
    this.OnGroundRaycast();
    if (((UnityEngine.Object) this.Animator))
      this.Animator.Animate();
    this.Move();
  }

  private void Move()
  {
    if (!this.active)
      return;
    this.rigidbody.linearVelocity = Vector3.Lerp(this.rigidbody.linearVelocity, ((this.targetPosition - this.body.position) * 3f), 1f);
    if (this.useGravity)
      return;
    this.rigidbody.AddForce(((-Physics.gravity) * this.rigidbody.mass * this._scale));
  }

  private void OnGroundRaycast()
  {
    RaycastHit raycastHit1 = default;
    bool flag1 = Physics.Raycast(this.body.TransformPoint(this._raycastOffset), Vector3.down, out raycastHit1, 1.3f * this._scale, (GTPlayer.Instance.locomotionEnabledLayers));
    RaycastHit raycastHit2 = default;
    bool flag2 = Physics.SphereCast(this.body.TransformPoint(this._raycastOffset), 0.3f * this._scale, Vector3.down, out raycastHit2, 1.3f * this._scale, (GTPlayer.Instance.locomotionEnabledLayers));
    RaycastHit raycastHit3;
    if (!(flag1 & flag2))
    {
      if (flag2)
      {
        raycastHit3 = raycastHit2;
      }
      else
      {
        if (!flag1)
        {
          this.onGround = false;
          return;
        }
        raycastHit3 = raycastHit1;
      }
    }
    else
      raycastHit3 = (double) raycastHit1.distance <= (double) raycastHit2.distance ? raycastHit1 : raycastHit2;
    this.lastNormal = raycastHit3.normal;
    this.onGround = true;
    this.lastGroundPosition = raycastHit3.point;
    this.lastGroundPosition.x = this.body.position.x;
    this.lastGroundPosition.z = this.body.position.z;
  }
}
