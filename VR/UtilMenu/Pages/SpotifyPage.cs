using SakuraaCastingMod.Core;
using SakuraaCastingMod.Shared.Integrations;
using SakuraaCastingMod.Shared.Models;
using SakuraaCastingMod.VR.Interaction;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

#nullable disable
namespace SakuraaCastingMod.VR.UtilMenu.Pages;

public class SpotifyPage : BasePage
{
  private UtilTab _premiumTab;
  private UtilTab _nonPremiumTab;
  private string _lastAlbumUrl;
  private Texture2D _currentCoverTex;
  private Coroutine _coverFetchCo;
  private bool _eventsWired;
  private bool _lastConnectionState;

  public override string PageName => "SPOTIFY";

  public override Material PageIcon => UtilMenuMain.Instance.Icons.Spotify;

  public override void BuildTabs()
  {
    this.Tabs.Clear();
    bool flag = ((UnityEngine.Object) SpotifyManager.Instance != (UnityEngine.Object) null) && SpotifyManager.Instance.IsConnected;
    this._lastConnectionState = flag;
    this._premiumTab = new UtilTab()
    {
      TabIcon = UtilMenuMain.Instance.Icons.Spotify,
      TabName = flag ? "Premium" : "Premium (Locked)"
    };
    if (!flag)
      this._premiumTab.Elements.Add(new MenuElement((!((UnityEngine.Object) SpotifyManager.Instance != (UnityEngine.Object) null) ? 0 : (SpotifyManager.Instance.HasClientId ? 1 : 0)) != 0 ? "LOGIN TO SPOTIFY" : "SET CLIENT ID IN LOADER", (Action) (() =>
      {
        if ((!((UnityEngine.Object) SpotifyManager.Instance != (UnityEngine.Object) null) ? 0 : (!SpotifyManager.Instance.IsConnected ? 1 : 0)) == 0)
          return;
        SpotifyManager.Instance.ConnectToSpotify();
      })));
    this.Tabs.Add(this._premiumTab);
    this._nonPremiumTab = new UtilTab()
    {
      TabIcon = UtilMenuMain.Instance.Icons.Server,
      TabName = "Basic"
    };
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    this._nonPremiumTab.Elements.Add(new MenuElement("PLAY / PAUSE", new Action(SpotifyManager.NativePlayPause)));
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    this._nonPremiumTab.Elements.Add(new MenuElement("NEXT TRACK", new Action(SpotifyManager.NativeNextTrack)));
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    this._nonPremiumTab.Elements.Add(new MenuElement("PREV TRACK", new Action(SpotifyManager.NativePreviousTrack)));
    this.Tabs.Add(this._nonPremiumTab);
  }

  public override void Start()
  {
    this.WireButtonsOnce();
    UtilMenuController instance = UtilMenuController.Instance;
    if (!((UnityEngine.Object) instance == (UnityEngine.Object) null))
    {
      if (((UnityEngine.Object) instance.spotifyRoot == (UnityEngine.Object) null))
      {
        UnityEngine.Debug.LogWarning((object) "[SpotifyPage] spotifyRoot is null - the Unity prefab Inspector field on UtilMenuController is not assigned. Drag the Spotify GameObject into the spotifyRoot slot to fix bleed-through.");
      }
      else
      {
        if (instance.spotifyRoot.activeSelf)
          instance.spotifyRoot.SetActive(false);
        if (((UnityEngine.Object) instance.spotifyAlbumCoverRenderer == (UnityEngine.Object) null))
          UnityEngine.Debug.LogWarning((object) "[SpotifyPage] spotifyAlbumCoverRenderer not wired (album cover won't render).");
        if (((UnityEngine.Object) instance.spotifySongTitleText == (UnityEngine.Object) null))
          UnityEngine.Debug.LogWarning((object) "[SpotifyPage] spotifySongTitleText not wired.");
        if (((UnityEngine.Object) instance.spotifyArtistNameText == (UnityEngine.Object) null))
          UnityEngine.Debug.LogWarning((object) "[SpotifyPage] spotifyArtistNameText not wired.");
        if (((UnityEngine.Object) instance.spotifyVolumeValueText == (UnityEngine.Object) null))
          UnityEngine.Debug.LogWarning((object) "[SpotifyPage] spotifyVolumeValueText not wired.");
        if (((UnityEngine.Object) instance.spotifyPausePlayBtn == (UnityEngine.Object) null))
          UnityEngine.Debug.LogWarning((object) "[SpotifyPage] spotifyPausePlayBtn not wired.");
        if (((UnityEngine.Object) instance.spotifyPauseIcon == (UnityEngine.Object) null))
          UnityEngine.Debug.LogWarning((object) "[SpotifyPage] spotifyPauseIcon not wired.");
        if (((UnityEngine.Object) instance.spotifyPlayIcon == (UnityEngine.Object) null))
          UnityEngine.Debug.LogWarning((object) "[SpotifyPage] spotifyPlayIcon not wired.");
        if (((UnityEngine.Object) instance.spotifyShuffleBtn == (UnityEngine.Object) null))
          UnityEngine.Debug.LogWarning((object) "[SpotifyPage] spotifyShuffleBtn not wired.");
        if (((UnityEngine.Object) instance.spotifyRepeatBtn == (UnityEngine.Object) null))
          UnityEngine.Debug.LogWarning((object) "[SpotifyPage] spotifyRepeatBtn not wired.");
        if (((UnityEngine.Object) instance.spotifyVolumeLeftBtn == (UnityEngine.Object) null))
          UnityEngine.Debug.LogWarning((object) "[SpotifyPage] spotifyVolumeLeftBtn not wired.");
        if (((UnityEngine.Object) instance.spotifyVolumeRightBtn == (UnityEngine.Object) null))
          UnityEngine.Debug.LogWarning((object) "[SpotifyPage] spotifyVolumeRightBtn not wired.");
        if (((UnityEngine.Object) instance.spotifyRewindBtn == (UnityEngine.Object) null))
          UnityEngine.Debug.LogWarning((object) "[SpotifyPage] spotifyRewindBtn not wired.");
        if (!((UnityEngine.Object) instance.spotifySkipBtn == (UnityEngine.Object) null))
          return;
        UnityEngine.Debug.LogWarning((object) "[SpotifyPage] spotifySkipBtn not wired.");
      }
    }
    else
      UnityEngine.Debug.LogWarning((object) "[SpotifyPage] UtilMenuController.Instance is null at Start - cannot deactivate Spotify prefab. Bleed-through likely.");
  }

  public override void OnTabSelected(int tabIndex)
  {
  }

  public override bool ShouldHideStandardBars()
  {
    return UtilMenuController.Instance.CurrentTabIndex == 0 && ((UnityEngine.Object) SpotifyManager.Instance != (UnityEngine.Object) null) && SpotifyManager.Instance.IsConnected;
  }

  public override void RefreshPageUI()
  {
    UtilMenuController instance = UtilMenuController.Instance;
    bool flag = instance.CurrentTabIndex == 0 & (((UnityEngine.Object) SpotifyManager.Instance != (UnityEngine.Object) null) && SpotifyManager.Instance.IsConnected);
    if (((UnityEngine.Object) instance.spotifyRoot))
      instance.spotifyRoot.SetActive(flag);
    if (!flag)
      return;
    this.PullStateIntoUI();
  }

  public override void LateUpdate()
  {
    bool flag;
    if ((flag = ((UnityEngine.Object) SpotifyManager.Instance != (UnityEngine.Object) null) && SpotifyManager.Instance.IsConnected) == this._lastConnectionState)
    {
      if (UtilMenuController.Instance.CurrentTabIndex != 0 || !flag)
        return;
      UtilMenuController instance = UtilMenuController.Instance;
      if ((((UnityEngine.Object) instance.spotifyRoot == (UnityEngine.Object) null) ? 1 : (!instance.spotifyRoot.activeSelf ? 1 : 0)) != 0)
        return;
      this.PullStateIntoUI();
    }
    else
    {
      ((BasePage) this).BuildTabs();
      UtilMenuController.Instance.RefreshUI();
    }
  }

  private void PullStateIntoUI()
  {
    UtilMenuController instance1 = UtilMenuController.Instance;
    SpotifyManager instance2 = SpotifyManager.Instance;
    if (((UnityEngine.Object) instance2 == (UnityEngine.Object) null))
      return;
    if (((UnityEngine.Object) instance1.spotifySongTitleText))
      ((TMP_Text) instance1.spotifySongTitleText).text = (instance2.CurrentTrackName ?? "").ToUpperInvariant();
    if (((UnityEngine.Object) instance1.spotifyArtistNameText))
      ((TMP_Text) instance1.spotifyArtistNameText).text = instance2.CurrentArtistName;
    if (((UnityEngine.Object) instance1.spotifyVolumeValueText))
      ((TMP_Text) instance1.spotifyVolumeValueText).text = instance2.CurrentVolume.ToString() + "%";
    bool isPlaying = instance2.IsPlaying;
    bool flag = !instance2.IsPlaying;
    if ((!((UnityEngine.Object) instance1.spotifyPauseIcon) ? 0 : (instance1.spotifyPauseIcon.activeSelf != isPlaying ? 1 : 0)) != 0)
      instance1.spotifyPauseIcon.SetActive(isPlaying);
    if ((!((UnityEngine.Object) instance1.spotifyPlayIcon) ? 0 : (instance1.spotifyPlayIcon.activeSelf != flag ? 1 : 0)) != 0)
      instance1.spotifyPlayIcon.SetActive(flag);
    SpotifyPage.SyncToggleVisual(instance1.spotifyShuffleBtn, instance2.ShuffleOn);
    SpotifyPage.SyncToggleVisual(instance1.spotifyRepeatBtn, instance2.RepeatMode == "track");
    string currentAlbumImageUrl = instance2.CurrentAlbumImageUrl;
    if ((string.IsNullOrEmpty(currentAlbumImageUrl) || !(currentAlbumImageUrl != this._lastAlbumUrl) ? 0 : (((UnityEngine.Object) instance1.spotifyAlbumCoverRenderer != (UnityEngine.Object) null) ? 1 : 0)) == 0)
      return;
    this._lastAlbumUrl = currentAlbumImageUrl;
    if (this._coverFetchCo != null)
      Plugin.Ins.StopCoroutine(this._coverFetchCo);
    this._coverFetchCo = Plugin.Ins.StartCoroutine(this.FetchCover(currentAlbumImageUrl));
  }

  private static void SyncToggleVisual(UtilFingerButton btn, bool on)
  {
    if (((UnityEngine.Object) btn == (UnityEngine.Object) null) || btn.isOn == on)
      return;
    btn.isOn = on;
    btn.UpdateColor();
  }

  private IEnumerator FetchCover(string url)
  {
    using (UnityWebRequest req = UnityWebRequestTexture.GetTexture(url))
    {
      yield return (object) req.SendWebRequest();
      if (req.result != (UnityWebRequest.Result) 1)
      {
        this._coverFetchCo = (Coroutine) null;
      }
      else
      {
        Texture2D tex = DownloadHandlerTexture.GetContent(req);
        if (((UnityEngine.Object) this._currentCoverTex != (UnityEngine.Object) null))
          UnityEngine.Object.Destroy((UnityEngine.Object) this._currentCoverTex);
        this._currentCoverTex = tex;
        Renderer rend = UtilMenuController.Instance.spotifyAlbumCoverRenderer;
        if ((!((UnityEngine.Object) rend != (UnityEngine.Object) null) ? 0 : (((UnityEngine.Object) rend.sharedMaterial != (UnityEngine.Object) null) ? 1 : 0)) != 0)
        {
          rend.sharedMaterial.SetTexture("_BaseMap", (Texture) tex);
          rend.sharedMaterial.SetTexture("_MainTex", (Texture) tex);
          Vector2 flipScale = new Vector2(-1f, 1f);
          Vector2 flipOffset = new Vector2(1f, 0.0f);
          rend.sharedMaterial.SetTextureScale("_BaseMap", flipScale);
          rend.sharedMaterial.SetTextureOffset("_BaseMap", flipOffset);
          rend.sharedMaterial.SetTextureScale("_MainTex", flipScale);
          rend.sharedMaterial.SetTextureOffset("_MainTex", flipOffset);
        }
        this._coverFetchCo = (Coroutine) null;
      }
    }
  }

  private void WireButtonsOnce()
  {
    if (this._eventsWired)
      return;
    UtilMenuController instance1 = UtilMenuController.Instance;
    if (((UnityEngine.Object) instance1 == (UnityEngine.Object) null))
      return;
    if (((UnityEngine.Object) instance1.spotifyPausePlayBtn))
      instance1.spotifyPausePlayBtn.onPressed += (Action<UtilFingerButton, bool>) ((_, __) => SpotifyManager.Instance?.TogglePlayback());
    if (((UnityEngine.Object) instance1.spotifyRewindBtn))
      instance1.spotifyRewindBtn.onPressed += (Action<UtilFingerButton, bool>) ((_, __) => SpotifyManager.Instance?.Previous());
    if (((UnityEngine.Object) instance1.spotifySkipBtn))
      instance1.spotifySkipBtn.onPressed += (Action<UtilFingerButton, bool>) ((_, __) => SpotifyManager.Instance?.Skip());
    if (((UnityEngine.Object) instance1.spotifyShuffleBtn))
      instance1.spotifyShuffleBtn.onPressed += (Action<UtilFingerButton, bool>) ((btn, __) =>
      {
        SpotifyManager instance2 = SpotifyManager.Instance;
        if (((UnityEngine.Object) instance2 == (UnityEngine.Object) null))
          return;
        bool on = !instance2.ShuffleOn;
        instance2.SetShuffle(on);
        SpotifyPage.SyncToggleVisual(btn, on);
      });
    if (((UnityEngine.Object) instance1.spotifyRepeatBtn))
      instance1.spotifyRepeatBtn.onPressed += (Action<UtilFingerButton, bool>) ((btn, __) =>
      {
        SpotifyManager instance3 = SpotifyManager.Instance;
        if (((UnityEngine.Object) instance3 == (UnityEngine.Object) null))
          return;
        bool on = instance3.RepeatMode != "track";
        instance3.ToggleTrackRepeat();
        SpotifyPage.SyncToggleVisual(btn, on);
      });
    if (((UnityEngine.Object) instance1.spotifyVolumeLeftBtn))
      instance1.spotifyVolumeLeftBtn.onPressed += (Action<UtilFingerButton, bool>) ((_, __) =>
      {
        SpotifyManager instance4 = SpotifyManager.Instance;
        if (!((UnityEngine.Object) instance4 != (UnityEngine.Object) null))
          return;
        instance4.SetVolume(instance4.CurrentVolume - 5);
      });
    if (((UnityEngine.Object) instance1.spotifyVolumeRightBtn))
      instance1.spotifyVolumeRightBtn.onPressed += (Action<UtilFingerButton, bool>) ((_, __) =>
      {
        SpotifyManager instance5 = SpotifyManager.Instance;
        if (!((UnityEngine.Object) instance5 != (UnityEngine.Object) null))
          return;
        instance5.SetVolume(instance5.CurrentVolume + 5);
      });
    this._eventsWired = true;
  }

  public override void OnPageClosed()
  {
    UtilMenuController instance = UtilMenuController.Instance;
    if ((!((UnityEngine.Object) instance != (UnityEngine.Object) null) || !((UnityEngine.Object) instance.spotifyRoot) ? 0 : (instance.spotifyRoot.activeSelf ? 1 : 0)) != 0)
      instance.spotifyRoot.SetActive(false);
    if (this._coverFetchCo == null)
      return;
    Plugin.Ins.StopCoroutine(this._coverFetchCo);
    this._coverFetchCo = (Coroutine) null;
  }
}
