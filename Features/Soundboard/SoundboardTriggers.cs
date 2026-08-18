using Photon.Pun;
using Photon.Realtime;
using SakuraaCastingMod.Core;
using SakuraaCastingMod.Shared.Helpers;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Features.Soundboard;

public static class SoundboardTriggers
{
  [SavedSetting("SoundboardTrigTagSomeone", "")]
  public static string TagSomeoneSound = "";
  [SavedSetting("SoundboardTrigGetTagged", "")]
  public static string GetTaggedSound = "";
  [SavedSetting("SoundboardTrigFirstTagged", "")]
  public static string FirstTaggedSound = "";
  [SavedSetting("SoundboardTrigRoundStart", "")]
  public static string RoundStartSound = "";
  [SavedSetting("SoundboardTrigRoundEnd", "")]
  public static string RoundEndSound = "";
  public static readonly (SoundboardTriggers.Trigger trig, string label)[] Defs = new (SoundboardTriggers.Trigger, string)[5]
  {
    (SoundboardTriggers.Trigger.TagSomeone, "TAG SOMEONE"),
    (SoundboardTriggers.Trigger.GetTagged, "GET TAGGED"),
    (SoundboardTriggers.Trigger.FirstTagged, "FIRST TAGGED"),
    (SoundboardTriggers.Trigger.RoundStart, "ROUND START"),
    (SoundboardTriggers.Trigger.RoundEnd, "ROUND OVER")
  };
  private static bool _hadState;
  private static readonly HashSet<int> _prevTagged = new HashSet<int>();
  private static bool _prevLocalTagged;
  private static bool _prevAllTagged;
  private static readonly Dictionary<SoundboardTriggers.Trigger, float> _lastFire = new Dictionary<SoundboardTriggers.Trigger, float>();
  private const float Cooldown = 0.8f;

  public static string GetSoundId(SoundboardTriggers.Trigger t)
  {
    string soundId;
    switch (t)
    {
      case SoundboardTriggers.Trigger.TagSomeone:
        soundId = SoundboardTriggers.TagSomeoneSound;
        break;
      case SoundboardTriggers.Trigger.GetTagged:
        soundId = SoundboardTriggers.GetTaggedSound;
        break;
      case SoundboardTriggers.Trigger.FirstTagged:
        soundId = SoundboardTriggers.FirstTaggedSound;
        break;
      case SoundboardTriggers.Trigger.RoundStart:
        soundId = SoundboardTriggers.RoundStartSound;
        break;
      case SoundboardTriggers.Trigger.RoundEnd:
        soundId = SoundboardTriggers.RoundEndSound;
        break;
      default:
        soundId = "";
        break;
    }
    return soundId;
  }

  public static void SetSoundId(SoundboardTriggers.Trigger t, string id)
  {
    id = id ?? "";
    switch (t)
    {
      case SoundboardTriggers.Trigger.TagSomeone:
        SoundboardTriggers.TagSomeoneSound = id;
        break;
      case SoundboardTriggers.Trigger.GetTagged:
        SoundboardTriggers.GetTaggedSound = id;
        break;
      case SoundboardTriggers.Trigger.FirstTagged:
        SoundboardTriggers.FirstTaggedSound = id;
        break;
      case SoundboardTriggers.Trigger.RoundStart:
        SoundboardTriggers.RoundStartSound = id;
        break;
      case SoundboardTriggers.Trigger.RoundEnd:
        SoundboardTriggers.RoundEndSound = id;
        break;
    }
    Configuration.SaveSettings();
  }

  public static void AllOff()
  {
    string str;
    SoundboardTriggers.RoundEndSound = str = "";
    SoundboardTriggers.RoundStartSound = str;
    SoundboardTriggers.FirstTaggedSound = str;
    SoundboardTriggers.GetTaggedSound = str;
    SoundboardTriggers.TagSomeoneSound = str;
    Configuration.SaveSettings();
  }

  public static bool AnyEnabled
  {
    get
    {
      return !string.IsNullOrEmpty(SoundboardTriggers.TagSomeoneSound) || !string.IsNullOrEmpty(SoundboardTriggers.GetTaggedSound) || !string.IsNullOrEmpty(SoundboardTriggers.FirstTaggedSound) || !string.IsNullOrEmpty(SoundboardTriggers.RoundStartSound) || !string.IsNullOrEmpty(SoundboardTriggers.RoundEndSound);
    }
  }

  public static void NotifyLocalTag()
  {
    SoundboardTriggers.Fire(SoundboardTriggers.Trigger.TagSomeone);
  }

  public static void Tick()
  {
    if (SoundboardTriggers.AnyEnabled)
    {
      GorillaTagManager gtagManager = Networking.GtagManager;
      Player localPlayer = PhotonNetwork.LocalPlayer;
      Player[] playerList = PhotonNetwork.PlayerList;
      if ((!Networking.InRoom || ((UnityEngine.Object) gtagManager == (UnityEngine.Object) null) || localPlayer == null || playerList == null ? 1 : (playerList.Length == 0 ? 1 : 0)) == 0)
      {
        HashSet<int> intSet1 = new HashSet<int>();
        foreach (Player player in playerList)
        {
          if (player != null)
            intSet1.Add(player.ActorNumber);
        }
        HashSet<int> intSet2 = new HashSet<int>();
        if (gtagManager.currentInfected != null)
        {
          foreach (NetPlayer netPlayer in gtagManager.currentInfected)
          {
            if ((netPlayer == null ? 0 : (intSet1.Contains(netPlayer.ActorNumber) ? 1 : 0)) != 0)
              intSet2.Add(netPlayer.ActorNumber);
          }
        }
        if ((gtagManager.currentIt == null ? 0 : (intSet1.Contains(gtagManager.currentIt.ActorNumber) ? 1 : 0)) != 0)
          intSet2.Add(gtagManager.currentIt.ActorNumber);
        bool flag1 = intSet2.Contains(localPlayer.ActorNumber);
        int count = intSet1.Count;
        bool flag2 = count > 1 && intSet2.Count >= count;
        if (SoundboardTriggers._hadState)
        {
          bool flag3 = false;
          foreach (int num in SoundboardTriggers._prevTagged)
          {
            if ((!intSet1.Contains(num) ? 0 : (!intSet2.Contains(num) ? 1 : 0)) != 0)
            {
              flag3 = true;
              break;
            }
          }
          if ((flag3 || SoundboardTriggers._prevTagged.Count != 0 || intSet2.Count <= 0 ? 0 : (!flag2 ? 1 : 0)) != 0)
            flag3 = true;
          if (!flag3)
          {
            if ((!flag2 ? 0 : (!SoundboardTriggers._prevAllTagged ? 1 : 0)) != 0)
              SoundboardTriggers.Fire(SoundboardTriggers.Trigger.RoundEnd);
            if ((!flag1 ? 0 : (!SoundboardTriggers._prevLocalTagged ? 1 : 0)) != 0)
              SoundboardTriggers.Fire(SoundboardTriggers.Trigger.GetTagged);
          }
          else
          {
            SoundboardTriggers.Fire(SoundboardTriggers.Trigger.RoundStart);
            if (flag1)
              SoundboardTriggers.Fire(SoundboardTriggers.Trigger.FirstTagged);
          }
        }
        SoundboardTriggers._prevTagged.Clear();
        foreach (int num in intSet2)
          SoundboardTriggers._prevTagged.Add(num);
        SoundboardTriggers._prevLocalTagged = flag1;
        SoundboardTriggers._prevAllTagged = flag2;
        SoundboardTriggers._hadState = true;
      }
      else
        SoundboardTriggers.ResetState();
    }
    else
      SoundboardTriggers.ResetState();
  }

  private static void ResetState()
  {
    if (!SoundboardTriggers._hadState)
      return;
    SoundboardTriggers._prevTagged.Clear();
    SoundboardTriggers._prevLocalTagged = false;
    SoundboardTriggers._prevAllTagged = false;
    SoundboardTriggers._hadState = false;
  }

  private static void Fire(SoundboardTriggers.Trigger t)
  {
    string soundId = SoundboardTriggers.GetSoundId(t);
    if (string.IsNullOrEmpty(soundId))
      return;
    float realtimeSinceStartup = Time.realtimeSinceStartup;
    float num;
    if ((!SoundboardTriggers._lastFire.TryGetValue(t, out num) ? 0 : ((double) realtimeSinceStartup - (double) num < 0.800000011920929 ? 1 : 0)) != 0)
      return;
    SoundboardTriggers._lastFire[t] = realtimeSinceStartup;
    SoundboardManager.Sound sound = SoundboardManager.FindSound(soundId);
    if (sound == null)
      return;
    SoundboardPlayer.Play(sound);
  }

  public enum Trigger
  {
    TagSomeone,
    GetTagged,
    FirstTagged,
    RoundStart,
    RoundEnd,
  }
}
