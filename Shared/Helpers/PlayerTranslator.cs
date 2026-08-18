using Photon.Pun;
using Photon.Realtime;
using SakuraaCastingMod.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Shared.Helpers;

public static class PlayerTranslator
{
  public static readonly List<VRRig> vrrigs = new List<VRRig>();
  public static readonly Dictionary<NetPlayer, VRRig> vrrigDict = new Dictionary<NetPlayer, VRRig>();
  private static bool _subscribed;

  public static List<VRRig> Vrrigs => PlayerTranslator.vrrigs;

  public static Dictionary<NetPlayer, VRRig> VRRigDict => PlayerTranslator.vrrigDict;

  public static void Initialize()
  {
    if (PlayerTranslator._subscribed)
      return;
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    VRRigCache.OnActiveRigsChanged += new Action(PlayerTranslator.RebuildCache);
    PlayerTranslator._subscribed = true;
    PlayerTranslator.RebuildCache();
  }

  private static void RebuildCache()
  {
    PlayerTranslator.vrrigs.Clear();
    PlayerTranslator.vrrigDict.Clear();
    if (!VRRigCache.isInitialized)
      return;
    foreach (RigContainer activeRigContainer in (IEnumerable<RigContainer>) VRRigCache.ActiveRigContainers)
    {
      PlayerTranslator.vrrigs.Add(activeRigContainer.Rig);
      if (activeRigContainer.Creator != null)
        PlayerTranslator.vrrigDict[activeRigContainer.Creator] = activeRigContainer.Rig;
    }
  }

  public static NetPlayer GetNetPlayerByPhoton(Player photonPlayer)
  {
    return photonPlayer != null ? NetworkSystem.Instance.GetPlayer(photonPlayer.ActorNumber) : (NetPlayer) null;
  }

  public static Player GetPhotonByNetPlayer(NetPlayer netPlayer)
  {
    return ((IEnumerable<Player>) PhotonNetwork.PlayerList).FirstOrDefault<Player>((Func<Player, bool>) (p => p.UserId == netPlayer.UserId));
  }

  public static GorillaData GetGorillaByNetPlayer(NetPlayer player)
  {
    return (player == null ? 1 : (GorillaDataHandler.GorillaDataDict == null ? 1 : 0)) == 0 ? (GorillaDataHandler.GorillaDataDict.TryGetValue(player.UserId, out GorillaData gorillaData) ? gorillaData : null) : (GorillaData) null;
  }

  public static NetPlayer GetNetPlayerByGorilla(GorillaData gorilla)
  {
    NetPlayer netPlayerByGorilla;
    if (gorilla == null)
    {
      netPlayerByGorilla = (NetPlayer) null;
    }
    else
    {
      foreach (RigContainer activeRigContainer in (IEnumerable<RigContainer>) VRRigCache.ActiveRigContainers)
      {
        if ((activeRigContainer.Creator == null ? 0 : (activeRigContainer.Creator.UserId == gorilla.UserId ? 1 : 0)) != 0)
        {
          netPlayerByGorilla = activeRigContainer.Creator;
          goto label_11;
        }
      }
      netPlayerByGorilla = (NetPlayer) null;
    }
label_11:
    return netPlayerByGorilla;
  }

  public static VRRig GetRigByGorilla(GorillaData gorilla)
  {
    VRRig rigByGorilla;
    if (gorilla != null)
    {
      foreach (RigContainer activeRigContainer in (IEnumerable<RigContainer>) VRRigCache.ActiveRigContainers)
      {
        if ((activeRigContainer.Creator == null ? 0 : (activeRigContainer.Creator.UserId == gorilla.UserId ? 1 : 0)) != 0)
        {
          rigByGorilla = activeRigContainer.Rig;
          goto label_11;
        }
      }
      rigByGorilla = (VRRig) null;
    }
    else
      rigByGorilla = (VRRig) null;
label_11:
    return rigByGorilla;
  }

  public static GorillaData GetGorillaByRig(VRRig rig)
  {
    GorillaData gorillaByRig;
    if (((UnityEngine.Object) rig == (UnityEngine.Object) null))
    {
      gorillaByRig = (GorillaData) null;
    }
    else
    {
      NetPlayer netPlayerByRig = PlayerTranslator.GetNetPlayerByRig(rig);
      gorillaByRig = netPlayerByRig != null ? PlayerTranslator.GetGorillaByNetPlayer(netPlayerByRig) : (GorillaData) null;
    }
    return gorillaByRig;
  }

  public static VRRig GetRigByNetPlayer(NetPlayer player)
  {
    VRRig vrRig;
    return player != null ? (PlayerTranslator.vrrigDict.TryGetValue(player, out vrRig) ? vrRig : (VRRig) null) : (VRRig) null;
  }

  public static NetPlayer GetNetPlayerByRig(VRRig rig)
  {
    NetPlayer netPlayerByRig;
    if (!((UnityEngine.Object) rig == (UnityEngine.Object) null))
    {
      foreach (RigContainer activeRigContainer in (IEnumerable<RigContainer>) VRRigCache.ActiveRigContainers)
      {
        if (((UnityEngine.Object) activeRigContainer.Rig == (UnityEngine.Object) rig))
        {
          netPlayerByRig = activeRigContainer.Creator;
          goto label_11;
        }
      }
      netPlayerByRig = (NetPlayer) null;
    }
    else
      netPlayerByRig = (NetPlayer) null;
label_11:
    return netPlayerByRig;
  }

  public static NetPlayer GetNetPlayerByUserID(string userId)
  {
    NetPlayer netPlayerByUserId;
    foreach (RigContainer activeRigContainer in (IEnumerable<RigContainer>) VRRigCache.ActiveRigContainers)
    {
      if ((activeRigContainer.Creator == null ? 0 : (activeRigContainer.Creator.UserId == userId ? 1 : 0)) != 0)
      {
        netPlayerByUserId = activeRigContainer.Creator;
        goto label_9;
      }
    }
    netPlayerByUserId = (NetPlayer) null;
label_9:
    return netPlayerByUserId;
  }
}
