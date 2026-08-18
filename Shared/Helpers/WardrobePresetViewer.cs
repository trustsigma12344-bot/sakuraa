using GorillaNetworking;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using SakMerge.Api;
using SakuraaCastingMod.Api;
using SakuraaCastingMod.Features.Overlays;
using SakuraaCastingMod.VR.UtilMenu;
using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Shared.Helpers;

public static class WardrobePresetViewer
{
  private static readonly AccessTools.FieldRef<CosmeticsController, CosmeticsController.CosmeticSet[]> SavedOutfitsRef = AccessTools.FieldRefAccess<CosmeticsController, CosmeticsController.CosmeticSet[]>("savedOutfits");
  private static readonly AccessTools.FieldRef<CosmeticsController, Vector3[]> SavedColorsRef = AccessTools.FieldRefAccess<CosmeticsController, Vector3[]>("savedColors");
  private static List<string> _names;
  private static string _selectedPreset;
  private static int _gtSlot = -1;
  private static float _lastColorNotify = -999f;

  private static List<string> Names
  {
    get => WardrobePresetViewer._names ?? (WardrobePresetViewer._names = SakuraaCastingMod.Api.Cosmetics.ListOutfits());
  }

  public static bool HasPresets => WardrobePresetViewer.Names.Count > 0;

  public static List<int> GetVisibleGtSlots()
  {
    List<int> intList = new List<int>();
    CosmeticsController instance = CosmeticsController.instance;
    List<int> visibleGtSlots;
    if ((((UnityEngine.Object) instance == (UnityEngine.Object) null) ? 1 : (!CosmeticsController.CanScrollOutfits() ? 1 : 0)) == 0)
    {
      CosmeticsController.CosmeticSet[] cosmeticSetArray = WardrobePresetViewer.SavedOutfitsRef.Invoke(instance);
      if (cosmeticSetArray != null)
      {
        bool flag = false;
        for (int index = 0; index < cosmeticSetArray.Length; ++index)
        {
          if ((cosmeticSetArray[index] == null ? 1 : (!cosmeticSetArray[index].HasAnyItems() ? 1 : 0)) == 0)
            intList.Add(index);
          else if (!flag)
          {
            intList.Add(index);
            flag = true;
          }
        }
        visibleGtSlots = intList;
      }
      else
        visibleGtSlots = intList;
    }
    else
      visibleGtSlots = intList;
    return visibleGtSlots;
  }

  public static string Label
  {
    get
    {
      List<string> names = WardrobePresetViewer.Names;
      List<int> visibleGtSlots = WardrobePresetViewer.GetVisibleGtSlots();
      int index = WardrobePresetViewer.EffectiveIndex(names, visibleGtSlots);
      string label;
      if (index < 0)
        label = names.Count == 1 ? "1 PRESET" : $"{names.Count} PRESETS";
      else if (index >= names.Count)
      {
        label = "OUTFIT #" + (visibleGtSlots[index - names.Count] + 1).ToString();
      }
      else
      {
        string upper = names[index].ToUpper();
        label = upper.Length > 20 ? upper.Substring(0, 19) + ".." : upper;
      }
      return label;
    }
  }

  public static bool Scroll(bool forward)
  {
    List<string> names = WardrobePresetViewer.Names;
    bool flag;
    if (names.Count == 0)
    {
      flag = false;
    }
    else
    {
      List<int> visibleGtSlots = WardrobePresetViewer.GetVisibleGtSlots();
      int num1 = names.Count + visibleGtSlots.Count;
      int num2 = WardrobePresetViewer.EffectiveIndex(names, visibleGtSlots);
      int index = forward ? (num2 + 1) % num1 : (num2 <= 0 ? num1 - 1 : num2 - 1);
      if (index >= names.Count)
        WardrobePresetViewer.SelectGtOutfit(visibleGtSlots[index - names.Count]);
      else if (!SakuraaCastingMod.Api.Cosmetics.LoadOutfit(names[index]))
        WardrobePresetViewer.MarkDirty();
      flag = true;
    }
    return flag;
  }

  public static void SelectGtOutfit(int slot)
  {
    WardrobePresetViewer._selectedPreset = (string) null;
    WardrobePresetViewer._gtSlot = slot;
    WardrobePresetViewer.ApplyGtOutfit(slot);
  }

  public static void NotifySelected(string name)
  {
    if (string.IsNullOrWhiteSpace(name))
      return;
    WardrobePresetViewer._selectedPreset = name.Trim();
    WardrobePresetViewer._gtSlot = -1;
    if (WardrobePresetViewer.IndexOf(WardrobePresetViewer._selectedPreset) >= 0)
      return;
    WardrobePresetViewer._names = (List<string>) null;
  }

  public static void MarkDirty()
  {
    WardrobePresetViewer._names = (List<string>) null;
    if (!((UnityEngine.Object) CosmeticsController.instance != (UnityEngine.Object) null))
      return;
    Action onOutfitsUpdated = CosmeticsController.instance.OnOutfitsUpdated;
    if (onOutfitsUpdated == null)
      return;
    onOutfitsUpdated();
  }

  public static void ApplyPlayerColor(float r, float g, float b)
  {
    VRRig offlineVrRig = ((UnityEngine.Object) GorillaTagger.Instance != (UnityEngine.Object) null) ? GorillaTagger.Instance.offlineVRRig : (VRRig) null;
    bool flag = ((UnityEngine.Object) offlineVrRig == (UnityEngine.Object) null) || (double) Mathf.Abs(offlineVrRig.playerColor.r - r) > 1.0 / 1000.0 || (double) Mathf.Abs(offlineVrRig.playerColor.g - g) > 1.0 / 1000.0 || (double) Mathf.Abs(offlineVrRig.playerColor.b - b) > 1.0 / 1000.0;
    Colors.Set(r, g, b);
    if ((!flag || WardrobePresetViewer.OthersWillSeeColorChange() ? 0 : ((double) Time.unscaledTime - (double) WardrobePresetViewer._lastColorNotify > 5.0 ? 1 : 0)) == 0)
      return;
    WardrobePresetViewer._lastColorNotify = Time.unscaledTime;
    Notification.Send("Get closer to the computer for your color code to update", Color.yellow, UtilMenuMain.Instance?.Icons?.ClothesHanger);
  }

  private static bool OthersWillSeeColorChange()
  {
    if ((((UnityEngine.Object) NetworkSystem.Instance == (UnityEngine.Object) null) ? 1 : (!NetworkSystem.Instance.InRoom ? 1 : 0)) != 0)
      return true;
    try
    {
      Player localPlayer = PhotonNetwork.LocalPlayer;
      GorillaComputer instance = GorillaComputer.instance;
      return (localPlayer == null || !((UnityEngine.Object) instance != (UnityEngine.Object) null) || !((UnityEngine.Object) instance.friendJoinCollider != (UnityEngine.Object) null) ? 0 : (instance.friendJoinCollider.playerIDsCurrentlyTouching.Contains(localPlayer.UserId) ? 1 : 0)) != 0 || localPlayer != null && CosmeticWardrobeProximityDetector.IsUserNearWardrobe(localPlayer.ActorNumber);
    }
    catch
    {
      return true;
    }
  }

  private static int EffectiveIndex(List<string> names, List<int> gtSlots)
  {
    int num1;
    if (WardrobePresetViewer._selectedPreset != null)
    {
      int num2 = WardrobePresetViewer.IndexOf(WardrobePresetViewer._selectedPreset);
      if (num2 >= 0)
      {
        num1 = num2;
        goto label_10;
      }
    }
    if (WardrobePresetViewer._gtSlot >= 0)
    {
      int visible = WardrobePresetViewer.MapSlotToVisible(gtSlots, WardrobePresetViewer._gtSlot);
      if (visible >= 0)
      {
        num1 = names.Count + visible;
        goto label_10;
      }
    }
    if ((WardrobePresetViewer._selectedPreset != null || WardrobePresetViewer._gtSlot >= 0 ? 0 : (gtSlots.Count > 0 ? 1 : 0)) != 0)
    {
      int visible = WardrobePresetViewer.MapSlotToVisible(gtSlots, CosmeticsController.SelectedOutfit);
      if (visible >= 0)
      {
        num1 = names.Count + visible;
        goto label_10;
      }
    }
    num1 = -1;
label_10:
    return num1;
  }

  private static int MapSlotToVisible(List<int> gtSlots, int slot)
  {
    int num = gtSlots.IndexOf(slot);
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    return num < 0 ? gtSlots.FindIndex(new Predicate<int>(WardrobePresetViewer.IsGtSlotEmpty)) : num;
  }

  private static bool IsGtSlotEmpty(int slot)
  {
    CosmeticsController instance = CosmeticsController.instance;
    CosmeticsController.CosmeticSet[] cosmeticSetArray = ((UnityEngine.Object) instance != (UnityEngine.Object) null) ? WardrobePresetViewer.SavedOutfitsRef.Invoke(instance) : (CosmeticsController.CosmeticSet[]) null;
    if (cosmeticSetArray == null || slot < 0 || slot >= cosmeticSetArray.Length)
      return false;
    return cosmeticSetArray[slot] == null || !cosmeticSetArray[slot].HasAnyItems();
  }

  private static void ApplyGtOutfit(int slot)
  {
    CosmeticsController instance = CosmeticsController.instance;
    if (((UnityEngine.Object) instance == (UnityEngine.Object) null))
      return;
    CosmeticsController.CosmeticSet[] cosmeticSetArray = WardrobePresetViewer.SavedOutfitsRef.Invoke(instance);
    if ((cosmeticSetArray == null || slot < 0 || slot >= cosmeticSetArray.Length ? 1 : (cosmeticSetArray[slot] == null ? 1 : 0)) != 0)
      return;
    instance.currentWornSet.CopyItems(cosmeticSetArray[slot]);
    Vector3[] vector3Array = WardrobePresetViewer.SavedColorsRef.Invoke(instance);
    if ((vector3Array == null ? 0 : (slot < vector3Array.Length ? 1 : 0)) != 0)
      WardrobePresetViewer.ApplyPlayerColor(vector3Array[slot].x, vector3Array[slot].y, vector3Array[slot].z);
    CosmeticsHelper.ApplyAndRefresh();
  }

  private static int IndexOf(string name)
  {
    return WardrobePresetViewer.Names.FindIndex((Predicate<string>) (n => string.Equals(n, name, StringComparison.OrdinalIgnoreCase)));
  }
}
