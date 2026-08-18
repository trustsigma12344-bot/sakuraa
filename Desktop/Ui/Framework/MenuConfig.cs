using SakuraaCastingMod.VR.UtilMenu.Utility;
using System;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Desktop.Ui.Framework;

public static class MenuConfig
{
  private static readonly Color FallbackMenuFill = new Color(0.07058824f, 0.07058824f, 0.07058824f, 0.7f);
  private static readonly Color FallbackOutline = new Color(1f, 0.7176471f, 0.772549033f);
  private static readonly Color FallbackButtonHover = new Color(1f, 0.4f, 0.6f);
  private static readonly Color FallbackButtonActive = new Color(0.3019608f, 0.149019614f, 0.0f);
  private static readonly Color FallbackText = new Color(1f, 1f, 1f);
  private static readonly Color FallbackInner = new Color(0.3f, 0.3f, 0.3f);
  private static float _themeFadeT = 1f;
  private static double _themeLastTime = -1.0;
  private static Color _prevFill;
  private static Color _prevHover;
  private static Color _prevActive;
  private static Color _prevOutline;
  private static Color _dispFill;
  private static Color _dispHover;
  private static Color _dispActive;
  private static Color _dispOutline;
  public static int OutlineThickness = 2;
  public static int CornerRadius = 8;
  public static int FontSize = 14;
  public static int HeaderFontSize = 16 /*0x10*/;
  public static float VerticalSpacing = 5f;
  public static int HorizontalPadding = 15;
  public static int VerticalPadding = 10;
  public static float DefaultMenuWidth = 200f;
  public static float HeaderHeight = 25f;
  public static float DefaultItemHeight = 25f;
  public static float DescriptionBoxWidth = 200f;
  public static float DescriptionBoxHeight = 60f;
  public static float DescriptionPadding = 5f;
  private static GUIStyle _menuBoxStyle;
  private static GUIStyle _buttonStyle;
  private static GUIStyle _labelStyle;
  private static GUIStyle _headerStyle;
  private static GUIStyle _textFieldStyle;
  private static GUIStyle _sliderStyle;
  private static GUIStyle _toggleStyle;
  private static GUIStyle _descriptionStyle;
  private static GUIStyle _centerTextStyle;
  private static GUIStyle _valueStyle;
  private static GUIStyle _wrapLabelStyle;
  private static GUIStyle _textFieldTransparentStyle;
  private static GUIStyle _fillBoxStyle;
  private static GUIStyle _outlineBoxStyle;
  private static Texture2D _menuBoxTexture;
  private static Texture2D _buttonTexture;
  private static Texture2D _buttonHoverTexture;
  private static Texture2D _buttonActiveTexture;
  private static Texture2D _sliderBackgroundTexture;
  private static Texture2D _sliderThumbTexture;
  private static Texture2D _descriptionBoxTexture;
  private static Texture2D _solidRoundedTexture;
  private static Texture2D _fillTexture;
  private static Texture2D _outlineTexture;
  private static Texture2D _gradientTexture;

  private static bool ThemeReady
  {
    get => ThemeManager.MaterialNames != null && ThemeManager.MaterialNames.Count > 0;
  }

  private static bool UseFallbacks
  {
    get
    {
      if (!MenuConfig.ThemeReady)
        return true;
      return ThemeManager.AvailableThemes.Count > ThemeManager.CurrentThemeIndex && ThemeManager.CurrentThemeIndex >= 0 && ThemeManager.AvailableThemes[ThemeManager.CurrentThemeIndex].Name == "DEFAULT";
    }
  }

  private static Color WithAlpha(Color c, float a) => new Color(c.r, c.g, c.b, a);

  public static Color MenuFillColor
  {
    get
    {
      return !MenuConfig.UseFallbacks ? MenuConfig.WithAlpha(ThemeManager.GetCustomColor("PANEL"), 0.7f) : MenuConfig.FallbackMenuFill;
    }
  }

  public static Color MenuOutlineColor
  {
    get
    {
      return !MenuConfig.UseFallbacks ? ThemeManager.GetCustomColor("SELECTED") : MenuConfig.FallbackOutline;
    }
  }

  public static Color ButtonHoverColor
  {
    get
    {
      return !MenuConfig.UseFallbacks ? ThemeManager.GetCustomColor("BUTTON") : MenuConfig.FallbackButtonHover;
    }
  }

  public static Color ButtonActiveColor
  {
    get
    {
      return !MenuConfig.UseFallbacks ? ThemeManager.GetCustomColor("PRESSED") : MenuConfig.FallbackButtonActive;
    }
  }

  public static Color TextColor
  {
    get
    {
      return !MenuConfig.UseFallbacks ? MenuConfig.WithAlpha(ThemeManager.GetCustomColor("TEXT 1"), 0.86f) : MenuConfig.WithAlpha(MenuConfig.FallbackText, 0.86f);
    }
  }

  public static Color DisabledColor
  {
    get
    {
      return !MenuConfig.UseFallbacks ? MenuConfig.WithAlpha(ThemeManager.GetCustomColor("TEXT 1"), 0.38f) : MenuConfig.WithAlpha(MenuConfig.FallbackText, 0.38f);
    }
  }

  public static Color SliderBgColor
  {
    get
    {
      return !MenuConfig.UseFallbacks ? ThemeManager.GetCustomColor("INNER") : MenuConfig.FallbackInner;
    }
  }

  public static Color DescriptionBgColor
  {
    get
    {
      return !MenuConfig.UseFallbacks ? MenuConfig.WithAlpha(ThemeManager.GetCustomColor("INNER"), 0.9f) : new Color(0.1f, 0.1f, 0.1f, 0.9f);
    }
  }

  private static Color Blend(Color from, Color to)
  {
    return Color.Lerp(from, to, UiAnim.EaseOutCubic(MenuConfig._themeFadeT));
  }

  public static Color FillColorNow
  {
    get
    {
      MenuConfig._dispFill = (double) MenuConfig._themeFadeT < 1.0 ? MenuConfig.Blend(MenuConfig._prevFill, MenuConfig.MenuFillColor) : MenuConfig.MenuFillColor;
      return MenuConfig._dispFill;
    }
  }

  public static Color HoverColorNow
  {
    get
    {
      MenuConfig._dispHover = (double) MenuConfig._themeFadeT < 1.0 ? MenuConfig.Blend(MenuConfig._prevHover, MenuConfig.ButtonHoverColor) : MenuConfig.ButtonHoverColor;
      return MenuConfig._dispHover;
    }
  }

  public static Color ActiveColorNow
  {
    get
    {
      MenuConfig._dispActive = (double) MenuConfig._themeFadeT < 1.0 ? MenuConfig.Blend(MenuConfig._prevActive, MenuConfig.ButtonActiveColor) : MenuConfig.ButtonActiveColor;
      return MenuConfig._dispActive;
    }
  }

  public static Color OutlineColorNow
  {
    get
    {
      MenuConfig._dispOutline = (double) MenuConfig._themeFadeT < 1.0 ? MenuConfig.Blend(MenuConfig._prevOutline, MenuConfig.MenuOutlineColor) : MenuConfig.MenuOutlineColor;
      return MenuConfig._dispOutline;
    }
  }

  public static Color CaretColorNow => new Color(1f, 1f, 1f, 0.95f);

  public static Color SelectionColorNow
  {
    get
    {
      Color hoverColorNow = MenuConfig.HoverColorNow;
      hoverColorNow.a = 0.45f;
      return hoverColorNow;
    }
  }

  public static void TickThemeFade()
  {
    if (Event.current.type != (EventType) 7)
      return;
    double sinceStartupAsDouble = Time.realtimeSinceStartupAsDouble;
    if (MenuConfig._themeLastTime < 0.0)
      MenuConfig._themeLastTime = sinceStartupAsDouble;
    float num = Mathf.Min((float) (sinceStartupAsDouble - MenuConfig._themeLastTime), 0.05f);
    MenuConfig._themeLastTime = sinceStartupAsDouble;
    if ((double) MenuConfig._themeFadeT >= 1.0)
      return;
    MenuConfig._themeFadeT = Mathf.Min(1f, MenuConfig._themeFadeT + num / 0.25f);
  }

  private static void BeginThemeFade()
  {
    MenuConfig._prevFill = MenuConfig._dispFill;
    MenuConfig._prevHover = MenuConfig._dispHover;
    MenuConfig._prevActive = MenuConfig._dispActive;
    MenuConfig._prevOutline = MenuConfig._dispOutline;
    MenuConfig._themeFadeT = 0.0f;
  }

  static MenuConfig()
  {
    ThemeManager.OnThemeApplied += new Action(MenuConfig.CleanUp);
    ThemeManager.OnThemeApplied += new Action(MenuConfig.BeginThemeFade);
  }

  private static Texture2D CreateRoundedTexture(
    Color fillColor,
    Color outlineColor,
    int width = 64 /*0x40*/,
    int height = 64 /*0x40*/)
  {
    Texture2D texture2D = new Texture2D(width, height, (TextureFormat) 5, false);
    Color color;
    // ISSUE: explicit constructor call
    color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
    float cornerRadius = (float) MenuConfig.CornerRadius;
    float num1 = cornerRadius - (float) MenuConfig.OutlineThickness;
    Texture2D roundedTexture;
    if ((double) cornerRadius <= 0.0)
    {
      roundedTexture = MenuConfig.CreateTexture(fillColor, outlineColor, width, height);
    }
    else
    {
      for (int index1 = 0; index1 < height; ++index1)
      {
        for (int index2 = 0; index2 < width; ++index2)
        {
          float num2 = 0.0f;
          float num3 = 0.0f;
          bool flag = false;
          if (((double) index2 >= (double) cornerRadius ? 0 : ((double) index1 < (double) cornerRadius ? 1 : 0)) == 0)
          {
            if (((double) index2 < (double) width - (double) cornerRadius ? 0 : ((double) index1 < (double) cornerRadius ? 1 : 0)) == 0)
            {
              if (((double) index2 >= (double) cornerRadius ? 0 : ((double) index1 >= (double) height - (double) cornerRadius ? 1 : 0)) == 0)
              {
                if (((double) index2 < (double) width - (double) cornerRadius ? 0 : ((double) index1 >= (double) height - (double) cornerRadius ? 1 : 0)) != 0)
                {
                  num2 = (float) index2 - (float) ((double) width - (double) cornerRadius - 1.0);
                  num3 = (float) index1 - (float) ((double) height - (double) cornerRadius - 1.0);
                  flag = true;
                }
              }
              else
              {
                num2 = (float) index2 - cornerRadius;
                num3 = (float) index1 - (float) ((double) height - (double) cornerRadius - 1.0);
                flag = true;
              }
            }
            else
            {
              num2 = (float) index2 - (float) ((double) width - (double) cornerRadius - 1.0);
              num3 = (float) index1 - cornerRadius;
              flag = true;
            }
          }
          else
          {
            num2 = (float) index2 - cornerRadius;
            num3 = (float) index1 - cornerRadius;
            flag = true;
          }
          if (flag)
          {
            float num4 = (float) ((double) num2 * (double) num2 + (double) num3 * (double) num3);
            float num5 = cornerRadius * cornerRadius;
            float num6 = num1 * num1;
            if ((double) num4 > (double) num5)
              texture2D.SetPixel(index2, index1, color);
            else if ((double) num4 <= (double) num6)
              texture2D.SetPixel(index2, index1, fillColor);
            else
              texture2D.SetPixel(index2, index1, outlineColor);
          }
          else if ((index2 < MenuConfig.OutlineThickness || index2 >= width - MenuConfig.OutlineThickness || index1 < MenuConfig.OutlineThickness ? 1 : (index1 >= height - MenuConfig.OutlineThickness ? 1 : 0)) != 0)
            texture2D.SetPixel(index2, index1, outlineColor);
          else
            texture2D.SetPixel(index2, index1, fillColor);
        }
      }
      texture2D.Apply();
      roundedTexture = texture2D;
    }
    return roundedTexture;
  }

  private static Texture2D CreateTexture(
    Color fillColor,
    Color outlineColor,
    int width = 64 /*0x40*/,
    int height = 64 /*0x40*/)
  {
    Texture2D texture = new Texture2D(width, height);
    for (int index1 = 0; index1 < height; ++index1)
    {
      for (int index2 = 0; index2 < width; ++index2)
      {
        if ((index2 < MenuConfig.OutlineThickness || index2 >= width - MenuConfig.OutlineThickness || index1 < MenuConfig.OutlineThickness ? 1 : (index1 >= height - MenuConfig.OutlineThickness ? 1 : 0)) != 0)
          texture.SetPixel(index2, index1, outlineColor);
        else
          texture.SetPixel(index2, index1, fillColor);
      }
    }
    texture.Apply();
    return texture;
  }

  public static GUIStyle GetMenuBoxStyle()
  {
    if (MenuConfig._menuBoxStyle == null)
    {
      MenuConfig._menuBoxTexture = MenuConfig.CreateRoundedTexture(MenuConfig.MenuFillColor, MenuConfig.MenuOutlineColor);
      MenuConfig._menuBoxStyle = new GUIStyle(GUI.skin.box)
      {
        normal = {
          background = MenuConfig._menuBoxTexture
        },
        border = new RectOffset(MenuConfig.CornerRadius, MenuConfig.CornerRadius, MenuConfig.CornerRadius, MenuConfig.CornerRadius),
        padding = new RectOffset(MenuConfig.HorizontalPadding, MenuConfig.HorizontalPadding, MenuConfig.VerticalPadding, MenuConfig.VerticalPadding),
        alignment = (TextAnchor) 4,
        fontSize = MenuConfig.FontSize
      };
    }
    return MenuConfig._menuBoxStyle;
  }

  public static GUIStyle GetButtonStyle()
  {
    if (MenuConfig._buttonStyle == null)
    {
      MenuConfig._buttonTexture = MenuConfig.CreateRoundedTexture(MenuConfig.MenuFillColor, MenuConfig.MenuOutlineColor);
      MenuConfig._buttonHoverTexture = MenuConfig.CreateRoundedTexture(MenuConfig.ButtonHoverColor, MenuConfig.MenuOutlineColor);
      MenuConfig._buttonActiveTexture = MenuConfig.CreateRoundedTexture(MenuConfig.ButtonActiveColor, MenuConfig.MenuOutlineColor);
      MenuConfig._buttonStyle = new GUIStyle(GUI.skin.button)
      {
        normal = {
          background = MenuConfig._buttonTexture,
          textColor = MenuConfig.TextColor
        },
        hover = {
          background = MenuConfig._buttonHoverTexture,
          textColor = MenuConfig.TextColor
        },
        active = {
          background = MenuConfig._buttonActiveTexture,
          textColor = MenuConfig.TextColor
        },
        border = new RectOffset(MenuConfig.CornerRadius, MenuConfig.CornerRadius, MenuConfig.CornerRadius, MenuConfig.CornerRadius),
        padding = new RectOffset(5, 5, 5, 5),
        alignment = (TextAnchor) 4,
        fontSize = MenuConfig.FontSize
      };
    }
    return MenuConfig._buttonStyle;
  }

  public static GUIStyle GetLabelStyle()
  {
    if (MenuConfig._labelStyle == null)
      MenuConfig._labelStyle = new GUIStyle(GUI.skin.label)
      {
        fontSize = MenuConfig.FontSize,
        normal = {
          textColor = MenuConfig.TextColor
        },
        alignment = (TextAnchor) 3
      };
    return MenuConfig._labelStyle;
  }

  public static GUIStyle GetCenterTextStyle()
  {
    if (MenuConfig._centerTextStyle == null)
      MenuConfig._centerTextStyle = new GUIStyle(MenuConfig.GetLabelStyle())
      {
        alignment = (TextAnchor) 4,
        richText = true
      };
    return MenuConfig._centerTextStyle;
  }

  public static GUIStyle GetWrapLabelStyle()
  {
    if (MenuConfig._wrapLabelStyle == null)
      MenuConfig._wrapLabelStyle = new GUIStyle(MenuConfig.GetLabelStyle())
      {
        wordWrap = true,
        richText = true,
        alignment = (TextAnchor) 0
      };
    return MenuConfig._wrapLabelStyle;
  }

  public static GUIStyle GetValueStyle()
  {
    if (MenuConfig._valueStyle == null)
      MenuConfig._valueStyle = new GUIStyle(MenuConfig.GetLabelStyle())
      {
        alignment = (TextAnchor) 5,
        richText = true
      };
    return MenuConfig._valueStyle;
  }

  public static GUIStyle GetHeaderStyle()
  {
    if (MenuConfig._headerStyle == null)
      MenuConfig._headerStyle = new GUIStyle(MenuConfig.GetLabelStyle())
      {
        fontSize = MenuConfig.HeaderFontSize,
        fontStyle = (FontStyle) 1,
        alignment = (TextAnchor) 1,
        wordWrap = true,
        clipping = (TextClipping) 0
      };
    return MenuConfig._headerStyle;
  }

  public static GUIStyle GetTextFieldStyle()
  {
    if (MenuConfig._textFieldStyle == null)
      MenuConfig._textFieldStyle = new GUIStyle(GUI.skin.textField)
      {
        fontSize = MenuConfig.FontSize,
        normal = {
          textColor = MenuConfig.TextColor
        }
      };
    return MenuConfig._textFieldStyle;
  }

  public static GUIStyle GetTextFieldTransparentStyle()
  {
    if (MenuConfig._textFieldTransparentStyle == null)
      MenuConfig._textFieldTransparentStyle = new GUIStyle(MenuConfig.GetTextFieldStyle())
      {
        normal = {
          background = (Texture2D) null,
          textColor = MenuConfig.TextColor
        },
        focused = {
          background = (Texture2D) null,
          textColor = MenuConfig.TextColor
        },
        hover = {
          background = (Texture2D) null,
          textColor = MenuConfig.TextColor
        },
        active = {
          background = (Texture2D) null,
          textColor = MenuConfig.TextColor
        },
        alignment = (TextAnchor) 3
      };
    return MenuConfig._textFieldTransparentStyle;
  }

  public static GUIStyle GetSliderStyle()
  {
    if (MenuConfig._sliderStyle == null)
    {
      MenuConfig._sliderBackgroundTexture = MenuConfig.CreateTexture(MenuConfig.SliderBgColor, MenuConfig.MenuOutlineColor);
      MenuConfig._sliderThumbTexture = MenuConfig.CreateTexture(MenuConfig.MenuOutlineColor, MenuConfig.MenuOutlineColor, 16 /*0x10*/, 16 /*0x10*/);
      MenuConfig._sliderStyle = new GUIStyle(GUI.skin.horizontalSlider)
      {
        normal = {
          background = MenuConfig._sliderBackgroundTexture
        }
      };
      GUI.skin.horizontalSliderThumb.normal.background = MenuConfig._sliderThumbTexture;
    }
    return MenuConfig._sliderStyle;
  }

  public static GUIStyle GetToggleStyle()
  {
    if (MenuConfig._toggleStyle == null)
      MenuConfig._toggleStyle = new GUIStyle(GUI.skin.toggle)
      {
        fontSize = MenuConfig.FontSize,
        normal = {
          textColor = MenuConfig.TextColor
        }
      };
    return MenuConfig._toggleStyle;
  }

  public static Texture2D GetSolidRoundedTexture()
  {
    if (((UnityEngine.Object) MenuConfig._solidRoundedTexture == (UnityEngine.Object) null))
      MenuConfig._solidRoundedTexture = MenuConfig.CreateRoundedTexture(Color.white, MenuConfig.MenuOutlineColor);
    return MenuConfig._solidRoundedTexture;
  }

  public static Texture2D GetFillTexture()
  {
    if (((UnityEngine.Object) MenuConfig._fillTexture == (UnityEngine.Object) null))
      MenuConfig._fillTexture = MenuConfig.CreateRoundedTexture(Color.white, Color.white);
    return MenuConfig._fillTexture;
  }

  public static Texture2D GetOutlineTexture()
  {
    if (((UnityEngine.Object) MenuConfig._outlineTexture == (UnityEngine.Object) null))
      MenuConfig._outlineTexture = MenuConfig.CreateRoundedTexture(new Color(1f, 1f, 1f, 0.0f), Color.white);
    return MenuConfig._outlineTexture;
  }

  public static Texture2D GetGradientTexture()
  {
    if (((UnityEngine.Object) MenuConfig._gradientTexture == (UnityEngine.Object) null))
    {
      Texture2D texture2D1 = new Texture2D(2, 64 /*0x40*/, (TextureFormat) 5, false);
      ((Texture) texture2D1).wrapMode = (TextureWrapMode) 1;
      Texture2D texture2D2 = texture2D1;
      for (int index = 0; index < 64 /*0x40*/; ++index)
      {
        float num = Mathf.Lerp(0.0f, 0.07f, (float) index / 63f);
        Color color;
        // ISSUE: explicit constructor call
        color = new Color(1f, 1f, 1f, num);
        texture2D2.SetPixel(0, index, color);
        texture2D2.SetPixel(1, index, color);
      }
      texture2D2.Apply();
      MenuConfig._gradientTexture = texture2D2;
    }
    return MenuConfig._gradientTexture;
  }

  public static GUIStyle GetFillBoxStyle()
  {
    if (MenuConfig._fillBoxStyle == null)
      MenuConfig._fillBoxStyle = new GUIStyle()
      {
        normal = {
          background = MenuConfig.GetFillTexture()
        },
        border = new RectOffset(MenuConfig.CornerRadius, MenuConfig.CornerRadius, MenuConfig.CornerRadius, MenuConfig.CornerRadius)
      };
    return MenuConfig._fillBoxStyle;
  }

  public static GUIStyle GetOutlineBoxStyle()
  {
    if (MenuConfig._outlineBoxStyle == null)
      MenuConfig._outlineBoxStyle = new GUIStyle()
      {
        normal = {
          background = MenuConfig.GetOutlineTexture()
        },
        border = new RectOffset(MenuConfig.CornerRadius, MenuConfig.CornerRadius, MenuConfig.CornerRadius, MenuConfig.CornerRadius)
      };
    return MenuConfig._outlineBoxStyle;
  }

  public static GUIStyle GetDescriptionStyle()
  {
    if (MenuConfig._descriptionStyle == null)
    {
      MenuConfig._descriptionBoxTexture = MenuConfig.CreateRoundedTexture(MenuConfig.DescriptionBgColor, MenuConfig.MenuOutlineColor);
      MenuConfig._descriptionStyle = new GUIStyle(GUI.skin.box)
      {
        normal = {
          background = MenuConfig._descriptionBoxTexture,
          textColor = MenuConfig.TextColor
        },
        border = new RectOffset(MenuConfig.CornerRadius, MenuConfig.CornerRadius, MenuConfig.CornerRadius, MenuConfig.CornerRadius),
        padding = new RectOffset(5, 5, 5, 5),
        wordWrap = true,
        fontSize = MenuConfig.FontSize - 2,
        alignment = (TextAnchor) 0
      };
    }
    return MenuConfig._descriptionStyle;
  }

  public static void CleanUp()
  {
    if (((UnityEngine.Object) MenuConfig._menuBoxTexture != (UnityEngine.Object) null))
      UnityEngine.Object.Destroy((UnityEngine.Object) MenuConfig._menuBoxTexture);
    if (((UnityEngine.Object) MenuConfig._buttonTexture != (UnityEngine.Object) null))
      UnityEngine.Object.Destroy((UnityEngine.Object) MenuConfig._buttonTexture);
    if (((UnityEngine.Object) MenuConfig._buttonHoverTexture != (UnityEngine.Object) null))
      UnityEngine.Object.Destroy((UnityEngine.Object) MenuConfig._buttonHoverTexture);
    if (((UnityEngine.Object) MenuConfig._buttonActiveTexture != (UnityEngine.Object) null))
      UnityEngine.Object.Destroy((UnityEngine.Object) MenuConfig._buttonActiveTexture);
    if (((UnityEngine.Object) MenuConfig._sliderBackgroundTexture != (UnityEngine.Object) null))
      UnityEngine.Object.Destroy((UnityEngine.Object) MenuConfig._sliderBackgroundTexture);
    if (((UnityEngine.Object) MenuConfig._sliderThumbTexture != (UnityEngine.Object) null))
      UnityEngine.Object.Destroy((UnityEngine.Object) MenuConfig._sliderThumbTexture);
    if (((UnityEngine.Object) MenuConfig._descriptionBoxTexture != (UnityEngine.Object) null))
      UnityEngine.Object.Destroy((UnityEngine.Object) MenuConfig._descriptionBoxTexture);
    if (((UnityEngine.Object) MenuConfig._solidRoundedTexture != (UnityEngine.Object) null))
      UnityEngine.Object.Destroy((UnityEngine.Object) MenuConfig._solidRoundedTexture);
    MenuConfig._solidRoundedTexture = (Texture2D) null;
    if (((UnityEngine.Object) MenuConfig._fillTexture != (UnityEngine.Object) null))
      UnityEngine.Object.Destroy((UnityEngine.Object) MenuConfig._fillTexture);
    if (((UnityEngine.Object) MenuConfig._outlineTexture != (UnityEngine.Object) null))
      UnityEngine.Object.Destroy((UnityEngine.Object) MenuConfig._outlineTexture);
    if (((UnityEngine.Object) MenuConfig._gradientTexture != (UnityEngine.Object) null))
      UnityEngine.Object.Destroy((UnityEngine.Object) MenuConfig._gradientTexture);
    MenuConfig._fillTexture = (Texture2D) null;
    MenuConfig._outlineTexture = (Texture2D) null;
    MenuConfig._gradientTexture = (Texture2D) null;
    MenuConfig._menuBoxStyle = (GUIStyle) null;
    MenuConfig._buttonStyle = (GUIStyle) null;
    MenuConfig._labelStyle = (GUIStyle) null;
    MenuConfig._headerStyle = (GUIStyle) null;
    MenuConfig._textFieldStyle = (GUIStyle) null;
    MenuConfig._sliderStyle = (GUIStyle) null;
    MenuConfig._toggleStyle = (GUIStyle) null;
    MenuConfig._descriptionStyle = (GUIStyle) null;
    MenuConfig._centerTextStyle = (GUIStyle) null;
    MenuConfig._valueStyle = (GUIStyle) null;
    MenuConfig._wrapLabelStyle = (GUIStyle) null;
    MenuConfig._textFieldTransparentStyle = (GUIStyle) null;
    MenuConfig._fillBoxStyle = (GUIStyle) null;
    MenuConfig._outlineBoxStyle = (GUIStyle) null;
  }
}
