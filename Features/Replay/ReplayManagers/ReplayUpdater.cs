using SakuraaCastingMod.Features.Replay.EditJson;
using System;
using System.Collections.Generic;

#nullable disable
namespace SakuraaCastingMod.Features.Replay.ReplayManagers;

internal static class ReplayUpdater
{
  private static Dictionary<int, Action<ReplayProject>> ReplayProjectUpdaters = new Dictionary<int, Action<ReplayProject>>()
  {
    {
      1,
      new Action<ReplayProject>(ReplayUpdater.UpdateReplayProjectV1ToV2)
    },
    {
      2,
      new Action<ReplayProject>(ReplayUpdater.UpdateReplayProjectV2ToV3)
    }
  };

  public static string[] CustomSplit(string str, string delimiter)
  {
    string[] strArray;
    if ((string.IsNullOrEmpty(str) ? 1 : (string.IsNullOrEmpty(delimiter) ? 1 : 0)) == 0)
    {
      List<string> stringList = new List<string>();
      int startIndex;
      int num;
      for (startIndex = 0; (num = str.IndexOf(delimiter, startIndex)) != -1; startIndex = num + delimiter.Length)
        stringList.Add(str.Substring(startIndex, num - startIndex));
      stringList.Add(str.Substring(startIndex));
      strArray = stringList.ToArray();
    }
    else
      strArray = new string[1]{ str };
    return strArray;
  }

  public static void UpdateReplayProject(ReplayProject replayProject)
  {
    int num = int.Parse(ReplayUpdater.CustomSplit(replayProject.FormatVersion, "v")[0]);
    if (num >= ReplayManager.NewestReplayProjectVersion)
      return;
    int key1 = -1;
    foreach (int key2 in ReplayUpdater.ReplayProjectUpdaters.Keys)
    {
      if ((key2 <= key1 ? 0 : (num <= key2 ? 1 : 0)) != 0)
        key1 = key2;
    }
    if (key1 == -1)
      return;
    ReplayUpdater.ReplayProjectUpdaters[key1](replayProject);
  }

  private static void UpdateReplayProjectV2ToV3(ReplayProject replayProject)
  {
    if (replayProject.SpeedKeyframes == null)
    {
      List<SpeedKeyframe> speedKeyframeList = new List<SpeedKeyframe>()
      {
        new SpeedKeyframe() { Time = 0.0f, SpeedValue = 1f }
      };
      replayProject.SpeedKeyframes = speedKeyframeList;
    }
    replayProject.FormatVersion = $"{3}vPRPH";
    ReplayUpdater.UpdateReplayProject(replayProject);
  }

  private static void UpdateReplayProjectV1ToV2(ReplayProject replayProject)
  {
    foreach (CameraKeyFrame cameraKeyFrame in replayProject.CameraKeyFrames)
    {
      if (cameraKeyFrame.firstPersonSettings == null)
        cameraKeyFrame.firstPersonSettings = new FirstPersonSettings();
    }
    replayProject.FormatVersion = $"{2}vPRPH";
    ReplayUpdater.UpdateReplayProject(replayProject);
  }
}
