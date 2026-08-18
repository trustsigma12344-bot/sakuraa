using Photon.Realtime;
using SakuraaCastingMod.VR.Interaction;
using SakuraaCastingMod.VR.UtilMenu;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Shared.Helpers;

public class RemoteModRepresentation : MonoBehaviour
{
  [SavedSetting("ShowOthersUtilMenu", true)]
  public static bool ShowOthersUtilMenu = true;
  private static readonly Vector3 MenuLocalPos = new Vector3(-0.0604f, 0.0769f, -0.0485f);
  private static readonly Quaternion MenuLocalRot = Quaternion.Euler(17.209f, 357.379f, 289.27f);
  public GameObject FakeMenu;
  private Player _owner;
  private VRRig _ownerRig;
  private GameObject _fakeKeyboard;
  private GameObject _fakePanel2;
  private bool _menuActive;
  private bool _keyboardActive;
  private bool _panel2Active;
  private Vector3 _menuPosOffset;
  private Vector3 _menuRotOffset;
  private Vector3 _lastAppliedPosOffset = Vector3.zero;
  private Vector3 _lastAppliedRotOffset = Vector3.zero;
  private readonly List<(string slot, Material mat)> _themedMaterials = new List<(string, Material)>();
  private bool _initializeComplete;

  public void Initialize(GameObject menuPrefab, Player owner)
  {
    this._owner = owner;
    this.StartCoroutine(this.InitializeAsync(menuPrefab));
  }

  private IEnumerator InitializeAsync(GameObject menuPrefab)
  {
    this.FakeMenu = UnityEngine.Object.Instantiate<GameObject>(menuPrefab, ((Component) this).transform);
    this.FakeMenu.SetActive(false);
    yield return (object) null;
    this.DestroyScript<UtilMenuController>(this.FakeMenu);
    this.DestroyScript<UtilFingerButton>(this.FakeMenu);
    this.DestroyScript<GorillaFingerButton>(this.FakeMenu);
    yield return (object) null;
    this.CleanupColliders(this.FakeMenu);
    yield return (object) null;
    this.ForceMenuDefaultState(this.FakeMenu);
    yield return (object) null;
    this.CloneThemedMaterials(this.FakeMenu);
    yield return (object) null;
    Transform kb = this.FakeMenu.transform.Find("panel1/keyboard");
    if (((UnityEngine.Object) kb))
    {
      this._fakeKeyboard = ((Component) kb).gameObject;
      this._fakeKeyboard.SetActive(false);
    }
    Transform p2 = this.FakeMenu.transform.Find("panel2");
    if (((UnityEngine.Object) p2))
    {
      this._fakePanel2 = ((Component) p2).gameObject;
      this._fakePanel2.SetActive(false);
    }
    this._initializeComplete = true;
  }

  private void ForceMenuDefaultState(GameObject menu)
  {
    Transform transform1 = menu.transform.Find("panel1/screenButtons");
    if (((UnityEngine.Object) transform1 != (UnityEngine.Object) null))
    {
      for (int index = 1; index <= 6; ++index)
      {
        Transform transform2 = transform1.Find("bar" + index.ToString());
        if (((UnityEngine.Object) transform2))
          ((Component) transform2).gameObject.SetActive(false);
      }
      Transform transform3 = transform1.Find("DescriptionText");
      if (((UnityEngine.Object) transform3))
        ((Component) transform3).gameObject.SetActive(false);
      Transform transform4 = transform1.Find("PlayerSelect");
      if (((UnityEngine.Object) transform4))
        ((Component) transform4).gameObject.SetActive(false);
      Transform transform5 = transform1.Find("Spotify");
      if (((UnityEngine.Object) transform5))
        ((Component) transform5).gameObject.SetActive(false);
      Transform transform6 = transform1.Find("MenuTitleText");
      if (((UnityEngine.Object) transform6))
        ((Component) transform6).gameObject.SetActive(false);
      Transform transform7 = transform1.Find("CustomMenuTitleText");
      if (((UnityEngine.Object) transform7))
        ((Component) transform7).gameObject.SetActive(false);
    }
    Transform transform8 = menu.transform.Find("panel1/pageButtons");
    if (((UnityEngine.Object) transform8 != (UnityEngine.Object) null))
    {
      Transform transform9 = transform8.Find("menuUp");
      if (((UnityEngine.Object) transform9))
        ((Component) transform9).gameObject.SetActive(false);
      Transform transform10 = transform8.Find("menuDown");
      if (((UnityEngine.Object) transform10))
        ((Component) transform10).gameObject.SetActive(false);
      for (int index = 1; index <= 5; ++index)
      {
        Transform buttonTransform = transform8.Find("menuBtn" + index.ToString());
        if (((UnityEngine.Object) buttonTransform))
          RemoteModRepresentation.BlankIcon(buttonTransform);
      }
    }
    Transform transform11 = menu.transform.Find("panel1/sidebar");
    if (((UnityEngine.Object) transform11 != (UnityEngine.Object) null))
    {
      for (int index = 1; index <= 6; ++index)
      {
        Transform buttonTransform = transform11.Find("sideBtn" + index.ToString());
        if (((UnityEngine.Object) buttonTransform))
          RemoteModRepresentation.BlankIcon(buttonTransform);
      }
    }
    Transform transform12 = menu.transform.Find("panel2");
    if (!((UnityEngine.Object) transform12 != (UnityEngine.Object) null))
      return;
    foreach (Component component in transform12)
      component.gameObject.SetActive(false);
  }

  private static void BlankIcon(Transform buttonTransform)
  {
    Transform transform = buttonTransform.Find("Icon");
    if (!((UnityEngine.Object) transform != (UnityEngine.Object) null))
      return;
    ((Component) transform).gameObject.SetActive(false);
  }

  private void DestroyScript<T>(GameObject root) where T : Component
  {
    foreach (T componentsInChild in root.GetComponentsInChildren<T>(true))
      UnityEngine.Object.Destroy((UnityEngine.Object) (object) componentsInChild);
  }

  public void UpdateState(
    bool isMenuOpen,
    bool isKeyboardOpen,
    bool isPanel2Open,
    float[] themeColors = null,
    Vector3 menuPosOffset = default (Vector3),
    Vector3 menuRotOffset = default (Vector3))
  {
    this._menuActive = isMenuOpen;
    this._keyboardActive = isKeyboardOpen;
    this._panel2Active = isPanel2Open;
    this._menuPosOffset = menuPosOffset;
    this._menuRotOffset = menuRotOffset;
    if ((themeColors == null ? 0 : (themeColors.Length >= 20 ? 1 : 0)) == 0)
      return;
    this.ApplyThemeColors(new Color(themeColors[0], themeColors[1], themeColors[2], themeColors[3]), new Color(themeColors[4], themeColors[5], themeColors[6], themeColors[7]), new Color(themeColors[8], themeColors[9], themeColors[10], themeColors[11]), new Color(themeColors[12], themeColors[13], themeColors[14], themeColors[15]), new Color(themeColors[16 /*0x10*/], themeColors[17], themeColors[18], themeColors[19]));
  }

  private void CloneThemedMaterials(GameObject root)
  {
    if ((((UnityEngine.Object) root == (UnityEngine.Object) null) ? 1 : (((UnityEngine.Object) UtilMenuMain.Instance == (UnityEngine.Object) null) ? 1 : 0)) != 0)
      return;
    Dictionary<Material, string> dictionary1 = new Dictionary<Material, string>();
    if (((UnityEngine.Object) UtilMenuMain.Instance.buttonMat != (UnityEngine.Object) null))
      dictionary1[UtilMenuMain.Instance.buttonMat] = "BUTTON";
    if (((UnityEngine.Object) UtilMenuMain.Instance.pressedButtonMat != (UnityEngine.Object) null))
      dictionary1[UtilMenuMain.Instance.pressedButtonMat] = "PRESSED";
    if (((UnityEngine.Object) UtilMenuMain.Instance.innerBtnMat != (UnityEngine.Object) null))
      dictionary1[UtilMenuMain.Instance.innerBtnMat] = "INNER";
    if (((UnityEngine.Object) UtilMenuMain.Instance.selectedBtnMat != (UnityEngine.Object) null))
      dictionary1[UtilMenuMain.Instance.selectedBtnMat] = "SELECTED";
    if (((UnityEngine.Object) UtilMenuMain.Instance.panelMat != (UnityEngine.Object) null))
      dictionary1[UtilMenuMain.Instance.panelMat] = "PANEL";
    if (dictionary1.Count == 0)
      return;
    Dictionary<Material, Material> dictionary2 = new Dictionary<Material, Material>();
    foreach (Renderer componentsInChild in root.GetComponentsInChildren<Renderer>(true))
    {
      Material[] sharedMaterials = componentsInChild.sharedMaterials;
      if ((sharedMaterials == null ? 1 : (sharedMaterials.Length == 0 ? 1 : 0)) == 0)
      {
        bool flag = false;
        Material[] materialArray = new Material[sharedMaterials.Length];
        for (int index = 0; index < sharedMaterials.Length; ++index)
        {
          Material key = sharedMaterials[index];
          string str = default;
          if ((!((UnityEngine.Object) key != (UnityEngine.Object) null) ? 0 : (dictionary1.TryGetValue(key, out str) ? 1 : 0)) != 0)
          {
            Material material;
            if (!dictionary2.TryGetValue(key, out material))
            {
              material = new Material(key);
              dictionary2[key] = material;
              this._themedMaterials.Add((str, material));
            }
            materialArray[index] = material;
            flag = true;
          }
          else
            materialArray[index] = key;
        }
        if (flag)
          componentsInChild.sharedMaterials = materialArray;
      }
    }
  }

  public void ApplyThemeColors(Color btn, Color prs, Color inr, Color sel, Color pnl)
  {
    for (int index = 0; index < this._themedMaterials.Count; ++index)
    {
      (string slot, Material mat) = this._themedMaterials[index];
      if (!((UnityEngine.Object) mat == (UnityEngine.Object) null))
      {
        switch (slot)
        {
          case "BUTTON":
            mat.color = btn;
            continue;
          case "PRESSED":
            mat.color = prs;
            continue;
          case "INNER":
            mat.color = inr;
            continue;
          case "SELECTED":
            mat.color = sel;
            continue;
          case "PANEL":
            mat.color = pnl;
            continue;
          default:
            continue;
        }
      }
    }
  }

  private void OnDestroy()
  {
    for (int index = 0; index < this._themedMaterials.Count; ++index)
    {
      Material mat = this._themedMaterials[index].mat;
      if (((UnityEngine.Object) mat != (UnityEngine.Object) null))
        UnityEngine.Object.Destroy((UnityEngine.Object) mat);
    }
    this._themedMaterials.Clear();
    if (!((UnityEngine.Object) this.FakeMenu != (UnityEngine.Object) null))
      return;
    UnityEngine.Object.Destroy((UnityEngine.Object) this.FakeMenu);
  }

  public void UnbindOwner()
  {
    this._owner = (Player) null;
    this._ownerRig = (VRRig) null;
    if ((!((UnityEngine.Object) this.FakeMenu != (UnityEngine.Object) null) ? 0 : (((UnityEngine.Object) this.FakeMenu.transform.parent != (UnityEngine.Object) ((Component) this).transform) ? 1 : 0)) != 0)
      this.FakeMenu.transform.SetParent(((Component) this).transform, false);
    if (((UnityEngine.Object) this.FakeMenu != (UnityEngine.Object) null))
      this.FakeMenu.SetActive(false);
    this._menuActive = false;
    this._keyboardActive = false;
    this._panel2Active = false;
    ((Component) this).gameObject.SetActive(false);
  }

  public void Rebind(Player newOwner)
  {
    this._owner = newOwner;
    this._ownerRig = (VRRig) null;
    ((Component) this).gameObject.SetActive(true);
    GameObject gameObject = ((Component) this).gameObject;
    string str1;
    if (newOwner == null)
    {
      str1 = (string) null;
    }
    else
    {
      str1 = newOwner.NickName;
      if (str1 != null)
        goto label_4;
    }
    str1 = "?";
label_4:
    string str2 = "RemoteMod_" + str1;
    ((UnityEngine.Object) gameObject).name = str2;
  }

  private Transform GetOwnerLeftHand()
  {
    if ((!((UnityEngine.Object) this._ownerRig == (UnityEngine.Object) null) ? 0 : (this._owner != null ? 1 : 0)) != 0)
      this._ownerRig = PlayerTranslator.GetRigByNetPlayer(PlayerTranslator.GetNetPlayerByPhoton(this._owner));
    return ((UnityEngine.Object) this._ownerRig != (UnityEngine.Object) null) ? this._ownerRig.leftHandTransform : (Transform) null;
  }

  private void Update()
  {
    if (!this._initializeComplete || !((UnityEngine.Object) this.FakeMenu != (UnityEngine.Object) null))
      return;
    Transform ownerLeftHand = this.GetOwnerLeftHand();
    if ((!((UnityEngine.Object) ownerLeftHand != (UnityEngine.Object) null) ? 0 : (((UnityEngine.Object) this.FakeMenu.transform.parent != (UnityEngine.Object) ownerLeftHand) ? 1 : 0)) == 0)
    {
      if ((!((UnityEngine.Object) ownerLeftHand != (UnityEngine.Object) null) ? 0 : ((this._menuPosOffset != this._lastAppliedPosOffset) ? 1 : ((this._menuRotOffset != this._lastAppliedRotOffset) ? 1 : 0))) != 0)
      {
        this.ApplyMenuLocalPose();
        this._lastAppliedPosOffset = this._menuPosOffset;
        this._lastAppliedRotOffset = this._menuRotOffset;
      }
    }
    else
    {
      this.FakeMenu.transform.SetParent(ownerLeftHand, false);
      this.ApplyMenuLocalPose();
      this._lastAppliedPosOffset = this._menuPosOffset;
      this._lastAppliedRotOffset = this._menuRotOffset;
    }
    bool flag1;
    if (!(flag1 = this._menuActive && RemoteModRepresentation.ShowOthersUtilMenu && ((UnityEngine.Object) ownerLeftHand != (UnityEngine.Object) null)))
    {
      if (this.FakeMenu.activeSelf)
        this.FakeMenu.SetActive(false);
    }
    else if (!this.FakeMenu.activeSelf)
      this.FakeMenu.SetActive(true);
    bool flag2 = flag1 && this._keyboardActive;
    if ((!((UnityEngine.Object) this._fakeKeyboard != (UnityEngine.Object) null) ? 0 : (this._fakeKeyboard.activeSelf != flag2 ? 1 : 0)) != 0)
      this._fakeKeyboard.SetActive(flag2);
    bool flag3 = flag1 && this._panel2Active;
    if ((!((UnityEngine.Object) this._fakePanel2 != (UnityEngine.Object) null) ? 0 : (this._fakePanel2.activeSelf != flag3 ? 1 : 0)) == 0)
      return;
    this._fakePanel2.SetActive(flag3);
  }

  private void ApplyMenuLocalPose()
  {
    if (((UnityEngine.Object) this.FakeMenu == (UnityEngine.Object) null))
      return;
    this.FakeMenu.transform.localPosition = (RemoteModRepresentation.MenuLocalPos + this._menuPosOffset);
    this.FakeMenu.transform.localRotation = (RemoteModRepresentation.MenuLocalRot * Quaternion.Euler(this._menuRotOffset));
  }

  private void CleanupColliders(GameObject obj)
  {
    foreach (Object componentsInChild in obj.GetComponentsInChildren<Collider>())
      UnityEngine.Object.Destroy(componentsInChild);
  }
}
