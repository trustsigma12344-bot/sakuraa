using SakuraaCastingMod.Shared.Helpers;
using SakuraaCastingMod.Shared.Models;
using SakuraaCastingMod.VR.UtilMenu;
using SakuraaCastingMod.VR.UtilMenu.Pages;
using SakuraaCastingMod.VR.UtilMenu.Utility;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Desktop.Ui.Framework.Menus;

public static class UtilMenuViewer
{
  [SavedSetting("ShowUtilMenuViewer", false)]
  public static bool ShowViewer = false;
  [SavedSetting("UtilMenuPosX", 80f)]
  public static float PosX = 80f;
  [SavedSetting("UtilMenuPosY", 200f)]
  public static float PosY = 200f;
  [SavedSetting("UtilMenuScale", 1f)]
  public static float ViewerScale = 1f;
  private static float _lastTotalHeight = 300f;
  private static bool _isDragging;
  private static Vector2 _dragOffset;
  private static GUIStyle _panelStyle;
  private static GUIStyle _tabBtnStyle;
  private static GUIStyle _tabBtnActiveStyle;
  private static GUIStyle _pageBtnStyle;
  private static GUIStyle _pageBtnActiveStyle;
  private static GUIStyle _elementBtnStyle;
  private static GUIStyle _elementToggleOnStyle;
  private static GUIStyle _elementToggleOffStyle;
  private static GUIStyle _sliderLabelStyle;
  private static GUIStyle _titleStyle;
  private static GUIStyle _descStyle;
  private static GUIStyle _infoTextStyle;
  private static GUIStyle _headerBarStyle;
  private static GUIStyle _panel2Style;
  private static GUIStyle _inputStyle;
  private static GUIStyle _sliderArrowStyle;
  private static Texture2D _panelTex;
  private static Texture2D _headerTex;
  private static Texture2D _tabTex;
  private static Texture2D _tabActiveTex;
  private static Texture2D _pageTex;
  private static Texture2D _pageActiveTex;
  private static Texture2D _btnTex;
  private static Texture2D _btnHoverTex;
  private static Texture2D _btnPressedTex;
  private static Texture2D _toggleOnTex;
  private static Texture2D _toggleOffTex;
  private static Texture2D _panel2Tex;
  private static Texture2D _inputTex;
  private static int _lastThemeIndex = -1;
  private static bool _lastThemeEnabled;
  private static bool _stylesBuilt;
  private static bool _themeReady;
  private const float PanelWidth = 440f;
  private const float TabBarHeight = 40f;
  private const float HeaderHeight = 36f;
  private const float ElementHeight = 36f;
  private const float ElementSpacing = 5f;
  private const float PageBtnWidth = 52f;
  private const float PageBtnHeight = 46f;
  private const float PageBtnSpacing = 5f;
  private const float ContentPadding = 10f;
  private const float Panel2Width = 320f;
  private const int MaxElements = 6;
  private static Vector2 _panel2Scroll;
  private static string _activeInputId;
  private static string _inputBuffer = "";

  public static Rect GetDisplayRect()
  {
    float viewerScale = UtilMenuViewer.ViewerScale;
    return new Rect(UtilMenuViewer.PosX, UtilMenuViewer.PosY, 440f * viewerScale, UtilMenuViewer._lastTotalHeight * viewerScale);
  }

  public static void Draw()
  {
    if (!UtilMenuViewer.ShowViewer)
      return;
    UtilMenuController instance = UtilMenuController.Instance;
    if ((((UnityEngine.Object) instance == (UnityEngine.Object) null) || instance.Pages == null ? 1 : (instance.Pages.Count == 0 ? 1 : 0)) != 0)
      return;
    UtilMenuViewer.RebuildStylesIfNeeded();
    if (!UtilMenuViewer._stylesBuilt)
      return;
    List<BasePage> pages = instance.Pages;
    int currentPageIndex = instance.CurrentPageIndex;
    int currentTabIndex = instance.CurrentTabIndex;
    if (currentPageIndex >= pages.Count)
      return;
    BasePage page = pages[currentPageIndex];
    int count = page.Tabs.Count;
    bool flag = UtilMenuViewer.ShouldShowPanel2(page, currentTabIndex);
    bool showPlayerList = UtilMenuViewer.ShouldShowPlayerList(page, currentTabIndex);
    float mainHeight = (float) (76.0 + (double) UtilMenuViewer.CalculateContentHeight(page, currentTabIndex, showPlayerList) + 20.0);
    UtilMenuViewer._lastTotalHeight = mainHeight;
    float viewerScale = UtilMenuViewer.ViewerScale;
    Rect headerRect;
    // ISSUE: explicit constructor call
    headerRect = new Rect(UtilMenuViewer.PosX, UtilMenuViewer.PosY, 440f * viewerScale, 36f * viewerScale);
    UtilMenuViewer.HandleDrag(headerRect);
    UtilMenuViewer.PosX = Mathf.Clamp(UtilMenuViewer.PosX, 0.0f, (float) Screen.width - (float) (500.0 + (flag ? 328.0 : 0.0)) * viewerScale);
    UtilMenuViewer.PosY = Mathf.Clamp(UtilMenuViewer.PosY, 0.0f, (float) Screen.height - mainHeight * viewerScale);
    Matrix4x4 matrix = GUI.matrix;
    GUIUtility.ScaleAroundPivot(new Vector2(viewerScale, viewerScale), new Vector2(UtilMenuViewer.PosX, UtilMenuViewer.PosY));
    Rect rect1;
    // ISSUE: explicit constructor call
    rect1 = new Rect(UtilMenuViewer.PosX, UtilMenuViewer.PosY, 440f, mainHeight);
    GUI.Box(rect1, "", UtilMenuViewer._panelStyle);
    Rect rect2;
    // ISSUE: explicit constructor call
    rect2 = new Rect(rect1.x, rect1.y, rect1.width, 36f);
    GUI.Box(rect2, "", UtilMenuViewer._headerBarStyle);
    GUI.Label(new Rect(rect2.x + 14f, rect2.y, rect2.width - 28f, 36f), page.PageName, UtilMenuViewer._titleStyle);
    float y1 = rect1.y + 36f;
    UtilMenuViewer.DrawTabBar(rect1.x, y1, rect1.width, count, currentTabIndex, page, instance);
    float x1 = (float) ((double) rect1.x + (double) rect1.width + 6.0);
    float y2 = rect1.y;
    UtilMenuViewer.DrawPageButtons(x1, y2, pages, currentPageIndex, instance);
    float y3 = (float) ((double) y1 + 40.0 + 10.0);
    float width = rect1.width - 20f;
    float x2 = rect1.x + 10f;
    if (showPlayerList)
      UtilMenuViewer.DrawPlayerList(x2, y3, width, page);
    else
      UtilMenuViewer.DrawElements(x2, y3, width, page, currentTabIndex, instance);
    if (flag)
      UtilMenuViewer.DrawPanel2((float) ((double) x1 + 52.0 + 6.0), rect1.y, mainHeight, page);
    GUI.matrix = matrix;
  }

  private static bool ShouldShowPlayerList(BasePage page, int tabIndex)
  {
    UtilMenuController instance = UtilMenuController.Instance;
    return ((UnityEngine.Object) instance.playerSelectRoot != (UnityEngine.Object) null) && instance.playerSelectRoot.activeSelf;
  }

  private static bool ShouldShowPanel2(BasePage page, int tabIndex)
  {
    UtilMenuController instance = UtilMenuController.Instance;
    return page is LobbyPage && ((UnityEngine.Object) instance.panel2Root != (UnityEngine.Object) null) && instance.panel2Root.activeSelf;
  }

  private static float CalculateContentHeight(BasePage page, int tabIndex, bool showPlayerList)
  {
    float contentHeight;
    if (showPlayerList)
    {
      UtilMenuController instance = UtilMenuController.Instance;
      int num1 = 0;
      for (int index = 0; index < instance.playerButtons.Count; ++index)
      {
        if (((Component) instance.playerButtons[index]).gameObject.activeSelf)
          ++num1;
      }
      float num2 = (!((UnityEngine.Object) instance.peSliderText != (UnityEngine.Object) null) ? 0 : (((Component) instance.peSliderText).gameObject.activeSelf ? 1 : 0)) != 0 ? 41f : 0.0f;
      contentHeight = (float) ((double) Mathf.Max(num1, 1) * 41.0 + (double) num2 + 5.0);
    }
    else if (tabIndex >= page.Tabs.Count)
    {
      contentHeight = 60f;
    }
    else
    {
      UtilTab tab = page.Tabs[tabIndex];
      contentHeight = !string.IsNullOrEmpty(tab.Description) ? 120f : (!string.IsNullOrEmpty(page.GetAccessError()) ? 120f : (float) ((double) Mathf.Min(tab.Elements.Count, 6) * 41.0 + 5.0));
    }
    return contentHeight;
  }

  private static void DrawTabBar(
    float x,
    float y,
    float width,
    int tabCount,
    int currentTab,
    BasePage page,
    UtilMenuController ctrl)
  {
    if (tabCount == 0)
      return;
    float num1 = width - 20f;
    float num2 = 3f;
    float num3 = (num1 - num2 * (float) (tabCount - 1)) / (float) tabCount;
    for (int index = 0; index < tabCount; ++index)
    {
      Rect rect1;
      // ISSUE: explicit constructor call
      rect1 = new Rect((float) ((double) x + 10.0 + (double) index * ((double) num3 + (double) num2)), y + 3f, num3, 34f);
      GUIStyle guiStyle = index == currentTab ? UtilMenuViewer._tabBtnActiveStyle : UtilMenuViewer._tabBtnStyle;
      Texture iconTexture = UtilMenuViewer.GetIconTexture(page.Tabs[index].TabIcon);
      if (((UnityEngine.Object) iconTexture != (UnityEngine.Object) null))
      {
        if (GUI.Button(rect1, "", guiStyle))
        {
          Sounds.PlayCasterClick(Sounds.subtleClickSfx);
          ctrl.OnTabPress(index + 1);
          UtilMenuViewer._activeInputId = (string) null;
        }
        float num4 = Mathf.Min(rect1.width - 8f, rect1.height - 6f);
        Rect rect2;
        // ISSUE: explicit constructor call
        rect2 = new Rect(rect1.x + (float) (((double) rect1.width - (double) num4) / 2.0), rect1.y + (float) (((double) rect1.height - (double) num4) / 2.0), num4, num4);
        GUI.DrawTexture(rect2, iconTexture, (ScaleMode) 2);
      }
      else if (GUI.Button(rect1, page.Tabs[index].TabName ?? $"Tab {index + 1}", guiStyle))
      {
        Sounds.PlayCasterClick(Sounds.subtleClickSfx);
        ctrl.OnTabPress(index + 1);
        UtilMenuViewer._activeInputId = (string) null;
      }
    }
  }

  private static void DrawPageButtons(
    float x,
    float y,
    List<BasePage> pages,
    int currentPageIndex,
    UtilMenuController ctrl)
  {
    for (int index = 0; index < pages.Count; ++index)
    {
      float num1 = y + (float) index * 51f;
      Rect rect1;
      // ISSUE: explicit constructor call
      rect1 = new Rect(x, num1, 52f, 46f);
      GUIStyle guiStyle = index == currentPageIndex ? UtilMenuViewer._pageBtnActiveStyle : UtilMenuViewer._pageBtnStyle;
      Texture iconTexture = UtilMenuViewer.GetIconTexture(pages[index].PageIcon);
      if (((UnityEngine.Object) iconTexture != (UnityEngine.Object) null))
      {
        if (GUI.Button(rect1, "", guiStyle))
        {
          Sounds.PlayCasterClick(Sounds.subtleClickSfx);
          ctrl.SwitchPage(index);
          UtilMenuViewer._activeInputId = (string) null;
        }
        float num2 = Mathf.Min(rect1.width - 8f, rect1.height - 8f);
        Rect rect2;
        // ISSUE: explicit constructor call
        rect2 = new Rect(rect1.x + (float) (((double) rect1.width - (double) num2) / 2.0), rect1.y + (float) (((double) rect1.height - (double) num2) / 2.0), num2, num2);
        GUI.DrawTexture(rect2, iconTexture, (ScaleMode) 2);
      }
      else
      {
        string str = pages[index].PageName;
        if (str.Length > 5)
          str = str.Substring(0, 5);
        if (GUI.Button(rect1, str, guiStyle))
        {
          Sounds.PlayCasterClick(Sounds.subtleClickSfx);
          ctrl.SwitchPage(index);
          UtilMenuViewer._activeInputId = (string) null;
        }
      }
    }
  }

  private static Texture GetIconTexture(Material mat)
  {
    return !((UnityEngine.Object) mat == (UnityEngine.Object) null) ? mat.mainTexture : (Texture) null;
  }

  private static void DrawElements(
    float x,
    float y,
    float width,
    BasePage page,
    int tabIndex,
    UtilMenuController ctrl)
  {
    if (tabIndex >= page.Tabs.Count)
      return;
    UtilTab tab = page.Tabs[tabIndex];
    if (string.IsNullOrEmpty(tab.Description))
    {
      string accessError = page.GetAccessError();
      if (string.IsNullOrEmpty(accessError))
      {
        float num1 = y;
        int num2 = Mathf.Min(tab.Elements.Count, 6);
        for (int index = 0; index < num2; ++index)
        {
          MenuElement element = tab.Elements[index];
          Rect rect;
          // ISSUE: explicit constructor call
          rect = new Rect(x, num1, width, 36f);
          switch (element.Type)
          {
            case ElementType.Button:
            case ElementType.HoldableButton:
              UtilMenuViewer.DrawButtonElement(rect, element, ctrl);
              break;
            case ElementType.Toggle:
              UtilMenuViewer.DrawToggleElement(rect, element, ctrl);
              break;
            case ElementType.Slider:
              UtilMenuViewer.DrawSliderElement(rect, element, ctrl);
              break;
            case ElementType.Input:
              UtilMenuViewer.DrawInputElement(rect, element, index, ctrl);
              break;
          }
          num1 += 41f;
        }
      }
      else
      {
        Rect rect;
        // ISSUE: explicit constructor call
        rect = new Rect(x, y, width, 110f);
        GUI.Label(rect, accessError, UtilMenuViewer._descStyle);
      }
    }
    else
    {
      Rect rect;
      // ISSUE: explicit constructor call
      rect = new Rect(x, y, width, 110f);
      GUI.Label(rect, tab.Description, UtilMenuViewer._descStyle);
    }
  }

  private static void DrawButtonElement(Rect rect, MenuElement element, UtilMenuController ctrl)
  {
    if (!GUI.Button(rect, element.Text, UtilMenuViewer._elementBtnStyle))
      return;
    Sounds.PlayCasterClick(Sounds.boingSfx);
    Action onClick = element.OnClick;
    if (onClick != null)
      onClick();
    ctrl.RefreshUI();
  }

  private static void DrawToggleElement(Rect rect, MenuElement element, UtilMenuController ctrl)
  {
    GUIStyle guiStyle = element.IsToggled ? UtilMenuViewer._elementToggleOnStyle : UtilMenuViewer._elementToggleOffStyle;
    if (!GUI.Button(rect, element.Text, guiStyle))
      return;
    Sounds.PlayCasterClick(Sounds.subtleClickSfx);
    element.IsToggled = !element.IsToggled;
    Action onToggle = element.OnToggle;
    if (onToggle != null)
      onToggle();
    ctrl.RefreshUI();
  }

  private static void DrawSliderElement(Rect rect, MenuElement element, UtilMenuController ctrl)
  {
    float num1 = 36f;
    float num2 = (float) ((double) rect.width - 72.0 - 8.0);
    Rect rect1;
    // ISSUE: explicit constructor call
    rect1 = new Rect(rect.x, rect.y, num1, rect.height);
    if (GUI.Button(rect1, "<", UtilMenuViewer._sliderArrowStyle))
    {
      Sounds.PlayCasterClick(Sounds.subtleClickSfx);
      Action onSliderLeft = element.OnSliderLeft;
      if (onSliderLeft != null)
        onSliderLeft();
      ctrl.RefreshUI();
    }
    Rect rect2;
    // ISSUE: explicit constructor call
    rect2 = new Rect((float) ((double) rect.x + (double) num1 + 4.0), rect.y, num2, rect.height);
    string str = element.Text;
    if (!string.IsNullOrEmpty(element.ValueText))
      str = $"{str}  :  {element.ValueText}";
    GUI.Label(rect2, str, UtilMenuViewer._sliderLabelStyle);
    Rect rect3;
    // ISSUE: explicit constructor call
    rect3 = new Rect(rect.x + rect.width - num1, rect.y, num1, rect.height);
    if (!GUI.Button(rect3, ">", UtilMenuViewer._sliderArrowStyle))
      return;
    Sounds.PlayCasterClick(Sounds.subtleClickSfx);
    Action onSliderRight = element.OnSliderRight;
    if (onSliderRight != null)
      onSliderRight();
    ctrl.RefreshUI();
  }

  private static void DrawInputElement(
    Rect rect,
    MenuElement element,
    int index,
    UtilMenuController ctrl)
  {
    string str1 = $"input_{index}";
    if (!(UtilMenuViewer._activeInputId == str1))
    {
      string str2 = string.IsNullOrEmpty(element.Text) ? "[ Click to type... ]" : element.Text;
      if (!GUI.Button(rect, "> " + str2, UtilMenuViewer._inputStyle))
        return;
      UtilMenuViewer._activeInputId = str1;
      UtilMenuViewer._inputBuffer = "";
      Action onClick = element.OnClick;
      if (onClick != null)
        onClick();
      ctrl.RefreshUI();
    }
    else
    {
      Rect rect1;
      // ISSUE: explicit constructor call
      rect1 = new Rect(rect.x - 2f, rect.y - 2f, rect.width + 4f, rect.height + 4f);
      Color color1 = GUI.color;
      Color customColor = ThemeManager.GetCustomColor("SELECTED");
      GUI.color = new Color(customColor.r, customColor.g, customColor.b, 0.6f);
      GUI.Box(rect1, "", UtilMenuViewer._panelStyle);
      GUI.color = color1;
      GUI.SetNextControlName($"utilinput_{index}");
      string str3 = GUI.TextField(rect, UtilMenuViewer._inputBuffer, UtilMenuViewer._inputStyle);
      if (str3 != UtilMenuViewer._inputBuffer)
      {
        UtilMenuViewer._inputBuffer = str3;
        KeyboardController instance = KeyboardController.Instance;
        if (((UnityEngine.Object) instance != (UnityEngine.Object) null))
        {
          instance.currentInput = UtilMenuViewer._inputBuffer;
          Action<string> onKeyPressed = instance.OnKeyPressed;
          if (onKeyPressed != null)
            onKeyPressed(UtilMenuViewer._inputBuffer);
        }
      }
      GUI.FocusControl($"utilinput_{index}");
      if (string.IsNullOrEmpty(UtilMenuViewer._inputBuffer))
      {
        Color color2 = GUI.color;
        GUI.color = new Color(1f, 1f, 1f, 0.35f);
        GUI.Label(rect, "  Type here, press Enter...", UtilMenuViewer._sliderLabelStyle);
        GUI.color = color2;
      }
      if ((Event.current.type != (EventType) 4 ? 0 : (Event.current.keyCode == (KeyCode) 13 ? 1 : 0)) != 0)
      {
        UtilMenuViewer._activeInputId = (string) null;
        KeyboardController instance = KeyboardController.Instance;
        if (((UnityEngine.Object) instance != (UnityEngine.Object) null))
        {
          Action onEnterPressed = instance.OnEnterPressed;
          if (onEnterPressed != null)
            onEnterPressed();
        }
        ctrl.RefreshUI();
        Event.current.Use();
      }
      else
      {
        if ((Event.current.type != (EventType) 4 ? 0 : (Event.current.keyCode == (KeyCode) 27 ? 1 : 0)) == 0)
          return;
        UtilMenuViewer._activeInputId = (string) null;
        KeyboardController instance = KeyboardController.Instance;
        if (((UnityEngine.Object) instance != (UnityEngine.Object) null))
          instance.CloseKeyboard();
        ctrl.RefreshUI();
        Event.current.Use();
      }
    }
  }

  private static void OnListPagerPressed(BasePage page, bool right)
  {
    switch (page)
    {
      case LobbyPage lobbyPage:
        if (!right)
        {
          lobbyPage.OnPlayerPageLeft();
          break;
        }
        lobbyPage.OnPlayerPageRight();
        break;
      case SoundboardPage soundboardPage:
        if (!right)
        {
          soundboardPage.OnTilePageLeft();
          break;
        }
        soundboardPage.OnTilePageRight();
        break;
    }
  }

  private static void OnListButtonPressed(BasePage page, int index)
  {
    switch (page)
    {
      case LobbyPage lobbyPage:
        lobbyPage.OnPlayerButtonPressed(index);
        break;
      case SoundboardPage soundboardPage:
        soundboardPage.OnTilePressed(index);
        break;
    }
  }

  private static void DrawPlayerList(float x, float y, float width, BasePage page)
  {
    UtilMenuController instance = UtilMenuController.Instance;
    if (((UnityEngine.Object) instance == (UnityEngine.Object) null))
      return;
    float num1 = y;
    if ((!((UnityEngine.Object) instance.peSliderText != (UnityEngine.Object) null) ? 0 : (((Component) instance.peSliderText).gameObject.activeSelf ? 1 : 0)) != 0)
    {
      float num2 = width / 3f;
      if (GUI.Button(new Rect(x, num1, num2, 36f), "<<", UtilMenuViewer._sliderArrowStyle))
      {
        Sounds.PlayCasterClick(Sounds.subtleClickSfx);
        UtilMenuViewer.OnListPagerPressed(page, false);
      }
      string text = ((UnityEngine.Object) instance.peSliderText != (UnityEngine.Object) null) ? ((TMP_Text) instance.peSliderText).text : "";
      GUI.Label(new Rect(x + num2, num1, num2, 36f), text, UtilMenuViewer._sliderLabelStyle);
      if (GUI.Button(new Rect(x + num2 * 2f, num1, num2, 36f), ">>", UtilMenuViewer._sliderArrowStyle))
      {
        Sounds.PlayCasterClick(Sounds.subtleClickSfx);
        UtilMenuViewer.OnListPagerPressed(page, true);
      }
      num1 += 41f;
    }
    for (int index = 0; index < instance.playerButtons.Count; ++index)
    {
      if (((Component) instance.playerButtons[index]).gameObject.activeSelf)
      {
        Rect rect;
        // ISSUE: explicit constructor call
        rect = new Rect(x, num1, width, 36f);
        GUIStyle guiStyle = instance.playerButtons[index].isOn ? UtilMenuViewer._pageBtnActiveStyle : UtilMenuViewer._elementBtnStyle;
        string text = index < instance.playerButtonTexts.Count ? ((TMP_Text) instance.playerButtonTexts[index]).text : "";
        if (GUI.Button(rect, text, guiStyle))
        {
          Sounds.PlayCasterClick(Sounds.subtleClickSfx);
          UtilMenuViewer.OnListButtonPressed(page, index);
        }
        num1 += 41f;
      }
    }
  }

  private static void DrawPanel2(float x, float y, float mainHeight, BasePage page)
  {
    if (!(page is LobbyPage lobbyPage))
      return;
    UtilMenuController instance = UtilMenuController.Instance;
    if ((((UnityEngine.Object) instance == (UnityEngine.Object) null) || ((UnityEngine.Object) instance.panel2Root == (UnityEngine.Object) null) ? 1 : (!instance.panel2Root.activeSelf ? 1 : 0)) != 0)
      return;
    float num1 = Mathf.Max(mainHeight, 380f);
    Rect rect1;
    // ISSUE: explicit constructor call
    rect1 = new Rect(x, y, 320f, num1);
    GUI.Box(rect1, "", UtilMenuViewer._panel2Style);
    float num2 = x + 10f;
    float num3 = y + 10f;
    float num4 = 300f;
    if (((UnityEngine.Object) instance.userInfoText != (UnityEngine.Object) null))
    {
      string[] strArray = (((TMP_Text) instance.userInfoText).text ?? "").Split('\n');
      foreach (string str in strArray)
      {
        if (!string.IsNullOrWhiteSpace(str))
        {
          bool flag;
          GUIStyle guiStyle = (flag = str == strArray[0]) ? UtilMenuViewer._titleStyle : UtilMenuViewer._infoTextStyle;
          float num5 = flag ? 28f : 22f;
          GUI.Label(new Rect(num2, num3, num4, num5), str, guiStyle);
          num3 += num5 + 2f;
        }
      }
      num3 += 6f;
    }
    float num6 = (float) (((double) num4 - 6.0) / 2.0);
    string str1 = ((UnityEngine.Object) instance.modsBtnText != (UnityEngine.Object) null) ? ((TMP_Text) instance.modsBtnText).text.Replace("\n", " ") : "MODS";
    string str2 = ((UnityEngine.Object) instance.cheatsBtnText != (UnityEngine.Object) null) ? ((TMP_Text) instance.cheatsBtnText).text.Replace("\n", " ") : "CHEATS";
    bool flag1 = ((UnityEngine.Object) instance.modsCheckBtn != (UnityEngine.Object) null) && instance.modsCheckBtn.isOn;
    bool flag2 = ((UnityEngine.Object) instance.cheatsCheckBtn != (UnityEngine.Object) null) && instance.cheatsCheckBtn.isOn;
    GUIStyle guiStyle1 = flag1 ? UtilMenuViewer._tabBtnActiveStyle : UtilMenuViewer._elementBtnStyle;
    GUIStyle guiStyle2 = flag2 ? UtilMenuViewer._tabBtnActiveStyle : UtilMenuViewer._elementBtnStyle;
    if (GUI.Button(new Rect(num2, num3, num6, 36f), str1, guiStyle1))
    {
      Sounds.PlayCasterClick(Sounds.subtleClickSfx);
      lobbyPage.OnCheckMods();
    }
    if (GUI.Button(new Rect((float) ((double) num2 + (double) num6 + 6.0), num3, num6, 36f), str2, guiStyle2))
    {
      Sounds.PlayCasterClick(Sounds.subtleClickSfx);
      lobbyPage.OnCheckCheats();
    }
    float num7 = num3 + 46f;
    if ((!((UnityEngine.Object) instance.modAndCheatListText != (UnityEngine.Object) null) ? 0 : (((Component) instance.modAndCheatListText).gameObject.activeSelf ? 1 : 0)) == 0)
    {
      if ((!((UnityEngine.Object) instance.volumeBarRoot != (UnityEngine.Object) null) ? 0 : (instance.volumeBarRoot.activeSelf ? 1 : 0)) != 0)
      {
        string text = ((UnityEngine.Object) instance.volumeValueText != (UnityEngine.Object) null) ? ((TMP_Text) instance.volumeValueText).text : "";
        GUI.Label(new Rect(num2, num7, num4, 24f), "VOLUME: " + text, UtilMenuViewer._titleStyle);
        float num8 = num7 + 28f;
        bool flag3;
        GUIStyle guiStyle3 = (flag3 = ((UnityEngine.Object) instance.volUnMuteBtn != (UnityEngine.Object) null) && !instance.volUnMuteBtn.isOn) ? UtilMenuViewer._elementToggleOnStyle : UtilMenuViewer._elementBtnStyle;
        if (GUI.Button(new Rect(num2, num8, num6, 36f), "VOL -", UtilMenuViewer._elementBtnStyle))
        {
          Sounds.PlayCasterClick(Sounds.subtleClickSfx);
          lobbyPage.OnVolumeChange(false);
        }
        if (GUI.Button(new Rect((float) ((double) num2 + (double) num6 + 6.0), num8, num6, 36f), "VOL +", UtilMenuViewer._elementBtnStyle))
        {
          Sounds.PlayCasterClick(Sounds.subtleClickSfx);
          lobbyPage.OnVolumeChange(true);
        }
        float num9 = num8 + 41f;
        if (GUI.Button(new Rect(num2, num9, num6, 36f), flag3 ? "UNMUTE" : "MUTE", guiStyle3))
        {
          Sounds.PlayCasterClick(Sounds.subtleClickSfx);
          lobbyPage.OnMuteToggle();
        }
        GUIStyle guiStyle4 = (!((UnityEngine.Object) instance.volPrioritizeBtn != (UnityEngine.Object) null) ? 0 : (instance.volPrioritizeBtn.isOn ? 1 : 0)) != 0 ? UtilMenuViewer._elementToggleOnStyle : UtilMenuViewer._elementBtnStyle;
        if (GUI.Button(new Rect((float) ((double) num2 + (double) num6 + 6.0), num9, num6, 36f), "PRIORITIZE", guiStyle4))
        {
          Sounds.PlayCasterClick(Sounds.subtleClickSfx);
          lobbyPage.OnPrioritizeBtn();
        }
        num7 = num9 + 41f;
      }
      if ((!((UnityEngine.Object) instance.cloneCosmeticsBtnObj != (UnityEngine.Object) null) ? 0 : (instance.cloneCosmeticsBtnObj.activeSelf ? 1 : 0)) != 0)
      {
        if (GUI.Button(new Rect(num2, num7, num4, 36f), "CLONE COSMETICS", UtilMenuViewer._elementBtnStyle))
        {
          Sounds.PlayCasterClick(Sounds.subtleClickSfx);
          lobbyPage.OnCloneCosmetics();
        }
        num7 += 41f;
      }
      if ((!((UnityEngine.Object) instance.reportBarRoot != (UnityEngine.Object) null) ? 0 : (instance.reportBarRoot.activeSelf ? 1 : 0)) == 0)
        return;
      if ((!((UnityEngine.Object) instance.reportMainBtn != (UnityEngine.Object) null) ? 0 : (!((Component) instance.reportMainBtn).gameObject.activeSelf ? 1 : 0)) == 0)
      {
        if (!GUI.Button(new Rect(num2, num7, num4, 36f), "REPORT", UtilMenuViewer._elementBtnStyle))
          return;
        Sounds.PlayCasterClick(Sounds.subtleClickSfx);
        lobbyPage.OnMainReportBtn();
      }
      else
      {
        string str3 = ((UnityEngine.Object) instance.reportSliderText != (UnityEngine.Object) null) ? ((TMP_Text) instance.reportSliderText).text : "REPORT:";
        string text = ((UnityEngine.Object) instance.reportValueText != (UnityEngine.Object) null) ? ((TMP_Text) instance.reportValueText).text : "";
        float num10 = 36f;
        float num11 = (float) ((double) num4 - 72.0 - 8.0);
        if (GUI.Button(new Rect(num2, num7, num10, 36f), "<", UtilMenuViewer._sliderArrowStyle))
        {
          Sounds.PlayCasterClick(Sounds.subtleClickSfx);
          lobbyPage.OnReportSlider(false);
        }
        GUI.Label(new Rect((float) ((double) num2 + (double) num10 + 4.0), num7, num11, 36f), $"{str3} {text}", UtilMenuViewer._sliderLabelStyle);
        if (GUI.Button(new Rect(num2 + num4 - num10, num7, num10, 36f), ">", UtilMenuViewer._sliderArrowStyle))
        {
          Sounds.PlayCasterClick(Sounds.subtleClickSfx);
          lobbyPage.OnReportSlider(true);
        }
        float num12 = num7 + 41f;
        if (!GUI.Button(new Rect(num2, num12, num4, 36f), "SEND REPORT", UtilMenuViewer._elementToggleOnStyle))
          return;
        Sounds.PlayCasterClick(Sounds.subtleClickSfx);
        lobbyPage.OnSendReport();
      }
    }
    else
    {
      string str4 = ((TMP_Text) instance.modAndCheatListText).text ?? "NONE";
      string str5 = flag1 ? "DETECTED MODS" : "DETECTED CHEATS";
      GUI.Label(new Rect(num2, num7, num4, 24f), str5, UtilMenuViewer._titleStyle);
      float num13 = num7 + 28f;
      string[] strArray = str4.Split('\n');
      float num14 = (float) ((double) num1 - ((double) num13 - (double) y) - 10.0);
      Rect rect2;
      // ISSUE: explicit constructor call
      rect2 = new Rect(num2, num13, num4, num14);
      Rect rect3;
      // ISSUE: explicit constructor call
      rect3 = new Rect(0.0f, 0.0f, num4 - 20f, (float) (Mathf.Max(strArray.Length, 1) * 24 + 10));
      UtilMenuViewer._panel2Scroll = GUI.BeginScrollView(rect2, UtilMenuViewer._panel2Scroll, rect3);
      float num15 = 0.0f;
      foreach (string str6 in strArray)
      {
        string str7 = str6.Replace("<color=red>", "").Replace("<color=green>", "").Replace("<color=yellow>", "").Replace("</color>", "");
        GUI.Label(new Rect(0.0f, num15, num4 - 20f, 22f), str7, UtilMenuViewer._infoTextStyle);
        num15 += 24f;
      }
      GUI.EndScrollView();
    }
  }

  private static void HandleDrag(Rect headerRect)
  {
    Event current = Event.current;
    if ((current.type != null || !headerRect.Contains(current.mousePosition) ? 0 : (current.button == 0 ? 1 : 0)) != 0)
    {
      UtilMenuViewer._isDragging = true;
      UtilMenuViewer._dragOffset = (current.mousePosition - new Vector2(UtilMenuViewer.PosX, UtilMenuViewer.PosY));
      current.Use();
    }
    if (!UtilMenuViewer._isDragging)
      return;
    if (current.type == (EventType) 3)
    {
      Vector2 vector2 = (current.mousePosition - UtilMenuViewer._dragOffset);
      UtilMenuViewer.PosX = vector2.x;
      UtilMenuViewer.PosY = vector2.y;
      current.Use();
    }
    else
    {
      if ((current.type != (EventType) 1 ? 0 : (current.button == 0 ? 1 : 0)) == 0)
        return;
      UtilMenuViewer._isDragging = false;
      current.Use();
    }
  }

  private static void RebuildStylesIfNeeded()
  {
    if (!UtilMenuViewer._themeReady)
    {
      if ((ThemeManager.GetCustomColor("PANEL") == Color.white))
        return;
      UtilMenuViewer._themeReady = true;
    }
    bool flag = ThemeManager.CurrentThemeIndex != UtilMenuViewer._lastThemeIndex || ThemeManager.IsCustomThemeEnabled != UtilMenuViewer._lastThemeEnabled;
    if ((!UtilMenuViewer._stylesBuilt ? 0 : (!flag ? 1 : 0)) != 0)
      return;
    UtilMenuViewer._lastThemeIndex = ThemeManager.CurrentThemeIndex;
    UtilMenuViewer._lastThemeEnabled = ThemeManager.IsCustomThemeEnabled;
    UtilMenuViewer.BuildStyles();
    UtilMenuViewer._stylesBuilt = true;
  }

  private static void BuildStyles()
  {
    UtilMenuViewer.DestroyTex(UtilMenuViewer._panelTex);
    UtilMenuViewer.DestroyTex(UtilMenuViewer._headerTex);
    UtilMenuViewer.DestroyTex(UtilMenuViewer._tabTex);
    UtilMenuViewer.DestroyTex(UtilMenuViewer._tabActiveTex);
    UtilMenuViewer.DestroyTex(UtilMenuViewer._pageTex);
    UtilMenuViewer.DestroyTex(UtilMenuViewer._pageActiveTex);
    UtilMenuViewer.DestroyTex(UtilMenuViewer._btnTex);
    UtilMenuViewer.DestroyTex(UtilMenuViewer._btnHoverTex);
    UtilMenuViewer.DestroyTex(UtilMenuViewer._btnPressedTex);
    UtilMenuViewer.DestroyTex(UtilMenuViewer._toggleOnTex);
    UtilMenuViewer.DestroyTex(UtilMenuViewer._toggleOffTex);
    UtilMenuViewer.DestroyTex(UtilMenuViewer._panel2Tex);
    UtilMenuViewer.DestroyTex(UtilMenuViewer._inputTex);
    Color customColor1 = ThemeManager.GetCustomColor("PANEL");
    Color customColor2 = ThemeManager.GetCustomColor("INNER");
    Color customColor3 = ThemeManager.GetCustomColor("BUTTON");
    Color customColor4 = ThemeManager.GetCustomColor("PRESSED");
    Color customColor5 = ThemeManager.GetCustomColor("SELECTED");
    Color customColor6 = ThemeManager.GetCustomColor("TEXT 1");
    Color customColor7 = ThemeManager.GetCustomColor("TEXT 2");
    Color color1;
    // ISSUE: explicit constructor call
    color1 = new Color(customColor1.r, customColor1.g, customColor1.b, 0.94f);
    Color color2 = UtilMenuViewer.Brighten(customColor1, 0.06f, 0.96f);
    Color color3;
    // ISSUE: explicit constructor call
    color3 = new Color(customColor2.r, customColor2.g, customColor2.b, 0.92f);
    Color color4;
    // ISSUE: explicit constructor call
    color4 = new Color(customColor5.r * 0.6f, customColor5.g * 0.6f, customColor5.b * 0.6f, 0.92f);
    Color color5;
    // ISSUE: explicit constructor call
    color5 = new Color((float) ((double) customColor2.r * 1.1000000238418579 + 0.039999999105930328), (float) ((double) customColor2.g * 1.1000000238418579 + 0.039999999105930328), (float) ((double) customColor2.b * 1.1000000238418579 + 0.039999999105930328), 0.9f);
    Color color6;
    // ISSUE: explicit constructor call
    color6 = new Color(customColor3.r * 0.7f, customColor3.g * 0.7f, customColor3.b * 0.7f, 0.92f);
    Color color7;
    // ISSUE: explicit constructor call
    color7 = new Color(customColor4.r, customColor4.g, customColor4.b, 0.9f);
    Color color8;
    // ISSUE: explicit constructor call
    color8 = new Color(customColor5.r * 0.45f, customColor5.g * 0.45f, customColor5.b * 0.45f, 0.9f);
    Color color9 = UtilMenuViewer.Brighten(customColor2, 0.03f, 0.85f);
    Color color10 = UtilMenuViewer.Darken(customColor2, 0.03f, 0.92f);
    UtilMenuViewer._panelTex = UtilMenuViewer.MakeTex(color1);
    UtilMenuViewer._headerTex = UtilMenuViewer.MakeTex(color2);
    UtilMenuViewer._tabTex = UtilMenuViewer.MakeTex(color3);
    UtilMenuViewer._tabActiveTex = UtilMenuViewer.MakeTex(color4);
    UtilMenuViewer._pageTex = UtilMenuViewer.MakeTex(color5);
    UtilMenuViewer._pageActiveTex = UtilMenuViewer.MakeTex(color4);
    UtilMenuViewer._btnTex = UtilMenuViewer.MakeTex(color5);
    UtilMenuViewer._btnHoverTex = UtilMenuViewer.MakeTex(color6);
    UtilMenuViewer._btnPressedTex = UtilMenuViewer.MakeTex(color7);
    UtilMenuViewer._toggleOnTex = UtilMenuViewer.MakeTex(color8);
    UtilMenuViewer._toggleOffTex = UtilMenuViewer.MakeTex(color9);
    UtilMenuViewer._panel2Tex = UtilMenuViewer.MakeTex(UtilMenuViewer.Darken(customColor1, 0.02f, 0.95f));
    UtilMenuViewer._inputTex = UtilMenuViewer.MakeTex(color10);
    Color color11 = customColor6;
    Color color12 = customColor7;
    Color color13;
    // ISSUE: explicit constructor call
    color13 = new Color(customColor7.r * 0.8f, customColor7.g * 0.8f, customColor7.b * 0.8f);
    UtilMenuViewer._panelStyle = new GUIStyle(GUI.skin.box)
    {
      normal = {
        background = UtilMenuViewer._panelTex
      },
      border = new RectOffset(2, 2, 2, 2)
    };
    UtilMenuViewer._headerBarStyle = new GUIStyle(GUI.skin.box)
    {
      normal = {
        background = UtilMenuViewer._headerTex
      },
      border = new RectOffset(2, 2, 2, 2)
    };
    UtilMenuViewer._titleStyle = new GUIStyle(GUI.skin.label)
    {
      fontSize = 16 /*0x10*/,
      fontStyle = (FontStyle) 1,
      normal = {
        textColor = color11
      },
      alignment = (TextAnchor) 3
    };
    UtilMenuViewer._descStyle = new GUIStyle(GUI.skin.label)
    {
      fontSize = 14,
      wordWrap = true,
      normal = {
        textColor = color12
      },
      alignment = (TextAnchor) 0,
      padding = new RectOffset(6, 6, 6, 6)
    };
    UtilMenuViewer._infoTextStyle = new GUIStyle(GUI.skin.label)
    {
      fontSize = 13,
      normal = {
        textColor = color13
      },
      alignment = (TextAnchor) 3
    };
    UtilMenuViewer._tabBtnStyle = new GUIStyle(GUI.skin.button)
    {
      normal = {
        background = UtilMenuViewer._tabTex,
        textColor = color13
      },
      hover = {
        background = UtilMenuViewer._btnHoverTex,
        textColor = color11
      },
      active = {
        background = UtilMenuViewer._tabActiveTex,
        textColor = color11
      },
      fontSize = 13,
      fontStyle = (FontStyle) 0,
      alignment = (TextAnchor) 4,
      padding = new RectOffset(4, 4, 2, 2),
      wordWrap = true
    };
    UtilMenuViewer._tabBtnActiveStyle = new GUIStyle(UtilMenuViewer._tabBtnStyle)
    {
      normal = {
        background = UtilMenuViewer._tabActiveTex,
        textColor = color11
      },
      fontStyle = (FontStyle) 1
    };
    UtilMenuViewer._pageBtnStyle = new GUIStyle(GUI.skin.button)
    {
      normal = {
        background = UtilMenuViewer._pageTex,
        textColor = color13
      },
      hover = {
        background = UtilMenuViewer._btnHoverTex,
        textColor = color11
      },
      active = {
        background = UtilMenuViewer._pageActiveTex,
        textColor = color11
      },
      fontSize = 11,
      alignment = (TextAnchor) 4,
      padding = new RectOffset(2, 2, 4, 4),
      wordWrap = true
    };
    UtilMenuViewer._pageBtnActiveStyle = new GUIStyle(UtilMenuViewer._pageBtnStyle)
    {
      normal = {
        background = UtilMenuViewer._pageActiveTex,
        textColor = color11
      },
      fontStyle = (FontStyle) 1
    };
    UtilMenuViewer._elementBtnStyle = new GUIStyle(GUI.skin.button)
    {
      normal = {
        background = UtilMenuViewer._btnTex,
        textColor = color11
      },
      hover = {
        background = UtilMenuViewer._btnHoverTex,
        textColor = color11
      },
      active = {
        background = UtilMenuViewer._btnPressedTex,
        textColor = color11
      },
      fontSize = 14,
      alignment = (TextAnchor) 4,
      padding = new RectOffset(8, 8, 2, 2),
      richText = true
    };
    UtilMenuViewer._elementToggleOnStyle = new GUIStyle(UtilMenuViewer._elementBtnStyle)
    {
      normal = {
        background = UtilMenuViewer._toggleOnTex,
        textColor = color11
      }
    };
    UtilMenuViewer._elementToggleOffStyle = new GUIStyle(UtilMenuViewer._elementBtnStyle)
    {
      normal = {
        background = UtilMenuViewer._toggleOffTex,
        textColor = color13
      }
    };
    UtilMenuViewer._sliderLabelStyle = new GUIStyle(GUI.skin.label)
    {
      fontSize = 14,
      normal = {
        textColor = color11
      },
      alignment = (TextAnchor) 4
    };
    UtilMenuViewer._sliderArrowStyle = new GUIStyle(UtilMenuViewer._elementBtnStyle)
    {
      fontSize = 16 /*0x10*/,
      fontStyle = (FontStyle) 1
    };
    UtilMenuViewer._panel2Style = new GUIStyle(GUI.skin.box)
    {
      normal = {
        background = UtilMenuViewer._panel2Tex
      },
      border = new RectOffset(2, 2, 2, 2)
    };
    UtilMenuViewer._inputStyle = new GUIStyle(GUI.skin.textField)
    {
      normal = {
        background = UtilMenuViewer._inputTex,
        textColor = color11
      },
      focused = {
        background = UtilMenuViewer._inputTex,
        textColor = color11
      },
      fontSize = 14,
      alignment = (TextAnchor) 4
    };
  }

  private static Color Brighten(Color c, float amount, float alpha)
  {
    return new Color(Mathf.Clamp01(c.r + amount), Mathf.Clamp01(c.g + amount), Mathf.Clamp01(c.b + amount), alpha);
  }

  private static Color Darken(Color c, float amount, float alpha)
  {
    return new Color(Mathf.Clamp01(c.r - amount), Mathf.Clamp01(c.g - amount), Mathf.Clamp01(c.b - amount), alpha);
  }

  private static void DestroyTex(Texture2D tex)
  {
    if (!((UnityEngine.Object) tex != (UnityEngine.Object) null))
      return;
    UnityEngine.Object.Destroy((UnityEngine.Object) tex);
  }

  private static Texture2D MakeTex(Color color)
  {
    Texture2D texture2D = new Texture2D(2, 2);
    Color[] colorArray = new Color[4];
    for (int index = 0; index < 4; ++index)
      colorArray[index] = color;
    texture2D.SetPixels(colorArray);
    texture2D.Apply();
    return texture2D;
  }
}
