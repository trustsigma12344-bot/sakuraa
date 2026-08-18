using SakuraaCastingMod.Core;
using SakuraaCastingMod.Shared.Helpers;
using SakuraaCastingMod.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Features.Overlays;

public static class LavaDistance
{
  public static GameObject PlayerDistanceCard;
  public static GameObject PlayerDistanceCanvasObj;
  public static Canvas PlayerDistanceCanvas;
  [SavedSetting("ShowDesktopTagDistance", false)]
  public static bool ShowDistanceCard;
  [SavedSetting("DistanceOffsetX", 0.0f)]
  public static float DistanceOffsetX;
  [SavedSetting("DistanceOffsetY", 0.0f)]
  public static float DistanceOffsetY;
  public static GorillaData CurrentSpecGorilla;
  public static GameObject PlayerDistanceCardObject;
  private static float _distCooldown;
  private static readonly Dictionary<GorillaData, float> UILavaDistances = new Dictionary<GorillaData, float>();

  public static void UpdateDistances()
  {
    LavaDistance._distCooldown -= Time.deltaTime;
    if ((double) LavaDistance._distCooldown > 0.0)
      return;
    LavaDistance._distCooldown = 0.2f;
    if ((!Networking.InRoom ? 1 : (((UnityEngine.Object) Networking.GtagManager == (UnityEngine.Object) null) ? 1 : 0)) != 0 || (AutoPilot.AutoPilotEnabled ? 0 : (!LavaDistance.ShowDistanceCard ? 1 : 0)) != 0)
      return;
    if (!(((GorillaGameManager) Networking.GtagManager).GameModeName() != "INFECTION"))
    {
      string localUserId = NetworkSystem.Instance.LocalPlayer.UserId;
      List<GorillaData> list1 = GorillaDataHandler.GorillaDataList.Where<GorillaData>((Func<GorillaData, bool>) (gorilla => gorilla.Infected && gorilla.UserId != localUserId && ((UnityEngine.Object) gorilla.BodyTransform != (UnityEngine.Object) null))).ToList<GorillaData>();
      List<GorillaData> list2 = GorillaDataHandler.GorillaDataList.Where<GorillaData>((Func<GorillaData, bool>) (gorilla => !gorilla.Infected && ((UnityEngine.Object) gorilla.BodyTransform != (UnityEngine.Object) null))).ToList<GorillaData>();
      LavaDistance.UILavaDistances.Clear();
      foreach (GorillaData gorillaData in list2)
      {
        GorillaData runnerGorilla = gorillaData;
        float num = float.MaxValue;
        if (list1.Count > 0)
          num = list1.Select<GorillaData, float>((Func<GorillaData, float>) (taggerRig => Vector3.Distance(runnerGorilla.BodyTransform.position, taggerRig.BodyTransform.position))).Min();
        LavaDistance.UILavaDistances[runnerGorilla] = num;
      }
    }
    else
      Notification.Send("You need to be in an infection lobby to use distance calculations!", Color.red);
  }

  public static void DrawDistanceCard()
  {
    if (((UnityEngine.Object) LavaDistance.PlayerDistanceCard == (UnityEngine.Object) null))
    {
      LavaDistance.PlayerDistanceCard = UnityEngine.Object.Instantiate<GameObject>(LavaDistance.PlayerDistanceCardObject, LavaDistance.PlayerDistanceCanvasObj.transform);
      LavaDistance.PlayerDistanceCard.transform.localScale = ((Vector2.one * 1.7f));
    }
    LavaDistance.PlayerDistanceCard.transform.position = new Vector3((float) Screen.width / 2f + LavaDistance.DistanceOffsetX, LavaDistance.DistanceOffsetY, 0.0f);
    if (LavaDistance.CurrentSpecGorilla == null)
      return;
    if (LavaDistance.CurrentSpecGorilla.Infected)
    {
      LavaDistance.PlayerDistanceCard.SetActive(false);
    }
    else
    {
      TMP_Text component = ((Component) LavaDistance.PlayerDistanceCard.transform.Find("DistanceCounterText")).GetComponent<TMP_Text>();
      component.font = Plugin.Ins.defaultFontAsset;
      float num;
      if (!LavaDistance.UILavaDistances.TryGetValue(LavaDistance.CurrentSpecGorilla, out num))
      {
        LavaDistance.PlayerDistanceCard.SetActive(false);
      }
      else
      {
        string str = (double) num > 9999.0 ? "-" : $"{Math.Round((double) num, 0)}";
        component.text = $"Lava Distance: {str}ft";
        LavaDistance.PlayerDistanceCard.SetActive(true);
      }
    }
  }

  public static void ToggleDistanceCard()
  {
    LavaDistance.ShowDistanceCard = !LavaDistance.ShowDistanceCard;
    if (LavaDistance.ShowDistanceCard)
      return;
    UnityEngine.Object.Destroy((UnityEngine.Object) LavaDistance.PlayerDistanceCard);
  }
}
