using GorillaNetworking;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Features.Replay.Recording;

public class CosmeticCollector
{
  private Dictionary<string, CosmeticItemsCache> CosmeticsCache = new Dictionary<string, CosmeticItemsCache>();
  private List<string> FailingCosmetics = new List<string>();
  private Dictionary<string, List<CosmeticData>> LastUpdateCosmetics = new Dictionary<string, List<CosmeticData>>();

  public void Store(SakuraaCastingMod.Features.Replay.Recording.Replay replay, VRRig vrrig, NetPlayer player, int replayTime)
  {
    GameObject cosmeticGO = (GameObject) null;
    if ((((UnityEngine.Object) vrrig == (UnityEngine.Object) null) ? 1 : (player == null ? 1 : 0)) != 0)
      return;
    string str = player.ActorNumber.ToString();
    List<CosmeticData> cosmeticDataList = new List<CosmeticData>();
    foreach (CosmeticsController.CosmeticItem cosmeticItem in vrrig.cosmeticSet.items)
    {
      bool flag = false;
      if ((!(cosmeticItem.displayName != "NOTHING") || cosmeticItem.displayName.Length <= 3 ? 0 : (!this.FailingCosmetics.Contains(cosmeticItem.displayName) ? 1 : 0)) != 0)
        flag = true;
      if ((!flag ? 0 : (!this.CosmeticsCache.ContainsKey(str + cosmeticItem.displayName) ? 1 : 0)) != 0)
      {
        CosmeticItemsCache cosmeticItemsCache = new CosmeticItemsCache();
        cosmeticGO = (GameObject) null;
        if (((UnityEngine.Object) cosmeticGO == (UnityEngine.Object) null))
          RecursiveFind(((Component) vrrig).gameObject.transform, cosmeticItem.displayName);
        if ((!((UnityEngine.Object) cosmeticGO == (UnityEngine.Object) null) ? 0 : (((UnityEngine.Object) ((Component) vrrig).gameObject).name == "Local Gorilla Player" ? 1 : 0)) != 0)
        {
          GameObject gameObject = GameObject.Find("Player Objects/Player VR Controller/GorillaPlayer/TurnParent/Main Camera/FirstPersonCosmeticsOverrides");
          if (((UnityEngine.Object) gameObject != (UnityEngine.Object) null))
            RecursiveFind(gameObject.transform, cosmeticItem.displayName);
        }
        if ((!((UnityEngine.Object) cosmeticGO == (UnityEngine.Object) null) ? 0 : (((UnityEngine.Object) ((Component) vrrig).gameObject).name == "Local Gorilla Player" ? 1 : 0)) != 0)
        {
          GameObject gameObject = GameObject.Find("Player Objects/Player VR Controller/GorillaPlayer/TurnParent/Main Camera/FirstPersonCosmeticsOverrides");
          if (((UnityEngine.Object) gameObject != (UnityEngine.Object) null))
            RecursiveFind(gameObject.transform, cosmeticItem.overrideDisplayName);
        }
        if ((!((UnityEngine.Object) cosmeticGO == (UnityEngine.Object) null) ? 0 : (((UnityEngine.Object) ((Component) vrrig).gameObject).name == "Local Gorilla Player" ? 1 : 0)) != 0)
        {
          GameObject gameObject = GameObject.Find("Player Objects/Player VR Controller/GorillaPlayer/TurnParent/Main Camera");
          if (((UnityEngine.Object) gameObject != (UnityEngine.Object) null))
            RecursiveFind(gameObject.transform, cosmeticItem.displayName);
        }
        if ((!((UnityEngine.Object) cosmeticGO == (UnityEngine.Object) null) ? 0 : (((UnityEngine.Object) ((Component) vrrig).gameObject).name == "Local Gorilla Player" ? 1 : 0)) != 0)
        {
          GameObject gameObject = GameObject.Find("Player Objects/Player VR Controller/GorillaPlayer/TurnParent/Main Camera");
          if (((UnityEngine.Object) gameObject != (UnityEngine.Object) null))
            RecursiveFind(gameObject.transform, cosmeticItem.overrideDisplayName);
        }
        if ((!((UnityEngine.Object) cosmeticGO == (UnityEngine.Object) null) ? 0 : (cosmeticItem.overrideDisplayName.Length > 3 ? 1 : 0)) != 0)
          RecursiveFind(((Component) vrrig).gameObject.transform, cosmeticItem.overrideDisplayName);
        if (!((UnityEngine.Object) cosmeticGO != (UnityEngine.Object) null))
        {
          this.FailingCosmetics.Add(cosmeticItem.displayName);
        }
        else
        {
          cosmeticItemsCache.CosmeticItem = cosmeticGO;
          cosmeticItemsCache.Transferrable = cosmeticGO.GetComponent<TransferrableObject>();
          if (((UnityEngine.Object) cosmeticItemsCache.Transferrable == (UnityEngine.Object) null))
            cosmeticItemsCache.Transferrable = cosmeticGO.GetComponentInChildren<TransferrableObject>();
          this.CosmeticsCache.Add(str + cosmeticItem.displayName, cosmeticItemsCache);
        }
      }
      else if (flag)
      {
        CosmeticItemsCache cosmeticItemsCache = this.CosmeticsCache[str + cosmeticItem.displayName];
        if (cosmeticItemsCache != null)
        {
          CosmeticData cosmeticData = new CosmeticData();
          cosmeticData.isActive = true;
          cosmeticData.actorNumber = player.ActorNumber;
          cosmeticData.displayName = cosmeticItem.displayName;
          if (((UnityEngine.Object) cosmeticItemsCache.Transferrable == (UnityEngine.Object) null))
          {
            cosmeticData.holdable = false;
            cosmeticData.state = 0;
          }
          else
          {
            cosmeticData.holdable = true;
            cosmeticData.state = (int) cosmeticItemsCache.Transferrable.currentState;
          }
          cosmeticDataList.Add(cosmeticData);
        }
      }
    }
    if (this.LastUpdateCosmetics.ContainsKey(player.UserId))
    {
      bool flag1 = false;
      if (this.LastUpdateCosmetics[player.UserId].Count != cosmeticDataList.Count)
        flag1 = true;
      foreach (CosmeticData c1 in cosmeticDataList)
      {
        bool flag2 = false;
        foreach (CosmeticData c2 in this.LastUpdateCosmetics[player.UserId])
        {
          if (CosmeticsAreTheSame(c1, c2))
            flag2 = true;
        }
        if (!flag2)
          flag1 = true;
      }
      if (flag1)
      {
        foreach (PlayerData playerData in replay.playerDatas)
        {
          if (playerData.actorNumber == player.ActorNumber)
            playerData.cosmeticsDatas.Add(new CosmeticsData()
            {
              time = replayTime,
              cosmeticsDatas = cosmeticDataList
            });
        }
      }
    }
    else
    {
      foreach (PlayerData playerData in replay.playerDatas)
      {
        if (playerData.actorNumber == player.ActorNumber)
          playerData.cosmeticsDatas.Add(new CosmeticsData()
          {
            time = replayTime,
            cosmeticsDatas = cosmeticDataList
          });
      }
    }
    this.LastUpdateCosmetics[player.UserId] = cosmeticDataList;

    void RecursiveFind(Transform start, string SearchName)
    {
      if ((((UnityEngine.Object) start == (UnityEngine.Object) null) ? 1 : (((UnityEngine.Object) ((Component) start).gameObject == (UnityEngine.Object) null) ? 1 : 0)) != 0)
        return;
      if ((!((UnityEngine.Object) cosmeticGO == (UnityEngine.Object) null) ? 0 : (((UnityEngine.Object) ((Component) start).gameObject).name.Contains(SearchName) ? 1 : 0)) != 0)
        cosmeticGO = ((Component) start).gameObject;
      foreach (Transform start1 in start)
        RecursiveFind(start1, SearchName);
    }

    static bool CosmeticsAreTheSame(CosmeticData c1, CosmeticData c2)
    {
      bool flag = false;
      if ((c1.state != c2.state || c1.holdable != c2.holdable || c1.actorNumber != c2.actorNumber || c1.isActive != c2.isActive ? 0 : (c1.displayName == c2.displayName ? 1 : 0)) != 0)
        flag = true;
      return flag;
    }
  }
}
