using HarmonyLib;

namespace SakuraaCastingMod.Core.Patches;

[HarmonyPatch(typeof(GorillaTagger))]
[HarmonyPatch("Start", MethodType.Normal)]
internal static class InitializedPatch
{
	public static void Postfix()
	{
	}
}
