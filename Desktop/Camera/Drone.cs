using SakuraaCastingMod.Core;
using SakuraaCastingMod.Desktop.Ui;
using SakuraaCastingMod.Features.Overlays;
using SakuraaCastingMod.Features.Tools;
using SakuraaCastingMod.Shared.Helpers;
using SakuraaCastingMod.VR.Interaction;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.XR;

#nullable disable
namespace SakuraaCastingMod.Desktop.Camera;

public static class Drone
{
  public static bool DroneStarted;
  public static bool IsCrashed;
  public static AudioClip DroneSound;
  public static AudioSource DroneAudioSource;
  private static Rigidbody _droneRb;
  public static GameObject DroneBodyObj;
  private const float StickDeadzone = 0.1f;
  private static bool _prevGpSouth;
  private static float _crashShakeTimer;
  private static float _crashShakeIntensity;
  private static AudioSource _crashAudioSource;
  private const float MaxThrust = 26f;
  private const float BasePitchTorque = 1.8f;
  private const float BaseRollTorque = 1.8f;
  private const float BaseYawTorque = 1.2f;
  private const float DroneAngularDrag = 4f;
  private const float MaxSpeed = 70f;
  private const float DragForward = 0.15f;
  private const float DragRight = 0.3f;
  private const float DragUp = 0.6f;
  [SavedSetting("DronePitchRate", 1f)]
  public static float PitchRate = 1f;
  [SavedSetting("DroneRollRate", 1f)]
  public static float RollRate = 1f;
  [SavedSetting("DroneYawRate", 0.2f)]
  public static float YawRate = 0.2f;
  [SavedSetting("DroneExpo", 1f)]
  public static float ExpoAmount = 1f;
  [SavedSetting("DroneStickSens", 1f)]
  public static float StickSensitivity = 1f;
  [SavedSetting("DroneMouseSens", 1f)]
  public static float MouseSensitivity = 1f;
  [SavedSetting("DroneFov", 90f)]
  public static float DroneFov = 90f;
  [SavedSetting("DroneCameraAngle", 20f)]
  public static float CameraAngle = 20f;
  [SavedSetting("DroneVolume", 0.3f)]
  public static float DroneVolume = 0.3f;
  private const float BaseMousePitchSens = 0.04f;
  private const float BaseMouseRollSens = 0.02f;
  private const float KeyboardYawScale = 0.5f;
  private static float _mouseDeltaX;
  private static float _mouseDeltaY;
  private static float _currentThrottle;

  private static float ApplyExpo(float x, float expo) => Mathf.Lerp(x, x * Mathf.Abs(x), expo);

  private static void CheckLockedCursor()
  {
    if (!MainMenus.ShowMainMenu)
    {
      Cursor.lockState = (CursorLockMode) 1;
      Cursor.visible = false;
    }
    else
    {
      Cursor.lockState = (CursorLockMode) 0;
      Cursor.visible = true;
    }
  }

  public static void TriggerCrash(float impactSpeed)
  {
    Drone.IsCrashed = true;
    float intensity = Mathf.Clamp01((float) (((double) impactSpeed - 15.0) / 35.0));
    Drone._crashShakeIntensity = (float) (10.0 + (double) intensity * 20.0);
    Drone._crashShakeTimer = (float) (0.60000002384185791 + (double) intensity * 0.60000002384185791);
    if (((UnityEngine.Object) Drone.DroneAudioSource != (UnityEngine.Object) null))
      Drone.DroneAudioSource.Stop();
    if (((UnityEngine.Object) Drone._crashAudioSource == (UnityEngine.Object) null))
      Drone._crashAudioSource = ((Component) Plugin.Ins.camera).gameObject.AddComponent<AudioSource>();
    Drone._crashAudioSource.clip = Drone.GenerateCrashClip(intensity);
    Drone._crashAudioSource.volume = (float) (0.40000000596046448 + (double) intensity * 0.40000000596046448);
    Drone._crashAudioSource.pitch = (float) (0.800000011920929 - (double) intensity * 0.30000001192092896);
    Drone._crashAudioSource.Play();
    if (((UnityEngine.Object) Drone._droneRb != (UnityEngine.Object) null))
    {
      Drone._droneRb.angularDrag = 0.5f;
      Vector3 vector3 = (UnityEngine.Random.insideUnitSphere * (float) (3.0 + (double) intensity * 8.0));
      Drone._droneRb.AddTorque(vector3, (ForceMode) 1);
    }
    Notification.Send("CRASHED! Press R to respawn.", Color.red);
  }

  private static AudioClip GenerateCrashClip(float intensity)
  {
    int num1 = 44100;
    int length = (int) (44100.0 * (0.800000011920929 + (double) intensity * 0.5));
    float[] numArray = new float[length];
    for (int index = 0; index < length; ++index)
    {
      float num2 = (float) index / (float) num1;
      float num3 = Mathf.Exp((float) (-(double) num2 * (3.0 + (double) intensity * 2.0)));
      float num4 = Mathf.Sin((float) (6.2831854820251465 * (40.0 + (double) intensity * 20.0)) * num2) * 0.6f;
      float num5 = (float) (((double) UnityEngine.Random.value * 2.0 - 1.0) * 0.699999988079071);
      float num6 = (float) ((double) Mathf.Sin(1130.97339f * num2) * (double) Mathf.Sin(43.9823f * num2) * 0.30000001192092896);
      numArray[index] = (num4 + num5 + num6) * num3;
    }
    AudioClip crashClip = AudioClip.Create("DroneCrash", length, 1, num1, false);
    crashClip.SetData(numArray, 0);
    return crashClip;
  }

  private static void StartDroneController()
  {
    UnityEngine.Camera camera = Plugin.Ins.camera;
    Drone.DroneStarted = true;
    Drone.IsCrashed = false;
    Drone._crashShakeTimer = 0.0f;
    Drone.DroneBodyObj = new GameObject("DroneBody");
    Drone.DroneBodyObj.layer = ((Component) camera).gameObject.layer;
    Drone.DroneBodyObj.transform.position = new Vector3(-75f, 12f, -85f);
    Drone._droneRb = Drone.DroneBodyObj.AddComponent<Rigidbody>();
    Drone._droneRb.collisionDetectionMode = (CollisionDetectionMode) 1;
    Drone.DroneBodyObj.AddComponent<DroneCrashHandler>();
    Drone.DroneBodyObj.AddComponent<BoxCollider>().size = new Vector3(0.3f, 0.3f, 0.3f);
    Drone._droneRb.mass = 0.68f;
    if (XRSettings.isDeviceActive)
      Notification.Send("Physics are calculated differently when in VR mode", Color.yellow);
    Drone._droneRb.useGravity = true;
    Drone._droneRb.drag = 0.0f;
    Drone._droneRb.angularDrag = 4f;
    ((Component) camera).transform.SetParent(Drone.DroneBodyObj.transform);
    ((Component) camera).transform.localPosition = Vector3.zero;
    ((Component) camera).transform.localRotation = Quaternion.Euler(-Drone.CameraAngle, 0.0f, 0.0f);
    Drone._mouseDeltaX = 0.0f;
    Drone._mouseDeltaY = 0.0f;
    Drone._currentThrottle = 0.0f;
    Drone.DroneAudioSource = ((Component) camera).gameObject.AddComponent<AudioSource>();
    Drone.DroneAudioSource.clip = Drone.DroneSound;
    Drone.DroneAudioSource.loop = true;
    Drone.DroneAudioSource.volume = 0.0f;
    Drone.DroneAudioSource.Play();
  }

  public static void UpdateDroneMode()
  {
    UnityEngine.Camera camera = Plugin.Ins.camera;
    bool flag1 = false;
    if ((XRSettings.isDeviceActive ? 0 : (Gamepad.current != null ? 1 : 0)) != 0)
    {
      bool isPressed;
      flag1 = (isPressed = Gamepad.current.buttonSouth.isPressed) && !Drone._prevGpSouth;
      Drone._prevGpSouth = isPressed;
    }
    bool flag2 = !XRSettings.isDeviceActive && ((UnityEngine.Object) InputManager.Ins != (UnityEngine.Object) null) && InputManager.Ins.rightPrimaryBtnSingle;
    if (((ButtonControl) Keyboard.current[Keybinds.DrResetKey]).wasPressedThisFrame | flag1 | flag2)
    {
      Plugin.Ins.ResetCameraObject();
      camera.fieldOfView = Drone.DroneFov;
      Cursor.lockState = (CursorLockMode) 1;
      Cursor.visible = false;
      Drone.StartDroneController();
    }
    else if (!Drone.DroneStarted)
    {
      Notification.Send("Press R / A button to spawn drone.", Color.yellow);
    }
    else
    {
      Drone.CheckLockedCursor();
      if ((double) Drone._crashShakeTimer > 0.0)
      {
        Drone._crashShakeTimer -= Time.deltaTime;
        float num = Drone._crashShakeIntensity * (Drone._crashShakeTimer / 1.2f);
        ((Component) Plugin.Ins.camera).transform.localRotation = Quaternion.Euler(-Drone.CameraAngle + UnityEngine.Random.Range(-num, num), UnityEngine.Random.Range(-num, num), UnityEngine.Random.Range((float) (-(double) num * 0.5), num * 0.5f));
      }
      if (Drone.IsCrashed)
        return;
      if ((Mouse.current == null ? 0 : (!MainMenus.ShowMainMenu ? 1 : 0)) != 0)
      {
        Drone._mouseDeltaX += ((InputControl<float>) ((Vector2Control) ((Pointer) Mouse.current).delta).x).ReadValue();
        Drone._mouseDeltaY += ((InputControl<float>) ((Vector2Control) ((Pointer) Mouse.current).delta).y).ReadValue();
      }
      double num1;
      if (!((UnityEngine.Object) Drone._droneRb != (UnityEngine.Object) null))
      {
        num1 = 0.0;
      }
      else
      {
        Vector3 velocity = Drone._droneRb.velocity;
        num1 = (double) ExtraTools.Clamp(velocity.magnitude, 0.0f, 40f);
      }
      float num2 = (float) num1;
      float num3 = 0.6f + Drone._currentThrottle * 0.8f + num2 * 0.015f;
      float num4 = Mathf.Lerp(0.02f, Drone.DroneVolume, Drone._currentThrottle) + num2 * (1f / 500f);
      Drone.DroneAudioSource.pitch = Mathf.Lerp(Drone.DroneAudioSource.pitch, num3, 6f * Time.deltaTime);
      Drone.DroneAudioSource.volume = Mathf.Lerp(Drone.DroneAudioSource.volume, num4, 6f * Time.deltaTime);
    }
  }

  public static void FixedUpdateDronePhysics()
  {
    if ((!Drone.DroneStarted || ((UnityEngine.Object) Drone._droneRb == (UnityEngine.Object) null) ? 1 : (Drone.IsCrashed ? 1 : 0)) != 0)
      return;
    Transform transform = Drone.DroneBodyObj.transform;
    float num1 = 0.0f;
    float num2 = 0.0f;
    float num3 = 0.0f;
    float num4 = 0.0f;
    if (((ButtonControl) Keyboard.current[Keybinds.DrThrottleUpKey]).isPressed)
      num1 += 0.7f;
    if (((ButtonControl) Keyboard.current[Keybinds.DrBoostKey]).isPressed)
      ++num1;
    if (((ButtonControl) Keyboard.current[Keybinds.DrThrottleDownKey]).isPressed)
      num1 -= 0.3f;
    if (((ButtonControl) Keyboard.current[Keybinds.DrDescendKey]).isPressed)
      num1 -= 0.3f;
    float num5 = num4 + (float) (((double) ((InputControl<float>) Keyboard.current[Keybinds.DrYawRightKey]).ReadValue() - (double) ((InputControl<float>) Keyboard.current[Keybinds.DrYawLeftKey]).ReadValue()) * 0.5);
    bool flag = false;
    if ((XRSettings.isDeviceActive ? 0 : (Gamepad.current != null ? 1 : 0)) != 0)
    {
      Vector2 vector2_1 = ((InputControl<Vector2>) Gamepad.current.leftStick).ReadValue();
      Vector2 vector2_2 = ((InputControl<Vector2>) Gamepad.current.rightStick).ReadValue();
      if ((double) Mathf.Abs(vector2_1.y) > 0.10000000149011612)
        num1 += vector2_1.y * Drone.StickSensitivity;
      if ((double) Mathf.Abs(vector2_1.x) > 0.10000000149011612)
        num5 += vector2_1.x * Drone.StickSensitivity;
      if ((double) vector2_2.magnitude > 0.10000000149011612)
      {
        flag = true;
        num2 += vector2_2.y * Drone.StickSensitivity;
        num3 += vector2_2.x * Drone.StickSensitivity;
      }
    }
    if ((XRSettings.isDeviceActive ? 0 : (((UnityEngine.Object) ControllerInputPoller.instance != (UnityEngine.Object) null) ? 1 : 0)) != 0)
    {
      UnityEngine.XR.InputDevice controllerDevice1 = ControllerInputPoller.instance.leftControllerDevice;
      UnityEngine.XR.InputDevice controllerDevice2 = ControllerInputPoller.instance.rightControllerDevice;
      Vector2 vector2_3 = default;
      if ((!controllerDevice1.isValid ? 0 : (controllerDevice1.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis, out vector2_3) ? 1 : 0)) != 0)
      {
        if ((double) Mathf.Abs(vector2_3.y) > 0.10000000149011612)
          num1 += vector2_3.y * Drone.StickSensitivity;
        if ((double) Mathf.Abs(vector2_3.x) > 0.10000000149011612)
          num5 += vector2_3.x * Drone.StickSensitivity;
      }
      Vector2 vector2_4 = default;
      if ((!controllerDevice2.isValid ? 0 : (controllerDevice2.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis, out vector2_4) ? 1 : 0)) != 0 && (double) vector2_4.magnitude > 0.10000000149011612)
      {
        flag = true;
        num2 += vector2_4.y * Drone.StickSensitivity;
        num3 += vector2_4.x * Drone.StickSensitivity;
      }
    }
    float num6 = 0.0f;
    float num7 = 0.0f;
    if (!flag)
    {
      num6 = Drone._mouseDeltaY * 0.04f * Drone.MouseSensitivity * Drone.PitchRate;
      num7 = (float) (-(double) Drone._mouseDeltaX * 0.019999999552965164) * Drone.MouseSensitivity * Drone.RollRate;
    }
    Drone._mouseDeltaX = 0.0f;
    Drone._mouseDeltaY = 0.0f;
    float num8 = Mathf.Clamp(num1, -0.3f, 1f);
    float x1 = Mathf.Clamp(num2, -1f, 1f);
    float x2 = Mathf.Clamp(num3, -1f, 1f);
    float x3 = Mathf.Clamp(num5, -1f, 1f);
    float num9 = Drone.ApplyExpo(x1, Drone.ExpoAmount);
    float num10 = Drone.ApplyExpo(x2, Drone.ExpoAmount);
    float num11 = Drone.ApplyExpo(x3, Drone.ExpoAmount);
    Drone._currentThrottle = Mathf.Clamp01(num8);
    float num12 = Drone._currentThrottle * 26f;
    Drone._droneRb.AddForce((transform.up * num12), (ForceMode) 0);
    Vector3 velocity = Drone._droneRb.velocity;
    float num13 = Mathf.Clamp01(velocity.magnitude / 70f);
    float num14 = (float) (1.0 + (double) num13 * (double) num13 * 25.0);
    float num15 = Vector3.Dot(velocity, transform.forward);
    float num16 = Vector3.Dot(velocity, transform.right);
    float num17 = Vector3.Dot(velocity, transform.up);
    Vector3 vector3_1 = (((((-transform.forward) * num15 * 0.15f) - (transform.right * num16 * 0.3f)) - (transform.up * num17 * 0.6f)) * num14);
    Drone._droneRb.AddForce(vector3_1, (ForceMode) 0);
    Vector3 vector3_2 = (((transform.right * num9 * 1.8f * Drone.PitchRate) + (transform.forward * (float) (-(double) num10 * 1.7999999523162842) * Drone.RollRate)) + (transform.up * num11 * 1.2f * Drone.YawRate));
    Drone._droneRb.AddTorque(vector3_2, (ForceMode) 0);
    if (((double) num6 != 0.0 ? 1 : ((double) num7 != 0.0 ? 1 : 0)) != 0)
      Drone._droneRb.rotation = (Drone._droneRb.rotation * Quaternion.Euler(num6, 0.0f, num7));
    ((Component) Plugin.Ins.camera).transform.localRotation = Quaternion.Euler(-Drone.CameraAngle, 0.0f, 0.0f);
    Plugin.Ins.camera.fieldOfView = Drone.DroneFov;
  }
}
