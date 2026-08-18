using SakuraaCastingMod.Features.Replay.Recording;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Features.Replay.Patches;

internal class RecorderPlayerEnteredPatch
{
  public static void Prefix(NetPlayer newPlayer)
  {
    if ((!RecorderSystemBase.RecorderEnabled ? 1 : (((UnityEngine.Object) RecorderSystemBase.Instance == (UnityEngine.Object) null) ? 1 : 0)) != 0 || newPlayer == null || RecorderSystemBase.Instance.ReplayTime == 0)
      return;
    RecorderSystemBase.Instance.HandleJoiningPlayer(newPlayer);
  }
}
