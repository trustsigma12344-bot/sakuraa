using GorillaNetworking;
using SakuraaCastingMod.Core;
using SakuraaCastingMod.Shared.Helpers;
using SakuraaCastingMod.VR.UtilMenu.Utility;
using System;
using System.Collections;
using System.ComponentModel;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Api;

[Browsable(true)]
public static class Rooms
{
  private static bool _hopInFlight;

  public static bool LobbyHop()
  {
    PhotonNetworkController instance = PhotonNetworkController.Instance;
    GorillaNetworkJoinTrigger currentJoinTrigger = ((UnityEngine.Object) instance != (UnityEngine.Object) null) ? instance.currentJoinTrigger : (GorillaNetworkJoinTrigger) null;
    bool flag;
    if ((((UnityEngine.Object) currentJoinTrigger == (UnityEngine.Object) null) ? 1 : (Rooms._hopInFlight ? 1 : 0)) != 0)
    {
      flag = false;
    }
    else
    {
      MonoBehaviour monoBehaviour = Rooms.CoroutineHost();
      if (!((UnityEngine.Object) monoBehaviour == (UnityEngine.Object) null))
      {
        monoBehaviour.StartCoroutine(Rooms.LobbyHopRoutine(currentJoinTrigger));
        flag = true;
      }
      else
        flag = false;
    }
    return flag;
  }

  private static MonoBehaviour CoroutineHost()
  {
    return !((UnityEngine.Object) FriendNetworkController.Instance != (UnityEngine.Object) null) ? (MonoBehaviour) Plugin.Ins : (MonoBehaviour) FriendNetworkController.Instance;
  }

  private static IEnumerator LobbyHopRoutine(GorillaNetworkJoinTrigger trigger)
  {
    Rooms._hopInFlight = true;
    try
    {
      FriendNetworkController fnc = FriendNetworkController.Instance;
      if (((UnityEngine.Object) fnc != (UnityEngine.Object) null))
      {
        yield return (object) fnc.SafeLeaveAndWait();
        if (!fnc.LastReJoinReachedIdle)
        {
          UnityEngine.Debug.LogWarning((object) "[Api.Rooms] lobby hop aborted, leave didn't settle cleanly; not shipping a possibly-stale ticket.");
          yield break;
        }
      }
      else
      {
        NetworkSystem.Instance.ReturnToSinglePlayer();
        yield return (object) new WaitForSeconds(2f);
        yield return (object) new WaitUntil((Func<bool>) (() => !NetworkSystem.Instance.InRoom));
      }
      JoinForensics.NoteModJoin("<lobby-hop>", false, "Api.Rooms.LobbyHop");
      ((GorillaTriggerBox) trigger).OnBoxTriggered();
      float startWait = Time.realtimeSinceStartup;
      yield return (object) new WaitUntil((Func<bool>) (() => ((UnityEngine.Object) NetworkSystem.Instance == (UnityEngine.Object) null) || NetworkSystem.Instance.netState != (NetSystemState) 2 || (double) Time.realtimeSinceStartup - (double) startWait > 3.0));
      float deadline = Time.realtimeSinceStartup + 20f;
      yield return (object) new WaitUntil((Func<bool>) (() => ((UnityEngine.Object) NetworkSystem.Instance == (UnityEngine.Object) null) || NetworkSystem.Instance.netState == (NetSystemState) 4 || NetworkSystem.Instance.netState == (NetSystemState) 2 || (double) Time.realtimeSinceStartup > (double) deadline));
      fnc = (FriendNetworkController) null;
    }
    finally
    {
      Rooms._hopInFlight = false;
    }
  }
}
