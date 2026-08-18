using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Features.Replay.Recording;

public class TagTracker
{
  private List<int> userIdsTaggedLastUpdate = new List<int>();

  public void Reset() => this.userIdsTaggedLastUpdate = new List<int>();

  public void Update(SakuraaCastingMod.Features.Replay.Recording.Replay replay, GorillaTagManager gtm, int replayTime)
  {
    foreach (PlayerData playerData in replay.playerDatas)
    {
      if (playerData.tagstimes.Count == 0)
      {
        playerData.tagstimes.Add(replayTime);
        playerData.tagstimes.Add(0);
      }
    }
    if (((UnityEngine.Object) gtm == (UnityEngine.Object) null))
      return;
    foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList)
    {
      NetPlayer netPlayer = (player);
      if ((this.userIdsTaggedLastUpdate.Contains(netPlayer.ActorNumber) ? 0 : (gtm.currentInfected.Contains(netPlayer) ? 1 : (gtm.currentIt == netPlayer ? 1 : 0))) != 0)
      {
        foreach (PlayerData playerData in replay.playerDatas)
        {
          if (playerData.actorNumber == netPlayer.ActorNumber)
          {
            playerData.tagstimes.Add(replayTime);
            playerData.tagstimes.Add(1);
          }
        }
      }
      else if ((!this.userIdsTaggedLastUpdate.Contains(netPlayer.ActorNumber) ? 0 : (gtm.currentInfected.Contains(netPlayer) ? 0 : (gtm.currentIt != netPlayer ? 1 : 0))) != 0)
      {
        foreach (PlayerData playerData in replay.playerDatas)
        {
          if (playerData.actorNumber == netPlayer.ActorNumber)
          {
            playerData.tagstimes.Add(replayTime);
            playerData.tagstimes.Add(0);
          }
        }
      }
    }
    this.userIdsTaggedLastUpdate.Clear();
    foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList)
    {
      NetPlayer netPlayer = (player);
      if ((gtm.currentInfected.Contains(netPlayer) ? 1 : (gtm.currentIt == netPlayer ? 1 : 0)) != 0)
        this.userIdsTaggedLastUpdate.Add(netPlayer.ActorNumber);
    }
  }
}
