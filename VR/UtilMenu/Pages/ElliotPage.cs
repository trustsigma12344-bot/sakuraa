using GorillaNetworking;
using GorillaTag.CosmeticSystem;
using SakuraaCastingMod.Features.Overlays;
using SakuraaCastingMod.Features.Visuals;
using SakuraaCastingMod.Shared.Helpers;
using SakuraaCastingMod.Shared.Models;
using SakuraaCastingMod.VR.UtilMenu.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.VR.UtilMenu.Pages;

public class ElliotPage : BasePage
{
  [SavedSetting("ElliotPageCycleSpeed", 45)]
  private int _spamSpeed = 45;
  private bool _spammingRandom;
  private bool _spammingBeanies;
  private bool _spammingRoses;
  private bool _spammingGrab;
  [SavedSetting("ElliotPageGrabSpamMuteLocal", false)]
  private bool _grabSpamMuteLocal = false;
  private readonly List<AudioSource> _mutedGrabSources = new List<AudioSource>();
  private bool _viewingCustomSpammers = false;
  private List<SpammerProfile> _customProfiles = new List<SpammerProfile>();
  private string _activeCustomProfileName = "";
  private int _customSpammerPage = 0;
  private Coroutine _spamCoroutine;
  private readonly string[] _beanieIds = new string[5]
  {
    "LHABH.",
    "LHABI.",
    "LHABK.",
    "LHABJ.",
    "LHABR."
  };
  private readonly string[] _roseIds = new string[5]
  {
    "LBAAX.",
    "LBAAY.",
    "LBAAW.",
    "LBAAV.",
    "LMATU."
  };
  private ElliotPage.WardrobeState _wardrobeState = ElliotPage.WardrobeState.Categories;
  private CosmeticsController.CosmeticCategory _selectedCategory;
  private int _wardrobeCategoryPage = 0;
  private int _wardrobeItemPage = 0;
  private bool _preferLeftHand = false;
  private string _cosmeticSearchQuery = "";
  private List<CosmeticsController.CosmeticItem> _cosmeticSearchResults = new List<CosmeticsController.CosmeticItem>();
  private int _cosmeticSearchOffset = 0;
  private bool _isSearchingCosmetics = false;
  private ElliotPage.CollectionView _collectionView = ElliotPage.CollectionView.Summary;
  private Dictionary<CosmeticsController.CosmeticCategory, List<ElliotPage.MissingEntry>> _missingByCategory = new Dictionary<CosmeticsController.CosmeticCategory, List<ElliotPage.MissingEntry>>();
  private int _totalCatalog;
  private int _totalMissing;
  private int _unobtainableMissingCount;
  private bool _collectionEverScanned;
  private CosmeticsController.CosmeticCategory _collectionSelectedCategory;
  private int _collectionCategoryPage = 0;
  private int _collectionItemPage = 0;

  public override string PageName => "ELLIOT CUSTOM";

  public override Material PageIcon => UtilMenuMain.Instance.Icons.Paintbrush;

  public override void Start()
  {
    // ISSUE: explicit non-virtual call
    base.Start();
    this.LoadCustomSpammers();
  }

  private void LoadCustomSpammers()
  {
    try
    {
      string str = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "BepInEx", "config", "SakuraaCameraClient");
      if (!Directory.Exists(str))
        Directory.CreateDirectory(str);
      string path = Path.Combine(str, "ElliotSpammers.txt");
      if (!File.Exists(path))
      {
        string contents = "# CUSTOM SPAMMER CONFIGURATION\n# ----------------------------\n# Format:\n# Name of List:\n# Hand: Left/Right/Both\n# CosmeticID\n# CosmeticID\n\nMy Custom List:\nHand: Both\nLBAAX.\nLHABH.\n\nRight Hand Only Example:\nHand: Right\nLBAAV.\nLMATU.\n";
        File.WriteAllText(path, contents);
        this.ParseSpammerFile(contents.Split('\n'));
      }
      else
        this.ParseSpammerFile(File.ReadAllLines(path));
    }
    catch (Exception ex)
    {
      UnityEngine.Debug.LogError((object) ("[ElliotPage] Failed to load custom spammers: " + ex.Message));
    }
  }

  private void ParseSpammerFile(string[] lines)
  {
    this._customProfiles.Clear();
    SpammerProfile spammerProfile = (SpammerProfile) null;
    foreach (string line in lines)
    {
      string str1 = line.Trim();
      if ((string.IsNullOrEmpty(str1) || str1.StartsWith("#") ? 1 : (str1.StartsWith("//") ? 1 : 0)) == 0)
      {
        if (!str1.EndsWith(":"))
        {
          if (spammerProfile != null)
          {
            if (!str1.StartsWith("Hand:", StringComparison.OrdinalIgnoreCase))
            {
              spammerProfile.Ids.Add(str1);
            }
            else
            {
              string str2 = str1.Substring(5).Trim();
              if (!string.IsNullOrEmpty(str2))
                spammerProfile.Hand = str2;
            }
          }
        }
        else
        {
          spammerProfile = new SpammerProfile()
          {
            Name = str1.Substring(0, str1.Length - 1).Trim()
          };
          this._customProfiles.Add(spammerProfile);
        }
      }
    }
  }

  public override void BuildTabs()
  {
    this.Tabs.Add(new UtilTab()
    {
      TabIcon = UtilMenuMain.Instance.Icons.ClothesHanger,
      TabName = "Spammers"
    });
    this.Tabs.Add(new UtilTab()
    {
      TabIcon = UtilMenuMain.Instance.Icons.Backpack,
      TabName = "Wardrobe"
    });
    this.Tabs.Add(new UtilTab()
    {
      TabIcon = UtilMenuMain.Instance.Icons.MagnifyingGlass,
      TabName = "Cosmetic Search"
    });
    this.Tabs.Add(new UtilTab()
    {
      TabIcon = UtilMenuMain.Instance.Icons.ClosedBook,
      TabName = "Collection"
    });
    this.Tabs.Add(new UtilTab()
    {
      TabIcon = UtilMenuMain.Instance.Icons.Headset,
      TabName = "Private"
    });
    this.UpdateSpammerTab();
    this.RefreshWardrobeTab();
    this.RefreshCosmeticSearchTab();
    this.RefreshCollectionTab();
    this.RefreshPrivateTab();
  }

  public override void OnTabSelected(int tabIndex)
  {
    switch (tabIndex)
    {
      case 0:
        this.UpdateSpammerTab();
        break;
      case 1:
        this.RefreshWardrobeTab();
        break;
      case 2:
        this.RefreshCosmeticSearchTab();
        break;
      case 3:
        this.RefreshCollectionTab();
        break;
      case 4:
        this.RefreshPrivateTab();
        break;
    }
  }

  private void RefreshPrivateTab()
  {
    if (this.Tabs.Count < 6)
      return;
    UtilTab tab = this.Tabs[4];
    tab.Elements.Clear();
    tab.Elements.Add(new MenuElement("ANONYMOUS: " + (SecureNetworkManager.AnonymousMode ? "ON" : "OFF"), (Action) (() =>
    {
      SecureNetworkManager.AnonymousMode = !SecureNetworkManager.AnonymousMode;
      Notification.Send(SecureNetworkManager.AnonymousMode ? "Anonymous mode ON - hidden from other mod users" : "Anonymous mode OFF - broadcasting again", SecureNetworkManager.AnonymousMode ? Color.green : Color.yellow);
      this.RefreshPrivateTab();
      UtilMenuController.Instance.RefreshUI();
    }), SecureNetworkManager.AnonymousMode)
    {
      Type = ElementType.Toggle
    });
    tab.Elements.Add(new MenuElement("HZ SPOOF: " + (HzSp.Enabled ? "ON" : "OFF"), (Action) (() =>
    {
      HzSp.Enabled = !HzSp.Enabled;
      this.RefreshPrivateTab();
      UtilMenuController.Instance.RefreshUI();
    }), HzSp.Enabled)
    {
      Type = ElementType.Toggle
    });
    tab.Elements.Add(new MenuElement("TARGET HZ", $"{HzSp.TargetHz} HZ", (Action) (() =>
    {
      HzSp.Adjust(-1);
      this.RefreshPrivateTab();
      UtilMenuController.Instance.RefreshUI();
    }), (Action) (() =>
    {
      HzSp.Adjust(1);
      this.RefreshPrivateTab();
      UtilMenuController.Instance.RefreshUI();
    })));
  }

  private void RefreshWardrobeTab()
  {
    if (this.Tabs.Count < 3)
      return;
    UtilTab tab = this.Tabs[1];
    tab.Elements.Clear();
    if (((UnityEngine.Object) CosmeticsController.instance == (UnityEngine.Object) null))
    {
      tab.Elements.Add(new MenuElement("COSMETICS NOT LOADED", (Action) (() => { })));
    }
    else
    {
      switch (this._wardrobeState)
      {
        case ElliotPage.WardrobeState.Categories:
          List<CosmeticsController.CosmeticCategory> cosmeticCategoryList = new List<CosmeticsController.CosmeticCategory>()
          {
            (CosmeticsController.CosmeticCategory) 1,
            (CosmeticsController.CosmeticCategory) 3,
            (CosmeticsController.CosmeticCategory) 2,
            (CosmeticsController.CosmeticCategory) 9,
            (CosmeticsController.CosmeticCategory) 4,
            (CosmeticsController.CosmeticCategory) 7,
            (CosmeticsController.CosmeticCategory) 10,
            (CosmeticsController.CosmeticCategory) 6,
            (CosmeticsController.CosmeticCategory) 8,
            (CosmeticsController.CosmeticCategory) 5,
            (CosmeticsController.CosmeticCategory) 11
          };
          int totalPages1 = Mathf.CeilToInt((float) cosmeticCategoryList.Count / 5f);
          if (this._wardrobeCategoryPage >= totalPages1)
            this._wardrobeCategoryPage = 0;
          if (this._wardrobeCategoryPage < 0)
            this._wardrobeCategoryPage = totalPages1 - 1;
          int num1 = this._wardrobeCategoryPage * 5;
          int num2 = Mathf.Min(5, cosmeticCategoryList.Count - num1);
          for (int index = 0; index < num2; ++index)
          {
            CosmeticsController.CosmeticCategory cat = cosmeticCategoryList[num1 + index];
            string text = cat.ToString().ToUpper();
            if (cat == (CosmeticsController.CosmeticCategory) 9)
              text = "HOLDABLES";
            tab.Elements.Add(new MenuElement(text, (Action) (() =>
            {
              this._selectedCategory = cat;
              this._wardrobeState = ElliotPage.WardrobeState.Items;
              this._wardrobeItemPage = 0;
              this.RefreshWardrobeTab();
              UtilMenuController.Instance.RefreshUI();
            })));
          }
          if (totalPages1 <= 1)
            break;
          tab.Elements.Add(new MenuElement($"PAGE {this._wardrobeCategoryPage + 1}/{totalPages1} >", (Action) (() =>
          {
            ++this._wardrobeCategoryPage;
            if (this._wardrobeCategoryPage >= totalPages1)
              this._wardrobeCategoryPage = 0;
            this.RefreshWardrobeTab();
            UtilMenuController.Instance.RefreshUI();
          })));
          break;
        case ElliotPage.WardrobeState.Items:
          tab.Elements.Add(new MenuElement("<< BACK TO CATEGORIES", (Action) (() =>
          {
            this._wardrobeState = ElliotPage.WardrobeState.Categories;
            this.RefreshWardrobeTab();
            UtilMenuController.Instance.RefreshUI();
          })));
          if ((this._selectedCategory == (CosmeticsController.CosmeticCategory) 9 ? 1 : (this._selectedCategory == (CosmeticsController.CosmeticCategory) 8 ? 1 : 0)) != 0)
            tab.Elements.Add(new MenuElement("SIDE: " + (this._preferLeftHand ? "LEFT" : "RIGHT"), (Action) (() =>
            {
              this._preferLeftHand = !this._preferLeftHand;
              this.RefreshWardrobeTab();
              UtilMenuController.Instance.RefreshUI();
            })));
          List<CosmeticsController.CosmeticItem> list = CosmeticsController.instance.unlockedCosmetics.Where<CosmeticsController.CosmeticItem>((Func<CosmeticsController.CosmeticItem, bool>) (x => !x.isNullItem && x.itemCategory == this._selectedCategory)).ToList<CosmeticsController.CosmeticItem>();
          if (list.Count == 0)
          {
            tab.Elements.Add(new MenuElement("- NO ITEMS OWNED -", (Action) (() => { })));
            break;
          }
          int num3 = 3;
          int totalPages2 = Mathf.CeilToInt((float) list.Count / 3f);
          if (this._wardrobeItemPage >= totalPages2)
            this._wardrobeItemPage = 0;
          int num4 = this._wardrobeItemPage * num3;
          int num5 = Mathf.Min(num3, list.Count - num4);
          for (int index = 0; index < num5; ++index)
          {
            CosmeticsController.CosmeticItem item = list[num4 + index];
            string str = string.IsNullOrEmpty(item.overrideDisplayName) ? item.itemName : item.overrideDisplayName;
            if (string.IsNullOrEmpty(str))
              str = "ITEM";
            string text = str.ToUpper();
            if (text.Length > 13)
              text = text.Substring(0, 13) + ".";
            bool initialValue;
            if (initialValue = CosmeticsController.instance.IsCosmeticEquipped(item))
              text = $"[ {text} ]";
            tab.Elements.Add(new MenuElement(text, (Action) (() =>
            {
              CosmeticsController.instance.ApplyCosmeticItemToSet(CosmeticsController.instance.currentWornSet, item, this._preferLeftHand, true);
              CosmeticsHelper.ApplyAndRefresh();
              this.RefreshWardrobeTab();
              UtilMenuController.Instance.RefreshUI();
            }), initialValue));
          }
          if (totalPages2 <= 1)
            break;
          tab.Elements.Add(new MenuElement($"PAGE {this._wardrobeItemPage + 1}/{totalPages2} >", (Action) (() =>
          {
            ++this._wardrobeItemPage;
            if (this._wardrobeItemPage >= totalPages2)
              this._wardrobeItemPage = 0;
            this.RefreshWardrobeTab();
            UtilMenuController.Instance.RefreshUI();
          })));
          break;
      }
    }
  }

  private void RefreshCosmeticSearchTab()
  {
    if (this.Tabs.Count < 4)
      return;
    UtilTab tab = this.Tabs[2];
    tab.Elements.Clear();
    string text1 = this._isSearchingCosmetics ? this._cosmeticSearchQuery : (string.IsNullOrEmpty(this._cosmeticSearchQuery) ? "SEARCH COSMETIC..." : "QUERY: " + this._cosmeticSearchQuery);
    ElementType type = this._isSearchingCosmetics ? ElementType.Input : ElementType.Button;
    tab.Elements.Add(new MenuElement(text1, (Action) (() =>
    {
      this._isSearchingCosmetics = true;
      KeyboardController.Instance.currentInput = this._cosmeticSearchQuery;
      this.RefreshCosmeticSearchTab();
      UtilMenuController.Instance.RefreshUI();
      KeyboardController.Instance.OnKeyPressed = (Action<string>) (str =>
      {
        this._cosmeticSearchQuery = str;
        this.RefreshCosmeticSearchTab();
        UtilMenuController.Instance.RefreshUI();
      });
      KeyboardController.Instance.OnEnterPressed = (Action) (() =>
      {
        this._isSearchingCosmetics = false;
        if ((string.IsNullOrEmpty(this._cosmeticSearchQuery) ? 0 : (((UnityEngine.Object) CosmeticsController.instance != (UnityEngine.Object) null) ? 1 : 0)) != 0)
        {
          HashSet<string> ownedSet = new HashSet<string>(CosmeticsController.instance.unlockedCosmetics.Where<CosmeticsController.CosmeticItem>((Func<CosmeticsController.CosmeticItem, bool>) (x => !x.isNullItem)).Select<CosmeticsController.CosmeticItem, string>((Func<CosmeticsController.CosmeticItem, string>) (x => x.itemName)));
          this._cosmeticSearchResults = CosmeticsController.instance.allCosmetics.Where<CosmeticsController.CosmeticItem>((Func<CosmeticsController.CosmeticItem, bool>) (x =>
          {
            if (x.isNullItem || !ownedSet.Contains(x.itemName))
              return false;
            if (!string.IsNullOrEmpty(x.displayName) && x.displayName.ToUpper().Contains(this._cosmeticSearchQuery.ToUpper()))
              return true;
            return !string.IsNullOrEmpty(x.overrideDisplayName) && x.overrideDisplayName.ToUpper().Contains(this._cosmeticSearchQuery.ToUpper());
          })).ToList<CosmeticsController.CosmeticItem>();
          this._cosmeticSearchOffset = 0;
        }
        this.RefreshCosmeticSearchTab();
        UtilMenuController.Instance.RefreshUI();
        KeyboardController.Instance.CloseKeyboard();
      });
      KeyboardController.Instance.OpenKeyboard();
    }), type));
    if (this._cosmeticSearchResults.Count != 0)
    {
      int totalPages = Mathf.CeilToInt((float) this._cosmeticSearchResults.Count / 4f);
      if (this._cosmeticSearchOffset >= totalPages)
        this._cosmeticSearchOffset = 0;
      int num1 = this._cosmeticSearchOffset * 4;
      int num2 = Mathf.Min(4, this._cosmeticSearchResults.Count - num1);
      for (int index = 0; index < num2; ++index)
      {
        CosmeticsController.CosmeticItem item = this._cosmeticSearchResults[num1 + index];
        string str = !string.IsNullOrEmpty(item.overrideDisplayName) ? item.overrideDisplayName : item.displayName;
        if (string.IsNullOrEmpty(str))
          str = item.itemName;
        if (string.IsNullOrEmpty(str))
          str = "ITEM";
        string text2 = str.ToUpper();
        if (text2.Length > 13)
          text2 = text2.Substring(0, 13) + ".";
        bool initialValue;
        if (initialValue = ((UnityEngine.Object) CosmeticsController.instance != (UnityEngine.Object) null) && CosmeticsController.instance.IsCosmeticEquipped(item))
          text2 = $"[ {text2} ]";
        tab.Elements.Add(new MenuElement(text2, (Action) (() =>
        {
          if (!((UnityEngine.Object) CosmeticsController.instance != (UnityEngine.Object) null))
            return;
          CosmeticsController.instance.ApplyCosmeticItemToSet(CosmeticsController.instance.currentWornSet, item, false, true);
          CosmeticsHelper.ApplyAndRefresh();
          this.RefreshCosmeticSearchTab();
          UtilMenuController.Instance.RefreshUI();
        }), initialValue));
      }
      if (totalPages <= 1)
        return;
      tab.Elements.Add(new MenuElement($"PAGE {this._cosmeticSearchOffset + 1}/{totalPages}", "", (Action) (() =>
      {
        --this._cosmeticSearchOffset;
        if (this._cosmeticSearchOffset < 0)
          this._cosmeticSearchOffset = totalPages - 1;
        this.RefreshCosmeticSearchTab();
        UtilMenuController.Instance.RefreshUI();
      }), (Action) (() =>
      {
        ++this._cosmeticSearchOffset;
        if (this._cosmeticSearchOffset >= totalPages)
          this._cosmeticSearchOffset = 0;
        this.RefreshCosmeticSearchTab();
        UtilMenuController.Instance.RefreshUI();
      }))
      {
        Type = ElementType.Slider
      });
    }
    else
      tab.Elements.Add(new MenuElement("- NO RESULTS -", (Action) (() => { })));
  }

  private static string CosmeticDisplayName(CosmeticsController.CosmeticItem i)
  {
    return !string.IsNullOrEmpty(i.overrideDisplayName) ? i.overrideDisplayName : (!string.IsNullOrEmpty(i.displayName) ? i.displayName : i.itemName);
  }

  private void RescanMissing()
  {
    this._missingByCategory.Clear();
    this._totalCatalog = 0;
    this._totalMissing = 0;
    this._unobtainableMissingCount = 0;
    this._collectionEverScanned = true;
    CosmeticsController instance = CosmeticsController.instance;
    if ((((UnityEngine.Object) instance == (UnityEngine.Object) null) ? 1 : (instance.allCosmetics == null ? 1 : 0)) != 0)
      return;
    Dictionary<string, bool> dictionary = new Dictionary<string, bool>();
    if (instance.v2_allCosmetics != null)
    {
      foreach (CosmeticInfoV2 v2AllCosmetic in instance.v2_allCosmetics)
      {
        if (!string.IsNullOrEmpty(v2AllCosmetic.playFabID))
          dictionary[v2AllCosmetic.playFabID] = v2AllCosmetic.enabled;
      }
    }
    HashSet<string> stringSet = new HashSet<string>(instance.unlockedCosmetics.Where<CosmeticsController.CosmeticItem>((Func<CosmeticsController.CosmeticItem, bool>) (x => !x.isNullItem)).Select<CosmeticsController.CosmeticItem, string>((Func<CosmeticsController.CosmeticItem, string>) (x => x.itemName)));
    foreach (CosmeticsController.CosmeticItem allCosmetic in instance.allCosmetics)
    {
      if ((allCosmetic.isNullItem ? 1 : (string.IsNullOrEmpty(allCosmetic.itemName) ? 1 : 0)) == 0)
      {
        ++this._totalCatalog;
        if (!stringSet.Contains(allCosmetic.itemName))
        {
          bool flag1;
          bool flag2 = !dictionary.TryGetValue(allCosmetic.itemName, out flag1) | flag1;
          List<ElliotPage.MissingEntry> missingEntryList;
          if (!this._missingByCategory.TryGetValue(allCosmetic.itemCategory, out missingEntryList))
          {
            missingEntryList = new List<ElliotPage.MissingEntry>();
            this._missingByCategory[allCosmetic.itemCategory] = missingEntryList;
          }
          missingEntryList.Add(new ElliotPage.MissingEntry()
          {
            Item = allCosmetic,
            Obtainable = flag2
          });
          ++this._totalMissing;
          if (!flag2)
            ++this._unobtainableMissingCount;
        }
      }
    }
    foreach (List<ElliotPage.MissingEntry> missingEntryList in this._missingByCategory.Values)
      missingEntryList.Sort((Comparison<ElliotPage.MissingEntry>) ((a, b) => a.Obtainable != b.Obtainable ? (a.Obtainable ? -1 : 1) : string.Compare(ElliotPage.CosmeticDisplayName(a.Item), ElliotPage.CosmeticDisplayName(b.Item), StringComparison.OrdinalIgnoreCase)));
  }

  private void ExportMissingToFile()
  {
    try
    {
      string str1 = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "BepInEx", "config", "SakuraaCameraClient");
      if (!Directory.Exists(str1))
        Directory.CreateDirectory(str1);
      string path = Path.Combine(str1, "ElliotMissingCosmetics.txt");
      StringBuilder stringBuilder = new StringBuilder();
      int num = this._totalMissing - this._unobtainableMissingCount;
      stringBuilder.AppendLine(string.Format("# MISSING COSMETICS, generated {0:yyyy-MM-dd HH:mm:ss}", (object) DateTime.Now));
      stringBuilder.AppendLine($"# Catalog: {this._totalCatalog}  Owned: {this._totalCatalog - this._totalMissing}  Missing: {this._totalMissing} ({num} obtainable, {this._unobtainableMissingCount} unavailable)");
      stringBuilder.AppendLine();
      foreach (CosmeticsController.CosmeticCategory key in Enum.GetValues(typeof (CosmeticsController.CosmeticCategory)))
      {
        List<ElliotPage.MissingEntry> missingEntryList;
        if ((!this._missingByCategory.TryGetValue(key, out missingEntryList) ? 1 : (missingEntryList.Count == 0 ? 1 : 0)) == 0)
        {
          stringBuilder.AppendLine($"# {key.ToString().ToUpper()} ({missingEntryList.Count} missing)");
          foreach (ElliotPage.MissingEntry missingEntry in missingEntryList)
          {
            string str2 = missingEntry.Obtainable ? "" : "  (unavailable)";
            stringBuilder.AppendLine($"{missingEntry.Item.itemName} {ElliotPage.CosmeticDisplayName(missingEntry.Item)}{str2}");
          }
          stringBuilder.AppendLine();
        }
      }
      File.WriteAllText(path, stringBuilder.ToString());
      Notification.Send($"Exported {this._totalMissing} missing cosmetics", Color.green);
    }
    catch (Exception ex)
    {
      UnityEngine.Debug.LogError((object) ("[ElliotPage] Failed to export missing cosmetics: " + ex.Message));
      Notification.Send("Export failed (see console)", Color.red);
    }
  }

  private void RefreshCollectionTab()
  {
    if (this.Tabs.Count < 5)
      return;
    UtilTab tab = this.Tabs[3];
    tab.Elements.Clear();
    CosmeticsController instance = CosmeticsController.instance;
    if ((((UnityEngine.Object) instance == (UnityEngine.Object) null) || instance.allCosmetics == null ? 1 : (instance.allCosmetics.Count == 0 ? 1 : 0)) == 0)
    {
      if (!this._collectionEverScanned)
        this.RescanMissing();
      if (this._collectionView == ElliotPage.CollectionView.Summary)
        this.BuildCollectionSummary(tab);
      else
        this.BuildCollectionCategoryView(tab);
    }
    else
      tab.Elements.Add(new MenuElement("CATALOG STILL LOADING…", (Action) (() => { })));
  }

  private void BuildCollectionSummary(UtilTab tab)
  {
    string str = this._unobtainableMissingCount > 0 ? $" (-{this._unobtainableMissingCount})" : "";
    tab.Elements.Add(new MenuElement($"MISSING: {this._totalMissing} / {this._totalCatalog}{str}", (Action) (() => { })));
    tab.Elements.Add(new MenuElement("RESCAN", (Action) (() =>
    {
      this.RescanMissing();
      this.RefreshCollectionTab();
      UtilMenuController.Instance.RefreshUI();
    })));
    tab.Elements.Add(new MenuElement("EXPORT TO FILE >>", (Action) (() => this.ExportMissingToFile())));
    List<CosmeticsController.CosmeticCategory> cosmeticCategoryList = new List<CosmeticsController.CosmeticCategory>();
    foreach (CosmeticsController.CosmeticCategory key in Enum.GetValues(typeof (CosmeticsController.CosmeticCategory)))
    {
      List<ElliotPage.MissingEntry> missingEntryList;
      if ((!this._missingByCategory.TryGetValue(key, out missingEntryList) ? 0 : (missingEntryList.Count > 0 ? 1 : 0)) != 0)
        cosmeticCategoryList.Add(key);
    }
    if (cosmeticCategoryList.Count == 0)
    {
      tab.Elements.Add(new MenuElement("- NOTHING MISSING -", (Action) (() => { })));
    }
    else
    {
      int num1 = cosmeticCategoryList.Count <= 3 ? 3 : 2;
      int totalPages = Mathf.CeilToInt((float) cosmeticCategoryList.Count / (float) num1);
      if (this._collectionCategoryPage >= totalPages)
        this._collectionCategoryPage = 0;
      if (this._collectionCategoryPage < 0)
        this._collectionCategoryPage = totalPages - 1;
      int num2 = this._collectionCategoryPage * num1;
      int num3 = Mathf.Min(num1, cosmeticCategoryList.Count - num2);
      for (int index = 0; index < num3; ++index)
      {
        CosmeticsController.CosmeticCategory cat = cosmeticCategoryList[num2 + index];
        int count = this._missingByCategory[cat].Count;
        string text = $"{cat.ToString().ToUpper()}: {count} >";
        tab.Elements.Add(new MenuElement(text, (Action) (() =>
        {
          this._collectionSelectedCategory = cat;
          this._collectionItemPage = 0;
          this._collectionView = ElliotPage.CollectionView.CategoryItems;
          this.RefreshCollectionTab();
          UtilMenuController.Instance.RefreshUI();
        })));
      }
      if (totalPages <= 1)
        return;
      tab.Elements.Add(new MenuElement($"PAGE {this._collectionCategoryPage + 1}/{totalPages}", "", (Action) (() =>
      {
        --this._collectionCategoryPage;
        if (this._collectionCategoryPage < 0)
          this._collectionCategoryPage = totalPages - 1;
        this.RefreshCollectionTab();
        UtilMenuController.Instance.RefreshUI();
      }), (Action) (() =>
      {
        ++this._collectionCategoryPage;
        if (this._collectionCategoryPage >= totalPages)
          this._collectionCategoryPage = 0;
        this.RefreshCollectionTab();
        UtilMenuController.Instance.RefreshUI();
      }))
      {
        Type = ElementType.Slider
      });
    }
  }

  private void BuildCollectionCategoryView(UtilTab tab)
  {
    tab.Elements.Add(new MenuElement("<< BACK TO SUMMARY", (Action) (() =>
    {
      this._collectionView = ElliotPage.CollectionView.Summary;
      this.RefreshCollectionTab();
      UtilMenuController.Instance.RefreshUI();
    })));
    List<ElliotPage.MissingEntry> missingEntryList;
    if ((!this._missingByCategory.TryGetValue(this._collectionSelectedCategory, out missingEntryList) ? 1 : (missingEntryList.Count == 0 ? 1 : 0)) == 0)
    {
      int totalPages = Mathf.CeilToInt((float) missingEntryList.Count / 4f);
      if (this._collectionItemPage >= totalPages)
        this._collectionItemPage = 0;
      if (this._collectionItemPage < 0)
        this._collectionItemPage = totalPages - 1;
      int num1 = this._collectionItemPage * 4;
      int num2 = Mathf.Min(4, missingEntryList.Count - num1);
      for (int index = 0; index < num2; ++index)
      {
        ElliotPage.MissingEntry missingEntry = missingEntryList[num1 + index];
        string str1 = ElliotPage.CosmeticDisplayName(missingEntry.Item).ToUpper();
        int length = 12;
        if (str1.Length > 12)
          str1 = str1.Substring(0, length) + ".";
        string str2 = missingEntry.Obtainable ? "" : " *";
        string text = $"{missingEntry.Item.itemName} {str1}{str2}";
        tab.Elements.Add(new MenuElement(text, (Action) (() => { })));
      }
      if (totalPages <= 1)
        return;
      tab.Elements.Add(new MenuElement($"PAGE {this._collectionItemPage + 1}/{totalPages}", "", (Action) (() =>
      {
        --this._collectionItemPage;
        if (this._collectionItemPage < 0)
          this._collectionItemPage = totalPages - 1;
        this.RefreshCollectionTab();
        UtilMenuController.Instance.RefreshUI();
      }), (Action) (() =>
      {
        ++this._collectionItemPage;
        if (this._collectionItemPage >= totalPages)
          this._collectionItemPage = 0;
        this.RefreshCollectionTab();
        UtilMenuController.Instance.RefreshUI();
      }))
      {
        Type = ElementType.Slider
      });
    }
    else
      tab.Elements.Add(new MenuElement("- NONE MISSING -", (Action) (() => { })));
  }

  private void UpdateSpammerTab()
  {
    if (this.Tabs.Count < 2)
      return;
    UtilTab tab = this.Tabs[0];
    tab.Elements.Clear();
    if (this._viewingCustomSpammers)
    {
      tab.Elements.Add(new MenuElement("<< BACK", (Action) (() =>
      {
        this._viewingCustomSpammers = false;
        this.UpdateSpammerTab();
        UtilMenuController.Instance.RefreshUI();
      })));
      tab.Elements.Add(new MenuElement("RELOAD CONFIG", (Action) (() =>
      {
        this.LoadCustomSpammers();
        this.UpdateSpammerTab();
        UtilMenuController.Instance.RefreshUI();
      })));
      int totalPages = Mathf.CeilToInt((float) this._customProfiles.Count / 3f);
      if (totalPages == 0)
        totalPages = 1;
      if (this._customSpammerPage >= totalPages)
        this._customSpammerPage = 0;
      if (this._customSpammerPage < 0)
        this._customSpammerPage = totalPages - 1;
      int num1 = this._customSpammerPage * 3;
      int num2 = Mathf.Min(3, this._customProfiles.Count - num1);
      if (this._customProfiles.Count != 0)
      {
        for (int index = 0; index < num2; ++index)
        {
          SpammerProfile profile = this._customProfiles[num1 + index];
          bool initialValue = this._activeCustomProfileName == profile.Name;
          string str = profile.Name.ToUpper();
          if (str.Length > 18)
            str = str.Substring(0, 18);
          tab.Elements.Add(new MenuElement($"{str}: {(initialValue ? "ON" : "OFF")}", (Action) (() => this.ToggleCustomProfile(profile)), initialValue)
          {
            Type = ElementType.Toggle
          });
        }
      }
      else
        tab.Elements.Add(new MenuElement("NO PROFILES IN CONFIG", (Action) (() => { })));
      if (totalPages <= 1)
        return;
      tab.Elements.Add(new MenuElement($"PAGE {this._customSpammerPage + 1}/{totalPages}", "", (Action) (() =>
      {
        --this._customSpammerPage;
        if (this._customSpammerPage < 0)
          this._customSpammerPage = totalPages - 1;
        this.UpdateSpammerTab();
        UtilMenuController.Instance.RefreshUI();
      }), (Action) (() =>
      {
        ++this._customSpammerPage;
        if (this._customSpammerPage >= totalPages)
          this._customSpammerPage = 0;
        this.UpdateSpammerTab();
        UtilMenuController.Instance.RefreshUI();
      }))
      {
        Type = ElementType.Slider
      });
    }
    else
    {
      tab.Elements.Add(new MenuElement("RANDOM SPAM: " + (this._spammingRandom ? "ON" : "OFF"), new Action(this.ToggleRandom), this._spammingRandom)
      {
        Type = ElementType.Toggle
      });
      tab.Elements.Add(new MenuElement("RAINBOW BEANIES: " + (this._spammingBeanies ? "ON" : "OFF"), new Action(this.ToggleBeanies), this._spammingBeanies)
      {
        Type = ElementType.Toggle
      });
      tab.Elements.Add(new MenuElement("RAINBOW ROSES: " + (this._spammingRoses ? "ON" : "OFF"), new Action(this.ToggleRoses), this._spammingRoses)
      {
        Type = ElementType.Toggle
      });
      tab.Elements.Add(new MenuElement("GRAB SPAM: " + (this._spammingGrab ? "ON" : "OFF"), new Action(this.ToggleGrab), this._spammingGrab)
      {
        Type = ElementType.Toggle
      });
      tab.Elements.Add(new MenuElement("MUTE LOCAL: " + (this._grabSpamMuteLocal ? "ON" : "OFF"), new Action(this.ToggleGrabMuteLocal), this._grabSpamMuteLocal)
      {
        Type = ElementType.Toggle
      });
      MenuElement speedEl = (MenuElement) null;
      speedEl = new MenuElement("SPEED", this._spamSpeed.ToString(), (Action) (() =>
      {
        this._spamSpeed = Mathf.Clamp(this._spamSpeed - 1, 1, 45);
        speedEl.ValueText = this._spamSpeed.ToString();
        UtilMenuController.Instance.RefreshUI();
      }), (Action) (() =>
      {
        this._spamSpeed = Mathf.Clamp(this._spamSpeed + 1, 1, 45);
        speedEl.ValueText = this._spamSpeed.ToString();
        UtilMenuController.Instance.RefreshUI();
      }))
      {
        Type = ElementType.Slider
      };
      tab.Elements.Add(speedEl);
      tab.Elements.Add(new MenuElement("CUSTOM SPAMMERS >>", (Action) (() =>
      {
        this._viewingCustomSpammers = true;
        this.UpdateSpammerTab();
        UtilMenuController.Instance.RefreshUI();
      })));
    }
  }

  private void ResetSpammers()
  {
    this._spammingRandom = false;
    this._spammingBeanies = false;
    this._spammingRoses = false;
    this._spammingGrab = false;
    this._activeCustomProfileName = "";
    this.StopSpam();
    this.RestoreMutedGrabSources();
  }

  private void ToggleRandom()
  {
    bool spammingRandom = this._spammingRandom;
    this.ResetSpammers();
    if (!spammingRandom)
    {
      this._spammingRandom = true;
      if (((UnityEngine.Object) UtilMenuMain.Instance != (UnityEngine.Object) null))
        this._spamCoroutine = UtilMenuMain.Instance.StartCoroutine(this.SpamRandomRoutine());
    }
    this.UpdateSpammerTab();
    UtilMenuController.Instance.RefreshUI();
  }

  private void ToggleBeanies()
  {
    bool spammingBeanies = this._spammingBeanies;
    this.ResetSpammers();
    if (!spammingBeanies)
    {
      this._spammingBeanies = true;
      if (((UnityEngine.Object) UtilMenuMain.Instance != (UnityEngine.Object) null))
        this._spamCoroutine = UtilMenuMain.Instance.StartCoroutine(this.CycleSpecificRoutine(this._beanieIds));
    }
    this.UpdateSpammerTab();
    UtilMenuController.Instance.RefreshUI();
  }

  private void ToggleRoses()
  {
    bool spammingRoses = this._spammingRoses;
    this.ResetSpammers();
    if (!spammingRoses)
    {
      this._spammingRoses = true;
      if (((UnityEngine.Object) UtilMenuMain.Instance != (UnityEngine.Object) null))
        this._spamCoroutine = UtilMenuMain.Instance.StartCoroutine(this.CycleSpecificRoutine(this._roseIds, "Right"));
    }
    this.UpdateSpammerTab();
    UtilMenuController.Instance.RefreshUI();
  }

  private void ToggleGrabMuteLocal()
  {
    this._grabSpamMuteLocal = !this._grabSpamMuteLocal;
    if (!this._grabSpamMuteLocal)
      this.RestoreMutedGrabSources();
    this.UpdateSpammerTab();
    UtilMenuController.Instance.RefreshUI();
  }

  private void ToggleGrab()
  {
    bool spammingGrab = this._spammingGrab;
    this.ResetSpammers();
    if (!spammingGrab)
    {
      this._spammingGrab = true;
      if (((UnityEngine.Object) UtilMenuMain.Instance != (UnityEngine.Object) null))
        this._spamCoroutine = UtilMenuMain.Instance.StartCoroutine(this.GrabSpamRoutine());
    }
    this.UpdateSpammerTab();
    UtilMenuController.Instance.RefreshUI();
  }

  private void ToggleCustomProfile(SpammerProfile profile)
  {
    bool flag = this._activeCustomProfileName == profile.Name;
    this.ResetSpammers();
    if (!flag)
    {
      this._activeCustomProfileName = profile.Name;
      if ((!((UnityEngine.Object) UtilMenuMain.Instance != (UnityEngine.Object) null) || profile.Ids == null ? 0 : (profile.Ids.Count > 0 ? 1 : 0)) != 0)
        this._spamCoroutine = UtilMenuMain.Instance.StartCoroutine(this.CycleSpecificRoutine(profile.Ids.ToArray(), profile.Hand));
    }
    this.UpdateSpammerTab();
    UtilMenuController.Instance.RefreshUI();
  }

  private void StopSpam()
  {
    if ((this._spamCoroutine == null ? 0 : (((UnityEngine.Object) UtilMenuMain.Instance != (UnityEngine.Object) null) ? 1 : 0)) == 0)
      return;
    UtilMenuMain.Instance.StopCoroutine(this._spamCoroutine);
    this._spamCoroutine = (Coroutine) null;
  }

  private IEnumerator SpamRandomRoutine()
  {
    while (this._spammingRandom)
    {
      try
      {
        if (((UnityEngine.Object) CosmeticsController.instance != (UnityEngine.Object) null))
        {
          List<CosmeticsController.CosmeticItem> validCosmetics = CosmeticsController.instance.unlockedCosmetics.Where<CosmeticsController.CosmeticItem>((Func<CosmeticsController.CosmeticItem, bool>) (x => !x.isNullItem && !string.IsNullOrEmpty(x.itemName) && x.itemName != "LBADE." && x.itemName != "LBAGS." && x.itemName != "LBANI.")).ToList<CosmeticsController.CosmeticItem>();
          if (validCosmetics.Count > 0)
          {
            CosmeticsController.CosmeticItem randomItem = validCosmetics[UnityEngine.Random.Range(0, validCosmetics.Count)];
            bool randomSide = (double) UnityEngine.Random.value > 0.5;
            CosmeticsController.instance.ApplyCosmeticItemToSet(CosmeticsController.instance.currentWornSet, randomItem, randomSide, true);
            CosmeticsHelper.ApplyAndRefresh();
            randomItem = new CosmeticsController.CosmeticItem();
          }
          validCosmetics = (List<CosmeticsController.CosmeticItem>) null;
        }
      }
      catch
      {
      }
      yield return (object) new WaitForSeconds(1f / (float) this._spamSpeed);
    }
  }

  private static TransferrableObject.PositionState DockedStateFor(
    BodyDockPositions.DropPositions zone)
  {
    BodyDockPositions.DropPositions dropPositions = zone;
    TransferrableObject.PositionState positionState;
    switch (dropPositions - 1)
    {
      case 0:
        positionState = (TransferrableObject.PositionState) 1;
        break;
      case (BodyDockPositions.DropPositions) 1:
        positionState = (TransferrableObject.PositionState) 2;
        break;
      case (BodyDockPositions.DropPositions) 2:
        positionState = (TransferrableObject.PositionState) 0;
        break;
      case (BodyDockPositions.DropPositions) 3:
        positionState = (TransferrableObject.PositionState) 16 /*0x10*/;
        break;
      default:
        if (dropPositions == (BodyDockPositions.DropPositions) 8)
        {
          positionState = (TransferrableObject.PositionState) 32 /*0x20*/;
          break;
        }
        if (dropPositions == (BodyDockPositions.DropPositions) 16 /*0x10*/)
        {
          positionState = (TransferrableObject.PositionState) 64 /*0x40*/;
          break;
        }
        goto case 2;
    }
    return positionState;
  }

  private IEnumerator GrabSpamRoutine()
  {
    bool grabbed = false;
    while (this._spammingGrab)
    {
      try
      {
        VRRig rig = ((UnityEngine.Object) GorillaTagger.Instance != (UnityEngine.Object) null) ? GorillaTagger.Instance.offlineVRRig : (VRRig) null;
        BodyDockPositions dock = ((UnityEngine.Object) rig != (UnityEngine.Object) null) ? rig.myBodyDockPositions : (BodyDockPositions) null;
        if ((!((UnityEngine.Object) dock != (UnityEngine.Object) null) ? 0 : (dock.allObjects != null ? 1 : 0)) != 0)
        {
          TransferrableObject[] objs = dock.allObjects;
          for (int i = 0; i < objs.Length; ++i)
          {
            TransferrableObject t = objs[i];
            if ((((UnityEngine.Object) t == (UnityEngine.Object) null) ? 1 : (!((Component) t).gameObject.activeSelf ? 1 : 0)) == 0 && t.storedZone != 0)
            {
              TransferrableObject.PositionState dockedState = ElliotPage.DockedStateFor(t.storedZone);
              if (dockedState != 0)
              {
                if (this._grabSpamMuteLocal)
                  this.MuteSourcesOn(t);
                if (grabbed)
                {
                  bool toRight = t.storedZone == (BodyDockPositions.DropPositions) 8 || t.storedZone == (BodyDockPositions.DropPositions) 1;
                  t.currentState = toRight ? (TransferrableObject.PositionState) 8 : (TransferrableObject.PositionState) 4;
                }
                else
                  t.currentState = dockedState;
                t = (TransferrableObject) null;
              }
            }
          }
          objs = (TransferrableObject[]) null;
        }
        if ((this._grabSpamMuteLocal ? 0 : (this._mutedGrabSources.Count > 0 ? 1 : 0)) != 0)
          this.RestoreMutedGrabSources();
        rig = (VRRig) null;
        dock = (BodyDockPositions) null;
      }
      catch
      {
      }
      grabbed = !grabbed;
      yield return (object) new WaitForSeconds(1f / (float) this._spamSpeed);
    }
    this.RestoreMutedGrabSources();
  }

  private void MuteSourcesOn(TransferrableObject t)
  {
    foreach (AudioSource componentsInChild in ((Component) t).GetComponentsInChildren<AudioSource>(true))
    {
      if ((((UnityEngine.Object) componentsInChild == (UnityEngine.Object) null) ? 1 : (componentsInChild.mute ? 1 : 0)) == 0)
      {
        componentsInChild.mute = true;
        this._mutedGrabSources.Add(componentsInChild);
      }
    }
  }

  private void RestoreMutedGrabSources()
  {
    for (int index = 0; index < this._mutedGrabSources.Count; ++index)
    {
      AudioSource mutedGrabSource = this._mutedGrabSources[index];
      if (((UnityEngine.Object) mutedGrabSource != (UnityEngine.Object) null))
        mutedGrabSource.mute = false;
    }
    this._mutedGrabSources.Clear();
  }

  private IEnumerator CycleSpecificRoutine(string[] ids, string handMode = "Both")
  {
    int index = 0;
    while ((this._spammingBeanies || this._spammingRoses ? 1 : (!string.IsNullOrEmpty(this._activeCustomProfileName) ? 1 : 0)) != 0)
    {
      try
      {
        if ((!((UnityEngine.Object) CosmeticsController.instance != (UnityEngine.Object) null) || ids == null ? 0 : (ids.Length != 0 ? 1 : 0)) != 0)
        {
          string id = ids[index];
          CosmeticsController.CosmeticItem item = CosmeticsController.instance.GetItemFromDict(id);
          if ((item.itemName == null ? 0 : (!item.isNullItem ? 1 : 0)) != 0)
          {
            bool applyLeft = false;
            string mode = handMode.ToUpper();
            applyLeft = mode == "LEFT" || !(mode == "RIGHT") && (double) UnityEngine.Random.value > 0.5;
            CosmeticsController.instance.ApplyCosmeticItemToSet(CosmeticsController.instance.currentWornSet, item, applyLeft, true);
            CosmeticsHelper.ApplyAndRefresh();
            mode = (string) null;
          }
          ++index;
          if (index >= ids.Length)
            index = 0;
          id = (string) null;
          item = new CosmeticsController.CosmeticItem();
        }
      }
      catch
      {
      }
      yield return (object) new WaitForSeconds(1f / (float) this._spamSpeed);
    }
  }

  private enum WardrobeState
  {
    Categories,
    Items,
  }

  private struct MissingEntry
  {
    public CosmeticsController.CosmeticItem Item;
    public bool Obtainable;
  }

  private enum CollectionView
  {
    Summary,
    CategoryItems,
  }
}
