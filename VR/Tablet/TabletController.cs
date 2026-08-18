using System;
using System.Collections.Generic;
using SakuraaCastingMod.Core;
using SakuraaCastingMod.Shared.Helpers;
using SakuraaCastingMod.VR.Interaction;
using SakuraaCastingMod.VR.UtilMenu;
using SakuraaCastingMod.VR.UtilMenu.Pages;
using SakuraaCastingMod.VR.UtilMenu.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;

namespace SakuraaCastingMod.VR.Tablet;

public class TabletController : MonoBehaviour
{
	public static TabletController Ins;

	public int currentTabletMode;

	public int currentTabletView;

	public Transform cameraFollower;

	[SavedSetting("TabletRollLocked", false)]
	private static bool _rollLocked;

	public bool locked;

	public List<GameObject> requiredLayerObjs = new List<GameObject>();

	public GameObject dynamicSlider;

	public Transform advancedOptions;

	[SavedSetting("TabletBodyPartTarget", 1)]
	private static int _bodyPartTarget = 1;

	private Transform _camSpot;

	[SavedSetting("TabletCentered3Pv", true)]
	private static bool _centered3Pv = true;

	[SavedSetting("TabletClipping", 0.1f)]
	private static float _clipping = 0.1f;

	private TextMeshPro _clippingVT;

	[SavedSetting("TabletDistance", 1.5f)]
	private static float _distance = 1.5f;

	private TextMeshPro _distanceVT;

	private TextMeshPro _dynamicTT;

	private TextMeshPro _dynamicVT;

	private Transform _extraOptions;

	private bool _flipped;

	[SavedSetting("TabletFov", 70f)]
	private static float _fov = 70f;

	private TextMeshPro _fovVT;

	[SavedSetting("TabletFpvSmoothness", "LOW")]
	private static string _fpvSmoothness = "LOW";

	[SavedSetting("TabletFpvClamping", true)]
	private static bool _fpvClamping = true;

	[SavedSetting("TabletFpvClampAngle", 30f)]
	private static float _fpvClampAngle = 30f;

	private bool _wasFpvClamped;

	[SavedSetting("Tablet3pvSmoothing", 0f)]
	private static float _thirdPvSmoothing = 0f;

	[SavedSetting("TabletFpvOffset", 0f)]
	private static float _fpvOffset = 0f;

	private TextMeshPro _fpvPosVT;

	private TextMeshPro _clampingTT;

	private TextMeshPro _clampingVT;

	public GameObject clampingSlider;

	private float _lastButtonPress;

	private float _pageSwitchTime;

	[SavedSetting("TabletRotation", 4f)]
	private static float _rotation = 4f;

	private TextMeshPro _rotationVT;

	private GameObject _screen1;

	private GameObject _screen2;

	private GameObject _screen3;

	private Transform _simpleOptions;

	[SavedSetting("TabletSize", 1f)]
	public static float Size = 1f;

	private TextMeshPro _sizeVT;

	private Transform _skibidiFollower;

	[SavedSetting("TabletSpeed", 1.8f)]
	private static float _speed = 1.8f;

	private TextMeshPro _speedVT;

	private GameObject _thirdPersonOffset;

	private RecordingPanelController _recordingPanelController;

	private List<GorillaFingerButton> _mode0Btns = new List<GorillaFingerButton>();

	private List<GorillaFingerButton> _mode1Btns = new List<GorillaFingerButton>();

	private List<GorillaFingerButton> _mode2Btns = new List<GorillaFingerButton>();

	private List<GorillaFingerButton> _mode3Btns = new List<GorillaFingerButton>();

	private List<GorillaFingerButton> _mode4Btns = new List<GorillaFingerButton>();

	private List<GorillaFingerButton> _lockBtns = new List<GorillaFingerButton>();

	private List<GorillaFingerButton> _flipBtns = new List<GorillaFingerButton>();

	private List<GorillaFingerButton> _recorderBtns = new List<GorillaFingerButton>();

	private List<GorillaFingerButton> _rollLockBtns = new List<GorillaFingerButton>();

	private List<GorillaFingerButton> _cosmeticBtns = new List<GorillaFingerButton>();

	private bool _tabletHidden;

	public void Awake()
	{
		Ins = this;
	}

	public void Start()
	{
		base.transform.position = new Vector3(-67f, 12f, -82f);
		base.transform.rotation = Quaternion.Euler(0f, -90f, 0f);
		Plugin.Ins.InitTablet();
		_simpleOptions = base.transform.Find("SimpleOptions");
		advancedOptions = base.transform.Find("AdvancedOptions");
		_extraOptions = base.transform.Find("ExtraOptions");
		_camSpot = base.transform.Find("bar_smooth").Find("CamSpot");
		_clippingVT = advancedOptions.Find("Clipping_slider").Find("ValueText").GetComponent<TextMeshPro>();
		_fovVT = advancedOptions.Find("Fov_slider").Find("ValueText").GetComponent<TextMeshPro>();
		Transform transform = advancedOptions.Find("FpvPos_slider");
		if (transform != null)
		{
			_fpvPosVT = transform.Find("ValueText").GetComponent<TextMeshPro>();
		}
		_dynamicTT = advancedOptions.Find("Dynamic_slider").Find("Text (TMP)").GetComponent<TextMeshPro>();
		_dynamicVT = advancedOptions.Find("Dynamic_slider").Find("ValueText").GetComponent<TextMeshPro>();
		Transform transform2 = advancedOptions.Find("Clamping_slider");
		if (transform2 != null)
		{
			clampingSlider = transform2.gameObject;
			_clampingTT = transform2.Find("Text (TMP)").GetComponent<TextMeshPro>();
			_clampingVT = transform2.Find("ValueText").GetComponent<TextMeshPro>();
		}
		_sizeVT = _extraOptions.Find("Size_slider").Find("ValueText").GetComponent<TextMeshPro>();
		_distanceVT = _extraOptions.Find("Distance_slider").Find("ValueText").GetComponent<TextMeshPro>();
		_speedVT = _extraOptions.Find("Speed_slider").Find("ValueText").GetComponent<TextMeshPro>();
		_rotationVT = _extraOptions.Find("Rotation_slider").Find("ValueText").GetComponent<TextMeshPro>();
		ThemeManager.OnThemeApplied += ApplyThemeColors;
		ApplyThemeColors();
		SwitchViewMode();
		_screen1 = _simpleOptions.Find("Plane").gameObject;
		_screen2 = advancedOptions.Find("Plane").gameObject;
		_screen3 = _extraOptions.Find("Plane").gameObject;
		_skibidiFollower = new GameObject().transform;
		cameraFollower = GameObject.Find("Player Objects/Player VR Controller/GorillaPlayer/TurnParent/Main Camera/Camera Follower").transform;
		Transform transform3 = FindDeep(base.transform, "RecordingPanel");
		if (!(transform3 != null))
		{
			UnityEngine.Debug.LogError("SakuraaCastingMod: RecordingPanel object not found in Tablet hierarchy!");
		}
		else
		{
			_recordingPanelController = transform3.GetComponent<RecordingPanelController>();
			if (_recordingPanelController == null)
			{
				_recordingPanelController = transform3.gameObject.AddComponent<RecordingPanelController>();
			}
		}
		if (XRSettings.isDeviceActive)
		{
			SetupButtons();
			return;
		}
		currentTabletView = 0;
		SwitchViewMode();
	}

	private void SetupButtons()
	{
		_mode0Btns.Clear();
		_mode1Btns.Clear();
		_mode2Btns.Clear();
		_mode3Btns.Clear();
		_mode4Btns.Clear();
		_lockBtns.Clear();
		_flipBtns.Clear();
		_recorderBtns.Clear();
		_rollLockBtns.Clear();
		_cosmeticBtns.Clear();
		SetupButton(_simpleOptions, "1stPerspectiveBtn", FirstPersonButton, _mode1Btns);
		SetupButton(_simpleOptions, "3rdPerspectiveBtn", ThirdPersonButton, _mode2Btns);
		SetupButton(_simpleOptions, "GoProModeBtn", GoProModeButton, _mode0Btns);
		SetupButton(_simpleOptions, "FollowModeBtn", FollowModeButton, _mode3Btns);
		SetupButton(_simpleOptions, "TrackMonkeBtn", TrackMonkeButton, _mode4Btns);
		SetupButton(_simpleOptions, "LockBtn", LockCameraButton, _lockBtns);
		SetupButton(_simpleOptions, "FlipBtn", FlipButton, _flipBtns);
		SetupButton(_simpleOptions, "RecorderToggleBtn", ToggleRecordingPanel, _recorderBtns);
		SetupButton(_simpleOptions, "SettingsBtn", SettingsButton);
		SetupButton(advancedOptions, "1stPerspectiveBtn", FirstPersonButton, _mode1Btns);
		SetupButton(advancedOptions, "3rdPerspectiveBtn", ThirdPersonButton, _mode2Btns);
		SetupButton(advancedOptions, "GoProModeBtn", GoProModeButton, _mode0Btns);
		SetupButton(advancedOptions, "FollowModeBtn", FollowModeButton, _mode3Btns);
		SetupButton(advancedOptions, "TrackMonkeBtn", TrackMonkeButton, _mode4Btns);
		SetupButton(advancedOptions, "LockBtn", LockCameraButton, _lockBtns);
		SetupButton(advancedOptions, "FlipBtn2", FlipButton, _flipBtns);
		SetupButton(advancedOptions, "RecorderToggleBtn", ToggleRecordingPanel, _recorderBtns);
		SetupButton(advancedOptions, "SettingsBtn2", SettingsButton);
		SetupButton(_extraOptions, "RollLockBtn", RollLockButton, _rollLockBtns);
		SetupButton(_extraOptions, "FlipBtn3", FlipButton, _flipBtns);
		SetupButton(_extraOptions, "RecorderToggleBtn", ToggleRecordingPanel, _recorderBtns);
		SetupButton(_extraOptions, "SettingsBtn3", SettingsButton);
		SetupButton(_extraOptions, "CosmeticsBtn", CosmeticButton, _cosmeticBtns);
	}

	private void SetupButton(Transform parent, string childName, UnityAction action, List<GorillaFingerButton> listToAdd = null)
	{
		if (!XRSettings.isDeviceActive || parent == null)
		{
			return;
		}
		Transform transform = parent.Find(childName);
		if (transform == null)
		{
			return;
		}
		GameObject gameObject = transform.gameObject;
		gameObject.layer = 18;
		if (!requiredLayerObjs.Contains(gameObject))
		{
			requiredLayerObjs.Add(gameObject);
		}
		GorillaFingerButton gfb = gameObject.GetComponent<GorillaFingerButton>();
		if (gfb == null)
		{
			gfb = gameObject.AddComponent<GorillaFingerButton>();
		}
		if (UtilMenuMain.Instance != null)
		{
			gfb.unpressedMaterial = UtilMenuMain.Instance.buttonMat;
			gfb.pressedMaterial = UtilMenuMain.Instance.pressedButtonMat;
		}
		gfb.onPressButton = new UnityEvent();
		gfb.onPressButton.AddListener(delegate
		{
			if (!CheckPress())
			{
				gfb.cancelSound = true;
			}
			else
			{
				action();
			}
		});
		listToAdd?.Add(gfb);
	}

	public void ToggleRecordingPanel()
	{
		if (_recordingPanelController != null)
		{
			_recordingPanelController.TogglePanel();
		}
	}

	private void ApplyThemeColors()
	{
		Color customColor = ThemeManager.GetCustomColor("BUTTON");
		if ((bool)_clippingVT)
		{
			_clippingVT.color = customColor;
		}
		if ((bool)_fovVT)
		{
			_fovVT.color = customColor;
		}
		if ((bool)_fpvPosVT)
		{
			_fpvPosVT.color = customColor;
		}
		if ((bool)_dynamicVT)
		{
			_dynamicVT.color = customColor;
		}
		if ((bool)_clampingVT)
		{
			_clampingVT.color = customColor;
		}
		if ((bool)_sizeVT)
		{
			_sizeVT.color = customColor;
		}
		if ((bool)_distanceVT)
		{
			_distanceVT.color = customColor;
		}
		if ((bool)_speedVT)
		{
			_speedVT.color = customColor;
		}
		if ((bool)_rotationVT)
		{
			_rotationVT.color = customColor;
		}
	}

	private void OnDestroy()
	{
		ThemeManager.OnThemeApplied -= ApplyThemeColors;
	}

	private Transform FindDeep(Transform root, string name)
	{
		foreach (Transform item in root)
		{
			if (!(item.name == name))
			{
				Transform transform2 = FindDeep(item, name);
				if (transform2 != null)
				{
					return transform2;
				}
				continue;
			}
			return item;
		}
		return null;
	}

	public void LateUpdate()
	{
		if (Plugin.Ins.modDisabled || Plugin.Ins.currentCameraMode != 1 || !XRSettings.isDeviceActive)
		{
			return;
		}
		VRRig myRig = Networking.MyRig;
		if (myRig == null)
		{
			return;
		}
		if (!InputManager.Ins.rightSecondaryBtnDouble)
		{
			if (InputManager.Ins.leftSecondaryBtnDouble)
			{
				locked = false;
				_tabletHidden = false;
				base.transform.position = myRig.leftHandTransform.position;
				base.transform.rotation = Quaternion.LookRotation(-(myRig.transform.position - base.transform.position));
				HapticEngine.Play(HapticPreset.TabletSummon, isLeftHand: true);
			}
		}
		else
		{
			locked = false;
			_tabletHidden = false;
			base.transform.position = myRig.rightHandTransform.position;
			base.transform.rotation = Quaternion.LookRotation(-(myRig.transform.position - base.transform.position));
			HapticEngine.Play(HapticPreset.TabletSummon, isLeftHand: false);
		}
		switch (currentTabletMode)
		{
		case 0:
			LateUpdateGoPro();
			break;
		case 1:
			LateUpdateFirstPerson();
			break;
		case 2:
			LateUpdateThirdPerson();
			break;
		case 3:
			LateUpdateRigFollow();
			break;
		case 4:
			LateUpdateRigTracking();
			break;
		}
		int num = currentTabletMode;
		bool flag = (uint)(num - 1) <= 1u;
		if (!flag || !_tabletHidden)
		{
			if (!locked)
			{
				_skibidiFollower.parent = myRig.headMesh.transform;
				_skibidiFollower.position = base.transform.position;
				_skibidiFollower.rotation = base.transform.rotation;
			}
			else
			{
				UpdateLocked();
			}
		}
		else
		{
			base.transform.position = Vector3.zero;
		}
		if ((currentTabletMode != 3 || _flipped) && (currentTabletMode != 4 || _flipped))
		{
			_screen1.transform.localScale = new Vector3(0.04616899f, 0.02476996f, 0.02508292f);
			_screen2.transform.localScale = new Vector3(0.0393403f, 0.02136979f, 0.02163692f);
			_screen3.transform.localScale = new Vector3(0.03856949f, 0.002410593f, 0.02169534f);
		}
		else
		{
			_screen1.transform.localScale = new Vector3(-0.04616899f, 0.02476996f, 0.02508292f);
			_screen2.transform.localScale = new Vector3(-0.0393403f, 0.02136979f, 0.02163692f);
			_screen3.transform.localScale = new Vector3(-0.03856949f, 0.002410593f, 0.02169534f);
		}
		Plugin.Ins.camera.fieldOfView = _fov;
		Plugin.Ins.camera.nearClipPlane = _clipping * Size;
		_sizeVT.text = $"{Math.Round(Size, 2)}";
		_clippingVT.text = $"{Math.Round(Size * _clipping, 2)}";
		_fovVT.text = $"{Math.Round(_fov, 2)}";
		if (_fpvPosVT != null)
		{
			_fpvPosVT.text = $"{Math.Round(_fpvOffset, 2)}";
		}
		string text = "Erm?!?";
		string text2 = "?????";
		switch (currentTabletMode)
		{
		case 1:
			text = "FPV SMOOTH";
			text2 = _fpvSmoothness ?? "";
			break;
		case 2:
			text = "POSITION";
			text2 = (_centered3Pv ? "CENTERED" : "SIDE");
			break;
		case 3:
			text = "TARGET";
			text2 = ((_bodyPartTarget == 0) ? "head" : "body");
			break;
		case 4:
			text = "TARGET";
			text2 = ((_bodyPartTarget == 0) ? "head" : "body");
			break;
		}
		_dynamicTT.text = text;
		_dynamicVT.text = text2;
		if (_clampingTT != null && _clampingVT != null)
		{
			string text3 = "";
			string text4 = "";
			switch (currentTabletMode)
			{
			case 2:
				text3 = "3PV SMOOTH";
				text4 = ((_thirdPvSmoothing <= 0f) ? "OFF" : $"{Math.Round(_thirdPvSmoothing, 2)}");
				break;
			case 1:
				text3 = "CLAMPING";
				text4 = (_fpvClamping ? $"{_fpvClampAngle}°" : "OFF");
				break;
			}
			_clampingTT.text = text3;
			_clampingVT.text = text4;
		}
		_distanceVT.text = $"{Math.Round(_distance, 2)}";
		_speedVT.text = $"{Math.Round(_speed, 2)}";
		_rotationVT.text = $"{Math.Round(_rotation, 2)}";
		UpdateButtonsVisuals();
	}

	private void UpdateButtonsVisuals()
	{
		SetBtnState(_mode0Btns, currentTabletMode == 0);
		SetBtnState(_mode1Btns, currentTabletMode == 1);
		SetBtnState(_mode2Btns, currentTabletMode == 2);
		SetBtnState(_mode3Btns, currentTabletMode == 3);
		SetBtnState(_mode4Btns, currentTabletMode == 4);
		SetBtnState(_lockBtns, locked);
		SetBtnState(_flipBtns, _flipped);
		SetBtnState(_rollLockBtns, _rollLocked);
		bool isActive = _recordingPanelController != null && _recordingPanelController.gameObject.activeSelf;
		SetBtnState(_recorderBtns, isActive);
		bool flag = CosmeticsPage.Instance != null && CosmeticsPage.IsHideCosmeticsEnabled;
		SetBtnState(_cosmeticBtns, !flag);
		static void SetBtnState(List<GorillaFingerButton> btns, bool flag2)
		{
			if (!(Plugin.Ins == null))
			{
				foreach (GorillaFingerButton btn in btns)
				{
					if (btn != null && btn.isOn != flag2)
					{
						btn.isOn = flag2;
						btn.UpdateColor();
					}
				}
			}
		}
	}

	private void FixedUpdate()
	{
		dynamicSlider.SetActive(currentTabletMode != 0);
		if (clampingSlider != null)
		{
			GameObject gameObject = clampingSlider;
			int num = currentTabletMode;
			bool active = (uint)(num - 1) <= 1u;
			gameObject.SetActive(active);
		}
	}

	public void UpdateLocked()
	{
		if (currentTabletMode != 3 && currentTabletMode != 4)
		{
			_skibidiFollower.parent = GorillaTagger.Instance.bodyCollider.transform;
			base.transform.position = Vector3.Lerp(base.transform.position, _skibidiFollower.position, 100f * Time.deltaTime);
			base.transform.rotation = Quaternion.Slerp(base.transform.rotation, _skibidiFollower.rotation, 100f * Time.deltaTime);
		}
		else
		{
			locked = false;
		}
	}

	private void LateUpdateThirdPerson()
	{
		if (_thirdPersonOffset == null)
		{
			_thirdPersonOffset = new GameObject();
		}
		if (_centered3Pv)
		{
			_thirdPersonOffset.transform.parent = Networking.MyRig.headMesh.transform;
			_thirdPersonOffset.transform.localPosition = new Vector3(0f, 0.12f, -1.4f);
			_thirdPersonOffset.transform.localRotation = Quaternion.Euler(0f, 0f, -1f);
		}
		else
		{
			_thirdPersonOffset.transform.parent = Plugin.Ins.originalCameraParent;
			Plugin.Ins.camera.transform.GetChild(0).gameObject.SetActive(value: true);
			_thirdPersonOffset.transform.localPosition = Plugin.Ins.originalPos;
			_thirdPersonOffset.transform.localRotation = Plugin.Ins.originalRot;
		}
		Plugin.Ins.camera.transform.parent = null;
		if (!(_thirdPvSmoothing <= 0f))
		{
			Plugin.Ins.camera.transform.position = Vector3.Lerp(Plugin.Ins.camera.transform.position, _thirdPersonOffset.transform.position, _thirdPvSmoothing * Time.deltaTime);
			Plugin.Ins.camera.transform.rotation = Quaternion.Slerp(Plugin.Ins.camera.transform.rotation, _thirdPersonOffset.transform.rotation, _thirdPvSmoothing * Time.deltaTime);
		}
		else
		{
			Plugin.Ins.camera.transform.position = _thirdPersonOffset.transform.position;
			Plugin.Ins.camera.transform.rotation = _thirdPersonOffset.transform.rotation;
		}
	}

	private void LateUpdateRigFollow()
	{
		VRRig myRig = Networking.MyRig;
		Transform transform = Plugin.Ins.camera.transform;
		if (transform.parent != _camSpot)
		{
			transform.parent = _camSpot;
			transform.localPosition = Vector3.zero;
			transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
		}
		locked = false;
		Vector3 vector = myRig.transform.position;
		switch (_bodyPartTarget)
		{
		case 1:
		{
			Vector3 position = myRig.headMesh.transform.position;
			vector = new Vector3(position.x, position.y - 0.3f, position.z);
			break;
		}
		case 0:
			vector = myRig.transform.position;
			break;
		}
		Vector3 vector2 = vector - base.transform.position;
		if (Vector3.Distance(base.transform.position, vector) > _distance)
		{
			Vector3 b = vector - vector2.normalized * Mathf.Min(vector2.magnitude, _distance);
			base.transform.position = Vector3.Lerp(base.transform.position, b, _speed * Time.deltaTime);
		}
		if (vector2 != Vector3.zero)
		{
			Quaternion b2 = Quaternion.LookRotation(-vector2);
			base.transform.rotation = Quaternion.Slerp(base.transform.rotation, b2, _rotation * Time.deltaTime);
		}
	}

	private void LateUpdateRigTracking()
	{
		VRRig myRig = Networking.MyRig;
		Transform transform = Plugin.Ins.camera.transform;
		if (transform.parent != _camSpot)
		{
			transform.parent = _camSpot;
			transform.localPosition = Vector3.zero;
			transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
		}
		locked = false;
		Vector3 vector = myRig.transform.position;
		if (_bodyPartTarget == 0)
		{
			vector = myRig.transform.position;
		}
		else if (_bodyPartTarget == 1)
		{
			Vector3 position = myRig.headMesh.transform.position;
			vector = new Vector3(position.x, position.y - 0.3f, position.z);
		}
		Vector3 vector2 = vector - base.transform.position;
		if (vector2 != Vector3.zero)
		{
			Quaternion b = Quaternion.LookRotation(-vector2);
			base.transform.rotation = Quaternion.Slerp(base.transform.rotation, b, _rotation * Time.deltaTime);
		}
	}

	public void Update()
	{
		if (currentTabletMode == 1 && _fpvSmoothness == "CLIPPY" && !(Networking.MyRig == null))
		{
			VRRig myRig = Networking.MyRig;
			Transform transform = myRig.mainCamera.transform;
			Plugin.Ins.camera.transform.parent = null;
			Vector3 position = new Vector3(0f, 0.12f, _fpvOffset);
			Vector3 b = myRig.headMesh.transform.TransformPoint(position);
			Plugin.Ins.camera.transform.position = Vector3.Lerp(Plugin.Ins.camera.transform.position, b, 85f * Time.deltaTime);
			Quaternion b2 = transform.rotation;
			if (_rollLocked)
			{
				b2 = Quaternion.LookRotation(transform.forward, Vector3.up);
			}
			float smoothnessVal = GetSmoothnessVal("fpv", _fpvSmoothness);
			Plugin.Ins.camera.transform.rotation = Quaternion.Slerp(Plugin.Ins.camera.transform.rotation, b2, smoothnessVal * Time.deltaTime);
		}
	}

	public void LateUpdateFirstPerson()
	{
		VRRig myRig = Networking.MyRig;
		if (myRig == null)
		{
			return;
		}
		_flipped = false;
		Transform transform = myRig.mainCamera.transform;
		Quaternion rotation = transform.rotation;
		if (_rollLocked)
		{
			rotation = Quaternion.LookRotation(transform.forward, Vector3.up);
		}
		Vector3 vector = new Vector3(0f, 0.12f, _fpvOffset);
		switch (_fpvSmoothness)
		{
		case "OFF":
			if (_rollLocked)
			{
				Plugin.Ins.camera.transform.parent = null;
				Plugin.Ins.camera.transform.position = myRig.headMesh.transform.TransformPoint(vector);
				Plugin.Ins.camera.transform.rotation = rotation;
			}
			else
			{
				Plugin.Ins.camera.transform.parent = myRig.headMesh.transform;
				Plugin.Ins.camera.transform.localPosition = vector;
				Plugin.Ins.camera.transform.localRotation = Quaternion.identity;
			}
			break;
		case "HIGH":
		case "MID":
		case "MID-HIGH":
		case "LOW":
		case "MID-LOW":
		case "LOWEST":
		{
			Plugin.Ins.camera.transform.parent = null;
			Vector3 position = myRig.headMesh.transform.TransformPoint(vector);
			Plugin.Ins.camera.transform.position = position;
			Quaternion rotation2 = transform.rotation;
			Quaternion quaternion = Plugin.Ins.camera.transform.rotation;
			bool wasFpvClamped;
			if (wasFpvClamped = _fpvClamping && Quaternion.Angle(quaternion, rotation2) > _fpvClampAngle)
			{
				quaternion = Quaternion.RotateTowards(rotation2, quaternion, _fpvClampAngle);
				if (!_wasFpvClamped)
				{
					HapticEngine.Play(HapticPreset.ClampBump, isLeftHand: false);
				}
			}
			_wasFpvClamped = wasFpvClamped;
			float smoothnessVal = GetSmoothnessVal("fpv", _fpvSmoothness);
			quaternion = Quaternion.Slerp(quaternion, rotation2, smoothnessVal * Time.deltaTime);
			if (_rollLocked)
			{
				quaternion = Quaternion.LookRotation(quaternion * Vector3.forward, Vector3.up);
			}
			Plugin.Ins.camera.transform.rotation = quaternion;
			break;
		}
		}
	}

	private float GetSmoothnessVal(string mode, string type)
	{
		if (!(mode == "fpv"))
		{
			UnityEngine.Debug.LogError("Incorrect usage of GetSmoothnessVal, inputted Mode is not 3pv or fpv!");
			return 0f;
		}
		return type switch
		{
			"MID-LOW" => 20f, 
			"CLIPPY" => 5f, 
			"LOW" => 25f, 
			"LOWEST" => 30f, 
			"MID-HIGH" => 10f, 
			"HIGH" => 5f, 
			"MID" => 15f, 
			_ => 15f, 
		};
	}

	public void LateUpdateGoPro()
	{
		if (Plugin.Ins.camera.transform.parent != _camSpot || _flipped)
		{
			Transform transform = Plugin.Ins.camera.transform;
			transform.parent = _camSpot;
			transform.localPosition = default(Vector3);
		}
		Plugin.Ins.camera.transform.localRotation = (_flipped ? Quaternion.Euler(90f, 180f, 0f) : Quaternion.Euler(-90f, 0f, 0f));
	}

	private bool CheckPress()
	{
		Plugin.Ins.camera.transform.GetChild(0).gameObject.SetActive(value: false);
		if (Time.time < _pageSwitchTime + 0.5f)
		{
			return false;
		}
		if (!((double)(Time.time - _lastButtonPress) <= 0.2))
		{
			_lastButtonPress = Time.time;
			if (Plugin.Ins.currentCameraMode != 1)
			{
				Plugin.Ins.currentCameraMode = 1;
				Plugin.Ins.OnModeChange();
			}
			return true;
		}
		return false;
	}

	public void SwitchViewMode()
	{
		_simpleOptions.gameObject.SetActive(currentTabletView == 0);
		advancedOptions.gameObject.SetActive(currentTabletView == 1);
		_extraOptions.gameObject.SetActive(currentTabletView == 2);
		_pageSwitchTime = Time.time;
	}

	public void FirstPersonButton()
	{
		Plugin.Ins.camera.transform.parent = null;
		currentTabletMode = 1;
		_tabletHidden = true;
		HapticEngine.Play(HapticPreset.ModeSwitch);
	}

	public void ThirdPersonButton()
	{
		currentTabletMode = 2;
		_tabletHidden = true;
		HapticEngine.Play(HapticPreset.ModeSwitch);
	}

	public void GoProModeButton()
	{
		currentTabletMode = 0;
		_tabletHidden = false;
		HapticEngine.Play(HapticPreset.ModeSwitch);
	}

	public void FollowModeButton()
	{
		currentTabletMode = 3;
		_tabletHidden = false;
		HapticEngine.Play(HapticPreset.ModeSwitch);
	}

	public void TrackMonkeButton()
	{
		currentTabletMode = 4;
		_tabletHidden = false;
		HapticEngine.Play(HapticPreset.ModeSwitch);
	}

	public void LockCameraButton()
	{
		if (currentTabletMode == 3 || currentTabletMode == 4)
		{
			currentTabletMode = 0;
		}
		locked = !locked;
		base.transform.parent = null;
		HapticEngine.Play(locked ? HapticPreset.ToggleOn : HapticPreset.ToggleOff);
	}

	public void RollLockButton()
	{
		_rollLocked = !_rollLocked;
		HapticEngine.Play(_rollLocked ? HapticPreset.ToggleOn : HapticPreset.ToggleOff);
	}

	public void SettingsButton()
	{
		if (currentTabletView == 2)
		{
			currentTabletView = 0;
		}
		else
		{
			currentTabletView++;
		}
		SwitchViewMode();
		HapticEngine.Play(HapticPreset.NavTap);
	}

	public void FlipButton()
	{
		currentTabletMode = 0;
		_flipped = !_flipped;
		_tabletHidden = false;
		HapticEngine.Play(HapticPreset.ModeSwitch);
	}

	public void CosmeticButton()
	{
		if (CosmeticsPage.Instance != null)
		{
			CosmeticsPage.Instance.ToggleCosmetics();
		}
	}

	public void FOVSliderLeft()
	{
		if (_fov <= 40f)
		{
			_fov = 135f;
		}
		else
		{
			_fov -= 5f;
		}
		HapticEngine.Play(HapticPreset.SliderTick);
	}

	public void FOVSliderRight()
	{
		if (_fov >= 140f)
		{
			_fov = 45f;
		}
		else
		{
			_fov += 5f;
		}
		HapticEngine.Play(HapticPreset.SliderTick);
	}

	public void ClippingSliderLeft()
	{
		if ((double)_clipping <= 0.02)
		{
			HapticEngine.Play(HapticPreset.SliderLimit);
			return;
		}
		_clipping -= 0.02f;
		HapticEngine.Play(HapticPreset.SliderTick);
	}

	public void ClippingSliderRight()
	{
		_clipping += 0.02f;
		HapticEngine.Play(HapticPreset.SliderTick);
	}

	public void FpvPosSliderLeft()
	{
		_fpvOffset -= 0.05f;
		HapticEngine.Play(HapticPreset.SliderTick);
	}

	public void FpvPosSliderRight()
	{
		_fpvOffset += 0.05f;
		HapticEngine.Play(HapticPreset.SliderTick);
	}

	public void ClampingSliderLeft()
	{
		switch (currentTabletMode)
		{
		case 2:
			_thirdPvSmoothing = Mathf.Max(0f, _thirdPvSmoothing - 0.5f);
			break;
		case 1:
			if (!_fpvClamping)
			{
				_fpvClamping = true;
				_fpvClampAngle = 60f;
			}
			else if (!(_fpvClampAngle <= 10f))
			{
				_fpvClampAngle -= 5f;
			}
			else
			{
				_fpvClamping = false;
			}
			break;
		}
		HapticEngine.Play(HapticPreset.SliderTick);
	}

	public void ClampingSliderRight()
	{
		switch (currentTabletMode)
		{
		case 1:
			if (_fpvClamping)
			{
				if (!(_fpvClampAngle >= 60f))
				{
					_fpvClampAngle += 5f;
				}
				else
				{
					_fpvClamping = false;
				}
			}
			else
			{
				_fpvClamping = true;
				_fpvClampAngle = 10f;
			}
			break;
		case 2:
			_thirdPvSmoothing += 0.5f;
			break;
		}
		HapticEngine.Play(HapticPreset.SliderTick);
	}

	public void SizeSliderRight()
	{
		Size += 0.05f;
		Plugin.Ins.UpdateTabletScale(Size);
		HapticEngine.Play(HapticPreset.SliderTick);
	}

	public void SizeSliderLeft()
	{
		if ((double)Size <= 0.1)
		{
			HapticEngine.Play(HapticPreset.SliderLimit);
			return;
		}
		Size -= 0.05f;
		Plugin.Ins.UpdateTabletScale(Size);
		HapticEngine.Play(HapticPreset.SliderTick);
	}

	public void DynamicSliderLeft()
	{
		switch (currentTabletMode)
		{
		case 1:
			_fpvSmoothness = _fpvSmoothness switch
			{
				"LOW" => "LOWEST", 
				"MID-HIGH" => "MID", 
				"CLIPPY" => "HIGH", 
				"MID-LOW" => "LOW", 
				"MID" => "MID-LOW", 
				"OFF" => "CLIPPY", 
				"HIGH" => "MID-HIGH", 
				_ => "OFF", 
			};
			break;
		case 2:
			_centered3Pv = !_centered3Pv;
			break;
		case 3:
			if (_bodyPartTarget != 0)
			{
				_bodyPartTarget--;
			}
			else
			{
				_bodyPartTarget = 1;
			}
			break;
		case 4:
			if (_bodyPartTarget == 0)
			{
				_bodyPartTarget = 1;
			}
			else
			{
				_bodyPartTarget--;
			}
			break;
		}
		HapticEngine.Play(HapticPreset.SliderTick);
	}

	public void DynamicSliderRight()
	{
		switch (currentTabletMode)
		{
		case 1:
			_fpvSmoothness = _fpvSmoothness switch
			{
				"MID-LOW" => "MID", 
				"LOWEST" => "LOW", 
				"LOW" => "MID-LOW", 
				"MID-HIGH" => "HIGH", 
				"MID" => "MID-HIGH", 
				"HIGH" => "CLIPPY", 
				"OFF" => "LOWEST", 
				_ => "OFF", 
			};
			break;
		case 2:
			_centered3Pv = !_centered3Pv;
			break;
		case 3:
			if (_bodyPartTarget != 0)
			{
				_bodyPartTarget--;
			}
			else
			{
				_bodyPartTarget = 1;
			}
			break;
		case 4:
			if (_bodyPartTarget != 0)
			{
				_bodyPartTarget--;
			}
			else
			{
				_bodyPartTarget = 1;
			}
			break;
		}
		HapticEngine.Play(HapticPreset.SliderTick);
	}

	public void RotationSliderLeft()
	{
		_rotation -= 0.2f;
		HapticEngine.Play(HapticPreset.SliderTick);
	}

	public void RotationSliderRight()
	{
		_rotation += 0.2f;
		HapticEngine.Play(HapticPreset.SliderTick);
	}

	public void SpeedSliderLeft()
	{
		if (_speed <= 0f)
		{
			HapticEngine.Play(HapticPreset.SliderLimit);
			return;
		}
		_speed -= 0.1f;
		HapticEngine.Play(HapticPreset.SliderTick);
	}

	public void SpeedSliderRight()
	{
		_speed += 0.1f;
		HapticEngine.Play(HapticPreset.SliderTick);
	}

	public void DistanceSliderLeft()
	{
		if (_distance <= 0f)
		{
			HapticEngine.Play(HapticPreset.SliderLimit);
			return;
		}
		_distance -= 0.5f;
		HapticEngine.Play(HapticPreset.SliderTick);
	}

	public void DistanceSliderRight()
	{
		_distance += 0.5f;
		HapticEngine.Play(HapticPreset.SliderTick);
	}
}
