using SakuraaCastingMod.Features.Replay.Recording;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Features.Replay.Patches;

internal class RecorderPlayerLeftPatch
{
  public static void Prefix(NetPlayer otherPlayer)
  {
    if ((!RecorderSystemBase.RecorderEnabled ? 1 : (((UnityEngine.Object) RecorderSystemBase.Instance == (UnityEngine.Object) null) ? 1 : 0)) != 0 || otherPlayer == null || RecorderSystemBase.Instance.ReplayTime == 0)
      return;
    RecorderSystemBase.Instance.HandleLeavingPlayer(otherPlayer);
  }
}
