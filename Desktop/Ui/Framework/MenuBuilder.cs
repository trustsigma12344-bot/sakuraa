using SakuraaCastingMod.Desktop.Ui.Framework.MenuItems;
using SakuraaCastingMod.Shared.Helpers;
using SakuraaOfflinePresets;
using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Desktop.Ui.Framework;

public class MenuBuilder
{
  public readonly string _title;
  public readonly List<MenuItem> Items = new List<MenuItem>();
  public Rect MenuRect;
  private Vector2 _scrollPosition = Vector2.zero;
  private readonly float _menuWidth;
  private bool _isDragging;
  private Vector2 _dragOffset;
  private bool _useScrollView;
  private float _maxHeight;
  private Action<MenuBuilder> _onDrawComplete;
  private float _fullHeight;
  private readonly List<MenuItem> _visibleItemsCache = new List<MenuItem>();
  private readonly List<float> _visibleHeights = new List<float>();
  private readonly List<MenuBuilder.MenuPage> _pages = new List<MenuBuilder.MenuPage>();
  private int _pagesBuiltCount = -1;
  private int _currentPage = 0;
  public static object ActiveDragOwner;
  private readonly AnimState _pagerPrev = new AnimState();
  private readonly AnimState _pagerNext = new AnimState();
  private float _animHeight = -1f;
  private float _animHeightVel;
  private float _dt;
  private double _lastRepaintTime = -1.0;
  private float _scrollVel;
  private float _scrollbarAlpha;
  private float _intro = 1f;
  private const float IntroDur = 0.18f;
  private const float IntroFromScale = 0.96f;
  private const float IntroRiseY = 6f;
  private static bool _drawErrorLogged;

  public bool IsMinimized { get; set; } = false;

  private float PagerHeight
  {
    get
    {
      return this._pages.Count <= 1 ? 0.0f : MenuConfig.DefaultItemHeight + MenuConfig.VerticalSpacing;
    }
  }

  public MenuBuilder(string title, float width = 0.0f)
  {
    this._title = title;
    this._menuWidth = (double) width > 0.0 ? width : MenuConfig.DefaultMenuWidth;
    Vector2 screen = GuiManager.RefToScreen(new Vector2(100f, 100f));
    this.MenuRect = new Rect(screen.x, screen.y, this._menuWidth, 100f);
    this._maxHeight = (float) Screen.height * 0.8f;
    GuiManager.RegisterMenu(this);
  }

  public MenuBuilder SetPosition(float x, float y)
  {
    this.MenuRect.x = x;
    this.MenuRect.y = y;
    this.MenuRect.x = Mathf.Clamp(this.MenuRect.x, 0.0f, (float) Screen.width - this.MenuRect.width);
    this.MenuRect.y = Mathf.Clamp(this.MenuRect.y, 0.0f, (float) Screen.height - this.MenuRect.height);
    return this;
  }

  public MenuBuilder SetPositionRef(float refX, float refY)
  {
    Vector2 screen = GuiManager.RefToScreen(new Vector2(refX, refY));
    return this.SetPosition(screen.x, screen.y);
  }

  public MenuBuilder SetMaxHeight(float maxHeight)
  {
    this._maxHeight = maxHeight;
    return this;
  }

  public MenuBuilder OnDrawComplete(Action<MenuBuilder> callback)
  {
    this._onDrawComplete = callback;
    return this;
  }

  public MenuBuilder AddItem(MenuItem item)
  {
    this.Items.Add(item);
    return this;
  }

  public MenuBuilder AddPage(string title) => this.AddItem((MenuItem) new PageBreakMenuItem(title));

  public MenuBuilder AddSection(string title)
  {
    this.AddSpace(8f);
    return this.AddItem((MenuItem) new LabelMenuItem()
    {
      Label = title,
      IsHeader = true
    });
  }

  public MenuBuilder AddButton(
    string label,
    Action onClick,
    string description = null,
    Func<bool> visibilityCondition = null)
  {
    ButtonMenuItem buttonMenuItem1 = new ButtonMenuItem();
    buttonMenuItem1.Label = label;
    buttonMenuItem1.OnClick = onClick;
    buttonMenuItem1.Description = description;
    ButtonMenuItem buttonMenuItem2 = buttonMenuItem1;
    if (visibilityCondition != null)
      buttonMenuItem2.IsVisible = visibilityCondition;
    return this.AddItem((MenuItem) buttonMenuItem2);
  }

  public MenuBuilder AddDynamicButton(
    Func<string> labelGetter,
    Action onClick,
    string description = null,
    Func<bool> visibilityCondition = null)
  {
    ButtonMenuItem buttonMenuItem1 = new ButtonMenuItem();
    buttonMenuItem1.OnClick = onClick;
    buttonMenuItem1.Description = description;
    ButtonMenuItem buttonMenuItem2 = buttonMenuItem1;
    buttonMenuItem2.SetDynamicLabel(labelGetter);
    if (visibilityCondition != null)
      buttonMenuItem2.IsVisible = visibilityCondition;
    return this.AddItem((MenuItem) buttonMenuItem2);
  }

  public MenuBuilder AddSlider(
    string label,
    float value,
    float min,
    float max,
    Action<float> onValueChanged,
    int decimals = 0,
    string description = null,
    Func<bool> visibilityCondition = null)
  {
    SliderMenuItem sliderMenuItem1 = new SliderMenuItem();
    sliderMenuItem1.Label = label;
    sliderMenuItem1.Value = value;
    sliderMenuItem1.MinValue = min;
    sliderMenuItem1.MaxValue = max;
    sliderMenuItem1.Decimals = decimals;
    sliderMenuItem1.OnValueChanged = onValueChanged;
    sliderMenuItem1.Description = description;
    SliderMenuItem sliderMenuItem2 = sliderMenuItem1;
    if (visibilityCondition != null)
      sliderMenuItem2.IsVisible = visibilityCondition;
    return this.AddItem((MenuItem) sliderMenuItem2);
  }

  public MenuBuilder AddTextField(
    string label,
    string text,
    Action<string> onTextChanged,
    int maxLength = 0,
    string description = null,
    Func<bool> visibilityCondition = null,
    bool forceUpperCase = false,
    bool disallowSpaces = false)
  {
    TextFieldMenuItem textFieldMenuItem1 = new TextFieldMenuItem();
    textFieldMenuItem1.Label = label;
    textFieldMenuItem1.Text = text;
    textFieldMenuItem1.MaxLength = maxLength;
    textFieldMenuItem1.OnTextChanged = onTextChanged;
    textFieldMenuItem1.Description = description;
    textFieldMenuItem1.ForceUpperCase = forceUpperCase;
    textFieldMenuItem1.DisallowSpaces = disallowSpaces;
    TextFieldMenuItem textFieldMenuItem2 = textFieldMenuItem1;
    if (visibilityCondition != null)
      textFieldMenuItem2.IsVisible = visibilityCondition;
    return this.AddItem((MenuItem) textFieldMenuItem2);
  }

  public MenuBuilder AddToggle(
    string label,
    bool isToggled,
    Action<bool> onToggled,
    string description = null,
    Func<bool> visibilityCondition = null)
  {
    SwitchMenuItem switchMenuItem1 = new SwitchMenuItem();
    switchMenuItem1.Label = label;
    switchMenuItem1.IsToggled = isToggled;
    switchMenuItem1.OnToggled = onToggled;
    switchMenuItem1.Description = description;
    SwitchMenuItem switchMenuItem2 = switchMenuItem1;
    if (visibilityCondition != null)
      switchMenuItem2.IsVisible = visibilityCondition;
    return this.AddItem((MenuItem) switchMenuItem2);
  }

  public MenuBuilder AddDynamicToggle(
    string label,
    Func<bool> stateGetter,
    Action<bool> onToggled,
    string description = null,
    Func<bool> visibilityCondition = null)
  {
    SwitchMenuItem switchMenuItem1 = new SwitchMenuItem();
    switchMenuItem1.Label = label;
    switchMenuItem1.StateGetter = stateGetter;
    switchMenuItem1.IsToggled = stateGetter != null && stateGetter();
    switchMenuItem1.OnToggled = onToggled;
    switchMenuItem1.Description = description;
    SwitchMenuItem switchMenuItem2 = switchMenuItem1;
    if (visibilityCondition != null)
      switchMenuItem2.IsVisible = visibilityCondition;
    return this.AddItem((MenuItem) switchMenuItem2);
  }

  public MenuBuilder AddLabel(
    string text,
    bool isHeader = false,
    string description = null,
    Func<bool> visibilityCondition = null)
  {
    LabelMenuItem labelMenuItem1 = new LabelMenuItem();
    labelMenuItem1.Label = text;
    labelMenuItem1.IsHeader = isHeader;
    labelMenuItem1.Description = description;
    LabelMenuItem labelMenuItem2 = labelMenuItem1;
    if (visibilityCondition != null)
      labelMenuItem2.IsVisible = visibilityCondition;
    return this.AddItem((MenuItem) labelMenuItem2);
  }

  public MenuBuilder AddDynamicLabel(
    Func<string> labelGetter,
    bool isHeader = false,
    string description = null,
    Func<bool> visibilityCondition = null)
  {
    LabelMenuItem labelMenuItem1 = new LabelMenuItem();
    labelMenuItem1.IsHeader = isHeader;
    labelMenuItem1.Description = description;
    LabelMenuItem labelMenuItem2 = labelMenuItem1;
    labelMenuItem2.SetDynamicLabel(labelGetter);
    if (visibilityCondition != null)
      labelMenuItem2.IsVisible = visibilityCondition;
    return this.AddItem((MenuItem) labelMenuItem2);
  }

  public MenuBuilder AddDropdown(
    string labelPrefix,
    Func<IList<string>> getOptions,
    Func<int> getSelectedIndex,
    Action<int> onSelected,
    string description = null,
    Func<bool> visibilityCondition = null)
  {
    DropdownMenuItem dropdownMenuItem1 = new DropdownMenuItem();
    dropdownMenuItem1.LabelPrefix = labelPrefix;
    dropdownMenuItem1.GetOptions = getOptions;
    dropdownMenuItem1.GetSelectedIndex = getSelectedIndex;
    dropdownMenuItem1.OnSelected = onSelected;
    dropdownMenuItem1.Description = description;
    DropdownMenuItem dropdownMenuItem2 = dropdownMenuItem1;
    if (visibilityCondition != null)
      dropdownMenuItem2.IsVisible = visibilityCondition;
    return this.AddItem((MenuItem) dropdownMenuItem2);
  }

  public MenuBuilder AddHoldButton(
    string baseLabel,
    float holdDuration,
    Action onHoldComplete,
    string description = null,
    Func<bool> visibilityCondition = null)
  {
    HoldButtonMenuItem holdButtonMenuItem1 = new HoldButtonMenuItem();
    holdButtonMenuItem1.BaseLabel = baseLabel;
    holdButtonMenuItem1.HoldDuration = holdDuration;
    holdButtonMenuItem1.OnHoldComplete = onHoldComplete;
    holdButtonMenuItem1.Description = description;
    HoldButtonMenuItem holdButtonMenuItem2 = holdButtonMenuItem1;
    if (visibilityCondition != null)
      holdButtonMenuItem2.IsVisible = visibilityCondition;
    return this.AddItem((MenuItem) holdButtonMenuItem2);
  }

  public MenuBuilder AddSpace(float height = 10f, Func<bool> visibilityCondition = null)
  {
    LabelMenuItem labelMenuItem1 = new LabelMenuItem();
    labelMenuItem1.Label = "";
    labelMenuItem1.Height = height;
    LabelMenuItem labelMenuItem2 = labelMenuItem1;
    if (visibilityCondition != null)
      labelMenuItem2.IsVisible = visibilityCondition;
    return this.AddItem((MenuItem) labelMenuItem2);
  }

  private void EnsurePages()
  {
    if (this._pagesBuiltCount == this.Items.Count)
      return;
    this._pagesBuiltCount = this.Items.Count;
    this._pages.Clear();
    MenuBuilder.MenuPage menuPage = new MenuBuilder.MenuPage((string) null);
    foreach (MenuItem menuItem in this.Items)
    {
      if (menuItem is PageBreakMenuItem pageBreakMenuItem)
      {
        if (menuPage.Items.Count > 0)
          this._pages.Add(menuPage);
        menuPage = new MenuBuilder.MenuPage(pageBreakMenuItem.PageTitle);
      }
      else
        menuPage.Items.Add(menuItem);
    }
    this._pages.Add(menuPage);
    if (this._currentPage >= this._pages.Count)
      this._currentPage = this._pages.Count - 1;
    if (this._currentPage >= 0)
      return;
    this._currentPage = 0;
  }

  private string CurrentPageTitle()
  {
    string title = this._pages[this._currentPage].Title;
    return string.IsNullOrEmpty(title) ? $"Page {this._currentPage + 1}" : title;
  }

  private float RefreshVisibleItems()
  {
    this._visibleItemsCache.Clear();
    this._visibleHeights.Clear();
    float num1 = 0.0f;
    float verticalSpacing = MenuConfig.VerticalSpacing;
    float width = (float) ((double) this._menuWidth - (double) (MenuConfig.HorizontalPadding * 2) - 20.0);
    List<MenuItem> items = this._pages[this._currentPage].Items;
    for (int index = 0; index < items.Count; ++index)
    {
      MenuItem menuItem = items[index];
      if (menuItem.IsVisible())
      {
        float num2 = menuItem.Measure(width);
        if (this._visibleItemsCache.Count > 0)
          num1 += verticalSpacing;
        num1 += num2;
        this._visibleItemsCache.Add(menuItem);
        this._visibleHeights.Add(num2);
      }
    }
    return num1;
  }

  private void CalculateLayout(float contentHeight)
  {
    float num = (float) (MenuConfig.VerticalPadding * 2) + MenuConfig.HeaderHeight + this.PagerHeight + contentHeight;
    this._useScrollView = (double) num > (double) this._maxHeight;
    this._fullHeight = this._useScrollView ? this._maxHeight : num;
    float target = this.IsMinimized ? (float) ((double) MenuConfig.HeaderHeight + (double) (MenuConfig.VerticalPadding * 2) + 10.0) : this._fullHeight;
    if ((double) this._animHeight < 0.0)
      this._animHeight = target;
    else if (Event.current.type == (EventType) 7)
      UiAnim.Spring(ref this._animHeight, ref this._animHeightVel, target, 9f, this._dt);
    this.MenuRect.height = this._animHeight;
    this.MenuRect.width = this._menuWidth;
    this.MenuRect.x = Mathf.Clamp(this.MenuRect.x, 0.0f, (float) Screen.width - this.MenuRect.width);
    this.MenuRect.y = Mathf.Clamp(this.MenuRect.y, 0.0f, Mathf.Max(0.0f, (float) Screen.height - target));
  }

  private void HandleInput(Event evt, Rect headerRect)
  {
    if (((int) evt.type != 0 || !headerRect.Contains(evt.mousePosition) ? 0 : (evt.button == 1 ? 1 : 0)) != 0)
    {
      this.IsMinimized = !this.IsMinimized;
      Sounds.PlayCasterClick(Sounds.subtleClickSfx);
      this.CalculateLayout(this.RefreshVisibleItems());
      evt.Use();
    }
    else
    {
      switch ((int) evt.type)
      {
        case 0:
          if (!headerRect.Contains(evt.mousePosition) || evt.button != 0)
            break;
          this._isDragging = true;
          InputDiag.NoteHeaderDrag(this._title);
          this._dragOffset = (evt.mousePosition - new Vector2(this.MenuRect.x, this.MenuRect.y));
          evt.Use();
          break;
        case 1:
          if (evt.button != 0 || !this._isDragging)
            break;
          this._isDragging = false;
          Action<MenuBuilder> onDrawComplete = this._onDrawComplete;
          if (onDrawComplete != null)
            onDrawComplete(this);
          evt.Use();
          break;
        case 3:
          if (!this._isDragging)
            break;
          float x = evt.mousePosition.x - this._dragOffset.x;
          float y = evt.mousePosition.y - this._dragOffset.y;
          if (evt.shift)
            MenuSnap.Apply(ref x, ref y, this.MenuRect.width, this.MenuRect.height);
          this.MenuRect.x = Mathf.Clamp(x, 0.0f, (float) Screen.width - this.MenuRect.width);
          this.MenuRect.y = Mathf.Clamp(y, 0.0f, (float) Screen.height - this.MenuRect.height);
          evt.Use();
          break;
      }
    }
  }

  public void BeginIntro() => this._intro = 0.0f;

  public void SetAnimHeight(float from)
  {
    this._animHeight = from;
    this._animHeightVel = 0.0f;
  }

  public void Draw()
  {
    if (this.Items.Count == 0)
      return;
    this.EnsurePages();
    if (Event.current.type == (EventType) 7)
    {
      double sinceStartupAsDouble = Time.realtimeSinceStartupAsDouble;
      if (this._lastRepaintTime < 0.0)
        this._lastRepaintTime = sinceStartupAsDouble;
      this._dt = Mathf.Min((float) (sinceStartupAsDouble - this._lastRepaintTime), 0.05f);
      this._lastRepaintTime = sinceStartupAsDouble;
      if ((double) this._intro < 1.0)
        this._intro = Mathf.Min(1f, this._intro + this._dt / 0.18f);
    }
    float contentHeight = this.RefreshVisibleItems();
    this.CalculateLayout(contentHeight);
    OfflinePresetStore.EnhanceDesktopMenu((object) this);
    bool flag = (double) this._intro < 1.0;
    Matrix4x4 matrix = GUI.matrix;
    if (flag)
    {
      float num1 = UiAnim.EaseOutCubic(this._intro);
      float num2 = Mathf.Lerp(0.96f, 1f, num1);
      float num3 = Mathf.Lerp(6f, 0.0f, num1);
      GUIUtility.ScaleAroundPivot(new Vector2(num2, num2), new Vector2(this.MenuRect.x + this.MenuRect.width * 0.5f, this.MenuRect.y - num3));
    }
    try
    {
      Event current = Event.current;
      InputDiag.NoteMenu(this._title, this.MenuRect, current);
      if (current.type == (EventType) 7)
      {
        Color color = GUI.color;
        GUI.color = new Color(0.0f, 0.0f, 0.0f, 0.35f);
        GUI.Box(new Rect(this.MenuRect.x - 2f, this.MenuRect.y + 4f, this.MenuRect.width + 4f, this.MenuRect.height + 2f), GUIContent.none, MenuConfig.GetFillBoxStyle());
        GUI.color = color;
      }
      GUI.Box(this.MenuRect, "", MenuConfig.GetMenuBoxStyle());
      Rect headerRect;
      // ISSUE: explicit constructor call
      headerRect = new Rect(this.MenuRect.x, this.MenuRect.y, this.MenuRect.width, MenuConfig.HeaderHeight + (float) MenuConfig.VerticalPadding);
      Rect rect1;
      // ISSUE: explicit constructor call
      rect1 = new Rect(this.MenuRect.x + (float) MenuConfig.HorizontalPadding, this.MenuRect.y + (float) MenuConfig.VerticalPadding, this.MenuRect.width - (float) (MenuConfig.HorizontalPadding * 2), MenuConfig.HeaderHeight);
      OfflinePresetStore.DrawDesktopMenuHeader(rect1, this._title, MenuConfig.GetHeaderStyle());
      this.HandleInput(current, headerRect);
      if ((!this._isDragging ? 0 : (current.shift ? 1 : 0)) != 0)
        MenuSnap.KeepAlive();
      if (!this.IsMinimized)
      {
        if (this._pages.Count > 1)
        {
          float num4 = this.MenuRect.y + MenuConfig.HeaderHeight + (float) MenuConfig.VerticalPadding;
          float num5 = this.MenuRect.x + (float) MenuConfig.HorizontalPadding;
          float num6 = this.MenuRect.width - (float) (MenuConfig.HorizontalPadding * 2);
          float defaultItemHeight = MenuConfig.DefaultItemHeight;
          Rect rect2;
          // ISSUE: explicit constructor call
          rect2 = new Rect(num5, num4, 28f, defaultItemHeight);
          if (ControlChrome.Draw(rect2, this._pagerPrev, clickSound: Sounds.subtleClickSfx).Clicked)
          {
            this._currentPage = (this._currentPage - 1 + this._pages.Count) % this._pages.Count;
            this._scrollPosition = Vector2.zero;
          }
          GUI.Label(rect2, "<", MenuConfig.GetCenterTextStyle());
          Rect rect3;
          // ISSUE: explicit constructor call
          rect3 = new Rect((float) ((double) num5 + (double) num6 - 28.0), num4, 28f, defaultItemHeight);
          if (ControlChrome.Draw(rect3, this._pagerNext, clickSound: Sounds.subtleClickSfx).Clicked)
          {
            this._currentPage = (this._currentPage + 1) % this._pages.Count;
            this._scrollPosition = Vector2.zero;
          }
          GUI.Label(rect3, ">", MenuConfig.GetCenterTextStyle());
          GUI.Label(new Rect(num5 + 28f, num4, num6 - 56f, defaultItemHeight), $"{this.CurrentPageTitle()} ({this._currentPage + 1}/{this._pages.Count})", MenuConfig.GetHeaderStyle());
        }
        float num7 = this.MenuRect.y + MenuConfig.HeaderHeight + (float) MenuConfig.VerticalPadding + this.PagerHeight;
        float num8 = this.MenuRect.height - (MenuConfig.HeaderHeight + (float) (MenuConfig.VerticalPadding * 2) + this.PagerHeight);
        if ((double) num8 <= 0.0)
          return;
        Rect rect4;
        // ISSUE: explicit constructor call
        rect4 = new Rect(this.MenuRect.x + (float) MenuConfig.HorizontalPadding, num7, this.MenuRect.width - (float) (MenuConfig.HorizontalPadding * 2), num8);
        float verticalSpacing = MenuConfig.VerticalSpacing;
        if (this._useScrollView)
        {
          float width = rect4.width;
          float num9 = Mathf.Max(0.0f, contentHeight - rect4.height);
          if ((current.type != (EventType) 6 ? 0 : (rect4.Contains(current.mousePosition) ? 1 : 0)) != 0)
          {
            this._scrollVel += current.delta.y * 40f;
            this._scrollbarAlpha = 1f;
            current.Use();
          }
          if (((int) current.type != 0 ? 0 : (rect4.Contains(current.mousePosition) ? 1 : 0)) != 0)
            this._scrollVel = 0.0f;
          if (current.type == (EventType) 7)
          {
            this._scrollPosition.y += this._scrollVel * this._dt;
            this._scrollVel *= Mathf.Exp(-8f * this._dt);
            if ((double) Mathf.Abs(this._scrollVel) < 1.0)
              this._scrollVel = 0.0f;
            this._scrollPosition.y = Mathf.Clamp(this._scrollPosition.y, 0.0f, num9);
            this._scrollbarAlpha = Mathf.Max(0.0f, this._scrollbarAlpha - this._dt * 1.5f);
          }
          Rect rect5;
          // ISSUE: explicit constructor call
          rect5 = new Rect(0.0f, 0.0f, width, contentHeight);
          this._scrollPosition = OfflinePresetStore.BeginDesktopScrollView((object) this, rect4, this._scrollPosition, rect5, GUIStyle.none, GUIStyle.none);
          try
          {
            float num10 = 0.0f;
            float viewTop = this._scrollPosition.y;
            float viewBottom = viewTop + rect4.height;
            for (int index = 0; index < this._visibleItemsCache.Count; ++index)
            {
              float itemHeight = this._visibleHeights[index];
              // Items scrolled out of view are clipped away visually but were still being
              // hit-tested, so an invisible item could swallow the click meant for whatever
              // is actually on screen at that spot. Don't draw them at all.
              if (num10 + itemHeight >= viewTop && num10 <= viewBottom)
                MenuBuilder.DrawItemSafe(this._visibleItemsCache[index], new Rect(0.0f, num10, width, itemHeight));
              num10 += itemHeight + verticalSpacing;
            }
          }
          finally
          {
            GUI.EndScrollView();
          }
          if ((current.type != (EventType) 7 || (double) num9 <= 0.0 ? 0 : ((double) this._scrollbarAlpha > 0.0099999997764825821 ? 1 : 0)) != 0)
          {
            float height = rect4.height;
            float num11 = Mathf.Max(20f, height * (rect4.height / contentHeight));
            float num12 = rect4.y + (float) (((double) height - (double) num11) * ((double) this._scrollPosition.y / (double) num9));
            Color outlineColorNow = MenuConfig.OutlineColorNow;
            outlineColorNow.a *= this._scrollbarAlpha;
            ControlChrome.FillRect(new Rect(rect4.xMax + 4f, num12, 1.5f, num11), outlineColorNow);
          }
        }
        else
        {
          float y = rect4.y;
          float width = rect4.width;
          float x = rect4.x;
          for (int index = 0; index < this._visibleItemsCache.Count; ++index)
          {
            MenuBuilder.DrawItemSafe(this._visibleItemsCache[index], new Rect(x, y, width, this._visibleHeights[index]));
            y += this._visibleHeights[index] + verticalSpacing;
          }
        }
      }
      else
        GUI.Label(new Rect(this.MenuRect.xMax - 15f, this.MenuRect.y + 5f, 20f, 20f), "*", MenuConfig.GetLabelStyle());
      OfflinePresetStore.DrawDesktopMenuHeaderForeground((object) this);
    }
    catch (Exception ex)
    {
      if (MenuBuilder._drawErrorLogged)
        return;
      UnityEngine.Debug.LogError((object) $"[MenuBuilder] '{this._title}' draw failed: {ex}");
      MenuBuilder._drawErrorLogged = true;
    }
    finally
    {
      if (flag)
        GUI.matrix = matrix;
    }
  }

  private static void DrawItemSafe(MenuItem item, Rect rect)
  {
    try
    {
      item.Draw(rect);
    }
    catch (Exception ex)
    {
      if (MenuBuilder._drawErrorLogged)
        return;
      UnityEngine.Debug.LogError((object) $"[MenuBuilder] item draw failed: {ex}");
      MenuBuilder._drawErrorLogged = true;
    }
  }

  private class MenuPage
  {
    public readonly string Title;
    public readonly List<MenuItem> Items = new List<MenuItem>();

    public MenuPage(string title) => this.Title = title;
  }
}
