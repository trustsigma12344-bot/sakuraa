using HarmonyLib;
using SakuraaCastingMod.Shared.Helpers;
using TMPro;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Core.Patches;

[HarmonyPatch(typeof (CosmeticWardrobe))]
internal static class WardrobeOutfitPatch
{
  private static readonly AccessTools.FieldRef<CosmeticWardrobe, CosmeticButton> PrevButton = AccessTools.FieldRefAccess<CosmeticWardrobe, CosmeticButton>("previousOutfit");
  private static readonly AccessTools.FieldRef<CosmeticWardrobe, CosmeticButton> NextButton = AccessTools.FieldRefAccess<CosmeticWardrobe, CosmeticButton>("nextOutfit");
  private static readonly AccessTools.FieldRef<CosmeticWardrobe, TMP_Text> OutfitText = AccessTools.FieldRefAccess<CosmeticWardrobe, TMP_Text>("outfitText");

  [HarmonyPrefix]
  [HarmonyPatch("HandlePressedPrevOutfitButton")]
  private static bool PrevPressed() => !WardrobePresetViewer.Scroll(false);

  [HarmonyPrefix]
  [HarmonyPatch("HandlePressedNextOutfitButton")]
  private static bool NextPressed() => !WardrobePresetViewer.Scroll(true);

  [HarmonyPrefix]
  [HarmonyPatch("UpdateOutfitButtons")]
  private static bool UpdateOutfitButtons(CosmeticWardrobe __instance)
  {
    bool flag;
    if (WardrobePresetViewer.HasPresets)
    {
      CosmeticButton cosmeticButton1 = WardrobeOutfitPatch.PrevButton.Invoke(__instance);
      CosmeticButton cosmeticButton2 = WardrobeOutfitPatch.NextButton.Invoke(__instance);
      ((Behaviour) cosmeticButton1).enabled = true;
      ((Behaviour) cosmeticButton2).enabled = true;
      ((GorillaPressableButton) cosmeticButton1).UpdateColor();
      ((GorillaPressableButton) cosmeticButton2).UpdateColor();
      WardrobeOutfitPatch.OutfitText.Invoke(__instance).text = WardrobePresetViewer.Label;
      flag = false;
    }
    else
      flag = true;
    return flag;
  }
}
