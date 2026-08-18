using HarmonyLib;
using Photon.Pun;
using SakuraaCastingMod.Features.AutoRef;

namespace SakuraaCastingMod.Core.Patches;

[HarmonyPatch(typeof(GorillaTagManager))]
[HarmonyPatch("NewVRRig", MethodType.Normal)]
internal class AntiJoinTag
{
	public static bool Prefix(GorillaTagManager __instance, NetPlayer player, int vrrigPhotonViewID, bool didTutorial)
	{
		if (!(AutoRefManager.Instance == null) && AutoRefManager.Instance.IsActive)
		{
			if (PhotonNetwork.IsMasterClient && !__instance.isCurrentlyTag)
			{
				__instance.UpdateState();
				if (!__instance.isCurrentlyTag)
				{
					if (player == (NetPlayer)PhotonNetwork.LocalPlayer || AutoRefManager.Instance.currentState == AutoRefManager.GameStates.MidRound)
					{
						__instance.AddInfectedPlayer(player);
					}
					__instance.UpdateInfectionState();
				}
			}
			return false;
		}
		return true;
	}
}
