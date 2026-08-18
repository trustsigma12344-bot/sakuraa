using UnityEngine;
using UnityEngine.InputSystem;

#nullable disable
namespace SakuraaCastingMod.VR.Interaction;

public class InputManager : MonoBehaviour
{
  public static InputManager Ins;
  [Header("Configuration")]
  private const float ClickThreshold = 0.4f;
  [Header("Continuous States (Is Holding)")]
  public bool leftGrip;
  public bool rightGrip;
  public bool leftSecondaryButton;
  public bool rightSecondaryButton;
  public bool leftPrimaryButton;
  public bool rightPrimaryButton;
  [Header("Left Hand Triggers")]
  public bool leftGripSingle;
  public bool leftGripDouble;
  public bool leftGripTriple;
  public bool leftSecondaryBtnSingle;
  public bool leftSecondaryBtnDouble;
  public bool leftSecondaryBtnTriple;
  public bool leftPrimaryBtnSingle;
  public bool leftPrimaryBtnDouble;
  public bool leftPrimaryBtnTriple;
  [Header("Right Hand Triggers")]
  public bool rightGripSingle;
  public bool rightGripDouble;
  public bool rightGripTriple;
  public bool rightSecondaryBtnSingle;
  public bool rightSecondaryBtnDouble;
  public bool rightSecondaryBtnTriple;
  public bool rightPrimaryBtnSingle;
  public bool rightPrimaryBtnDouble;
  public bool rightPrimaryBtnTriple;
  [Header("Keyboard & Gamepad")]
  public bool f3Double;
  public Vector2 gpLeftStick;
  public Vector2 gpRightStick;
  private float _lGripTime;
  private float _lSecTime;
  private float _lPrimTime;
  private int _lGripCount;
  private int _lSecCount;
  private int _lPrimCount;
  private float _rGripTime;
  private float _rSecTime;
  private float _rPrimTime;
  private int _rGripCount;
  private int _rSecCount;
  private int _rPrimCount;
  private float _f3Last;

  private void Awake() => InputManager.Ins = this;

  private void Update()
  {
    if (((UnityEngine.Object) ControllerInputPoller.instance == (UnityEngine.Object) null))
      return;
    this.ResetFrameInputs();
    bool leftGrab = ControllerInputPoller.instance.leftGrab;
    if (!this.leftGrip & leftGrab)
      this.HandleClick(ref this._lGripCount, ref this._lGripTime, out this.leftGripSingle, out this.leftGripDouble, out this.leftGripTriple);
    this.leftGrip = leftGrab;
    bool controllerSecondaryButton1 = ControllerInputPoller.instance.leftControllerSecondaryButton;
    if (!this.leftSecondaryButton & controllerSecondaryButton1)
      this.HandleClick(ref this._lSecCount, ref this._lSecTime, out this.leftSecondaryBtnSingle, out this.leftSecondaryBtnDouble, out this.leftSecondaryBtnTriple);
    this.leftSecondaryButton = controllerSecondaryButton1;
    bool controllerPrimaryButton1 = ControllerInputPoller.instance.leftControllerPrimaryButton;
    if (!this.leftPrimaryButton & controllerPrimaryButton1)
      this.HandleClick(ref this._lPrimCount, ref this._lPrimTime, out this.leftPrimaryBtnSingle, out this.leftPrimaryBtnDouble, out this.leftPrimaryBtnTriple);
    this.leftPrimaryButton = controllerPrimaryButton1;
    bool rightGrab = ControllerInputPoller.instance.rightGrab;
    if (!this.rightGrip & rightGrab)
      this.HandleClick(ref this._rGripCount, ref this._rGripTime, out this.rightGripSingle, out this.rightGripDouble, out this.rightGripTriple);
    this.rightGrip = rightGrab;
    bool controllerSecondaryButton2 = ControllerInputPoller.instance.rightControllerSecondaryButton;
    if (!this.rightSecondaryButton & controllerSecondaryButton2)
      this.HandleClick(ref this._rSecCount, ref this._rSecTime, out this.rightSecondaryBtnSingle, out this.rightSecondaryBtnDouble, out this.rightSecondaryBtnTriple);
    this.rightSecondaryButton = controllerSecondaryButton2;
    bool controllerPrimaryButton2 = ControllerInputPoller.instance.rightControllerPrimaryButton;
    if (!this.rightPrimaryButton & controllerPrimaryButton2)
      this.HandleClick(ref this._rPrimCount, ref this._rPrimTime, out this.rightPrimaryBtnSingle, out this.rightPrimaryBtnDouble, out this.rightPrimaryBtnTriple);
    this.rightPrimaryButton = controllerPrimaryButton2;
    if (Gamepad.current != null)
    {
      this.gpLeftStick = ((InputControl<Vector2>) Gamepad.current.leftStick).ReadValue();
      this.gpRightStick = ((InputControl<Vector2>) Gamepad.current.rightStick).ReadValue();
    }
    this.CheckReset(this._lGripTime, ref this._lGripCount);
    this.CheckReset(this._lSecTime, ref this._lSecCount);
    this.CheckReset(this._lPrimTime, ref this._lPrimCount);
    this.CheckReset(this._rGripTime, ref this._rGripCount);
    this.CheckReset(this._rSecTime, ref this._rSecCount);
    this.CheckReset(this._rPrimTime, ref this._rPrimCount);
  }

  private void ResetFrameInputs()
  {
    this.leftGripSingle = false;
    this.leftGripDouble = false;
    this.leftGripTriple = false;
    this.leftSecondaryBtnSingle = false;
    this.leftSecondaryBtnDouble = false;
    this.leftSecondaryBtnTriple = false;
    this.leftPrimaryBtnSingle = false;
    this.leftPrimaryBtnDouble = false;
    this.leftPrimaryBtnTriple = false;
    this.rightGripSingle = false;
    this.rightGripDouble = false;
    this.rightGripTriple = false;
    this.rightSecondaryBtnSingle = false;
    this.rightSecondaryBtnDouble = false;
    this.rightSecondaryBtnTriple = false;
    this.rightPrimaryBtnSingle = false;
    this.rightPrimaryBtnDouble = false;
    this.rightPrimaryBtnTriple = false;
    this.f3Double = false;
  }

  private void HandleClick(
    ref int count,
    ref float lastTime,
    out bool single,
    out bool dbl,
    out bool triple)
  {
    single = false;
    dbl = false;
    triple = false;
    float time = Time.time;
    if ((double) time - (double) lastTime <= 0.40000000596046448)
      ++count;
    else
      count = 1;
    lastTime = time;
    if (count != 1)
    {
      if (count != 2)
      {
        if (count < 3)
          return;
        triple = true;
        count = 0;
      }
      else
        dbl = true;
    }
    else
      single = true;
  }

  private void CheckReset(float lastTime, ref int count)
  {
    if ((count <= 0 ? 0 : ((double) Time.time - (double) lastTime > 0.40000000596046448 ? 1 : 0)) == 0)
      return;
    count = 0;
  }
}
