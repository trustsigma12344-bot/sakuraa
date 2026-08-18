using Photon.Realtime;
using PlayFab;
using PlayFab.ClientModels;
using SakuraaCastingMod.Core;
using SakuraaCastingMod.Shared.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Video;

#nullable disable
namespace SakuraaCastingMod.Features.Visuals;

public static class RankVisuals
{
  public static readonly Dictionary<string, DateTime> PlayerJoinDates = new Dictionary<string, DateTime>();
  public static Material BasicPackMat;
  public static Material CreatorPackMat;
  public static Material MasterPackMat;
  public static Sprite BasicPackSprite;
  public static Sprite CreatorPackSprite;
  public static Sprite MasterPackSprite;
  private static readonly Dictionary<string, bool> PlayerJoinedAfterPaid = new Dictionary<string, bool>();
  public static readonly Dictionary<VRRig, string> PlayerPlatforms = new Dictionary<VRRig, string>();
  private static readonly HashSet<string> _joinDateInFlight = new HashSet<string>();
  private static readonly HashSet<string> _joinDateFailed = new HashSet<string>();
  private static bool _fetchingJoinDates;
  [SavedSetting("PlatCheckOnNamesEnabled", false)]
  public static bool PlatCheckOnNamesEnabled;
  [SavedSetting("PlatCheckOnLeaderboardEnabled", false)]
  public static bool PlatCheckOnLeaderboardEnabled;
  [SavedSetting("RankCheckOnNamesEnabled", true)]
  public static bool RankCheckOnNamesEnabled = true;
  [SavedSetting("RankCheckOnLeaderboardEnabled", true)]
  public static bool RankCheckOnLeaderboardEnabled = true;
  private static bool _playedVideo;
  public static GameObject PttCanvasObj;
  public static Canvas PttCanvas;
  private static VideoPlayer _videoPlayer;
  private static GameObject _introCanvas;
  public static Material TransparencyMat;
  public static Shader ChromaKeyShader;
  [SavedSetting("ShorHzOnNameTags", false)]
  public static bool ShowHzOnName;
  [SavedSetting("AutoColorHzNameTags", true)]
  public static bool AutoColorHzTag = true;
  private const string ExpectedSafeContent = "{\"ObsidianMC\":\"Obsidian\",\"genesis\":\"Genesis\",\"elux\":\"Elux\",\"VioletFreeUser\":\"Violet Free\",\"Hidden Menu\":\"Hidden\",\"void\":\"Void\",\"6XpyykmrCthKhFeUfkYGxv7xnXpoe2\":\"CCMV2\",\"cronos\":\"Cronos\",\"ORBIT\":\"Orbit (Weeb)\",\"Violet On Top\":\"Violet\",\"ElixirMenu\":\"Elixir\",\"Elixir\":\"Elixir\",\"VioletPaidUser\":\"Violet Paid\",\"EmoteWheel\":\"Emotes\",\"MistUser\":\"Mist\",\"Untitled\":\"Untitled\",\"void_menu_open\":\"Void\",\"dark\":\"ShibaGT Dark\",\"oblivionuser\":\"Oblivion\",\"eyerock reborn\":\"EyeRock\",\"asteroidlite\":\"Asteroid Lite\",\"cokecosmetics\":\"Coke Cosmetx\",\"ØƦƁƖƬ\":\"Orbit\",\"FNgMenu\":\"Fortnite Gooner Menu\",\"y u lookin in here weirdo\":\"Malachi Menu Reborn\",\"Atlas\":\"Atlas\",\"Euphoric\":\"Euphoric\",\"CurrentEmote\":\"Vortex Emotes\"}";

  public static Material GetMatForTier(byte tier)
  {
    Material matForTier;
    switch (tier)
    {
      case 1:
        matForTier = RankVisuals.BasicPackMat;
        break;
      case 2:
        matForTier = RankVisuals.CreatorPackMat;
        break;
      case 3:
        matForTier = RankVisuals.MasterPackMat;
        break;
      default:
        matForTier = (Material) null;
        break;
    }
    return matForTier;
  }

  public static Sprite GetSpriteForTier(byte tier)
  {
    Sprite spriteForTier;
    switch (tier)
    {
      case 1:
        spriteForTier = RankVisuals.BasicPackSprite;
        break;
      case 2:
        spriteForTier = RankVisuals.CreatorPackSprite;
        break;
      case 3:
        spriteForTier = RankVisuals.MasterPackSprite;
        break;
      default:
        spriteForTier = (Sprite) null;
        break;
    }
    return spriteForTier;
  }

  public static void StartShenanigans()
  {
    if (Plugin.Ins.XPosition != 0)
      RankVisuals.PlatCheckOnNamesEnabled = true;
    Plugin.Ins.StartCoroutine(RankVisuals.CheckForVideoUpdateLoop());
  }

  private static IEnumerator FetchJoinDates()
  {
    if (PlayerTranslator.vrrigDict != null && !RankVisuals._fetchingJoinDates)
    {
      RankVisuals._fetchingJoinDates = true;
      List<NetPlayer> playersToCheck = PlayerTranslator.vrrigDict.Values.Where<VRRig>((Func<VRRig, bool>) (rig => ((UnityEngine.Object) rig != (UnityEngine.Object) null) && rig.OwningNetPlayer != null)).Select<VRRig, NetPlayer>((Func<VRRig, NetPlayer>) (rig => rig.OwningNetPlayer)).Where<NetPlayer>((Func<NetPlayer, bool>) (np => !string.IsNullOrEmpty(np.UserId) && !RankVisuals.PlayerJoinDates.ContainsKey(np.UserId) && !RankVisuals._joinDateInFlight.Contains(np.UserId) && !RankVisuals._joinDateFailed.Contains(np.UserId))).ToList<NetPlayer>();
      foreach (NetPlayer netPlayer1 in playersToCheck)
      {
        NetPlayer netPlayer = netPlayer1;
        if (netPlayer != null)
        {
          string userId = netPlayer.UserId;
          if (!RankVisuals.PlayerJoinDates.ContainsKey(userId) && !RankVisuals._joinDateInFlight.Contains(userId) && !RankVisuals._joinDateFailed.Contains(userId))
          {
            RankVisuals._joinDateInFlight.Add(userId);
            // ISSUE: reference to a compiler-generated field
            // ISSUE: reference to a compiler-generated field
            PlayFabClientAPI.GetAccountInfo(new GetAccountInfoRequest()
            {
              PlayFabId = userId
            }, new Action<GetAccountInfoResult>(RankVisuals.HandleGetAccountInfoResponse), (Action<PlayFabError>) (err =>
            {
              RankVisuals._joinDateInFlight.Remove(userId);
              RankVisuals._joinDateFailed.Add(userId);
              RankVisuals.PlayFabErrorHandlingBs(err);
            }), (object) null, (Dictionary<string, string>) null);
            yield return (object) new WaitForSeconds(1f);
            netPlayer = (NetPlayer) null;
          }
        }
      }
      RankVisuals._fetchingJoinDates = false;
    }
  }

  public static void OnPhotonRoomJoined()
  {
    if (((UnityEngine.Object) Plugin.Ins == (UnityEngine.Object) null))
      return;
    Plugin.Ins.StartCoroutine(RankVisuals.DelayedCheckPlatAndRanks());
  }

  public static void OnPhotonPlayerEntered(Player newPlayer)
  {
    if (((UnityEngine.Object) Plugin.Ins == (UnityEngine.Object) null))
      return;
    Plugin.Ins.StartCoroutine(RankVisuals.DelayedCheckPlatAndRanks());
  }

  private static IEnumerator DelayedCheckPlatAndRanks()
  {
    yield return (object) new WaitForSeconds(2f);
    RankVisuals.CheckPlatAndRanks();
  }

  private static void PlayFabErrorHandlingBs(PlayFabError error)
  {
    UnityEngine.Debug.LogError((object) $"PlayFab Error ({error.Error}): {error.ErrorMessage}");
    if (error.ErrorDetails == null)
      return;
    foreach (KeyValuePair<string, List<string>> errorDetail in error.ErrorDetails)
      UnityEngine.Debug.LogError((object) $"  {errorDetail.Key}: {string.Join(", ", (IEnumerable<string>) errorDetail.Value)}");
  }

  private static void HandleGetAccountInfoResponse(GetAccountInfoResult res)
  {
    if (res?.AccountInfo == null)
      return;
    DateTime dateTime = DateTime.Parse("12/13/2022");
    RankVisuals._joinDateInFlight.Remove(res.AccountInfo.PlayFabId);
    RankVisuals.PlayerJoinDates[res.AccountInfo.PlayFabId] = res.AccountInfo.Created;
    if (!(res.AccountInfo.Created > dateTime))
      RankVisuals.PlayerJoinedAfterPaid[res.AccountInfo.PlayFabId] = false;
    else
      RankVisuals.PlayerJoinedAfterPaid[res.AccountInfo.PlayFabId] = true;
  }

  private static string StripSpace(string input)
  {
    return string.IsNullOrEmpty(input) ? "" : new string(input.Where<char>((Func<char, bool>) (c => !char.IsWhiteSpace(c))).ToArray<char>());
  }

  public static void CheckPlatAndRanks()
  {
    if (!Networking.InRoom || (RankVisuals.PlatCheckOnNamesEnabled ? 0 : (!RankVisuals.PlatCheckOnLeaderboardEnabled ? 1 : 0)) != 0)
      return;
    Plugin.Ins.StartCoroutine(RankVisuals.FetchJoinDates());
    RankVisuals.PlayerPlatforms.Clear();
    if (PlayerTranslator.vrrigDict == null)
      return;
    foreach (VRRig key in PlayerTranslator.vrrigDict.Values)
    {
      if (key.OwningNetPlayer != null)
      {
        NetPlayer owningNetPlayer = key.OwningNetPlayer;
        if (key.IsItemAllowed("S. FIRST LOGIN"))
        {
          RankVisuals.PlayerPlatforms.Add(key, "steam");
        }
        else
        {
          bool flag;
          if (RankVisuals.PlayerJoinedAfterPaid.TryGetValue(owningNetPlayer.UserId, out flag) & flag)
            RankVisuals.PlayerPlatforms.Add(key, "oculus");
          else if ((!RankVisuals.PlayerJoinedAfterPaid.ContainsKey(owningNetPlayer.UserId) ? 0 : (!RankVisuals.PlayerJoinedAfterPaid[owningNetPlayer.UserId] ? 1 : 0)) == 0)
            RankVisuals.PlayerPlatforms.Add(key, "...");
          else
            RankVisuals.PlayerPlatforms.Add(key, "?");
        }
      }
    }
  }

  private static IEnumerator CheckForVideoUpdateLoop()
  {
    while (true)
    {
      yield return (object) new WaitForSeconds(67f);
      yield return (object) Plugin.Ins.StartCoroutine(RankVisuals.ChVid());
      yield return (object) Plugin.Ins.StartCoroutine(RankVisuals.CheckForVidUpdate());
    }
  }

  private static IEnumerator CheckForVidUpdate() { yield break; }

  private static void PlayVideo()
  {
  }

  private static IEnumerator ChVid() { yield break; }

  private static void OnVideoPrepared(VideoPlayer source) => source.Play();

  private static void OnVideoError(VideoPlayer source, string message)
  {
    UnityEngine.Debug.LogError((object) ("[Skibidi] Video Player Error: " + message));
    if (((UnityEngine.Object) RankVisuals._introCanvas != (UnityEngine.Object) null))
      UnityEngine.Object.Destroy((UnityEngine.Object) RankVisuals._introCanvas);
    RankVisuals._playedVideo = false;
    Configuration.VidLog();
  }

  private static void OnVideoEnd(VideoPlayer vp)
  {
    if (((UnityEngine.Object) vp != (UnityEngine.Object) null))
    {
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      vp.loopPointReached -= new VideoPlayer.EventHandler(RankVisuals.OnVideoEnd);
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      vp.errorReceived -= new VideoPlayer.ErrorEventHandler(RankVisuals.OnVideoError);
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      vp.prepareCompleted -= new VideoPlayer.EventHandler(RankVisuals.OnVideoPrepared);
      vp.Stop();
    }
    if (((UnityEngine.Object) RankVisuals._videoPlayer != (UnityEngine.Object) null))
    {
      UnityEngine.Object.Destroy((UnityEngine.Object) RankVisuals._videoPlayer);
      RankVisuals._videoPlayer = (VideoPlayer) null;
    }
    if (((UnityEngine.Object) RankVisuals._introCanvas != (UnityEngine.Object) null))
    {
      UnityEngine.Object.Destroy((UnityEngine.Object) RankVisuals._introCanvas);
      RankVisuals._introCanvas = (GameObject) null;
    }
    Configuration.VidLog();
  }
}
