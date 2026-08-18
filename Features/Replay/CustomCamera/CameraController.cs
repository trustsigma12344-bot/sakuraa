using SakuraaCastingMod.Features.Replay.EditJson;
using SakuraaCastingMod.Features.Replay.ReplayManagers;
using SakuraaCastingMod.Features.Replay.utils;
using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

#nullable disable
namespace SakuraaCastingMod.Features.Replay.CustomCamera;

internal class CameraController
{
  public static int CullingMask;
  public static bool UseKeyCustomCamera;
  public static CameraController instance;
  public static Camera shoulderCam;
  private GorillaController gorillaController;
  public Camera camera;

  public ReplayProject replayProject => ReplayManager.replayProject;

  private float currentTime => this.gorillaController.GetCurrentRealTime();

  public CameraController(GorillaController gorillaController)
  {
    this.camera = new GameObject("CameraHolder").AddComponent<Camera>();
    CameraExtensions.GetUniversalAdditionalCameraData(this.camera).allowXRRendering = false;
    this.camera.cullingMask = CameraController.CullingMask & -524289;
    this.camera.clearFlags = (CameraClearFlags) 1;
    this.camera.fieldOfView = 100f;
    this.camera.farClipPlane = 1000f;
    this.camera.nearClipPlane = 0.01f;
    this.camera.depth = 2f;
    this.gorillaController = gorillaController;
    ((Component) this.camera).gameObject.SetActive(false);
  }

  public void Update()
  {
    if (!CameraController.UseKeyCustomCamera)
    {
      if (!((Component) this.camera).gameObject.activeSelf)
        return;
      ((Component) this.camera).gameObject.SetActive(false);
    }
    else
    {
      if (!((Component) this.camera).gameObject.activeSelf)
        ((Component) this.camera).gameObject.SetActive(true);
      CameraKeyFrame currentKeyFrame = this.GetCurrentKeyFrame(this.currentTime);
      if (currentKeyFrame == null)
      {
        ((Component) this.camera).transform.position = new Vector3(-64.5f, 12.745f, -82.168f);
        ((Component) this.camera).transform.rotation = new Quaternion(0.0f, -1f, 0.0f, 0.0f);
      }
      else
      {
        CameraKeyFrame nextKeyFrame = this.GetNextKeyFrame(currentKeyFrame.Time);
        if (nextKeyFrame != null)
        {
          this.camera.fieldOfView = currentKeyFrame.FOV;
          this.camera.nearClipPlane = currentKeyFrame.NearClip;
          this.camera.farClipPlane = currentKeyFrame.FarClip;
          ((Component) this.camera).transform.position = this.GetInterpolatedPos(currentKeyFrame, nextKeyFrame, (CameraKeyFrame) null);
          ((Component) this.camera).transform.eulerAngles = this.GetInterpolatedRot(currentKeyFrame, nextKeyFrame);
        }
        else
        {
          ((Component) this.camera).transform.position = new Vector3(currentKeyFrame.PosX, currentKeyFrame.PosY, currentKeyFrame.PosZ);
          ((Component) this.camera).transform.eulerAngles = new Vector3(currentKeyFrame.RotX, currentKeyFrame.RotY, currentKeyFrame.RotZ);
        }
      }
    }
  }

  public Vector3 GetInterpolatedRot(
    CameraKeyFrame CurrentCamera,
    CameraKeyFrame NextCamera,
    float percentDone = -1f,
    bool BlockSmoothing = false)
  {
    Vector3 current;
    // ISSUE: explicit constructor call
    current = new Vector3(CurrentCamera.RotX, CurrentCamera.RotY, CurrentCamera.RotZ);
    Vector3 next;
    // ISSUE: explicit constructor call
    next = new Vector3(NextCamera.RotX, NextCamera.RotY, NextCamera.RotZ);
    float percentDone1 = (float) (((double) this.currentTime - (double) CurrentCamera.Time) / ((double) NextCamera.Time - (double) CurrentCamera.Time));
    if ((double) percentDone > -0.5)
      percentDone1 = percentDone;
    Vector3 endRotation = Vector3.zero;
    Vector3 zero = Vector3.zero;
    Quaternion quaternion;
    if (CurrentCamera.CameraRotationType == 1)
      endRotation = this.GetInterpolatedRotationLiner(next, current, percentDone1);
    else if (CurrentCamera.CameraRotationType != 2)
    {
      if (CurrentCamera.CameraRotationType == 3)
      {
        Quaternion rotation = this.gorillaController.GetGorillaFromID(CurrentCamera.TargetPlayerID).gorillaRig.Head.transform.rotation;
        endRotation = rotation.eulerAngles;
      }
    }
    else
    {
      Gorilla gorillaFromId = this.gorillaController.GetGorillaFromID(CurrentCamera.TargetPlayerID);
      if (gorillaFromId != null)
      {
        quaternion = Quaternion.LookRotation((gorillaFromId.gorillaRig.Head.transform.position - ((Component) this.camera).transform.position), Vector3.up);
        endRotation = quaternion.eulerAngles;
      }
      else
        endRotation = current;
    }
    Vector3 interpolatedRot;
    if ((!CurrentCamera.cameraSmoothingSettings.UseSmoothing ? 0 : (!BlockSmoothing ? 1 : 0)) == 0)
    {
      interpolatedRot = endRotation;
    }
    else
    {
      quaternion = ((Component) this.camera).transform.rotation;
      interpolatedRot = this.CustomSlerpXYZ(quaternion.eulerAngles, endRotation, Mathf.Clamp(Time.deltaTime * 10f, 0.0f, 1f));
    }
    return interpolatedRot;
  }

  public float GetCameraPathDistance(CameraKeyFrame CurrentCamera, CameraKeyFrame NextCamera)
  {
    Vector3 vector3_1;
    // ISSUE: explicit constructor call
    vector3_1 = new Vector3(CurrentCamera.PosX, CurrentCamera.PosY, CurrentCamera.PosZ);
    Vector3 vector3_2;
    // ISSUE: explicit constructor call
    vector3_2 = new Vector3(NextCamera.PosX, NextCamera.PosY, NextCamera.PosZ);
    float cameraPathDistance;
    if (CurrentCamera.CameraMovmentType == 0)
      cameraPathDistance = float.PositiveInfinity;
    else if (CurrentCamera.CameraMovmentType == 1)
      cameraPathDistance = Vector3.Distance(vector3_1, vector3_2);
    else if (CurrentCamera.CameraMovmentType == 2)
    {
      float num1 = Math.Abs(vector3_2.x - vector3_1.x);
      float num2 = Math.Abs(vector3_2.z - vector3_1.z);
      cameraPathDistance = (float) (Math.PI * ((double) num1 + (double) num2) * (3.0 * (Math.Pow((double) num1 - (double) num2, 2.0) / (Math.Pow((double) num1 + (double) num2, 2.0) * (Math.Sqrt(-3.0 * (Math.Pow((double) num1 - (double) num2, 2.0) / Math.Pow((double) num1 + (double) num2, 2.0)) + 4.0) + 10.0))) + 1.0)) / 4f;
    }
    else
      cameraPathDistance = 0.0f;
    return cameraPathDistance;
  }

  public Vector3 GetInterpolatedPos(
    CameraKeyFrame CurrentCamera,
    CameraKeyFrame NextCamera,
    CameraKeyFrame LastCamera,
    float percentDone = -1f,
    bool BlockSmoothing = false)
  {
    Vector3 current;
    // ISSUE: explicit constructor call
    current = new Vector3(CurrentCamera.PosX, CurrentCamera.PosY, CurrentCamera.PosZ);
    Vector3 next;
    // ISSUE: explicit constructor call
    next = new Vector3(NextCamera.PosX, NextCamera.PosY, NextCamera.PosZ);
    float percentDone1 = (float) (((double) this.currentTime - (double) CurrentCamera.Time) / ((double) NextCamera.Time - (double) CurrentCamera.Time));
    if ((double) percentDone > -0.5)
      percentDone1 = percentDone;
    Vector3 vector3_1 = Vector3.zero;
    Vector3 interpolatedPos = Vector3.zero;
    if (CurrentCamera.CameraMovmentType == 0)
      vector3_1 = current;
    else if (CurrentCamera.CameraMovmentType == 1)
      vector3_1 = this.GetInterpolatedPositionLiner(next, current, percentDone1);
    else if (CurrentCamera.CameraMovmentType != 2)
    {
      if (CurrentCamera.CameraMovmentType == 3)
      {
        Gorilla gorillaFromId = this.gorillaController.GetGorillaFromID(CurrentCamera.TargetPlayerID);
        if (gorillaFromId != null)
        {
          Vector3 vector3_2;
          // ISSUE: explicit constructor call
          vector3_2 = new Vector3(CurrentCamera.firstPersonSettings.OffsetX, CurrentCamera.firstPersonSettings.OffsetY, CurrentCamera.firstPersonSettings.OffsetZ);
          Vector3 vector3_3 = (gorillaFromId.gorillaRig.Head.transform.rotation * vector3_2);
          vector3_1 = (gorillaFromId.gorillaRig.Head.transform.position + vector3_3);
        }
      }
    }
    else
      vector3_1 = this.GetInterpolatedPositionCurve(next, current, percentDone1, CurrentCamera.cameraMoventCurveSettings.UseMainCurve, CurrentCamera.cameraMoventCurveSettings.CurveY);
    if ((!CurrentCamera.cameraSmoothingSettings.UseSmoothing ? 0 : (!BlockSmoothing ? 1 : 0)) != 0)
    {
      float num1 = ((Component) this.camera).transform.position.x * CurrentCamera.cameraSmoothingSettings.SmoothingLevel;
      float num2 = ((Component) this.camera).transform.position.y * CurrentCamera.cameraSmoothingSettings.SmoothingLevel;
      float num3 = ((Component) this.camera).transform.position.z * CurrentCamera.cameraSmoothingSettings.SmoothingLevel;
      float num4 = num1 + vector3_1.x;
      float num5 = num2 + vector3_1.y;
      float num6 = num3 + vector3_1.z;
      // ISSUE: explicit constructor call
      interpolatedPos = new Vector3(num4 / (CurrentCamera.cameraSmoothingSettings.SmoothingLevel + 1f), num5 / (CurrentCamera.cameraSmoothingSettings.SmoothingLevel + 1f), num6 / (CurrentCamera.cameraSmoothingSettings.SmoothingLevel + 1f));
    }
    else
      interpolatedPos = vector3_1;
    return interpolatedPos;
  }

  public CameraKeyFrame GetPreviousKeyFrame(float CurrentTime)
  {
    float num = -1f;
    int index1 = 0;
    for (int index2 = 0; index2 < this.replayProject.CameraKeyFrames.Count; ++index2)
    {
      if (((double) this.replayProject.CameraKeyFrames[index2].Time >= (double) CurrentTime ? 0 : ((double) this.replayProject.CameraKeyFrames[index2].Time > (double) num ? 1 : 0)) != 0)
      {
        num = this.replayProject.CameraKeyFrames[index2].Time;
        index1 = index2;
      }
    }
    return (double) num != -1.0 ? this.replayProject.CameraKeyFrames[index1] : (CameraKeyFrame) null;
  }

  private CameraKeyFrame GetCurrentKeyFrame(float CurrentTime)
  {
    float num = -1f;
    int index1 = 0;
    for (int index2 = 0; index2 < this.replayProject.CameraKeyFrames.Count; ++index2)
    {
      if (((double) this.replayProject.CameraKeyFrames[index2].Time >= (double) CurrentTime ? 0 : ((double) this.replayProject.CameraKeyFrames[index2].Time > (double) num ? 1 : 0)) != 0)
      {
        num = this.replayProject.CameraKeyFrames[index2].Time;
        index1 = index2;
      }
    }
    return (double) num == -1.0 ? (CameraKeyFrame) null : this.replayProject.CameraKeyFrames[index1];
  }

  public CameraKeyFrame GetNextKeyFrame(float CurrentTime)
  {
    float num = float.PositiveInfinity;
    int index1 = 0;
    for (int index2 = 0; index2 < this.replayProject.CameraKeyFrames.Count; ++index2)
    {
      if (((double) this.replayProject.CameraKeyFrames[index2].Time >= (double) num ? 0 : ((double) this.replayProject.CameraKeyFrames[index2].Time > (double) CurrentTime ? 1 : 0)) != 0)
      {
        num = this.replayProject.CameraKeyFrames[index2].Time;
        index1 = index2;
      }
    }
    return (double) num != double.PositiveInfinity ? this.replayProject.CameraKeyFrames[index1] : (CameraKeyFrame) null;
  }

  public void AddCamera()
  {
    ReplayHistory.Record();
    CameraKeyFrame cameraKeyFrame = new CameraKeyFrame();
    cameraKeyFrame.Time = this.currentTime;
    Camera shoulderCam = CameraController.shoulderCam;
    cameraKeyFrame.ID = RandomUtils.GenerateRandomString(25);
    cameraKeyFrame.PosX = ((Component) shoulderCam).gameObject.transform.position.x;
    cameraKeyFrame.PosY = ((Component) shoulderCam).gameObject.transform.position.y;
    cameraKeyFrame.PosZ = ((Component) shoulderCam).gameObject.transform.position.z;
    cameraKeyFrame.RotX = ((Component) shoulderCam).gameObject.transform.eulerAngles.x;
    cameraKeyFrame.RotY = ((Component) shoulderCam).gameObject.transform.eulerAngles.y;
    cameraKeyFrame.RotZ = ((Component) shoulderCam).gameObject.transform.eulerAngles.z;
    cameraKeyFrame.CameraMovmentType = 1;
    cameraKeyFrame.CameraRotationType = 1;
    cameraKeyFrame.FOV = 90f;
    cameraKeyFrame.TargetPlayerID = 1;
    cameraKeyFrame.NearClip = 0.01f;
    cameraKeyFrame.FarClip = 10000f;
    cameraKeyFrame.cameraMoventCurveSettings = new CameraMoventCurveSettings()
    {
      UseMainCurve = true,
      UseMainCurveY = true,
      CurveY = false
    };
    cameraKeyFrame.cameraSmoothingSettings = new CameraSmoothingSettings()
    {
      UseSmoothing = false,
      SmoothingLevel = 20f
    };
    cameraKeyFrame.firstPersonSettings = new FirstPersonSettings();
    this.replayProject.CameraKeyFrames.Add(cameraKeyFrame);
  }

  public void DeleteCamera(int Index)
  {
    ReplayHistory.Record();
    this.replayProject.CameraKeyFrames.RemoveAt(Index);
  }

  public void DeleteCamera(CameraKeyFrame cameraKeyFrame)
  {
    ReplayHistory.Record();
    this.replayProject.CameraKeyFrames.Remove(cameraKeyFrame);
  }

  private Vector3 GetInterpolatedPositionLiner(Vector3 next, Vector3 current, float percentDone)
  {
    return Vector3.Lerp(current, next, percentDone);
  }

  private Vector3 GetInterpolatedRotationLiner(Vector3 next, Vector3 current, float percentDone)
  {
    return this.CustomSlerpXYZ(current, next, percentDone);
  }

  private Vector3 GetInterpolatedPositionCurve(
    Vector3 next,
    Vector3 current,
    float percentDone,
    bool AltCurve,
    bool CurveY)
  {
    float num1 = next.z - current.z;
    float num2 = !AltCurve ? next.x - current.x : next.x - current.x;
    float num3 = Math.Abs(num1) / Math.Abs(num2);
    float num4 = 0.0f;
    float num5 = 0.0f;
    if (((double) Math.Abs(num1) != (double) num1 ? 0 : ((double) Mathf.Abs(num2) == (double) num2 ? 1 : 0)) != 0)
    {
      num4 = !AltCurve ? 90f : 270f;
      num5 = 2f;
    }
    else if (((double) Math.Abs(num1) == (double) num1 ? 0 : ((double) Mathf.Abs(num2) != (double) num2 ? 1 : 0)) == 0)
    {
      if (((double) Math.Abs(num1) != (double) num1 ? 0 : ((double) Mathf.Abs(num2) != (double) num2 ? 1 : 0)) == 0)
      {
        if (((double) Math.Abs(num1) == (double) num1 ? 0 : ((double) Mathf.Abs(num2) == (double) num2 ? 1 : 0)) != 0)
        {
          num4 = !AltCurve ? 0.0f : 180f;
          num5 = 3f;
        }
      }
      else
      {
        num4 = !AltCurve ? 0.0f : 180f;
        num5 = 1f;
      }
    }
    else
    {
      num4 = !AltCurve ? 90f : 270f;
      num5 = 4f;
    }
    float num6 = num4 + percentDone * 90f;
    if (((double) num4 == 90.0 || (double) num4 == 0.0 ? ((double) num5 == 3.0 ? 1 : ((double) num5 == 2.0 ? 1 : 0)) : 0) != 0)
      num6 = num4 + (float) ((1.0 - (double) percentDone) * 90.0);
    if (((double) num4 != 270.0 ? 0 : ((double) num5 == 4.0 ? 1 : 0)) != 0)
      num6 = num4 + (float) ((1.0 - (double) percentDone) * 90.0);
    if (((double) num4 != 180.0 ? 0 : ((double) num5 == 1.0 ? 1 : 0)) != 0)
      num6 = num4 + (float) ((1.0 - (double) percentDone) * 90.0);
    float num7 = (float) Math.Cos(Math.PI / 180.0 * (double) num6) * Math.Abs(num2);
    float num8 = (float) Math.Sin(Math.PI / 180.0 * (double) num6) * Math.Abs(num2) * num3;
    float num9 = current.x + num2 + num7;
    if (((double) num4 == 270.0 || (double) num4 == 0.0 ? ((double) num5 == 3.0 || (double) num5 == 2.0 ? 1 : ((double) num5 == 1.0 ? 1 : 0)) : 0) != 0)
      num9 = current.x + num7;
    if (((double) num4 != 90.0 ? 0 : ((double) num5 == 4.0 ? 1 : 0)) != 0)
      num9 = current.x + num7;
    if (((double) num4 != 0.0 ? 0 : ((double) num5 == 1.0 ? 1 : 0)) != 0)
      num9 = current.x + num2 + num7;
    if (((double) num4 != 180.0 ? 0 : ((double) num5 == 1.0 ? 1 : 0)) != 0)
      num9 = current.x + num7;
    float num10 = current.z + num8;
    if (((double) num4 == 270.0 || (double) num4 == 0.0 ? ((double) num5 == 3.0 ? 1 : ((double) num5 == 2.0 ? 1 : 0)) : 0) != 0)
      num10 = current.z + num1 + num8;
    if (((double) num4 != 90.0 ? 0 : ((double) num5 == 4.0 ? 1 : 0)) != 0)
      num10 = current.z + num1 + num8;
    if (((double) num4 != 180.0 ? 0 : ((double) num5 == 1.0 ? 1 : 0)) != 0)
      num10 = current.z + num1 + num8;
    float num11 = (next.y - current.y) * percentDone + current.y;
    return new Vector3(num9, num11, num10);
  }

  private Vector3 CustomSlerpXYZ(Vector3 startRotation, Vector3 endRotation, float t)
  {
    for (int index = 0; index < 3; ++index)
    {
      if ((double) endRotation[index] - (double) startRotation[index] <= 180.0)
      {
        if ((double) endRotation[index] - (double) startRotation[index] < -180.0)
        {
          ref Vector3 local = ref endRotation;
          int num = index;
          local[num] = local[num] + 360f;
        }
      }
      else
      {
        ref Vector3 local = ref endRotation;
        int num = index;
        local[num] = local[num] - 360f;
      }
    }
    Vector3 vector3;
    // ISSUE: explicit constructor call
    vector3 = new Vector3(Mathf.Lerp(startRotation.x, endRotation.x, t), Mathf.Lerp(startRotation.y, endRotation.y, t), Mathf.Lerp(startRotation.z, endRotation.z, t));
    return vector3;
  }
}
