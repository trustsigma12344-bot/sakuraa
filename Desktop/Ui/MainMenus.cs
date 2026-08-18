using GorillaNetworking;
using SakuraaCastingMod.Core;
using SakuraaCastingMod.Desktop.Ui.Framework;
using SakuraaCastingMod.Desktop.Ui.Framework.MenuItems;
using SakuraaCastingMod.Desktop.Ui.Framework.Menus;
using SakuraaCastingMod.Features.AutoRef;
using SakuraaCastingMod.Features.Overlays;
using SakuraaCastingMod.Features.Replay;
using SakuraaCastingMod.Features.Replay.Recording;
using SakuraaCastingMod.Features.Tools;
using SakuraaCastingMod.Features.Visuals;
using SakuraaCastingMod.Features.World;
using SakuraaCastingMod.Shared.Helpers;
using SakuraaCastingMod.VR.UtilMenu.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Desktop.Ui;

public static class MainMenus
{
  public static bool ShowMainMenu = true;
  [SavedSetting("DesktopToolTipsEnabled", true)]
  public static bool ToolTipsEnabled = true;
  private static string _inputText = "LOBBYCODE";
  private static readonly int[] SelectableCameraModes = new int[8]
  {
    0,
    1,
    2,
    3,
    4,
    5,
    6,
    7
  };
  private static MenuBuilder _mainMenu;
  private static bool _showUiOptions = false;
  private static bool _desktopThemeBootstrapped;

  public static void ToggleMenuKeyPressed() => ReplayMenuCoordinator.Cycle();

  public static void InitializeMainMenu()
  {
    if (MainMenus._mainMenu != null)
      return;
    GuiManager.MenuInfo menuInfo = GuiManager.GetMenuInfo("main_menu");
    Vector2 vector2 = (menuInfo.MenuPosition != Vector2.zero) ? menuInfo.MenuPosition : GuiManager.RefToScreen(new Vector2(100f, 100f));
    TextFieldMenuItem textFieldMenuItem1 = new TextFieldMenuItem();
    textFieldMenuItem1.Label = "Lobby Code";
    textFieldMenuItem1.Text = MainMenus._inputText;
    textFieldMenuItem1.MaxLength = 12;
    textFieldMenuItem1.ForceUpperCase = true;
    textFieldMenuItem1.DisallowSpaces = true;
    textFieldMenuItem1.Description = "Enter a lobby code to join";
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    textFieldMenuItem1.IsVisible = new Func<bool>(Networking.CheckNotInRoom);
    TextFieldMenuItem textFieldMenuItem2 = textFieldMenuItem1;
    textFieldMenuItem2.OnTextChanged = (Action<string>) (text => MainMenus._inputText = text);
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    MainMenus._mainMenu = new MenuBuilder("Sakuraa Client", 250f).AddSpace(25f).AddDropdown("Mode", (Func<IList<string>>) (() => (IList<string>) ((IEnumerable<int>) MainMenus.SelectableCameraModes).Select<int, string>((Func<int, string>) (m => ExtraTools.UpperFirst(Plugin.Ins.CameraModes[m]))).ToList<string>()), (Func<int>) (() =>
    {
      int num = Array.IndexOf<int>(MainMenus.SelectableCameraModes, Plugin.Ins.currentCameraMode);
      return num < 0 ? 0 : num;
    }), (Action<int>) (pos =>
    {
      if ((pos < 0 ? 1 : (pos >= MainMenus.SelectableCameraModes.Length ? 1 : 0)) != 0)
        return;
      int selectableCameraMode = MainMenus.SelectableCameraModes[pos];
      if (selectableCameraMode == Plugin.Ins.currentCameraMode)
        return;
      Plugin.Ins.currentCameraMode = selectableCameraMode;
      Plugin.Ins.OnModeChange();
    }), "Pick a camera mode").AddItem((MenuItem) textFieldMenuItem2).AddButton("Join", (Action) (() =>
    {
      if (!((UnityEngine.Object) PhotonNetworkController.Instance != (UnityEngine.Object) null) || !GorillaComputer.instance.CheckAutoBanListForName(MainMenus._inputText) || string.IsNullOrWhiteSpace(MainMenus._inputText))
        return;
      PhotonNetworkController.Instance.AttemptToJoinSpecificRoom(MainMenus._inputText, (JoinType) 0);
    }), "Join the specified lobby", new Func<bool>(Networking.CheckNotInRoom)).AddButton("Leave Room", (Action) (() => Networking.LeaveRoom()), "Leave the current lobby", (Func<bool>) (() => Networking.CheckInRoom() && !Plugin.ReplayCompatibilityMode)).AddSpace().AddDynamicToggle("ToolTips", (Func<bool>) (() => MainMenus.ToolTipsEnabled), (Action<bool>) (v => MainMenus.ToolTipsEnabled = v), "Toggle showing tooltips like this one!").AddSpace().AddDynamicButton((Func<string>) (() => !MainMenus._showUiOptions ? "▼ Show UI Options" : "▲ Hide UI Options"), (Action) (() => MainMenus._showUiOptions = !MainMenus._showUiOptions), "Show or hide the UI toggles below").AddSpace(5f, (Func<bool>) (() => MainMenus._showUiOptions)).AddLabel("World", true, visibilityCondition: (Func<bool>) (() => MainMenus._showUiOptions)).AddDynamicToggle("Name Changer", (Func<bool>) (() => Plugin.Ins.showNameChangerMenu), (Action<bool>) (v => Plugin.Ins.showNameChangerMenu = v), "Show or hide the name changer menu", (Func<bool>) (() => MainMenus._showUiOptions)).AddDynamicToggle("Environment", (Func<bool>) (() => WorldManager.ShowTimeChangerMenu), (Action<bool>) (v => WorldManager.ShowTimeChangerMenu = v), "Show or hide the time-of-day and weather menu", (Func<bool>) (() => MainMenus._showUiOptions)).AddDynamicToggle("Map Loader", (Func<bool>) (() => WorldManager.ShowMapLoader), (Action<bool>) (v => WorldManager.ShowMapLoader = v), "Show or hide the map loader menu", (Func<bool>) (() => MainMenus._showUiOptions)).AddDynamicToggle("Teleporter", (Func<bool>) (() => SakTeleporter.ShowMenu), (Action<bool>) (v => SakTeleporter.ShowMenu = v), "Show or hide the teleporter menu", (Func<bool>) (() => MainMenus._showUiOptions)).AddSpace(8f, (Func<bool>) (() => MainMenus._showUiOptions)).AddLabel("Overlays", true, visibilityCondition: (Func<bool>) (() => MainMenus._showUiOptions)).AddDynamicToggle("Edit Layout", (Func<bool>) (() => LayoutEditor.IsEditing), (Action<bool>) (_ => LayoutEditor.Toggle()), "Toggle the overlay layout editor: drag and resize on-screen overlays.", (Func<bool>) (() => MainMenus._showUiOptions)).AddDynamicToggle("MiniMap", (Func<bool>) (() => MiniMap.MiniMapEnabled), (Action<bool>) (_ => MiniMap.ToggleMiniMap()), "Toggle the minimap overlay", (Func<bool>) (() => MainMenus._showUiOptions)).AddDynamicToggle("Scoreboard", (Func<bool>) (() => Scoreboard.ShowScoreboard), (Action<bool>) (v => Scoreboard.ShowScoreboard = v), "Show or hide the scoreboard", (Func<bool>) (() => MainMenus._showUiOptions)).AddDynamicToggle("Show FPS", (Func<bool>) (() => FpsCounter.ShowFps), (Action<bool>) (v => FpsCounter.ShowFps = v), "Toggle display of FPS counter", (Func<bool>) (() => MainMenus._showUiOptions)).AddDynamicToggle("Mic Toggle Icon", (Func<bool>) (() => ToggleMicOverlay.ShowToggleMicIcon), (Action<bool>) (v => ToggleMicOverlay.ShowToggleMicIcon = v), "Show or hide the mic mute indicator in the bottom-right while the menu is open", (Func<bool>) (() => MainMenus._showUiOptions)).AddSpace(8f, (Func<bool>) (() => MainMenus._showUiOptions)).AddLabel("Tools", true, visibilityCondition: (Func<bool>) (() => MainMenus._showUiOptions)).AddDynamicToggle("Keybinds", (Func<bool>) (() => Keybinds.ShowKeybindMenu), (Action<bool>) (_ => KeybindsMenu.ToggleMenu()), "Show or hide the keybind configuration menu", (Func<bool>) (() => MainMenus._showUiOptions)).AddDynamicToggle("Rewind", (Func<bool>) (() => RewindViewer.ShowEditorMenu), (Action<bool>) (_ => RewindMenu.ToggleMenu()), "Enable or disable the rewind feature", (Func<bool>) (() => MainMenus._showUiOptions)).AddDynamicToggle("Util Menu", (Func<bool>) (() => UtilMenuViewer.ShowViewer), (Action<bool>) (v => UtilMenuViewer.ShowViewer = v), "Show or hide the VR utility menu viewer", (Func<bool>) (() => MainMenus._showUiOptions)).AddDynamicToggle("Auto Ref Beta", (Func<bool>) (() => AutoRefManager.ShowMenu), (Action<bool>) (v => AutoRefManager.ShowMenu = v), "Show or hide the Auto Ref Beta control panel", (Func<bool>) (() => MainMenus._showUiOptions)).AddSpace(8f, (Func<bool>) (() => MainMenus._showUiOptions)).AddLabel("Replay", true, visibilityCondition: (Func<bool>) (() => MainMenus._showUiOptions)).AddLabel("Replay System disabled in loader", visibilityCondition: (Func<bool>) (() => MainMenus._showUiOptions && !FeatureToggles.ShowReplaySystem)).AddButton("Switch to Replays Ui", (Action) (() => ReplayMenuCoordinator.OpenReplayViewer()), "Open the replay viewer to load and edit replays", (Func<bool>) (() => MainMenus._showUiOptions && FeatureToggles.ShowReplaySystem)).AddDynamicToggle("Replay Recorder", (Func<bool>) (() => RecorderSystemBase.RecorderEnabled), (Action<bool>) (v =>
    {
      RecorderSystemBase.RecorderEnabled = v;
      if ((v ? 0 : (((UnityEngine.Object) RecorderSystemBase.Instance != (UnityEngine.Object) null) ? 1 : 0)) == 0)
        return;
      RecorderSystemBase.Instance.ManualStop();
    }), "Master switch for the replay recorder. On reveals its options below it.", (Func<bool>) (() => MainMenus._showUiOptions && FeatureToggles.ShowReplaySystem)).AddSpace(8f, (Func<bool>) (() => MainMenus._showUiOptions)).AddLabel("Performance", true, visibilityCondition: (Func<bool>) (() => MainMenus._showUiOptions)).AddDynamicToggle("FPS Unlocker", (Func<bool>) (() => !FpsCounter.FPSCapped), (Action<bool>) (v =>
    {
      FpsCounter.FPSCapped = !v;
      if (!v)
        return;
      Notification.Send("In rare cases the FPS unlocker can cause jitter and make players look laggier than they are, on high end machines.", Color.yellow);
    }), "Unlock the FPS cap (on = uncapped, off = engine default)", (Func<bool>) (() => MainMenus._showUiOptions)).AddSpace(visibilityCondition: (Func<bool>) (() => MainMenus._showUiOptions && Networking.InRoom)).AddLabel("In-Room", true, visibilityCondition: (Func<bool>) (() => MainMenus._showUiOptions && Networking.InRoom)).AddDynamicToggle("Leaderboard", (Func<bool>) (() => Leaderboard.ShowLeaderboard), (Action<bool>) (_ => Leaderboard.ToggleLeaderboard()), "Toggle the player leaderboard overlay", (Func<bool>) (() => MainMenus._showUiOptions && Networking.InRoom)).AddDynamicToggle("NameTags", (Func<bool>) (() => NameTags.NameTagsEnabled), (Action<bool>) (_ => NameTags.ToggleNameTagVisibility()), "Toggle player name tags", (Func<bool>) (() => MainMenus._showUiOptions && Networking.InRoom)).AddDynamicToggle("KillFeed", (Func<bool>) (() => KillFeed.KillFeedEnabled), (Action<bool>) (_ => KillFeed.ToggleKillFeed()), "Toggle the kill feed overlay", (Func<bool>) (() => MainMenus._showUiOptions && Networking.InRoom)).AddDynamicToggle("Distance", (Func<bool>) (() => LavaDistance.ShowDistanceCard), (Action<bool>) (_ => LavaDistance.ToggleDistanceCard()), "Toggle the distance card overlay", (Func<bool>) (() => MainMenus._showUiOptions && Networking.InRoom)).AddDynamicToggle("Player Smooth menu", (Func<bool>) (() => Interpolation.RigSmoothEnabled), (Action<bool>) (v =>
    {
      InterpolationMenu.ApplyInterpolationSettings();
      Interpolation.RigSmoothEnabled = v;
    }), "Show options for player movement smoothing", (Func<bool>) (() => MainMenus._showUiOptions && Networking.InRoom)).AddSpace(8f, (Func<bool>) (() => MainMenus._showUiOptions)).AddLabel("Config", true, visibilityCondition: (Func<bool>) (() => MainMenus._showUiOptions)).AddDynamicToggle("Presets Menu", (Func<bool>) (() => PresetsMenu.ShowMenu), (Action<bool>) (v => PresetsMenu.ShowMenu = v), "Toggle the preset manager (create / load / delete saved presets)", (Func<bool>) (() => MainMenus._showUiOptions)).AddDropdown("Theme", (Func<IList<string>>) (() =>
    {
      MainMenus.EnsureThemeManagerInitialized();
      return (IList<string>) ThemeManager.AvailableThemes.Select<ThemePreset, string>((Func<ThemePreset, string>) (t => t.Name)).ToList<string>();
    }), (Func<int>) (() => ThemeManager.CurrentThemeIndex), (Action<int>) (idx =>
    {
      MainMenus.EnsureThemeManagerInitialized();
      ThemeManager.SetThemeIndex(idx);
    }), "Pick a visual theme", (Func<bool>) (() => MainMenus._showUiOptions)).AddDynamicToggle("Theme Editor", (Func<bool>) (() => ThemeEditorMenu.ShowMenu), (Action<bool>) (v =>
    {
      if (v)
        ThemeEditorMenu.Open();
      else
        ThemeEditorMenu.ShowMenu = false;
    }), "Create & edit a custom color theme with a live preview", (Func<bool>) (() => MainMenus._showUiOptions)).AddSpace(8f, (Func<bool>) (() => MainMenus._showUiOptions)).AddLabel("Help", true, visibilityCondition: (Func<bool>) (() => MainMenus._showUiOptions)).AddLabel("Drag the title bar to move a menu.", visibilityCondition: (Func<bool>) (() => MainMenus._showUiOptions)).AddLabel("Right-click the title bar to minimize it.", visibilityCondition: (Func<bool>) (() => MainMenus._showUiOptions)).AddDynamicLabel((Func<string>) (() => $"Toggle menus with [{Keybinds.ToggleMenuKey}]."), visibilityCondition: (Func<bool>) (() => MainMenus._showUiOptions)).AddLabel("Hover any control to see what it does.", visibilityCondition: (Func<bool>) (() => MainMenus._showUiOptions)).AddLabel("Mouse-wheel scrolls long menus.", visibilityCondition: (Func<bool>) (() => MainMenus._showUiOptions)).AddSpace().AddButton($"Close Menu [{Keybinds.ToggleMenuKey.ToString()}]", (Action) (() => MainMenus.ShowMainMenu = false), $"Hide the menu (can be shown again with \"{Keybinds.ToggleMenuKey.ToString()}\" key)");
    MainMenus._mainMenu.SetPosition(vector2.x, vector2.y);
    GuiManager.RegisterMenu("main_menu", MainMenus._mainMenu);
  }

  private static string GetCurrentThemeName()
  {
    MainMenus.EnsureThemeManagerInitialized();
    return (ThemeManager.AvailableThemes.Count <= ThemeManager.CurrentThemeIndex ? 0 : (ThemeManager.CurrentThemeIndex >= 0 ? 1 : 0)) == 0 ? "DEFAULT" : ThemeManager.AvailableThemes[ThemeManager.CurrentThemeIndex].Name;
  }

  private static void EnsureThemeManagerInitialized()
  {
    if (ThemeManager.AvailableThemes.Count != 0)
      return;
    ThemeManager.Initialize();
  }

  public static void Draw()
  {
    if (Plugin.Ins.XPosition == 0)
      return;
    MenuConfig.TickThemeFade();
    MenuSnap.DrawOverlay();
    DropdownPopup.HandleInput();
    if ((MainMenus._desktopThemeBootstrapped ? 0 : (ThemeManager.AvailableThemes.Count > 0 ? 1 : 0)) != 0)
    {
      MenuConfig.CleanUp();
      MainMenus._desktopThemeBootstrapped = true;
    }
    if (ThemeEditorMenu.ConsumeColorsDirty())
      MenuConfig.CleanUp();
    if (!MainMenus.ShowMainMenu)
      return;
    if (LayoutEditor.IsEditing)
    {
      if (UtilMenuViewer.ShowViewer)
        UtilMenuViewer.Draw();
      TooltipOverlay.Draw();
    }
    else
    {
      if (MainMenus._mainMenu == null)
        MainMenus.InitializeMainMenu();
      MainMenus._mainMenu?.Draw();
      ModeMenus.Draw();
      if (Networking.InRoom)
      {
        if (NameTags.NameTagsEnabled)
          NameTagsMenu.Draw();
        if (Leaderboard.ShowLeaderboard)
          LeaderboardMenu.Draw();
        if (Interpolation.RigSmoothEnabled)
          InterpolationMenu.Draw();
      }
      if (Keybinds.ShowKeybindMenu)
        KeybindsMenu.Draw();
      if (Plugin.Ins.showNameChangerMenu)
        NameChangeMenu.Draw();
      if (WorldManager.ShowTimeChangerMenu)
        TimeChangerMenu.Draw();
      if (Scoreboard.ShowScoreboard)
        ScoreboardMenu.Draw();
      if (AutoRefManager.ShowMenu)
        AutoRefMenu.Draw();
      if (WorldManager.ShowMapLoader)
        MapLoaderMenu.Draw();
      if (SakTeleporter.ShowMenu)
        TeleporterMenu.Draw();
      if (FpsCounter.ShowFps)
        FpsCounter.Draw();
      if (MiniMap.MiniMapEnabled)
        MiniMapMenu.Draw();
      if (RewindViewer.ShowEditorMenu)
        RewindMenu.Draw();
      if (LegalChecker.LegalVisible)
        LegalBtnMenu.Draw();
      PresetsMenu.Draw();
      ThemeEditorMenu.Draw();
      if (UtilMenuViewer.ShowViewer)
        UtilMenuViewer.Draw();
      DropdownPopup.DrawVisuals();
      TooltipOverlay.Draw();
    }
  }
}
