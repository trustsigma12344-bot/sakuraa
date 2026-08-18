using SakuraaCastingMod.Features.Replay.EditJson;
using SakuraaCastingMod.Features.Replay.ReplayManagers;

#nullable disable
namespace SakuraaCastingMod.Features.Replay;

internal static class SpeedManager
{
  private static ReplayProject replayProject => ReplayManager.replayProject;

  public static float GetCurrentSpeed(float currentTime)
  {
    SpeedKeyframe speedKeyframe1 = (SpeedKeyframe) null;
    foreach (SpeedKeyframe speedKeyframe2 in SpeedManager.replayProject.SpeedKeyframes)
    {
      if (speedKeyframe1 == null)
        speedKeyframe1 = speedKeyframe2;
      if (((double) speedKeyframe1.Time > (double) speedKeyframe2.Time ? 0 : ((double) speedKeyframe2.Time < (double) currentTime ? 1 : 0)) != 0)
        speedKeyframe1 = speedKeyframe2;
    }
    return speedKeyframe1.SpeedValue;
  }

  public static float GetTotalProjectTimeWithSpeed()
  {
    float projectTimeWithSpeed = 0.0f;
    for (int index = 0; index < SpeedManager.replayProject.SpeedKeyframes.Count; ++index)
    {
      SpeedKeyframe speedKeyframe = SpeedManager.replayProject.SpeedKeyframes[index];
      float num = (index + 1 < SpeedManager.replayProject.SpeedKeyframes.Count ? SpeedManager.replayProject.SpeedKeyframes[index + 1].Time : ReplayManager.FindGreatestEndTime()) - speedKeyframe.Time;
      projectTimeWithSpeed += num / speedKeyframe.SpeedValue;
    }
    return projectTimeWithSpeed;
  }

  public static float GetGorillaFinalTimeWithSpeed(Gorilla gorilla)
  {
    float finalTimeWithSpeed = 0.0f;
    bool flag = false;
    for (int index = 0; index < SpeedManager.replayProject.SpeedKeyframes.Count && !flag; ++index)
    {
      float num1 = SpeedManager.replayProject.SpeedKeyframes[index].Time;
      float num2;
      if (index + 1 >= SpeedManager.replayProject.SpeedKeyframes.Count)
      {
        num2 = gorilla.EndTime / 100f;
      }
      else
      {
        num2 = SpeedManager.replayProject.SpeedKeyframes[index + 1].Time;
        if ((double) gorilla.EndTime / 100.0 < (double) num2)
        {
          flag = true;
          num2 = gorilla.EndTime / 100f;
        }
      }
      if ((double) gorilla.StartTime / 100.0 > (double) num1)
        num1 = gorilla.StartTime / 100f;
      if ((double) num1 <= (double) num2)
      {
        float num3 = num2 - num1;
        finalTimeWithSpeed += num3 / SpeedManager.replayProject.SpeedKeyframes[index].SpeedValue;
      }
    }
    return finalTimeWithSpeed;
  }

  public static float RealTimeToSpeedTime(float realTime)
  {
    float speedTime = 0.0f;
    for (int index = 0; index < SpeedManager.replayProject.SpeedKeyframes.Count; ++index)
    {
      SpeedKeyframe speedKeyframe = SpeedManager.replayProject.SpeedKeyframes[index];
      if ((double) speedKeyframe.Time <= (double) realTime)
      {
        float num1;
        if (index + 1 < SpeedManager.replayProject.SpeedKeyframes.Count)
        {
          num1 = SpeedManager.replayProject.SpeedKeyframes[index + 1].Time;
          if ((double) realTime < (double) num1)
            num1 = realTime;
        }
        else
          num1 = realTime;
        float num2 = num1 - speedKeyframe.Time;
        speedTime += num2 / speedKeyframe.SpeedValue;
      }
      else
        break;
    }
    return speedTime;
  }

  public static float SpeedTimeToRealTime(float SpeedTime)
  {
    float realTime = 0.0f;
    for (int index = 0; index < SpeedManager.replayProject.SpeedKeyframes.Count; ++index)
    {
      SpeedKeyframe speedKeyframe = SpeedManager.replayProject.SpeedKeyframes[index];
      if ((double) SpeedManager.RealTimeToSpeedTime(speedKeyframe.Time) <= (double) SpeedTime)
      {
        float num1;
        if (index + 1 < SpeedManager.replayProject.SpeedKeyframes.Count)
        {
          num1 = SpeedManager.RealTimeToSpeedTime(SpeedManager.replayProject.SpeedKeyframes[index + 1].Time);
          if ((double) SpeedTime < (double) num1)
            num1 = SpeedTime;
        }
        else
          num1 = SpeedTime;
        float num2 = num1 - SpeedManager.RealTimeToSpeedTime(speedKeyframe.Time);
        realTime += num2 * speedKeyframe.SpeedValue;
      }
      else
        break;
    }
    return realTime;
  }

  public static float PixelsToSecondSpeedAjusted(float NormalPxSec, float Px)
  {
    float secondSpeedAjusted = 0.0f;
    int num = 0;
    while (num < SpeedManager.replayProject.SpeedKeyframes.Count)
      ++num;
    return secondSpeedAjusted;
  }

  public static void DeleteFrame(SpeedKeyframe speedFrame)
  {
    ReplayHistory.Record();
    SpeedManager.replayProject.SpeedKeyframes.Remove(speedFrame);
  }

  public static void AddSpeedFrame(float time)
  {
    ReplayHistory.Record();
    SpeedManager.replayProject.SpeedKeyframes.Add(new SpeedKeyframe()
    {
      Time = time,
      SpeedValue = 1f
    });
  }
}
