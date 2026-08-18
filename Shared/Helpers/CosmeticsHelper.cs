using GorillaNetworking;
using System;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Shared.Helpers;

public static class CosmeticsHelper
{
  public static void ApplyAndRefresh()
  {
    if (((UnityEngine.Object) CosmeticsController.instance == (UnityEngine.Object) null))
      return;
    CosmeticsController.instance.UpdateWornCosmetics(true);
    Action cosmeticsUpdated = CosmeticsController.instance.OnCosmeticsUpdated;
    if (cosmeticsUpdated == null)
      return;
    cosmeticsUpdated();
  }
}
