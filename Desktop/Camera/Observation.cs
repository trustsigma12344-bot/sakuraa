using BepInEx;
using Newtonsoft.Json;
using SakuraaCastingMod.Core;
using SakuraaCastingMod.Desktop.Ui.Framework.MenuItems;
using SakuraaCastingMod.Features.Overlays;
using SakuraaCastingMod.Features.Tools;
using SakuraaCastingMod.Shared.Helpers;
using SakuraaCastingMod.Shared.Models;
using SakuraaCastingMod.VR.Interaction;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.XR;

#nullable disable
namespace SakuraaCastingMod.Desktop.Camera;

public static class Observation
{
  public static bool ShowNests;
  public static readonly List<GameObject> NestList = new List<GameObject>();
  private static GameObject _closestNest;
  private static float _lastSwitchCountDown;
  [SavedSetting("ObservationMinSwitchInterval", 5f)]
  public static float MinSwitchInterval = 5f;
  [SavedSetting("ObservationSmooth", false)]
  public static bool SmoothObservation;
  [SavedSetting("ObservationLerp", 2f)]
  public static float ObservationLerp = 2f;
  [SavedSetting("ObservationSlerp", 3f)]
  public static float ObservationSlerp = 3f;
  [SavedSetting("ObservationVerticalSpeed", 5f)]
  public static float VerticalMoveSpeed = 5f;
  public static float VerticalOffset;
  private static Vector3 _lastTargetNestPos;

  public static void UpdateObservationSpec()
  {
    if (!Networking.InRoom)
    {
      Plugin.Ins.currentCameraMode = (Plugin.Ins.currentCameraMode + 1) % Plugin.Ins.CameraModes.Length;
      Notification.Send("You need to be in a lobby to unlock more modes.", Color.red);
      Plugin.Ins.OnModeChange();
    }
    else if (Observation.NestList.Count < 1)
    {
      Plugin.Ins.currentCameraMode = (Plugin.Ins.currentCameraMode + 1) % Plugin.Ins.CameraModes.Length;
      Notification.Send("You need to have at least 1 nest placed down to use observation spectating. You can place nests in freecam mode!", Color.red);
      Plugin.Ins.OnModeChange();
    }
    else
    {
      if (!TextFieldMenuItem.AnyFocused)
      {
        if (((ButtonControl) Keyboard.current[Keybinds.ObUpKey]).isPressed)
          Observation.VerticalOffset += Observation.VerticalMoveSpeed * Time.deltaTime;
        if (((ButtonControl) Keyboard.current[Keybinds.ObDownKey]).isPressed)
          Observation.VerticalOffset += -1f * Observation.VerticalMoveSpeed * Time.deltaTime;
      }
      if ((XRSettings.isDeviceActive ? 0 : (Gamepad.current != null ? 1 : 0)) != 0)
      {
        float num1 = ((InputControl<float>) Gamepad.current.rightTrigger).ReadValue();
        float num2 = ((InputControl<float>) Gamepad.current.leftTrigger).ReadValue();
        if ((double) num1 > 0.10000000149011612)
          Observation.VerticalOffset += num1 * Observation.VerticalMoveSpeed * Time.deltaTime;
        if ((double) num2 > 0.10000000149011612)
          Observation.VerticalOffset -= num2 * Observation.VerticalMoveSpeed * Time.deltaTime;
      }
      if ((XRSettings.isDeviceActive ? 0 : (((UnityEngine.Object) InputManager.Ins != (UnityEngine.Object) null) ? 1 : 0)) != 0)
      {
        if (InputManager.Ins.rightGrip)
          Observation.VerticalOffset += Observation.VerticalMoveSpeed * Time.deltaTime;
        if (InputManager.Ins.leftGrip)
          Observation.VerticalOffset -= Observation.VerticalMoveSpeed * Time.deltaTime;
      }
      if (!AutoPilot.AutoPilotEnabled)
      {
        if (((ButtonControl) Keyboard.current.digit0Key).wasPressedThisFrame)
          PlayerSpec.SpecPlayer(0);
        if (((ButtonControl) Keyboard.current.digit1Key).wasPressedThisFrame)
          PlayerSpec.SpecPlayer(1);
        if (((ButtonControl) Keyboard.current.digit2Key).wasPressedThisFrame)
          PlayerSpec.SpecPlayer(2);
        if (((ButtonControl) Keyboard.current.digit3Key).wasPressedThisFrame)
          PlayerSpec.SpecPlayer(3);
        if (((ButtonControl) Keyboard.current.digit4Key).wasPressedThisFrame)
          PlayerSpec.SpecPlayer(4);
        if (((ButtonControl) Keyboard.current.digit5Key).wasPressedThisFrame)
          PlayerSpec.SpecPlayer(5);
        if (((ButtonControl) Keyboard.current.digit6Key).wasPressedThisFrame)
          PlayerSpec.SpecPlayer(6);
        if (((ButtonControl) Keyboard.current.digit7Key).wasPressedThisFrame)
          PlayerSpec.SpecPlayer(7);
        if (((ButtonControl) Keyboard.current.digit8Key).wasPressedThisFrame)
          PlayerSpec.SpecPlayer(8);
        if (((ButtonControl) Keyboard.current.digit9Key).wasPressedThisFrame)
          PlayerSpec.SpecPlayer(9);
      }
      else
      {
        AutoPilot.APCooldown -= Time.deltaTime;
        if ((double) AutoPilot.APCooldown <= 0.0)
        {
          AutoPilot.APCooldown = AutoPilot.APCooldownInterval;
          PlayerSpec.SpecPlayer(AutoPilot.GetClosest());
        }
      }
      if (GorillaDataHandler.GorillaDataList[PlayerSpec.SpecGorilla] == null)
        return;
      GorillaData gorillaData = GorillaDataHandler.GorillaDataList[PlayerSpec.SpecGorilla];
      Observation._lastSwitchCountDown -= Time.deltaTime;
      if (((double) Observation._lastSwitchCountDown <= 0.0 ? 1 : (((UnityEngine.Object) Observation._closestNest == (UnityEngine.Object) null) ? 1 : 0)) != 0)
      {
        Observation.UpdateClosestNest(gorillaData);
        Observation._lastSwitchCountDown = Observation.MinSwitchInterval;
      }
      Vector3 position = Observation._closestNest.transform.position;
      if ((position != Observation._lastTargetNestPos))
        Observation.VerticalOffset = 0.0f;
      Observation._lastTargetNestPos = position;
      Vector3 vector3;
      // ISSUE: explicit constructor call
      vector3 = new Vector3(position.x, Observation._closestNest.transform.position.y + Observation.VerticalOffset, position.z);
      UnityEngine.Camera camera = Plugin.Ins.camera;
      if (Observation.SmoothObservation)
      {
        ((Component) camera).transform.position = Vector3.Lerp(((Component) camera).transform.position, vector3, Observation.ObservationLerp * Time.deltaTime);
        ((Component) camera).transform.rotation = Quaternion.Slerp(((Component) camera).transform.rotation, Quaternion.LookRotation((gorillaData.BodyTransform.position - ((Component) camera).transform.position)), Observation.ObservationSlerp * Time.deltaTime);
      }
      else
      {
        ((Component) camera).transform.position = vector3;
        ((Component) camera).transform.LookAt(gorillaData.BodyTransform);
      }
      camera.nearClipPlane = Plugin.clippingPlaneNear;
      bool flag;
      if (((flag = ((ButtonControl) Keybinds.Keyboard[Keybinds.ZoomKey]).isPressed) || XRSettings.isDeviceActive ? 0 : (Gamepad.current != null ? 1 : 0)) != 0)
        flag = Gamepad.current.buttonNorth.isPressed;
      if ((flag || XRSettings.isDeviceActive ? 0 : (((UnityEngine.Object) InputManager.Ins != (UnityEngine.Object) null) ? 1 : 0)) != 0)
        flag = InputManager.Ins.rightPrimaryButton;
      if (flag)
        camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, Plugin.zoomFov, 0.1f * Time.deltaTime * Plugin.Ins.lerpMult);
      else
        camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, Plugin.fov, 0.1f * Time.deltaTime * Plugin.Ins.lerpMult);
    }
  }

  public static void UpdateClosestNest(GorillaData gorilla)
  {
    GameObject gameObject = Observation._closestNest;
    float num1 = float.MaxValue;
    foreach (GameObject nest in Observation.NestList)
    {
      float num2 = Vector3.Distance(gorilla.BodyTransform.position, nest.transform.position);
      if ((double) num2 < (double) num1)
      {
        num1 = num2;
        gameObject = nest;
      }
    }
    Observation._closestNest = gameObject;
  }

  public static void ClearNests()
  {
    foreach (Object nest in Observation.NestList)
      UnityEngine.Object.Destroy(nest);
    Observation.NestList.Clear();
  }

  public static void ToggleNestVisibility()
  {
    Observation.ShowNests = !Observation.ShowNests;
    foreach (GameObject nest in Observation.NestList)
      ((Renderer) nest.GetComponent<MeshRenderer>()).enabled = Observation.ShowNests;
  }

  public static void CreateNest()
  {
    GameObject primitive = GameObject.CreatePrimitive((PrimitiveType) 0);
    Transform transform = ((Component) Plugin.Ins.camera).transform;
    primitive.transform.position = transform.position;
    primitive.transform.rotation = transform.rotation;
    ((Collider) primitive.GetComponent<SphereCollider>()).enabled = false;
    Renderer component = primitive.GetComponent<Renderer>();
    component.material.shader = Shader.Find("GorillaTag/UberShader");
    component.enabled = Observation.ShowNests;
    Observation.NestList.Add(primitive);
  }

  public static void SaveGameObjects(List<GameObject> gameObjects)
  {
    NestSaveModel nestSaveModel = new NestSaveModel();
    foreach (GameObject gameObject in gameObjects)
    {
      GameObjectModel gameObjectModel = new GameObjectModel()
      {
        position = gameObject.transform.position,
        rotation = gameObject.transform.rotation
      };
      nestSaveModel.gameObjectDataList.Add(gameObjectModel);
    }
    string contents = JsonConvert.SerializeObject((object) nestSaveModel);
    string str = Path.Combine(Paths.ConfigPath, "SakuraaCameraClient");
    if (!Directory.Exists(str))
      Directory.CreateDirectory(str);
    File.WriteAllText(Path.Combine(str, "NestsSave.json"), contents);
    Notification.Send("Nests saved to config folder!", Color.green);
  }

  public static void LoadGameObjects()
  {
    string path = Path.Combine(Path.Combine(Paths.ConfigPath, "SakuraaCameraClient"), "NestsSave.json");
    Observation.ClearNests();
    if (File.Exists(path))
    {
      foreach (GameObjectModel gameObjectData in JsonConvert.DeserializeObject<NestSaveModel>(File.ReadAllText(path)).gameObjectDataList)
      {
        GameObject primitive = GameObject.CreatePrimitive((PrimitiveType) 0);
        primitive.transform.position = gameObjectData.position;
        primitive.transform.rotation = gameObjectData.rotation;
        ((Collider) primitive.GetComponent<SphereCollider>()).enabled = false;
        primitive.GetComponent<Renderer>().material.shader = Shader.Find("GorillaTag/UberShader");
        Observation.NestList.Add(primitive);
      }
      Notification.Send("Nests loaded from file :)", Color.green);
    }
    else
      Notification.Send("No save file was found in config folder.", Color.red);
  }
}
