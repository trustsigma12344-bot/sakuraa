using HarmonyLib;
using SakuraaCastingMod.Features.AutoRef;

namespace SakuraaCastingMod.Core.Patches;

[HarmonyPatch(typeof(GorillaTagManager))]
[HarmonyPatch("ReportTag", MethodType.Normal)]
[HarmonyPriority(800)]
internal class AntiTeamTagPatch
{
	public static bool Prefix(NetPlayer taggedPlayer, NetPlayer taggingPlayer)
	{
		AutoRefManager instance = AutoRefManager.Instance;
		if (instance == null || !instance.IsActive)
		{
			return true;
		}
		if (instance.Spectators.Contains(taggingPlayer))
		{
			return false;
		}
		if (!instance.IsExtraPlayer(taggingPlayer))
		{
			bool flag = instance.Team1Players.Contains(taggedPlayer);
			bool flag2 = instance.Team1Players.Contains(taggingPlayer);
			bool flag3 = instance.Team2Players.Contains(taggedPlayer);
			bool flag4 = instance.Team2Players.Contains(taggingPlayer);
			if (flag & flag2)
			{
				return false;
			}
			if (!(flag3 & flag4))
			{
				return true;
			}
			return false;
		}
		return false;
	}
}
