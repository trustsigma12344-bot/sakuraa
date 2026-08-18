using System;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.VR.Interaction;

public class HoldableButton : MonoBehaviour
{
  public Action Callback;
  public float holdTime = 2f;
  public MeshRenderer targetRenderer;
  public Material unpressedMat;
  public Material pressedMat;
  private bool _isHolding;
  private bool _holdingHandLeft;
  private float _timer;
  private Color _startColor;
  private Color _endColor;
  private bool _colorsInitialized;

  private void Start()
  {
    if (((UnityEngine.Object) this.targetRenderer == (UnityEngine.Object) null))
      this.targetRenderer = ((Component) this).GetComponent<MeshRenderer>();
    this._startColor = (!((UnityEngine.Object) this.unpressedMat != (UnityEngine.Object) null) ? 0 : (this.unpressedMat.HasProperty("_Color") ? 1 : 0)) != 0 ? this.unpressedMat.color : (!((UnityEngine.Object) this.targetRenderer != (UnityEngine.Object) null) ? Color.white : ((Renderer) this.targetRenderer).sharedMaterial.color);
    this._endColor = (!((UnityEngine.Object) this.pressedMat != (UnityEngine.Object) null) ? 0 : (this.pressedMat.HasProperty("_Color") ? 1 : 0)) == 0 ? Color.red : this.pressedMat.color;
    this._colorsInitialized = true;
  }

  private void Update()
  {
    if ((!this._colorsInitialized ? 1 : (((UnityEngine.Object) this.targetRenderer == (UnityEngine.Object) null) ? 1 : 0)) != 0)
      return;
    if (this._isHolding)
    {
      this._timer += Time.deltaTime;
      float progress = Mathf.Clamp01(this._timer / this.holdTime);
      ((Renderer) this.targetRenderer).material.color = Color.Lerp(this._startColor, this._endColor, progress);
      HapticEngine.SetHoldProgress(this._holdingHandLeft, progress);
      if ((double) this._timer < (double) this.holdTime)
        return;
      Action callback = this.Callback;
      if (callback != null)
        callback();
      HapticEngine.Play(HapticPreset.HoldConfirm, this._holdingHandLeft);
      if ((!((UnityEngine.Object) GorillaTagger.Instance != (UnityEngine.Object) null) ? 0 : (((UnityEngine.Object) GorillaTagger.Instance.offlineVRRig != (UnityEngine.Object) null) ? 1 : 0)) != 0)
        GorillaTagger.Instance.offlineVRRig.PlayHandTapLocal(67, this._holdingHandLeft, 0.25f);
      this.ResetHold();
    }
    else
    {
      if ((double) this._timer <= 0.0)
        return;
      this._timer = 0.0f;
      this.ResetHold();
    }
  }

  private void OnTriggerEnter(Collider other)
  {
    GorillaTriggerColliderHandIndicator componentInParent = ((Component) other).GetComponentInParent<GorillaTriggerColliderHandIndicator>();
    if (((UnityEngine.Object) componentInParent == (UnityEngine.Object) null))
      return;
    this._isHolding = true;
    this._holdingHandLeft = componentInParent.isLeftHand;
    HapticEngine.Play(HapticPreset.HoldStart, this._holdingHandLeft);
  }

  private void OnTriggerExit(Collider other)
  {
    if (!((UnityEngine.Object) ((Component) other).GetComponentInParent<GorillaTriggerColliderHandIndicator>()))
      return;
    if ((!this._isHolding ? 0 : ((double) this._timer > 0.0 ? 1 : 0)) != 0)
      HapticEngine.Play(HapticPreset.HoldCancel, this._holdingHandLeft);
    this.ResetHold();
  }

  private void ResetHold()
  {
    this._isHolding = false;
    this._timer = 0.0f;
    HapticEngine.CancelHold(this._holdingHandLeft);
    if ((!((UnityEngine.Object) this.targetRenderer != (UnityEngine.Object) null) ? 0 : (((UnityEngine.Object) this.unpressedMat != (UnityEngine.Object) null) ? 1 : 0)) == 0)
      return;
    ((Renderer) this.targetRenderer).material = this.unpressedMat;
  }
}
