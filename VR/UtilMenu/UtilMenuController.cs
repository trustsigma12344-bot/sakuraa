using GorillaLocomotion;
using SakuraaCastingMod.Core;
using SakuraaCastingMod.Shared.Helpers;
using SakuraaCastingMod.Shared.Models;
using SakuraaCastingMod.VR.Interaction;
using SakuraaCastingMod.VR.UtilMenu.Pages;
using SakuraaCastingMod.VR.UtilMenu.Utility;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.VR.UtilMenu;

public class UtilMenuController : MonoBehaviour
{
  public static UtilMenuController Instance;
  public bool isMenuEnabled = false;
  [SavedSetting("MenuToggleButton", 2)]
  public static UtilMenuController.MenuToggleButton CurrentToggleButton = UtilMenuController.MenuToggleButton.Primary;
  [SavedSetting("MenuClickType", 1)]
  public static UtilMenuController.MenuClickType CurrentClickType = UtilMenuController.MenuClickType.Double;
  [SavedSetting("MenuHoverEnabled", false)]
  public static bool IsHovering = false;
  [SavedSetting("UtilMenuOffsetPosX", 0.0f)]
  public static float OffsetPosX = 0.0f;
  [SavedSetting("UtilMenuOffsetPosY", 0.0f)]
  public static float OffsetPosY = 0.0f;
  [SavedSetting("UtilMenuOffsetPosZ", 0.0f)]
  public static float OffsetPosZ = 0.0f;
  [SavedSetting("UtilMenuOffsetRotX", 0.0f)]
  public static float OffsetRotX = 0.0f;
  [SavedSetting("UtilMenuOffsetRotY", 0.0f)]
  public static float OffsetRotY = 0.0f;
  [SavedSetting("UtilMenuOffsetRotZ", 0.0f)]
  public static float OffsetRotZ = 0.0f;
  private readonly Vector3 _childPosOffset = new Vector3(-0.086f, -0.049f, 0.1051f);
  private readonly Quaternion _childRotOffset = Quaternion.Euler(70.787f, 90f, 90f);
  public GameObject menuObj;
  public List<GameObject> requiredLayerObjs = new List<GameObject>();
  public List<UtilBarModel> BarList = new List<UtilBarModel>();
  public List<GameObject> sideBtnList = new List<GameObject>();
  public List<GameObject> bottomBtnList = new List<GameObject>();
  public TextMeshPro menuTitle;
  public TextMeshPro descriptionText;
  public GameObject menuBackBtnObj;
  public GameObject menuForwardBtnObj;
  public GameObject playerSelectRoot;
  public List<UtilFingerButton> playerButtons = new List<UtilFingerButton>();
  public List<TextMeshPro> playerButtonTexts = new List<TextMeshPro>();
  public GameObject barExtraRoot;
  public UtilFingerButton gunSelectBtn;
  public UtilFingerButton gunSelectSmallBtn;
  public UtilFingerButton peSliderLeft;
  public UtilFingerButton peSliderRight;
  public TextMeshPro peSliderText;
  public TextMeshPro peValueText;
  public GameObject panel2Root;
  public GameObject playerBackgroundObj;
  public TextMeshPro userInfoText;
  public TextMeshPro modAndCheatListText;
  public GameObject spotifyRoot;
  public Renderer spotifyAlbumCoverRenderer;
  public TextMeshPro spotifySongTitleText;
  public TextMeshPro spotifyArtistNameText;
  public TextMeshPro spotifyVolumeValueText;
  public UtilFingerButton spotifyVolumeLeftBtn;
  public UtilFingerButton spotifyVolumeRightBtn;
  public UtilFingerButton spotifyShuffleBtn;
  public UtilFingerButton spotifyRepeatBtn;
  public UtilFingerButton spotifyPausePlayBtn;
  public UtilFingerButton spotifyRewindBtn;
  public UtilFingerButton spotifySkipBtn;
  public GameObject spotifyPauseIcon;
  public GameObject spotifyPlayIcon;
  public GameObject volumeBarRoot;
  public UtilFingerButton volDownBtn;
  public UtilFingerButton volUpBtn;
  public UtilFingerButton volPrioritizeBtn;
  public UtilFingerButton volUnMuteBtn;
  public TextMeshPro volumeValueText;
  public GameObject reportBarRoot;
  public UtilFingerButton reportMainBtn;
  public UtilFingerButton reportSendBtn;
  public UtilFingerButton reportSliderLeft;
  public UtilFingerButton reportSliderRight;
  public TextMeshPro reportSliderText;
  public TextMeshPro reportValueText;
  public GameObject cloneCosmeticsBtnObj;
  public GameObject modCheckBarRoot;
  public UtilFingerButton modsCheckBtn;
  public UtilFingerButton cheatsCheckBtn;
  public TextMeshPro modsBtnText;
  public TextMeshPro cheatsBtnText;
  private Material _whiteMat;
  private List<BasePage> _pages = new List<BasePage>();
  private int _currentPageIndex = 0;
  private int _currentTabIndex = 0;
  private int _pageOffset = 0;

  public int CurrentPageIndex => this._currentPageIndex;

  public int CurrentTabIndex => this._currentTabIndex;

  public BasePage CurrentPage
  {
    get
    {
      return this._pages != null && this._currentPageIndex < this._pages.Count ? this._pages[this._currentPageIndex] : (BasePage) null;
    }
  }

  public List<BasePage> Pages => this._pages;

  private void Awake()
  {
    UtilMenuController.Instance = this;
    this.menuObj = ((Component) this).gameObject;
  }

  private void Start()
  {
    this.InitializeUIObjects();
    if (((UnityEngine.Object) this.descriptionText == (UnityEngine.Object) null))
      this.FindDescriptionText();
    this.LoadPages();
    this.RefreshUI();
    this.UpdateHandInteractions();
  }

  private void InitializeUIObjects()
  {
    for (int index = 0; index < this.BarList.Count; ++index)
    {
      UtilBarModel bar = this.BarList[index];
      if (!((UnityEngine.Object) bar.BarTransform == (UnityEngine.Object) null))
      {
        if (((UnityEngine.Object) bar.TextInputBorder == (UnityEngine.Object) null))
          bar.TextInputBorder = bar.BarTransform.Find("TextInputBorder");
        if (((UnityEngine.Object) bar.TextInputBorder != (UnityEngine.Object) null))
        {
          ((Component) bar.TextInputBorder).gameObject.SetActive(false);
          Collider collider = default;
          if (((Component) bar.TextInputBorder).TryGetComponent<Collider>(out collider))
            collider.enabled = false;
          if (((UnityEngine.Object) bar.InputText == (UnityEngine.Object) null))
          {
            Transform transform = bar.TextInputBorder.Find("InputText");
            if (((UnityEngine.Object) transform != (UnityEngine.Object) null))
              bar.InputText = ((Component) transform).GetComponent<TextMeshPro>();
          }
          if (((UnityEngine.Object) bar.InputText != (UnityEngine.Object) null))
            ((TMP_Text) bar.InputText).text = "";
        }
      }
    }
    MeshRenderer meshRenderer = default;
    if ((this.BarList.Count <= 0 ? 0 : (((UnityEngine.Object) this.BarList[0].ScreenButton != (UnityEngine.Object) null) ? 1 : 0)) != 0 && ((Component) this.BarList[0].ScreenButton).TryGetComponent<MeshRenderer>(out meshRenderer))
      this._whiteMat = ((Renderer) meshRenderer).material;
    if (!((UnityEngine.Object) this._whiteMat == (UnityEngine.Object) null))
      return;
    Shader shader = Shader.Find("Standard");
    if (!((UnityEngine.Object) shader != (UnityEngine.Object) null))
      return;
    this._whiteMat = new Material(shader)
    {
      color = Color.white
    };
  }

  private void FindDescriptionText()
  {
    Transform transform1 = ((Component) this).transform.Find("panel1");
    if (!((UnityEngine.Object) transform1 != (UnityEngine.Object) null))
      return;
    Transform transform2 = transform1.Find("screenButtons");
    if (!((UnityEngine.Object) transform2 != (UnityEngine.Object) null))
      return;
    Transform transform3 = transform2.Find("DescriptionText");
    if (!((UnityEngine.Object) transform3))
      return;
    this.descriptionText = ((Component) transform3).GetComponent<TextMeshPro>();
  }

  private void FixedUpdate()
  {
    if (!this.isMenuEnabled || (this._pages == null || this._currentPageIndex < 0 ? 0 : (this._currentPageIndex < this._pages.Count ? 1 : 0)) == 0)
      return;
    this._pages[this._currentPageIndex].FixedUpdate();
  }

  private void Update()
  {
    if (((UnityEngine.Object) InputManager.Ins == (UnityEngine.Object) null) || !this.CheckInputTrigger())
      return;
    if ((!((UnityEngine.Object) KeyboardController.Instance != (UnityEngine.Object) null) ? 0 : (KeyboardController.Instance.IsKeyboardActive ? 1 : 0)) != 0)
    {
      this.SnapToHand();
    }
    else
    {
      this.isMenuEnabled = !this.isMenuEnabled;
      HapticEngine.Play(this.isMenuEnabled ? HapticPreset.MenuOpen : HapticPreset.MenuClose, true);
      if ((!this.isMenuEnabled ? 0 : (UtilMenuController.IsHovering ? 1 : 0)) == 0)
        return;
      this.SnapToHoverPos();
    }
  }

  private bool CheckInputTrigger()
  {
    bool flag;
    if (!InputManager.Ins.f3Double)
    {
      switch (UtilMenuController.CurrentToggleButton)
      {
        case UtilMenuController.MenuToggleButton.Grip:
          switch (UtilMenuController.CurrentClickType)
          {
            case UtilMenuController.MenuClickType.Single:
              flag = InputManager.Ins.leftGripSingle;
              goto label_16;
            case UtilMenuController.MenuClickType.Double:
              flag = InputManager.Ins.leftGripDouble;
              goto label_16;
            case UtilMenuController.MenuClickType.Triple:
              flag = InputManager.Ins.leftGripTriple;
              goto label_16;
          }
          break;
        case UtilMenuController.MenuToggleButton.Secondary:
          switch (UtilMenuController.CurrentClickType)
          {
            case UtilMenuController.MenuClickType.Single:
              flag = InputManager.Ins.leftSecondaryBtnSingle;
              goto label_16;
            case UtilMenuController.MenuClickType.Double:
              flag = InputManager.Ins.leftSecondaryBtnDouble;
              goto label_16;
            case UtilMenuController.MenuClickType.Triple:
              flag = InputManager.Ins.leftSecondaryBtnTriple;
              goto label_16;
          }
          break;
        case UtilMenuController.MenuToggleButton.Primary:
          switch (UtilMenuController.CurrentClickType)
          {
            case UtilMenuController.MenuClickType.Single:
              flag = InputManager.Ins.leftPrimaryBtnSingle;
              goto label_16;
            case UtilMenuController.MenuClickType.Double:
              flag = InputManager.Ins.leftPrimaryBtnDouble;
              goto label_16;
            case UtilMenuController.MenuClickType.Triple:
              flag = InputManager.Ins.leftPrimaryBtnTriple;
              goto label_16;
          }
          break;
      }
      flag = false;
    }
    else
      flag = true;
label_16:
    return flag;
  }

  private void LateUpdate()
  {
    if (CosmeticsPage.Instance != null)
      CosmeticsPage.Instance.UpdateCosmeticsLogic();
    if (!this.isMenuEnabled)
    {
      if (!(((Component) this).transform.position != Vector3.zero))
        return;
      ((Component) this).transform.position = Vector3.zero;
    }
    else
    {
      if ((this._pages == null || this._currentPageIndex < 0 ? 0 : (this._currentPageIndex < this._pages.Count ? 1 : 0)) != 0)
        this._pages[this._currentPageIndex].LateUpdate();
      if ((!((UnityEngine.Object) KeyboardController.Instance != (UnityEngine.Object) null) ? 0 : (KeyboardController.Instance.IsKeyboardActive ? 1 : 0)) != 0)
        return;
      if (UtilMenuController.IsHovering)
        this.SnapToHoverPos();
      else
        this.SnapToHand();
    }
  }

  private void SnapToHand()
  {
    if ((((UnityEngine.Object) GorillaTagger.Instance == (UnityEngine.Object) null) ? 1 : (((UnityEngine.Object) GorillaTagger.Instance.leftHandTransform == (UnityEngine.Object) null) ? 1 : 0)) != 0)
      return;
    Transform controllerTransform = GTPlayer.Instance.LeftHand.controllerTransform;
    if (((UnityEngine.Object) controllerTransform == (UnityEngine.Object) null))
      return;
    ((Component) this).transform.position = controllerTransform.TransformPoint(0.015f + UtilMenuController.OffsetPosX, UtilMenuController.OffsetPosY - 0.05f, UtilMenuController.OffsetPosZ - 0.025f);
    ((Component) this).transform.rotation = ((controllerTransform.rotation * Quaternion.Euler(325f, 10f, 85f)) * Quaternion.Euler(UtilMenuController.OffsetRotX, UtilMenuController.OffsetRotY, UtilMenuController.OffsetRotZ));
  }

  private void SnapToHoverPos()
  {
    Transform transform = ((UnityEngine.Object) Camera.main) ? ((Component) Camera.main).transform : (Transform) null;
    if (((UnityEngine.Object) transform == (UnityEngine.Object) null))
      return;
    Vector3 vector3 = (transform.position + (transform.forward * 0.4f));
    Quaternion quaternion = (Quaternion.LookRotation((transform.position - ((Component) this).transform.position)) * Quaternion.Inverse(this._childRotOffset));
    ((Component) this).transform.position = Vector3.Lerp(((Component) this).transform.position, (vector3 - (quaternion * this._childPosOffset)), Time.deltaTime * 10f);
    ((Component) this).transform.rotation = Quaternion.Slerp(((Component) this).transform.rotation, quaternion, Time.deltaTime * 10f);
  }

  public void UpdateHandInteractions()
  {
    bool isHovering = UtilMenuController.IsHovering;
    foreach (UtilFingerButton componentsInChild in ((Component) this).GetComponentsInChildren<UtilFingerButton>(true))
      componentsInChild.allowLeftHand = isHovering;
  }

  public void SaveInputSettings()
  {
    Configuration.SaveSettings();
    this.UpdateHandInteractions();
  }

  public void LoadInputSettings()
  {
    Configuration.LoadSettings();
    this.UpdateHandInteractions();
  }

  public void LoadPages()
  {
    this._pages.Clear();
    this._pages.Add((BasePage) new LobbyPage());
    this._pages.Add((BasePage) new SpotifyPage());
    this._pages.Add((BasePage) new CosmeticsPage());
    this._pages.Add((BasePage) new NameTagsPage());
    this._pages.Add((BasePage) new WorldPage());
    this._pages.Add((BasePage) new SoundboardPage());
    this._pages.Add((BasePage) new MicPage());
    this._pages.Add((BasePage) new NotificationsPage());
    this._pages.Add((BasePage) new SettingsPage());
    if (UtilMenuMain.Instance.specialVariant == 1)
      this._pages.Add((BasePage) new ElliotPage());
    if (this._pages == null)
      return;
    for (int index = 0; index < this._pages.Count; ++index)
      this._pages[index].Start();
  }

  public void RefreshUI()
  {
    if (this._pages.Count == 0)
      return;
    if (this._currentPageIndex >= this._pages.Count)
      this._currentPageIndex = 0;
    BasePage page = this._pages[this._currentPageIndex];
    this.DeactivateAllCustomRoots();
    page.RefreshPageUI();
    bool flag1 = page.ShouldHideStandardBars();
    if (((UnityEngine.Object) this.menuTitle != (UnityEngine.Object) null))
      ((TMP_Text) this.menuTitle).text = page.PageName;
    if (this._currentTabIndex >= page.Tabs.Count)
      this._currentTabIndex = 0;
    UtilTab tab = page.Tabs[this._currentTabIndex];
    if (flag1)
    {
      foreach (UtilBarModel bar in this.BarList)
        ((Component) bar.BarTransform).gameObject.SetActive(false);
      if (((UnityEngine.Object) this.descriptionText != (UnityEngine.Object) null))
        ((Component) this.descriptionText).gameObject.SetActive(false);
      this.UpdateNavigationButtons();
    }
    else
    {
      bool flag2 = false;
      string str = "";
      if (!string.IsNullOrEmpty(tab.Description))
      {
        flag2 = true;
        str = tab.Description;
      }
      else
      {
        string accessError = page.GetAccessError();
        if (!string.IsNullOrEmpty(accessError))
        {
          flag2 = true;
          str = accessError;
        }
      }
      if (((UnityEngine.Object) this.descriptionText != (UnityEngine.Object) null))
      {
        ((Component) this.descriptionText).gameObject.SetActive(flag2);
        ((TMP_Text) this.descriptionText).text = str;
      }
      for (int index = 0; index < this.BarList.Count; ++index)
      {
        UtilBarModel bar = this.BarList[index];
        if (flag2)
          ((Component) bar.BarTransform).gameObject.SetActive(false);
        else if (index >= tab.Elements.Count)
        {
          ((Component) bar.BarTransform).gameObject.SetActive(false);
        }
        else
        {
          ((Component) bar.BarTransform).gameObject.SetActive(true);
          MenuElement element = tab.Elements[index];
          bool flag3 = element.Type == ElementType.Slider;
          bool flag4 = element.Type == ElementType.Input;
          bool flag5 = element.Type == ElementType.HoldableButton;
          if (((UnityEngine.Object) bar.ScreenButton))
          {
            ((Component) bar.ScreenButton).gameObject.SetActive(!flag3 && !flag4);
            if ((flag3 ? 0 : (!flag4 ? 1 : 0)) != 0)
            {
              MeshRenderer component1 = ((Component) bar.ScreenButton).GetComponent<MeshRenderer>();
              if (((UnityEngine.Object) component1 != (UnityEngine.Object) null))
              {
                Material innerBtnMat = UtilMenuMain.Instance.innerBtnMat;
                Material pressedButtonMat = UtilMenuMain.Instance.pressedButtonMat;
                Material material = innerBtnMat;
                if ((!((UnityEngine.Object) UtilMenuMain.Instance != (UnityEngine.Object) null) ? 0 : (UtilMenuMain.Instance.Icons != null ? 1 : 0)) != 0)
                {
                  Material iconMat1 = UtilMenuMain.Instance.Icons.Minus;
                  Material iconMat2 = UtilMenuMain.Instance.Icons.Plus;
                  if (((UnityEngine.Object) element.CustomLeftIcon != (UnityEngine.Object) null))
                    iconMat1 = element.CustomLeftIcon;
                  if (((UnityEngine.Object) element.CustomRightIcon != (UnityEngine.Object) null))
                    iconMat2 = element.CustomRightIcon;
                  this.SetButtonIcon(((Component) bar.SliderLeftButton).gameObject, iconMat1);
                  this.SetButtonIcon(((Component) bar.SliderRightButton).gameObject, iconMat2);
                }
                if ((element.Type != ElementType.Toggle ? 0 : (element.IsToggled ? 1 : 0)) != 0)
                  material = pressedButtonMat;
                if (((UnityEngine.Object) ((Renderer) component1).sharedMaterial != (UnityEngine.Object) material))
                  ((Renderer) component1).sharedMaterial = material;
              }
              if (!flag5)
              {
                SakuraaCastingMod.VR.Interaction.HoldableButton component2 = ((Component) bar.ScreenButton).GetComponent<SakuraaCastingMod.VR.Interaction.HoldableButton>();
                if (((UnityEngine.Object) component2))
                  UnityEngine.Object.Destroy((UnityEngine.Object) component2);
                UtilFingerButton component3 = ((Component) bar.ScreenButton).GetComponent<UtilFingerButton>();
                if (((UnityEngine.Object) component3))
                  ((Behaviour) component3).enabled = true;
              }
              else
              {
                UtilFingerButton component4 = ((Component) bar.ScreenButton).GetComponent<UtilFingerButton>();
                if (((UnityEngine.Object) component4))
                  ((Behaviour) component4).enabled = false;
                SakuraaCastingMod.VR.Interaction.HoldableButton holdableButton = ((Component) bar.ScreenButton).GetComponent<SakuraaCastingMod.VR.Interaction.HoldableButton>();
                if (((UnityEngine.Object) holdableButton == (UnityEngine.Object) null))
                  holdableButton = ComponentUtils.AddComponent<SakuraaCastingMod.VR.Interaction.HoldableButton>((Component) bar.ScreenButton);
                holdableButton.Callback = element.OnClick;
                holdableButton.holdTime = (double) element.HoldTime > 0.0 ? element.HoldTime : 2f;
                if (((UnityEngine.Object) component4))
                {
                  holdableButton.targetRenderer = component4.buttonRenderer;
                  holdableButton.unpressedMat = component4.unpressedMaterial;
                  holdableButton.pressedMat = component4.pressedMaterial;
                }
                ((Behaviour) holdableButton).enabled = true;
              }
            }
          }
          if (((UnityEngine.Object) bar.SliderLeftButton))
            ((Component) bar.SliderLeftButton).gameObject.SetActive(flag3);
          if (((UnityEngine.Object) bar.SliderRightButton))
            ((Component) bar.SliderRightButton).gameObject.SetActive(flag3);
          if (((UnityEngine.Object) bar.SliderTextTransform))
            ((Component) bar.SliderTextTransform).gameObject.SetActive(flag3);
          if (((UnityEngine.Object) bar.TextInputBorder))
            ((Component) bar.TextInputBorder).gameObject.SetActive(flag4);
          if (!flag3)
          {
            if (flag4)
            {
              if (((UnityEngine.Object) bar.InputText))
                ((TMP_Text) bar.InputText).text = element.Text;
              if (((UnityEngine.Object) bar.ButtonText))
                ((TMP_Text) bar.ButtonText).text = "";
            }
            else if (((UnityEngine.Object) bar.ButtonText))
              ((TMP_Text) bar.ButtonText).text = element.Text;
          }
          else
          {
            if (((UnityEngine.Object) bar.SliderText))
              ((TMP_Text) bar.SliderText).text = element.Text;
            if (((UnityEngine.Object) bar.ValueText))
            {
              bool flag6 = !string.IsNullOrEmpty(element.ValueText);
              ((Component) bar.ValueText).gameObject.SetActive(flag6);
              if (flag6)
                ((TMP_Text) bar.ValueText).text = element.ValueText;
            }
          }
        }
      }
      this.UpdateNavigationButtons();
    }
  }

  private void UpdateNavigationButtons()
  {
    BasePage page = this._pages[this._currentPageIndex];
    for (int index = 0; index < this.sideBtnList.Count; ++index)
    {
      if (index < page.Tabs.Count)
      {
        this.sideBtnList[index].SetActive(true);
        UtilFingerButton component = this.sideBtnList[index].GetComponent<UtilFingerButton>();
        if (((UnityEngine.Object) component != (UnityEngine.Object) null))
        {
          component.isOn = index == this._currentTabIndex;
          component.UpdateColor();
          this.SetButtonIcon(this.sideBtnList[index], page.Tabs[index].TabIcon);
        }
      }
      else
        this.sideBtnList[index].SetActive(false);
    }
    for (int index1 = 0; index1 < this.bottomBtnList.Count; ++index1)
    {
      int index2 = this._pageOffset + index1;
      if (index2 < this._pages.Count)
      {
        this.bottomBtnList[index1].SetActive(true);
        UtilFingerButton component = this.bottomBtnList[index1].GetComponent<UtilFingerButton>();
        if (((UnityEngine.Object) component != (UnityEngine.Object) null))
        {
          component.isOn = index2 == this._currentPageIndex;
          component.UpdateColor();
          this.SetButtonIcon(this.bottomBtnList[index1], this._pages[index2].PageIcon);
        }
      }
      else
        this.bottomBtnList[index1].SetActive(false);
    }
    if (((UnityEngine.Object) this.menuBackBtnObj))
      this.menuBackBtnObj.SetActive(this._pageOffset > 0);
    if (!((UnityEngine.Object) this.menuForwardBtnObj))
      return;
    this.menuForwardBtnObj.SetActive(this._pageOffset + 5 < this._pages.Count);
  }

  private void SetButtonIcon(GameObject buttonObj, Material iconMat)
  {
    if ((((UnityEngine.Object) buttonObj == (UnityEngine.Object) null) ? 1 : (((UnityEngine.Object) iconMat == (UnityEngine.Object) null) ? 1 : 0)) != 0)
      return;
    Transform transform = buttonObj.transform.Find("Icon");
    if (!((UnityEngine.Object) transform != (UnityEngine.Object) null))
      return;
    MeshRenderer component = ((Component) transform).GetComponent<MeshRenderer>();
    if ((!((UnityEngine.Object) component != (UnityEngine.Object) null) ? 0 : (((UnityEngine.Object) ((Renderer) component).sharedMaterial != (UnityEngine.Object) iconMat) ? 1 : 0)) == 0)
      return;
    ((Renderer) component).sharedMaterial = iconMat;
  }

  private bool IsInputBlocked()
  {
    return ((UnityEngine.Object) KeyboardController.Instance != (UnityEngine.Object) null) && KeyboardController.Instance.IsKeyboardActive;
  }

  private void DeactivateAllCustomRoots()
  {
    if ((!((UnityEngine.Object) this.playerSelectRoot) ? 0 : (this.playerSelectRoot.activeSelf ? 1 : 0)) != 0)
      this.playerSelectRoot.SetActive(false);
    if ((!((UnityEngine.Object) this.barExtraRoot) ? 0 : (this.barExtraRoot.activeSelf ? 1 : 0)) != 0)
      this.barExtraRoot.SetActive(false);
    if ((!((UnityEngine.Object) this.panel2Root) ? 0 : (this.panel2Root.activeSelf ? 1 : 0)) != 0)
      this.panel2Root.SetActive(false);
    if ((!((UnityEngine.Object) this.spotifyRoot) ? 0 : (this.spotifyRoot.activeSelf ? 1 : 0)) == 0)
      return;
    this.spotifyRoot.SetActive(false);
  }

  public void SwitchPage(int index)
  {
    if ((this.IsInputBlocked() || index < 0 ? 0 : (index < this._pages.Count ? 1 : 0)) == 0)
      return;
    if (this._currentPageIndex >= 0)
      this._pages[this._currentPageIndex].OnPageClosed();
    this._currentPageIndex = index;
    this._currentTabIndex = 0;
    this.RefreshUI();
  }

  public void OnTabPress(int index)
  {
    if (this.IsInputBlocked())
      return;
    int tabIndex = index - 1;
    if ((tabIndex < 0 ? 0 : (tabIndex < this._pages[this._currentPageIndex].Tabs.Count ? 1 : 0)) == 0)
      return;
    this._pages[this._currentPageIndex].OnTabSelected(tabIndex);
    this._currentTabIndex = tabIndex;
    this.RefreshUI();
  }

  public void OnScreenButtonPress(int index)
  {
    if (this.IsInputBlocked())
      return;
    BasePage page = this._pages[this._currentPageIndex];
    UtilTab tab = page.Tabs[this._currentTabIndex];
    if ((page.GetAccessError() != null ? 1 : (!string.IsNullOrEmpty(tab.Description) ? 1 : 0)) != 0)
      return;
    int index1 = index - 1;
    if (index1 >= tab.Elements.Count)
      return;
    MenuElement element = tab.Elements[index1];
    if (element.Type == ElementType.Toggle)
    {
      element.IsToggled = !element.IsToggled;
      HapticEngine.Play(element.IsToggled ? HapticPreset.ToggleOn : HapticPreset.ToggleOff);
      Action onToggle = element.OnToggle;
      if (onToggle != null)
        onToggle();
      this.RefreshUI();
    }
    else
    {
      if (element.Type != ElementType.Button)
        return;
      Action onClick = element.OnClick;
      if (onClick != null)
        onClick();
      this.RefreshUI();
    }
  }

  public void OnSliderLeftPress(int index)
  {
    if (this.IsInputBlocked())
      return;
    this.ProcessSlider(index, true);
  }

  public void OnSliderRightPress(int index)
  {
    if (this.IsInputBlocked())
      return;
    this.ProcessSlider(index, false);
  }

  private void ProcessSlider(int index, bool isLeft)
  {
    BasePage page = this._pages[this._currentPageIndex];
    UtilTab tab = page.Tabs[this._currentTabIndex];
    if ((page.GetAccessError() != null ? 1 : (!string.IsNullOrEmpty(tab.Description) ? 1 : 0)) != 0)
      return;
    int index1 = index - 1;
    if (index1 >= tab.Elements.Count)
      return;
    if (isLeft)
    {
      Action onSliderLeft = tab.Elements[index1].OnSliderLeft;
      if (onSliderLeft != null)
        onSliderLeft();
    }
    else
    {
      Action onSliderRight = tab.Elements[index1].OnSliderRight;
      if (onSliderRight != null)
        onSliderRight();
    }
    HapticEngine.Play(HapticPreset.SliderTick);
    this.RefreshUI();
  }

  public void MenuBackButton()
  {
    if ((this.IsInputBlocked() ? 0 : (this._pageOffset - 5 >= 0 ? 1 : 0)) == 0)
      return;
    this._pageOffset -= 5;
    this.RefreshUI();
  }

  public void MenuForwardButtton()
  {
    if ((this.IsInputBlocked() ? 0 : (this._pageOffset + 5 < this._pages.Count ? 1 : 0)) == 0)
      return;
    this._pageOffset += 5;
    this.RefreshUI();
  }

  public void Menu1Button() => this.SwitchPage(this._pageOffset);

  public void Menu2Button() => this.SwitchPage(this._pageOffset + 1);

  public void Menu3Button() => this.SwitchPage(this._pageOffset + 2);

  public void Menu4Button() => this.SwitchPage(this._pageOffset + 3);

  public void Menu5Button() => this.SwitchPage(this._pageOffset + 4);

  public enum MenuToggleButton
  {
    Grip,
    Secondary,
    Primary,
  }

  public enum MenuClickType
  {
    Single,
    Double,
    Triple,
  }
}
