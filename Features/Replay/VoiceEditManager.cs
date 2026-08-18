using SakuraaCastingMod.Features.Replay.EditJson;
using SakuraaCastingMod.Features.Replay.ReplayJson;
using SakuraaCastingMod.Features.Replay.ReplayManagers;
using System.Collections.Generic;

#nullable disable
namespace SakuraaCastingMod.Features.Replay;

internal static class VoiceEditManager
{
  private static ReplayProject replayProject => ReplayManager.replayProject;

  public static bool IsActorMutedAt(int actorNumber, float realTime)
  {
    bool flag1;
    if ((VoiceEditManager.replayProject == null ? 1 : (VoiceEditManager.replayProject.VoiceKeyFrames == null ? 1 : 0)) != 0)
    {
      flag1 = false;
    }
    else
    {
      bool flag2 = false;
      float num = float.NegativeInfinity;
      foreach (VoiceKeyFrame voiceKeyFrame in VoiceEditManager.replayProject.VoiceKeyFrames)
      {
        if (voiceKeyFrame.ActorNumber == actorNumber && ((double) voiceKeyFrame.Time > (double) realTime ? 0 : ((double) voiceKeyFrame.Time >= (double) num ? 1 : 0)) != 0)
        {
          num = voiceKeyFrame.Time;
          flag2 = voiceKeyFrame.Muted;
        }
      }
      flag1 = flag2;
    }
    return flag1;
  }

  public static void ToggleAt(int actorNumber, float realTime)
  {
    if (VoiceEditManager.replayProject == null)
      return;
    ReplayHistory.Record();
    if (VoiceEditManager.replayProject.VoiceKeyFrames == null)
      VoiceEditManager.replayProject.VoiceKeyFrames = new List<VoiceKeyFrame>();
    bool flag = VoiceEditManager.IsActorMutedAt(actorNumber, realTime);
    VoiceEditManager.replayProject.VoiceKeyFrames.Add(new VoiceKeyFrame()
    {
      Time = realTime,
      ActorNumber = actorNumber,
      Muted = !flag
    });
  }

  public static void DeleteFrame(VoiceKeyFrame frame)
  {
    if ((VoiceEditManager.replayProject == null ? 1 : (VoiceEditManager.replayProject.VoiceKeyFrames == null ? 1 : 0)) != 0)
      return;
    ReplayHistory.Record();
    VoiceEditManager.replayProject.VoiceKeyFrames.Remove(frame);
  }

  public static void SetAllAt(bool muted, float time)
  {
    if (VoiceEditManager.replayProject == null)
      return;
    ReplayHistory.Record();
    if (VoiceEditManager.replayProject.VoiceKeyFrames == null)
      VoiceEditManager.replayProject.VoiceKeyFrames = new List<VoiceKeyFrame>();
    foreach (Player player in VoiceEditManager.GetPlayers())
      VoiceEditManager.replayProject.VoiceKeyFrames.Add(new VoiceKeyFrame()
      {
        Time = time,
        ActorNumber = player.actornumber,
        Muted = muted
      });
  }

  public static List<Player> GetPlayers()
  {
    List<Player> playerList = new List<Player>();
    List<Player> players;
    if ((VoiceEditManager.replayProject == null ? 1 : (VoiceEditManager.replayProject.replayInfos == null ? 1 : 0)) == 0)
    {
      foreach (ReplayInfo replayInfo in VoiceEditManager.replayProject.replayInfos)
      {
        if ((replayInfo.replay == null ? 1 : (replayInfo.replay.players == null ? 1 : 0)) == 0)
        {
          foreach (Player player1 in replayInfo.replay.players)
          {
            bool flag = false;
            foreach (Player player2 in playerList)
            {
              if (player2.actornumber == player1.actornumber)
              {
                flag = true;
                break;
              }
            }
            if (!flag)
              playerList.Add(player1);
          }
        }
      }
      players = playerList;
    }
    else
      players = playerList;
    return players;
  }
}
