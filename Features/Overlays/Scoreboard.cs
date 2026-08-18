using Photon.Realtime;
using SakuraaCastingMod.Features.AutoRef;
using SakuraaCastingMod.Features.Tools;
using SakuraaCastingMod.Shared.Helpers;
using SakuraaCastingMod.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;

#nullable disable
namespace SakuraaCastingMod.Features.Overlays;

public static class Scoreboard
{
  [SavedSetting("ShowScoreboard", false)]
  public static bool ShowScoreboard;
  [SavedSetting("CurrentScoreboard", 0)]
  public static int CurrentScoreboard;
  public static GameObject ScoreboardCanvasObj;
  public static Canvas ScoreboardCanvas;
  public static GameObject MainScoreboardObj;
  public static GameObject CherryScoreboardObj;
  public static GameObject PlayerScoreboardObj;
  public static TMP_Text Team1NameText;
  public static TMP_Text Team2NameText;
  public static TMP_Text Team1ScoreText;
  public static TMP_Text Team2ScoreText;
  public static TMP_Text CurrentTimerText;
  public static TMP_Text OldTimerText;
  private static bool _isTimerPaused = true;
  [SavedSetting("ScoreboardPauseOnRoundEnd", false)]
  public static bool PauseOnRoundEnd = true;
  internal static float _currentTimerTime = -10f;
  private static float _oldTime;
  public static string Team1Name = "TTT";
  public static string Team2Name = "TSO";
  private static float _team1Score;
  private static float _team2Score;
  [SavedSetting("ScoreboardTimerWait", true)]
  public static bool TimerWait = true;
  public static bool CountDown;
  [SavedSetting("ScoreboardPosX", 0.0f)]
  private static float _scorePosX;
  [SavedSetting("ScoreboardPosY", 0.0f)]
  private static float _scorePosY;
  [SavedSetting("ScoreboardScale", 1f)]
  internal static float ScoreScale = 1f;
  private static float _lastScoreboardUpdateTime;
  private static readonly Dictionary<GorillaData, Scoreboard.PlayerSbData> PlayerSbDataDict = new Dictionary<GorillaData, Scoreboard.PlayerSbData>();
  public static GameObject CgtScoreboardObj;
  private static readonly StringBuilder TimerStringBuilder = new StringBuilder(8);
  private static string _autoRefRosterSig;

  public static void SetAutoRefScores(int t1, int t2)
  {
    Scoreboard._team1Score = (float) t1;
    Scoreboard._team2Score = (float) t2;
  }

  public static void SetAutoRefTime(float seconds)
  {
    if (!Scoreboard.AutoRefControlled)
      return;
    Scoreboard._currentTimerTime = seconds;
  }

  public static void SetAutoRefOldTime(float seconds)
  {
    if ((!Scoreboard.AutoRefControlled ? 1 : ((double) seconds < 0.0 ? 1 : 0)) != 0)
      return;
    Scoreboard._oldTime = seconds;
  }

  public static bool AutoRefControlled
  {
    get
    {
      return ((UnityEngine.Object) AutoRefManager.Instance != (UnityEngine.Object) null) && AutoRefManager.Instance.IsActive;
    }
  }

  internal static Vector2 _scorePos
  {
    get => new Vector2(Scoreboard._scorePosX, Scoreboard._scorePosY);
    set
    {
      Scoreboard._scorePosX = value.x;
      Scoreboard._scorePosY = value.y;
    }
  }

  public static void TransformScoreboards()
  {
    if (!Scoreboard.ShowScoreboard)
      return;
    if (((UnityEngine.Object) Scoreboard.MainScoreboardObj != (UnityEngine.Object) null))
      Scoreboard.MainScoreboardObj.transform.position = (new Vector2(Scoreboard._scorePos.x, Scoreboard._scorePos.y));
    if (((UnityEngine.Object) Scoreboard.CherryScoreboardObj != (UnityEngine.Object) null))
      Scoreboard.CherryScoreboardObj.transform.position = (new Vector2(Scoreboard._scorePos.x, Scoreboard._scorePos.y));
    if (((UnityEngine.Object) Scoreboard.PlayerScoreboardObj != (UnityEngine.Object) null))
      Scoreboard.PlayerScoreboardObj.transform.position = (new Vector2(Scoreboard._scorePos.x, Scoreboard._scorePos.y));
    if (((UnityEngine.Object) Scoreboard.CgtScoreboardObj != (UnityEngine.Object) null))
      Scoreboard.CgtScoreboardObj.transform.position = (new Vector2(Scoreboard._scorePos.x, Scoreboard._scorePos.y));
    if (((UnityEngine.Object) Scoreboard.MainScoreboardObj != (UnityEngine.Object) null))
      Scoreboard.MainScoreboardObj.transform.localScale = (new Vector2(10f * Scoreboard.ScoreScale, Scoreboard.ScoreScale));
    if (((UnityEngine.Object) Scoreboard.CherryScoreboardObj != (UnityEngine.Object) null))
      Scoreboard.CherryScoreboardObj.transform.localScale = (new Vector2(11f * Scoreboard.ScoreScale, 11f * Scoreboard.ScoreScale));
    if (((UnityEngine.Object) Scoreboard.PlayerScoreboardObj != (UnityEngine.Object) null))
      Scoreboard.PlayerScoreboardObj.transform.localScale = (new Vector2(Scoreboard.ScoreScale, Scoreboard.ScoreScale));
    if (!((UnityEngine.Object) Scoreboard.CgtScoreboardObj != (UnityEngine.Object) null))
      return;
    Scoreboard.CgtScoreboardObj.transform.localScale = (new Vector2(Scoreboard.ScoreScale, Scoreboard.ScoreScale));
  }

  public static void AddTagScore(Player tagger, Player tagged)
  {
    if ((Scoreboard._isTimerPaused || Scoreboard.CurrentScoreboard != 2 ? 0 : (tagger != null ? 1 : 0)) == 0)
      return;
    NetPlayer netPlayerByPhoton = PlayerTranslator.GetNetPlayerByPhoton(tagger);
    if (netPlayerByPhoton == null)
      return;
    GorillaData gorillaByNetPlayer = PlayerTranslator.GetGorillaByNetPlayer(netPlayerByPhoton);
    Scoreboard.PlayerSbData playerSbData;
    if (gorillaByNetPlayer == null || !Scoreboard.PlayerSbDataDict.TryGetValue(gorillaByNetPlayer, out playerSbData))
      return;
    if ((double) Time.time - (double) playerSbData.LastTagReq >= 2.0)
      playerSbData.Score += 30f;
    playerSbData.LastTagReq = Time.time;
  }

  public static void UpdateScoreboard()
  {
    bool flag1 = Scoreboard.ShowScoreboard && Scoreboard.CurrentScoreboard == 0;
    bool flag2 = Scoreboard.ShowScoreboard && Scoreboard.CurrentScoreboard == 1;
    bool flag3 = Scoreboard.ShowScoreboard && Scoreboard.CurrentScoreboard == 2;
    bool flag4 = Scoreboard.ShowScoreboard && Scoreboard.CurrentScoreboard == 3;
    if ((!((UnityEngine.Object) Scoreboard.MainScoreboardObj != (UnityEngine.Object) null) ? 0 : (Scoreboard.MainScoreboardObj.activeSelf != flag1 ? 1 : 0)) != 0)
      Scoreboard.MainScoreboardObj.SetActive(flag1);
    if ((!((UnityEngine.Object) Scoreboard.CherryScoreboardObj != (UnityEngine.Object) null) ? 0 : (Scoreboard.CherryScoreboardObj.activeSelf != flag2 ? 1 : 0)) != 0)
      Scoreboard.CherryScoreboardObj.SetActive(flag2);
    if ((!((UnityEngine.Object) Scoreboard.PlayerScoreboardObj != (UnityEngine.Object) null) ? 0 : (Scoreboard.PlayerScoreboardObj.activeSelf != flag3 ? 1 : 0)) != 0)
      Scoreboard.PlayerScoreboardObj.SetActive(flag3);
    if ((!((UnityEngine.Object) Scoreboard.CgtScoreboardObj != (UnityEngine.Object) null) ? 0 : (Scoreboard.CgtScoreboardObj.activeSelf != flag4 ? 1 : 0)) != 0)
      Scoreboard.CgtScoreboardObj.SetActive(flag4);
    if (!Scoreboard.ShowScoreboard)
      return;
    if ((Scoreboard.CurrentScoreboard != 2 ? 0 : (Scoreboard.AutoRefControlled ? 1 : 0)) != 0)
    {
      string str = Scoreboard.BuildAutoRefRosterSig();
      if (str != Scoreboard._autoRefRosterSig)
      {
        Scoreboard._autoRefRosterSig = str;
        Scoreboard.PopulateFromAutoRef();
      }
    }
    else
      Scoreboard._autoRefRosterSig = (string) null;
    if (((UnityEngine.Object) Scoreboard.CurrentTimerText != (UnityEngine.Object) null))
    {
      TimeSpan timeSpan = TimeSpan.FromSeconds((double) Scoreboard._currentTimerTime);
      Scoreboard.TimerStringBuilder.Clear();
      Scoreboard.TimerStringBuilder.AppendFormat("{0:00}:{1:00}", (object) timeSpan.Minutes, (object) timeSpan.Seconds);
      Scoreboard.CurrentTimerText.SetText(Scoreboard.TimerStringBuilder);
    }
    if (((UnityEngine.Object) Scoreboard.OldTimerText != (UnityEngine.Object) null))
    {
      TimeSpan timeSpan = TimeSpan.FromSeconds((double) Scoreboard._oldTime);
      Scoreboard.TimerStringBuilder.Clear();
      Scoreboard.TimerStringBuilder.AppendFormat("{0:00}:{1:00}", (object) timeSpan.Minutes, (object) timeSpan.Seconds);
      Scoreboard.OldTimerText.SetText(Scoreboard.TimerStringBuilder);
    }
    if (!Scoreboard._isTimerPaused)
    {
      bool flag5 = GorillaDataHandler.GorillaDataDict.Count > 0;
      bool flag6 = ((!Networking.InRoom ? 0 : (((UnityEngine.Object) Networking.GtagManager != (UnityEngine.Object) null) ? 1 : 0)) & (flag5 ? 1 : 0)) != 0 && Networking.GtagManager.currentInfected.Count == GorillaDataHandler.GorillaDataDict.Count;
      if (Scoreboard.PauseOnRoundEnd & flag6)
      {
        Scoreboard._isTimerPaused = true;
      }
      else
      {
        float unscaledDeltaTime = Time.unscaledDeltaTime;
        Scoreboard._currentTimerTime += Scoreboard.CountDown ? -unscaledDeltaTime : unscaledDeltaTime;
      }
    }
    Keyboard current = Keyboard.current;
    if ((current == null ? 0 : (!Scoreboard.AutoRefControlled ? 1 : 0)) != 0)
    {
      bool isPressed;
      if (((isPressed = ((ButtonControl) current.leftShiftKey).isPressed) ? 0 : (((ButtonControl) current[Keybinds.SbScoreDownKey]).wasPressedThisFrame ? 1 : 0)) != 0)
        --Scoreboard._team1Score;
      if ((isPressed ? 0 : (((ButtonControl) current[Keybinds.SbScoreUpKey]).wasPressedThisFrame ? 1 : 0)) != 0)
        ++Scoreboard._team1Score;
      if ((!isPressed ? 0 : (((ButtonControl) current[Keybinds.SbScoreDownKey]).wasPressedThisFrame ? 1 : 0)) != 0)
        --Scoreboard._team2Score;
      if ((!isPressed ? 0 : (((ButtonControl) current[Keybinds.SbScoreUpKey]).wasPressedThisFrame ? 1 : 0)) != 0)
        ++Scoreboard._team2Score;
      if (((ButtonControl) current[Keybinds.SbPauseKey]).wasPressedThisFrame)
        Scoreboard._isTimerPaused = !Scoreboard._isTimerPaused;
      if (((ButtonControl) current[Keybinds.SbResetTimerKey]).wasPressedThisFrame)
      {
        Scoreboard._isTimerPaused = true;
        Scoreboard._oldTime = Scoreboard._currentTimerTime;
        Scoreboard._currentTimerTime = Scoreboard.CountDown ? 1800f : (Scoreboard.TimerWait ? -10f : 0.0f);
      }
    }
    if (((UnityEngine.Object) Scoreboard.Team1NameText != (UnityEngine.Object) null))
      Scoreboard.Team1NameText.SetText(Scoreboard.Team1Name);
    if (((UnityEngine.Object) Scoreboard.Team2NameText != (UnityEngine.Object) null))
      Scoreboard.Team2NameText.SetText(Scoreboard.Team2Name);
    if (((UnityEngine.Object) Scoreboard.Team1ScoreText != (UnityEngine.Object) null))
      Scoreboard.Team1ScoreText.SetText(Scoreboard._team1Score.ToString());
    if (((UnityEngine.Object) Scoreboard.Team2ScoreText != (UnityEngine.Object) null))
      Scoreboard.Team2ScoreText.SetText(Scoreboard._team2Score.ToString());
    if ((Scoreboard.CountDown || (double) Scoreboard._currentTimerTime >= 0.0 ? 0 : ((double) Scoreboard._currentTimerTime >= -1.0 ? 1 : 0)) != 0)
      ++Scoreboard._currentTimerTime;
    if (((Scoreboard.AutoRefControlled ? (Scoreboard.AutoRefRoundLive() ? 1 : 0) : (!Scoreboard._isTimerPaused ? 1 : 0)) == 0 ? 1 : ((double) Scoreboard._currentTimerTime < 0.0 ? 1 : 0)) != 0)
      Scoreboard._lastScoreboardUpdateTime = Time.time;
    float num1 = Time.time - Scoreboard._lastScoreboardUpdateTime;
    if (((double) num1 < 2.0 ? 0 : (Scoreboard.CurrentScoreboard == 2 ? 1 : 0)) == 0)
      return;
    if (!Networking.InRoom)
      return;
    try
    {
      Scoreboard.PlayerSbData[] array = Scoreboard.PlayerSbDataDict.Values.ToArray<Scoreboard.PlayerSbData>();
      foreach (Scoreboard.PlayerSbData pd in array)
      {
        pd.CacheComponents();
        if ((pd.GorillaData == null ? 1 : (((UnityEngine.Object) pd.Card == (UnityEngine.Object) null) ? 1 : 0)) == 0 && (!Scoreboard.CheckPlayerScoreCards(pd) ? 1 : (!((Component) pd.Card).gameObject.activeInHierarchy ? 1 : 0)) == 0)
        {
          bool infected;
          if (!(infected = pd.GorillaData.Infected))
          {
            if (((UnityEngine.Object) pd.CachedGorillaImage != (UnityEngine.Object) null))
              ((Graphic) pd.CachedGorillaImage).color = pd.GorillaData.Color;
            pd.Score += num1;
          }
          if (((UnityEngine.Object) pd.CachedLavaMonkeObject != (UnityEngine.Object) null))
            pd.CachedLavaMonkeObject.SetActive(infected);
        }
      }
      float teamPoints1 = Scoreboard.GetTeamPoints(1);
      float teamPoints2 = Scoreboard.GetTeamPoints(2);
      foreach (Scoreboard.PlayerSbData playerSbData in array)
      {
        if ((playerSbData.GorillaData == null || ((UnityEngine.Object) playerSbData.Card == (UnityEngine.Object) null) ? 1 : (!((Component) playerSbData.Card).gameObject.activeInHierarchy ? 1 : 0)) == 0)
        {
          float num2 = playerSbData.Num <= 4 ? teamPoints1 : teamPoints2;
          string str = "0";
          if ((double) num2 > 0.0099999997764825821)
            str = Math.Max(0.0, Math.Round((double) playerSbData.Score / (double) num2 * 100.0)).ToString();
          if (((UnityEngine.Object) playerSbData.CachedScoreText != (UnityEngine.Object) null))
            ((TMP_Text) playerSbData.CachedScoreText).SetText(str);
          if (((UnityEngine.Object) playerSbData.CachedNameText != (UnityEngine.Object) null))
            ((TMP_Text) playerSbData.CachedNameText).SetText(playerSbData.GorillaData.UserName);
        }
      }
    }
    catch (Exception ex)
    {
      UnityEngine.Debug.LogError((object) $"Error updating player scoreboard: {ex}");
      Notification.Send("Error updating player scores.", Color.red);
    }
    Scoreboard._lastScoreboardUpdateTime = Time.time;
  }

  private static bool CheckPlayerScoreCards(Scoreboard.PlayerSbData pd)
  {
    bool flag;
    if (pd == null)
      flag = false;
    else if ((pd.GorillaData == null ? 0 : (GorillaDataHandler.GorillaDataDict.ContainsKey(pd.GorillaData.UserId) ? 1 : 0)) == 0)
    {
      Notification.Send("A player left, please pause the timer and update the player scoreboard", Color.red);
      if (((UnityEngine.Object) pd.Card != (UnityEngine.Object) null))
        ((Component) pd.Card).gameObject.SetActive(false);
      if (pd.GorillaData != null)
        Scoreboard.PlayerSbDataDict.Remove(pd.GorillaData);
      pd.GorillaData = (GorillaData) null;
      flag = false;
    }
    else
    {
      if (((UnityEngine.Object) pd.Card != (UnityEngine.Object) null))
        ((Component) pd.Card).gameObject.SetActive(true);
      flag = true;
    }
    return flag;
  }

  private static float GetTeamPoints(int givenTeamNum)
  {
    float teamPoints = 0.0f;
    try
    {
      foreach (Scoreboard.PlayerSbData playerSbData in Scoreboard.PlayerSbDataDict.Values.ToArray<Scoreboard.PlayerSbData>())
      {
        if ((playerSbData == null ? 1 : (playerSbData.GorillaData == null ? 1 : 0)) == 0 && (playerSbData.Num > 4 || givenTeamNum != 1 ? (playerSbData.Num <= 4 ? 0 : (givenTeamNum == 2 ? 1 : 0)) : 1) != 0)
          teamPoints += playerSbData.Score;
      }
    }
    catch (Exception ex)
    {
      UnityEngine.Debug.LogError((object) $"Error calculating team points: {ex}");
    }
    return teamPoints;
  }

  public static void ResetScoreboard()
  {
    try
    {
      switch (Scoreboard.CurrentScoreboard)
      {
        case 0:
          if (((UnityEngine.Object) Scoreboard.MainScoreboardObj == (UnityEngine.Object) null))
            break;
          Scoreboard.Team1NameText = (TMP_Text) ((Component) Scoreboard.MainScoreboardObj.transform.GetChild(0).Find("Team1Text"))?.GetComponent<TextMeshProUGUI>();
          Scoreboard.Team2NameText = (TMP_Text) ((Component) Scoreboard.MainScoreboardObj.transform.GetChild(0).Find("Team2Text"))?.GetComponent<TextMeshProUGUI>();
          Scoreboard.Team1ScoreText = (TMP_Text) ((Component) Scoreboard.MainScoreboardObj.transform.GetChild(0).Find("Score1"))?.GetComponent<TextMeshProUGUI>();
          Scoreboard.Team2ScoreText = (TMP_Text) ((Component) Scoreboard.MainScoreboardObj.transform.GetChild(0).Find("Score2"))?.GetComponent<TextMeshProUGUI>();
          Scoreboard.CurrentTimerText = (TMP_Text) ((Component) Scoreboard.MainScoreboardObj.transform.GetChild(0).Find("Timer"))?.GetComponent<TextMeshProUGUI>();
          Scoreboard.OldTimerText = (TMP_Text) ((Component) Scoreboard.MainScoreboardObj.transform.GetChild(0).Find("Timer (1)"))?.GetComponent<TextMeshProUGUI>();
          break;
        case 1:
          if (((UnityEngine.Object) Scoreboard.CherryScoreboardObj == (UnityEngine.Object) null))
            break;
          Scoreboard.Team1NameText = (TMP_Text) ((Component) Scoreboard.CherryScoreboardObj.transform.GetChild(0).Find("Team1Text"))?.GetComponent<TextMeshProUGUI>();
          Scoreboard.Team2NameText = (TMP_Text) ((Component) Scoreboard.CherryScoreboardObj.transform.GetChild(0).Find("Team2Text"))?.GetComponent<TextMeshProUGUI>();
          Scoreboard.Team1ScoreText = (TMP_Text) ((Component) Scoreboard.CherryScoreboardObj.transform.GetChild(0).Find("Score1"))?.GetComponent<TextMeshProUGUI>();
          Scoreboard.Team2ScoreText = (TMP_Text) ((Component) Scoreboard.CherryScoreboardObj.transform.GetChild(0).Find("Score2"))?.GetComponent<TextMeshProUGUI>();
          Scoreboard.CurrentTimerText = (TMP_Text) ((Component) Scoreboard.CherryScoreboardObj.transform.GetChild(0).Find("Timer"))?.GetComponent<TextMeshProUGUI>();
          Scoreboard.OldTimerText = (TMP_Text) ((Component) Scoreboard.CherryScoreboardObj.transform.GetChild(0).Find("Timer (1)"))?.GetComponent<TextMeshProUGUI>();
          break;
        case 2:
          if (((UnityEngine.Object) Scoreboard.PlayerScoreboardObj == (UnityEngine.Object) null))
            break;
          Scoreboard.Team1NameText = (TMP_Text) ((Component) Scoreboard.PlayerScoreboardObj.transform.GetChild(0).GetChild(1).GetChild(0).Find("TeamText"))?.GetComponent<TextMeshProUGUI>();
          Scoreboard.Team2NameText = (TMP_Text) ((Component) Scoreboard.PlayerScoreboardObj.transform.GetChild(1).GetChild(1).GetChild(0).Find("TeamText"))?.GetComponent<TextMeshProUGUI>();
          Scoreboard.Team1ScoreText = (TMP_Text) ((Component) Scoreboard.PlayerScoreboardObj.transform.GetChild(0).GetChild(1).GetChild(2).Find("Text (TMP)"))?.GetComponent<TextMeshProUGUI>();
          Scoreboard.Team2ScoreText = (TMP_Text) ((Component) Scoreboard.PlayerScoreboardObj.transform.GetChild(1).GetChild(1).GetChild(2).Find("Text (TMP)"))?.GetComponent<TextMeshProUGUI>();
          Scoreboard.CurrentTimerText = (TMP_Text) ((Component) Scoreboard.PlayerScoreboardObj.transform.GetChild(2).Find("Timer"))?.GetComponent<TextMeshProUGUI>();
          Scoreboard.OldTimerText = (TMP_Text) ((Component) Scoreboard.PlayerScoreboardObj.transform.GetChild(2).Find("Timer (1)"))?.GetComponent<TextMeshProUGUI>();
          break;
        case 3:
          if (((UnityEngine.Object) Scoreboard.CgtScoreboardObj == (UnityEngine.Object) null))
            break;
          Scoreboard.Team1NameText = (TMP_Text) ((Component) Scoreboard.CgtScoreboardObj.transform.GetChild(0).Find("Team1Text"))?.GetComponent<TextMeshProUGUI>();
          Scoreboard.Team2NameText = (TMP_Text) ((Component) Scoreboard.CgtScoreboardObj.transform.GetChild(0).Find("Team2Text"))?.GetComponent<TextMeshProUGUI>();
          Scoreboard.Team1ScoreText = (TMP_Text) ((Component) Scoreboard.CgtScoreboardObj.transform.GetChild(0).Find("Score1"))?.GetComponent<TextMeshProUGUI>();
          Scoreboard.Team2ScoreText = (TMP_Text) ((Component) Scoreboard.CgtScoreboardObj.transform.GetChild(0).Find("Score2"))?.GetComponent<TextMeshProUGUI>();
          Scoreboard.CurrentTimerText = (TMP_Text) ((Component) Scoreboard.CgtScoreboardObj.transform.GetChild(0).Find("Timer"))?.GetComponent<TextMeshProUGUI>();
          Scoreboard.OldTimerText = (TMP_Text) ((Component) Scoreboard.CgtScoreboardObj.transform.GetChild(0).Find("Timer (1)"))?.GetComponent<TextMeshProUGUI>();
          break;
      }
    }
    catch (Exception ex)
    {
      UnityEngine.Debug.LogError((object) $"Error resetting scoreboard UI elements: {ex}");
    }
  }

  private static void AddPlayerToScoreboard(
    string playerIndexStr,
    int playerNum,
    Transform cardRoot,
    int teamNum)
  {
    int result;
    if ((!int.TryParse(playerIndexStr, out result) || result < 0 ? 1 : (result >= GorillaDataHandler.GorillaDataList.Count ? 1 : 0)) != 0)
      throw new ArgumentOutOfRangeException($"Invalid player index '{playerIndexStr}' or index out of bounds.");
    GorillaData gorillaData = GorillaDataHandler.GorillaDataList[result];
    if (gorillaData == null)
      throw new NullReferenceException($"GorillaData not found for index {result}.");
    Transform child = cardRoot.Find("players")?.GetChild(playerNum - (teamNum == 1 ? 1 : 5));
    if (((UnityEngine.Object) child == (UnityEngine.Object) null))
      throw new NullReferenceException($"Player card transform not found for player number {playerNum} in team {teamNum}.");
    Scoreboard.PlayerSbDataDict.Add(gorillaData, new Scoreboard.PlayerSbData()
    {
      Num = playerNum,
      GorillaData = gorillaData,
      Card = child,
      Score = 0.0f,
      LastTagReq = 0.0f
    });
  }

  private static bool AutoRefRoundLive()
  {
    return Scoreboard.AutoRefControlled && AutoRefManager.Instance.IsRoundLive;
  }

  private static string BuildAutoRefRosterSig()
  {
    AutoRefManager instance = AutoRefManager.Instance;
    string str;
    if (!((UnityEngine.Object) instance == (UnityEngine.Object) null))
    {
      StringBuilder stringBuilder = new StringBuilder();
      foreach (NetPlayer netPlayer in instance.Team1Players.Take<NetPlayer>(4))
        stringBuilder.Append(netPlayer?.UserId).Append('|');
      stringBuilder.Append('#');
      foreach (NetPlayer netPlayer in instance.Team2Players.Take<NetPlayer>(4))
        stringBuilder.Append(netPlayer?.UserId).Append('|');
      str = stringBuilder.ToString();
    }
    else
      str = "";
    return str;
  }

  private static void PopulateFromAutoRef()
  {
    if (((UnityEngine.Object) Scoreboard.PlayerScoreboardObj == (UnityEngine.Object) null))
      return;
    AutoRefManager instance = AutoRefManager.Instance;
    if (((UnityEngine.Object) instance == (UnityEngine.Object) null))
      return;
    Scoreboard.PlayerSbDataDict.Clear();
    Transform child1 = Scoreboard.PlayerScoreboardObj.transform.GetChild(0);
    Transform child2 = Scoreboard.PlayerScoreboardObj.transform.GetChild(1);
    if ((((UnityEngine.Object) child1 == (UnityEngine.Object) null) ? 1 : (((UnityEngine.Object) child2 == (UnityEngine.Object) null) ? 1 : 0)) != 0)
      return;
    int num1 = 1;
    foreach (NetPlayer player in instance.Team1Players.Take<NetPlayer>(4))
      Scoreboard.AddGorillaToScoreboard(PlayerTranslator.GetGorillaByNetPlayer(player), num1++, child1, 1);
    int num2 = 5;
    foreach (NetPlayer player in instance.Team2Players.Take<NetPlayer>(4))
      Scoreboard.AddGorillaToScoreboard(PlayerTranslator.GetGorillaByNetPlayer(player), num2++, child2, 2);
    Scoreboard._lastScoreboardUpdateTime = Time.time - 3f;
  }

  private static void AddGorillaToScoreboard(
    GorillaData gorillaData,
    int playerNum,
    Transform cardRoot,
    int teamNum)
  {
    if ((gorillaData == null ? 1 : (((UnityEngine.Object) cardRoot == (UnityEngine.Object) null) ? 1 : 0)) != 0 || Scoreboard.PlayerSbDataDict.ContainsKey(gorillaData))
      return;
    Transform child = cardRoot.Find("players")?.GetChild(playerNum - (teamNum == 1 ? 1 : 5));
    if (((UnityEngine.Object) child == (UnityEngine.Object) null))
      return;
    Scoreboard.PlayerSbDataDict.Add(gorillaData, new Scoreboard.PlayerSbData()
    {
      Num = playerNum,
      GorillaData = gorillaData,
      Card = child,
      Score = 0.0f,
      LastTagReq = 0.0f
    });
  }

  public static void UpdatePlayerScoreboardPositions(
    string p1,
    string p2,
    string p3,
    string p4,
    string p5,
    string p6,
    string p7,
    string p8)
  {
    if (!Networking.InRoom)
      throw new InvalidOperationException("Must be in a room to update player scoreboard positions.");
    if (((UnityEngine.Object) Scoreboard.PlayerScoreboardObj == (UnityEngine.Object) null))
      throw new NullReferenceException("PlayerScoreboardObj is not assigned.");
    if (GorillaDataHandler.GorillaDataList.Count < 9)
      throw new InvalidOperationException("Requires at least 9 players in the lobby (including self).");
    Scoreboard.PlayerSbDataDict.Clear();
    try
    {
      Transform child1 = Scoreboard.PlayerScoreboardObj.transform.GetChild(0);
      Transform child2 = Scoreboard.PlayerScoreboardObj.transform.GetChild(1);
      if ((((UnityEngine.Object) child1 == (UnityEngine.Object) null) ? 1 : (((UnityEngine.Object) child2 == (UnityEngine.Object) null) ? 1 : 0)) != 0)
        throw new NullReferenceException("Team root transforms not found in PlayerScoreboardObj.");
      Scoreboard.AddPlayerToScoreboard(p1, 1, child1, 1);
      Scoreboard.AddPlayerToScoreboard(p2, 2, child1, 1);
      Scoreboard.AddPlayerToScoreboard(p3, 3, child1, 1);
      Scoreboard.AddPlayerToScoreboard(p4, 4, child1, 1);
      Scoreboard.AddPlayerToScoreboard(p5, 5, child2, 2);
      Scoreboard.AddPlayerToScoreboard(p6, 6, child2, 2);
      Scoreboard.AddPlayerToScoreboard(p7, 7, child2, 2);
      Scoreboard.AddPlayerToScoreboard(p8, 8, child2, 2);
      Scoreboard._lastScoreboardUpdateTime = Time.time - 3f;
      Scoreboard.UpdateScoreboard();
    }
    catch (Exception ex)
    {
      Scoreboard.PlayerSbDataDict.Clear();
      UnityEngine.Debug.LogError((object) $"Failed to update player scoreboard positions: {ex}");
      throw;
    }
  }

  internal class PlayerSbData
  {
    public int Num;
    public GorillaData GorillaData;
    public Transform Card;
    public float Score;
    public float LastTagReq;
    public RawImage CachedGorillaImage;
    public GameObject CachedLavaMonkeObject;
    public TextMeshProUGUI CachedScoreText;
    public TextMeshProUGUI CachedNameText;

    public void CacheComponents()
    {
      if ((((UnityEngine.Object) this.Card == (UnityEngine.Object) null) ? 1 : (((UnityEngine.Object) this.CachedGorillaImage != (UnityEngine.Object) null) ? 1 : 0)) != 0)
        return;
      Transform transform1 = this.Card.Find("Gorilla");
      if (((UnityEngine.Object) transform1 != (UnityEngine.Object) null))
      {
        this.CachedGorillaImage = ((Component) transform1).GetComponent<RawImage>();
        Transform transform2 = transform1.Find("LavaMonke");
        if (((UnityEngine.Object) transform2 != (UnityEngine.Object) null))
          this.CachedLavaMonkeObject = ((Component) transform2).gameObject;
      }
      Transform transform3 = this.Card.Find("ScoreText");
      if (((UnityEngine.Object) transform3 != (UnityEngine.Object) null))
        this.CachedScoreText = ((Component) transform3).GetComponent<TextMeshProUGUI>();
      Transform transform4 = this.Card.Find("NameBase");
      if ((!((UnityEngine.Object) transform4 != (UnityEngine.Object) null) ? 0 : (transform4.childCount > 0 ? 1 : 0)) == 0)
        return;
      Transform child = transform4.GetChild(0);
      if (!((UnityEngine.Object) child != (UnityEngine.Object) null))
        return;
      this.CachedNameText = ((Component) child).GetComponent<TextMeshProUGUI>();
    }
  }
}
