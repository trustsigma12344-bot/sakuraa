using Newtonsoft.Json;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;

#nullable enable
namespace SakuraaCastingMod.Shared.Integrations;

public class SpotifyManager : MonoBehaviour
{
  public static 
  #nullable disable
  SpotifyManager Instance;
  private bool _initialized = false;
  private const string RedirectUrl = "http://127.0.0.1:5000/callback";
  private SpotifyClient _spotify;
  private EmbedIOAuthServer _server;
  private string _verifier;
  private bool _autoConnectStarted;
  private float _timer;

  private static string ClientId { get; set; }

  public bool HasClientId
  {
    get
    {
      bool hasClientId;
      if ((string.IsNullOrEmpty(SpotifyManager.ClientId) ? 0 : (SpotifyManager.ClientId != "PASTE_YOUR_CLIENT_ID_HERE" ? 1 : 0)) != 0)
      {
        hasClientId = true;
      }
      else
      {
        this.LoadClientId();
        hasClientId = !string.IsNullOrEmpty(SpotifyManager.ClientId) && SpotifyManager.ClientId != "PASTE_YOUR_CLIENT_ID_HERE";
      }
      return hasClientId;
    }
  }

  private string TokenPath => Path.Combine(Application.persistentDataPath, "spotify_token.json");

  public int CurrentVolume { get; private set; } = 50;

  public bool IsConnected { get; private set; }

  public string CurrentTrackName { get; private set; } = "Not Connected";

  public string CurrentArtistName { get; private set; } = "";

  public string CurrentAlbumImageUrl { get; private set; } = "";

  public bool IsPlaying { get; private set; }

  public bool ShuffleOn { get; private set; }

  public string RepeatMode { get; private set; } = "off";

  [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
  internal static extern void keybd_event(uint bVk, uint bScan, uint dwFlags, uint dwExtraInfo);

  internal static void SendKey(SpotifyManager.VirtualKeyCodes virtualKeyCode)
  {
    SpotifyManager.keybd_event((uint) virtualKeyCode, 0U, 0U, 0U);
  }

  public static void NativeNextTrack()
  {
    SpotifyManager.SendKey(SpotifyManager.VirtualKeyCodes.NEXT_TRACK);
  }

  public static void NativePreviousTrack()
  {
    SpotifyManager.SendKey(SpotifyManager.VirtualKeyCodes.PREVIOUS_TRACK);
  }

  public static void NativePlayPause()
  {
    SpotifyManager.SendKey(SpotifyManager.VirtualKeyCodes.PLAY_PAUSE);
  }

  private void Awake()
  {
    SpotifyManager.Instance = this;
    UnityEngine.Object.DontDestroyOnLoad((UnityEngine.Object) ((Component) this).gameObject);
  }

  private void Start() => this.BeginAutoConnect();

  public void Initialize()
  {
    if (this._initialized)
      return;
    this.LoadClientId();
    this.BeginAutoConnect();
    this._initialized = true;
    UnityEngine.Debug.Log((object) "[SakUtil-Spotify] Initialized on demand.");
  }

  private void BeginAutoConnect()
  {
    if ((this._autoConnectStarted ? 1 : (this.IsConnected ? 1 : 0)) != 0)
      return;
    this._autoConnectStarted = true;
    this.StartCoroutine(this.AutoConnectLoop());
  }

  private IEnumerator AutoConnectLoop()
  {
    for (float waited = 0.0f; (this.HasClientId ? 0 : ((double) waited < 60.0 ? 1 : 0)) != 0; waited += 2f)
      yield return (object) new WaitForSeconds(2f);
    if ((!this.HasClientId ? 1 : (!File.Exists(this.TokenPath) ? 1 : 0)) == 0)
    {
      float[] backoff = new float[6]
      {
        2f,
        4f,
        8f,
        15f,
        30f,
        60f
      };
      int failedAttempts = 0;
      while ((this.IsConnected ? 0 : (failedAttempts < 10 ? 1 : 0)) != 0)
      {
        Task task = this.TryLoadSavedTokenAsync();
        while (!task.IsCompleted)
          yield return (object) null;
        if (this.IsConnected)
          yield break;
        float wait = backoff[Mathf.Min(failedAttempts, backoff.Length - 1)];
        ++failedAttempts;
        yield return (object) new WaitForSeconds(wait);
        task = (Task) null;
      }
      if (!this.IsConnected)
        UnityEngine.Debug.LogWarning((object) "[SakUtil-Spotify] Auto-login exhausted retries; saved token kept for next launch / manual login.");
    }
  }

  private void LoadClientId()
  {
    try
    {
      string settingString = WebSocketBridge.GetSettingString("SpotifyClientId");
      string str;
      if (settingString == null)
      {
        str = (string) null;
      }
      else
      {
        str = settingString.Trim();
        if (str != null)
          goto label_5;
      }
      str = "";
label_5:
      SpotifyManager.ClientId = str;
    }
    catch (Exception ex)
    {
      UnityEngine.Debug.LogError((object) ("[SakUtil-Spotify] Failed to load Client ID: " + ex.Message));
      SpotifyManager.ClientId = "";
    }
  }

  private async Task TryLoadSavedTokenAsync()
  {
    if ((!this.HasClientId ? 1 : (!File.Exists(this.TokenPath) ? 1 : 0)) != 0)
      return;
    try
    {
      UnityEngine.Debug.Log((object) "[SakUtil-Spotify] Found saved token, attempting auto-login...");
      string json = File.ReadAllText(this.TokenPath);
      PKCETokenResponse token = JsonConvert.DeserializeObject<PKCETokenResponse>(json);
      if (token == null)
        return;
      PKCEAuthenticator authenticator = new PKCEAuthenticator(SpotifyManager.ClientId, token);
      authenticator.TokenRefreshed += (EventHandler<PKCETokenResponse>) ((sender, newToken) =>
      {
        try
        {
          File.WriteAllText(this.TokenPath, JsonConvert.SerializeObject((object) newToken));
        }
        catch (Exception ex)
        {
          UnityEngine.Debug.LogWarning((object) ("[SakUtil-Spotify] Token save failed: " + ex.Message));
        }
        UnityEngine.Debug.Log((object) "[SakUtil-Spotify] Token refreshed & saved.");
      });
      SpotifyClientConfig config = SpotifyClientConfig.CreateDefault().WithAuthenticator((IAuthenticator) authenticator);
      this._spotify = new SpotifyClient(config);
      PrivateUser privateUser = await this._spotify.UserProfile.Current();
      this.IsConnected = true;
      this.CurrentTrackName = "Connected (Saved)!";
      UnityEngine.Debug.Log((object) "[SakUtil-Spotify] Auto-login successful!");
      json = (string) null;
      token = (PKCETokenResponse) null;
      authenticator = (PKCEAuthenticator) null;
      config = (SpotifyClientConfig) null;
    }
    catch (Exception ex)
    {
      UnityEngine.Debug.LogWarning((object) ("[SakUtil-Spotify] Auto-login attempt failed (token kept, will retry): " + ex.Message));
    }
  }

  public async void ConnectToSpotify()
  {
    int num = default;
    if (num != 0 && (string.IsNullOrEmpty(SpotifyManager.ClientId) ? 1 : (SpotifyManager.ClientId == "PASTE_YOUR_CLIENT_ID_HERE" ? 1 : 0)) != 0)
    {
      this.LoadClientId();
      if ((string.IsNullOrEmpty(SpotifyManager.ClientId) ? 1 : (SpotifyManager.ClientId == "PASTE_YOUR_CLIENT_ID_HERE" ? 1 : 0)) != 0)
      {
        UnityEngine.Debug.LogError((object) "[SakUtil-Spotify] Client ID is missing. Please enter it in the loader settings.");
        return;
      }
    }
    try
    {
      (string str1, string str2) = PKCEUtil.GenerateCodes(100);
      this._verifier = str1;
      this._server = new EmbedIOAuthServer(new Uri("http://127.0.0.1:5000/callback"), 5000);
      await this._server.Start();
      this._server.AuthorizationCodeReceived += new Func<object, AuthorizationCodeResponse, Task>(this.OnAuthorizationCodeReceived);
      LoginRequest request = new LoginRequest(this._server.BaseUri, SpotifyManager.ClientId, (LoginRequest.ResponseType) 0)
      {
        CodeChallengeMethod = "S256",
        CodeChallenge = str2,
        Scope = (ICollection<string>) new string[3]
        {
          "user-read-playback-state",
          "user-modify-playback-state",
          "user-read-currently-playing"
        }
      };
      Application.OpenURL(request.ToUri().ToString());
      str1 = (string) null;
      str2 = (string) null;
      request = (LoginRequest) null;
    }
    catch (Exception ex)
    {
      UnityEngine.Debug.LogError((object) ("[SakUtil-Spotify] Auth Start Error: " + ex.Message));
    }
  }

  private async Task OnAuthorizationCodeReceived(object sender, AuthorizationCodeResponse response)
  {
    await this._server.Stop();
    try
    {
      SpotifyClientConfig config = SpotifyClientConfig.CreateDefault();
      PKCETokenResponse tokenResponse = await new OAuthClient(config).RequestToken(new PKCETokenRequest(SpotifyManager.ClientId, response.Code, new Uri("http://127.0.0.1:5000/callback"), this._verifier));
      File.WriteAllText(this.TokenPath, JsonConvert.SerializeObject((object) tokenResponse));
      await this.TryLoadSavedTokenAsync();
    }
    catch (Exception ex)
    {
      UnityEngine.Debug.LogError((object) ("[SakUtil-Spotify] Failed to load Client ID: " + ex.Message));
      SpotifyManager.ClientId = "";
    }
  }

  public async void Skip()
  {
    if (!this.IsConnected)
      return;
    try
    {
      int num = await this._spotify.Player.SkipNext() ? 1 : 0;
    }
    catch
    {
    }
  }

  public async void Previous()
  {
    if (!this.IsConnected)
      return;
    try
    {
      int num = await this._spotify.Player.SkipPrevious() ? 1 : 0;
    }
    catch
    {
    }
  }

  public async void Pause()
  {
    if (!this.IsConnected)
      return;
    try
    {
      int num = await this._spotify.Player.PausePlayback() ? 1 : 0;
    }
    catch
    {
    }
  }

  public async void Resume()
  {
    if (!this.IsConnected)
      return;
    try
    {
      int num = await this._spotify.Player.ResumePlayback() ? 1 : 0;
    }
    catch
    {
    }
  }

  public async void TogglePlayback()
  {
    if (!this.IsConnected)
      return;
    try
    {
      CurrentlyPlayingContext playback = await this._spotify.Player.GetCurrentPlayback();
      if ((playback == null ? 0 : (playback.IsPlaying ? 1 : 0)) != 0)
      {
        int num1 = await this._spotify.Player.PausePlayback() ? 1 : 0;
      }
      else
      {
        int num2 = await this._spotify.Player.ResumePlayback() ? 1 : 0;
      }
      playback = (CurrentlyPlayingContext) null;
    }
    catch
    {
    }
  }

  public async void SetVolume(int percent)
  {
    if (!this.IsConnected)
      return;
    try
    {
      int num = await this._spotify.Player.SetVolume(new PlayerVolumeRequest(Mathf.Clamp(percent, 0, 100))) ? 1 : 0;
    }
    catch
    {
    }
  }

  public async void SetShuffle(bool on)
  {
    if (!this.IsConnected)
      return;
    try
    {
      int num = await this._spotify.Player.SetShuffle(new PlayerShuffleRequest(on)) ? 1 : 0;
      this.ShuffleOn = on;
    }
    catch
    {
    }
  }

  public async void ToggleTrackRepeat()
  {
    if (!this.IsConnected)
      return;
    bool currentlyTrack = this.RepeatMode == "track";
    PlayerSetRepeatRequest.State next = currentlyTrack ? (PlayerSetRepeatRequest.State) 2 : (PlayerSetRepeatRequest.State) 0;
    try
    {
      int num = await this._spotify.Player.SetRepeat(new PlayerSetRepeatRequest(next)) ? 1 : 0;
      this.RepeatMode = currentlyTrack ? "off" : "track";
    }
    catch
    {
    }
  }

  public async void Search(string query, Action<List<FullTrack>> onResults)
  {
    if (!this.IsConnected)
      return;
    try
    {
      SearchRequest searchRequest = new SearchRequest((SearchRequest.Types) 8, query);
      SearchResponse response = await this._spotify.Search.Item(searchRequest);
      if (response.Tracks != null)
      {
        Action<List<FullTrack>> action = onResults;
        if (action != null)
          action(response.Tracks.Items);
      }
      searchRequest = (SearchRequest) null;
      response = (SearchResponse) null;
    }
    catch (Exception ex)
    {
      UnityEngine.Debug.LogError((object) ("[SakUtil-Spotify] Search Error: " + ex.Message));
    }
  }

  public async void PlayTrack(string uri)
  {
    if (!this.IsConnected)
      return;
    try
    {
      int num = await this._spotify.Player.ResumePlayback(new PlayerResumePlaybackRequest()
      {
        Uris = (IList<string>) new List<string>() { uri }
      }) ? 1 : 0;
      await Task.Delay(500);
      this.UpdateCurrentTrack();
    }
    catch
    {
    }
  }

  private void Update()
  {
    if (!this.IsConnected)
      return;
    this._timer += Time.deltaTime;
    if ((double) this._timer < 3.0)
      return;
    this._timer = 0.0f;
    this.UpdateCurrentTrack();
  }

  private async void UpdateCurrentTrack()
  {
    try
    {
      CurrentlyPlayingContext playback = await this._spotify.Player.GetCurrentPlayback();
      if (playback != null)
      {
        int? volumePercent;
        int num;
        if (playback.Device != null)
        {
          volumePercent = playback.Device.VolumePercent;
          num = volumePercent.HasValue ? 1 : 0;
        }
        else
          num = 0;
        if (num != 0)
        {
          volumePercent = playback.Device.VolumePercent;
          this.CurrentVolume = volumePercent.Value;
        }
        this.IsPlaying = playback.IsPlaying;
        this.ShuffleOn = playback.ShuffleState;
        if (!string.IsNullOrEmpty(playback.RepeatState))
          this.RepeatMode = playback.RepeatState;
        if (playback.Item is FullTrack track)
        {
          this.CurrentTrackName = track.Name;
          this.CurrentArtistName = track.Artists.Count > 0 ? track.Artists[0].Name : "Unknown";
          this.CurrentAlbumImageUrl = track.Album == null || track.Album.Images == null || track.Album.Images.Count <= 0 ? "" : track.Album.Images[0].Url;
        }
        track = (FullTrack) null;
      }
      playback = (CurrentlyPlayingContext) null;
    }
    catch
    {
    }
  }

  internal enum VirtualKeyCodes : uint
  {
    NEXT_TRACK = 176, // 0x000000B0
    PREVIOUS_TRACK = 177, // 0x000000B1
    PLAY_PAUSE = 179, // 0x000000B3
  }
}
