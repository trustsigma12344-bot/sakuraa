using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using SakuraaCastingMod.Shared.Helpers;

namespace SakuraaCastingMod.Core.Patches;

[HarmonyPatch(typeof(GorillaTagManager))]
[HarmonyPatch("ReportTag", MethodType.Normal)]
[HarmonyPriority(0)]
internal class TagSnitcher
{
	public static void Prefix(NetPlayer taggedPlayer, NetPlayer taggingPlayer, bool __runOriginal)
	{
		if (!__runOriginal || Networking.GtagManager.currentInfected.Contains(taggedPlayer) || !Networking.GtagManager.currentInfected.Contains(taggingPlayer))
		{
			return;
		}
		Player tagger = null;
		Player tagged = null;
		Player[] playerList = PhotonNetwork.PlayerList;
		foreach (Player player in playerList)
		{
			if (player.UserId == taggingPlayer.UserId)
			{
				tagger = player;
			}
			if (player.UserId == taggedPlayer.UserId)
			{
				tagged = player;
			}
		}
		TagEventManager.TriggerTagEvent(tagger, tagged);
	}
}
