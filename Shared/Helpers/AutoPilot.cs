using SakuraaCastingMod.Core;
using SakuraaCastingMod.Shared.Models;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Shared.Helpers;

public static class AutoPilot
{
  public static bool AutoPilotEnabled;
  public static float APCooldown;
  public static float APCooldownInterval = 5f;
  public static bool PickTaggers;

  public static int GetClosest()
  {
    List<GorillaData> gorillaDataList = GorillaDataHandler.GorillaDataList;
    int closest;
    if (gorillaDataList.Count != 0)
    {
      string userId = NetworkSystem.Instance?.LocalPlayer?.UserId;
      float num1 = float.MaxValue;
      int num2 = -1;
      int num3 = -1;
      for (int index1 = 0; index1 < gorillaDataList.Count; ++index1)
      {
        GorillaData gorillaData1 = gorillaDataList[index1];
        if ((gorillaData1 == null ? 1 : (((UnityEngine.Object) gorillaData1.BodyTransform == (UnityEngine.Object) null) ? 1 : 0)) == 0 && !gorillaData1.Infected && !(gorillaData1.UserId == userId))
        {
          Vector3 position = gorillaData1.BodyTransform.position;
          for (int index2 = 0; index2 < gorillaDataList.Count; ++index2)
          {
            GorillaData gorillaData2 = gorillaDataList[index2];
            if ((gorillaData2 == null ? 1 : (((UnityEngine.Object) gorillaData2.BodyTransform == (UnityEngine.Object) null) ? 1 : 0)) == 0 && gorillaData2.Infected && !(gorillaData2.UserId == userId))
            {
              float num4 = Vector3.Distance(position, gorillaData2.BodyTransform.position);
              if ((double) num4 < (double) num1)
              {
                num1 = num4;
                num2 = index1;
                num3 = index2;
              }
            }
          }
        }
      }
      closest = num2 < 0 ? AutoPilot.GetNearestToCamera(gorillaDataList, userId) : (AutoPilot.PickTaggers ? num3 : num2);
    }
    else
      closest = 0;
    return closest;
  }

  private static int GetNearestToCamera(List<GorillaData> list, string localUserId)
  {
    Vector3 position = ((Component) Plugin.Ins.camera).transform.position;
    float num1 = float.MaxValue;
    int num2 = -1;
    for (int index = 0; index < list.Count; ++index)
    {
      GorillaData gorillaData = list[index];
      if ((gorillaData == null ? 1 : (((UnityEngine.Object) gorillaData.BodyTransform == (UnityEngine.Object) null) ? 1 : 0)) == 0 && !(gorillaData.UserId == localUserId))
      {
        float num3 = Vector3.Distance(position, gorillaData.BodyTransform.position);
        if ((double) num3 < (double) num1)
        {
          num1 = num3;
          num2 = index;
        }
      }
    }
    return num2 >= 0 ? num2 : 0;
  }
}
