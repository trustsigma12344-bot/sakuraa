using SakuraaCastingMod.Desktop.Ui.Framework.MenuItems;
using SakuraaCastingMod.Features.Overlays;
using SakuraaCastingMod.Shared.Helpers;
using System;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Desktop.Ui.Framework.Menus;

public static class ScoreboardMenu
{
  public static MenuBuilder _scoreboardMenu;
  private static string _player1SbPos = "1";
  private static string _player2SbPos = "2";
  private static string _player3SbPos = "3";
  private static string _player4SbPos = "4";
  private static string _player5SbPos = "5";
  private static string _player6SbPos = "6";
  private static string _player7SbPos = "7";
  private static string _player8SbPos = "9";

  public static void Draw()
  {
    if (ScoreboardMenu._scoreboardMenu == null)
      ScoreboardMenu.Initialize();
    ScoreboardMenu._scoreboardMenu.Draw();
  }

  public static void Initialize()
  {
    TextFieldMenuItem textFieldMenuItem1 = new TextFieldMenuItem();
    textFieldMenuItem1.Label = "Blue Team Name:";
    textFieldMenuItem1.Text = Scoreboard.Team1Name;
    textFieldMenuItem1.MaxLength = Scoreboard.CurrentScoreboard == 2 ? 24 : 4;
    textFieldMenuItem1.ForceUpperCase = true;
    textFieldMenuItem1.Description = "Set the name for Team 1 (Blue)";
    textFieldMenuItem1.OnTextChanged = (Action<string>) (text => Scoreboard.Team1Name = text);
    TextFieldMenuItem textFieldMenuItem2 = textFieldMenuItem1;
    TextFieldMenuItem textFieldMenuItem3 = new TextFieldMenuItem();
    textFieldMenuItem3.Label = "Red Team Name:";
    textFieldMenuItem3.Text = Scoreboard.Team2Name;
    textFieldMenuItem3.MaxLength = Scoreboard.CurrentScoreboard == 2 ? 24 : 4;
    textFieldMenuItem3.ForceUpperCase = true;
    textFieldMenuItem3.Description = "Set the name for Team 2 (Red)";
    textFieldMenuItem3.OnTextChanged = (Action<string>) (text => Scoreboard.Team2Name = text);
    TextFieldMenuItem textFieldMenuItem4 = textFieldMenuItem3;
    TextFieldMenuItem textFieldMenuItem5 = new TextFieldMenuItem();
    textFieldMenuItem5.Label = "Player 1 leaderboard num:";
    textFieldMenuItem5.Text = ScoreboardMenu._player1SbPos;
    textFieldMenuItem5.MaxLength = 1;
    textFieldMenuItem5.ForceUpperCase = true;
    textFieldMenuItem5.Description = "Player index for Team 1 Slot 1";
    textFieldMenuItem5.IsVisible = (Func<bool>) (() => Scoreboard.CurrentScoreboard == 2 && !Scoreboard.AutoRefControlled);
    textFieldMenuItem5.OnTextChanged = (Action<string>) (text => ScoreboardMenu._player1SbPos = text);
    TextFieldMenuItem textFieldMenuItem6 = textFieldMenuItem5;
    TextFieldMenuItem textFieldMenuItem7 = new TextFieldMenuItem();
    textFieldMenuItem7.Label = "Player 2 leaderboard num:";
    textFieldMenuItem7.Text = ScoreboardMenu._player2SbPos;
    textFieldMenuItem7.MaxLength = 1;
    textFieldMenuItem7.ForceUpperCase = true;
    textFieldMenuItem7.Description = "Player index for Team 1 Slot 2";
    textFieldMenuItem7.IsVisible = (Func<bool>) (() => Scoreboard.CurrentScoreboard == 2 && !Scoreboard.AutoRefControlled);
    textFieldMenuItem7.OnTextChanged = (Action<string>) (text => ScoreboardMenu._player2SbPos = text);
    TextFieldMenuItem textFieldMenuItem8 = textFieldMenuItem7;
    TextFieldMenuItem textFieldMenuItem9 = new TextFieldMenuItem();
    textFieldMenuItem9.Label = "Player 3 leaderboard num:";
    textFieldMenuItem9.Text = ScoreboardMenu._player3SbPos;
    textFieldMenuItem9.MaxLength = 1;
    textFieldMenuItem9.ForceUpperCase = true;
    textFieldMenuItem9.Description = "Player index for Team 1 Slot 3";
    textFieldMenuItem9.IsVisible = (Func<bool>) (() => Scoreboard.CurrentScoreboard == 2 && !Scoreboard.AutoRefControlled);
    textFieldMenuItem9.OnTextChanged = (Action<string>) (text => ScoreboardMenu._player3SbPos = text);
    TextFieldMenuItem textFieldMenuItem10 = textFieldMenuItem9;
    TextFieldMenuItem textFieldMenuItem11 = new TextFieldMenuItem();
    textFieldMenuItem11.Label = "Player 4 leaderboard num:";
    textFieldMenuItem11.Text = ScoreboardMenu._player4SbPos;
    textFieldMenuItem11.MaxLength = 1;
    textFieldMenuItem11.ForceUpperCase = true;
    textFieldMenuItem11.Description = "Player index for Team 1 Slot 4";
    textFieldMenuItem11.IsVisible = (Func<bool>) (() => Scoreboard.CurrentScoreboard == 2 && !Scoreboard.AutoRefControlled);
    textFieldMenuItem11.OnTextChanged = (Action<string>) (text => ScoreboardMenu._player4SbPos = text);
    TextFieldMenuItem textFieldMenuItem12 = textFieldMenuItem11;
    TextFieldMenuItem textFieldMenuItem13 = new TextFieldMenuItem();
    textFieldMenuItem13.Label = "Player 1 leaderboard num:";
    textFieldMenuItem13.Text = ScoreboardMenu._player5SbPos;
    textFieldMenuItem13.MaxLength = 1;
    textFieldMenuItem13.ForceUpperCase = true;
    textFieldMenuItem13.Description = "Player index for Team 2 Slot 1";
    textFieldMenuItem13.IsVisible = (Func<bool>) (() => Scoreboard.CurrentScoreboard == 2 && !Scoreboard.AutoRefControlled);
    textFieldMenuItem13.OnTextChanged = (Action<string>) (text => ScoreboardMenu._player5SbPos = text);
    TextFieldMenuItem textFieldMenuItem14 = textFieldMenuItem13;
    TextFieldMenuItem textFieldMenuItem15 = new TextFieldMenuItem();
    textFieldMenuItem15.Label = "Player 2 leaderboard num:";
    textFieldMenuItem15.Text = ScoreboardMenu._player6SbPos;
    textFieldMenuItem15.MaxLength = 1;
    textFieldMenuItem15.ForceUpperCase = true;
    textFieldMenuItem15.Description = "Player index for Team 2 Slot 2";
    textFieldMenuItem15.IsVisible = (Func<bool>) (() => Scoreboard.CurrentScoreboard == 2 && !Scoreboard.AutoRefControlled);
    textFieldMenuItem15.OnTextChanged = (Action<string>) (text => ScoreboardMenu._player6SbPos = text);
    TextFieldMenuItem textFieldMenuItem16 = textFieldMenuItem15;
    TextFieldMenuItem textFieldMenuItem17 = new TextFieldMenuItem();
    textFieldMenuItem17.Label = "Player 3 leaderboard num:";
    textFieldMenuItem17.Text = ScoreboardMenu._player7SbPos;
    textFieldMenuItem17.MaxLength = 1;
    textFieldMenuItem17.ForceUpperCase = true;
    textFieldMenuItem17.Description = "Player index for Team 2 Slot 3";
    textFieldMenuItem17.IsVisible = (Func<bool>) (() => Scoreboard.CurrentScoreboard == 2 && !Scoreboard.AutoRefControlled);
    textFieldMenuItem17.OnTextChanged = (Action<string>) (text => ScoreboardMenu._player7SbPos = text);
    TextFieldMenuItem textFieldMenuItem18 = textFieldMenuItem17;
    TextFieldMenuItem textFieldMenuItem19 = new TextFieldMenuItem();
    textFieldMenuItem19.Label = "Player 4 leaderboard num:";
    textFieldMenuItem19.Text = ScoreboardMenu._player8SbPos;
    textFieldMenuItem19.MaxLength = 1;
    textFieldMenuItem19.ForceUpperCase = true;
    textFieldMenuItem19.Description = "Player index for Team 2 Slot 4";
    textFieldMenuItem19.IsVisible = (Func<bool>) (() => Scoreboard.CurrentScoreboard == 2 && !Scoreboard.AutoRefControlled);
    textFieldMenuItem19.OnTextChanged = (Action<string>) (text => ScoreboardMenu._player8SbPos = text);
    TextFieldMenuItem textFieldMenuItem20 = textFieldMenuItem19;
    ScoreboardMenu._scoreboardMenu = new MenuBuilder("ScoreBoard Options", 250f).SetPositionRef(100f, 300f).AddSpace(15f).AddDynamicLabel((Func<string>) (() => "<color=yellow>Controlled by Auto Ref - manual controls locked</color>"), visibilityCondition: (Func<bool>) (() => Scoreboard.AutoRefControlled)).AddItem((MenuItem) textFieldMenuItem2).AddItem((MenuItem) textFieldMenuItem4).AddDynamicButton((Func<string>) (() => "Timer starts at: " + (Scoreboard.TimerWait ? "-10" : "0")), (Action) (() =>
    {
      Scoreboard.TimerWait = !Scoreboard.TimerWait;
      if ((double) Scoreboard._currentTimerTime > 0.0)
        return;
      Scoreboard._currentTimerTime = Scoreboard.TimerWait ? -10f : 0.0f;
    }), "Toggle whether the timer starts at -10s or 0s", (Func<bool>) (() => !Scoreboard.AutoRefControlled)).AddDynamicToggle("Round end pause", (Func<bool>) (() => Scoreboard.PauseOnRoundEnd), (Action<bool>) (v =>
    {
      Scoreboard.PauseOnRoundEnd = v;
      if (!Scoreboard.PauseOnRoundEnd)
        return;
      Scoreboard.CountDown = false;
    }), "Toggle pausing the timer when all players are tagged", (Func<bool>) (() => !Scoreboard.AutoRefControlled)).AddDynamicButton((Func<string>) (() => "Counting: " + (Scoreboard.CountDown ? "down" : "up")), (Action) (() =>
    {
      float currentTimerTime = Scoreboard._currentTimerTime;
      if ((double) currentTimerTime > 0.0 && (double) currentTimerTime < 1800.0)
        return;
      Scoreboard.CountDown = !Scoreboard.CountDown;
      if (Scoreboard.CountDown)
      {
        Scoreboard._currentTimerTime = 1800f;
        Scoreboard.PauseOnRoundEnd = false;
      }
      else
        Scoreboard._currentTimerTime = Scoreboard.TimerWait ? -10f : 0.0f;
    }), "Toggle timer count direction (requires timer reset)", (Func<bool>) (() => !Scoreboard.AutoRefControlled)).AddDynamicButton((Func<string>) (() =>
    {
      string str;
      switch (Scoreboard.CurrentScoreboard)
      {
        case 0:
          str = "default";
          break;
        case 1:
          str = "cherry";
          break;
        case 2:
          str = "player";
          break;
        case 3:
          str = "cgt";
          break;
        default:
          str = "??";
          break;
      }
      return "Scoreboard Overlay: " + str;
    }), (Action) (() =>
    {
      Scoreboard.CurrentScoreboard = (Scoreboard.CurrentScoreboard + 1) % 4;
      Scoreboard.ResetScoreboard();
    }), "Cycle through available scoreboard overlay styles").AddSlider("X Position:", Scoreboard._scorePos.x, 0.0f, (float) Screen.width, (Action<float>) (value =>
    {
      Vector2 scorePos = Scoreboard._scorePos;
      scorePos.x = value;
      Scoreboard._scorePos = scorePos;
      Scoreboard.TransformScoreboards();
    }), description: "Adjust horizontal position of the scoreboard overlay").AddSlider("Y Position:", Scoreboard._scorePos.y, 0.0f, (float) Screen.height, (Action<float>) (value =>
    {
      Vector2 scorePos = Scoreboard._scorePos;
      scorePos.y = value;
      Scoreboard._scorePos = scorePos;
      Scoreboard.TransformScoreboards();
    }), description: "Adjust vertical position of the scoreboard overlay").AddSlider("Scale:", Scoreboard.ScoreScale, 0.2f, 2f, (Action<float>) (value =>
    {
      Scoreboard.ScoreScale = value;
      Scoreboard.TransformScoreboards();
    }), 1, "Adjust the size of the scoreboard overlay").AddButton("Center", (Action) (() =>
    {
      Scoreboard._scorePos = new Vector2((float) Screen.width / 2f, (float) Screen.height / 2f);
      Scoreboard.TransformScoreboards();
    }), "Center the scoreboard overlay on the screen").AddSpace().AddLabel("Keybinds:").AddLabel("Space | Unpause Timer").AddLabel("N | Reset Timer").AddLabel("- | -Blue").AddLabel("= | +Blue").AddLabel("Shift- | -Red").AddLabel("Shift= | +Red").AddLabel("<color=lime>Players auto-filled by Auto Ref</color>", visibilityCondition: (Func<bool>) (() => Scoreboard.CurrentScoreboard == 2 && Scoreboard.AutoRefControlled)).AddLabel("Team 1 players:", visibilityCondition: (Func<bool>) (() => Scoreboard.CurrentScoreboard == 2 && !Scoreboard.AutoRefControlled)).AddItem((MenuItem) textFieldMenuItem6).AddItem((MenuItem) textFieldMenuItem8).AddItem((MenuItem) textFieldMenuItem10).AddItem((MenuItem) textFieldMenuItem12).AddLabel("Team 2 players:", visibilityCondition: (Func<bool>) (() => Scoreboard.CurrentScoreboard == 2 && !Scoreboard.AutoRefControlled)).AddItem((MenuItem) textFieldMenuItem14).AddItem((MenuItem) textFieldMenuItem16).AddItem((MenuItem) textFieldMenuItem18).AddItem((MenuItem) textFieldMenuItem20).AddButton("Update Positions", (Action) (() =>
    {
      if (!Networking.InRoom)
        Notification.Send("You need to be in a room to use this!", Color.red);
      else if (GorillaDataHandler.GorillaDataList.Count < 9)
      {
        Notification.Send("You need at-least 9 people in a lobby to use this!", Color.red);
      }
      else
      {
        try
        {
          Scoreboard.UpdatePlayerScoreboardPositions(ScoreboardMenu._player1SbPos, ScoreboardMenu._player2SbPos, ScoreboardMenu._player3SbPos, ScoreboardMenu._player4SbPos, ScoreboardMenu._player5SbPos, ScoreboardMenu._player6SbPos, ScoreboardMenu._player7SbPos, ScoreboardMenu._player8SbPos);
          Notification.Send("Player scoreboard positions updated.", Color.green);
        }
        catch (Exception ex)
        {
          Notification.Send("Error updating positions", Color.red);
          UnityEngine.Debug.LogError((object) "Scoreboard position update error: {ex}");
        }
      }
    }), "Assign players to scoreboard slots based on lobby index", (Func<bool>) (() => Scoreboard.CurrentScoreboard == 2 && !Scoreboard.AutoRefControlled));
  }
}
