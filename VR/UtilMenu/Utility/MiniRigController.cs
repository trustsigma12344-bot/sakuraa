using SakuraaCastingMod.Shared.Helpers;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.VR.UtilMenu.Utility;

public class MiniRigController : MonoBehaviour
{
  private MiniatureRig _miniRig;
  private VRRig _targetRig;
  private Transform _mountPoint;
  private float _scale = 0.07f;

  public void SetTarget(NetPlayer player, Transform mount)
  {
    if ((player == null ? 1 : (((UnityEngine.Object) mount == (UnityEngine.Object) null) ? 1 : 0)) == 0)
    {
      VRRig rigByNetPlayer = PlayerTranslator.GetRigByNetPlayer(player);
      if (((UnityEngine.Object) rigByNetPlayer == (UnityEngine.Object) null))
      {
        this.Hide();
      }
      else
      {
        if (this._miniRig == null)
          this._miniRig = new MiniatureRig();
        this._mountPoint = mount;
        if ((((UnityEngine.Object) this._targetRig != (UnityEngine.Object) rigByNetPlayer) ? 1 : (!this._miniRig.IsActive ? 1 : 0)) == 0)
          return;
        this._targetRig = rigByNetPlayer;
        this._miniRig.SetTarget(this._targetRig, ((Component) this._mountPoint).gameObject.layer);
        this._miniRig.Activate();
      }
    }
    else
      this.Hide();
  }

  public void Hide()
  {
    if (this._miniRig != null)
      this._miniRig.DeActivate();
    this._targetRig = (VRRig) null;
  }

  private void LateUpdate()
  {
    if ((((UnityEngine.Object) UtilMenuController.Instance == (UnityEngine.Object) null) ? 1 : (!UtilMenuController.Instance.isMenuEnabled ? 1 : 0)) != 0)
    {
      if ((this._miniRig == null ? 0 : (this._miniRig.IsActive ? 1 : 0)) == 0)
        return;
      this.Hide();
    }
    else
    {
      if ((this._miniRig == null || !this._miniRig.IsActive || ((UnityEngine.Object) this._targetRig == (UnityEngine.Object) null) ? 1 : (((UnityEngine.Object) this._mountPoint == (UnityEngine.Object) null) ? 1 : 0)) != 0)
        return;
      if (!((UnityEngine.Object) this._targetRig == (UnityEngine.Object) null))
      {
        this._miniRig.SyncBonesAndColors();
        if (!((UnityEngine.Object) this._miniRig.GorillaHolder != (UnityEngine.Object) null))
          return;
        this._miniRig.GorillaHolder.transform.position = ((this._mountPoint.position + (this._mountPoint.forward * 0.02f)) + (this._mountPoint.up * -0.004f));
        this._miniRig.GorillaHolder.transform.rotation = (this._mountPoint.rotation * Quaternion.Euler(-90f, 0.0f, 0.0f));
        this._miniRig.GorillaHolder.transform.localScale = (Vector3.one * this._scale);
      }
      else
        this.Hide();
    }
  }

  private void OnDestroy()
  {
    if (this._miniRig == null)
      return;
    this._miniRig.DestroyRig();
  }
}
