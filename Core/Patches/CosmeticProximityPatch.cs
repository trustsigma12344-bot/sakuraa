using HarmonyLib;

#nullable disable
namespace SakuraaCastingMod.Core.Patches;

[HarmonyPatch(typeof (CosmeticWardrobeProximityDetector), "IsUserNearWardrobe")]
public class CosmeticProximityPatch
{
  private static bool Prefix(CosmeticWardrobeProximityDetector __instance, int actorNr) => true;
}
