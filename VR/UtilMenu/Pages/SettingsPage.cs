using GorillaNetworking;
using SakuraaCastingMod.Core;
using SakuraaCastingMod.Features.Overlays;
using SakuraaCastingMod.Shared.Helpers;
using SakuraaCastingMod.Shared.Models;
using SakuraaCastingMod.VR.Interaction;
using SakuraaCastingMod.VR.UtilMenu.Utility;
using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.VR.UtilMenu.Pages;

public class SettingsPage : BasePage
{
  private int _profileOffset = 0;
  private string _profileInput = "";
  private bool _isTypingProfile = false;
  private int _seenPresetVersion = -1;
  private bool _isEditingName = false;
  private string _nameInput = "";
  private SettingsPage.OffsetsView _offsetsView = SettingsPage.OffsetsView.Selection;
  private const float PosStep = 0.005f;
  private const float PosMin = -0.2f;
  private const float PosMax = 0.2f;
  private const float RotStep = 1f;
  private const float RotMin = -180f;
  private const float RotMax = 180f;

  public override string PageName => "SETTINGS";

  public override Material PageIcon => UtilMenuMain.Instance.Icons.FileSettingsIcon;

  public override void BuildTabs()
  {
    this.Tabs.Add(new UtilTab()
    {
      TabIcon = UtilMenuMain.Instance.Icons.Save,
      Description = (string) null
    });
    this.Tabs.Add(new UtilTab()
    {
      TabIcon = UtilMenuMain.Instance.Icons.ClosedBook,
      Description = (string) null
    });
    this.Tabs.Add(new UtilTab()
    {
      TabIcon = UtilMenuMain.Instance.Icons.FileSettingsIcon,
      Description = (string) null
    });
    this.Tabs.Add(new UtilTab()
    {
      TabIcon = UtilMenuMain.Instance.Icons.MoveArrows,
      Description = (string) null
    });
    this.RefreshGeneralTab();
    this.RefreshProfileTab();
    this.RefreshInputTab();
    this.RefreshOffsetsTab();
  }

  public override void OnTabSelected(int tabIndex)
  {
    if (tabIndex == 0)
      this.RefreshGeneralTab();
    if (tabIndex == 1)
      this.RefreshProfileTab();
    if (tabIndex == 2)
      this.RefreshInputTab();
    if (tabIndex != 3)
      return;
    this.RefreshOffsetsTab();
  }

  public override void LateUpdate()
  {
    if (Configuration.PresetListVersion == this._seenPresetVersion)
      return;
    this._seenPresetVersion = Configuration.PresetListVersion;
    this.RefreshProfileTab();
    this.RefreshGeneralTab();
    UtilMenuController.Instance?.RefreshUI();
  }

  private void RefreshInputTab()
  {
    if (this.Tabs.Count < 3)
      return;
    UtilTab tab = this.Tabs[2];
    tab.Elements.Clear();
    UtilMenuController controller = UtilMenuController.Instance;
    if (((UnityEngine.Object) controller == (UnityEngine.Object) null))
      return;
    tab.Elements.Add(new MenuElement("OPEN WITH: " + UtilMenuController.CurrentToggleButton.ToString().ToUpper(), "", (Action) (() =>
    {
      int num = (int) (UtilMenuController.CurrentToggleButton - 1);
      if (num < 0)
        num = 2;
      UtilMenuController.CurrentToggleButton = (UtilMenuController.MenuToggleButton) num;
      controller.SaveInputSettings();
      this.RefreshInputTab();
      controller.RefreshUI();
    }), (Action) (() =>
    {
      int num = (int) (UtilMenuController.CurrentToggleButton + 1);
      if (num > 2)
        num = 0;
      UtilMenuController.CurrentToggleButton = (UtilMenuController.MenuToggleButton) num;
      controller.SaveInputSettings();
      this.RefreshInputTab();
      controller.RefreshUI();
    }))
    {
      Type = ElementType.Slider
    });
    tab.Elements.Add(new MenuElement("CLICKS: " + UtilMenuController.CurrentClickType.ToString().ToUpper(), "", (Action) (() =>
    {
      int num = (int) (UtilMenuController.CurrentClickType - 1);
      if (num < 0)
        num = 2;
      UtilMenuController.CurrentClickType = (UtilMenuController.MenuClickType) num;
      controller.SaveInputSettings();
      this.RefreshInputTab();
      controller.RefreshUI();
    }), (Action) (() =>
    {
      int num = (int) (UtilMenuController.CurrentClickType + 1);
      if (num > 2)
        num = 0;
      UtilMenuController.CurrentClickType = (UtilMenuController.MenuClickType) num;
      controller.SaveInputSettings();
      this.RefreshInputTab();
      controller.RefreshUI();
    }))
    {
      Type = ElementType.Slider
    });
    tab.Elements.Add(new MenuElement("HAPTICS: " + SettingsPage.HapticsLevelLabel(HapticEngine.Level), "", (Action) (() =>
    {
      if (HapticEngine.Level > HapticsLevel.Disabled)
        --HapticEngine.Level;
      Configuration.SaveSettings();
      HapticEngine.Play(HapticPreset.ButtonClick);
      this.RefreshInputTab();
      controller.RefreshUI();
    }), (Action) (() =>
    {
      if (HapticEngine.Level < HapticsLevel.ALot)
        ++HapticEngine.Level;
      Configuration.SaveSettings();
      HapticEngine.Play(HapticPreset.ButtonClick);
      this.RefreshInputTab();
      controller.RefreshUI();
    }))
    {
      Type = ElementType.Slider
    });
  }

  private static string HapticsLevelLabel(HapticsLevel level)
  {
    string str;
    switch (level)
    {
      case HapticsLevel.Disabled:
        str = "DISABLED";
        break;
      case HapticsLevel.Minimal:
        str = "MINIMAL";
        break;
      case HapticsLevel.ALot:
        str = "A LOT";
        break;
      default:
        str = "NORMAL";
        break;
    }
    return str;
  }

  private void RefreshOffsetsTab()
  {
    if (this.Tabs.Count < 4)
      return;
    UtilTab tab = this.Tabs[3];
    tab.Elements.Clear();
    switch (this._offsetsView)
    {
      case SettingsPage.OffsetsView.Position:
        this.BuildPositionView(tab);
        break;
      case SettingsPage.OffsetsView.Rotation:
        this.BuildRotationView(tab);
        break;
      default:
        this.BuildSelectionView(tab);
        break;
    }
  }

  private void BuildSelectionView(UtilTab tab)
  {
    tab.Elements.Add(new MenuElement("POSITION >", (Action) (() =>
    {
      this._offsetsView = SettingsPage.OffsetsView.Position;
      this.RefreshOffsetsTab();
      UtilMenuController.Instance?.RefreshUI();
    })));
    tab.Elements.Add(new MenuElement("ROTATION >", (Action) (() =>
    {
      this._offsetsView = SettingsPage.OffsetsView.Rotation;
      this.RefreshOffsetsTab();
      UtilMenuController.Instance?.RefreshUI();
    })));
    tab.Elements.Add(new MenuElement("RESET OFFSETS [HOLD]", (Action) (() =>
    {
      UtilMenuController.OffsetPosX = 0.0f;
      UtilMenuController.OffsetPosY = 0.0f;
      UtilMenuController.OffsetPosZ = 0.0f;
      UtilMenuController.OffsetRotX = 0.0f;
      UtilMenuController.OffsetRotY = 0.0f;
      UtilMenuController.OffsetRotZ = 0.0f;
      Configuration.SaveSettings();
      this.RefreshOffsetsTab();
      UtilMenuController.Instance?.RefreshUI();
    }), 2f));
  }

  private void BuildPositionView(UtilTab tab)
  {
    tab.Elements.Add(this.BuildPosSlider("LEFT / RIGHT", (Func<float>) (() => UtilMenuController.OffsetPosX), (Action<float>) (v => UtilMenuController.OffsetPosX = v)));
    tab.Elements.Add(this.BuildPosSlider("DOWN / UP", (Func<float>) (() => UtilMenuController.OffsetPosY), (Action<float>) (v => UtilMenuController.OffsetPosY = v)));
    tab.Elements.Add(this.BuildPosSlider("BACK / FORWARD", (Func<float>) (() => UtilMenuController.OffsetPosZ), (Action<float>) (v => UtilMenuController.OffsetPosZ = v)));
    tab.Elements.Add(this.BuildBackToSelection());
  }

  private void BuildRotationView(UtilTab tab)
  {
    tab.Elements.Add(this.BuildRotSlider("TILT (X)", (Func<float>) (() => UtilMenuController.OffsetRotX), (Action<float>) (v => UtilMenuController.OffsetRotX = v)));
    tab.Elements.Add(this.BuildRotSlider("TURN (Y)", (Func<float>) (() => UtilMenuController.OffsetRotY), (Action<float>) (v => UtilMenuController.OffsetRotY = v)));
    tab.Elements.Add(this.BuildRotSlider("ROLL (Z)", (Func<float>) (() => UtilMenuController.OffsetRotZ), (Action<float>) (v => UtilMenuController.OffsetRotZ = v)));
    tab.Elements.Add(this.BuildBackToSelection());
  }

  private MenuElement BuildBackToSelection()
  {
    return new MenuElement("< BACK", (Action) (() =>
    {
      this._offsetsView = SettingsPage.OffsetsView.Selection;
      this.RefreshOffsetsTab();
      UtilMenuController.Instance?.RefreshUI();
    }));
  }

  private MenuElement BuildPosSlider(string label, Func<float> getter, Action<float> setter)
  {
    MenuElement el = (MenuElement) null;
    el = new MenuElement(label, getter().ToString("F3"), (Action) (() =>
    {
      setter(Mathf.Clamp(getter() - 0.005f, -0.2f, 0.2f));
      el.ValueText = getter().ToString("F3");
      Configuration.SaveSettings();
      UtilMenuController.Instance?.RefreshUI();
    }), (Action) (() =>
    {
      setter(Mathf.Clamp(getter() + 0.005f, -0.2f, 0.2f));
      el.ValueText = getter().ToString("F3");
      Configuration.SaveSettings();
      UtilMenuController.Instance?.RefreshUI();
    }))
    {
      Type = ElementType.Slider
    };
    return el;
  }

  private MenuElement BuildRotSlider(string label, Func<float> getter, Action<float> setter)
  {
    MenuElement el = (MenuElement) null;
    el = new MenuElement(label, getter().ToString("F1"), (Action) (() =>
    {
      setter(Mathf.Clamp(getter() - 1f, -180f, 180f));
      el.ValueText = getter().ToString("F1");
      Configuration.SaveSettings();
      UtilMenuController.Instance?.RefreshUI();
    }), (Action) (() =>
    {
      setter(Mathf.Clamp(getter() + 1f, -180f, 180f));
      el.ValueText = getter().ToString("F1");
      Configuration.SaveSettings();
      UtilMenuController.Instance?.RefreshUI();
    }))
    {
      Type = ElementType.Slider
    };
    return el;
  }

  private void RefreshGeneralTab()
  {
    if (this.Tabs.Count < 1)
      return;
    UtilTab tab = this.Tabs[0];
    tab.Elements.Clear();
    string str = string.IsNullOrEmpty(Configuration.ActivePresetName) ? "CUSTOM" : Configuration.ActivePresetName.ToUpper();
    tab.Elements.Add(new MenuElement("CURRENT: " + str, (Action) (() => { })));
    tab.Elements.Add(new MenuElement("RESET TO DEFAULTS [HOLD]", (Action) (() =>
    {
      Configuration.ResetToDefaults();
      ThemeManager.RefreshTheme();
      this.RefreshGeneralTab();
      UtilMenuController.Instance.RefreshUI();
    }), 2f));
    tab.Elements.Add(new MenuElement("RELOAD FROM CLOUD", (Action) (() =>
    {
      Configuration.LoadSettings();
      ThemeManager.RefreshTheme();
      UtilMenuController.Instance.RefreshUI();
    })));
    string text = this._isEditingName ? (this._nameInput == "" ? "TYPE NAME..." : this._nameInput) : "CHANGE NAME";
    ElementType type = this._isEditingName ? ElementType.Input : ElementType.Button;
    tab.Elements.Add(new MenuElement(text, (Action) (() =>
    {
      if (this._isEditingName)
      {
        this._isEditingName = false;
        KeyboardController.Instance.CloseKeyboard();
        this.RefreshGeneralTab();
        UtilMenuController.Instance.RefreshUI();
      }
      else
      {
        this._isEditingName = true;
        if (((UnityEngine.Object) GorillaComputer.instance != (UnityEngine.Object) null))
          this._nameInput = GorillaComputer.instance.currentName;
        KeyboardController.Instance.currentInput = this._nameInput;
        this.RefreshGeneralTab();
        UtilMenuController.Instance.RefreshUI();
        KeyboardController.Instance.OnKeyPressed = (Action<string>) (k =>
        {
          this._nameInput = k.ToUpper();
          this.RefreshGeneralTab();
          UtilMenuController.Instance.RefreshUI();
        });
        KeyboardController.Instance.OnEnterPressed = (Action) (() =>
        {
          if (!string.IsNullOrEmpty(this._nameInput))
            PlayerHelper.UpdateName(this._nameInput);
          this._isEditingName = false;
          KeyboardController.Instance.CloseKeyboard();
          this.RefreshGeneralTab();
          UtilMenuController.Instance.RefreshUI();
        });
        KeyboardController.Instance.OpenKeyboard();
      }
    }), type));
    tab.Elements.Add(new MenuElement("THEME: " + this.GetThemeName(), "", (Action) (() =>
    {
      ThemeManager.CycleTheme(-1);
      this.RefreshGeneralTab();
      UtilMenuController.Instance.RefreshUI();
    }), (Action) (() =>
    {
      ThemeManager.CycleTheme(1);
      this.RefreshGeneralTab();
      UtilMenuController.Instance.RefreshUI();
    }))
    {
      Type = ElementType.Slider
    });
    tab.Elements.Add(new MenuElement("SEE OTHERS' UTIL MENU: " + (RemoteModRepresentation.ShowOthersUtilMenu ? "ON" : "OFF"), (Action) (() =>
    {
      RemoteModRepresentation.ShowOthersUtilMenu = !RemoteModRepresentation.ShowOthersUtilMenu;
      this.RefreshGeneralTab();
      UtilMenuController.Instance.RefreshUI();
    }), RemoteModRepresentation.ShowOthersUtilMenu)
    {
      Type = ElementType.Toggle
    });
  }

  private string GetThemeName()
  {
    return ThemeManager.AvailableThemes.Count <= ThemeManager.CurrentThemeIndex ? "DEFAULT" : ThemeManager.AvailableThemes[ThemeManager.CurrentThemeIndex].Name;
  }

  private void RefreshProfileTab()
  {
    if (this.Tabs.Count < 2)
      return;
    UtilTab tab = this.Tabs[1];
    tab.Elements.Clear();
    string text1 = this._isTypingProfile ? (this._profileInput == "" ? "TYPE NAME..." : this._profileInput) : "CREATE NEW PROFILE";
    tab.Elements.Add(new MenuElement(text1, (Action) (() =>
    {
      if (this._isTypingProfile)
      {
        this._isTypingProfile = false;
        KeyboardController.Instance.CloseKeyboard();
        this.RefreshProfileTab();
      }
      else
      {
        this._isTypingProfile = true;
        this._profileInput = "";
        KeyboardController.Instance.ClearInput();
        KeyboardController.Instance.currentInput = "";
        this.RefreshProfileTab();
        UtilMenuController.Instance.RefreshUI();
        KeyboardController.Instance.OnKeyPressed = (Action<string>) (k =>
        {
          this._profileInput = k.ToUpper();
          this.RefreshProfileTab();
          UtilMenuController.Instance.RefreshUI();
        });
        KeyboardController.Instance.OnEnterPressed = (Action) (() =>
        {
          string str = (this._profileInput ?? "").Trim();
          if (!string.IsNullOrEmpty(str))
          {
            foreach (PresetMeta availablePreset in Configuration.AvailablePresets)
            {
              if (string.Equals(availablePreset.Name, str, StringComparison.OrdinalIgnoreCase))
              {
                Notification.Send($"Preset name \"{str}\" already exists", Color.red);
                return;
              }
            }
            if (Configuration.CreatePresetFromCurrent(str))
              Notification.Send($"Saving preset \"{str}\"…", Color.green);
            this._isTypingProfile = false;
            this._profileInput = "";
            KeyboardController.Instance.CloseKeyboard();
            this.RefreshProfileTab();
            UtilMenuController.Instance.RefreshUI();
          }
          else
          {
            this._isTypingProfile = false;
            KeyboardController.Instance.CloseKeyboard();
            this.RefreshProfileTab();
            UtilMenuController.Instance.RefreshUI();
          }
        });
        KeyboardController.Instance.OpenKeyboard();
      }
      UtilMenuController.Instance.RefreshUI();
    }), this._isTypingProfile ? ElementType.Input : ElementType.Button));
    if (Configuration.InitialLoadComplete)
    {
      List<PresetMeta> presetMetaList = Configuration.AvailablePresets ?? new List<PresetMeta>();
      int total = presetMetaList.Count;
      if (this._profileOffset >= total)
        this._profileOffset = 0;
      for (int index1 = 0; index1 < 4; ++index1)
      {
        int index2 = this._profileOffset + index1;
        if (index2 < total)
        {
          PresetMeta presetMeta = presetMetaList[index2];
          string text2 = presetMeta.IsPending ? presetMeta.Name + " (SAVING…)" : presetMeta.Name;
          if (presetMeta.IsPending)
          {
            tab.Elements.Add(new MenuElement(text2, (Action) (() => { }))
            {
              CustomLeftIcon = UtilMenuMain.Instance.Icons.Save
            });
          }
          else
          {
            string pName = presetMeta.Name;
            tab.Elements.Add(new MenuElement(text2, "DEL | LOAD", (Action) (() =>
            {
              Configuration.DeleteProfile(pName);
              this.RefreshProfileTab();
              UtilMenuController.Instance.RefreshUI();
            }), (Action) (() =>
            {
              Configuration.LoadProfile(pName);
              ThemeManager.RefreshTheme();
              UtilMenuController.Instance.RefreshUI();
            }))
            {
              CustomLeftIcon = UtilMenuMain.Instance.Icons.TrashIcon,
              CustomRightIcon = UtilMenuMain.Instance.Icons.Save
            });
          }
        }
      }
      if (total <= 4)
        return;
      tab.Elements.Add(new MenuElement("NEXT PAGE >>", (Action) (() =>
      {
        this._profileOffset += 4;
        if (this._profileOffset >= total)
          this._profileOffset = 0;
        this.RefreshProfileTab();
        UtilMenuController.Instance.RefreshUI();
      })));
    }
    else
      tab.Elements.Add(new MenuElement("LOADING…", (Action) (() => { })));
  }

  private enum OffsetsView
  {
    Selection,
    Position,
    Rotation,
  }
}
