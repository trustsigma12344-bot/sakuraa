using SakuraaCastingMod.Shared.Helpers;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#nullable disable
namespace SakuraaCastingMod.VR.Interaction;

public class UtilFingerButton : MonoBehaviour
{
  public Material pressedMaterial;
  public Material unpressedMaterial;
  public MeshRenderer buttonRenderer;
  public bool isNavigationButton = false;
  public bool isOn;
  public float debounceTime = 0.25f;
  public bool allowLeftHand = false;
  public bool testPress;
  public bool testHandLeft;
  public string offText;
  public string onText;
  public Text myText;
  public UnityEvent onPressButton;
  private float _touchTime;

  private void Awake()
  {
    if (!((UnityEngine.Object) this.buttonRenderer == (UnityEngine.Object) null))
      return;
    this.buttonRenderer = ((Component) this).GetComponent<MeshRenderer>();
  }

  private void OnTriggerEnter(Collider collider)
  {
    if ((!((Behaviour) this).enabled ? 1 : ((double) Time.time < (double) this._touchTime + (double) this.debounceTime ? 1 : 0)) != 0)
      return;
    GorillaTriggerColliderHandIndicator componentInParent = ((Component) collider).GetComponentInParent<GorillaTriggerColliderHandIndicator>();
    if (((UnityEngine.Object) componentInParent == (UnityEngine.Object) null) || (!componentInParent.isLeftHand ? 0 : (!this.allowLeftHand ? 1 : 0)) != 0)
      return;
    this._touchTime = Time.time;
    if ((this.isOn ? 0 : (!this.isNavigationButton ? 1 : 0)) != 0)
      this.StartCoroutine(this.ButtonFlash());
    HapticEngine.BeginPress(componentInParent.isLeftHand);
    this.onPressButton?.Invoke();
    Action<UtilFingerButton, bool> onPressed = this.onPressed;
    if (onPressed != null)
      onPressed(this, componentInParent.isLeftHand);
    this.ButtonActivation();
    this.ButtonActivationWithHand(componentInParent.isLeftHand);
    AudioClip clip = this.isNavigationButton ? Sounds.subtleClickSfx : Sounds.boingSfx;
    if (!((UnityEngine.Object) clip != (UnityEngine.Object) null))
      GorillaTagger.Instance.offlineVRRig.PlayHandTapLocal(67, componentInParent.isLeftHand, 0.05f);
    else
      Sounds.PlaySound(clip, isLeftHand: componentInParent.isLeftHand);
    HapticEngine.EndPress(this.isNavigationButton ? HapticPreset.NavTap : HapticPreset.ButtonClick);
    this.UpdateColor();
  }

  private IEnumerator ButtonFlash()
  {
    if ((!((UnityEngine.Object) this.buttonRenderer != (UnityEngine.Object) null) ? 0 : (((UnityEngine.Object) this.pressedMaterial != (UnityEngine.Object) null) ? 1 : 0)) != 0)
      ((Renderer) this.buttonRenderer).material = this.pressedMaterial;
    yield return (object) new WaitForSeconds(0.1f);
    this.UpdateColor();
  }

  public event Action<UtilFingerButton, bool> onPressed;

  public virtual void UpdateColor()
  {
    if (!this.isOn)
    {
      if ((!((UnityEngine.Object) this.buttonRenderer != (UnityEngine.Object) null) ? 0 : (((UnityEngine.Object) this.unpressedMaterial != (UnityEngine.Object) null) ? 1 : 0)) == 0)
        return;
      ((Renderer) this.buttonRenderer).material = this.unpressedMaterial;
    }
    else
    {
      if ((!((UnityEngine.Object) this.buttonRenderer != (UnityEngine.Object) null) ? 0 : (((UnityEngine.Object) this.pressedMaterial != (UnityEngine.Object) null) ? 1 : 0)) == 0)
        return;
      ((Renderer) this.buttonRenderer).material = this.pressedMaterial;
    }
  }

  public virtual void ButtonActivation()
  {
  }

  public virtual void ButtonActivationWithHand(bool isLeftHand)
  {
  }

  public virtual void ResetState()
  {
    this.isOn = false;
    this.UpdateColor();
  }

  public void SetButtonListener(UnityAction action)
  {
    if (this.onPressButton == null)
      this.onPressButton = new UnityEvent();
    ((UnityEventBase) this.onPressButton).RemoveAllListeners();
    this.onPressButton.AddListener(action);
  }
}
