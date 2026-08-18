using ExitGames.Client.Photon;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using SakuraaCastingMod.Shared.Helpers;
using System;

#nullable disable
namespace SakuraaCastingMod.Core.Patches;

[HarmonyPatch(typeof (PhotonNetwork), "RaiseEvent", new Type[] {typeof (byte), typeof (object), typeof (RaiseEventOptions), typeof (SendOptions)})]
internal class EventPatch
{
  public static void Postfix(
    byte eventCode,
    object eventContent,
    RaiseEventOptions raiseEventOptions,
    SendOptions sendOptions)
  {
    if ((eventCode == (byte) 2 ? 1 : (eventCode == (byte) 1 ? 1 : 0)) == 0)
      return;
    object[] objArray = (object[]) eventContent;
    string str1 = (string) objArray[0];
    string str2 = (string) objArray[1];
    Player tagger = (Player) null;
    Player tagged = (Player) null;
    foreach (Player player in PhotonNetwork.PlayerList)
    {
      if (player.UserId == str1)
        tagger = player;
      if (player.UserId == str2)
        tagged = player;
    }
    TagEventManager.TriggerTagEvent(tagger, tagged);
  }
}
