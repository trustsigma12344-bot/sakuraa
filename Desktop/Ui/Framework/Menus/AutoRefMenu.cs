using SakuraaCastingMod.Features.AutoRef;
using SakuraaCastingMod.Features.Overlays;
using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Desktop.Ui.Framework.Menus;

public static class AutoRefMenu
{
  public static MenuBuilder _autoRefMenu;
  private const int MaxTeamSlots = 9;
  private const int MaxSpecSlots = 8;
  private static readonly float[] GtcLengths = new float[4]
  {
    60f,
    30f,
    20f,
    15f
  };

  private static bool Enabled()
  {
    return ((UnityEngine.Object) AutoRefManager.Instance != (UnityEngine.Object) null) && AutoRefManager.Instance.IsModEnabled;
  }

  private static bool IsGtc()
  {
    return ((UnityEngine.Object) AutoRefManager.Instance != (UnityEngine.Object) null) && AutoRefManager.Instance.currentGameMode == AutoRefManager.GameModes.Gtc;
  }

  private static bool IsCgt()
  {
    return ((UnityEngine.Object) AutoRefManager.Instance != (UnityEngine.Object) null) && AutoRefManager.Instance.currentGameMode == AutoRefManager.GameModes.Cgt;
  }

  private static bool HostEligible()
  {
    return ((UnityEngine.Object) AutoRefManager.Instance != (UnityEngine.Object) null) && AutoRefManager.Instance.IsLegitimateHost;
  }

  private static bool RefDeciding()
  {
    return ((UnityEngine.Object) AutoRefManager.Instance != (UnityEngine.Object) null) && AutoRefManager.Instance.IsAwaitingRefDecision;
  }

  private static bool RefDecidingCgt() => AutoRefMenu.RefDeciding() && AutoRefMenu.IsCgt();

  private static string Team(int n) => AutoRefManager.Instance?.TeamName(n) ?? $"Team {n}";

  private static string HostStatusLine()
  {
    AutoRefManager instance = AutoRefManager.Instance;
    string str1;
    if (((UnityEngine.Object) instance == (UnityEngine.Object) null))
    {
      str1 = "";
    }
    else
    {
      string str2 = instance.HostBlockReason();
      str1 = str2 == null ? "<color=lime>You are the lobby host.</color>" : $"<color=yellow>{str2}</color>";
    }
    return str1;
  }

  public static void Draw()
  {
    if (AutoRefMenu._autoRefMenu == null)
      AutoRefMenu.Initialize();
    AutoRefMenu._autoRefMenu.Draw();
  }

  public static void Initialize()
  {
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    MenuBuilder menuBuilder = new MenuBuilder("Auto Ref Beta", 300f).SetPositionRef(340f, 140f).AddSpace(15f).AddDynamicButton((Func<string>) (() => AutoRefMenu.Enabled() ? "Auto Ref: ON  (tap to stop)" : (AutoRefMenu.HostEligible() ? "Auto Ref: OFF  (tap to start)" : "Auto Ref: HOST ONLY")), (Action) (() => AutoRefManager.Instance?.ToggleEnabled()), "Start/stop the automated referee. Only the lobby host can run it.").AddDynamicLabel(new Func<string>(AutoRefMenu.HostStatusLine)).AddDynamicLabel((Func<string>) (() => "State: " + AutoRefManager.Instance?.FriendlyState), visibilityCondition: new Func<bool>(AutoRefMenu.Enabled)).AddDynamicLabel((Func<string>) (() => AutoRefManager.Instance?.statusLine ?? ""), visibilityCondition: new Func<bool>(AutoRefMenu.Enabled)).AddDynamicLabel((Func<string>) (() => $"<color=yellow>{AutoRefManager.Instance?.instructionLine}</color>"), visibilityCondition: new Func<bool>(AutoRefMenu.Enabled)).AddLabel("<color=orange>Substitute decision needed:</color>", visibilityCondition: new Func<bool>(AutoRefMenu.RefDeciding)).AddButton("Give 5 More Minutes", (Action) (() => AutoRefManager.Instance?.RefGiveMoreTime()), "Keep waiting another 5 minutes for a substitute", new Func<bool>(AutoRefMenu.RefDeciding)).AddHoldButton("Switch to Smaller Teams", 1.2f, (Action) (() => AutoRefManager.Instance?.RefReduceTeamSize()), "CGT only: hold to drop both teams to the smaller size and play on", new Func<bool>(AutoRefMenu.RefDecidingCgt)).AddHoldButton("Continue Uneven", 1.2f, (Action) (() => AutoRefManager.Instance?.RefContinueUneven()), "CGT only: hold to play on with uneven counts (e.g. 2v3)", new Func<bool>(AutoRefMenu.RefDecidingCgt)).AddHoldButton("Forfeit Short Team", 1.5f, (Action) (() => AutoRefManager.Instance?.RefForfeitShortTeam()), "Hold: the short-handed team forfeits the match", new Func<bool>(AutoRefMenu.RefDeciding)).AddSpace(8f).AddLabel("Setup", true).AddDropdown("Mode", (Func<IList<string>>) (() => (IList<string>) new List<string>()
    {
      "GTC (Classic)",
      "CGT (Chase)"
    }), (Func<int>) (() =>
    {
      AutoRefManager instance = AutoRefManager.Instance;
      return instance == null ? 0 : (int) instance.currentGameMode;
    }), (Action<int>) (idx => AutoRefManager.Instance?.SetMode(idx)), "Match mode: GTC (classic infection) or CGT (chase)").AddSlider("Team Size", ((UnityEngine.Object) AutoRefManager.Instance != (UnityEngine.Object) null) ? (float) AutoRefManager.Instance.TargetTeamSize : 4f, 2f, 9f, (Action<float>) (v => AutoRefManager.Instance?.SetTeamSize((int) v)), description: "Players per team (2v2 – 9v9)").AddLabel("<color=#bbbbbb>To join a lobby, use the main room joiner.</color>").AddDropdown("Match Length", (Func<IList<string>>) (() => (IList<string>) new List<string>()
    {
      "60m",
      "30m",
      "20m",
      "15m"
    }), (Func<int>) (() =>
    {
      AutoRefManager instance = AutoRefManager.Instance;
      float num1 = instance != null ? instance.GtcMatchLengthMinutes : 60f;
      int num2;
      for (int index = 0; index < AutoRefMenu.GtcLengths.Length; ++index)
      {
        if ((double) Math.Abs(AutoRefMenu.GtcLengths[index] - num1) < 0.5)
        {
          num2 = index;
          goto label_6;
        }
      }
      num2 = 0;
label_6:
      return num2;
    }), (Action<int>) (idx =>
    {
      if (!((UnityEngine.Object) AutoRefManager.Instance != (UnityEngine.Object) null))
        return;
      AutoRefManager.Instance.GtcMatchLengthMinutes = AutoRefMenu.GtcLengths[idx];
    }), "GTC global match timer length", new Func<bool>(AutoRefMenu.IsGtc)).AddSlider("Score To Win", ((UnityEngine.Object) AutoRefManager.Instance != (UnityEngine.Object) null) ? (float) AutoRefManager.Instance.CgtScoreToWin : 5f, 1f, 15f, (Action<float>) (v =>
    {
      if (!((UnityEngine.Object) AutoRefManager.Instance != (UnityEngine.Object) null))
        return;
      AutoRefManager.Instance.CgtScoreToWin = (int) v;
    }), description: "CGT first-to score limit", visibilityCondition: new Func<bool>(AutoRefMenu.IsCgt)).AddButton("Fix Infection", (Action) (() => AutoRefManager.Instance?.ForceFixCGTInfection()), "Force-correct chaser/runner infection states", (Func<bool>) (() => AutoRefMenu.Enabled() && AutoRefMenu.IsCgt() && AutoRefManager.Instance.currentState == AutoRefManager.GameStates.RoundStart)).AddSpace(8f).AddDynamicToggle("Scoreboard Overlay", (Func<bool>) (() => Scoreboard.ShowScoreboard), (Action<bool>) (v => Scoreboard.ShowScoreboard = v), "Show or hide the on-screen scoreboard overlay").AddDynamicLabel((Func<string>) (() => $"T1: {AutoRefManager.Instance?.team1Score}   T2: {AutoRefManager.Instance?.team2Score}"), visibilityCondition: new Func<bool>(AutoRefMenu.Enabled)).AddLabel("Team 1", visibilityCondition: new Func<bool>(AutoRefMenu.Enabled));
    for (int index = 0; index < 9; ++index)
    {
      int idx = index;
      menuBuilder.AddDynamicLabel((Func<string>) (() => AutoRefManager.Instance?.RosterLine(1, idx) ?? ""), visibilityCondition: (Func<bool>) (() => AutoRefMenu.Enabled() && idx < AutoRefManager.Instance.RosterCount(1)));
    }
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    menuBuilder.AddLabel("Team 2", visibilityCondition: new Func<bool>(AutoRefMenu.Enabled));
    for (int index = 0; index < 9; ++index)
    {
      int idx = index;
      menuBuilder.AddDynamicLabel((Func<string>) (() => AutoRefManager.Instance?.RosterLine(2, idx) ?? ""), visibilityCondition: (Func<bool>) (() => AutoRefMenu.Enabled() && idx < AutoRefManager.Instance.RosterCount(2)));
    }
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    menuBuilder.AddLabel("Spectators", visibilityCondition: new Func<bool>(AutoRefMenu.Enabled));
    for (int index = 0; index < 8; ++index)
    {
      int idx = index;
      menuBuilder.AddDynamicLabel((Func<string>) (() => AutoRefManager.Instance?.RosterLine(0, idx) ?? ""), visibilityCondition: (Func<bool>) (() => AutoRefMenu.Enabled() && idx < AutoRefManager.Instance.RosterCount(0)));
    }
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    menuBuilder.AddSpace(8f).AddLabel("Match Control", true).AddDynamicLabel((Func<string>) (() => $"Score:  {AutoRefMenu.Team(1)} {AutoRefManager.Instance?.team1Score}  -  {AutoRefManager.Instance?.team2Score} {AutoRefMenu.Team(2)}"), visibilityCondition: new Func<bool>(AutoRefMenu.Enabled)).AddDynamicButton((Func<string>) (() => AutoRefMenu.Team(1) + "  +1"), (Action) (() => AutoRefManager.Instance?.AdjustScore(1, 1)), "Add a point to this team", new Func<bool>(AutoRefMenu.Enabled)).AddDynamicButton((Func<string>) (() => AutoRefMenu.Team(1) + "  -1"), (Action) (() => AutoRefManager.Instance?.AdjustScore(1, -1)), "Remove a point from this team", new Func<bool>(AutoRefMenu.Enabled)).AddDynamicButton((Func<string>) (() => AutoRefMenu.Team(2) + "  +1"), (Action) (() => AutoRefManager.Instance?.AdjustScore(2, 1)), "Add a point to this team", new Func<bool>(AutoRefMenu.Enabled)).AddDynamicButton((Func<string>) (() => AutoRefMenu.Team(2) + "  -1"), (Action) (() => AutoRefManager.Instance?.AdjustScore(2, -1)), "Remove a point from this team", new Func<bool>(AutoRefMenu.Enabled)).AddHoldButton("Undo Last Round Point", 1.2f, (Action) (() => AutoRefManager.Instance?.UndoLastRound()), "Hold: revert the point from the most recent round result", new Func<bool>(AutoRefMenu.Enabled)).AddHoldButton("Restart This Round", 1.2f, (Action) (() => AutoRefManager.Instance?.ForceResetRound()), "Hold: re-tag everyone and replay the current round. Scores unchanged.", new Func<bool>(AutoRefMenu.Enabled)).AddHoldButton("Re-Detect Teams", 1.2f, (Action) (() => AutoRefManager.Instance?.ReLockTeams()), "Hold: re-read jersey colors and re-pin teams. Use after a sub or color change.", new Func<bool>(AutoRefMenu.Enabled)).AddHoldButton("Swap Chasers", 1.2f, (Action) (() => AutoRefManager.Instance?.SwapChasers()), "CGT only: hold to flip which team is chasing.", (Func<bool>) (() => AutoRefMenu.Enabled() && AutoRefMenu.IsCgt())).AddHoldButton("Assign Waiting Sub", 1.2f, (Action) (() => AutoRefManager.Instance?.AssignSubFromSpectators()), "Hold: put the first spectator onto the short-handed team (manual sub).", (Func<bool>) (() => AutoRefMenu.Enabled() && AutoRefManager.Instance.IsShortHanded)).AddHoldButton("Pause Ref", 1f, (Action) (() => AutoRefManager.Instance?.TogglePause()), "Hold: freeze the engine without disabling it.", (Func<bool>) (() => AutoRefMenu.Enabled() && !AutoRefManager.Instance.IsPaused)).AddHoldButton("Resume Ref", 1f, (Action) (() => AutoRefManager.Instance?.TogglePause()), "Hold: resume from where it paused.", (Func<bool>) (() => AutoRefMenu.Enabled() && AutoRefManager.Instance.IsPaused)).AddSpace(8f).AddLabel("<color=#ff6666>Danger Zone</color>", true).AddHoldButton("Set Score 0 - 0", 1.5f, (Action) (() => AutoRefManager.Instance?.ResetScores()), "Hold: zero both team scores.", new Func<bool>(AutoRefMenu.Enabled)).AddHoldButton("New Match (Full Reset)", 2f, (Action) (() => AutoRefManager.Instance?.FullReset()), "Hold: clear scores, teams, and all match state for a fresh match.", new Func<bool>(AutoRefMenu.Enabled));
    AutoRefMenu._autoRefMenu = menuBuilder;
  }
}
