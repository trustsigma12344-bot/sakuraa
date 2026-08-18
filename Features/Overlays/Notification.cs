using SakuraaCastingMod.VR.UtilMenu;
using SakuraaOfflinePresets;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

#nullable disable
namespace SakuraaCastingMod.Features.Overlays;

public static class Notification
{
  public const int VrHudLayer = 19;
  private const float PcNotificationDuration = 3f;
  private const int PcTextSize = 20;
  private const float PcMargin = 10f;
  private const float PcPanelPad = 9f;
  private const float PcPanelAlpha = 0.47f;
  private const float PcLeftColWidth = 33f;
  private const float PcDiamondSize = 31f;
  private const float PcDiamondOffX = 15f;
  private const float PcIconSize = 26.5f;
  private const float PcMinPanelH = 39f;
  private const float PcTextClear = 6f;
  private static float _pcNotifierCountdown;
  private static string _pcNotification = "";
  private static Color _pcNotificationColor;
  private static Texture _pcIcon;
  private static GameObject _mainCamera;
  private static GameObject _hudObj;
  private static GameObject _hudObj2;
  private static RectTransform _vrStack;
  private static Font _vrFont;
  private static Material _vrTextMat;
  private static Material _vrPanelMat;
  public static float VrDecayTime = 3f;
  public static int VrFontSize = 40;
  public static bool VrEnabled = true;
  private const int MaxVrCards = 5;
  private const float EnterDuration = 0.45f;
  private const float ExitDuration = 0.3f;
  private const float EnterFade = 0.12f;
  private const float OvershootPx = 26f;
  private static readonly List<Notification.VrCard> _vrCards = new List<Notification.VrCard>();
  private static bool _vrHasInit = false;
  private static Notification.NotificationHost _host;
  private static Rect _lastCardRect;

  private static float SweepIn(float t, float from, float to, float overshoot)
  {
    float num1;
    if ((double) t < 0.699999988079071)
    {
      float num2 = 1f - Mathf.Pow(1f - t / 0.7f, 3f);
      num1 = Mathf.Lerp(from, to + overshoot, num2);
    }
    else
    {
      float num3 = (float) (((double) t - 0.699999988079071) / 0.30000001192092896);
      float num4 = (float) ((double) num3 * (double) num3 * (3.0 - 2.0 * (double) num3));
      num1 = Mathf.Lerp(to + overshoot, to, num4);
    }
    return num1;
  }

  private static void EnsureHost()
  {
    if (!((UnityEngine.Object) Notification._host == (UnityEngine.Object) null))
      return;
    GameObject gameObject = new GameObject("SakuraaNotificationHost");
    UnityEngine.Object.DontDestroyOnLoad((UnityEngine.Object) gameObject);
    Notification._host = gameObject.AddComponent<Notification.NotificationHost>();
  }

  public static void Send(string message, Color color, Material icon = null)
  {
    Texture icon1 = Notification.ResolveIconTexture(icon);
    Notification.SendPc(Notification.StripRichTags(message), color, icon1);
    if (!XRSettings.isDeviceActive)
      return;
    Notification.SendVrCard(message, color, icon1);
  }

  public static void Send(string message, Material icon = null)
  {
    Notification.Send(message, Color.white, icon);
  }

  private static string StripRichTags(string message)
  {
    return message.Contains("<") ? Regex.Replace(message, "<[^>]*>", "") : message;
  }

  private static Texture ResolveIconTexture(Material icon)
  {
    Material material = icon;
    if ((!((UnityEngine.Object) material == (UnityEngine.Object) null) || !((UnityEngine.Object) UtilMenuMain.Instance != (UnityEngine.Object) null) ? 0 : (UtilMenuMain.Instance.Icons != null ? 1 : 0)) != 0)
      material = UtilMenuMain.Instance.Icons.RingingBell;
    return ((UnityEngine.Object) material != (UnityEngine.Object) null) ? material.mainTexture : (Texture) null;
  }

  private static void SendPc(string message, Color color, Texture icon)
  {
    Notification.EnsureHost();
    Notification._pcNotification = message;
    Notification._pcNotificationColor = color;
    Notification._pcIcon = icon;
    Notification._pcNotifierCountdown = 3f;
  }

  public static void SetFontSize(int size) => Notification.VrFontSize = size;

  public static void ClearVrNotifications()
  {
    foreach (Notification.VrCard vrCard in Notification._vrCards)
    {
      if (((UnityEngine.Object) vrCard.Root != (UnityEngine.Object) null))
        UnityEngine.Object.Destroy((UnityEngine.Object) vrCard.Root);
    }
    Notification._vrCards.Clear();
  }

  public static void UpdateDuration()
  {
    if ((double) Notification._pcNotifierCountdown <= 0.0)
      return;
    Notification._pcNotifierCountdown -= Time.deltaTime;
    if ((double) Notification._pcNotifierCountdown > 0.0)
      return;
    Notification._pcNotification = "";
  }

  public static Rect GetDisplayRect()
  {
    return (double) Notification._lastCardRect.width > 0.0 ? Notification._lastCardRect : new Rect(20f + LayoutEditor.NotificationPosX, (float) ((double) Screen.height - 20.0 - 78.0) + LayoutEditor.NotificationPosY, 620f, 78f);
  }

  public static void DrawNotifier()
  {
    OfflinePresetStore.PrepareNotificationLayout();
    bool isEditing = LayoutEditor.IsEditing;
    string str = Notification._pcNotification;
    Color color1 = Notification._pcNotificationColor;
    Texture texture = Notification._pcIcon;
    if (string.IsNullOrEmpty(str))
    {
      if (!isEditing)
        return;
      str = "Notification preview";
      color1 = Color.cyan;
      texture = Notification.ResolveIconTexture((Material) null);
    }
    float num1 = 3f - Notification._pcNotifierCountdown;
    float t = isEditing ? 1f : Mathf.Clamp01(num1 / 0.45f);
    float num2 = isEditing ? 0.0f : 1f - Mathf.Clamp01(Notification._pcNotifierCountdown / 0.3f);
    float num3 = isEditing ? 1f : Mathf.Min(Mathf.Clamp01(num1 / 0.12f), 1f - num2);
    GUIStyle guiStyle = new GUIStyle()
    {
      fontSize = 20,
      alignment = (TextAnchor) 3,
      wordWrap = false,
      normal = {
        textColor = new Color(1f, 1f, 1f, num3)
      }
    };
    Vector2 vector2_1 = guiStyle.CalcSize(new GUIContent(str));
    float num4 = Mathf.Max(vector2_1.y + 18f, 39f);
    float num5 = 33f;
    float num6 = 63f;
    float num7 = Mathf.Max(84f, 119.020004f);
    float num8 = (float) ((double) num7 - (double) num5 + (double) vector2_1.x + 9.0);
    float num9 = num5 + num8;
    float to = 10f + LayoutEditor.NotificationPosX;
    float num10 = Notification.SweepIn(t, (float) (-(double) num9 - 6.0), to, 26f);
    if ((double) num2 > 0.0)
      num10 = Mathf.Lerp(to, (float) (-(double) num9 - 6.0), num2 * num2 * num2);
    float num11 = (float) Screen.height - 10f - num4 + LayoutEditor.NotificationPosY;
    Notification._lastCardRect = new Rect(num10, num11, num9, num4);
    Color color2 = GUI.color;
    Rect rect;
    // ISSUE: explicit constructor call
    rect = new Rect(num10 + num5, num11, num8, num4);
    GUI.color = new Color(0.0f, 0.0f, 0.0f, 0.47f * num3);
    GUI.DrawTexture(rect, (Texture) Texture2D.whiteTexture);
    Vector2 vector2_2;
    // ISSUE: explicit constructor call
    vector2_2 = new Vector2(num10 + num6, num11 + num4 / 2f);
    Matrix4x4 matrix = GUI.matrix;
    GUIUtility.RotateAroundPivot(45f, vector2_2);
    GUI.color = new Color(color1.r, color1.g, color1.b, color1.a * num3);
    GUI.DrawTexture(new Rect(vector2_2.x - 15.5f, vector2_2.y - 15.5f, 31f, 31f), (Texture) Texture2D.whiteTexture);
    GUI.matrix = matrix;
    if (((UnityEngine.Object) texture != (UnityEngine.Object) null))
    {
      GUI.color = new Color(1f, 1f, 1f, num3);
      GUI.DrawTexture(new Rect(vector2_2.x - 13.25f, vector2_2.y - 13.25f, 26.5f, 26.5f), texture, (ScaleMode) 2, true);
    }
    GUI.color = color2;
    GUI.Label(new Rect(num10 + num7, rect.y, vector2_1.x, num4), str, guiStyle);
  }

  private static void SendVrCard(string message, Color diamondColor, Texture icon)
  {
    Notification.EnsureHost();
    Notification.EnsureVrInit();
    if ((!Notification.VrEnabled || !Notification._vrHasInit ? 1 : (((UnityEngine.Object) Notification._vrStack == (UnityEngine.Object) null) ? 1 : 0)) != 0)
      return;
    float num1 = (float) Notification.VrFontSize / 39f;
    float num2 = 18f * num1;
    float num3 = 66f * num1;
    float num4 = 62f * num1;
    float num5 = 53f * num1;
    float num6 = 63f * num1;
    float num7 = num6 + num4 * 0.71f;
    float num8 = Mathf.Max(num3 + num2, num7 + 12f * num1);
    float num9 = 780f * num1;
    GameObject parent1 = new GameObject("NotificationCard")
    {
      layer = 19
    };
    RectTransform rectTransform1 = parent1.AddComponent<RectTransform>();
    ((Transform) rectTransform1).SetParent((Transform) Notification._vrStack, false);
    RectTransform rectTransform2 = rectTransform1;
    RectTransform rectTransform3 = rectTransform1;
    Vector2 vector2_1;
    // ISSUE: explicit constructor call
    vector2_1 = new Vector2(0.5f, 0.0f);
    Vector2 vector2_2 = vector2_1;
    rectTransform3.anchorMax = vector2_2;
    Vector2 vector2_3 = vector2_1;
    rectTransform2.anchorMin = vector2_3;
    rectTransform1.pivot = new Vector2(0.5f, 0.0f);
    CanvasGroup canvasGroup = parent1.AddComponent<CanvasGroup>();
    canvasGroup.alpha = 0.0f;
    GameObject parent2 = Notification.NewUiChild(parent1, "Panel");
    RectTransform component1 = parent2.GetComponent<RectTransform>();
    component1.anchorMin = Vector2.zero;
    component1.anchorMax = Vector2.one;
    component1.offsetMin = new Vector2(num3, 0.0f);
    component1.offsetMax = Vector2.zero;
    RawImage rawImage1 = parent2.AddComponent<RawImage>();
    rawImage1.texture = (Texture) Texture2D.whiteTexture;
    ((Graphic) rawImage1).color = new Color(0.0f, 0.0f, 0.0f, 0.47f);
    ((Graphic) rawImage1).material = Notification._vrPanelMat;
    GameObject gameObject1 = Notification.NewUiChild(parent2, "Text");
    RectTransform component2 = gameObject1.GetComponent<RectTransform>();
    component2.anchorMin = Vector2.zero;
    component2.anchorMax = Vector2.one;
    UnityEngine.UI.Text text = gameObject1.AddComponent<UnityEngine.UI.Text>();
    text.font = Notification._vrFont;
    text.fontSize = Notification.VrFontSize;
    ((Graphic) text).color = Color.white;
    text.alignment = (TextAnchor) 3;
    text.horizontalOverflow = (HorizontalWrapMode) 1;
    text.verticalOverflow = (VerticalWrapMode) 1;
    ((Graphic) text).material = Notification._vrTextMat;
    text.text = message;
    float num10 = Mathf.Min(text.preferredWidth, num9);
    if ((double) text.preferredWidth > (double) num9)
      text.horizontalOverflow = (HorizontalWrapMode) 0;
    float num11 = num8 + num10 + num2;
    rectTransform1.sizeDelta = new Vector2(num11, 10f);
    component2.offsetMin = new Vector2(num8 - num3, 0.0f);
    component2.offsetMax = new Vector2(-num2, 0.0f);
    float num12 = Mathf.Max(text.preferredHeight + num2 * 2f, 78f * num1);
    rectTransform1.sizeDelta = new Vector2(num11, num12);
    GameObject gameObject2 = Notification.NewUiChild(parent1, "Diamond");
    RectTransform component3 = gameObject2.GetComponent<RectTransform>();
    RectTransform rectTransform4 = component3;
    RectTransform rectTransform5 = component3;
    // ISSUE: explicit constructor call
    vector2_1 = new Vector2(0.0f, 0.5f);
    Vector2 vector2_4 = vector2_1;
    rectTransform5.anchorMax = vector2_4;
    Vector2 vector2_5 = vector2_1;
    rectTransform4.anchorMin = vector2_5;
    component3.pivot = new Vector2(0.5f, 0.5f);
    component3.anchoredPosition = new Vector2(num6, 0.0f);
    component3.sizeDelta = new Vector2(num4, num4);
    ((Transform) component3).localRotation = Quaternion.Euler(0.0f, 0.0f, 45f);
    RawImage rawImage2 = gameObject2.AddComponent<RawImage>();
    rawImage2.texture = (Texture) Texture2D.whiteTexture;
    ((Graphic) rawImage2).color = diamondColor;
    ((Graphic) rawImage2).material = Notification._vrPanelMat;
    if (((UnityEngine.Object) icon != (UnityEngine.Object) null))
    {
      GameObject gameObject3 = Notification.NewUiChild(parent1, "Icon");
      RectTransform component4 = gameObject3.GetComponent<RectTransform>();
      RectTransform rectTransform6 = component4;
      RectTransform rectTransform7 = component4;
      // ISSUE: explicit constructor call
      vector2_1 = new Vector2(0.0f, 0.5f);
      Vector2 vector2_6 = vector2_1;
      rectTransform7.anchorMax = vector2_6;
      Vector2 vector2_7 = vector2_1;
      rectTransform6.anchorMin = vector2_7;
      component4.pivot = new Vector2(0.5f, 0.5f);
      component4.anchoredPosition = new Vector2(num6, 0.0f);
      component4.sizeDelta = new Vector2(num5, num5);
      RawImage rawImage3 = gameObject3.AddComponent<RawImage>();
      rawImage3.texture = icon;
      ((Graphic) rawImage3).material = Notification._vrPanelMat;
    }
    Notification._vrCards.Add(new Notification.VrCard()
    {
      Root = parent1,
      Rt = rectTransform1,
      Group = canvasGroup,
      SpawnTime = Time.time,
      Expiry = Time.time + Notification.VrDecayTime,
      Height = num12,
      Width = num11,
      Scale = num1
    });
    while (Notification._vrCards.Count > 5)
    {
      if (((UnityEngine.Object) Notification._vrCards[0].Root != (UnityEngine.Object) null))
        UnityEngine.Object.Destroy((UnityEngine.Object) Notification._vrCards[0].Root);
      Notification._vrCards.RemoveAt(0);
    }
    Notification.RecomputeVrTargets();
    Notification.VrCard vrCard = Notification._vrCards[Notification._vrCards.Count - 1];
    vrCard.Rt.anchoredPosition = new Vector2(-Notification.VrSweepTravel(vrCard), vrCard.TargetY);
  }

  private static GameObject NewUiChild(GameObject parent, string name)
  {
    GameObject gameObject = new GameObject(name)
    {
      layer = 19
    };
    ((Transform) gameObject.AddComponent<RectTransform>()).SetParent(parent.transform, false);
    return gameObject;
  }

  private static float VrSweepTravel(Notification.VrCard c)
  {
    return (float) ((double) c.Width * 0.5 + 60.0 * (double) c.Scale);
  }

  private static void RecomputeVrTargets()
  {
    float num1 = (float) (12.0 * ((double) Notification.VrFontSize / 39.0));
    float num2 = 0.0f;
    for (int index = Notification._vrCards.Count - 1; index >= 0; --index)
    {
      Notification._vrCards[index].TargetY = num2;
      num2 += Notification._vrCards[index].Height + num1;
    }
  }

  private static void EnsureVrInit()
  {
    if (Notification._vrHasInit)
      return;
    Notification._mainCamera = GameObject.Find("Main Camera");
    if (((UnityEngine.Object) Notification._mainCamera == (UnityEngine.Object) null))
      return;
    Notification._hudObj = new GameObject("NOTIFICATION_HUD_CANVAS")
    {
      layer = 19
    };
    Notification._hudObj2 = new GameObject("NOTIFICATION_HUD_PARENT")
    {
      layer = 19
    };
    Camera component = Notification._mainCamera.GetComponent<Camera>();
    if (((UnityEngine.Object) component != (UnityEngine.Object) null))
      component.cullingMask |= 524288 /*0x080000*/;
    Canvas canvas = Notification._hudObj.AddComponent<Canvas>();
    Notification._hudObj.AddComponent<CanvasScaler>();
    Notification._hudObj.AddComponent<GraphicRaycaster>();
    ((Behaviour) canvas).enabled = true;
    canvas.renderMode = (RenderMode) 2;
    canvas.worldCamera = Notification._mainCamera.GetComponent<Camera>();
    Notification._hudObj.GetComponent<RectTransform>().sizeDelta = new Vector2(5f, 5f);
    Notification._hudObj2.transform.SetPositionAndRotation(Notification._mainCamera.transform.position, Notification._mainCamera.transform.rotation);
    Notification._hudObj.transform.parent = Notification._hudObj2.transform;
    ((Transform) Notification._hudObj.GetComponent<RectTransform>()).localPosition = new Vector3(0.0f, 0.0f, 2.8f);
    Notification._hudObj.transform.localRotation = Quaternion.identity;
    Notification._hudObj.transform.localScale = Vector3.one;
    GameObject gameObject = GameObject.Find("COC Text");
    Notification._vrFont = ((UnityEngine.Object) gameObject != (UnityEngine.Object) null) ? gameObject.GetComponent<UnityEngine.UI.Text>().font : Resources.GetBuiltinResource<Font>("Arial.ttf");
    if (((UnityEngine.Object) Notification._vrTextMat == (UnityEngine.Object) null))
      Notification._vrTextMat = new Material(Shader.Find("GUI/Text Shader"));
    if (((UnityEngine.Object) Notification._vrPanelMat == (UnityEngine.Object) null))
    {
      Notification._vrPanelMat = new Material(Shader.Find("UI/Default"));
      Notification._vrPanelMat.SetInt("unity_GUIZTestMode", 8);
    }
    Notification._vrStack = new GameObject("NotificationStack")
    {
      layer = 19
    }.AddComponent<RectTransform>();
    ((Transform) Notification._vrStack).SetParent(Notification._hudObj.transform, false);
    Notification._vrStack.pivot = new Vector2(0.5f, 0.0f);
    Notification._vrStack.sizeDelta = new Vector2(10f, 10f);
    ((Transform) Notification._vrStack).localScale = new Vector3(1f / 400f, 1f / 400f, 1f);
    ((Transform) Notification._vrStack).localPosition = new Vector3(-0.6f, -0.6f, 0.0f);
    Notification._vrHasInit = true;
  }

  private static void VrFixedUpdate()
  {
    if (Notification._vrHasInit)
    {
      if ((!((UnityEngine.Object) Notification._hudObj2 != (UnityEngine.Object) null) ? 0 : (((UnityEngine.Object) Notification._mainCamera != (UnityEngine.Object) null) ? 1 : 0)) != 0)
      {
        Notification._hudObj2.transform.position = Notification._mainCamera.transform.position;
        Notification._hudObj2.transform.rotation = Notification._mainCamera.transform.rotation;
      }
      float time = Time.time;
      bool flag = false;
      for (int index = Notification._vrCards.Count - 1; index >= 0; --index)
      {
        if ((!((UnityEngine.Object) Notification._vrCards[index].Root != (UnityEngine.Object) null) ? 0 : ((double) time < (double) Notification._vrCards[index].Expiry ? 1 : 0)) == 0)
        {
          if (((UnityEngine.Object) Notification._vrCards[index].Root != (UnityEngine.Object) null))
            UnityEngine.Object.Destroy((UnityEngine.Object) Notification._vrCards[index].Root);
          Notification._vrCards.RemoveAt(index);
          flag = true;
        }
      }
      if (flag)
        Notification.RecomputeVrTargets();
      float num1 = 1f - Mathf.Exp((float) (-(double) Time.fixedDeltaTime * 14.0));
      foreach (Notification.VrCard vrCard in Notification._vrCards)
      {
        if (!((UnityEngine.Object) vrCard.Root == (UnityEngine.Object) null))
        {
          float t = Mathf.Clamp01((float) (((double) time - (double) vrCard.SpawnTime) / 0.44999998807907104));
          float num2 = 1f - Mathf.Clamp01((float) (((double) vrCard.Expiry - (double) time) / 0.30000001192092896));
          float num3 = Notification.VrSweepTravel(vrCard);
          Vector2 anchoredPosition = vrCard.Rt.anchoredPosition;
          anchoredPosition.x = Notification.SweepIn(t, -num3, 0.0f, 26f * vrCard.Scale);
          if ((double) num2 > 0.0)
            anchoredPosition.x = Mathf.Lerp(0.0f, -num3, num2 * num2 * num2);
          anchoredPosition.y = Mathf.Lerp(anchoredPosition.y, vrCard.TargetY, num1);
          vrCard.Rt.anchoredPosition = anchoredPosition;
          float num4 = Mathf.Clamp01((float) (((double) time - (double) vrCard.SpawnTime) / 0.11999999731779099));
          vrCard.Group.alpha = Mathf.Min(num4, 1f - num2);
        }
      }
    }
    else
    {
      if (!((UnityEngine.Object) GameObject.Find("Main Camera") != (UnityEngine.Object) null))
        return;
      Notification.EnsureVrInit();
    }
  }

  private class VrCard
  {
    public GameObject Root;
    public RectTransform Rt;
    public CanvasGroup Group;
    public float SpawnTime;
    public float Expiry;
    public float Height;
    public float Width;
    public float Scale;
    public float TargetY;
  }

  private class NotificationHost : MonoBehaviour
  {
    private void FixedUpdate()
    {
      Notification.VrFixedUpdate();
      Notification.UpdateDuration();
    }
  }
}
