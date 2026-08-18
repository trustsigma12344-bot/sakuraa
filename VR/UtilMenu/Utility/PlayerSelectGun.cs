using GorillaLocomotion;
using SakuraaCastingMod.Core;
using SakuraaCastingMod.Shared.Helpers;
using SakuraaCastingMod.VR.Interaction;
using System;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.VR.UtilMenu.Utility;

public class PlayerSelectGun : MonoBehaviour
{
  public Action<VRRig> Callback;
  private GameObject _visual;
  private TextMesh _nameText;
  private LineRenderer _line;
  private bool _wasTriggerDown;

  private void Start()
  {
    this._line = ((Component) this).gameObject.AddComponent<LineRenderer>();
    this._line.startWidth = 0.01f;
    this._line.endWidth = 0.005f;
    Material material = ExtraTools.MakeMaterialTransparent(new Material(Shader.Find("GorillaTag/UberShader")));
    material.color = new Color(1f, 0.41f, 0.71f, 0.7f);
    ((Renderer) this._line).material = material;
    this._visual = GameObject.CreatePrimitive((PrimitiveType) 0);
    this._visual.transform.localScale = (Vector3.one * 0.2f);
    UnityEngine.Object.Destroy((UnityEngine.Object) this._visual.GetComponent<Collider>());
    this._visual.GetComponent<Renderer>().material = material;
    GameObject gameObject = new GameObject("NameText");
    gameObject.transform.SetParent(this._visual.transform);
    gameObject.transform.localPosition = Vector3.zero;
    gameObject.transform.localScale = (Vector3.one * 0.5f);
    this._nameText = gameObject.AddComponent<TextMesh>();
    this._nameText.fontSize = 24;
    this._nameText.anchor = (TextAnchor) 4;
    this._nameText.color = Color.white;
    this._visual.SetActive(false);
    ((Component) this._nameText).gameObject.SetActive(false);
  }

  private void Update()
  {
    bool flag = (double) ControllerInputPoller.instance.rightControllerIndexFloat > 0.5;
    if ((!((UnityEngine.Object) UtilMenuController.Instance != (UnityEngine.Object) null) ? 0 : (UtilMenuController.Instance.isMenuEnabled ? 1 : 0)) != 0)
    {
      this._visual.SetActive(true);
      ((Renderer) this._line).enabled = true;
      Transform rightHandTransform = GorillaTagger.Instance.rightHandTransform;
      Vector3 direction = ((rightHandTransform.rotation * Quaternion.Euler(45f, -10f, 0.0f)) * Vector3.forward);
      this._line.SetPosition(0, rightHandTransform.position);
      RaycastHit hit;
      VRRig rig;
      if (this.PhysicsRaycast(rightHandTransform.position, direction, out hit, out rig))
      {
        this._visual.transform.position = hit.point;
        this._line.SetPosition(1, hit.point);
        if ((!((UnityEngine.Object) rig != (UnityEngine.Object) null) ? 0 : (!rig.isLocal ? 1 : 0)) == 0)
        {
          this._nameText.text = "";
        }
        else
        {
          this._nameText.text = rig.Creator != null ? rig.Creator.NickName : "Unknown";
          ((Component) this._nameText).transform.LookAt(((Component) Plugin.Ins.camera).transform);
          ((Component) this._nameText).transform.Rotate(0.0f, 180f, 0.0f);
          if ((!flag ? 0 : (!this._wasTriggerDown ? 1 : 0)) != 0)
          {
            Sounds.PlaySound(Sounds.shootSfx, 0.5f);
            HapticEngine.Play(HapticPreset.GunShot, false);
            Action<VRRig> callback = this.Callback;
            if (callback != null)
              callback(rig);
          }
        }
      }
      else
      {
        Vector3 vector3 = (rightHandTransform.position + (direction * 10f));
        this._visual.transform.position = vector3;
        this._line.SetPosition(1, vector3);
        this._nameText.text = "";
      }
    }
    else
    {
      this._visual.SetActive(false);
      ((Renderer) this._line).enabled = false;
    }
    this._wasTriggerDown = flag;
  }

  private bool PhysicsRaycast(
    Vector3 origin,
    Vector3 direction,
    out RaycastHit hit,
    out VRRig rig)
  {
    RaycastHit[] raycastHitArray = Physics.RaycastAll(origin, direction, 1000f);
    rig = (VRRig) null;
    hit = new RaycastHit();
    float num = float.MaxValue;
    foreach (RaycastHit raycastHit in raycastHitArray)
    {
      if (((1 << ((Component) raycastHit.collider).gameObject.layer & (GTPlayer.Instance.locomotionEnabledLayers)) != 0 ? 1 : (!((UnityEngine.Object) ((Component) raycastHit.collider).GetComponentInParent<VRRig>() != (UnityEngine.Object) null) ? 0 : (!((Component) raycastHit.collider).GetComponentInParent<VRRig>().isLocal ? 1 : 0))) != 0 && (double) raycastHit.distance < (double) num)
      {
        num = raycastHit.distance;
        hit = raycastHit;
        rig = ((Component) raycastHit.collider).GetComponentInParent<VRRig>();
      }
    }
    return ((UnityEngine.Object) hit.collider != (UnityEngine.Object) null);
  }

  private void OnDestroy()
  {
    if (!((UnityEngine.Object) this._visual))
      return;
    UnityEngine.Object.Destroy((UnityEngine.Object) this._visual);
  }
}
