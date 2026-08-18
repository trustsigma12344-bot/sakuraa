using SakuraaCastingMod.Core;
using SakuraaCastingMod.Features.Visuals;
using SakuraaCastingMod.Shared.Helpers;
using SakuraaCastingMod.Shared.Models;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
namespace SakuraaCastingMod.Features.Overlays;

public static class Leaderboard
{
  [SavedSetting("ShowLeaderboard", false)]
  public static bool ShowLeaderboard;
  public static GameObject LeaderboardCanvasObj;
  public static Canvas LeaderboardCanvas;
  public static GameObject PlayerCardObject;
  public static Dictionary<string, CachedPlayerCard> PlayerCards = new Dictionary<string, CachedPlayerCard>();
  private static readonly Color InfectedColor;
  private static readonly HashSet<string> _activePlayerIds;
  private static readonly List<string> _staleIds;

  public static void ToggleLeaderboard()
  {
    Leaderboard.ShowLeaderboard = !Leaderboard.ShowLeaderboard;
    if (Leaderboard.ShowLeaderboard)
      return;
    Leaderboard.Clear();
  }

  public static void Clear()
  {
    foreach (KeyValuePair<string, CachedPlayerCard> playerCard in Leaderboard.PlayerCards)
      UnityEngine.Object.Destroy((UnityEngine.Object) playerCard.Value.GameObject);
    Leaderboard.PlayerCards.Clear();
  }

  private static CachedPlayerCard GetPlayerCard(string playerID)
  {
    CachedPlayerCard cachedPlayerCard = default;
    return (playerID == null ? 0 : (Leaderboard.PlayerCards.TryGetValue(playerID, out cachedPlayerCard) ? 1 : 0)) != 0 ? cachedPlayerCard : (CachedPlayerCard) null;
  }

  private static void AddPlayerCard(string playerID, GameObject playerCardObj)
  {
    if (playerID == null)
      return;
    CachedPlayerCard cachedPlayerCard = new CachedPlayerCard(playerCardObj);
    if (!Leaderboard.PlayerCards.ContainsKey(playerID))
      Leaderboard.PlayerCards.Add(playerID, cachedPlayerCard);
  }

  public static void DrawLeaderboard()
  {
    float leaderboardHeight = Leaderboard.CalculateLeaderboardHeight();
    Rect rect;
    // ISSUE: explicit constructor call
    rect = new Rect(10f, (float) ((double) Screen.height - (double) leaderboardHeight - 10.0), 400f, leaderboardHeight);
    GUILayout.BeginArea(rect);
    Leaderboard._activePlayerIds.Clear();
    for (int index = 0; index < GorillaDataHandler.GorillaDataList.Count; ++index)
    {
      GorillaData gorillaData = GorillaDataHandler.GorillaDataList[index];
      string userId = gorillaData.UserId;
      if ((((UnityEngine.Object) gorillaData.BodyTransform == (UnityEngine.Object) null) ? 1 : (userId == null ? 1 : 0)) == 0)
      {
        Leaderboard._activePlayerIds.Add(userId);
        CachedPlayerCard playerCard = Leaderboard.GetPlayerCard(userId);
        if (playerCard == null)
        {
          GameObject playerCardObj = UnityEngine.Object.Instantiate<GameObject>(Leaderboard.PlayerCardObject, Leaderboard.LeaderboardCanvasObj.transform);
          Leaderboard.AddPlayerCard(userId, playerCardObj);
          playerCard = Leaderboard.GetPlayerCard(userId);
        }
        Leaderboard.SetPlayerCardProperties(playerCard, gorillaData, index, GorillaDataHandler.GorillaDataList.Count);
      }
    }
    Leaderboard._staleIds.Clear();
    foreach (KeyValuePair<string, CachedPlayerCard> playerCard in Leaderboard.PlayerCards)
    {
      if (!Leaderboard._activePlayerIds.Contains(playerCard.Key))
        Leaderboard._staleIds.Add(playerCard.Key);
    }
    foreach (string staleId in Leaderboard._staleIds)
    {
      CachedPlayerCard cachedPlayerCard;
      if (Leaderboard.PlayerCards.TryGetValue(staleId, out cachedPlayerCard))
      {
        UnityEngine.Object.Destroy((UnityEngine.Object) cachedPlayerCard.GameObject);
        Leaderboard.PlayerCards.Remove(staleId);
      }
    }
    GUILayout.EndArea();
  }

  private static void SetPlayerCardProperties(
    CachedPlayerCard card,
    GorillaData gorilla,
    int index,
    int playerCount)
  {
    if ((card == null ? 1 : (gorilla == null ? 1 : 0)) != 0)
      return;
    float leaderboardPosX = LayoutEditor.LeaderboardPosX;
    float num = (float) (playerCount - index) * 36f + LayoutEditor.LeaderboardPosY;
    card.Transform.position = (new Vector2(leaderboardPosX, num));
    card.Transform.localScale = ((Vector2.one * 1.7f));
    bool flag1 = false;
    byte tier = default;
    if ((!RankVisuals.RankCheckOnLeaderboardEnabled || !((UnityEngine.Object) SecureNetworkManager.Instance != (UnityEngine.Object) null) || !((UnityEngine.Object) card.RankImage != (UnityEngine.Object) null) || string.IsNullOrEmpty(gorilla.UserId) ? 0 : (SecureNetworkManager.Instance.TryGetRemoteTier(gorilla.UserId, out tier) ? 1 : 0)) != 0)
    {
      Sprite spriteForTier = RankVisuals.GetSpriteForTier(tier);
      if (((UnityEngine.Object) spriteForTier != (UnityEngine.Object) null))
      {
        card.RankImage.sprite = spriteForTier;
        flag1 = true;
      }
    }
    if ((!((UnityEngine.Object) card.RankObject != (UnityEngine.Object) null) ? 0 : (card.RankObject.activeSelf != flag1 ? 1 : 0)) != 0)
      card.RankObject.SetActive(flag1);
    bool flag2 = false;
    bool flag3 = false;
    if (RankVisuals.PlatCheckOnLeaderboardEnabled)
    {
      VRRig rigByGorilla = PlayerTranslator.GetRigByGorilla(gorilla);
      string str = default;
      if ((!((UnityEngine.Object) rigByGorilla != (UnityEngine.Object) null) ? 0 : (RankVisuals.PlayerPlatforms.TryGetValue(rigByGorilla, out str) ? 1 : 0)) != 0)
      {
        switch (str)
        {
          case "steam":
            flag2 = true;
            break;
          case "oculus":
            flag3 = true;
            break;
        }
      }
    }
    if (card.SteamIcon.activeSelf != flag2)
      card.SteamIcon.SetActive(flag2);
    if (card.MetaIcon.activeSelf != flag3)
      card.MetaIcon.SetActive(flag3);
    if (((UnityEngine.Object) card.BackgroundImage != (UnityEngine.Object) null))
      ((Graphic) card.BackgroundImage).color = gorilla.Infected ? Leaderboard.InfectedColor : gorilla.Color;
    if (((UnityEngine.Object) card.NumberText != (UnityEngine.Object) null))
    {
      card.NumberText.text = $"{index}";
      if (((UnityEngine.Object) card.NumberText.font != (UnityEngine.Object) Plugin.Ins.defaultFontAsset))
        card.NumberText.font = Plugin.Ins.defaultFontAsset;
    }
    if (!((UnityEngine.Object) card.NameText != (UnityEngine.Object) null))
      return;
    if (card.NameText.text != gorilla.UserName)
      card.NameText.text = gorilla.UserName;
    if (!((UnityEngine.Object) card.NameText.font != (UnityEngine.Object) Plugin.Ins.defaultFontAsset))
      return;
    card.NameText.font = Plugin.Ins.defaultFontAsset;
  }

  private static float CalculateLeaderboardHeight()
  {
    return (float) (GorillaDataHandler.GorillaDataList.Count * 40);
  }

  static Leaderboard()
  {
    Color color = default;
    Leaderboard.InfectedColor = ColorUtility.TryParseHtmlString("#8C1A00", out color) ? color : Color.white;
    Leaderboard._activePlayerIds = new HashSet<string>();
    Leaderboard._staleIds = new List<string>();
  }
}
