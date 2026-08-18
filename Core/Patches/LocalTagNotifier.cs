using HarmonyLib;
using SakuraaCastingMod.Features.Soundboard;

namespace SakuraaCastingMod.Core.Patches;

[HarmonyPatch(typeof(GorillaTagManager))]
[HarmonyPatch("LocalTag", MethodType.Normal)]
internal class LocalTagNotifier
{
	public static void Postfix(NetPlayer taggedPlayer, NetPlayer taggingPlayer, bool __runOriginal)
	{
		if (__runOriginal && taggingPlayer != null && taggedPlayer != null && taggingPlayer.IsLocal && !taggedPlayer.IsLocal)
		{
			SoundboardTriggers.NotifyLocalTag();
		}
	}
}
