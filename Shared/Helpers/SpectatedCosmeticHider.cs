using GorillaNetworking;
using System;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Shared.Helpers;

public static class SpectatedCosmeticHider
{
  private static VRRig _hiddenRig;

  public static void HideFor(VRRig rig)
  {
    if (!((UnityEngine.Object) rig == (UnityEngine.Object) null))
    {
      if ((!((UnityEngine.Object) SpectatedCosmeticHider._hiddenRig != (UnityEngine.Object) null) ? 0 : (((UnityEngine.Object) SpectatedCosmeticHider._hiddenRig != (UnityEngine.Object) rig) ? 1 : 0)) != 0)
        SpectatedCosmeticHider.SetRigCosmetics(SpectatedCosmeticHider._hiddenRig, true);
      SpectatedCosmeticHider._hiddenRig = rig;
      SpectatedCosmeticHider.SetRigCosmetics(rig, false);
    }
    else
      SpectatedCosmeticHider.Restore();
  }

  public static void Restore()
  {
    if (((UnityEngine.Object) SpectatedCosmeticHider._hiddenRig != (UnityEngine.Object) null))
      SpectatedCosmeticHider.SetRigCosmetics(SpectatedCosmeticHider._hiddenRig, true);
    SpectatedCosmeticHider._hiddenRig = (VRRig) null;
  }

  private static void SetRigCosmetics(VRRig rig, bool enable)
  {
    if ((((UnityEngine.Object) rig == (UnityEngine.Object) null) || rig.cosmeticSet?.items == null ? 1 : (rig.cosmeticsObjectRegistry == null ? 1 : 0)) != 0)
      return;
    foreach (CosmeticsController.CosmeticItem cosmeticItem in rig.cosmeticSet.items)
    {
      if (!cosmeticItem.isNullItem)
        SpectatedCosmeticHider.SetItemVisibility(rig, cosmeticItem, enable);
    }
  }

  private static void SetItemVisibility(
    VRRig rig,
    CosmeticsController.CosmeticItem item,
    bool enable)
  {
    CosmeticItemRegistry cosmeticsObjectRegistry = rig.cosmeticsObjectRegistry;
    CosmeticItemInstance cosmeticItemInstance = cosmeticsObjectRegistry.Cosmetic(item.displayName) ?? cosmeticsObjectRegistry.Cosmetic(item.itemName);
    if (cosmeticItemInstance == null)
      return;
    CosmeticsController.CosmeticCategory itemCategory = item.itemCategory;
    CosmeticsController.CosmeticSlots cosmeticSlots;
    switch (itemCategory - 1)
    {
      case 0:
        cosmeticSlots = (CosmeticsController.CosmeticSlots) 0;
        break;
      case (GorillaNetworking.CosmeticsController.CosmeticCategory) 1:
        cosmeticSlots = (CosmeticsController.CosmeticSlots) 1;
        break;
      case (GorillaNetworking.CosmeticsController.CosmeticCategory) 2:
        cosmeticSlots = (CosmeticsController.CosmeticSlots) 2;
        break;
      case (GorillaNetworking.CosmeticsController.CosmeticCategory) 3:
      case (GorillaNetworking.CosmeticsController.CosmeticCategory) 4:
      case (GorillaNetworking.CosmeticsController.CosmeticCategory) 5:
        try
        {
          cosmeticSlots = (CosmeticsController.CosmeticSlots) Enum.Parse(typeof (CosmeticsController.CosmeticSlots), item.itemCategory.ToString());
          break;
        }
        catch
        {
          return;
        }
      case (GorillaNetworking.CosmeticsController.CosmeticCategory) 6:
        cosmeticSlots = (CosmeticsController.CosmeticSlots) 11;
        break;
      default:
        if (itemCategory == (CosmeticsController.CosmeticCategory) 10)
        {
          cosmeticSlots = (CosmeticsController.CosmeticSlots) 12;
          break;
        }
        goto case 3;
    }
    if (enable)
      cosmeticItemInstance.EnableItem(cosmeticSlots, rig);
    else
      cosmeticItemInstance.DisableItem(cosmeticSlots);
  }
}
