using Photon.Realtime;
using SakuraaCastingMod.Core;
using SakuraaCastingMod.Shared.Helpers;
using SakuraaCastingMod.Shared.Models;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
namespace SakuraaCastingMod.Features.Overlays;

public static class KillFeed
{
  [SavedSetting("KillFeedEnabled", false)]
  public static bool KillFeedEnabled;
  public static GameObject KillFeedCanvasObj;
  public static GameObject KillFeedCardObject;
  private static readonly List<KillFeed.KillFeedItem> ActiveFeedItems = new List<KillFeed.KillFeedItem>();
  private static readonly Queue<KillFeed.KillFeedItem> Pool = new Queue<KillFeed.KillFeedItem>();

  public static int ActiveItemCount => KillFeed.ActiveFeedItems.Count;

  public static void ToggleKillFeed()
  {
    KillFeed.KillFeedEnabled = !KillFeed.KillFeedEnabled;
    if (KillFeed.KillFeedEnabled)
      return;
    KillFeed.ClearFeed();
  }

  private static void ClearFeed()
  {
    foreach (KillFeed.KillFeedItem activeFeedItem in KillFeed.ActiveFeedItems)
    {
      if (activeFeedItem.TimerRoutine != null)
        Plugin.Ins.StopCoroutine(activeFeedItem.TimerRoutine);
      activeFeedItem.GameObject.SetActive(false);
      KillFeed.Pool.Enqueue(activeFeedItem);
    }
    KillFeed.ActiveFeedItems.Clear();
  }

  public static void CreateKillfeedCard(Player tagger, Player tagged)
  {
    if (!KillFeed.KillFeedEnabled)
      return;
    if ((((UnityEngine.Object) GorillaGameManager.instance == (UnityEngine.Object) null) ? 1 : (GorillaGameManager.instance.GameModeName() != "INFECTION" ? 1 : 0)) == 0)
    {
      GorillaData gorillaData1;
      GorillaData gorillaData2 = default;
      if ((!GorillaDataHandler.GorillaDataDict.TryGetValue(tagger.UserId, out gorillaData1) ? 1 : (!GorillaDataHandler.GorillaDataDict.TryGetValue(tagged.UserId, out gorillaData2) ? 1 : 0)) != 0)
        return;
      KillFeed.KillFeedItem cardFromPool = KillFeed.GetCardFromPool();
      cardFromPool.TaggerName.text = tagger.NickName;
      cardFromPool.TaggedName.text = tagged.NickName;
      ((Graphic) cardFromPool.TaggerBg).color = gorillaData1.Color;
      ((Graphic) cardFromPool.TaggerMiddleBg).color = gorillaData1.Color;
      ((Graphic) cardFromPool.TaggedBg).color = gorillaData2.Color;
      cardFromPool.GameObject.transform.localScale = (Vector3.one * 1.7f);
      cardFromPool.GameObject.SetActive(true);
      if (cardFromPool.TimerRoutine != null)
        Plugin.Ins.StopCoroutine(cardFromPool.TimerRoutine);
      cardFromPool.TimerRoutine = Plugin.Ins.StartCoroutine(KillFeed.KillFeedDestroyTimer(cardFromPool));
      KillFeed.ActiveFeedItems.Add(cardFromPool);
      KillFeed.RecalibrateKillFeedPositions();
    }
    else
      Notification.Send("You need to be in an infection lobby for killfeed!", Color.red);
  }

  private static KillFeed.KillFeedItem GetCardFromPool()
  {
    KillFeed.KillFeedItem cardFromPool;
    if (KillFeed.Pool.Count > 0)
    {
      KillFeed.KillFeedItem killFeedItem = KillFeed.Pool.Dequeue();
      killFeedItem.GameObject.transform.SetAsLastSibling();
      cardFromPool = killFeedItem;
    }
    else
    {
      GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(KillFeed.KillFeedCardObject, KillFeed.KillFeedCanvasObj.transform);
      cardFromPool = new KillFeed.KillFeedItem()
      {
        GameObject = gameObject,
        RectTransform = gameObject.GetComponent<RectTransform>(),
        TaggerName = ((Component) gameObject.transform.Find("TaggerPlayerName")).GetComponent<TMP_Text>(),
        TaggedName = ((Component) gameObject.transform.Find("TaggedPlayerName")).GetComponent<TMP_Text>(),
        TaggerBg = ((Component) gameObject.transform.Find("CurvedBehind/TaggerPlayerBg")).GetComponent<Image>(),
        TaggerMiddleBg = ((Component) gameObject.transform.Find("TaggerMiddleBg")).GetComponent<Image>(),
        TaggedBg = ((Component) gameObject.transform.Find("TaggedPlayerBg")).GetComponent<Image>()
      };
    }
    return cardFromPool;
  }

  private static IEnumerator KillFeedDestroyTimer(KillFeed.KillFeedItem item)
  {
    yield return (object) new WaitForSeconds(30f);
    if (KillFeed.ActiveFeedItems.Contains(item))
    {
      KillFeed.ActiveFeedItems.Remove(item);
      item.GameObject.SetActive(false);
      KillFeed.Pool.Enqueue(item);
      KillFeed.RecalibrateKillFeedPositions();
    }
  }

  private static void RecalibrateKillFeedPositions()
  {
    float num1 = (float) Screen.height + LayoutEditor.KillFeedOffsetY;
    if (Scoreboard.CurrentScoreboard == 2)
      num1 -= 180f;
    float num2 = (float) Screen.width - 210f + LayoutEditor.KillFeedOffsetX;
    for (int index = 0; index < KillFeed.ActiveFeedItems.Count; ++index)
      KillFeed.ActiveFeedItems[index].GameObject.transform.position = new Vector3(num2, num1 - (float) (index + 1) * 40f, 0.0f);
  }

  private class KillFeedItem
  {
    public GameObject GameObject;
    public RectTransform RectTransform;
    public TMP_Text TaggerName;
    public TMP_Text TaggedName;
    public Image TaggerBg;
    public Image TaggerMiddleBg;
    public Image TaggedBg;
    public Coroutine TimerRoutine;
  }
}
