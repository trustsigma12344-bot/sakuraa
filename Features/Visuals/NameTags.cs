using SakuraaCastingMod.Core;
using SakuraaCastingMod.Desktop.Camera;
using SakuraaCastingMod.Shared.Helpers;
using SakuraaCastingMod.Shared.Models;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
namespace SakuraaCastingMod.Features.Visuals;

public static class NameTags
{
  [SavedSetting("NameTagsEnabled", false)]
  public static bool NameTagsEnabled;
  [SavedSetting("NameTagSelfVisible", false)]
  public static bool SelfNameTagVisible;
  [SavedSetting("NameTagFont", "default")]
  public static string CurrentNameFont = "default";
  [SavedSetting("HideSpecTag", true)]
  public static bool HideSpecTagEnabled = true;
  [SavedSetting("HideSpecTagFPOnly", true)]
  public static bool HideSpecTagFirstPersonOnly = true;
  [SavedSetting("NameTagScale", 0.6f)]
  public static float NameTagScale = 0.6f;
  [SavedSetting("NameTagPosition", 0.5f)]
  public static float NameTagPosition = 0.5f;
  public static GameObject NameTag;
  private static readonly Dictionary<string, NameTags.NameTagReferences> NameTagDict = new Dictionary<string, NameTags.NameTagReferences>();
  private static readonly FieldInfo _fpsField = typeof (VRRig).GetField("fps", BindingFlags.Instance | BindingFlags.NonPublic);
  private static float _nextDataUpdateTime;
  private const float DataUpdateInterval = 0.25f;
  private static Shader _cachedUberShader;
  private static readonly HashSet<string> _liveTagIds = new HashSet<string>();
  private static readonly List<string> _staleTagIds = new List<string>();
  private static readonly Color Color150Hz = new Color(0.0f, 1f, 0.8f);
  private static readonly Color Color50Hz = new Color(0.0f, 1f, 0.09f);
  private static readonly Color Color25Hz = new Color(1f, 0.65f, 0.0f);
  private static readonly Color ColorLowHz = new Color(1f, 0.157f, 0.0f);

  public static void LateUpdate()
  {
    if (NameTags.NameTagsEnabled)
    {
      if (NameTags.NameTagDict.Count > 0)
      {
        NameTags._liveTagIds.Clear();
        List<GorillaData> gorillaDataList = GorillaDataHandler.GorillaDataList;
        for (int index = 0; index < gorillaDataList.Count; ++index)
        {
          GorillaData gorillaData = gorillaDataList[index];
          if ((gorillaData == null ? 0 : (!string.IsNullOrEmpty(gorillaData.UserId) ? 1 : 0)) != 0)
            NameTags._liveTagIds.Add(gorillaData.UserId);
        }
        NameTags._staleTagIds.Clear();
        foreach (KeyValuePair<string, NameTags.NameTagReferences> keyValuePair in NameTags.NameTagDict)
        {
          if (!NameTags._liveTagIds.Contains(keyValuePair.Key))
            NameTags._staleTagIds.Add(keyValuePair.Key);
        }
        for (int index = 0; index < NameTags._staleTagIds.Count; ++index)
        {
          string staleTagId = NameTags._staleTagIds[index];
          NameTags.NameTagReferences nameTagReferences;
          if (NameTags.NameTagDict.TryGetValue(staleTagId, out nameTagReferences))
          {
            if (((UnityEngine.Object) nameTagReferences?.MainObject != (UnityEngine.Object) null))
              UnityEngine.Object.Destroy((UnityEngine.Object) nameTagReferences.MainObject);
            NameTags.NameTagDict.Remove(staleTagId);
          }
        }
      }
      bool flag1;
      if (flag1 = (double) Time.time >= (double) NameTags._nextDataUpdateTime)
        NameTags._nextDataUpdateTime = Time.time + 0.25f;
      List<GorillaData> gorillaDataList1 = GorillaDataHandler.GorillaDataList;
      Transform transform = ((Component) Plugin.Ins.camera).transform;
      if ((((UnityEngine.Object) NetworkSystem.Instance == (UnityEngine.Object) null) ? 1 : (NetworkSystem.Instance.LocalPlayer == null ? 1 : 0)) != 0)
        return;
      string userId1 = NetworkSystem.Instance.LocalPlayer.UserId;
      string userId2 = (!NameTags.HideSpecTagEnabled ? 0 : (!NameTags.HideSpecTagFirstPersonOnly ? 1 : (PlayerSpec.SpecPart == "eyes" ? 1 : 0))) == 0 || PlayerSpec.SpecGorilla < 0 || PlayerSpec.SpecGorilla >= gorillaDataList1.Count ? (string) null : gorillaDataList1[PlayerSpec.SpecGorilla]?.UserId;
      foreach (GorillaData gorilla in gorillaDataList1)
      {
        bool flag2 = gorilla.UserId == userId1;
        if ((Networking.InRoom ? 0 : (!flag2 ? 1 : 0)) == 0 && !(!NameTags.SelfNameTagVisible & flag2))
        {
          if ((userId2 == null ? 0 : (gorilla.UserId == userId2 ? 1 : 0)) == 0)
          {
            NameTags.NameTagReferences refs;
            if (NameTags.NameTagDict.TryGetValue(gorilla.UserId, out refs))
            {
              if (!((UnityEngine.Object) refs.MainObject == (UnityEngine.Object) null))
              {
                NameTags.UpdateTransform(gorilla, refs, transform);
                if ((flag1 ? 1 : (((TMP_Text) refs.NameText).text != gorilla.UserName ? 1 : 0)) != 0)
                  NameTags.UpdateVisualData(gorilla, refs);
              }
            }
            else
              NameTags.CreateNameTag(gorilla);
          }
          else
          {
            NameTags.NameTagReferences nameTagReferences;
            if ((!NameTags.NameTagDict.TryGetValue(gorilla.UserId, out nameTagReferences) ? 0 : (((UnityEngine.Object) nameTagReferences.MainObject != (UnityEngine.Object) null) ? 1 : 0)) != 0)
              nameTagReferences.MainObject.SetActive(false);
          }
        }
      }
    }
    else
    {
      if (NameTags.NameTagDict.Count <= 0)
        return;
      NameTags.ClearNameTags();
    }
  }

  private static void UpdateTransform(
    GorillaData gorilla,
    NameTags.NameTagReferences refs,
    Transform camTransform)
  {
    refs.MainObject.SetActive(true);
    Vector3 position = gorilla.BodyTransform.position;
    position.y += NameTags.NameTagPosition;
    refs.MainTransform.position = position;
    refs.MainTransform.LookAt(camTransform);
  }

  private static void UpdateVisualData(GorillaData gorilla, NameTags.NameTagReferences refs)
  {
    if (((TMP_Text) refs.NameText).text != gorilla.UserName)
      ((TMP_Text) refs.NameText).text = gorilla.UserName;
    ((Graphic) refs.NameText).color = gorilla.Color;
    TextMeshPro nameText = refs.NameText;
    TMP_FontAsset tmpFontAsset;
    switch (NameTags.CurrentNameFont)
    {
      case "default":
        tmpFontAsset = Plugin.Ins.defaultFontAsset;
        break;
      case "gtag":
        tmpFontAsset = Plugin.Ins.gtagFontAsset;
        break;
      case "pixel":
        tmpFontAsset = Plugin.Ins.pixelFontAsset;
        break;
      case "gotham":
        tmpFontAsset = Plugin.Ins.gothamFontAsset;
        break;
      case "designer":
        tmpFontAsset = Plugin.Ins.designerFontAsset;
        break;
      default:
        tmpFontAsset = ((TMP_Text) refs.NameText).font;
        break;
    }
    ((TMP_Text) nameText).font = tmpFontAsset;
    if ((!RankVisuals.ShowHzOnName ? 0 : (!Plugin.ReplayCompatibilityMode ? 1 : 0)) == 0)
    {
      if (((TMP_Text) refs.HzText).text != "")
        ((TMP_Text) refs.HzText).text = "";
    }
    else
    {
      VRRig rigByGorilla = PlayerTranslator.GetRigByGorilla(gorilla);
      if ((!((UnityEngine.Object) rigByGorilla != (UnityEngine.Object) null) ? 0 : (NameTags._fpsField != (FieldInfo) null ? 1 : 0)) != 0)
      {
        int num = (int) NameTags._fpsField.GetValue((object) rigByGorilla);
        ((TMP_Text) refs.HzText).text = num.ToString() + "hz";
        if (!RankVisuals.AutoColorHzTag)
          ((Graphic) refs.HzText).color = gorilla.Color;
        else if (num < 150)
        {
          if (num <= 50)
          {
            if (num <= 25)
              ((Graphic) refs.HzText).color = NameTags.ColorLowHz;
            else
              ((Graphic) refs.HzText).color = NameTags.Color25Hz;
          }
          else
            ((Graphic) refs.HzText).color = NameTags.Color50Hz;
        }
        else
          ((Graphic) refs.HzText).color = NameTags.Color150Hz;
      }
    }
    NameTags.UpdatePlatformIcons(gorilla, refs);
  }

  private static void UpdatePlatformIcons(GorillaData gorilla, NameTags.NameTagReferences refs)
  {
    bool flag1 = false;
    byte tier = default;
    if ((!RankVisuals.RankCheckOnNamesEnabled || Plugin.ReplayCompatibilityMode || !((UnityEngine.Object) SecureNetworkManager.Instance != (UnityEngine.Object) null) || !((UnityEngine.Object) refs.RankIconPlaneRenderer != (UnityEngine.Object) null) || string.IsNullOrEmpty(gorilla.UserId) ? 0 : (SecureNetworkManager.Instance.TryGetRemoteTier(gorilla.UserId, out tier) ? 1 : 0)) != 0)
    {
      Material matForTier = RankVisuals.GetMatForTier(tier);
      if (((UnityEngine.Object) matForTier != (UnityEngine.Object) null))
      {
        ((Renderer) refs.RankIconPlaneRenderer).sharedMaterial = matForTier;
        flag1 = true;
      }
    }
    if ((!((UnityEngine.Object) refs.RankIcon != (UnityEngine.Object) null) ? 0 : (refs.RankIcon.activeSelf != flag1 ? 1 : 0)) != 0)
      refs.RankIcon.SetActive(flag1);
    if ((!RankVisuals.PlatCheckOnNamesEnabled || Plugin.ReplayCompatibilityMode ? 0 : (Plugin.Ins.XPosition != 0 ? 1 : 0)) == 0)
    {
      if (refs.SteamIcon.activeSelf)
        refs.SteamIcon.SetActive(false);
      if (!refs.MetaIcon.activeSelf)
        return;
      refs.MetaIcon.SetActive(false);
    }
    else
    {
      VRRig rigByGorilla = PlayerTranslator.GetRigByGorilla(gorilla);
      bool flag2 = false;
      bool flag3 = false;
      string str = default;
      if ((!((UnityEngine.Object) rigByGorilla != (UnityEngine.Object) null) ? 0 : (RankVisuals.PlayerPlatforms.TryGetValue(rigByGorilla, out str) ? 1 : 0)) != 0)
      {
        switch (str)
        {
          case "steam":
            flag2 = true;
            break;
          case "oculus":
            flag3 = true;
            break;
        }
      }
      if (refs.SteamIcon.activeSelf != flag2)
        refs.SteamIcon.SetActive(flag2);
      if (refs.MetaIcon.activeSelf != flag3)
        refs.MetaIcon.SetActive(flag3);
      if (!(flag2 | flag3))
        return;
      float num = Mathf.Min(16f, NameTags.GetNameWidthEstimate(gorilla.UserName.Length));
      Vector3 vector3;
      // ISSUE: explicit constructor call
      vector3 = new Vector3(num, 0.0f, 0.0f);
      if (flag2)
        refs.SteamIcon.transform.localPosition = vector3;
      if (!flag3)
        return;
      refs.MetaIcon.transform.localPosition = vector3;
    }
  }

  private static float GetNameWidthEstimate(int length)
  {
    float nameWidthEstimate;
    switch (length)
    {
      case 1:
        nameWidthEstimate = 4f;
        break;
      case 2:
        nameWidthEstimate = 5f;
        break;
      case 3:
        nameWidthEstimate = 8f;
        break;
      case 4:
        nameWidthEstimate = 10f;
        break;
      case 5:
        nameWidthEstimate = 12f;
        break;
      case 6:
        nameWidthEstimate = 14f;
        break;
      default:
        nameWidthEstimate = 16f;
        break;
    }
    return nameWidthEstimate;
  }

  public static void SwitchNameFont()
  {
    string str;
    switch (NameTags.CurrentNameFont)
    {
      case "default":
        str = "gtag";
        break;
      case "gtag":
        str = "pixel";
        break;
      case "pixel":
        str = "gotham";
        break;
      case "gotham":
        str = "designer";
        break;
      case "designer":
        str = "default";
        break;
      default:
        str = NameTags.CurrentNameFont;
        break;
    }
    NameTags.CurrentNameFont = str;
  }

  public static void ToggleNameTagVisibility()
  {
    NameTags.NameTagsEnabled = !NameTags.NameTagsEnabled;
    NameTags.ClearNameTags();
  }

  public static void ClearNameTags()
  {
    foreach (KeyValuePair<string, NameTags.NameTagReferences> keyValuePair in NameTags.NameTagDict)
    {
      if ((keyValuePair.Value == null ? 0 : (((UnityEngine.Object) keyValuePair.Value.MainObject != (UnityEngine.Object) null) ? 1 : 0)) != 0)
        UnityEngine.Object.Destroy((UnityEngine.Object) keyValuePair.Value.MainObject);
    }
    NameTags.NameTagDict.Clear();
  }

  public static void RemoveNameTagFor(string userId)
  {
    NameTags.NameTagReferences nameTagReferences;
    if (string.IsNullOrEmpty(userId) || !NameTags.NameTagDict.TryGetValue(userId, out nameTagReferences))
      return;
    if ((nameTagReferences == null ? 0 : (((UnityEngine.Object) nameTagReferences.MainObject != (UnityEngine.Object) null) ? 1 : 0)) != 0)
      UnityEngine.Object.Destroy((UnityEngine.Object) nameTagReferences.MainObject);
    NameTags.NameTagDict.Remove(userId);
  }

  private static void CreateNameTag(GorillaData gorilla)
  {
    GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(NameTags.NameTag);
    NameTags.NameTagReferences nameTagReferences = new NameTags.NameTagReferences();
    nameTagReferences.MainObject = gameObject;
    nameTagReferences.MainTransform = gameObject.transform;
    Transform transform1 = gameObject.transform.Find("NameTagTMP");
    if (((UnityEngine.Object) transform1))
    {
      nameTagReferences.NameText = ((Component) transform1).GetComponent<TextMeshPro>();
      MeshRenderer component = ((Component) transform1).GetComponent<MeshRenderer>();
      if (((UnityEngine.Object) component))
      {
        if (((UnityEngine.Object) NameTags._cachedUberShader == (UnityEngine.Object) null))
          NameTags._cachedUberShader = Shader.Find("GorillaTag/UberShader");
        if (((UnityEngine.Object) NameTags._cachedUberShader != (UnityEngine.Object) null))
          ((Renderer) component).material.shader = NameTags._cachedUberShader;
      }
    }
    Transform transform2 = gameObject.transform.Find("hzTag");
    if (((UnityEngine.Object) transform2))
    {
      nameTagReferences.HzTagObject = ((Component) transform2).gameObject;
      nameTagReferences.HzText = ((Component) transform2).GetComponent<TextMeshPro>();
    }
    Transform transform3 = gameObject.transform.Find("rankIcon");
    if (((UnityEngine.Object) transform3))
    {
      nameTagReferences.RankIcon = ((Component) transform3).gameObject;
      Transform transform4 = transform3.Find("Plane");
      if (((UnityEngine.Object) transform4))
        nameTagReferences.RankIconPlaneRenderer = ((Component) transform4).GetComponent<MeshRenderer>();
    }
    Transform transform5 = gameObject.transform.Find("steam");
    if (((UnityEngine.Object) transform5))
      nameTagReferences.SteamIcon = ((Component) transform5).gameObject;
    Transform transform6 = gameObject.transform.Find("meta");
    if (((UnityEngine.Object) transform6))
      nameTagReferences.MetaIcon = ((Component) transform6).gameObject;
    Transform transform7 = gameObject.transform;
    transform7.localScale = (transform7.localScale * 0.065f * NameTags.NameTagScale);
    gameObject.transform.parent = (Transform) null;
    NameTags.NameTagDict.Add(gorilla.UserId, nameTagReferences);
    gameObject.SetActive(false);
  }

  private class NameTagReferences
  {
    public GameObject MainObject;
    public Transform MainTransform;
    public TextMeshPro NameText;
    public TextMeshPro HzText;
    public GameObject HzTagObject;
    public GameObject RankIcon;
    public MeshRenderer RankIconPlaneRenderer;
    public GameObject SteamIcon;
    public GameObject MetaIcon;
  }
}
