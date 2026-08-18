using GorillaNetworking;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using SakuraaCastingMod.Features.Overlays;
using SakuraaCastingMod.Shared.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Features.AutoRef;

public class AutoRefManager : MonoBehaviour
{
  public static AutoRefManager Instance;
  private const int MinCodeLength = 5;
  [SavedSetting("AutoRefShowMenu", false)]
  public static bool ShowMenu;
  public AutoRefManager.GameModes currentGameMode = AutoRefManager.GameModes.Gtc;
  public int team1Score = 0;
  public int team2Score = 0;
  public List<NetPlayer> Team1Players = new List<NetPlayer>();
  public List<NetPlayer> Team2Players = new List<NetPlayer>();
  public List<NetPlayer> Spectators = new List<NetPlayer>();
  public TagTriger StumpTrigger;
  private readonly List<GameObject> _arena = new List<GameObject>();
  public float GtcMatchLengthMinutes = 60f;
  private float _gtcMatchStartTime = -1f;
  public int CgtScoreToWin = 5;
  private bool _matchIsOver = false;
  private float _gtcRoundStartTime = -1f;
  private float _refOldClock = -1f;
  private readonly Dictionary<NetPlayer, float> _jumpPenaltyUntil = new Dictionary<NetPlayer, float>();
  private float _jumpReapplyTime = 0.0f;
  private const float SubWaitSeconds = 180f;
  private const float RefExtensionSeconds = 300f;
  private const int MaxSubBreaks = 3;
  private float _subWaitUntil = 0.0f;
  private int _subBreaksUsed = 0;
  private bool _playUneven = false;
  private float _subVoiceDebounce = 0.0f;
  private bool _paused = false;
  private float _pauseStart = 0.0f;
  private int _lastScoringTeam = 0;
  private bool _rosterLocked = false;
  private readonly HashSet<string> _team1UserIds = new HashSet<string>();
  private readonly HashSet<string> _team2UserIds = new HashSet<string>();
  public string statusLine = "Disabled";
  public string instructionLine = "";
  public AutoRefManager.GameStates currentState = AutoRefManager.GameStates.None;
  private float _stateTimer = 0.0f;
  private float _checkTimer = 0.0f;
  private float _stallStartTime = 0.0f;
  private float _readyStartTime = 0.0f;
  private float _lastFreezeCooldown = 0.0f;
  private float _spectatorFreezeTimer = 0.0f;
  private float _selectionPhaseTimer = 30f;
  private float _voiceDebounce = 0.0f;
  private float _postTagDelay = 0.0f;
  private bool _hasAnnouncedMatch = false;
  private bool _spokenIntro = false;
  private bool _spokenCount3;
  private bool _spokenCount2;
  private bool _spokenCount1;
  private bool _spokenGo;
  private bool _spokenRunnersRun;
  private bool _spokenTaggersTag;
  private int _lastT1Alive = -1;
  private int _lastT2Alive = -1;
  private bool _spoken30SecWarn = false;
  private bool _spoken10SecWarn = false;
  private bool _cgtIsSecondHeat = false;
  private float _cgtHeatTimer = 0.0f;
  private float _cgtTeam1SurvivalTime = 0.0f;
  private float _cgtTeam2SurvivalTime = 0.0f;
  private bool _cgtTeam1IsChasing = true;
  private const float CgtTimeCap = 180f;
  private bool _waitingForRunnersAudio = false;
  private float _taggersReleaseTimestamp = 0.0f;
  private GorillaTagManager _tagManager;
  private const float PregameWait = 5f;
  private const float ColorTolerance = 0.3f;
  private const float StartDelaySeconds = 10f;
  private const float SelectionTimeout = 30f;

  public bool IsModEnabled { get; private set; }

  public bool IsLegitimateHost => PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient;

  public bool IsActive => this.IsModEnabled && this.IsLegitimateHost;

  public bool IsRoundLive
  {
    get => !this._paused && this.currentState == AutoRefManager.GameStates.MidRound;
  }

  public int TargetTeamSize { get; private set; } = 4;

  public Color Team1Color { get; private set; } = Color.white;

  public Color Team2Color { get; private set; } = Color.white;

  public bool AreTeamsDetected { get; private set; } = false;

  private static void Speak(string text) => RefVoiceManager.Instance?.Speak(text);

  private static void PreCache(string text) => RefVoiceManager.Instance?.PreCache(text);

  private static bool VoiceBusy
  {
    get
    {
      return ((UnityEngine.Object) RefVoiceManager.Instance != (UnityEngine.Object) null) && RefVoiceManager.Instance.IsSpeaking;
    }
  }

  private void Awake()
  {
    AutoRefManager.Instance = this;
    if (!((UnityEngine.Object) RefVoiceManager.Instance == (UnityEngine.Object) null))
      return;
    ((Component) this).gameObject.AddComponent<RefVoiceManager>();
  }

  private void BuildArena()
  {
    if (this.StumpTrigger != null)
      return;
    this.StumpTrigger = new TagTriger(new Vector3(-66.55f, 12.104f, -80.098f), new Quaternion(0.0f, 0.0f, 0.0f, 1f), new Vector3(1f, 1.2f, 1f));
    this.StumpTrigger.ShouldTag = false;
    this._arena.Add(this.StumpTrigger.Obj);
    GameObject primitive = GameObject.CreatePrimitive((PrimitiveType) 3);
    primitive.transform.position = new Vector3(-63.774f, 11.036f, -83.045f);
    Renderer component = primitive.GetComponent<Renderer>();
    if (((UnityEngine.Object) component != (UnityEngine.Object) null))
      UnityEngine.Object.Destroy((UnityEngine.Object) component);
    this._arena.Add(primitive);
    TagTriger tagTriger1 = new TagTriger(new Vector3(-63.486f, 12.641f, -84.689f), new Quaternion(0.0f, 0.0f, 0.0f, 1f), new Vector3(1.6452055f, 1.74323523f, 1f));
    TagTriger tagTriger2 = new TagTriger(new Vector3(-68.346f, 12.249f, -85.199f), new Quaternion(0.0f, 0.260782629f, 0.0f, 0.9653976f), new Vector3(0.979867935f, 1.64636362f, 1f));
    TagTriger tagTriger3 = new TagTriger(new Vector3(-66.954f, 10.365f, -84.502f), new Quaternion(0.0f, 0.260782629f, 0.0f, 0.9653976f), new Vector3(1.32105792f, 1.64636362f, 1.3124f));
    TagTriger tagTriger4 = new TagTriger(new Vector3(-20.55f, 5.244f, -73.32f), new Quaternion(0.0f, 0.173390046f, 0.0f, 0.984853268f), new Vector3(1.32105792f, 1.64636362f, 4.32357025f));
    this._arena.Add(tagTriger1.Obj);
    this._arena.Add(tagTriger2.Obj);
    this._arena.Add(tagTriger3.Obj);
    this._arena.Add(tagTriger4.Obj);
  }

  private void DestroyArena()
  {
    foreach (GameObject gameObject in this._arena)
    {
      if (((UnityEngine.Object) gameObject != (UnityEngine.Object) null))
        UnityEngine.Object.Destroy((UnityEngine.Object) gameObject);
    }
    this._arena.Clear();
    this.StumpTrigger = (TagTriger) null;
  }

  public void ToggleEnabled()
  {
    if (this.IsModEnabled)
    {
      this.DisableRef();
    }
    else
    {
      string message = this.HostBlockReason();
      if (message != null)
        Notification.Send(message, Color.red);
      else
        this.EnableRef();
    }
  }

  public string HostBlockReason()
  {
    return PhotonNetwork.InRoom ? (PhotonNetwork.IsMasterClient ? ((PhotonNetwork.CurrentRoom != null ? PhotonNetwork.CurrentRoom.Name : "").Length < 5 ? $"Auto Ref requires you to be in a code with at least {5.ToString()} characters." : (string) null) : "Only the lobby host can use Auto Ref.") : "You must be in a lobby first.";
  }

  private void EnableRef()
  {
    string message = this.HostBlockReason();
    if (message != null)
    {
      Notification.Send(message, Color.red);
    }
    else
    {
      this.IsModEnabled = true;
      this.BuildArena();
      this._hasAnnouncedMatch = false;
      this.FullReset();
      AutoRefManager.Speak("Auto Ref Enabled.");
      this.PreCacheCommonPhrases();
      Notification.Send("Auto Ref enabled.", Color.green);
    }
  }

  private void DisableRef(bool autoLostHost = false, bool autoBadCode = false)
  {
    this.IsModEnabled = false;
    this._paused = false;
    this.UnlockRoster();
    this.DestroyArena();
    this._jumpPenaltyUntil.Clear();
    RefVoiceManager.Instance?.ResetState();
    this.statusLine = autoLostHost ? "Disabled (not host)" : (autoBadCode ? "Disabled (code too short)" : "Disabled");
    this.instructionLine = "";
    Notification.Send(autoLostHost ? "Auto Ref turned off: you are no longer the lobby host." : (autoBadCode ? $"Auto Ref turned off: room code must be at least {5} characters." : "Auto Ref disabled."), Color.yellow);
  }

  private bool IsCodeLongEnough()
  {
    return PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.Name != null && PhotonNetwork.CurrentRoom.Name.Length >= 5;
  }

  public void SetMode(int index)
  {
    this.ChangeMode(index == 1 ? AutoRefManager.GameModes.Cgt : AutoRefManager.GameModes.Gtc);
  }

  public void SetTeamSize(int newSize) => this.TargetTeamSize = Mathf.Clamp(newSize, 2, 9);

  public void ResetScores()
  {
    this.team1Score = 0;
    this.team2Score = 0;
    this._gtcMatchStartTime = -1f;
    this._lastScoringTeam = 0;
    Notification.Send("Score set to 0 – 0.", Color.green);
  }

  public void ForceResetRound()
  {
    this.TransitionTo(AutoRefManager.GameStates.RoundEnd);
    Notification.Send("Round restarted.", Color.green);
  }

  public bool IsPaused => this._paused;

  public void TogglePause()
  {
    if (!this.IsModEnabled)
      return;
    this._paused = !this._paused;
    if (this._paused)
    {
      this._pauseStart = Time.time;
      RefVoiceManager.Instance?.ResetState();
    }
    else
    {
      float num = Time.time - this._pauseStart;
      if ((double) this._gtcRoundStartTime > 0.0)
        this._gtcRoundStartTime += num;
      if ((double) this._gtcMatchStartTime > 0.0)
        this._gtcMatchStartTime += num;
      this._subWaitUntil += num;
      this._taggersReleaseTimestamp += num;
    }
    Notification.Send(this._paused ? "Auto Ref paused." : "Auto Ref resumed.", this._paused ? Color.yellow : Color.green);
  }

  public void AdjustScore(int team, int delta)
  {
    if (team == 1)
      this.team1Score = Mathf.Max(0, this.team1Score + delta);
    else
      this.team2Score = Mathf.Max(0, this.team2Score + delta);
    this._lastScoringTeam = 0;
  }

  public void UndoLastRound()
  {
    if ((this._lastScoringTeam != 1 ? 0 : (this.team1Score > 0 ? 1 : 0)) != 0)
    {
      --this.team1Score;
    }
    else
    {
      if ((this._lastScoringTeam != 2 ? 0 : (this.team2Score > 0 ? 1 : 0)) == 0)
      {
        Notification.Send("Nothing to undo.", Color.yellow);
        return;
      }
      --this.team2Score;
    }
    Notification.Send($"Undid last point ({this.TeamName(this._lastScoringTeam)}).", Color.green);
    this._lastScoringTeam = 0;
  }

  public void SwapChasers()
  {
    if (this.currentGameMode != AutoRefManager.GameModes.Cgt)
      return;
    this._cgtTeam1IsChasing = !this._cgtTeam1IsChasing;
    Notification.Send($"Chasers: {this.TeamName(this._cgtTeam1IsChasing ? 1 : 2)}.", Color.green);
  }

  public bool IsShortHanded => this._rosterLocked && this.IsAnyTeamShort();

  public void AssignSubFromSpectators()
  {
    if (!this._rosterLocked)
      Notification.Send("Teams aren't locked yet.", Color.yellow);
    else if (this.Spectators.Count != 0)
    {
      NetPlayer spectator = this.Spectators[0];
      if ((spectator == null ? 1 : (string.IsNullOrEmpty(spectator.UserId) ? 1 : 0)) != 0)
        return;
      int shortTeam = this.GetShortTeam();
      (shortTeam == 1 ? this._team1UserIds : this._team2UserIds).Add(spectator.UserId);
      Notification.Send($"{spectator.NickName} assigned to {this.TeamName(shortTeam)}.", Color.green);
    }
    else
      Notification.Send("No spectators to assign.", Color.yellow);
  }

  public string FriendlyState
  {
    get
    {
      string friendlyState;
      if (this._paused)
      {
        friendlyState = "Paused";
      }
      else
      {
        switch (this.currentState)
        {
          case AutoRefManager.GameStates.PreGame:
            friendlyState = "Waiting for reset";
            break;
          case AutoRefManager.GameStates.RoundStart:
            friendlyState = this.currentGameMode == AutoRefManager.GameModes.Gtc ? "Selecting taggers" : "Heat setup";
            break;
          case AutoRefManager.GameStates.Countdown:
            friendlyState = "Countdown";
            break;
          case AutoRefManager.GameStates.MidRound:
            friendlyState = "Round in progress";
            break;
          case AutoRefManager.GameStates.RoundEnd:
            friendlyState = "Round over";
            break;
          case AutoRefManager.GameStates.None:
            friendlyState = "Idle";
            break;
          case AutoRefManager.GameStates.MatchOver:
            friendlyState = "Match over";
            break;
          case AutoRefManager.GameStates.SubWait:
            friendlyState = "Waiting for substitute";
            break;
          case AutoRefManager.GameStates.RefDecision:
            friendlyState = "Referee deciding";
            break;
          default:
            friendlyState = this.currentState.ToString();
            break;
        }
      }
      return friendlyState;
    }
  }

  public void JoinRoom(string code)
  {
    if (string.IsNullOrWhiteSpace(code))
      return;
    this.StartCoroutine(this.JoinRoomRoutine(code.ToUpper()));
  }

  public int RosterCount(int team)
  {
    if (team == 1)
      return this.Team1Players.Count;
    return team != 2 ? this.Spectators.Count : this.Team2Players.Count;
  }

  public float GetRefClockSeconds()
  {
    return this.currentGameMode == AutoRefManager.GameModes.Cgt ? Mathf.Clamp(this._cgtHeatTimer, 0.0f, 180f) : ((double) this._gtcRoundStartTime > 0.0 ? Mathf.Max(0.0f, Time.time - this._gtcRoundStartTime) : 0.0f);
  }

  public string TeamName(int team)
  {
    string str1;
    if (this.AreTeamsDetected)
    {
      string str2 = AutoRefManager.ColorName(this.Team1Color);
      string str3 = AutoRefManager.ColorName(this.Team2Color);
      if (str2 != str3)
      {
        string str4 = team == 1 ? str2 : str3;
        str1 = $"{char.ToUpper(str4[0]).ToString()}{str4.Substring(1)} team";
        goto label_4;
      }
    }
    str1 = team == 1 ? "Team 1" : "Team 2";
label_4:
    return str1;
  }

  private static string ColorName(Color c)
  {
    float num1 = default;
    float num2 = default;
    float num3 = default;
    Color.RGBToHSV(c, out num1, out num2, out num3);
    string str;
    if ((double) num3 >= 0.18000000715255737)
    {
      if ((double) num2 < 0.18000000715255737)
      {
        str = (double) num3 > 0.75 ? "white" : "gray";
      }
      else
      {
        float num4 = num1 * 360f;
        str = (double) num4 < 15.0 ? "red" : ((double) num4 >= 42.0 ? ((double) num4 < 70.0 ? "yellow" : ((double) num4 >= 160.0 ? ((double) num4 < 200.0 ? "cyan" : ((double) num4 < 250.0 ? "blue" : ((double) num4 >= 295.0 ? ((double) num4 >= 335.0 ? "red" : "pink") : "purple"))) : "green")) : "orange");
      }
    }
    else
      str = "black";
    return str;
  }

  private static string SpokenTime(float totalSeconds)
  {
    int num1 = Mathf.Max(0, Mathf.RoundToInt(totalSeconds));
    int num2 = num1 / 60;
    int num3 = num1 % 60;
    string str1 = $"{num2} minute{(num2 == 1 ? (object) "" : (object) "s")}";
    string str2 = $"{num3} second{(num3 == 1 ? (object) "" : (object) "s")}";
    return num2 == 0 ? str2 : (num3 == 0 ? str1 : $"{str1} and {str2}");
  }

  private IEnumerable<NetPlayer> RosteredPlayers()
  {
    return this.Team1Players.Take<NetPlayer>(this.TargetTeamSize).Concat<NetPlayer>(this.Team2Players.Take<NetPlayer>(this.TargetTeamSize));
  }

  private void PenalizeJumpStart(NetPlayer p)
  {
    if (p == null)
      return;
    float num1 = Time.time + 5f;
    float num2;
    if ((!this._jumpPenaltyUntil.TryGetValue(p, out num2) ? 1 : ((double) num2 < (double) num1 ? 1 : 0)) == 0)
      return;
    this._jumpPenaltyUntil[p] = num1;
  }

  private void ApplyActiveJumpPenalties()
  {
    if (this._jumpPenaltyUntil.Count == 0)
      return;
    bool flag;
    if (flag = (double) Time.time >= (double) this._jumpReapplyTime)
      this._jumpReapplyTime = Time.time + 1f;
    List<NetPlayer> netPlayerList = (List<NetPlayer>) null;
    foreach (KeyValuePair<NetPlayer, float> keyValuePair in this._jumpPenaltyUntil)
    {
      if ((double) Time.time < (double) keyValuePair.Value)
      {
        if ((!flag ? 0 : (keyValuePair.Key != null ? 1 : 0)) != 0)
          this.ApplyStallPenalty(keyValuePair.Key);
      }
      else
        (netPlayerList ?? (netPlayerList = new List<NetPlayer>())).Add(keyValuePair.Key);
    }
    if (netPlayerList == null)
      return;
    foreach (NetPlayer key in netPlayerList)
      this._jumpPenaltyUntil.Remove(key);
  }

  public string RosterLine(int team, int index)
  {
    List<NetPlayer> netPlayerList1;
    switch (team)
    {
      case 1:
        netPlayerList1 = this.Team1Players;
        break;
      case 2:
        netPlayerList1 = this.Team2Players;
        break;
      default:
        netPlayerList1 = this.Spectators;
        break;
    }
    List<NetPlayer> netPlayerList2 = netPlayerList1;
    string str1;
    if ((index < 0 ? 1 : (index >= netPlayerList2.Count ? 1 : 0)) != 0)
    {
      str1 = "";
    }
    else
    {
      NetPlayer p = netPlayerList2[index];
      if (p != null)
      {
        string str2 = this.IsInfected(p) ? " <color=red>(INF)</color>" : " <color=lime>(SUR)</color>";
        if ((team == 0 ? 0 : (this.IsExtraPlayer(p) ? 1 : 0)) != 0)
          str2 += " <color=orange>(X)</color>";
        str1 = p.NickName + str2;
      }
      else
        str1 = "";
    }
    return str1;
  }

  private void ChangeMode(AutoRefManager.GameModes newMode)
  {
    if (this.currentGameMode == newMode)
      return;
    this.currentGameMode = newMode;
    this.UnlockRoster();
    this.TransitionTo(AutoRefManager.GameStates.PreGame);
    this._cgtIsSecondHeat = false;
  }

  public void FullReset()
  {
    this.team1Score = 0;
    this.team2Score = 0;
    this._gtcMatchStartTime = -1f;
    this._gtcRoundStartTime = -1f;
    this._refOldClock = -1f;
    this._jumpPenaltyUntil.Clear();
    this._subBreaksUsed = 0;
    this._playUneven = false;
    this._subWaitUntil = 0.0f;
    this._paused = false;
    this._lastScoringTeam = 0;
    this._matchIsOver = false;
    this._cgtIsSecondHeat = false;
    this._cgtTeam1IsChasing = true;
    this.UnlockRoster();
    this.TransitionTo(AutoRefManager.GameStates.PreGame);
  }

  private void PreCacheCommonPhrases()
  {
    foreach (string text in new List<string>()
    {
      "Three",
      "Two",
      "One",
      "Runners, run!",
      "Taggers, tag!",
      "Round over.",
      "Taggers selected. Taggers, get to the gazebo. Runners, run when you hear taggers tag.",
      "All players, get under gazebo, away from edges.",
      "Time cap.",
      "30 seconds remaining.",
      "10 seconds!"
    })
      AutoRefManager.PreCache(text);
  }

  private IEnumerator JoinRoomRoutine(string code)
  {
    this.statusLine = $"Joining Room {code}...";
    if (PhotonNetwork.InRoom)
      PhotonNetwork.LeaveRoom(true);
    yield return (object) new WaitForSeconds(1f);
    if (((UnityEngine.Object) PhotonNetworkController.Instance != (UnityEngine.Object) null))
      PhotonNetworkController.Instance.AttemptToJoinSpecificRoom(code, (JoinType) 0);
  }

  private void Update()
  {
    if (!this.IsModEnabled)
      return;
    if (!this.IsLegitimateHost)
      this.DisableRef(true);
    else if (this.IsCodeLongEnough())
    {
      if (!this._paused)
      {
        this._tagManager = Networking.GtagManager;
        if (((UnityEngine.Object) this._tagManager == (UnityEngine.Object) null))
          this._tagManager = GorillaGameManager.instance as GorillaTagManager;
        if (((UnityEngine.Object) this._tagManager == (UnityEngine.Object) null) || !PhotonNetwork.InRoom)
          return;
        this.TagSelfLogic();
        this.UpdateTeams();
        if (this.currentState == AutoRefManager.GameStates.PreGame)
          this.UntagNearComputer();
        if ((this.currentState == AutoRefManager.GameStates.PreGame || this.currentState == AutoRefManager.GameStates.RoundEnd || this.currentState == AutoRefManager.GameStates.MatchOver || this.currentState == AutoRefManager.GameStates.SubWait ? 0 : (this.currentState != AutoRefManager.GameStates.RefDecision ? 1 : 0)) != 0)
          this.ManageRestrictedPlayers();
        if ((this.currentGameMode != AutoRefManager.GameModes.Gtc || (double) this._gtcMatchStartTime <= 0.0 || this._matchIsOver || this.currentState == AutoRefManager.GameStates.MatchOver || this.currentState == AutoRefManager.GameStates.SubWait ? 0 : (this.currentState != AutoRefManager.GameStates.RefDecision ? 1 : 0)) != 0 && (double) Time.time - (double) this._gtcMatchStartTime > (double) this.GtcMatchLengthMinutes * 60.0)
          this.EndMatchGTC();
        this.RunStateMachine();
        Scoreboard.SetAutoRefScores(this.team1Score, this.team2Score);
        Scoreboard.SetAutoRefTime(this.GetRefClockSeconds());
        Scoreboard.SetAutoRefOldTime(this._refOldClock);
        this.ApplyActiveJumpPenalties();
      }
      else
      {
        this.statusLine = "Paused";
        this.instructionLine = "Hold Resume to continue.";
      }
    }
    else
      this.DisableRef(autoBadCode: true);
  }

  private void TagSelfLogic()
  {
    if ((PhotonNetwork.CurrentRoom.PlayerCount < (byte) 1 ? 0 : (!this._tagManager.currentInfected.Contains((PhotonNetwork.LocalPlayer)) ? 1 : 0)) == 0)
      return;
    this._tagManager.AddInfectedPlayer((PhotonNetwork.LocalPlayer), true);
  }

  public void TryTagPlayer(NetPlayer player)
  {
    if (((UnityEngine.Object) this._tagManager == (UnityEngine.Object) null) || (!PhotonNetwork.IsMasterClient ? 0 : (!this._tagManager.currentInfected.Contains(player) ? 1 : 0)) == 0)
      return;
    this._tagManager.AddInfectedPlayer(player, true);
  }

  public void TryUntagPlayer(NetPlayer player)
  {
    if ((((UnityEngine.Object) this._tagManager == (UnityEngine.Object) null) ? 1 : (!PhotonNetwork.IsMasterClient ? 1 : 0)) != 0 || !this._tagManager.currentInfected.Contains(player))
      return;
    this._tagManager.currentInfected.Remove(player);
    MethodInfo method = typeof (GorillaTagManager).GetMethod("ClearInfectionState", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
    if (!(method != (MethodInfo) null))
      return;
    method.Invoke((object) this._tagManager, (object[]) null);
  }

  public bool IsExtraPlayer(NetPlayer p)
  {
    return this.Team1Players.Contains(p) ? this.Team1Players.IndexOf(p) >= this.TargetTeamSize : this.Team2Players.Contains(p) && this.Team2Players.IndexOf(p) >= this.TargetTeamSize;
  }

  private void ManageRestrictedPlayers()
  {
    bool flag;
    if (flag = (double) Time.time > (double) this._spectatorFreezeTimer)
      this._spectatorFreezeTimer = Time.time + 3f;
    List<NetPlayer> netPlayerList = new List<NetPlayer>((IEnumerable<NetPlayer>) this.Spectators);
    for (int targetTeamSize = this.TargetTeamSize; targetTeamSize < this.Team1Players.Count; ++targetTeamSize)
      netPlayerList.Add(this.Team1Players[targetTeamSize]);
    for (int targetTeamSize = this.TargetTeamSize; targetTeamSize < this.Team2Players.Count; ++targetTeamSize)
      netPlayerList.Add(this.Team2Players[targetTeamSize]);
    foreach (NetPlayer netPlayer in netPlayerList)
    {
      if (netPlayer != null)
      {
        if (this.IsInfected(netPlayer))
          this.TryUntagPlayer(netPlayer);
        if (flag)
          this.FreezeNonPlayer(netPlayer);
      }
    }
  }

  private void ApplyStallPenalty(NetPlayer player) => this.SendStatusEffect("TaggedTime", player);

  private void FreezeNonPlayer(NetPlayer player) => this.SendStatusEffect("FrozenTime", player);

  private void SendStatusEffect(string effectName, NetPlayer player)
  {
    Type type = AccessTools.TypeByName("RoomSystem");
    if (type == (Type) null)
      return;
    MethodInfo methodInfo = AccessTools.Method(type, "SendStatusEffectToPlayer", (Type[]) null, (Type[]) null);
    if (methodInfo == (MethodInfo) null)
      return;
    Type nestedType = type.GetNestedType("StatusEffects", BindingFlags.NonPublic | BindingFlags.Public);
    if (nestedType == (Type) null)
      return;
    object obj = Enum.Parse(nestedType, effectName);
    methodInfo.Invoke((object) null, new object[2]
    {
      obj,
      (object) player
    });
  }

  private void TagEveryone()
  {
    if ((!PhotonNetwork.IsMasterClient ? 1 : (((UnityEngine.Object) this._tagManager == (UnityEngine.Object) null) ? 1 : 0)) != 0)
      return;
    foreach (Player player in PhotonNetwork.PlayerList)
    {
      NetPlayer netPlayer = (player);
      if (!this._tagManager.currentInfected.Contains(netPlayer))
        this._tagManager.AddInfectedPlayer(netPlayer, true);
    }
  }

  public bool CheckIfPlayerInGazebo(NetPlayer player)
  {
    float num1 = -58f;
    float num2 = -66.43f;
    float num3 = 6f;
    float num4 = 2.16f;
    float num5 = -59.13f;
    float num6 = -66f;
    VRRig playerVrRig = ((GorillaGameManager) this._tagManager).FindPlayerVRRig(player);
    bool flag;
    if (!((UnityEngine.Object) playerVrRig == (UnityEngine.Object) null))
    {
      Vector3 position = ((Component) playerVrRig).transform.position;
      flag = (double) position.x < (double) num1 && (double) position.x > (double) num2 && (double) position.y < (double) num3 && (double) position.y > (double) num4 && (double) position.z < (double) num5 && (double) position.z > (double) num6;
    }
    else
      flag = false;
    return flag;
  }

  public void UntagNearComputer()
  {
    float num1 = -67.775f;
    float num2 = -69.155f;
    float num3 = 12.788f;
    float num4 = 11.229f;
    float num5 = -81.958f;
    float num6 = -83.991f;
    foreach (NetPlayer player in this._tagManager.currentInfected)
    {
      if (player != NetworkSystem.Instance.LocalPlayer)
      {
        VRRig playerVrRig = ((GorillaGameManager) this._tagManager).FindPlayerVRRig(player);
        if (!((UnityEngine.Object) playerVrRig == (UnityEngine.Object) null))
        {
          Vector3 position = ((Component) playerVrRig).transform.position;
          if (((double) position.x <= (double) num2 || (double) position.x >= (double) num1 || (double) position.y <= (double) num4 || (double) position.y >= (double) num3 || (double) position.z >= (double) num6 ? 0 : ((double) position.z < (double) num5 ? 1 : 0)) != 0)
            this.TryUntagPlayer(player);
        }
      }
    }
  }

  private bool IsInfected(NetPlayer p)
  {
    return ((UnityEngine.Object) this._tagManager != (UnityEngine.Object) null) && this._tagManager.currentInfected.Contains(p);
  }

  private void UpdateTeams()
  {
    if ((double) Time.time < (double) this._checkTimer)
      return;
    this._checkTimer = Time.time + 1.5f;
    if (!this._rosterLocked)
      this.UpdateTeamsByColor();
    else
      this.UpdateTeamsByRoster();
  }

  private void UpdateTeamsByColor()
  {
    List<(Color, NetPlayer)> valueTupleList = new List<(Color, NetPlayer)>();
    foreach (VRRig vrrig in PlayerTranslator.Vrrigs)
    {
      if ((((UnityEngine.Object) vrrig == (UnityEngine.Object) null) ? 1 : (vrrig.OwningNetPlayer == null ? 1 : 0)) == 0 && (vrrig.isOfflineVRRig ? 1 : (vrrig.isMyPlayer ? 1 : 0)) == 0)
        valueTupleList.Add((vrrig.playerColor, vrrig.OwningNetPlayer));
    }
    List<AutoRefManager.ColorCluster> colorClusterList = new List<AutoRefManager.ColorCluster>();
    foreach ((Color, NetPlayer) valueTuple in valueTupleList)
    {
      bool flag = false;
      foreach (AutoRefManager.ColorCluster colorCluster in colorClusterList)
      {
        if (this.IsColorClose(valueTuple.Item1, colorCluster.AverageColor, 0.3f))
        {
          colorCluster.Add(valueTuple.Item1, valueTuple.Item2);
          flag = true;
          break;
        }
      }
      if (!flag)
        colorClusterList.Add(new AutoRefManager.ColorCluster(valueTuple.Item1, valueTuple.Item2));
    }
    colorClusterList.Sort((Comparison<AutoRefManager.ColorCluster>) ((a, b) => b.Count.CompareTo(a.Count)));
    this.Team1Players.Clear();
    this.Team2Players.Clear();
    this.Spectators.Clear();
    if (colorClusterList.Count >= 1)
    {
      this.Team1Color = colorClusterList[0].AverageColor;
      this.Team1Players.AddRange((IEnumerable<NetPlayer>) colorClusterList[0].Members);
    }
    if (colorClusterList.Count < 2)
    {
      this.AreTeamsDetected = false;
    }
    else
    {
      this.Team2Color = colorClusterList[1].AverageColor;
      this.Team2Players.AddRange((IEnumerable<NetPlayer>) colorClusterList[1].Members);
      this.AreTeamsDetected = true;
    }
    for (int index = 2; index < colorClusterList.Count; ++index)
      this.Spectators.AddRange((IEnumerable<NetPlayer>) colorClusterList[index].Members);
    if ((double) Time.time <= (double) this._voiceDebounce)
      return;
    if (this.Team1Players.Count <= this.TargetTeamSize)
    {
      if (this.Team2Players.Count <= this.TargetTeamSize)
      {
        if (this.Team1Players.Count + this.Team2Players.Count + this.Spectators.Count >= this.TargetTeamSize * 2)
        {
          if (this.Spectators.Count <= 0)
            return;
          int num1 = this.TargetTeamSize - this.Team1Players.Count;
          int num2 = this.TargetTeamSize - this.Team2Players.Count;
          if ((num1 > 0 ? 1 : (num2 > 0 ? 1 : 0)) == 0)
            return;
          int num3 = Mathf.Min(num1 + num2, this.Spectators.Count);
          string str = "";
          for (int index = 0; index < num3; ++index)
            str = $"{str}{this.Spectators[index].NickName}, ";
          AutoRefManager.Speak(str + " please use a matching color code with your team");
          this._voiceDebounce = Time.time + 15f;
        }
        else
        {
          AutoRefManager.Speak("Waiting for players.");
          this._voiceDebounce = Time.time + 30f;
        }
      }
      else
      {
        AutoRefManager.Speak(this.Team2Players.Last<NetPlayer>().NickName + ", your team is full. Please change your color.");
        this._voiceDebounce = Time.time + 10f;
      }
    }
    else
    {
      AutoRefManager.Speak(this.Team1Players.Last<NetPlayer>().NickName + ", your team is full. Please change your color.");
      this._voiceDebounce = Time.time + 10f;
    }
  }

  private bool IsColorClose(Color a, Color b, float tolerance)
  {
    return (double) Mathf.Abs(a.r - b.r) + (double) Mathf.Abs(a.g - b.g) + (double) Mathf.Abs(a.b - b.b) < (double) tolerance;
  }

  private void UpdateTeamsByRoster()
  {
    this.Team1Players.Clear();
    this.Team2Players.Clear();
    this.Spectators.Clear();
    foreach (KeyValuePair<NetPlayer, VRRig> keyValuePair in PlayerTranslator.VRRigDict)
    {
      NetPlayer key = keyValuePair.Key;
      VRRig vrRig = keyValuePair.Value;
      if ((key == null || ((UnityEngine.Object) vrRig == (UnityEngine.Object) null) || vrRig.isOfflineVRRig ? 1 : (vrRig.isMyPlayer ? 1 : 0)) == 0)
      {
        string userId = key.UserId;
        if ((string.IsNullOrEmpty(userId) ? 0 : (this._team1UserIds.Contains(userId) ? 1 : 0)) == 0)
        {
          if ((string.IsNullOrEmpty(userId) ? 0 : (this._team2UserIds.Contains(userId) ? 1 : 0)) == 0)
            this.Spectators.Add(key);
          else
            this.Team2Players.Add(key);
        }
        else
          this.Team1Players.Add(key);
      }
    }
  }

  private void LockRoster()
  {
    this._team1UserIds.Clear();
    this._team2UserIds.Clear();
    for (int index = 0; (index >= this.Team1Players.Count ? 0 : (index < this.TargetTeamSize ? 1 : 0)) != 0; ++index)
    {
      if (!string.IsNullOrEmpty(this.Team1Players[index].UserId))
        this._team1UserIds.Add(this.Team1Players[index].UserId);
    }
    for (int index = 0; (index >= this.Team2Players.Count ? 0 : (index < this.TargetTeamSize ? 1 : 0)) != 0; ++index)
    {
      if (!string.IsNullOrEmpty(this.Team2Players[index].UserId))
        this._team2UserIds.Add(this.Team2Players[index].UserId);
    }
    this._rosterLocked = this._team1UserIds.Count > 0 && this._team2UserIds.Count > 0;
    if (!this._rosterLocked)
      return;
    this.AreTeamsDetected = true;
  }

  public void ReLockTeams()
  {
    this._rosterLocked = false;
    this.UpdateTeamsByColor();
    this.LockRoster();
    Notification.Send("Teams re-detected and re-locked.", Color.green);
  }

  private void UnlockRoster()
  {
    this._rosterLocked = false;
    this._team1UserIds.Clear();
    this._team2UserIds.Clear();
  }

  private void RunStateMachine()
  {
    switch (this.currentState)
    {
      case AutoRefManager.GameStates.PreGame:
        this.HandlePreGame();
        break;
      case AutoRefManager.GameStates.RoundStart:
        if (this.currentGameMode != AutoRefManager.GameModes.Gtc)
        {
          this.HandleRoundStart_CGT();
          break;
        }
        this.HandleRoundStart_GTC();
        break;
      case AutoRefManager.GameStates.Countdown:
        this.HandleCountdown();
        break;
      case AutoRefManager.GameStates.MidRound:
        if (this.currentGameMode != AutoRefManager.GameModes.Gtc)
        {
          this.HandleMidRound_CGT();
          break;
        }
        this.HandleMidRound_GTC();
        break;
      case AutoRefManager.GameStates.RoundEnd:
        this.HandleRoundEnd();
        break;
      case AutoRefManager.GameStates.MatchOver:
        this.HandleMatchOver();
        break;
      case AutoRefManager.GameStates.SubWait:
        this.HandleSubWait();
        break;
      case AutoRefManager.GameStates.RefDecision:
        this.HandleRefDecision();
        break;
    }
  }

  private void HandlePreGame()
  {
    this.statusLine = "Waiting for Reset";
    if (this.StumpTrigger != null)
      this.StumpTrigger.ShouldTag = false;
    if (this._tagManager.currentInfected.Count <= 3)
    {
      if ((!this._rosterLocked || this._playUneven ? 0 : (this.IsAnyTeamShort() ? 1 : 0)) != 0)
        this.BeginSubFlow();
      else if ((this._playUneven ? (this.Team1Players.Count <= 0 ? 0 : (this.Team2Players.Count > 0 ? 1 : 0)) : (!this.AreTeamsDetected || this.Team1Players.Count < this.TargetTeamSize ? 0 : (this.Team2Players.Count >= this.TargetTeamSize ? 1 : 0))) == 0)
      {
        this.instructionLine = $"Waiting for teams ({this.TargetTeamSize}v{this.TargetTeamSize})...";
        if (((double) Time.time <= (double) this._voiceDebounce + 2.0 ? 0 : (this.AreTeamsDetected ? 1 : 0)) == 0)
          return;
        AutoRefManager.Speak("Waiting for full teams.");
        this._voiceDebounce = Time.time + 10f;
      }
      else if (!AutoRefManager.VoiceBusy)
      {
        this.instructionLine = "Teams get ready. Preparing...";
        if (!this._spokenIntro)
        {
          if (!this._hasAnnouncedMatch)
          {
            AutoRefManager.Speak($"Teams identified. {this.TargetTeamSize} versus {this.TargetTeamSize}. Match initiating.");
            this._hasAnnouncedMatch = true;
          }
          else
            AutoRefManager.Speak("Teams get ready. Round starting.");
          this._spokenIntro = true;
        }
        this._stateTimer -= Time.deltaTime;
        if ((double) this._stateTimer > 0.0)
          return;
        if (!this._rosterLocked)
          this.LockRoster();
        this.TransitionTo(AutoRefManager.GameStates.RoundStart);
      }
      else
        this.instructionLine = "Waiting for audio to clear...";
    }
    else
      this.instructionLine = "Waiting for Game Reset... (Tag last survivor!)";
  }

  private void HandleCountdown()
  {
    if (this.StumpTrigger != null)
      this.StumpTrigger.ShouldTag = false;
    this.instructionLine = "Get set...";
    this._stateTimer -= Time.deltaTime;
    if (((double) this._stateTimer > 2.0 ? 0 : (!this._spokenCount2 ? 1 : 0)) != 0)
    {
      AutoRefManager.Speak("Three");
      this._spokenCount2 = true;
    }
    if (((double) this._stateTimer > 1.2999999523162842 ? 0 : (!this._spokenCount1 ? 1 : 0)) != 0)
    {
      AutoRefManager.Speak("Two");
      this._spokenCount1 = true;
    }
    if (((double) this._stateTimer > 0.60000002384185791 ? 0 : (!this._spokenCount3 ? 1 : 0)) != 0)
    {
      AutoRefManager.Speak("One");
      this._spokenCount3 = true;
    }
    foreach (NetPlayer rosteredPlayer in this.RosteredPlayers())
    {
      if (!this.CheckIfPlayerInGazebo(rosteredPlayer))
        this.PenalizeJumpStart(rosteredPlayer);
    }
    if ((double) this._stateTimer > 0.0)
      return;
    AutoRefManager.Speak("Taggers, tag!");
    this.instructionLine = "Go!";
    if ((double) this._gtcMatchStartTime <= 0.0)
      this._gtcMatchStartTime = Time.time;
    this._gtcRoundStartTime = Time.time;
    this.TransitionTo(AutoRefManager.GameStates.MidRound);
  }

  private void HandleRoundEnd()
  {
    this.statusLine = "Round Over";
    this.instructionLine = "AUTO-RESETTING... (Tagging Everyone)";
    if (this.StumpTrigger != null)
      this.StumpTrigger.ShouldTag = false;
    this.TagEveryone();
    this._stateTimer -= Time.deltaTime;
    if ((double) this._stateTimer > 0.0)
      return;
    this.TransitionTo(AutoRefManager.GameStates.PreGame);
  }

  private void HandleMatchOver()
  {
    this.statusLine = "MATCH OVER";
    this.instructionLine = "Press 'Full Reset' to start new match.";
    if (this.StumpTrigger != null)
      this.StumpTrigger.ShouldTag = false;
    this.TagEveryone();
  }

  private bool IsAnyTeamShort()
  {
    return this.Team1Players.Count < this.TargetTeamSize || this.Team2Players.Count < this.TargetTeamSize;
  }

  private int GetShortTeam() => this.Team1Players.Count > this.Team2Players.Count ? 2 : 1;

  private void BeginSubFlow()
  {
    if (this._subBreaksUsed < 3)
    {
      ++this._subBreaksUsed;
      this._subWaitUntil = Time.time + 180f;
      this._subVoiceDebounce = 0.0f;
      this.currentState = AutoRefManager.GameStates.SubWait;
      AutoRefManager.Speak($"{this.TeamName(this.GetShortTeam())} is a player short. Waiting up to three minutes for a substitute. {$"Break {this._subBreaksUsed} of {3}."}");
    }
    else
    {
      AutoRefManager.Speak("No substitution breaks remaining. The human referee is deciding what to do next.");
      this.EnterRefDecision();
    }
  }

  private void HandleSubWait()
  {
    this.statusLine = "Waiting for Substitute";
    if (this.StumpTrigger != null)
      this.StumpTrigger.ShouldTag = false;
    this.TryFillSubs();
    if (!this.IsAnyTeamShort())
    {
      AutoRefManager.Speak("Substitute in. Resuming.");
      this.TransitionTo(AutoRefManager.GameStates.PreGame);
    }
    else
    {
      float num = this._subWaitUntil - Time.time;
      if ((double) num <= 0.0)
      {
        this.EnterRefDecision();
      }
      else
      {
        this.instructionLine = $"Waiting for sub - {Mathf.CeilToInt(num)}s left (break {this._subBreaksUsed}/{3})";
        if ((double) Time.time <= (double) this._subVoiceDebounce)
          return;
        AutoRefManager.Speak(this.TeamName(this.GetShortTeam()) + " still needs a substitute.");
        this._subVoiceDebounce = Time.time + 45f;
      }
    }
  }

  private string RefOptionsLine()
  {
    return this.currentGameMode != AutoRefManager.GameModes.Gtc ? "More Time / Forfeit / Smaller Teams / Continue Uneven" : "Give 5 min, or Forfeit (GTC: no uneven teams)";
  }

  private void EnterRefDecision()
  {
    this.currentState = AutoRefManager.GameStates.RefDecision;
    this.statusLine = "Referee Deciding";
    this.instructionLine = "Referee: " + this.RefOptionsLine();
    AutoRefManager.Speak("The human referee is deciding what to do next.");
  }

  private void HandleRefDecision()
  {
    this.statusLine = "Referee Deciding";
    if (this.StumpTrigger != null)
      this.StumpTrigger.ShouldTag = false;
    this.instructionLine = "Awaiting referee: " + this.RefOptionsLine();
  }

  private void TryFillSubs()
  {
    if (this.Team1Players.Count < this.TargetTeamSize)
      this.TryFillSub(1);
    if (this.Team2Players.Count >= this.TargetTeamSize)
      return;
    this.TryFillSub(2);
  }

  private bool TryFillSub(int team)
  {
    HashSet<string> stringSet = team == 1 ? this._team1UserIds : this._team2UserIds;
    Color b = team == 1 ? this.Team1Color : this.Team2Color;
    bool flag;
    foreach (NetPlayer spectator in this.Spectators)
    {
      if ((spectator == null ? 1 : (string.IsNullOrEmpty(spectator.UserId) ? 1 : 0)) == 0 && !stringSet.Contains(spectator.UserId))
      {
        VRRig playerVrRig = ((UnityEngine.Object) this._tagManager != (UnityEngine.Object) null) ? ((GorillaGameManager) this._tagManager).FindPlayerVRRig(spectator) : (VRRig) null;
        if (!((UnityEngine.Object) playerVrRig == (UnityEngine.Object) null) && this.IsColorClose(playerVrRig.playerColor, b, 0.3f))
        {
          stringSet.Add(spectator.UserId);
          AutoRefManager.Speak($"Substitute added to {this.TeamName(team)}.");
          flag = true;
          goto label_8;
        }
      }
    }
    flag = false;
label_8:
    return flag;
  }

  public bool IsAwaitingRefDecision => this.currentState == AutoRefManager.GameStates.RefDecision;

  public void RefGiveMoreTime()
  {
    if (this.currentState != AutoRefManager.GameStates.RefDecision)
      return;
    this._subWaitUntil = Time.time + 300f;
    this._subVoiceDebounce = 0.0f;
    this.currentState = AutoRefManager.GameStates.SubWait;
    AutoRefManager.Speak("Referee granted five more minutes. Waiting for a substitute.");
  }

  public void RefForfeitShortTeam()
  {
    if (this.currentState != AutoRefManager.GameStates.RefDecision)
      return;
    int shortTeam = this.GetShortTeam();
    int team = shortTeam == 1 ? 2 : 1;
    this._matchIsOver = true;
    this.currentState = AutoRefManager.GameStates.MatchOver;
    this.UnlockRoster();
    AutoRefManager.Speak($"{this.TeamName(shortTeam)} forfeits. {this.TeamName(team)} wins the match.");
  }

  public void RefReduceTeamSize()
  {
    if (this.currentState != AutoRefManager.GameStates.RefDecision || this.currentGameMode != AutoRefManager.GameModes.Cgt)
      return;
    int num = Mathf.Max(1, Mathf.Min(this.Team1Players.Count, this.Team2Players.Count));
    this.TargetTeamSize = num;
    this.ReLockTeams();
    this._playUneven = false;
    AutoRefManager.Speak($"Switching to {num} versus {num}. Resuming.");
    this.TransitionTo(AutoRefManager.GameStates.PreGame);
  }

  public void RefContinueUneven()
  {
    if (this.currentState != AutoRefManager.GameStates.RefDecision || this.currentGameMode != AutoRefManager.GameModes.Cgt)
      return;
    this._playUneven = true;
    AutoRefManager.Speak($"Playing on with uneven teams. {this.Team1Players.Count} versus {this.Team2Players.Count}.");
    this.TransitionTo(AutoRefManager.GameStates.PreGame);
  }

  private void HandleRoundStart_GTC()
  {
    this.statusLine = "Select Taggers (Step into Gazebo)";
    if (!this._spokenIntro)
    {
      AutoRefManager.Speak($"Someone on {this.TeamName(1)}, and someone on {this.TeamName(2)}, step into the gazebo to volunteer as taggers.");
      this._spokenIntro = true;
    }
    bool flag1 = this.Team1Players.Any<NetPlayer>((Func<NetPlayer, bool>) (p => this.IsInfected(p)));
    bool flag2 = this.Team2Players.Any<NetPlayer>((Func<NetPlayer, bool>) (p => this.IsInfected(p)));
    if (!flag1)
    {
      NetPlayer player = this.Team1Players.FirstOrDefault<NetPlayer>((Func<NetPlayer, bool>) (p => this.CheckIfPlayerInGazebo(p) && !this.IsInfected(p)));
      if (player == null)
      {
        if (((double) this._selectionPhaseTimer > 0.0 ? 0 : (this.Team1Players.Count > 0 ? 1 : 0)) != 0)
          this.TryTagPlayer(this.Team1Players[UnityEngine.Random.Range(0, this.Team1Players.Count)]);
      }
      else
        this.TryTagPlayer(player);
    }
    if (!flag2)
    {
      NetPlayer player = this.Team2Players.FirstOrDefault<NetPlayer>((Func<NetPlayer, bool>) (p => this.CheckIfPlayerInGazebo(p) && !this.IsInfected(p)));
      if (player == null)
      {
        if (((double) this._selectionPhaseTimer > 0.0 ? 0 : (this.Team2Players.Count > 0 ? 1 : 0)) != 0)
          this.TryTagPlayer(this.Team2Players[UnityEngine.Random.Range(0, this.Team2Players.Count)]);
      }
      else
        this.TryTagPlayer(player);
    }
    if ((!flag1 ? 1 : (!flag2 ? 1 : 0)) != 0)
    {
      this._selectionPhaseTimer -= Time.deltaTime;
      this.instructionLine = $"Step on Table to start! (Auto-pick in {this._selectionPhaseTimer:F0}s)";
    }
    else
    {
      if (!this._spokenCount1)
      {
        AutoRefManager.Speak("Taggers selected. Taggers, get to the gazebo. Runners, run when you hear taggers tag.");
        this._spokenCount1 = true;
      }
      bool flag3 = false;
      bool flag4 = false;
      foreach (NetPlayer player in this._tagManager.currentInfected)
      {
        if ((!this.Team1Players.Contains(player) ? 0 : (this.CheckIfPlayerInGazebo(player) ? 1 : 0)) != 0)
          flag3 = true;
        if ((!this.Team2Players.Contains(player) ? 0 : (this.CheckIfPlayerInGazebo(player) ? 1 : 0)) != 0)
          flag4 = true;
      }
      if ((double) this._readyStartTime == 0.0)
        this._readyStartTime = Time.time;
      bool flag5 = flag3 & flag4;
      bool flag6 = (double) Time.time - (double) this._readyStartTime >= 10.0;
      if ((flag5 ? 0 : (!flag6 ? 1 : 0)) == 0)
        this.TransitionTo(AutoRefManager.GameStates.Countdown);
      else
        this.instructionLine = $"Taggers to the gazebo... ({this.TeamName(1)}: {(flag3 ? "OK" : "...")} | {this.TeamName(2)}: {(flag4 ? "OK" : "...")})";
    }
  }

  private void HandleMidRound_GTC()
  {
    this.statusLine = "Round Live";
    if (this.StumpTrigger != null)
      this.StumpTrigger.ShouldTag = true;
    int num1 = this.Team1Players.Count<NetPlayer>((Func<NetPlayer, bool>) (p => !this.IsInfected(p)));
    int num2 = this.Team2Players.Count<NetPlayer>((Func<NetPlayer, bool>) (p => !this.IsInfected(p)));
    this.instructionLine = $"{this.TeamName(1)} alive: {num1} | {this.TeamName(2)} alive: {num2}";
    if (num1 != 0)
    {
      if (num2 != 0)
        return;
      this.EndGtcRound(1);
    }
    else
      this.EndGtcRound(2);
  }

  private void EndGtcRound(int winningTeam)
  {
    if (winningTeam == 1)
      ++this.team1Score;
    else
      ++this.team2Score;
    this._lastScoringTeam = winningTeam;
    string str = this.TeamName(winningTeam);
    this.instructionLine = $"Round over - {str} wins!";
    AutoRefManager.Speak($"Round over. {str} wins the round.");
    if ((double) this._gtcRoundStartTime > 0.0)
      this._refOldClock = Time.time - this._gtcRoundStartTime;
    this._gtcRoundStartTime = -1f;
    this.TransitionTo(AutoRefManager.GameStates.RoundEnd);
  }

  private void EndMatchGTC()
  {
    this._matchIsOver = true;
    this.currentState = AutoRefManager.GameStates.MatchOver;
    this.UnlockRoster();
    string str1;
    string str2;
    if (this.team1Score <= this.team2Score)
    {
      if (this.team2Score > this.team1Score)
      {
        str1 = this.TeamName(2);
        str2 = this.GetTeamNameString(this.Team2Players);
      }
      else
      {
        str1 = "It's a draw";
        str2 = "Everyone";
      }
    }
    else
    {
      str1 = this.TeamName(1);
      str2 = this.GetTeamNameString(this.Team1Players);
    }
    AutoRefManager.Speak($"Time's up! Good game! {str1} won the match! {str2}, congratulations.");
  }

  public void ForceFixCGTInfection()
  {
    List<NetPlayer> netPlayerList1 = this._cgtTeam1IsChasing ? this.Team1Players : this.Team2Players;
    List<NetPlayer> netPlayerList2 = this._cgtTeam1IsChasing ? this.Team2Players : this.Team1Players;
    foreach (NetPlayer netPlayer in netPlayerList1)
    {
      if (!this.IsInfected(netPlayer))
        this.TryTagPlayer(netPlayer);
    }
    foreach (NetPlayer netPlayer in netPlayerList2)
    {
      if (this.IsInfected(netPlayer))
        this.TryUntagPlayer(netPlayer);
    }
  }

  private void HandleRoundStart_CGT()
  {
    List<NetPlayer> first = this._cgtTeam1IsChasing ? this.Team1Players : this.Team2Players;
    List<NetPlayer> second = this._cgtTeam1IsChasing ? this.Team2Players : this.Team1Players;
    string str = this.TeamName(this._cgtTeam1IsChasing ? 1 : 2);
    this.statusLine = $"Heat Setup: {str} chasing";
    if (!this._spokenIntro)
    {
      AutoRefManager.Speak(str + " are the chasers. Everyone to the gazebo.");
      this._spokenIntro = true;
    }
    bool flag = true;
    foreach (NetPlayer netPlayer in first)
    {
      if (!this.IsInfected(netPlayer))
      {
        this.TryTagPlayer(netPlayer);
        flag = false;
      }
    }
    foreach (NetPlayer netPlayer in second)
    {
      if (this.IsInfected(netPlayer))
      {
        this.TryUntagPlayer(netPlayer);
        flag = false;
      }
    }
    if (!flag)
    {
      this.instructionLine = "Adjusting Infection States...";
    }
    else
    {
      if ((double) this._readyStartTime == 0.0)
      {
        List<NetPlayer> list = first.Concat<NetPlayer>((IEnumerable<NetPlayer>) second).Where<NetPlayer>((Func<NetPlayer, bool>) (p => p != null && !this.CheckIfPlayerInGazebo(p))).ToList<NetPlayer>();
        if (list.Count > 0)
        {
          this.instructionLine = list.Count == 1 ? $"Waiting for {list[0].NickName} to get under gazebo..." : "Waiting for EVERYONE to be in Gazebo...";
          if ((double) Time.time <= (double) this._voiceDebounce)
            return;
          AutoRefManager.Speak(list.Count == 1 ? list[0].NickName + ", get under the gazebo." : "All players, get under gazebo, away from edges.");
          this._voiceDebounce = Time.time + 6f;
          return;
        }
        this._readyStartTime = Time.time;
      }
      float num1 = Time.time - this._readyStartTime;
      float num2 = 3.5f;
      if ((double) num1 < (double) num2)
      {
        this.instructionLine = "Counting down Runners...";
        if (((double) num1 <= 0.5 ? 0 : (!this._spokenCount3 ? 1 : 0)) != 0)
        {
          AutoRefManager.Speak("Three");
          this._spokenCount3 = true;
        }
        if (((double) num1 <= 1.2000000476837158 ? 0 : (!this._spokenCount2 ? 1 : 0)) != 0)
        {
          AutoRefManager.Speak("Two");
          this._spokenCount2 = true;
        }
        if (((double) num1 <= 1.8999999761581421 ? 0 : (!this._spokenCount1 ? 1 : 0)) != 0)
        {
          AutoRefManager.Speak("One");
          this._spokenCount1 = true;
        }
        foreach (NetPlayer netPlayer in first.Concat<NetPlayer>((IEnumerable<NetPlayer>) second))
        {
          if (!this.CheckIfPlayerInGazebo(netPlayer))
            this.PenalizeJumpStart(netPlayer);
        }
      }
      else
      {
        if (!this._spokenRunnersRun)
        {
          AutoRefManager.Speak("Runners, run!");
          this._spokenRunnersRun = true;
          this._waitingForRunnersAudio = true;
        }
        if (!this._waitingForRunnersAudio)
        {
          float num3 = this._taggersReleaseTimestamp - Time.time;
          if ((double) num3 > 0.0)
          {
            this.instructionLine = $"Runners Running! Taggers Released in {num3:F1}s";
            if (((double) num3 >= 2.2000000476837158 ? 0 : (!this._spokenGo ? 1 : 0)) != 0)
            {
              AutoRefManager.Speak("Three");
              this._spokenGo = true;
            }
            if (((double) num3 >= 1.5 ? 0 : (!this._spoken30SecWarn ? 1 : 0)) != 0)
            {
              AutoRefManager.Speak("Two");
              this._spoken30SecWarn = true;
            }
            if (((double) num3 >= 0.800000011920929 ? 0 : (!this._spoken10SecWarn ? 1 : 0)) != 0)
            {
              AutoRefManager.Speak("One");
              this._spoken10SecWarn = true;
            }
            foreach (NetPlayer netPlayer in first)
            {
              if (!this.CheckIfPlayerInGazebo(netPlayer))
                this.PenalizeJumpStart(netPlayer);
            }
          }
          else
          {
            if (!this._spokenTaggersTag)
            {
              AutoRefManager.Speak("Taggers, tag!");
              this._spokenTaggersTag = true;
              this._postTagDelay = Time.time + 1f;
            }
            if ((double) Time.time <= (double) this._postTagDelay)
              return;
            this.TransitionTo(AutoRefManager.GameStates.MidRound);
          }
        }
        else
        {
          this.instructionLine = "Waiting for 'Runners Run' audio to finish...";
          if (AutoRefManager.VoiceBusy)
            return;
          this._waitingForRunnersAudio = false;
          this._taggersReleaseTimestamp = Time.time + 10f;
        }
      }
    }
  }

  private void HandleMidRound_CGT()
  {
    this.statusLine = "Heat In Progress";
    this._cgtHeatTimer += Time.deltaTime;
    this.instructionLine = $"Time: {this._cgtHeatTimer:F1} / {(ValueType) 180f}";
    float num = 180f - this._cgtHeatTimer;
    if (((double) num > 30.0 ? 0 : (!this._spoken30SecWarn ? 1 : 0)) != 0)
    {
      AutoRefManager.Speak("30 seconds remaining.");
      this._spoken30SecWarn = true;
    }
    if (((double) num > 10.0 ? 0 : (!this._spoken10SecWarn ? 1 : 0)) != 0)
    {
      AutoRefManager.Speak("10 seconds!");
      this._spoken10SecWarn = true;
    }
    if (this.StumpTrigger != null)
      this.StumpTrigger.ShouldTag = true;
    if (((this._cgtTeam1IsChasing ? (IEnumerable<NetPlayer>) this.Team2Players : (IEnumerable<NetPlayer>) this.Team1Players).Count<NetPlayer>((Func<NetPlayer, bool>) (p => !this.IsInfected(p))) == 0 ? 1 : ((double) this._cgtHeatTimer >= 180.0 ? 1 : 0)) == 0)
      return;
    float totalSeconds = (double) this._cgtHeatTimer >= 180.0 ? 180f : this._cgtHeatTimer;
    this._refOldClock = totalSeconds;
    string str = AutoRefManager.SpokenTime(totalSeconds);
    if ((double) this._cgtHeatTimer >= 180.0)
    {
      this.TagEveryone();
      AutoRefManager.Speak("Time cap.");
    }
    else
      AutoRefManager.Speak($"Round over. Time: {str}.");
    if (!this._cgtTeam1IsChasing)
      this._cgtTeam1SurvivalTime = totalSeconds;
    else
      this._cgtTeam2SurvivalTime = totalSeconds;
    if (this._cgtIsSecondHeat)
    {
      this.CalculateCGTWinner();
    }
    else
    {
      this._cgtIsSecondHeat = true;
      this._cgtTeam1IsChasing = !this._cgtTeam1IsChasing;
      this.TransitionTo(AutoRefManager.GameStates.PreGame);
    }
  }

  private void CalculateCGTWinner()
  {
    float num = Mathf.Abs(this._cgtTeam1SurvivalTime - this._cgtTeam2SurvivalTime);
    if (((double) this._cgtTeam1SurvivalTime < 180.0 ? 0 : ((double) this._cgtTeam2SurvivalTime >= 180.0 ? 1 : 0)) == 0)
    {
      if ((double) num > 2.0)
      {
        if ((double) this._cgtTeam1SurvivalTime <= (double) this._cgtTeam2SurvivalTime)
        {
          ++this.team2Score;
          this._lastScoringTeam = 2;
          AutoRefManager.Speak($"{this.TeamName(2)} wins the round. Score {this.team2Score} to {this.team1Score}.");
        }
        else
        {
          ++this.team1Score;
          this._lastScoringTeam = 1;
          AutoRefManager.Speak($"{this.TeamName(1)} wins the round. Score {this.team1Score} to {this.team2Score}.");
        }
      }
      else
        AutoRefManager.Speak($"Draw. Times too close. Score {this.team1Score} to {this.team2Score}.");
    }
    else
      AutoRefManager.Speak($"Draw. Score {this.team1Score} to {this.team2Score}.");
    if ((this.team1Score >= this.CgtScoreToWin ? 1 : (this.team2Score >= this.CgtScoreToWin ? 1 : 0)) != 0)
    {
      this.EndMatchCGT(this.team1Score >= this.CgtScoreToWin);
    }
    else
    {
      this._cgtIsSecondHeat = false;
      this._cgtTeam1SurvivalTime = 0.0f;
      this._cgtTeam2SurvivalTime = 0.0f;
      this._cgtTeam1IsChasing = true;
      this.TransitionTo(AutoRefManager.GameStates.RoundEnd);
    }
  }

  private void EndMatchCGT(bool team1Won)
  {
    this._matchIsOver = true;
    this.currentState = AutoRefManager.GameStates.MatchOver;
    this.UnlockRoster();
    AutoRefManager.Speak($"Good game! {(team1Won ? this.TeamName(1) : this.TeamName(2))} won the match! {(team1Won ? this.GetTeamNameString(this.Team1Players) : this.GetTeamNameString(this.Team2Players))}, congratulations.");
  }

  private string GetTeamNameString(List<NetPlayer> players)
  {
    return string.Join(", ", players.Take<NetPlayer>(this.TargetTeamSize).Select<NetPlayer, string>((Func<NetPlayer, string>) (p => p.NickName)));
  }

  private void TransitionTo(AutoRefManager.GameStates newState)
  {
    this.currentState = newState;
    this._spokenIntro = false;
    this._spokenCount3 = false;
    this._spokenCount2 = false;
    this._spokenCount1 = false;
    this._spokenGo = false;
    this._spokenRunnersRun = false;
    this._spokenTaggersTag = false;
    this._spoken30SecWarn = false;
    this._spoken10SecWarn = false;
    this._lastT1Alive = -1;
    this._lastT2Alive = -1;
    this._waitingForRunnersAudio = false;
    this._taggersReleaseTimestamp = 0.0f;
    this._postTagDelay = 0.0f;
    if (newState == AutoRefManager.GameStates.RoundEnd)
      this._stateTimer = 5f;
    if (newState == AutoRefManager.GameStates.Countdown)
      this._stateTimer = 3f;
    if (newState == AutoRefManager.GameStates.PreGame)
      this._stateTimer = 5f;
    if (newState != AutoRefManager.GameStates.RoundStart)
      return;
    this._stallStartTime = 0.0f;
    this._readyStartTime = 0.0f;
    this._selectionPhaseTimer = 30f;
    if (this.currentGameMode != AutoRefManager.GameModes.Cgt)
      return;
    this._cgtHeatTimer = 0.0f;
  }

  public enum GameModes
  {
    Gtc,
    Cgt,
  }

  public enum GameStates
  {
    PreGame,
    RoundStart,
    Countdown,
    MidRound,
    RoundEnd,
    None,
    MatchOver,
    SubWait,
    RefDecision,
  }

  private class ColorCluster
  {
    private Color _sumColor;
    public int Count;
    public List<NetPlayer> Members = new List<NetPlayer>();

    public Color AverageColor => (this._sumColor / (float) this.Count);

    public ColorCluster(Color start, NetPlayer p)
    {
      this._sumColor = start;
      this.Count = 1;
      this.Members.Add(p);
    }

    public void Add(Color c, NetPlayer p)
    {
      this._sumColor = (this._sumColor + c);
      ++this.Count;
      this.Members.Add(p);
    }
  }
}
