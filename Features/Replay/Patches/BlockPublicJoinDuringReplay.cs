using GorillaNetworking;
using HarmonyLib;
using SakuraaCastingMod.Core;
using SakuraaCastingMod.Features.Overlays;
using System;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Features.Replay.Patches;

[HarmonyPatch]
internal class BlockPublicJoinDuringReplay
{
  private static MethodBase TargetMethod()
  {
    return (MethodBase) AccessTools.Method(typeof (PhotonNetworkController), "AttemptToJoinPublicRoom", (Type[]) null, (Type[]) null);
  }

  private static bool Prefix()
  {
    bool flag;
    if (!Plugin.ReplayCompatibilityMode)
    {
      flag = true;
    }
    else
    {
      Notification.Send("Can't join a room during replay playback.", Color.yellow);
      flag = false;
    }
    return flag;
  }
}
