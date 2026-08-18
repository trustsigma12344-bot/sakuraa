using Newtonsoft.Json;
using SakuraaCastingMod.Features.Overlays;
using SakuraaCastingMod.Features.Replay.EditJson;
using SakuraaCastingMod.Features.Replay.ReplayJson;
using SakuraaCastingMod.Features.Replay.utils;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Features.Replay.ReplayManagers;

public static class ReplayManager
{
  public static int NewestReplayProjectVersion = 3;

  public static event Action<ReplayProject> OnReplayProjectChanged;

  public static ReplayProject replayProject { get; private set; }

  public static string CurrentReplayProjectFormatVersion
  {
    get => $"{ReplayManager.NewestReplayProjectVersion}vPRPH";
  }

  private static void SetReplayProject(ReplayProject project)
  {
    ReplayManager.replayProject = project;
    Action<ReplayProject> replayProjectChanged = ReplayManager.OnReplayProjectChanged;
    if (replayProjectChanged == null)
      return;
    replayProjectChanged(ReplayManager.replayProject);
  }

  public static void UnloadProject() => ReplayManager.SetReplayProject((ReplayProject) null);

  public static void SaveProject()
  {
    if (ReplayManager.replayProject == null)
      return;
    File.WriteAllText(FileSelector.GetFilePathToSaveTo("ReplayProject.ReplayProject"), JsonConvert.SerializeObject((object) ReplayManager.replayProject));
  }

  public static float FindGreatestEndTime()
  {
    float num1 = 0.0f;
    float greatestEndTime;
    if (ReplayManager.replayProject != null)
    {
      foreach (ReplayInfo replayInfo in ReplayManager.replayProject.replayInfos)
      {
        float num2 = (float) (replayInfo.replay.FinalTime / 100) + replayInfo.StartOffsetTime;
        if ((double) num2 > (double) num1)
          num1 = num2;
      }
      greatestEndTime = num1;
    }
    else
      greatestEndTime = num1;
    return greatestEndTime;
  }

  public static int FindTotalPlayersInProject()
  {
    int num = 0;
    int playersInProject;
    if (ReplayManager.replayProject == null)
    {
      playersInProject = num;
    }
    else
    {
      foreach (ReplayInfo replayInfo in ReplayManager.replayProject.replayInfos)
        num += replayInfo.replay.playerDatas.Count;
      playersInProject = num;
    }
    return playersInProject;
  }

  public static Player GetPlayerInfoFromData(PlayerData data, SakuraaCastingMod.Features.Replay.ReplayJson.Replay replay)
  {
    Player playerInfoFromData = (Player) null;
    foreach (Player player in replay.players)
    {
      if (player.actornumber == data.actorNumber)
        playerInfoFromData = player;
    }
    return playerInfoFromData;
  }

  private static bool RawIsProject(string rawString)
  {
    try
    {
      ReplayManager.FormatPeek formatPeek = JsonConvert.DeserializeObject<ReplayManager.FormatPeek>(rawString);
      return formatPeek != null && formatPeek.FormatVersion != null && formatPeek.FormatVersion.EndsWith("PRPH");
    }
    catch
    {
      return false;
    }
  }

  public static ReplayProject ParseProjectFromPath(string filePath, out bool wasProjectFile)
  {
    wasProjectFile = false;
    ReplayProject projectFromPath;
    if (!string.IsNullOrEmpty(filePath))
    {
      string rawString = File.ReadAllText(filePath);
      if (ReplayManager.RawIsProject(rawString))
      {
        wasProjectFile = true;
        ReplayProject replayProject = JsonConvert.DeserializeObject<ReplayProject>(rawString);
        ReplayUpdater.UpdateReplayProject(replayProject);
        projectFromPath = replayProject;
      }
      else
      {
        SakuraaCastingMod.Features.Replay.ReplayJson.Replay replay = JsonConvert.DeserializeObject<SakuraaCastingMod.Features.Replay.ReplayJson.Replay>(rawString);
        if (replay.ID == null)
          replay.ID = RandomUtils.GenerateRandomNumberString(5);
        ReplayInfo replayInfo = new ReplayInfo()
        {
          StartOffsetTime = 0.0f,
          replay = replay,
          ReplayFolderName = Path.GetDirectoryName(filePath)
        };
        projectFromPath = new ReplayProject()
        {
          FormatVersion = ReplayManager.CurrentReplayProjectFormatVersion,
          CameraKeyFrames = new List<CameraKeyFrame>(),
          replayInfos = new List<ReplayInfo>()
          {
            replayInfo
          },
          SpeedKeyframes = new List<SpeedKeyframe>()
          {
            new SpeedKeyframe() { Time = 0.0f, SpeedValue = 1f }
          },
          VoiceKeyFrames = new List<VoiceKeyFrame>()
        };
      }
    }
    else
      projectFromPath = (ReplayProject) null;
    return projectFromPath;
  }

  public static void ApplyParsed(ReplayProject parsed, bool wasProjectFile)
  {
    if (parsed == null)
      return;
    if (ReplayManager.replayProject == null)
    {
      ReplayManager.SetReplayProject(parsed);
    }
    else
    {
      if (wasProjectFile)
        return;
      ReplayManager.replayProject.replayInfos.Add(parsed.replayInfos[0]);
    }
  }

  public static bool BlockedByRoom()
  {
    bool flag;
    if ((!((UnityEngine.Object) NetworkSystem.Instance != (UnityEngine.Object) null) ? 0 : (NetworkSystem.Instance.InRoom ? 1 : 0)) == 0)
    {
      flag = false;
    }
    else
    {
      Notification.Send("Leave the room before loading a replay.", Color.yellow);
      flag = true;
    }
    return flag;
  }

  public static void LoadFile()
  {
    if (ReplayManager.BlockedByRoom())
      return;
    ReplayManager.LoadFromPath(FileSelector.GetFilePathFromUser());
  }

  public static void LoadFromPath(string filePath)
  {
    if (string.IsNullOrEmpty(filePath) || ReplayManager.BlockedByRoom())
      return;
    bool wasProjectFile;
    ReplayManager.ApplyParsed(ReplayManager.ParseProjectFromPath(filePath, out wasProjectFile), wasProjectFile);
  }

  private class FormatPeek
  {
    public string FormatVersion { get; set; }
  }
}
