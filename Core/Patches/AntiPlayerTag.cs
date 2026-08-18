using System.Reflection;
using HarmonyLib;
using SakuraaCastingMod.Features.AutoRef;

namespace SakuraaCastingMod.Core.Patches;

[HarmonyPatch]
internal class AntiPlayerTag
{
	private static MethodBase TargetMethod()
	{
		return AccessTools.Method(AccessTools.TypeByName("GameModeSerializer"), "ReportTag");
	}

	private static bool Prefix()
	{
		if (!(AutoRefManager.Instance == null) && AutoRefManager.Instance.IsActive)
		{
			if (AutoRefManager.Instance.currentState == AutoRefManager.GameStates.Countdown)
			{
				return false;
			}
			if (AutoRefManager.Instance.currentState == AutoRefManager.GameStates.RoundStart)
			{
				return false;
			}
			return true;
		}
		return true;
	}
}
