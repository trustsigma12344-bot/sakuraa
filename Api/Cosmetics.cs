using GorillaNetworking;
using SakuraaCastingMod.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Api;

[Browsable(true)]
public static class Cosmetics
{
  public static List<string> ListOutfits()
  {
    return Cosmetics.LoadProfiles().Where<CosmeticProfile>((Func<CosmeticProfile, bool>) (p => p != null && !string.IsNullOrWhiteSpace(p.ProfileName))).Select<CosmeticProfile, string>((Func<CosmeticProfile, string>) (p => p.ProfileName.Trim())).ToList<string>();
  }

  public static bool LoadOutfit(string name)
  {
    CosmeticsController instance = CosmeticsController.instance;
    bool flag;
    if ((((UnityEngine.Object) instance == (UnityEngine.Object) null) ? 1 : (string.IsNullOrWhiteSpace(name) ? 1 : 0)) == 0)
    {
      CosmeticProfile profile = Cosmetics.LoadProfiles().FirstOrDefault<CosmeticProfile>((Func<CosmeticProfile, bool>) (p => p != null && string.Equals(p.ProfileName, name.Trim(), StringComparison.OrdinalIgnoreCase)));
      if (profile == null)
      {
        flag = false;
      }
      else
      {
        WardrobePresetViewer.NotifySelected(profile.ProfileName);
        Cosmetics.ApplyProfile(instance, profile);
        flag = true;
      }
    }
    else
      flag = false;
    return flag;
  }

  public static bool ToggleCosmetic(string name)
  {
    CosmeticsController instance = CosmeticsController.instance;
    bool flag;
    if ((((UnityEngine.Object) instance == (UnityEngine.Object) null) ? 1 : (string.IsNullOrWhiteSpace(name) ? 1 : 0)) == 0)
    {
      CosmeticsController.CosmeticItem ownedItem = Cosmetics.FindOwnedItem(instance, name.Trim());
      if (ownedItem.isNullItem)
      {
        flag = false;
      }
      else
      {
        instance.ApplyCosmeticItemToSet(instance.currentWornSet, ownedItem, false, true);
        CosmeticsHelper.ApplyAndRefresh();
        flag = true;
      }
    }
    else
      flag = false;
    return flag;
  }

  private static CosmeticsController.CosmeticItem FindOwnedItem(
    CosmeticsController cc,
    string query)
  {
    CosmeticsController.CosmeticItem cosmeticItem = cc.nullItem;
    CosmeticsController.CosmeticItem ownedItem;
    foreach (CosmeticsController.CosmeticItem unlockedCosmetic in cc.unlockedCosmetics)
    {
      if (Cosmetics.IsWearable(unlockedCosmetic))
      {
        if (!string.Equals(Cosmetics.CanonicalName(cc, unlockedCosmetic), query, StringComparison.OrdinalIgnoreCase))
        {
          if ((!cosmeticItem.isNullItem ? 0 : (Cosmetics.Matches(unlockedCosmetic, query) ? 1 : 0)) != 0)
            cosmeticItem = unlockedCosmetic;
        }
        else
        {
          ownedItem = unlockedCosmetic;
          goto label_10;
        }
      }
    }
    ownedItem = cosmeticItem;
label_10:
    return ownedItem;
  }

  private static string CanonicalName(CosmeticsController cc, CosmeticsController.CosmeticItem item)
  {
    string itemDisplayName = cc.GetItemDisplayName(item);
    return !string.IsNullOrEmpty(itemDisplayName) ? itemDisplayName : (string.IsNullOrEmpty(item.overrideDisplayName) ? item.itemName : item.overrideDisplayName);
  }

  public static bool ClearAll()
  {
    CosmeticsController instance = CosmeticsController.instance;
    bool flag;
    if (((UnityEngine.Object) instance == (UnityEngine.Object) null))
    {
      flag = false;
    }
    else
    {
      instance.currentWornSet.ClearSet(instance.nullItem);
      CosmeticsHelper.ApplyAndRefresh();
      flag = true;
    }
    return flag;
  }

  public static void Randomize()
  {
    CosmeticsController instance = CosmeticsController.instance;
    if (((UnityEngine.Object) instance == (UnityEngine.Object) null))
      return;
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    List<CosmeticsController.CosmeticItem> list = instance.unlockedCosmetics.Where<CosmeticsController.CosmeticItem>(new Func<CosmeticsController.CosmeticItem, bool>(Cosmetics.IsWearable)).ToList<CosmeticsController.CosmeticItem>();
    if (list.Count == 0)
      return;
    instance.currentWornSet.ClearSet(instance.nullItem);
    foreach (CosmeticsController.CosmeticItem cosmeticItem in list)
    {
      if ((double) UnityEngine.Random.value > 0.5)
        instance.ApplyCosmeticItemToSet(instance.currentWornSet, cosmeticItem, (double) UnityEngine.Random.value > 0.5, true);
    }
    CosmeticsHelper.ApplyAndRefresh();
  }

  public static bool ClonePlayer(NetPlayer player)
  {
    return player != null && Cosmetics.CloneRig(PlayerTranslator.GetRigByNetPlayer(player));
  }

  public static bool ClonePlayer(string playerName)
  {
    return Cosmetics.CloneRig(ApiLookup.FindRigByName(playerName));
  }

  private static bool CloneRig(VRRig sourceRig)
  {
    CosmeticsController instance = CosmeticsController.instance;
    bool flag1;
    if ((((UnityEngine.Object) instance == (UnityEngine.Object) null) ? 1 : (((UnityEngine.Object) sourceRig == (UnityEngine.Object) null) ? 1 : 0)) == 0)
    {
      instance.currentWornSet.ClearSet(instance.nullItem);
      CosmeticsController.CosmeticItem[] items1 = sourceRig.cosmeticSet.items;
      CosmeticsController.CosmeticItem[] items2 = instance.currentWornSet.items;
      for (int index = 0; (index >= items1.Length ? 0 : (index < items2.Length ? 1 : 0)) != 0; ++index)
      {
        if (!items1[index].isNullItem)
        {
          CosmeticsController.CosmeticItem localItem = instance.GetItemFromDict(items1[index].itemName);
          bool flag2 = instance.unlockedCosmetics.Any<CosmeticsController.CosmeticItem>((Func<CosmeticsController.CosmeticItem, bool>) (x => x.itemName == localItem.itemName));
          if (!localItem.isNullItem & flag2)
            items2[index] = localItem;
        }
      }
      CosmeticsHelper.ApplyAndRefresh();
      flag1 = true;
    }
    else
      flag1 = false;
    return flag1;
  }

  private static void ApplyProfile(CosmeticsController cc, CosmeticProfile profile)
  {
    cc.currentWornSet.ClearSet(cc.nullItem);
    CosmeticsController.CosmeticItem[] items = cc.currentWornSet.items;
    if ((profile.SidedItems == null ? 0 : (profile.SidedItems.Count > 0 ? 1 : 0)) != 0)
    {
      foreach (SavedCosmeticItem sidedItem in profile.SidedItems)
      {
        CosmeticsController.CosmeticItem itemFromDict = cc.GetItemFromDict(sidedItem.Id);
        if ((itemFromDict.isNullItem || sidedItem.Index < 0 ? 0 : (sidedItem.Index < items.Length ? 1 : 0)) != 0)
          items[sidedItem.Index] = itemFromDict;
      }
    }
    else if (profile.ItemIds != null)
    {
      foreach (string itemId in profile.ItemIds)
      {
        CosmeticsController.CosmeticItem itemFromDict = cc.GetItemFromDict(itemId);
        if (!itemFromDict.isNullItem)
          cc.ApplyCosmeticItemToSet(cc.currentWornSet, itemFromDict, false, true);
      }
    }
    if ((profile.Color == null ? 0 : (profile.Color.Length >= 3 ? 1 : 0)) != 0)
      WardrobePresetViewer.ApplyPlayerColor(profile.Color[0], profile.Color[1], profile.Color[2]);
    CosmeticsHelper.ApplyAndRefresh();
  }

  private static bool IsWearable(CosmeticsController.CosmeticItem item)
  {
    return (item.isNullItem || string.IsNullOrEmpty(item.itemName) ? 1 : (item.isThrowable ? 1 : 0)) == 0 && (item.itemCategory == null ? 1 : (item.itemCategory == (CosmeticsController.CosmeticCategory) 12 ? 1 : 0)) == 0 && (item.itemCategory != (CosmeticsController.CosmeticCategory) 13 ? 0 : (item.bundledItems == null ? 1 : 0)) == 0;
  }

  private static bool Matches(CosmeticsController.CosmeticItem item, string query)
  {
    return Cosmetics.ContainsIgnoreCase(item.overrideDisplayName, query) || Cosmetics.ContainsIgnoreCase(item.displayName, query) || Cosmetics.ContainsIgnoreCase(item.itemName, query);
  }

  private static bool ContainsIgnoreCase(string value, string query)
  {
    return value != null && value.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
  }

  private static List<CosmeticProfile> LoadProfiles() => CosmeticProfileStore.LoadProfiles();
}
