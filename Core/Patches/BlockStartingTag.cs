using System;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using SakuraaCastingMod.Features.AutoRef;
using UnityEngine;

namespace SakuraaCastingMod.Core.Patches;

[HarmonyPatch(typeof(GorillaTagManager))]
[HarmonyPatch("AddInfectedPlayer", MethodType.Normal)]
[HarmonyPatch(new Type[]
{
	typeof(NetPlayer),
	typeof(bool)
})]
internal class BlockStartingTag
{
	public static bool Prefix(GorillaTagManager __instance, ref NetPlayer infectedPlayer, bool withTagStop)
	{
		if (!(AutoRefManager.Instance == null) && AutoRefManager.Instance.IsActive)
		{
			if (__instance.currentInfected.Count == 0)
			{
				infectedPlayer = PhotonNetwork.LocalPlayer;
				return true;
			}
			if (!((double)Time.time < __instance.timeInfectedGameEnded + 7.0))
			{
				return true;
			}
			if (PhotonNetwork.CurrentRoom.PlayerCount >= 4 && !__instance.currentInfected.Contains(PhotonNetwork.LocalPlayer))
			{
				Player localPlayer = PhotonNetwork.LocalPlayer;
				VRRig vRRig = __instance.FindPlayerVRRig(localPlayer);
				if (vRRig != null)
				{
					infectedPlayer = localPlayer;
					return true;
				}
			}
			return false;
		}
		return true;
	}
}
