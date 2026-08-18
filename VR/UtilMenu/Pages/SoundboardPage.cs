using SakuraaCastingMod.Core;
using SakuraaCastingMod.Features.Soundboard;
using SakuraaCastingMod.Shared.Models;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.VR.UtilMenu.Pages;

public class SoundboardPage : BasePage
{
  public static SoundboardPage Instance;
  private const int TabBoard = 0;
  private const int TabTriggers = 1;
  private const int TabKeybinds = 2;
  private const int TabOptions = 3;
  private UtilTab _boardTab;
  private SoundboardPage.BoardMode _mode = SoundboardPage.BoardMode.PageList;
  private int _listChunk;
  private int _viewIndex;
  private SoundboardPage.PickKind _pickKind = SoundboardPage.PickKind.None;
  private SoundboardTriggers.Trigger _pickTrigger;
  private SoundboardKeybinds.Slot _pickSlot;
  private int _pickView;
  private string _lastShownPlayingId;
  private float _inputGuardUntil;
  private const float InputGuardSeconds = 0.45f;

  public override string PageName
  {
    get => this._pickKind == SoundboardPage.PickKind.None ? "SOUNDBOARD" : "SELECT SOUND";
  }

  public override Material PageIcon => UtilMenuMain.Instance.Icons.HorizontalBarList;

  private bool InputGuarded => (double) Time.realtimeSinceStartup < (double) this._inputGuardUntil;

  private void ArmInputGuard() => this._inputGuardUntil = Time.realtimeSinceStartup + 0.45f;

  public SoundboardPage()
  {
    SoundboardPage.Instance = this;
    SoundboardManager.OnManifestChanged += new Action(this.OnManifestChanged);
  }

  public override void BuildTabs()
  {
    this._boardTab = new UtilTab()
    {
      TabIcon = UtilMenuMain.Instance.Icons.HorizontalBarList,
      TabName = "Board"
    };
    this.Tabs.Add(this._boardTab);
    this.AddTab(UtilMenuMain.Instance.Icons.RingingBell);
    this.AddTab(UtilMenuMain.Instance.Icons.Headset);
    this.AddTab(UtilMenuMain.Instance.Icons.Options);
    this.RefreshTriggersTab();
    this.RefreshKeybindsTab();
    this.RefreshOptionsTab();
  }

  private void OnManifestChanged()
  {
    if (SoundboardPage.Instance != this)
    {
      SoundboardManager.OnManifestChanged -= new Action(this.OnManifestChanged);
    }
    else
    {
      if (this._viewIndex >= SoundboardManager.Views.Count)
        this._viewIndex = 0;
      if (this._pickView >= SoundboardManager.Views.Count)
        this._pickView = 0;
      this._listChunk = 0;
      this.RefreshTriggersTab();
      this.RefreshKeybindsTab();
      this.RefreshOptionsTab();
      if (!((UnityEngine.Object) UtilMenuController.Instance != (UnityEngine.Object) null))
        return;
      UtilMenuController.Instance.RefreshUI();
    }
  }

  public override void OnTabSelected(int tabIndex)
  {
    UtilMenuController instance = UtilMenuController.Instance;
    int num1 = ((UnityEngine.Object) instance != (UnityEngine.Object) null) ? instance.CurrentTabIndex : -1;
    if (this._pickKind != 0)
    {
      int num2 = this._pickKind == SoundboardPage.PickKind.Trigger ? 1 : 2;
      if ((tabIndex != num2 ? 1 : (num1 == num2 ? 1 : 0)) != 0)
        this._pickKind = SoundboardPage.PickKind.None;
    }
    if ((tabIndex != 0 || num1 != 0 ? 0 : (this._mode == SoundboardPage.BoardMode.Board ? 1 : 0)) != 0)
      this._mode = SoundboardPage.BoardMode.PageList;
    if (tabIndex == 1)
      this.RefreshTriggersTab();
    if (tabIndex != 2)
      return;
    this.RefreshKeybindsTab();
  }

  public override void OnPageClosed()
  {
    this._pickKind = SoundboardPage.PickKind.None;
    this._mode = SoundboardPage.BoardMode.PageList;
  }

  public override bool ShouldHideStandardBars()
  {
    UtilMenuController instance = UtilMenuController.Instance;
    return (((UnityEngine.Object) instance == (UnityEngine.Object) null) ? 1 : (!SoundboardManager.HasContent ? 1 : 0)) == 0 && (instance.CurrentTabIndex == 0 || (this._pickKind == SoundboardPage.PickKind.Trigger ? instance.CurrentTabIndex == 1 : this._pickKind == SoundboardPage.PickKind.Keybind && instance.CurrentTabIndex == 2));
  }

  public override void RefreshPageUI()
  {
    UtilMenuController instance = UtilMenuController.Instance;
    if (((UnityEngine.Object) instance == (UnityEngine.Object) null))
      return;
    if (instance.CurrentTabIndex != 0)
    {
      this._boardTab.Description = (string) null;
      if (!((BasePage) this).ShouldHideStandardBars())
        return;
      this.UpdatePickerUI();
    }
    else if (!SoundboardManager.HasContent)
    {
      this._boardTab.Description = SoundboardManager.HasManifest ? "NO SOUNDS YET.\nADD SOUNDS IN THE SAKURAA LOADER'S SOUNDBOARD TAB." : "WAITING FOR THE LOADER...\nSOUNDS ARE MANAGED IN THE SAKURAA LOADER.";
    }
    else
    {
      this._boardTab.Description = (string) null;
      if (this._mode == SoundboardPage.BoardMode.PageList)
        this.UpdatePageListUI();
      else
        this.UpdateBoardUI();
    }
  }

  public void OnTilePressed(int buttonIndex)
  {
    if (this.InputGuarded)
      return;
    if (this._pickKind != 0)
      this.OnPickerTilePressed(buttonIndex);
    else if (this._mode == SoundboardPage.BoardMode.PageList)
    {
      this.OnPageListPressed(buttonIndex);
    }
    else
    {
      List<SoundboardManager.TileView> views = SoundboardManager.Views;
      if (this._viewIndex >= views.Count)
        return;
      SoundboardManager.TileView tileView = views[this._viewIndex];
      if ((buttonIndex < 0 ? 1 : (buttonIndex >= tileView.Tiles.Count ? 1 : 0)) != 0)
        return;
      SoundboardManager.Sound tile = tileView.Tiles[buttonIndex];
      if (!(SoundboardPlayer.CurrentSoundId == tile.Id))
        SoundboardPlayer.Play(tile);
      else
        SoundboardPlayer.Stop();
      UtilMenuController.Instance?.RefreshUI();
    }
  }

  public void OnTilePageLeft() => this.StepPager(-1);

  public void OnTilePageRight() => this.StepPager(1);

  private void StepPager(int dir)
  {
    if (this.InputGuarded)
      return;
    if (this._pickKind != 0)
    {
      int count = SoundboardManager.Views.Count;
      if (count > 1)
        this._pickView = (this._pickView + dir + count) % count;
    }
    else if (this._mode == SoundboardPage.BoardMode.PageList)
    {
      int num = this.PageListChunks();
      if (num > 1)
        this._listChunk = (this._listChunk + dir + num) % num;
    }
    else
    {
      int count = SoundboardManager.Views.Count;
      if (count > 1)
        this._viewIndex = (this._viewIndex + dir + count) % count;
    }
    UtilMenuController.Instance?.RefreshUI();
  }

  private int PageListChunks()
  {
    return Mathf.Max(1, Mathf.CeilToInt((float) SoundboardManager.Groups.Count / 10f));
  }

  private void OnPageListPressed(int buttonIndex)
  {
    List<SoundboardManager.Group> groups = SoundboardManager.Groups;
    int index = this._listChunk * 10 + buttonIndex;
    if ((buttonIndex < 0 ? 1 : (index >= groups.Count ? 1 : 0)) != 0)
      return;
    this._viewIndex = groups[index].FirstView;
    this._mode = SoundboardPage.BoardMode.Board;
    this.ArmInputGuard();
    UtilMenuController.Instance?.RefreshUI();
  }

  private void UpdatePageListUI()
  {
    UtilMenuController instance = UtilMenuController.Instance;
    if (!((UnityEngine.Object) instance.playerSelectRoot))
      return;
    List<SoundboardManager.Group> groups = SoundboardManager.Groups;
    int num1 = this.PageListChunks();
    if (this._listChunk >= num1)
      this._listChunk = 0;
    int num2 = this._listChunk * 10;
    instance.playerSelectRoot.SetActive(true);
    for (int index1 = 0; index1 < instance.playerButtons.Count; ++index1)
    {
      int index2 = num2 + index1;
      if (index2 >= groups.Count)
      {
        ((Component) instance.playerButtons[index1]).gameObject.SetActive(false);
      }
      else
      {
        SoundboardManager.Group group = groups[index2];
        ((Component) instance.playerButtons[index1]).gameObject.SetActive(true);
        if (index1 < instance.playerButtonTexts.Count)
          ((TMP_Text) instance.playerButtonTexts[index1]).text = $"{SoundboardPage.Truncate(group.Name.ToUpper(), 12)} ({group.SoundTotal})";
        instance.playerButtons[index1].isOn = false;
        instance.playerButtons[index1].UpdateColor();
      }
    }
    SoundboardPage.UpdateBarExtra(num1 > 1 ? $"SELECT A PAGE {this._listChunk + 1}/{num1}" : "SELECT A PAGE", "", num1 > 1);
  }

  private void UpdateBoardUI()
  {
    UtilMenuController instance = UtilMenuController.Instance;
    if (!((UnityEngine.Object) instance.playerSelectRoot))
      return;
    List<SoundboardManager.TileView> views = SoundboardManager.Views;
    if (this._viewIndex >= views.Count)
      this._viewIndex = 0;
    SoundboardManager.TileView tileView = views[this._viewIndex];
    instance.playerSelectRoot.SetActive(true);
    for (int index = 0; index < instance.playerButtons.Count; ++index)
    {
      if (index >= tileView.Tiles.Count)
      {
        ((Component) instance.playerButtons[index]).gameObject.SetActive(false);
      }
      else
      {
        SoundboardManager.Sound tile = tileView.Tiles[index];
        ((Component) instance.playerButtons[index]).gameObject.SetActive(true);
        if (index < instance.playerButtonTexts.Count)
          ((TMP_Text) instance.playerButtonTexts[index]).text = SoundboardPage.TileLabel(tile);
        bool flag = SoundboardPlayer.CurrentSoundId == tile.Id;
        instance.playerButtons[index].isOn = flag;
        instance.playerButtons[index].UpdateColor();
      }
    }
    SoundboardPage.UpdateBarExtra(tileView.Label, "TAB = BACK", views.Count > 1);
  }

  private static void UpdateBarExtra(string label, string hint, bool showArrows)
  {
    UtilMenuController instance = UtilMenuController.Instance;
    if (!((UnityEngine.Object) instance.barExtraRoot))
      return;
    instance.barExtraRoot.SetActive(true);
    if (((UnityEngine.Object) instance.gunSelectBtn))
      ((Component) instance.gunSelectBtn).gameObject.SetActive(false);
    if (((UnityEngine.Object) instance.gunSelectSmallBtn))
      ((Component) instance.gunSelectSmallBtn).gameObject.SetActive(false);
    if (((UnityEngine.Object) instance.peSliderLeft))
      ((Component) instance.peSliderLeft).gameObject.SetActive(showArrows);
    if (((UnityEngine.Object) instance.peSliderRight))
      ((Component) instance.peSliderRight).gameObject.SetActive(showArrows);
    if (((UnityEngine.Object) instance.peSliderText))
    {
      ((Component) instance.peSliderText).gameObject.SetActive(true);
      ((TMP_Text) instance.peSliderText).text = label;
    }
    if (!((UnityEngine.Object) instance.peValueText))
      return;
    ((TMP_Text) instance.peValueText).text = hint;
  }

  private static string TileLabel(SoundboardManager.Sound s)
  {
    return SoundboardPage.Truncate(string.IsNullOrEmpty(s.Name) ? "SOUND" : s.Name, 16 /*0x10*/);
  }

  private static string Truncate(string text, int max)
  {
    return text.Length <= max ? text : text.Substring(0, max - 1) + "…";
  }

  public override void LateUpdate()
  {
    UtilMenuController instance = UtilMenuController.Instance;
    if ((((UnityEngine.Object) instance == (UnityEngine.Object) null) ? 1 : (instance.CurrentPage != this ? 1 : 0)) != 0 || (instance.CurrentTabIndex != 0 ? 1 : (this._mode != SoundboardPage.BoardMode.Board ? 1 : 0)) != 0 || SoundboardPlayer.CurrentSoundId == this._lastShownPlayingId)
      return;
    this._lastShownPlayingId = SoundboardPlayer.CurrentSoundId;
    instance.RefreshUI();
  }

  private void BeginPick(SoundboardTriggers.Trigger t)
  {
    if (this.InputGuarded || !SoundboardManager.HasContent)
      return;
    this._pickKind = SoundboardPage.PickKind.Trigger;
    this._pickTrigger = t;
    this._pickView = SoundboardManager.FindViewOf(SoundboardTriggers.GetSoundId(t));
    this.ArmInputGuard();
    UtilMenuController.Instance?.RefreshUI();
  }

  private void BeginPick(SoundboardKeybinds.Slot s)
  {
    if (this.InputGuarded || !SoundboardManager.HasContent)
      return;
    this._pickKind = SoundboardPage.PickKind.Keybind;
    this._pickSlot = s;
    this._pickView = SoundboardManager.FindViewOf(SoundboardKeybinds.GetSoundId(s));
    this.ArmInputGuard();
    UtilMenuController.Instance?.RefreshUI();
  }

  private string PickTargetLabel()
  {
    string str;
    if (this._pickKind == SoundboardPage.PickKind.Trigger)
    {
      foreach ((SoundboardTriggers.Trigger trig, string label) def in SoundboardTriggers.Defs)
      {
        if (def.trig == this._pickTrigger)
        {
          str = def.label;
          goto label_13;
        }
      }
    }
    else if (this._pickKind == SoundboardPage.PickKind.Keybind)
    {
      foreach ((SoundboardKeybinds.Slot slot, string label) def in SoundboardKeybinds.Defs)
      {
        if (def.slot == this._pickSlot)
        {
          str = def.label;
          goto label_13;
        }
      }
    }
    str = "";
label_13:
    return str;
  }

  private string PickCurrentId()
  {
    if (this._pickKind == SoundboardPage.PickKind.Trigger)
      return SoundboardTriggers.GetSoundId(this._pickTrigger);
    return this._pickKind != SoundboardPage.PickKind.Keybind ? "" : SoundboardKeybinds.GetSoundId(this._pickSlot);
  }

  private void UpdatePickerUI()
  {
    UtilMenuController instance = UtilMenuController.Instance;
    if (!((UnityEngine.Object) instance.playerSelectRoot))
      return;
    List<SoundboardManager.TileView> views = SoundboardManager.Views;
    if (this._pickView >= views.Count)
      this._pickView = 0;
    SoundboardManager.TileView tileView = views[this._pickView];
    string str = this.PickCurrentId();
    instance.playerSelectRoot.SetActive(true);
    for (int index = 0; index < instance.playerButtons.Count; ++index)
    {
      if (index < tileView.Tiles.Count)
      {
        SoundboardManager.Sound tile = tileView.Tiles[index];
        ((Component) instance.playerButtons[index]).gameObject.SetActive(true);
        if (index < instance.playerButtonTexts.Count)
          ((TMP_Text) instance.playerButtonTexts[index]).text = SoundboardPage.TileLabel(tile);
        instance.playerButtons[index].isOn = tile.Id == str;
        instance.playerButtons[index].UpdateColor();
      }
      else
        ((Component) instance.playerButtons[index]).gameObject.SetActive(false);
    }
    SoundboardPage.UpdateBarExtra(tileView.Label, "FOR " + this.PickTargetLabel(), views.Count > 1);
  }

  private void OnPickerTilePressed(int buttonIndex)
  {
    List<SoundboardManager.TileView> views = SoundboardManager.Views;
    if (this._pickView >= views.Count)
    {
      this._pickKind = SoundboardPage.PickKind.None;
    }
    else
    {
      SoundboardManager.TileView tileView = views[this._pickView];
      if ((buttonIndex < 0 ? 1 : (buttonIndex >= tileView.Tiles.Count ? 1 : 0)) != 0)
        return;
      string id1 = tileView.Tiles[buttonIndex].Id;
      string id2 = id1 == this.PickCurrentId() ? "" : id1;
      if (this._pickKind == SoundboardPage.PickKind.Trigger)
        SoundboardTriggers.SetSoundId(this._pickTrigger, id2);
      else if (this._pickKind == SoundboardPage.PickKind.Keybind)
        SoundboardKeybinds.SetSoundId(this._pickSlot, id2);
      this._pickKind = SoundboardPage.PickKind.None;
      this.ArmInputGuard();
      this.RefreshTriggersTab();
      this.RefreshKeybindsTab();
      UtilMenuController.Instance?.RefreshUI();
    }
  }

  private void RefreshTriggersTab()
  {
    if (this.Tabs.Count <= 1)
      return;
    UtilTab tab = this.Tabs[1];
    tab.Elements.Clear();
    foreach ((SoundboardTriggers.Trigger trig, string label) def in SoundboardTriggers.Defs)
    {
      SoundboardTriggers.Trigger t = def.trig;
      tab.Elements.Add(new MenuElement($"{def.label}: {SoundboardPage.AssignedName(SoundboardTriggers.GetSoundId(t))}", (Action) (() => this.BeginPick(t))));
    }
    tab.Elements.Add(new MenuElement("ALL TRIGGERS OFF", (Action) (() =>
    {
      SoundboardTriggers.AllOff();
      this.RefreshTriggersTab();
      UtilMenuController.Instance?.RefreshUI();
    }), 1f));
  }

  private void RefreshKeybindsTab()
  {
    if (this.Tabs.Count <= 2)
      return;
    UtilTab tab = this.Tabs[2];
    tab.Elements.Clear();
    foreach ((SoundboardKeybinds.Slot slot, string label) def in SoundboardKeybinds.Defs)
    {
      SoundboardKeybinds.Slot s = def.slot;
      string str = SoundboardKeybinds.ConflictsWithMenuToggle(s) ? "MENU BTN" : SoundboardPage.AssignedName(SoundboardKeybinds.GetSoundId(s));
      tab.Elements.Add(new MenuElement($"{def.label}: {str}", (Action) (() => this.BeginPick(s))));
    }
    tab.Elements.Add(new MenuElement("ALL KEYBINDS OFF", (Action) (() =>
    {
      SoundboardKeybinds.AllOff();
      this.RefreshKeybindsTab();
      UtilMenuController.Instance?.RefreshUI();
    }), 1f));
  }

  private static string AssignedName(string soundId)
  {
    string str;
    if (!string.IsNullOrEmpty(soundId))
    {
      SoundboardManager.Sound sound = SoundboardManager.FindSound(soundId);
      str = sound != null ? SoundboardPage.Truncate(sound.Name.ToUpper(), 10) : "OFF";
    }
    else
      str = "OFF";
    return str;
  }

  private void RefreshOptionsTab()
  {
    if (this.Tabs.Count <= 3)
      return;
    UtilTab tab = this.Tabs[3];
    tab.Elements.Clear();
    tab.Elements.Add(new MenuElement("VOLUME", SoundboardPage.VolumeText(), (Action) (() => this.NudgeVolume(-0.1f)), (Action) (() => this.NudgeVolume(0.1f)))
    {
      Type = ElementType.Slider
    });
    tab.Elements.Add(new MenuElement("STOP SOUND", (Action) (() =>
    {
      SoundboardPlayer.Stop();
      UtilMenuController.Instance?.RefreshUI();
    })));
    tab.Elements.Add(new MenuElement($"SOUNDS: {SoundboardManager.SoundCount}/{200}", (Action) (() => { })));
  }

  private static string VolumeText()
  {
    return $"{Mathf.RoundToInt(Mathf.Clamp01(SoundboardPlayer.Volume) * 100f)}%";
  }

  private void NudgeVolume(float delta)
  {
    SoundboardPlayer.Volume = Mathf.Clamp01(Mathf.Round((float) (((double) SoundboardPlayer.Volume + (double) delta) * 10.0)) / 10f);
    Configuration.SaveSettings();
    this.RefreshOptionsTab();
    UtilMenuController.Instance?.RefreshUI();
  }

  private enum BoardMode
  {
    PageList,
    Board,
  }

  private enum PickKind
  {
    None,
    Trigger,
    Keybind,
  }
}
