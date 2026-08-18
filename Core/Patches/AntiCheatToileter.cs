using HarmonyLib;
using SakuraaCastingMod.Features.AutoRef;

namespace SakuraaCastingMod.Core.Patches;

[HarmonyPatch(typeof(MonkeAgent))]
[HarmonyPatch("DispatchReport", MethodType.Normal)]
internal class AntiCheatToileter
{
	private static bool Prefix()
	{
		if (AutoRefManager.Instance == null || !AutoRefManager.Instance.IsActive)
		{
			return true;
		}
		return false;
	}
}
