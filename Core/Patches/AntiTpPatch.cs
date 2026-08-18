using GorillaLocomotion;
using HarmonyLib;
using SakuraaCastingMod.Features.AutoRef;

namespace SakuraaCastingMod.Core.Patches;

[HarmonyPatch(typeof(GTPlayer))]
[HarmonyPatch("AntiTeleportTechnology", MethodType.Normal)]
internal class AntiTpPatch
{
	public static bool Prefix()
	{
		if (AutoRefManager.Instance == null || !AutoRefManager.Instance.IsActive)
		{
			return true;
		}
		return false;
	}
}
