using Liv.Lck;
using Liv.Lck.Recorder;
using SakuraaCastingMod.Core;
using SakuraaCastingMod.VR.Interaction;
using SakuraaCastingMod.VR.UtilMenu;
using SakuraaCastingMod.VR.UtilMenu.Utility;
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

#nullable disable
namespace SakuraaCastingMod.VR.Tablet;

public class RecordingPanelController : MonoBehaviour
{
  private bool _hasRecorderInitialized = false;
  public TextMeshPro recorderStatusText;
  public GorillaFingerButton btn1;
  public GorillaFingerButton btn2;
  public GameObject openFolderIcon;
  public GameObject yesTextObj;
  public GameObject recordIcon;
  public GameObject stopIcon;
  public GameObject noTextObj;
  private readonly Vector3 _closedPos = new Vector3(0.0f, -0.14f, 0.0125f);
  private readonly Vector3 _midOpenPos = new Vector3(0.0f, -0.28f, 0.0125f);
  private readonly Vector3 _openPos = new Vector3(0.0f, -0.25f, -0.06f);
  private readonly Quaternion _closedRot = Quaternion.Euler(0.0f, 0.0f, 0.0f);
  private readonly Quaternion _openRot = Quaternion.Euler(40f, 0.0f, 0.0f);
  private bool _isOpen;
  private bool _isAnimating;
  private float _animTimer;
  private const float AnimDuration = 0.8f;
  private const float AnimMidPoint = 0.4f;
  private RecordingPanelController.PanelState _currentState = RecordingPanelController.PanelState.Idle;
  private float _lastInteractionTime;
  private LckService _lckService;
  private LckCamera _lckCamera;
  private GameObject _clonedRecorderObj;
  private bool _serviceReady = false;
  private float _recordingStartTime;
  private string _lastSavedPath = "";
  private ScriptableObject _cachedLivFeature = (ScriptableObject) null;

  private void Start()
  {
    this.InitializeUIReferences();
    ((Component) this).transform.localPosition = this._closedPos;
    ((Component) this).transform.localRotation = this._closedRot;
    if (((UnityEngine.Object) this.recorderStatusText != (UnityEngine.Object) null))
      ((TMP_Text) this.recorderStatusText).text = "NOT INITIALIZED";
    ThemeManager.OnThemeApplied += new Action(this.ApplyThemeColors);
    this.ApplyThemeColors();
    ((Component) this).gameObject.SetActive(false);
  }

  private void ApplyThemeColors()
  {
    Color customColor = ThemeManager.GetCustomColor("BUTTON");
    RecordingPanelController.ApplyColorToObj(this.openFolderIcon, customColor);
    RecordingPanelController.ApplyColorToObj(this.yesTextObj, customColor);
    RecordingPanelController.ApplyColorToObj(this.recordIcon, customColor);
    RecordingPanelController.ApplyColorToObj(this.stopIcon, customColor);
    RecordingPanelController.ApplyColorToObj(this.noTextObj, customColor);
  }

  private static void ApplyColorToObj(GameObject obj, Color c)
  {
    if (((UnityEngine.Object) obj == (UnityEngine.Object) null))
      return;
    TextMeshPro component1 = obj.GetComponent<TextMeshPro>();
    if (((UnityEngine.Object) component1 != (UnityEngine.Object) null))
    {
      ((Graphic) component1).color = c;
    }
    else
    {
      Renderer component2 = obj.GetComponent<Renderer>();
      if (!((UnityEngine.Object) component2 != (UnityEngine.Object) null))
        return;
      component2.material.color = c;
    }
  }

  private IEnumerator InitializeRecorderRoutine()
  {
    while ((((UnityEngine.Object) Plugin.Ins == (UnityEngine.Object) null) ? 1 : (((UnityEngine.Object) Plugin.Ins.camera == (UnityEngine.Object) null) ? 1 : 0)) != 0)
      yield return (object) new WaitForSeconds(1f);
    LckResult<LckService> serviceResult;
    for (serviceResult = LckService.GetService(); !serviceResult.Success; serviceResult = LckService.GetService())
      yield return (object) new WaitForSeconds(1f);
    this._lckService = serviceResult.Result;
    CameraResolutionDescriptor resolution = new CameraResolutionDescriptor(1920U, 1080U);
    CameraTrackDescriptor highQuality = new CameraTrackDescriptor(resolution, 20000000U, 60U, 320000U);
    this._lckService.SetTrackDescriptor(highQuality);
    UnityEngine.Debug.Log((object) "RecordingPanel: High Quality Settings Applied (1080p/60FPS/20Mbps).");
    this.DisableLckRenderFeature();
    try
    {
      Camera sourceCam = Plugin.Ins.camera;
      this._clonedRecorderObj = new GameObject("LCK_Ghost_Recorder");
      this._clonedRecorderObj.SetActive(false);
      this._clonedRecorderObj.transform.SetParent(((Component) sourceCam).transform);
      this._clonedRecorderObj.transform.localPosition = Vector3.zero;
      this._clonedRecorderObj.transform.localRotation = Quaternion.identity;
      Camera ghostCam = this._clonedRecorderObj.AddComponent<Camera>();
      ghostCam.CopyFrom(sourceCam);
      ((Behaviour) ghostCam).enabled = false;
      this._lckCamera = this._clonedRecorderObj.AddComponent<LckCamera>();
      typeof (LckCamera).GetField("_camera", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue((object) this._lckCamera, (object) ghostCam);
      this._clonedRecorderObj.SetActive(true);
      sourceCam = (Camera) null;
      ghostCam = (Camera) null;
    }
    catch (Exception ex)
    {
      UnityEngine.Debug.LogError((object) ("[RecordingPanel] Critical Error setup LckCamera: " + ex.Message));
      if (!((UnityEngine.Object) this.recorderStatusText))
        yield break;
      ((TMP_Text) this.recorderStatusText).text = "CAMERA ERROR";
      yield break;
    }
    if (this._lckService != null)
    {
      LckResult micResult = this._lckService.SetMicrophoneCaptureActive(true);
      if (micResult.Success)
      {
        this._lckService.SetMicrophoneGain(1f);
        UnityEngine.Debug.Log((object) "RecordingPanel: Microphone Enabled.");
      }
      this._lckService.OnRecordingSaved -= new Action<LckResult<RecordingData>>(this.OnRecordingSaved);
      this._lckService.OnRecordingSaved += new Action<LckResult<RecordingData>>(this.OnRecordingSaved);
      micResult = (LckResult) null;
    }
    this._serviceReady = true;
    UnityEngine.Debug.Log((object) "RecordingPanel: LIV Recorder Successfully Initialized.");
    if ((!((UnityEngine.Object) this.recorderStatusText != (UnityEngine.Object) null) ? 0 : (this._currentState == RecordingPanelController.PanelState.Idle ? 1 : 0)) != 0)
      ((TMP_Text) this.recorderStatusText).text = "PRESS RECORD\n1080p 60FPS\nNOT RECORDING";
  }

  private void DisableLckRenderFeature()
  {
    try
    {
      RenderPipelineAsset renderPipelineAsset = GraphicsSettings.renderPipelineAsset;
      if (((UnityEngine.Object) renderPipelineAsset == (UnityEngine.Object) null))
        return;
      FieldInfo field1 = renderPipelineAsset.GetType().GetField("m_RendererDataList", BindingFlags.Instance | BindingFlags.NonPublic);
      if (field1 == (FieldInfo) null || !(field1.GetValue((object) renderPipelineAsset) is IList list1))
        return;
      foreach (object obj1 in (IEnumerable) list1)
      {
        if (obj1 != null)
        {
          FieldInfo field2 = obj1.GetType().GetField("m_RendererFeatures", BindingFlags.Instance | BindingFlags.NonPublic);
          if (!(field2 == (FieldInfo) null) && field2.GetValue(obj1) is IList list2)
          {
            foreach (object obj2 in (IEnumerable) list2)
            {
              if (obj2 != null && obj2.GetType().FullName == "Liv.Lck.Rendering.LckCompositionRenderFeature")
              {
                MethodInfo method = obj2.GetType().GetMethod("SetActive", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (!(method != (MethodInfo) null))
                {
                  FieldInfo field3 = obj2.GetType().GetField("m_Active", BindingFlags.Instance | BindingFlags.NonPublic);
                  if (field3 != (FieldInfo) null)
                  {
                    field3.SetValue(obj2, (object) false);
                    this._cachedLivFeature = obj2 as ScriptableObject;
                    UnityEngine.Debug.Log((object) "RecordingPanel: DISABLED LIV COMPOSITION OVERLAY (Via Reflection Field).");
                    return;
                  }
                }
                else
                {
                  method.Invoke(obj2, new object[1]
                  {
                    (object) false
                  });
                  this._cachedLivFeature = obj2 as ScriptableObject;
                  UnityEngine.Debug.Log((object) "RecordingPanel: DISABLED LIV COMPOSITION OVERLAY (Via Reflection Method).");
                  return;
                }
              }
            }
          }
        }
      }
    }
    catch (Exception ex)
    {
      UnityEngine.Debug.LogError((object) $"RecordingPanel: Failed to disable Render Feature via Reflection: {ex}");
    }
  }

  private void ReEnableLckRenderFeature()
  {
    if (!((UnityEngine.Object) this._cachedLivFeature != (UnityEngine.Object) null))
      return;
    try
    {
      MethodInfo method = this._cachedLivFeature.GetType().GetMethod("SetActive", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
      if (!(method != (MethodInfo) null))
        return;
      method.Invoke((object) this._cachedLivFeature, new object[1]
      {
        (object) true
      });
    }
    catch
    {
    }
  }

  private void OnRecordingSaved(LckResult<RecordingData> result)
  {
    if (result.Success)
    {
      this._lastSavedPath = result.Result.RecordingFilePath;
      UnityEngine.Debug.Log((object) ("RecordingPanel: Video saved to " + this._lastSavedPath));
    }
    else
      UnityEngine.Debug.LogError((object) ("RecordingPanel: Save Failed: " + result.Message));
  }

  private void StartRecording()
  {
    if (this._serviceReady)
    {
      if ((this._lckService == null ? 1 : (((UnityEngine.Object) this._lckCamera == (UnityEngine.Object) null) ? 1 : 0)) != 0)
      {
        UnityEngine.Debug.LogError((object) "RecordingPanel: Service or Camera is null.");
      }
      else
      {
        if (((UnityEngine.Object) this._clonedRecorderObj != (UnityEngine.Object) null))
          this._clonedRecorderObj.SetActive(true);
        LckResult lckResult1 = this._lckService.SetActiveCamera(this._lckCamera.CameraId, (string) null);
        if (!lckResult1.Success)
        {
          UnityEngine.Debug.LogError((object) ("RecordingPanel: Failed to set active camera: " + lckResult1.Message));
        }
        else
        {
          this._lckService.SetMicrophoneCaptureActive(true);
          LckResult lckResult2 = this._lckService.StartRecording();
          if (lckResult2.Success)
          {
            this._recordingStartTime = Time.time;
            this._currentState = RecordingPanelController.PanelState.Recording;
          }
          else
            UnityEngine.Debug.LogError((object) ("RecordingPanel: Failed to start recording: " + lckResult2.Message));
        }
      }
    }
    else
    {
      if (!((UnityEngine.Object) this.recorderStatusText))
        return;
      ((TMP_Text) this.recorderStatusText).text = "WAITING FOR\nSERVICE...";
    }
  }

  private void StopRecording()
  {
    if ((!this._serviceReady ? 0 : (this._lckService != null ? 1 : 0)) != 0)
      this._lckService.StopRecording();
    this._currentState = RecordingPanelController.PanelState.Idle;
  }

  private void OpenRecordingFolder()
  {
    string str;
    if ((string.IsNullOrEmpty(this._lastSavedPath) ? 0 : (File.Exists(this._lastSavedPath) ? 1 : 0)) == 0)
    {
      str = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos), "Gorilla Tag");
      if (!Directory.Exists(str))
        Directory.CreateDirectory(str);
    }
    else
      str = Path.GetDirectoryName(this._lastSavedPath);
    if (!Directory.Exists(str))
      return;
    Process.Start("explorer.exe", str);
  }

  private void InitializeUIReferences()
  {
    if (((UnityEngine.Object) this.recorderStatusText == (UnityEngine.Object) null))
    {
      Transform deep = this.FindDeep(((Component) this).transform, "RecorderStatusText");
      if (!((UnityEngine.Object) deep != (UnityEngine.Object) null))
        this.LogMissing("RecorderStatusText");
      else
        this.recorderStatusText = ((Component) deep).GetComponent<TextMeshPro>();
    }
    Transform deep1 = this.FindDeep(((Component) this).transform, "Btn1");
    if (((UnityEngine.Object) deep1 != (UnityEngine.Object) null))
    {
      if (((UnityEngine.Object) this.openFolderIcon == (UnityEngine.Object) null))
        this.openFolderIcon = ((Component) this.FindDeep(deep1, "OpenFolderIcon"))?.gameObject;
      if (((UnityEngine.Object) this.yesTextObj == (UnityEngine.Object) null))
        this.yesTextObj = ((Component) this.FindDeep(deep1, "YesText"))?.gameObject;
      this.btn1 = this.SetupButton(((Component) deep1).gameObject);
      if (((UnityEngine.Object) this.btn1 != (UnityEngine.Object) null))
        this.btn1.onPressed += new Action<GorillaFingerButton, bool>(this.OnBtn1Pressed);
    }
    else
      this.LogMissing("Btn1");
    Transform deep2 = this.FindDeep(((Component) this).transform, "Btn2");
    if (((UnityEngine.Object) deep2 != (UnityEngine.Object) null))
    {
      if (((UnityEngine.Object) this.recordIcon == (UnityEngine.Object) null))
        this.recordIcon = ((Component) this.FindDeep(deep2, "RecordIcon"))?.gameObject;
      if (((UnityEngine.Object) this.stopIcon == (UnityEngine.Object) null))
        this.stopIcon = ((Component) this.FindDeep(deep2, "StopIcon"))?.gameObject;
      if (((UnityEngine.Object) this.noTextObj == (UnityEngine.Object) null))
        this.noTextObj = ((Component) this.FindDeep(deep2, "NoText"))?.gameObject;
      this.btn2 = this.SetupButton(((Component) deep2).gameObject);
      if (!((UnityEngine.Object) this.btn2 != (UnityEngine.Object) null))
        return;
      this.btn2.onPressed += new Action<GorillaFingerButton, bool>(this.OnBtn2Pressed);
    }
    else
      this.LogMissing("Btn2");
  }

  private void OnBtn1Pressed(GorillaFingerButton button, bool isLeftHand)
  {
    if ((this._isAnimating ? 1 : (!this.IsCooldownReady() ? 1 : 0)) != 0)
      return;
    this._lastInteractionTime = Time.time;
    switch (this._currentState)
    {
      case RecordingPanelController.PanelState.Idle:
        this.OpenRecordingFolder();
        break;
      case RecordingPanelController.PanelState.ConfirmStart:
        this.StartRecording();
        HapticEngine.Play(this._currentState == RecordingPanelController.PanelState.Recording ? HapticPreset.RecStart : HapticPreset.Error, isLeftHand);
        HapticEngine.SuppressPressDefault();
        break;
      case RecordingPanelController.PanelState.Recording:
        this.OpenRecordingFolder();
        break;
      case RecordingPanelController.PanelState.ConfirmStop:
        this.StopRecording();
        HapticEngine.Play(HapticPreset.RecStop, isLeftHand);
        HapticEngine.SuppressPressDefault();
        break;
    }
    this.UpdateUIState();
  }

  private void OnBtn2Pressed(GorillaFingerButton button, bool isLeftHand)
  {
    if ((this._isAnimating ? 1 : (!this.IsCooldownReady() ? 1 : 0)) != 0)
      return;
    this._lastInteractionTime = Time.time;
    switch (this._currentState)
    {
      case RecordingPanelController.PanelState.Idle:
        this._currentState = RecordingPanelController.PanelState.ConfirmStart;
        HapticEngine.Play(HapticPreset.ConfirmWarning, isLeftHand);
        HapticEngine.SuppressPressDefault();
        break;
      case RecordingPanelController.PanelState.ConfirmStart:
        this._currentState = RecordingPanelController.PanelState.Idle;
        break;
      case RecordingPanelController.PanelState.Recording:
        this._currentState = RecordingPanelController.PanelState.ConfirmStop;
        HapticEngine.Play(HapticPreset.ConfirmWarning, isLeftHand);
        HapticEngine.SuppressPressDefault();
        break;
      case RecordingPanelController.PanelState.ConfirmStop:
        this._currentState = RecordingPanelController.PanelState.Recording;
        break;
    }
    this.UpdateUIState();
  }

  private void UpdateUIState()
  {
    if (((UnityEngine.Object) this.openFolderIcon))
      this.openFolderIcon.SetActive(false);
    if (((UnityEngine.Object) this.yesTextObj))
      this.yesTextObj.SetActive(false);
    if (((UnityEngine.Object) this.recordIcon))
      this.recordIcon.SetActive(false);
    if (((UnityEngine.Object) this.stopIcon))
      this.stopIcon.SetActive(false);
    if (((UnityEngine.Object) this.noTextObj))
      this.noTextObj.SetActive(false);
    bool flag = true;
    switch (this._currentState)
    {
      case RecordingPanelController.PanelState.Idle:
        if (((UnityEngine.Object) this.openFolderIcon) & flag)
          this.openFolderIcon.SetActive(true);
        if (((UnityEngine.Object) this.recordIcon) & flag)
          this.recordIcon.SetActive(true);
        if (!this._serviceReady)
        {
          if ((!((UnityEngine.Object) this.recorderStatusText != (UnityEngine.Object) null) ? 0 : (((TMP_Text) this.recorderStatusText).text != "INITIALIZING\nRECORDER..." ? 1 : 0)) == 0)
            break;
          ((TMP_Text) this.recorderStatusText).text = "INITIALIZING\nRECORDER...";
          break;
        }
        if (!((UnityEngine.Object) this.recorderStatusText != (UnityEngine.Object) null))
          break;
        ((TMP_Text) this.recorderStatusText).text = "PRESS RECORD\n--:--\nNOT RECORDING";
        break;
      case RecordingPanelController.PanelState.ConfirmStart:
        if (((UnityEngine.Object) this.recorderStatusText != (UnityEngine.Object) null))
          ((TMP_Text) this.recorderStatusText).text = "START RECORDING?";
        if (((UnityEngine.Object) this.yesTextObj))
          this.yesTextObj.SetActive(true);
        if (!((UnityEngine.Object) this.noTextObj))
          break;
        this.noTextObj.SetActive(true);
        break;
      case RecordingPanelController.PanelState.Recording:
        if (((UnityEngine.Object) this.openFolderIcon))
          this.openFolderIcon.SetActive(true);
        if (!((UnityEngine.Object) this.stopIcon))
          break;
        this.stopIcon.SetActive(true);
        break;
      case RecordingPanelController.PanelState.ConfirmStop:
        if (((UnityEngine.Object) this.recorderStatusText != (UnityEngine.Object) null))
          ((TMP_Text) this.recorderStatusText).text = "STOP RECORDING?";
        if (((UnityEngine.Object) this.yesTextObj))
          this.yesTextObj.SetActive(true);
        if (!((UnityEngine.Object) this.noTextObj))
          break;
        this.noTextObj.SetActive(true);
        break;
    }
  }

  private GorillaFingerButton SetupButton(GameObject btnObj)
  {
    btnObj.layer = 18;
    BoxCollider boxCollider = btnObj.GetComponent<BoxCollider>();
    if (((UnityEngine.Object) boxCollider == (UnityEngine.Object) null))
      boxCollider = btnObj.AddComponent<BoxCollider>();
    ((Collider) boxCollider).isTrigger = true;
    GorillaFingerButton gorillaFingerButton = btnObj.GetComponent<GorillaFingerButton>();
    if (((UnityEngine.Object) gorillaFingerButton == (UnityEngine.Object) null))
      gorillaFingerButton = btnObj.AddComponent<GorillaFingerButton>();
    if (((UnityEngine.Object) UtilMenuMain.Instance != (UnityEngine.Object) null))
    {
      gorillaFingerButton.unpressedMaterial = UtilMenuMain.Instance.buttonMat;
      gorillaFingerButton.pressedMaterial = UtilMenuMain.Instance.pressedButtonMat;
    }
    if ((!((UnityEngine.Object) TabletController.Ins != (UnityEngine.Object) null) ? 0 : (!TabletController.Ins.requiredLayerObjs.Contains(btnObj) ? 1 : 0)) != 0)
      TabletController.Ins.requiredLayerObjs.Add(btnObj);
    gorillaFingerButton.debounceTime = 1f;
    return gorillaFingerButton;
  }

  public void TogglePanel()
  {
    if (this._isOpen)
      this.ClosePanel();
    else
      this.OpenPanel();
  }

  public void OpenPanel()
  {
    if ((this._isOpen ? 1 : (this._isAnimating ? 1 : 0)) != 0)
      return;
    if (!this._hasRecorderInitialized)
    {
      if (((UnityEngine.Object) this.recorderStatusText != (UnityEngine.Object) null))
        ((TMP_Text) this.recorderStatusText).text = "INITIALIZING\nRECORDER...";
      if (((UnityEngine.Object) TabletController.Ins != (UnityEngine.Object) null))
        TabletController.Ins.StartCoroutine(this.InitializeRecorderRoutine());
      else if (((UnityEngine.Object) Plugin.Ins != (UnityEngine.Object) null))
        Plugin.Ins.StartCoroutine(this.InitializeRecorderRoutine());
      this._hasRecorderInitialized = true;
    }
    ((Component) this).gameObject.SetActive(true);
    this.SetButtonsInteractable(false);
    this._isOpen = true;
    this._isAnimating = true;
    this._animTimer = 0.0f;
    ((Component) this).transform.localPosition = this._closedPos;
    ((Component) this).transform.localRotation = this._closedRot;
    this.UpdateUIState();
  }

  public void ClosePanel()
  {
    if ((!this._isOpen ? 1 : (this._isAnimating ? 1 : 0)) != 0)
      return;
    this.SetButtonsInteractable(false);
    this._isOpen = false;
    this._isAnimating = true;
    this._animTimer = 0.0f;
  }

  private void SetButtonsInteractable(bool interactable)
  {
    if (((UnityEngine.Object) this.btn1 != (UnityEngine.Object) null))
      ((Component) this.btn1).GetComponent<Collider>().enabled = interactable;
    if (!((UnityEngine.Object) this.btn2 != (UnityEngine.Object) null))
      return;
    ((Component) this.btn2).GetComponent<Collider>().enabled = interactable;
  }

  private bool IsCooldownReady() => (double) Time.time - (double) this._lastInteractionTime >= 1.0;

  private Transform FindDeep(Transform root, string name)
  {
    Transform deep1;
    foreach (Transform root1 in root)
    {
      if (!(((UnityEngine.Object) root1).name == name))
      {
        Transform deep2 = this.FindDeep(root1, name);
        if (((UnityEngine.Object) deep2 != (UnityEngine.Object) null))
        {
          deep1 = deep2;
          goto label_11;
        }
      }
      else
      {
        deep1 = root1;
        goto label_11;
      }
    }
    deep1 = (Transform) null;
label_11:
    return deep1;
  }

  private void LogMissing(string name)
  {
    UnityEngine.Debug.LogError((object) $"RecordingPanel: Could not find '{name}'");
  }

  private void Update()
  {
    if (this._isAnimating)
    {
      this._animTimer += Time.deltaTime;
      if (!this._isOpen)
      {
        if ((double) this._animTimer < 0.40000000596046448)
        {
          float num = this._animTimer / 0.4f;
          ((Component) this).transform.localPosition = Vector3.Lerp(this._openPos, this._midOpenPos, num);
          ((Component) this).transform.localRotation = Quaternion.Lerp(this._openRot, this._closedRot, num);
        }
        else
        {
          ((Component) this).transform.localPosition = Vector3.Lerp(this._midOpenPos, this._closedPos, (float) (((double) this._animTimer - 0.40000000596046448) / 0.40000000596046448));
          ((Component) this).transform.localRotation = this._closedRot;
        }
        if ((double) this._animTimer >= 0.800000011920929)
        {
          this._isAnimating = false;
          ((Component) this).transform.localPosition = this._closedPos;
          ((Component) this).transform.localRotation = this._closedRot;
          ((Component) this).gameObject.SetActive(false);
        }
      }
      else
      {
        if ((double) this._animTimer >= 0.40000000596046448)
        {
          float num = (float) (((double) this._animTimer - 0.40000000596046448) / 0.40000000596046448);
          ((Component) this).transform.localPosition = Vector3.Lerp(this._midOpenPos, this._openPos, num);
          ((Component) this).transform.localRotation = Quaternion.Lerp(this._closedRot, this._openRot, num);
        }
        else
        {
          ((Component) this).transform.localPosition = Vector3.Lerp(this._closedPos, this._midOpenPos, this._animTimer / 0.4f);
          ((Component) this).transform.localRotation = this._closedRot;
        }
        if ((double) this._animTimer >= 0.800000011920929)
        {
          this._isAnimating = false;
          this.SetButtonsInteractable(true);
          ((Component) this).transform.localPosition = this._openPos;
          ((Component) this).transform.localRotation = this._openRot;
        }
      }
    }
    if ((!this._isOpen ? 0 : (!this._isAnimating ? 1 : 0)) != 0)
    {
      if (!this._serviceReady)
      {
        if ((!((UnityEngine.Object) this.recorderStatusText != (UnityEngine.Object) null) ? 0 : (((TMP_Text) this.recorderStatusText).text != "INITIALIZING\nRECORDER..." ? 1 : 0)) != 0)
          ((TMP_Text) this.recorderStatusText).text = "INITIALIZING\nRECORDER...";
      }
      else if (this._currentState == RecordingPanelController.PanelState.Recording && ((UnityEngine.Object) this.recorderStatusText != (UnityEngine.Object) null))
      {
        float num = Time.time - this._recordingStartTime;
        ((TMP_Text) this.recorderStatusText).text = "RECORDING\n" + $"MAX QUALITY\n{(int) ((double) num / 60.0):D2}:{(int) ((double) num % 60.0):D2}";
      }
    }
    if ((!((UnityEngine.Object) this._clonedRecorderObj != (UnityEngine.Object) null) || !((UnityEngine.Object) Plugin.Ins != (UnityEngine.Object) null) ? 0 : (((UnityEngine.Object) Plugin.Ins.camera != (UnityEngine.Object) null) ? 1 : 0)) == 0)
      return;
    Camera component = this._clonedRecorderObj.GetComponent<Camera>();
    if (!((UnityEngine.Object) component != (UnityEngine.Object) null))
      return;
    component.fieldOfView = Plugin.Ins.camera.fieldOfView;
    component.nearClipPlane = Plugin.Ins.camera.nearClipPlane;
  }

  private void OnDestroy()
  {
    ThemeManager.OnThemeApplied -= new Action(this.ApplyThemeColors);
    if (((UnityEngine.Object) this.btn1 != (UnityEngine.Object) null))
      this.btn1.onPressed -= new Action<GorillaFingerButton, bool>(this.OnBtn1Pressed);
    if (((UnityEngine.Object) this.btn2 != (UnityEngine.Object) null))
      this.btn2.onPressed -= new Action<GorillaFingerButton, bool>(this.OnBtn2Pressed);
    if (((UnityEngine.Object) this._clonedRecorderObj != (UnityEngine.Object) null))
      UnityEngine.Object.Destroy((UnityEngine.Object) this._clonedRecorderObj);
    this.ReEnableLckRenderFeature();
  }

  private enum PanelState
  {
    Idle,
    ConfirmStart,
    Recording,
    ConfirmStop,
  }
}
