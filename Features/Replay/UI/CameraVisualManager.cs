using SakuraaCastingMod.Features.Replay.CustomCamera;
using SakuraaCastingMod.Features.Replay.EditJson;
using SakuraaCastingMod.Features.Replay.utils;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Features.Replay.UI;

internal class CameraVisualManager
{
  private Dictionary<string, CameraKeyFrame> oldCameraKeyFrames = new Dictionary<string, CameraKeyFrame>();
  private Dictionary<string, GameObject> fakeCameraVisuals = new Dictionary<string, GameObject>();
  private ReplayProject replayProject;

  public CameraVisualManager(ReplayProject replayProject) => this.replayProject = replayProject;

  public void Update(bool uiOn)
  {
    if (!uiOn)
    {
      this.ClearAllVisuals();
    }
    else
    {
      this.UpdateCameraVisuals();
      this.RemoveDeletedCameraVisuals();
    }
  }

  public void ClearAllVisuals()
  {
    foreach (Object @object in this.fakeCameraVisuals.Values)
      UnityEngine.Object.DestroyImmediate(@object);
    this.oldCameraKeyFrames.Clear();
    this.fakeCameraVisuals.Clear();
  }

  private void UpdateCameraVisuals()
  {
    foreach (CameraKeyFrame cameraKeyFrame in this.replayProject.CameraKeyFrames)
    {
      if (!this.oldCameraKeyFrames.ContainsKey(cameraKeyFrame.ID))
        this.oldCameraKeyFrames.Add(cameraKeyFrame.ID, CameraKeyFrameUtils.MakeNewCopy(cameraKeyFrame));
      if (!this.fakeCameraVisuals.ContainsKey(cameraKeyFrame.ID))
      {
        this.fakeCameraVisuals.Add(cameraKeyFrame.ID, this.CreateCameraVisual(cameraKeyFrame));
        CameraKeyFrame previousKeyFrame = CameraController.instance.GetPreviousKeyFrame(cameraKeyFrame.Time);
        if (previousKeyFrame != null)
          this.ReRenderCameraVisual(previousKeyFrame);
      }
      if (!CameraKeyFrameUtils.AreEqual(this.oldCameraKeyFrames[cameraKeyFrame.ID], cameraKeyFrame))
      {
        UnityEngine.Debug.Log((object) "Rerendering Camera");
        this.oldCameraKeyFrames[cameraKeyFrame.ID] = CameraKeyFrameUtils.MakeNewCopy(cameraKeyFrame);
        UnityEngine.Object.DestroyImmediate((UnityEngine.Object) this.fakeCameraVisuals[cameraKeyFrame.ID]);
        this.fakeCameraVisuals[cameraKeyFrame.ID] = this.CreateCameraVisual(cameraKeyFrame);
      }
    }
  }

  private void RemoveDeletedCameraVisuals()
  {
    List<string> stringList = new List<string>();
    foreach (string key in this.fakeCameraVisuals.Keys)
    {
      bool flag = false;
      foreach (CameraKeyFrame cameraKeyFrame in this.replayProject.CameraKeyFrames)
      {
        if (key == cameraKeyFrame.ID)
        {
          flag = true;
          break;
        }
      }
      if (!flag)
        stringList.Add(key);
    }
    foreach (string key in stringList)
    {
      CameraKeyFrame previousKeyFrame = CameraController.instance.GetPreviousKeyFrame(this.oldCameraKeyFrames[key].Time);
      if (previousKeyFrame != null)
        this.ReRenderCameraVisual(previousKeyFrame);
      UnityEngine.Object.DestroyImmediate((UnityEngine.Object) this.fakeCameraVisuals[key]);
      this.fakeCameraVisuals.Remove(key);
      this.oldCameraKeyFrames.Remove(key);
    }
  }

  private void ReRenderCameraVisual(CameraKeyFrame cameraKey)
  {
    if (!this.fakeCameraVisuals.ContainsKey(cameraKey.ID))
      this.fakeCameraVisuals[cameraKey.ID] = this.CreateCameraVisual(cameraKey);
    UnityEngine.Debug.Log((object) "Rendering Prev Camera");
    UnityEngine.Object.DestroyImmediate((UnityEngine.Object) this.fakeCameraVisuals[cameraKey.ID]);
    this.fakeCameraVisuals[cameraKey.ID] = this.CreateCameraVisual(cameraKey);
  }

  private GameObject CreateCameraVisual(CameraKeyFrame cameraKey)
  {
    GameObject mainCameraPoint = this.CreateMainCameraPoint(cameraKey);
    this.CreateDirectionIndicator(mainCameraPoint);
    this.CreateConnectionLine(mainCameraPoint, cameraKey);
    return mainCameraPoint;
  }

  private GameObject CreateMainCameraPoint(CameraKeyFrame cameraKey)
  {
    GameObject primitive = GameObject.CreatePrimitive((PrimitiveType) 3);
    primitive.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
    ((Renderer) primitive.GetComponent<MeshRenderer>()).material = Constants.darkFur;
    if ((cameraKey.CameraMovmentType == 3 ? 1 : (cameraKey.CameraMovmentType == 4 ? 1 : 0)) == 0)
      ((Renderer) primitive.GetComponent<MeshRenderer>()).material.color = UIConstants.CAMERA_VISUAL_COLOR;
    else
      ((Renderer) primitive.GetComponent<MeshRenderer>()).material.color = UIConstants.CAMERA_ANCHOR_COLOR;
    primitive.transform.position = new Vector3(cameraKey.PosX, cameraKey.PosY, cameraKey.PosZ);
    primitive.transform.rotation = Quaternion.Euler(new Vector3(cameraKey.RotX, cameraKey.RotY, cameraKey.RotZ));
    return primitive;
  }

  private GameObject CreateDirectionIndicator(GameObject parent)
  {
    GameObject primitive = GameObject.CreatePrimitive((PrimitiveType) 0);
    primitive.transform.parent = parent.transform;
    primitive.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
    primitive.transform.localPosition = new Vector3(0.0f, 0.0f, 0.5f);
    ((Renderer) primitive.GetComponent<MeshRenderer>()).material = Constants.darkFur;
    ((Renderer) primitive.GetComponent<MeshRenderer>()).material.color = UIConstants.CAMERA_DIRECTION_COLOR;
    return primitive;
  }

  private void CreateConnectionLine(GameObject mainPoint, CameraKeyFrame cameraKey)
  {
    CameraKeyFrame nextKeyFrame = CameraController.instance.GetNextKeyFrame(cameraKey.Time);
    if ((nextKeyFrame == null || cameraKey.CameraMovmentType == 3 ? 1 : (cameraKey.CameraMovmentType == 4 ? 1 : 0)) != 0)
      return;
    LineRenderer lineRenderer = mainPoint.AddComponent<LineRenderer>();
    this.ConfigureLineRenderer(lineRenderer);
    List<Vector3> pathPoints = this.CalculatePathPoints(cameraKey, nextKeyFrame);
    lineRenderer.positionCount = pathPoints.Count;
    lineRenderer.SetPositions(pathPoints.ToArray());
  }

  private void ConfigureLineRenderer(LineRenderer lineRenderer)
  {
    Material material = new Material(Shader.Find("GorillaTag/UberShader"));
    material.color = Color.white;
    material.SetFloat("_Glossiness", 0.0f);
    ((Renderer) lineRenderer).material = material;
    lineRenderer.startWidth = 0.05f;
  }

  private List<Vector3> CalculatePathPoints(CameraKeyFrame startKey, CameraKeyFrame endKey)
  {
    List<Vector3> pathPoints = new List<Vector3>();
    pathPoints.Add(new Vector3(startKey.PosX, startKey.PosY, startKey.PosZ));
    for (int index = 1; index < 26; ++index)
    {
      if (index != 26)
      {
        float percentDone = (float) index / 26f;
        pathPoints.Add(CameraController.instance.GetInterpolatedPos(startKey, endKey, (CameraKeyFrame) null, percentDone, true));
      }
    }
    pathPoints.Add(new Vector3(endKey.PosX, endKey.PosY, endKey.PosZ));
    return pathPoints;
  }
}
