using SakuraaCastingMod.Core;
using SakuraaCastingMod.Desktop.Ui.Framework;
using SakuraaCastingMod.Features.Replay.compatability;
using SakuraaCastingMod.Features.Replay.CustomCamera;
using SakuraaCastingMod.Features.Replay.UI;
using System;
using System.Collections;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Features.Replay;

public class ReplaySystemBase : MonoBehaviour
{
  public static GorillaController gorillaController;
  private UiManager uiManager;
  private CameraController cameraController;
  public static ReplaySystemBase Instance;

  internal static UiManager Ui
  {
    get
    {
      return !((UnityEngine.Object) ReplaySystemBase.Instance != (UnityEngine.Object) null) ? (UiManager) null : ReplaySystemBase.Instance.uiManager;
    }
  }

  private void Awake()
  {
    ReplaySystemBase.Instance = this;
    this.StartCoroutine(this.DelayTheStart());
  }

  public IEnumerator DelayTheStart()
  {
    yield return (object) new WaitUntil((Func<bool>) (() => ((UnityEngine.Object) GorillaTagger.Instance != (UnityEngine.Object) null) && ((UnityEngine.Object) Plugin.Ins != (UnityEngine.Object) null) && ((UnityEngine.Object) Plugin.Ins.camera != (UnityEngine.Object) null) && ((UnityEngine.Object) VRRig.LocalRig != (UnityEngine.Object) null) && ((UnityEngine.Object) VRRig.LocalRig.mainSkin != (UnityEngine.Object) null)));
    this.LateStart();
  }

  public void LateStart()
  {
    Constants.darkFur = ((Renderer) VRRig.LocalRig.mainSkin).materials[0];
    Camera camera = Plugin.Ins.camera;
    CameraController.CullingMask = camera.cullingMask;
    CameraController.shoulderCam = camera;
    ReplaySystemBase.gorillaController = new GorillaController();
    this.cameraController = new CameraController(ReplaySystemBase.gorillaController);
    CameraController.instance = this.cameraController;
    this.uiManager = (UiManager) new ContentUI(ReplaySystemBase.gorillaController);
  }

  private void OnGUI()
  {
    if (this.uiManager != null)
    {
      this.uiManager.OnGUI();
      if (this.uiManager.UiON)
      {
        ReplayBrowserMenu.Draw();
        ReplayVoiceMenu.Draw();
        ReplayToolsMenu.Draw();
        KeyframeEditMenu.Draw();
        TooltipOverlay.Draw();
      }
    }
    ReplayLoadingOverlay.Draw();
  }

  private void FixedUpdate()
  {
    if (ReplaySystemBase.gorillaController == null)
      return;
    ReplaySystemBase.gorillaController.Update();
  }

  private void Update()
  {
    if ((this.cameraController == null ? 1 : (this.uiManager == null ? 1 : 0)) == 0)
    {
      this.cameraController.Update();
      this.uiManager.Update();
      foreach (GorillaRigExposer exposer in GorillaRigExposer.exposers)
        exposer.update();
    }
    else
      UnityEngine.Debug.Log((object) "The camera controller or ui manager is null");
  }

  private void LateUpdate()
  {
    if ((this.uiManager == null ? 0 : (this.uiManager.UiON ? 1 : 0)) == 0)
      return;
    Cursor.lockState = (CursorLockMode) 0;
    Cursor.visible = true;
  }
}
