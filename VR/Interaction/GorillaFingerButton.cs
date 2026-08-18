using SakuraaCastingMod.Shared.Helpers;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#nullable disable
namespace SakuraaCastingMod.VR.Interaction;

public class GorillaFingerButton : MonoBehaviour
{
  public Material pressedMaterial;
  public Material unpressedMaterial;
  public MeshRenderer buttonRenderer;
  public bool isNavigationButton = true;
  public bool isOn;
  public float debounceTime = 0.25f;
  public bool testPress;
  public bool testHandLeft;
  public bool cancelSound = false;
  public string offText;
  public string onText;
  public TMP_Text myTmpText;
  public TMP_Text myTmpText2;
  public Text myText;
  public UnityEvent onPressButton;
  private float touchTime;

  private void OnTriggerEnter(Collider collider)
  {
    if ((!((Behaviour) this).enabled ? 1 : ((double) Time.time < (double) this.touchTime + (double) this.debounceTime ? 1 : 0)) != 0)
      return;
    GorillaTriggerColliderHandIndicator componentInParent = ((Component) collider).GetComponentInParent<GorillaTriggerColliderHandIndicator>();
    if (((UnityEngine.Object) componentInParent == (UnityEngine.Object) null))
      return;
    this.touchTime = Time.time;
    this.cancelSound = false;
    this.StartCoroutine(this.ButtonFlash());
    HapticEngine.BeginPress(componentInParent.isLeftHand);
    this.onPressButton?.Invoke();
    Action<GorillaFingerButton, bool> onPressed = this.onPressed;
    if (onPressed != null)
      onPressed(this, componentInParent.isLeftHand);
    this.ButtonActivation();
    this.ButtonActivationWithHand(componentInParent.isLeftHand);
    if (!this.cancelSound)
    {
      AudioClip clip = this.isNavigationButton ? Sounds.subtleClickSfx : Sounds.boingSfx;
      if (((UnityEngine.Object) clip != (UnityEngine.Object) null))
        Sounds.PlaySound(clip, isLeftHand: componentInParent.isLeftHand);
      else
        GorillaTagger.Instance.offlineVRRig.PlayHandTapLocal(67, componentInParent.isLeftHand, 0.05f);
      HapticEngine.EndPress(this.isNavigationButton ? HapticPreset.NavTap : HapticPreset.ButtonClick);
    }
    else
      HapticEngine.AbortPress();
  }

  private IEnumerator ButtonFlash()
  {
    if ((!((UnityEngine.Object) this.buttonRenderer != (UnityEngine.Object) null) ? 0 : (((UnityEngine.Object) this.pressedMaterial != (UnityEngine.Object) null) ? 1 : 0)) != 0)
      ((Renderer) this.buttonRenderer).material = this.pressedMaterial;
    yield return (object) new WaitForSeconds(0.1f);
    this.UpdateColor();
  }

  public event Action<GorillaFingerButton, bool> onPressed;

  public virtual void UpdateColor()
  {
    if (!this.isOn)
    {
      if (((UnityEngine.Object) this.buttonRenderer != (UnityEngine.Object) null))
        ((Renderer) this.buttonRenderer).material = this.unpressedMaterial;
      if (((UnityEngine.Object) this.myTmpText != (UnityEngine.Object) null))
        this.myTmpText.text = this.offText;
      if (((UnityEngine.Object) this.myTmpText2 != (UnityEngine.Object) null))
      {
        this.myTmpText2.text = this.offText;
      }
      else
      {
        if (!((UnityEngine.Object) this.myText != (UnityEngine.Object) null))
          return;
        this.myText.text = this.offText;
      }
    }
    else
    {
      if (((UnityEngine.Object) this.buttonRenderer != (UnityEngine.Object) null))
        ((Renderer) this.buttonRenderer).material = this.pressedMaterial;
      if (((UnityEngine.Object) this.myTmpText != (UnityEngine.Object) null))
        this.myTmpText.text = this.onText;
      if (!((UnityEngine.Object) this.myTmpText2 != (UnityEngine.Object) null))
      {
        if (!((UnityEngine.Object) this.myText != (UnityEngine.Object) null))
          return;
        this.myText.text = this.onText;
      }
      else
        this.myTmpText2.text = this.onText;
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
}
