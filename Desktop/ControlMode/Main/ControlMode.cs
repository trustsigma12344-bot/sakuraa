using SakuraaCastingMod.Core;
using SakuraaCastingMod.Desktop.ControlMode.Animators;
using SakuraaCastingMod.Desktop.ControlMode.Menus;
using SakuraaCastingMod.Desktop.ControlMode.Rigging;
using SakuraaCastingMod.Features.Overlays;
using SakuraaCastingMod.Shared.Helpers;
using UnityEngine;
using UnityEngine.XR;

#nullable disable
namespace SakuraaCastingMod.Desktop.ControlMode.Main;

public class ControlMode : MonoBehaviour
{
  public static SakuraaCastingMod.Desktop.ControlMode.Main.ControlMode Instance;
  private static bool _enabled;
  public AnimatorBase walkAnimator;
  public AnimatorBase flyAnimator;
  public AnimatorBase handAnimator;
  public ControlModeGUI fakeComputerController;
  private ControlModeInputHandler _inputHandler;

  private void Update()
  {
    if (Plugin.Ins.currentCameraMode == 3)
    {
      if (XRSettings.isDeviceActive)
      {
        Notification.Send("You cannot use control mode with a VR connected", Color.red);
        this.Enabled = false;
      }
      else
      {
        if (!this.Enabled)
          Notification.Send("Use at your own risk in public lobbies", Color.orange);
        this.Enabled = true;
      }
    }
    else
      this.Enabled = false;
  }

  public bool Enabled
  {
    get => SakuraaCastingMod.Desktop.ControlMode.Main.ControlMode._enabled;
    set
    {
      if (SakuraaCastingMod.Desktop.ControlMode.Main.ControlMode._enabled == value)
        return;
      SakuraaCastingMod.Desktop.ControlMode.Main.ControlMode._enabled = value;
      if (value)
      {
        ControlModeInputHandler.Pitch = 0.0f;
        if (((UnityEngine.Object) this._inputHandler != (UnityEngine.Object) null))
          ((Behaviour) this._inputHandler).enabled = true;
        ((Component) this).gameObject.GetOrAddComponent<WalkingModeRig>();
        this.walkAnimator = (AnimatorBase) ((Component) this).gameObject.GetOrAddComponent<WalkAnimator>();
        this.flyAnimator = (AnimatorBase) ((Component) this).gameObject.GetOrAddComponent<FlyAnimator>();
        this.handAnimator = (AnimatorBase) ((Component) this).gameObject.GetOrAddComponent<PoseAnimator>();
        this.fakeComputerController = ((Component) this).gameObject.GetOrAddComponent<ControlModeGUI>();
        if (!((UnityEngine.Object) WalkingModeRig.Instance != (UnityEngine.Object) null))
          return;
        WalkingModeRig.Instance.Animator = this.walkAnimator;
      }
      else
      {
        if (((UnityEngine.Object) this._inputHandler != (UnityEngine.Object) null))
          ((Behaviour) this._inputHandler).enabled = false;
        if (((UnityEngine.Object) WalkingModeRig.Instance != (UnityEngine.Object) null))
          ((Component) WalkingModeRig.Instance).Obliterate();
        if (((UnityEngine.Object) this.walkAnimator != (UnityEngine.Object) null))
          ((Component) this.walkAnimator).Obliterate();
        if (((UnityEngine.Object) this.flyAnimator != (UnityEngine.Object) null))
          ((Component) this.flyAnimator).Obliterate();
        if (((UnityEngine.Object) this.handAnimator != (UnityEngine.Object) null))
          ((Component) this.handAnimator).Obliterate();
        if (!((UnityEngine.Object) this.fakeComputerController != (UnityEngine.Object) null))
          return;
        ((Component) this.fakeComputerController).Obliterate();
      }
    }
  }

  private void Awake()
  {
    SakuraaCastingMod.Desktop.ControlMode.Main.ControlMode.Instance = this;
    this._inputHandler = ((Component) this).gameObject.GetOrAddComponent<ControlModeInputHandler>();
    ((Behaviour) this._inputHandler).enabled = false;
  }

  public void FUpdateControlMode()
  {
    if (XRSettings.isDeviceActive)
    {
      Plugin.Ins.currentCameraMode = (Plugin.Ins.currentCameraMode + 1) % Plugin.Ins.CameraModes.Length;
      Plugin.Ins.OnModeChange();
    }
    if (((UnityEngine.Object) ((Component) Plugin.Ins.camera).transform.parent != (UnityEngine.Object) Plugin.Ins.originalCameraParent))
    {
      ((Component) Plugin.Ins.camera).transform.parent = Networking.MyRig.headMesh.transform;
      ((Component) Plugin.Ins.camera).transform.localPosition = new Vector3(0.0f, 0.12f, 0.0f);
      ((Component) Plugin.Ins.camera).transform.localRotation = Quaternion.Euler(ControlModeInputHandler.Pitch, 0.0f, 0.0f);
    }
    Plugin.Ins.camera.fieldOfView = ControlModeSettings.Fov;
    Plugin.Ins.camera.nearClipPlane = ControlModeSettings.Clipping;
  }
}
