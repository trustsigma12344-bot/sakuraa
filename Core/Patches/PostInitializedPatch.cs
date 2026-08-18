using HarmonyLib;

namespace SakuraaCastingMod.Core.Patches;

[HarmonyPatch(typeof(GorillaTagger))]
[HarmonyPatch("Start", MethodType.Normal)]
internal static class PostInitializedPatch
{
	private static void Postfix()
	{
		UtillaNetworkController.events.TriggerGameInitialized();
	}
}
