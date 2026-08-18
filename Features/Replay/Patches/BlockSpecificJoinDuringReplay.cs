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
internal class BlockSpecificJoinDuringReplay
{
  private static MethodBase TargetMethod()
  {
    return (MethodBase) AccessTools.Method(typeof (PhotonNetworkController), "AttemptToJoinSpecificRoom", (Type[]) null, (Type[]) null);
  }

  private static bool Prefix()
  {
    bool flag;
    if (Plugin.ReplayCompatibilityMode)
    {
      Notification.Send("Can't join a room during replay playback.", Color.yellow);
      flag = false;
    }
    else
      flag = true;
    return flag;
  }
}
