using GorillaNetworking;
using SakuraaCastingMod.Core;
using SakuraaCastingMod.Shared.Helpers;
using SakuraaCastingMod.Shared.Models;
using SakuraaCastingMod.VR.Tablet;
using SakuraaCastingMod.VR.UtilMenu.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.VR.UtilMenu.Pages;

public class CosmeticsPage : BasePage
{
  public static CosmeticsPage Instance;
  private bool _profilesLoaded = false;
  private string _newProfileName = "";
  private bool _isNamingProfile = false;
  private int _profilePageOffset = 0;
  private List<CosmeticProfile> _profiles = new List<CosmeticProfile>();
  [SavedSetting("HideCosmetics", false)]
  public static bool IsHideCosmeticsEnabled = false;
  [SavedSetting("HiddenCosmeticCategories", "Hat;Face")]
  private static string _hiddenCategoriesString = "Hat;Face";
  private bool _wasHiddenFrame = false;
  private bool _viewingHiddenTypes = false;
  private int _hiddenTypesPage = 0;
  private static HashSet<CosmeticsController.CosmeticCategory> _categoriesToHide = new HashSet<CosmeticsController.CosmeticCategory>();

  public override string PageName => "COSMETICS";

  public override Material PageIcon => UtilMenuMain.Instance.Icons.ClothesHanger;

  public override void Start()
  {
    CosmeticsPage.Instance = this;
    this.LoadHiddenCategories();
    this.LoadProfilesFromFile();
    this._profilesLoaded = true;
    if (((UnityEngine.Object) CosmeticsController.instance != (UnityEngine.Object) null))
      CosmeticsController.instance.OnOutfitsUpdated += new Action(this.OnGtOutfitsUpdated);
    this.RefreshProfilesTab();
    if (!((UnityEngine.Object) UtilMenuController.Instance != (UnityEngine.Object) null))
      return;
    UtilMenuController.Instance.RefreshUI();
  }

  private void OnGtOutfitsUpdated()
  {
    this.RefreshProfilesTab();
    if (!((UnityEngine.Object) UtilMenuController.Instance != (UnityEngine.Object) null))
      return;
    UtilMenuController.Instance.RefreshUI();
  }

  private void LoadHiddenCategories()
  {
    CosmeticsPage._categoriesToHide.Clear();
    if (string.IsNullOrEmpty(CosmeticsPage._hiddenCategoriesString))
      return;
    foreach (string str in CosmeticsPage._hiddenCategoriesString.Split(';'))
    {
      CosmeticsController.CosmeticCategory result;
      if (Enum.TryParse<CosmeticsController.CosmeticCategory>(str, out result))
        CosmeticsPage._categoriesToHide.Add(result);
    }
  }

  private void SaveHiddenCategories()
  {
    CosmeticsPage._hiddenCategoriesString = string.Join<CosmeticsController.CosmeticCategory>(";", (IEnumerable<CosmeticsController.CosmeticCategory>) CosmeticsPage._categoriesToHide);
    Configuration.SaveSettings();
  }

  public override void BuildTabs()
  {
    this.Tabs.Add(new UtilTab()
    {
      TabIcon = UtilMenuMain.Instance.Icons.ClothesHanger,
      TabName = "Profiles"
    });
    this.AddTab(UtilMenuMain.Instance.Icons.Options);
    this.RefreshProfilesTab();
    this.RefreshOptionsTab();
  }

  private void RefreshOptionsTab()
  {
    if (this.Tabs.Count < 2)
      return;
    UtilTab tab = this.Tabs[1];
    tab.Elements.Clear();
    if (this._viewingHiddenTypes)
    {
      tab.Elements.Add(new MenuElement("<< BACK", (Action) (() =>
      {
        this._viewingHiddenTypes = false;
        this.RefreshOptionsTab();
        UtilMenuController.Instance.RefreshUI();
      })));
      List<CosmeticsController.CosmeticCategory> list = Enum.GetValues(typeof (CosmeticsController.CosmeticCategory)).Cast<CosmeticsController.CosmeticCategory>().Where<CosmeticsController.CosmeticCategory>((Func<CosmeticsController.CosmeticCategory, bool>) (c => c != null && c != (CosmeticsController.CosmeticCategory) 12 && c != (CosmeticsController.CosmeticCategory) 13)).ToList<CosmeticsController.CosmeticCategory>();
      int totalPages = Mathf.CeilToInt((float) list.Count / 4f);
      if (this._hiddenTypesPage >= totalPages)
        this._hiddenTypesPage = 0;
      int num1 = this._hiddenTypesPage * 4;
      int num2 = Mathf.Min(4, list.Count - num1);
      for (int index = 0; index < num2; ++index)
      {
        CosmeticsController.CosmeticCategory cat = list[num1 + index];
        bool isHidden = CosmeticsPage._categoriesToHide.Contains(cat);
        string str = cat.ToString().ToUpper();
        if (str == "ARMS")
          str = "HOLDABLES";
        tab.Elements.Add(new MenuElement($"HIDE {str}: {(isHidden ? "ON" : "OFF")}", (Action) (() =>
        {
          if (isHidden)
          {
            CosmeticsPage._categoriesToHide.Remove(cat);
            if (CosmeticsPage.IsHideCosmeticsEnabled)
              this.SetCategoryVisibility(cat, true);
          }
          else
          {
            CosmeticsPage._categoriesToHide.Add(cat);
            if (CosmeticsPage.IsHideCosmeticsEnabled)
              this.SetCategoryVisibility(cat, false);
          }
          this.SaveHiddenCategories();
          this.RefreshOptionsTab();
          UtilMenuController.Instance.RefreshUI();
        }), isHidden)
        {
          Type = ElementType.Toggle
        });
      }
      if (totalPages <= 1)
        return;
      tab.Elements.Add(new MenuElement($"PAGE {this._hiddenTypesPage + 1}/{totalPages}", "", (Action) (() =>
      {
        --this._hiddenTypesPage;
        if (this._hiddenTypesPage < 0)
          this._hiddenTypesPage = totalPages - 1;
        this.RefreshOptionsTab();
        UtilMenuController.Instance.RefreshUI();
      }), (Action) (() =>
      {
        ++this._hiddenTypesPage;
        if (this._hiddenTypesPage >= totalPages)
          this._hiddenTypesPage = 0;
        this.RefreshOptionsTab();
        UtilMenuController.Instance.RefreshUI();
      }))
      {
        Type = ElementType.Slider
      });
    }
    else
    {
      tab.Elements.Add(new MenuElement("HIDE COSMETICS IN FPV", (Action) null)
      {
        Type = ElementType.Toggle,
        IsToggled = CosmeticsPage.IsHideCosmeticsEnabled,
        OnToggle = new Action(this.ToggleCosmetics)
      });
      tab.Elements.Add(new MenuElement("CONFIGURE HIDDEN TYPES >>", (Action) (() =>
      {
        this._viewingHiddenTypes = true;
        this._hiddenTypesPage = 0;
        this.RefreshOptionsTab();
        UtilMenuController.Instance.RefreshUI();
      })));
    }
  }

  public void ToggleCosmetics()
  {
    CosmeticsPage.IsHideCosmeticsEnabled = !CosmeticsPage.IsHideCosmeticsEnabled;
    Configuration.SaveSettings();
    this.RefreshOptionsTab();
    if (!((UnityEngine.Object) UtilMenuController.Instance != (UnityEngine.Object) null))
      return;
    UtilMenuController.Instance.RefreshUI();
  }

  public void UpdateCosmeticsLogic()
  {
    bool flag = ((UnityEngine.Object) TabletController.Ins != (UnityEngine.Object) null) && TabletController.Ins.currentTabletMode == 1;
    if (CosmeticsPage.IsHideCosmeticsEnabled & flag)
    {
      this.SetSelectedCosmeticsEnabled(false);
      this._wasHiddenFrame = true;
    }
    else
    {
      if (!this._wasHiddenFrame)
        return;
      this.SetSelectedCosmeticsEnabled(true);
      this._wasHiddenFrame = false;
    }
  }

  private void SetCategoryVisibility(CosmeticsController.CosmeticCategory category, bool visible)
  {
    if ((((UnityEngine.Object) CosmeticsController.instance == (UnityEngine.Object) null) ? 1 : (CosmeticsController.instance.currentWornSet == null ? 1 : 0)) != 0)
      return;
    foreach (CosmeticsController.CosmeticItem cosmeticItem in ((IEnumerable<CosmeticsController.CosmeticItem>) CosmeticsController.instance.currentWornSet.items).Where<CosmeticsController.CosmeticItem>((Func<CosmeticsController.CosmeticItem, bool>) (item => !item.isNullItem && item.itemCategory == category)).ToList<CosmeticsController.CosmeticItem>())
      this.SetItemVisibility(cosmeticItem, visible);
  }

  private void SetSelectedCosmeticsEnabled(bool enable)
  {
    if ((((UnityEngine.Object) CosmeticsController.instance == (UnityEngine.Object) null) ? 1 : (CosmeticsController.instance.currentWornSet == null ? 1 : 0)) != 0)
      return;
    foreach (CosmeticsController.CosmeticItem cosmeticItem in ((IEnumerable<CosmeticsController.CosmeticItem>) CosmeticsController.instance.currentWornSet.items).Where<CosmeticsController.CosmeticItem>((Func<CosmeticsController.CosmeticItem, bool>) (item => !item.isNullItem && CosmeticsPage._categoriesToHide.Contains(item.itemCategory))).ToList<CosmeticsController.CosmeticItem>())
      this.SetItemVisibility(cosmeticItem, enable);
  }

  private void SetItemVisibility(CosmeticsController.CosmeticItem item, bool enable)
  {
    CosmeticItemRegistry cosmeticsObjectRegistry = GorillaTagger.Instance.offlineVRRig.cosmeticsObjectRegistry;
    if (cosmeticsObjectRegistry == null)
      return;
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
    if (!enable)
      cosmeticItemInstance.DisableItem(cosmeticSlots);
    else
      cosmeticItemInstance.EnableItem(cosmeticSlots, GorillaTagger.Instance.offlineVRRig);
  }

  public override void OnTabSelected(int tabIndex)
  {
    if ((tabIndex != 0 ? 1 : (this._profilesLoaded ? 1 : 0)) != 0)
      return;
    this.LoadProfilesFromFile();
    this.RefreshProfilesTab();
    this._profilesLoaded = true;
  }

  private void LoadProfilesFromFile() => this._profiles = CosmeticProfileStore.LoadProfiles();

  private void SaveProfilesToFile() => CosmeticProfileStore.SaveProfiles(this._profiles);

  private void RefreshProfilesTab()
  {
    if (this.Tabs.Count < 1)
      return;
    UtilTab tab = this.Tabs[0];
    tab.Elements.Clear();
    string text = this._isNamingProfile ? (string.IsNullOrEmpty(this._newProfileName) ? "ENTER NAME..." : this._newProfileName) : ">> CREATE NEW PRESET <<";
    tab.Elements.Add(new MenuElement(text, (Action) (() =>
    {
      if (this._isNamingProfile)
        return;
      this._isNamingProfile = true;
      this._newProfileName = "";
      KeyboardController.Instance.currentInput = "";
      this.RefreshProfilesTab();
      UtilMenuController.Instance.RefreshUI();
      KeyboardController.Instance.OnKeyPressed = (Action<string>) (str =>
      {
        this._newProfileName = str;
        this.RefreshProfilesTab();
        UtilMenuController.Instance.RefreshUI();
      });
      KeyboardController.Instance.OnEnterPressed = (Action) (() =>
      {
        this._isNamingProfile = false;
        string name = this._newProfileName.Trim();
        this._newProfileName = "";
        if (!string.IsNullOrEmpty(name))
          this.SaveCurrentProfile(name);
        this.RefreshProfilesTab();
        UtilMenuController.Instance.RefreshUI();
        KeyboardController.Instance.CloseKeyboard();
      });
      KeyboardController.Instance.OpenKeyboard();
    }), this._isNamingProfile ? ElementType.Input : ElementType.Button));
    tab.Elements.Add(new MenuElement("UNEQUIP ALL", (Action) (() =>
    {
      this.ClearAllCosmetics();
      UtilMenuController.Instance.RefreshUI();
    })));
    List<int> visibleGtSlots = WardrobePresetViewer.GetVisibleGtSlots();
    int num1 = this._profiles.Count + visibleGtSlots.Count;
    int totalPages = Mathf.CeilToInt((float) num1 / 2f);
    if (totalPages == 0)
      totalPages = 1;
    if (this._profilePageOffset >= totalPages)
      this._profilePageOffset = 0;
    int num2 = this._profilePageOffset * 2;
    int num3 = Mathf.Min(2, num1 - num2);
    for (int index1 = 0; index1 < num3; ++index1)
    {
      int index2 = num2 + index1;
      if (index2 >= this._profiles.Count)
      {
        int slot = visibleGtSlots[index2 - this._profiles.Count];
        tab.Elements.Add(new MenuElement($"OUTFIT #{slot + 1}", (Action) (() => WardrobePresetViewer.SelectGtOutfit(slot))));
      }
      else
      {
        CosmeticProfile profile = this._profiles[index2];
        tab.Elements.Add(new MenuElement(profile.ProfileName.ToUpper(), "DEL | LOAD", (Action) (() =>
        {
          this.DeleteProfile(profile);
          this.RefreshProfilesTab();
          UtilMenuController.Instance.RefreshUI();
        }), (Action) (() => this.LoadProfile(profile)))
        {
          Type = ElementType.Slider,
          CustomLeftIcon = UtilMenuMain.Instance.Icons.TrashIcon,
          CustomRightIcon = UtilMenuMain.Instance.Icons.Save
        });
      }
    }
    if (totalPages <= 1)
      return;
    tab.Elements.Add(new MenuElement("PAGE", $"{this._profilePageOffset + 1}/{totalPages}", (Action) (() =>
    {
      --this._profilePageOffset;
      if (this._profilePageOffset < 0)
        this._profilePageOffset = totalPages - 1;
      this.RefreshProfilesTab();
      UtilMenuController.Instance.RefreshUI();
    }), (Action) (() =>
    {
      ++this._profilePageOffset;
      if (this._profilePageOffset >= totalPages)
        this._profilePageOffset = 0;
      this.RefreshProfilesTab();
      UtilMenuController.Instance.RefreshUI();
    }))
    {
      Type = ElementType.Slider
    });
  }

  private void ClearAllCosmetics()
  {
    if (((UnityEngine.Object) CosmeticsController.instance == (UnityEngine.Object) null))
      return;
    CosmeticsController.instance.currentWornSet.ClearSet(CosmeticsController.instance.nullItem);
    CosmeticsHelper.ApplyAndRefresh();
  }

  private void SaveCurrentProfile(string name)
  {
    if (((UnityEngine.Object) CosmeticsController.instance == (UnityEngine.Object) null))
      return;
    CosmeticProfile cosmeticProfile = new CosmeticProfile()
    {
      ProfileName = name
    };
    VRRig offlineVrRig = ((UnityEngine.Object) GorillaTagger.Instance != (UnityEngine.Object) null) ? GorillaTagger.Instance.offlineVRRig : (VRRig) null;
    if (((UnityEngine.Object) offlineVrRig != (UnityEngine.Object) null))
      cosmeticProfile.Color = new float[3]
      {
        offlineVrRig.playerColor.r,
        offlineVrRig.playerColor.g,
        offlineVrRig.playerColor.b
      };
    CosmeticsController.CosmeticItem[] items = CosmeticsController.instance.currentWornSet.items;
    for (int index = 0; index < items.Length; ++index)
    {
      CosmeticsController.CosmeticItem cosmeticItem = items[index];
      if (!cosmeticItem.isNullItem)
      {
        cosmeticProfile.ItemIds.Add(cosmeticItem.itemName);
        cosmeticProfile.SidedItems.Add(new SavedCosmeticItem()
        {
          Id = cosmeticItem.itemName,
          Index = index
        });
      }
    }
    this._profiles.Add(cosmeticProfile);
    this.SaveProfilesToFile();
    WardrobePresetViewer.MarkDirty();
  }

  private void LoadProfile(CosmeticProfile profile)
  {
    if (((UnityEngine.Object) CosmeticsController.instance == (UnityEngine.Object) null))
      return;
    WardrobePresetViewer.NotifySelected(profile.ProfileName);
    CosmeticsController.instance.currentWornSet.ClearSet(CosmeticsController.instance.nullItem);
    CosmeticsController.CosmeticItem[] items = CosmeticsController.instance.currentWornSet.items;
    if ((profile.SidedItems == null ? 0 : (profile.SidedItems.Count > 0 ? 1 : 0)) != 0)
    {
      foreach (SavedCosmeticItem sidedItem in profile.SidedItems)
      {
        CosmeticsController.CosmeticItem itemFromDict = CosmeticsController.instance.GetItemFromDict(sidedItem.Id);
        if ((itemFromDict.isNullItem || sidedItem.Index < 0 ? 0 : (sidedItem.Index < items.Length ? 1 : 0)) != 0)
          items[sidedItem.Index] = itemFromDict;
      }
    }
    else
    {
      foreach (string itemId in profile.ItemIds)
      {
        CosmeticsController.CosmeticItem itemFromDict = CosmeticsController.instance.GetItemFromDict(itemId);
        if (!itemFromDict.isNullItem)
          CosmeticsController.instance.ApplyCosmeticItemToSet(CosmeticsController.instance.currentWornSet, itemFromDict, false, true);
      }
    }
    if ((profile.Color == null ? 0 : (profile.Color.Length >= 3 ? 1 : 0)) != 0)
      WardrobePresetViewer.ApplyPlayerColor(profile.Color[0], profile.Color[1], profile.Color[2]);
    CosmeticsHelper.ApplyAndRefresh();
  }

  private void DeleteProfile(CosmeticProfile profile)
  {
    this._profiles.Remove(profile);
    this.SaveProfilesToFile();
    WardrobePresetViewer.MarkDirty();
  }
}
