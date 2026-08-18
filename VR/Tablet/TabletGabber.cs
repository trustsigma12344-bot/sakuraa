using SakuraaCastingMod.Core;
using SakuraaCastingMod.VR.Interaction;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.VR.Tablet;

internal class TabletGabber : MonoBehaviour
{
  private int prevMode;
  private bool wasLocked;

  private Transform RightHandTransform => GorillaTagger.Instance.rightHandTransform;

  private Transform LeftHandTransform => GorillaTagger.Instance.leftHandTransform;

  private void Start()
  {
    ((Component) this).gameObject.layer = 18;
    TabletController.Ins.requiredLayerObjs.Add(((Component) this).gameObject);
  }

  private void Update()
  {
  }

  private void OnTriggerStay(Collider col)
  {
    if ((!((UnityEngine.Object) col).name.Contains("Right") ? 0 : (InputManager.Ins.rightGrip ? 1 : 0)) != 0)
    {
      if (((UnityEngine.Object) Plugin.Ins.tabletObj.transform.parent != (UnityEngine.Object) this.RightHandTransform))
        HapticEngine.Play(HapticPreset.GrabStart, false);
      Plugin.Ins.tabletObj.transform.parent = this.RightHandTransform;
      ((Component) ((Component) Plugin.Ins.camera).transform.GetChild(0)).gameObject.SetActive(false);
      this.prevMode = TabletController.Ins.currentTabletMode;
      Plugin.Ins.currentCameraMode = 1;
      if ((TabletController.Ins.currentTabletMode == 3 ? 1 : (TabletController.Ins.currentTabletMode == 4 ? 1 : 0)) != 0)
        TabletController.Ins.currentTabletMode = 0;
      if (!TabletController.Ins.locked)
      {
        this.wasLocked = false;
      }
      else
      {
        this.wasLocked = true;
        TabletController.Ins.locked = false;
      }
    }
    else if ((!((UnityEngine.Object) col).name.Contains("Left") ? 0 : (InputManager.Ins.leftGrip ? 1 : 0)) != 0)
    {
      if (((UnityEngine.Object) Plugin.Ins.tabletObj.transform.parent != (UnityEngine.Object) this.LeftHandTransform))
        HapticEngine.Play(HapticPreset.GrabStart, true);
      Plugin.Ins.tabletObj.transform.parent = this.LeftHandTransform;
      ((Component) ((Component) Plugin.Ins.camera).transform.GetChild(0)).gameObject.SetActive(false);
      this.prevMode = TabletController.Ins.currentTabletMode;
      Plugin.Ins.currentCameraMode = 1;
      if ((TabletController.Ins.currentTabletMode == 3 ? 1 : (TabletController.Ins.currentTabletMode == 4 ? 1 : 0)) != 0)
        TabletController.Ins.currentTabletMode = 0;
      if (TabletController.Ins.locked)
      {
        this.wasLocked = true;
        TabletController.Ins.locked = false;
      }
      else
        this.wasLocked = false;
    }
    bool flag = ((UnityEngine.Object) Plugin.Ins.tabletObj.transform.parent == (UnityEngine.Object) this.RightHandTransform) && !InputManager.Ins.rightGrip;
    bool isLeftHand = ((UnityEngine.Object) Plugin.Ins.tabletObj.transform.parent == (UnityEngine.Object) this.LeftHandTransform) && !InputManager.Ins.leftGrip;
    if ((flag ? 0 : (!isLeftHand ? 1 : 0)) != 0)
      return;
    if (this.wasLocked)
      TabletController.Ins.locked = true;
    HapticEngine.Play(HapticPreset.GrabRelease, isLeftHand);
    TabletController.Ins.currentTabletMode = this.prevMode;
    Plugin.Ins.tabletObj.transform.parent = (Transform) null;
    Plugin.listenerBool = false;
  }
}
