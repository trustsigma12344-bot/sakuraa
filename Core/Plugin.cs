using BepInEx;
using ExitGames.Client.Photon;
using GorillaLocomotion;
using GorillaNetworking;
using Photon.Pun;
using Photon.Realtime;
using SakMerge.Api;
using SakuraaCastingMod.Desktop.Camera;
using SakuraaCastingMod.Desktop.Ui;
using SakuraaCastingMod.Features.AutoRef;
using SakuraaCastingMod.Features.Overlays;
using SakuraaCastingMod.Features.Replay;
using SakuraaCastingMod.Features.Replay.Recording;
using SakuraaCastingMod.Features.Replay.ReplayManagers;
using SakuraaCastingMod.Features.Soundboard;
using SakuraaCastingMod.Features.Tools;
using SakuraaCastingMod.Features.Visuals;
using SakuraaCastingMod.Features.World;
using SakuraaCastingMod.Shared.Helpers;
using SakuraaCastingMod.Shared.Integrations;
using SakuraaCastingMod.Shared.Integrations.DiscordSDK;
using SakuraaCastingMod.VR.Interaction;
using SakuraaCastingMod.VR.Tablet;
using SakuraaCastingMod.VR.UtilMenu;
using SakuraaCastingMod.VR.UtilMenu.Utility;
using SakuraaOfflinePresets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR;

#nullable disable
namespace SakuraaCastingMod.Core;

public class Plugin : MonoBehaviour
{
  public static Plugin Ins;
  private bool _initialized;
  public readonly int XPosition = 2;
  public Transform cameraParent;
  public UnityEngine.Camera camera;
  public Transform originalCameraParent;
  public Vector3 originalPos;
  public Quaternion originalRot;
  public AudioSource CameraAudioSource;
  [SavedSetting("ListenerEnabled", true)]
  public static bool listenerBool = true;
  [SavedSetting("FieldOfView", 90f)]
  public static float fov = 90f;
  [SavedSetting("MovementSpeed", 1f)]
  public static float moveSpeed = 1f;
  [SavedSetting("RotationSpeed", 10f)]
  public static float rotationSpeed = 10f;
  [SavedSetting("ZoomFieldOfView", 15f)]
  public static float zoomFov = 15f;
  [SavedSetting("ClippingPlaneNear", 0.01f)]
  public static float clippingPlaneNear = 0.01f;
  [SavedSetting("FreeCamSmoothing", true)]
  public static bool freeCamSmoothing = true;
  [SavedSetting("FreeCamSmoothingSpeed", 4.5f)]
  public static float freeCamSmoothingSpeed = 4.5f;
  public readonly string[] CameraModes = new string[8]
  {
    "disabled",
    "tablet",
    "freecam",
    "control",
    "drone",
    "player",
    "observe",
    "director"
  };
  public int currentCameraMode;
  private int _lastFrameMode = -1;
  public bool modDisabled;
  public float lerpMult = 114f;
  private AudioListener _playerListener;
  private AudioListener _cameraListener;
  [SavedSetting("ShowNameChangerMenu", false)]
  public bool showNameChangerMenu;
  private AssetBundle _assetBundle;
  private Font _defaultFont;
  public TMP_FontAsset defaultFontAsset;
  private Font _gtagFont;
  public TMP_FontAsset gtagFontAsset;
  private Font _pixelFont;
  public TMP_FontAsset pixelFontAsset;
  private Font _gothamFont;
  public TMP_FontAsset gothamFontAsset;
  private Font _designerFont;
  public TMP_FontAsset designerFontAsset;
  public GameObject gorillaComputer;
  public GameObject toggleMicObj;
  public GameObject toggleMicNormalObj;
  public GameObject toggleMicMutedObj;
  public bool isPttTypeUnmuted = true;
  public GameObject tabletObj;
  private GameObject _remoteMenuPrefab;
  private UnityEngine.Camera _mirroredDisplayCam;
  private GameObject _mirroredDisplayCamObject;

  public static bool ReplayCompatibilityMode => ReplayManager.replayProject != null;

  public IEnumerator Start()
  {
    Plugin.Ins = this;
    Capabilities.SetTier((Tier) 3);
    UnityEngine.Debug.Log((object) "Sakuraa Client awaiting for PhotonNetworkController. . .");
    yield return (object) new WaitUntil((Func<bool>) (() => ((UnityEngine.Object) PhotonNetworkController.Instance != (UnityEngine.Object) null)));
    yield return (object) new WaitForSeconds(0.5f);
    UnityEngine.Debug.Log((object) "Sakuraa Client now starting . . .");
    PlayerTranslator.Initialize();
    ModMessageHandler.Initialize();
    FeatureToggles.Refresh();
    HarmonyPatches.ApplyHarmonyPatches();
    HzSp.Init();
    GameObject replayHolder = new GameObject("SakuraaReplaySystem");
    replayHolder.AddComponent<ReplaySystemBase>();
    UnityEngine.Object.DontDestroyOnLoad((UnityEngine.Object) replayHolder);
    GameObject recorderHolder = new GameObject("SakuraaReplayRecorder");
    recorderHolder.AddComponent<RecorderSystemBase>();
    UnityEngine.Object.DontDestroyOnLoad((UnityEngine.Object) recorderHolder);
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    PhotonNetwork.NetworkingClient.EventReceived += new Action<EventData>(TagEventManager.OnEvent);
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    PhotonNetwork.NetworkingClient.EventReceived += new Action<EventData>(ConsoleDetector.OnEvent);
    TagEventManager.OnTagEvent += new TagEventManager.TagEventHandler(this.OnTag);
    ((Component) this).gameObject.AddComponent<AutoRefManager>();
    if (XRSettings.isDeviceActive)
    {
      UnityEngine.Debug.Log((object) "SCM: user is on VR");
      Plugin.listenerBool = false;
      MainMenus.ShowMainMenu = false;
      this.currentCameraMode = 1;
    }
    if (FeatureToggles.ShowDesktopCasting)
      Keybinds.Initialize();
    yield return (object) new WaitUntil((Func<bool>) (() => ((UnityEngine.Object) GorillaTagger.Instance != (UnityEngine.Object) null) && ((UnityEngine.Object) GTPlayer.Instance != (UnityEngine.Object) null)));
    Transform shoulderCam = GorillaTagger.Instance.thirdPersonCamera.transform.Find("Shoulder Camera");
    if (!((UnityEngine.Object) shoulderCam != (UnityEngine.Object) null))
      UnityEngine.Debug.LogWarning((object) "Shoulder Camera not found");
    else
      this.camera = ((Component) shoulderCam).GetComponent<UnityEngine.Camera>();
    this._playerListener = ((Component) GTPlayer.Instance).GetComponentInChildren<AudioListener>();
    if (((UnityEngine.Object) this.camera != (UnityEngine.Object) null))
    {
      Transform cameraTransform;
      this._cameraListener = ComponentUtils.AddComponent<AudioListener>((Component) (cameraTransform = ((Component) this.camera).transform));
      this.originalCameraParent = GorillaTagger.Instance.thirdPersonCamera.transform;
      this.originalPos = cameraTransform.localPosition;
      this.originalRot = cameraTransform.localRotation;
      this.cameraParent = new GameObject("SakuraaSpectatorCamera").transform;
      ((Component) this.camera).transform.parent = this.cameraParent;
      this.camera.cullingMask &= -524289;
      cameraTransform = (Transform) null;
    }
    this.gorillaComputer = GameObject.Find("GorillaComputer");
    if (((UnityEngine.Object) this.gorillaComputer == (UnityEngine.Object) null))
      UnityEngine.Debug.LogWarning((object) "GorillaComputer not found!");
    Character.InitFakeComputer();
    UnityEngine.Debug.Log((object) "Starting to retrieve assets");
    if (FeatureToggles.ShowTablet)
    {
      UtilMenuMain utilMenuMain = ((Component) this).gameObject.AddComponent<UtilMenuMain>();
      UtilMenuMain.Instance = utilMenuMain;
      utilMenuMain = (UtilMenuMain) null;
    }
    this.RetrieveAssets();
    UnityEngine.Debug.Log((object) "Actually retrieved assets");
    if (((UnityEngine.Object) this.camera != (UnityEngine.Object) null))
      this.ResetCameraObject();
    GameObject bdnObj = GameObject.Find("BetterDayNight");
    if (((UnityEngine.Object) bdnObj != (UnityEngine.Object) null))
      WorldManager.Bdnm = bdnObj.GetComponent<BetterDayNightManager>();
    else
      UnityEngine.Debug.LogWarning((object) "[SCM] BetterDayNight object not found! Day/Night features may not work.");
    if (((UnityEngine.Object) this._cameraListener != (UnityEngine.Object) null))
      ((Behaviour) this._cameraListener).enabled = true;
    if (((UnityEngine.Object) this._playerListener != (UnityEngine.Object) null))
      ((Behaviour) this._playerListener).enabled = false;
    this._initialized = true;
    LegalChecker.CheckLegalVisibility();
    WorldManager.CheckMaps();
    RankVisuals.StartShenanigans();
    if (FeatureToggles.DiscordRpc)
    {
      try
      {
        string parentDir = Directory.GetParent(Paths.ManagedPath)?.FullName;
        string dataPluginsPath = Path.Combine(parentDir ?? throw new InvalidOperationException(), "Plugins", "x86_64");
        if (Directory.Exists(dataPluginsPath))
        {
          string destinationFile = Path.Combine(dataPluginsPath, "discord_game_sdk.dll");
          Assembly assembly = Assembly.GetExecutingAssembly();
          Stream resourceStream = assembly.GetManifestResourceStream("SakuraaCastingMod.Shared.Integrations.Libs.discord_game_sdk.dll");
          try
          {
            if (resourceStream != null)
            {
              using (FileStream fileStream = new FileStream(destinationFile, FileMode.Create, FileAccess.Write))
                resourceStream.CopyTo((Stream) fileStream);
              UnityEngine.Debug.Log((object) ("Successfully extracted discord_game_sdk.dll to: " + destinationFile));
            }
            else
              UnityEngine.Debug.LogError((object) "Could not find embedded resource: SakuraaCastingMod.Shared.Integrations.Libs.discord_game_sdk.dll");
          }
          finally
          {
            resourceStream?.Dispose();
          }
          parentDir = (string) null;
          dataPluginsPath = (string) null;
          destinationFile = (string) null;
          assembly = (Assembly) null;
          resourceStream = (Stream) null;
        }
        else
        {
          UnityEngine.Debug.LogError((object) (dataPluginsPath + " does not exist?? how the fuck did you even load the mod"));
          yield break;
        }
      }
      catch (Exception ex)
      {
        UnityEngine.Debug.LogError((object) $"Failed to extract embedded resource: {ex}");
      }
      UnityEngine.Debug.Log((object) "Starting the bullshit");
      try
      {
        DiscordWrapper.Construct();
        DiscordWrapper.SetActivity((Func<Activity, Activity>) (activity =>
        {
          string str1;
          switch (this.XPosition)
          {
            case 0:
              str1 = "Basic Pack";
              break;
            case 1:
              str1 = "Creator Pack";
              break;
            default:
              str1 = "Master Creator Pack";
              break;
          }
          string str2 = str1;
          activity.Details = str2;
          activity.State = "GtagCamera.com";
          activity.Assets = new ActivityAssets()
          {
            LargeImage = "in-gametablet"
          };
          return activity;
        }));
        DiscordWrapper.UpdateActivity();
      }
      catch (Exception ex)
      {
        UnityEngine.Debug.LogError((object) ("Discord RPC failed to initialize: " + ex.Message));
      }
    }
    else
      UnityEngine.Debug.Log((object) "[SCM] DiscordRpc disabled by loader, skipping Discord SDK init and DLL extraction.");
    this.CameraAudioSource = ((Component) this.camera).gameObject.AddComponent<AudioSource>();
    Configuration.InitialSettingsLoad();
    if (((UnityEngine.Object) ((Component) this).GetComponent<SecureNetworkManager>() == (UnityEngine.Object) null))
    {
      SecureNetworkManager snm = ((Component) this).gameObject.AddComponent<SecureNetworkManager>();
      snm.remoteMenuPrefab = this._remoteMenuPrefab;
      snm = (SecureNetworkManager) null;
    }
    if (((UnityEngine.Object) ((Component) this).GetComponent<FriendNetworkController>() == (UnityEngine.Object) null))
      ((Component) this).gameObject.AddComponent<FriendNetworkController>();
    yield return (object) new WaitUntil((Func<bool>) (() => ((UnityEngine.Object) FriendNetworkController.Instance != (UnityEngine.Object) null)));
    if (!XRSettings.isDeviceActive)
    {
      FriendNetworkController fnc = FriendNetworkController.Instance;
      fnc.AllowFastJoins = PermissionOption.DISABLE;
      fnc.AllowRequests = PermissionOption.DISABLE;
      fnc.SetShareRoom(ShareRoomOption.DISABLE);
      fnc = (FriendNetworkController) null;
    }
    FriendNetworkController.Instance.SendFriggenPacketYo(OpCode.PING, (object) null);
    FriendNetworkController.Instance.LoadConfigState();
  }

  public void OnTag(Photon.Realtime.Player tagger, Photon.Realtime.Player tagged)
  {
    KillFeed.CreateKillfeedCard(tagger, tagged);
    Scoreboard.AddTagScore(tagger, tagged);
  }

  private static T LoadAssetSafe<T>(AssetBundle bundle, string assetName) where T : UnityEngine.Object
  {
    T obj1;
    if (((UnityEngine.Object) bundle == (UnityEngine.Object) null))
    {
      obj1 = default (T);
    }
    else
    {
      T obj2 = bundle.LoadAsset<T>(assetName);
      if (((UnityEngine.Object) obj2 == (UnityEngine.Object) null))
        UnityEngine.Debug.LogError((object) "[SCM] FAILED TO A LOAD ASSET");
      obj1 = obj2;
    }
    return obj1;
  }

  private static GameObject InstantiateAssetSafe(
    AssetBundle bundle,
    string assetName,
    Transform parent = null)
  {
    GameObject gameObject1 = Plugin.LoadAssetSafe<GameObject>(bundle, assetName);
    GameObject gameObject2;
    if (!((UnityEngine.Object) gameObject1 == (UnityEngine.Object) null))
    {
      GameObject gameObject3 = UnityEngine.Object.Instantiate<GameObject>(gameObject1, parent);
      ((UnityEngine.Object) gameObject3).name = assetName;
      gameObject2 = gameObject3;
    }
    else
      gameObject2 = (GameObject) null;
    return gameObject2;
  }

  private static Sprite MakeSpriteFromMat(Material m)
  {
    Sprite sprite;
    if (((UnityEngine.Object) m == (UnityEngine.Object) null))
    {
      sprite = (Sprite) null;
    }
    else
    {
      Texture2D mainTexture = m.mainTexture as Texture2D;
      sprite = !((UnityEngine.Object) mainTexture == (UnityEngine.Object) null) ? Sprite.Create(mainTexture, new UnityEngine.Rect(0.0f, 0.0f, (float) ((Texture) mainTexture).width, (float) ((Texture) mainTexture).height), new Vector2(0.5f, 0.5f)) : (Sprite) null;
    }
    return sprite;
  }

  private static void DeriveKeyAndIV(out byte[] key, out byte[] iv)
  {
    using (Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes($"SakuraaCastMod", Encoding.UTF8.GetBytes($"b10ss0m{1815.ToString("x")}scm"), 48000, HashAlgorithmName.SHA256))
    {
      key = rfc2898DeriveBytes.GetBytes(32 /*0x20*/);
      iv = rfc2898DeriveBytes.GetBytes(16 /*0x10*/);
    }
  }

  private void RetrieveAssets()
  {
    Stream manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SakuraaCastingMod.Core.scmab2");
    if (manifestResourceStream == null)
    {
      UnityEngine.Debug.LogError((object) "AssetBundle Stream is null!");
    }
    else
    {
      byte[] array;
      using (MemoryStream destination = new MemoryStream())
      {
        manifestResourceStream.CopyTo((Stream) destination);
        array = destination.ToArray();
      }
      manifestResourceStream.Dispose();
      AssetBundle assetBundle = AssetBundle.LoadFromMemory(array);
      if (!((UnityEngine.Object) assetBundle == (UnityEngine.Object) null))
      {
        TextAsset textAsset = assetBundle.LoadAsset<TextAsset>("_payload");
        if (!((UnityEngine.Object) textAsset == (UnityEngine.Object) null))
        {
          byte[] bytes = textAsset.bytes;
          assetBundle.Unload(true);
          byte[] key;
          byte[] iv;
          Plugin.DeriveKeyAndIV(out key, out iv);
          byte[] numArray;
          using (Aes aes = Aes.Create())
          {
            aes.Key = key;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            using (ICryptoTransform decryptor = aes.CreateDecryptor())
              numArray = decryptor.TransformFinalBlock(bytes, 0, bytes.Length);
          }
          this._assetBundle = AssetBundle.LoadFromMemory(numArray);
          if (((UnityEngine.Object) this._assetBundle == (UnityEngine.Object) null))
          {
            UnityEngine.Debug.LogError((object) "Failed to load CastingMod AssetBundle!");
            Notification.Send("Failed to load CastingMod Assets!", Color.red);
          }
          else
          {
            UnityEngine.Debug.Log((object) $"[SCM] Loaded CastingMod AssetBundle with {this._assetBundle.GetAllAssetNames().Length} assets");
            Leaderboard.LeaderboardCanvasObj = new GameObject("LeaderboardCanvas");
            Leaderboard.LeaderboardCanvas = Leaderboard.LeaderboardCanvasObj.AddComponent<Canvas>();
            Leaderboard.LeaderboardCanvas.renderMode = (RenderMode) 0;
            Leaderboard.LeaderboardCanvasObj.AddComponent<CanvasScaler>();
            Leaderboard.LeaderboardCanvasObj.AddComponent<GraphicRaycaster>();
            Scoreboard.ScoreboardCanvasObj = new GameObject("ScoreboardCanvas");
            Scoreboard.ScoreboardCanvas = Scoreboard.ScoreboardCanvasObj.AddComponent<Canvas>();
            Scoreboard.ScoreboardCanvas.renderMode = (RenderMode) 0;
            Scoreboard.ScoreboardCanvasObj.AddComponent<CanvasScaler>();
            Scoreboard.ScoreboardCanvasObj.AddComponent<GraphicRaycaster>();
            Scoreboard.MainScoreboardObj = Plugin.InstantiateAssetSafe(this._assetBundle, "Scoreboard", ((Component) Scoreboard.ScoreboardCanvas).transform);
            Scoreboard.CherryScoreboardObj = Plugin.InstantiateAssetSafe(this._assetBundle, "SakuraaScoreboard", ((Component) Scoreboard.ScoreboardCanvas).transform);
            Scoreboard.PlayerScoreboardObj = Plugin.InstantiateAssetSafe(this._assetBundle, "PlayerScoreboard", ((Component) Scoreboard.ScoreboardCanvas).transform);
            Scoreboard.CgtScoreboardObj = Plugin.InstantiateAssetSafe(this._assetBundle, "CgtScoreboard", ((Component) Scoreboard.ScoreboardCanvas).transform);
            if (((UnityEngine.Object) Scoreboard.MainScoreboardObj != (UnityEngine.Object) null))
            {
              Scoreboard._scorePos = new Vector2((float) Screen.width / 2f, (float) (Screen.height - 135));
              Transform child = Scoreboard.MainScoreboardObj.transform.GetChild(0);
              if (((UnityEngine.Object) child != (UnityEngine.Object) null))
              {
                Scoreboard.Team1NameText = (TMP_Text) ((Component) child.Find("Team1Text"))?.GetComponent<TextMeshProUGUI>();
                Scoreboard.Team2NameText = (TMP_Text) ((Component) child.Find("Team2Text"))?.GetComponent<TextMeshProUGUI>();
                Scoreboard.Team1ScoreText = (TMP_Text) ((Component) child.Find("Score1"))?.GetComponent<TextMeshProUGUI>();
                Scoreboard.Team2ScoreText = (TMP_Text) ((Component) child.Find("Score2"))?.GetComponent<TextMeshProUGUI>();
                Scoreboard.CurrentTimerText = (TMP_Text) ((Component) child.Find("Timer"))?.GetComponent<TextMeshProUGUI>();
                Scoreboard.OldTimerText = (TMP_Text) ((Component) child.Find("Timer (1)"))?.GetComponent<TextMeshProUGUI>();
              }
              Scoreboard.MainScoreboardObj.SetActive(false);
            }
            if (((UnityEngine.Object) Scoreboard.CherryScoreboardObj))
              Scoreboard.CherryScoreboardObj.SetActive(false);
            if (((UnityEngine.Object) Scoreboard.PlayerScoreboardObj))
              Scoreboard.PlayerScoreboardObj.SetActive(false);
            if (((UnityEngine.Object) Scoreboard.CgtScoreboardObj))
              Scoreboard.CgtScoreboardObj.SetActive(false);
            if (((UnityEngine.Object) Scoreboard.PlayerScoreboardObj != (UnityEngine.Object) null))
            {
              Transform transform = Scoreboard.PlayerScoreboardObj.transform;
              transform.localScale = (transform.localScale * 1.2f);
              if (Scoreboard.PlayerScoreboardObj.transform.childCount >= 2)
              {
                Scoreboard.PlayerScoreboardObj.transform.GetChild(0).position = new Vector3(585f, Scoreboard.PlayerScoreboardObj.transform.GetChild(0).position.y);
                Scoreboard.PlayerScoreboardObj.transform.GetChild(1).position = new Vector3((float) (Screen.width - 585), Scoreboard.PlayerScoreboardObj.transform.GetChild(0).position.y);
              }
              Scoreboard.PlayerScoreboardObj.SetActive(false);
            }
            KillFeed.KillFeedCanvasObj = new GameObject("KillFeedCanvas");
            KillFeed.KillFeedCanvasObj.AddComponent<Canvas>().renderMode = (RenderMode) 0;
            KillFeed.KillFeedCanvasObj.AddComponent<CanvasScaler>();
            KillFeed.KillFeedCanvasObj.AddComponent<GraphicRaycaster>();
            LavaDistance.PlayerDistanceCanvasObj = new GameObject("PlayerDistanceCanvas");
            LavaDistance.PlayerDistanceCanvas = LavaDistance.PlayerDistanceCanvasObj.AddComponent<Canvas>();
            LavaDistance.PlayerDistanceCanvas.renderMode = (RenderMode) 0;
            LavaDistance.PlayerDistanceCanvasObj.AddComponent<CanvasScaler>();
            LavaDistance.PlayerDistanceCanvasObj.AddComponent<GraphicRaycaster>();
            LavaDistance.PlayerDistanceCardObject = Plugin.LoadAssetSafe<GameObject>(this._assetBundle, "PlayerDistanceCard");
            Leaderboard.PlayerCardObject = Plugin.LoadAssetSafe<GameObject>(this._assetBundle, "PlayerCard");
            KillFeed.KillFeedCardObject = Plugin.LoadAssetSafe<GameObject>(this._assetBundle, "KillFeedCard");
            this._defaultFont = Plugin.LoadAssetSafe<Font>(this._assetBundle, "font");
            this.defaultFontAsset = Plugin.LoadAssetSafe<TMP_FontAsset>(this._assetBundle, "font SDF");
            this._gtagFont = Plugin.LoadAssetSafe<Font>(this._assetBundle, "gtag_font");
            this.gtagFontAsset = Plugin.LoadAssetSafe<TMP_FontAsset>(this._assetBundle, "gtag_font SDF");
            this._pixelFont = Plugin.LoadAssetSafe<Font>(this._assetBundle, "Pixel Font");
            this.pixelFontAsset = Plugin.LoadAssetSafe<TMP_FontAsset>(this._assetBundle, "pixel Font SDF");
            this._designerFont = Plugin.LoadAssetSafe<Font>(this._assetBundle, "Designer");
            this.designerFontAsset = Plugin.LoadAssetSafe<TMP_FontAsset>(this._assetBundle, "Designer SDF");
            this._gothamFont = Plugin.LoadAssetSafe<Font>(this._assetBundle, "Gotham Ultra");
            this.gothamFontAsset = Plugin.LoadAssetSafe<TMP_FontAsset>(this._assetBundle, "Gotham Ultra SDF");
            RankVisuals.ChromaKeyShader = Plugin.LoadAssetSafe<Shader>(this._assetBundle, "ChromaKeyShader");
            RankVisuals.TransparencyMat = Plugin.LoadAssetSafe<Material>(this._assetBundle, "Transparency");
            if ((!((UnityEngine.Object) RankVisuals.TransparencyMat != (UnityEngine.Object) null) ? 0 : (((UnityEngine.Object) RankVisuals.ChromaKeyShader != (UnityEngine.Object) null) ? 1 : 0)) != 0)
              RankVisuals.TransparencyMat.shader = RankVisuals.ChromaKeyShader;
            Sounds.boingSfx = Plugin.LoadAssetSafe<AudioClip>(this._assetBundle, "boingSfx");
            Sounds.shootSfx = Plugin.LoadAssetSafe<AudioClip>(this._assetBundle, "shootSfx");
            Sounds.subtleClickSfx = Plugin.LoadAssetSafe<AudioClip>(this._assetBundle, "subtleClickSfx");
            NameTags.NameTag = Plugin.LoadAssetSafe<GameObject>(this._assetBundle, "NameTagPlate");
            ((Component) this).gameObject.AddComponent<NameTagCleanupBridge>();
            Drone.DroneSound = Plugin.LoadAssetSafe<AudioClip>(this._assetBundle, "droneSfx");
            if (FeatureToggles.ShowDesktopCasting)
            {
              RankVisuals.PttCanvasObj = new GameObject("ToggleMicCanvas");
              RankVisuals.PttCanvas = RankVisuals.PttCanvasObj.AddComponent<Canvas>();
              RankVisuals.PttCanvas.renderMode = (RenderMode) 0;
              RankVisuals.PttCanvasObj.AddComponent<CanvasScaler>();
              RankVisuals.PttCanvasObj.AddComponent<GraphicRaycaster>();
              GameObject gameObject = Plugin.LoadAssetSafe<GameObject>(this._assetBundle, "ToggleMicIcon");
              if (((UnityEngine.Object) gameObject != (UnityEngine.Object) null))
              {
                this.toggleMicObj = UnityEngine.Object.Instantiate<GameObject>(gameObject, RankVisuals.PttCanvasObj.transform);
                this.toggleMicNormalObj = ((Component) this.toggleMicObj.transform.Find("MicIcon"))?.gameObject;
                this.toggleMicMutedObj = ((Component) this.toggleMicObj.transform.Find("MicMutedIcon"))?.gameObject;
                Transform transform = this.toggleMicObj.transform;
                transform.localScale = (transform.localScale * 0.5f);
                ToggleMicOverlay.UpdatePosition();
              }
            }
            RankVisuals.BasicPackMat = Plugin.LoadAssetSafe<Material>(this._assetBundle, "basicPackMat");
            RankVisuals.CreatorPackMat = Plugin.LoadAssetSafe<Material>(this._assetBundle, "creatorPackMat");
            RankVisuals.MasterPackMat = Plugin.LoadAssetSafe<Material>(this._assetBundle, "masterPackMat");
            RankVisuals.BasicPackSprite = Plugin.MakeSpriteFromMat(RankVisuals.BasicPackMat);
            RankVisuals.CreatorPackSprite = Plugin.MakeSpriteFromMat(RankVisuals.CreatorPackMat);
            RankVisuals.MasterPackSprite = Plugin.MakeSpriteFromMat(RankVisuals.MasterPackMat);
            MiniMap.MiniMapCanvasObj = new GameObject("MiniMapCanvas");
            MiniMap.MiniMapCanvas = MiniMap.MiniMapCanvasObj.AddComponent<Canvas>();
            MiniMap.MiniMapCanvas.renderMode = (RenderMode) 0;
            MiniMap.MiniMapCanvasObj.AddComponent<CanvasScaler>();
            MiniMap.MiniMapCanvasObj.AddComponent<GraphicRaycaster>();
            GameObject gameObject1 = Plugin.LoadAssetSafe<GameObject>(this._assetBundle, "MiniMapDisplay");
            if (((UnityEngine.Object) gameObject1 != (UnityEngine.Object) null))
            {
              MiniMap.MiniMapObj = UnityEngine.Object.Instantiate<GameObject>(gameObject1, MiniMap.MiniMapCanvasObj.transform);
              MiniMap.MiniMapObj.transform.position = (new Vector2(150f, (float) (Screen.height - 150)));
              Transform transform = MiniMap.MiniMapObj.transform;
              transform.localScale = (transform.localScale * 3f);
            }
            MiniMap.MiniMapCameraObj = new GameObject("MiniMapCam");
            MiniMap.MiniMapCamera = MiniMap.MiniMapCameraObj.AddComponent<UnityEngine.Camera>();
            MiniMap.MiniMapCamera.cullingMask &= -524289;
            MiniMap.MiniMapCameraObj.transform.rotation = Quaternion.Euler(90f, 90f, 180f);
            MiniMap.MiniMapCamera.orthographic = true;
            MiniMap.MiniMapCamera.targetTexture = Plugin.LoadAssetSafe<RenderTexture>(this._assetBundle, "MiniMapRenderTexture");
            MiniMap.MiniMapCanvasObj.SetActive(false);
            if (FeatureToggles.ShowCameraTablet)
            {
              this._mirroredDisplayCamObject = new GameObject("DisplayCam");
              this._mirroredDisplayCam = this._mirroredDisplayCamObject.AddComponent<UnityEngine.Camera>();
              this._mirroredDisplayCam.cullingMask &= -524289;
              this._mirroredDisplayCam.targetTexture = Plugin.LoadAssetSafe<RenderTexture>(this._assetBundle, "TabletRenderTexture");
              if (((UnityEngine.Object) this.camera != (UnityEngine.Object) null))
              {
                this.camera.stereoTargetEye = (StereoTargetEyeMask) 0;
                this._mirroredDisplayCamObject.transform.parent = ((Component) this.camera).transform;
              }
              ((Component) this._mirroredDisplayCam).transform.localPosition = new Vector3();
              ((Component) this._mirroredDisplayCam).transform.localRotation = new Quaternion();
            }
            if (FeatureToggles.ShowCameraTablet)
            {
              GameObject gameObject2 = Plugin.LoadAssetSafe<GameObject>(this._assetBundle, "TabletHoldable");
              if (((UnityEngine.Object) gameObject2 != (UnityEngine.Object) null))
              {
                this.tabletObj = UnityEngine.Object.Instantiate<GameObject>(gameObject2);
                UnityEngine.Object.DontDestroyOnLoad((UnityEngine.Object) this.tabletObj);
                this.tabletObj.AddComponent<TabletController>();
                this.tabletObj.gameObject.layer = 18;
                Transform transform1 = this.tabletObj.transform.Find("bar_smooth");
                if (((UnityEngine.Object) transform1 != (UnityEngine.Object) null))
                {
                  Transform transform2 = transform1.Find("leftHandle");
                  if (((UnityEngine.Object) transform2 != (UnityEngine.Object) null))
                  {
                    ((Component) transform2).gameObject.layer = 18;
                    ComponentUtils.AddComponent<TabletGabber>((Component) transform2);
                  }
                  Transform transform3 = transform1.Find("rightHandle");
                  if (((UnityEngine.Object) transform3 != (UnityEngine.Object) null))
                  {
                    ((Component) transform3).gameObject.layer = 18;
                    ComponentUtils.AddComponent<TabletGabber>((Component) transform3);
                  }
                }
                this.tabletObj.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
              }
            }
            GameObject gameObject3 = FeatureToggles.ShowTablet ? Plugin.LoadAssetSafe<GameObject>(this._assetBundle, "SakUtilMenu") : (GameObject) null;
            this._remoteMenuPrefab = gameObject3;
            if (!((UnityEngine.Object) gameObject3 != (UnityEngine.Object) null))
            {
              if (FeatureToggles.ShowTablet)
                UnityEngine.Debug.LogError((object) "[SCM] SAKUTILMENU PREFAB NOT FOUND! Menu will not load.");
            }
            else
            {
              int num = XRSettings.isDeviceActive ? 1 : 0;
              GameObject gameObject4 = UnityEngine.Object.Instantiate<GameObject>(gameObject3, Vector3.zero, Quaternion.identity);
              UnityEngine.Object.DontDestroyOnLoad((UnityEngine.Object) gameObject4);
              gameObject4.AddComponent<UtilMenuController>();
              Transform transform4 = gameObject4.transform.Find("panel1");
              if (transform4 != null)
              {
                Transform transform5 = transform4.Find("keyboard");
                if (transform5 != null)
                  ComponentUtils.AddComponent<KeyboardController>((Component) transform5);
              }
              if (((UnityEngine.Object) UtilMenuMain.Instance != (UnityEngine.Object) null))
              {
                UtilMenuMain.Instance.menu = gameObject4;
                UtilMenuMain.Instance.buttonMat = Plugin.LoadAssetSafe<Material>(this._assetBundle, "buttons");
                UtilMenuMain.Instance.pressedButtonMat = Plugin.LoadAssetSafe<Material>(this._assetBundle, "selectedButton");
                UtilMenuMain.Instance.innerBtnMat = Plugin.LoadAssetSafe<Material>(this._assetBundle, "inner button");
                UtilMenuMain.Instance.selectedBtnMat = Plugin.LoadAssetSafe<Material>(this._assetBundle, "selectedButton");
                UtilMenuMain.Instance.panelMat = Plugin.LoadAssetSafe<Material>(this._assetBundle, "panel");
                UtilMenuMain.Instance.Icons.ThreeFriends = Plugin.LoadAssetSafe<Material>(this._assetBundle, "3friends");
                UtilMenuMain.Instance.Icons.FourPeople = Plugin.LoadAssetSafe<Material>(this._assetBundle, "4people");
                UtilMenuMain.Instance.Icons.Backpack = Plugin.LoadAssetSafe<Material>(this._assetBundle, "backpack");
                UtilMenuMain.Instance.Icons.Backward = Plugin.LoadAssetSafe<Material>(this._assetBundle, "backward");
                UtilMenuMain.Instance.Icons.CellTower = Plugin.LoadAssetSafe<Material>(this._assetBundle, "cellTower");
                UtilMenuMain.Instance.Icons.ClosedBook = Plugin.LoadAssetSafe<Material>(this._assetBundle, "closedBook");
                UtilMenuMain.Instance.Icons.ClothesHanger = Plugin.LoadAssetSafe<Material>(this._assetBundle, "clothesHanger");
                UtilMenuMain.Instance.Icons.Crosshair = Plugin.LoadAssetSafe<Material>(this._assetBundle, "crosshair");
                UtilMenuMain.Instance.Icons.Discord = Plugin.LoadAssetSafe<Material>(this._assetBundle, "discord");
                UtilMenuMain.Instance.Icons.FileSettingsIcon = Plugin.LoadAssetSafe<Material>(this._assetBundle, "fileSettingsIcon");
                UtilMenuMain.Instance.Icons.Forward = Plugin.LoadAssetSafe<Material>(this._assetBundle, "forward");
                UtilMenuMain.Instance.Icons.Headset = Plugin.LoadAssetSafe<Material>(this._assetBundle, "hedset");
                UtilMenuMain.Instance.Icons.HorizontalBarList = Plugin.LoadAssetSafe<Material>(this._assetBundle, "horizontalBarList");
                UtilMenuMain.Instance.Icons.Info = Plugin.LoadAssetSafe<Material>(this._assetBundle, "info");
                UtilMenuMain.Instance.Icons.MagnifyingGlass = Plugin.LoadAssetSafe<Material>(this._assetBundle, "magnifyingGlass");
                UtilMenuMain.Instance.Icons.Mic = Plugin.LoadAssetSafe<Material>(this._assetBundle, "mic");
                UtilMenuMain.Instance.Icons.Minus = Plugin.LoadAssetSafe<Material>(this._assetBundle, "minus");
                UtilMenuMain.Instance.Icons.OpenFolderIcon = Plugin.LoadAssetSafe<Material>(this._assetBundle, "openFolderIcon");
                UtilMenuMain.Instance.Icons.Options = Plugin.LoadAssetSafe<Material>(this._assetBundle, "options");
                UtilMenuMain.Instance.Icons.Paintbrush = Plugin.LoadAssetSafe<Material>(this._assetBundle, "paintbrush");
                UtilMenuMain.Instance.Icons.Plus = Plugin.LoadAssetSafe<Material>(this._assetBundle, "plus");
                UtilMenuMain.Instance.Icons.Radar = Plugin.LoadAssetSafe<Material>(this._assetBundle, "radar");
                UtilMenuMain.Instance.Icons.RecordIcon = Plugin.LoadAssetSafe<Material>(this._assetBundle, "recordIcon");
                UtilMenuMain.Instance.Icons.RecordIcon1 = Plugin.LoadAssetSafe<Material>(this._assetBundle, "recordIcon 1");
                UtilMenuMain.Instance.Icons.RingingBell = Plugin.LoadAssetSafe<Material>(this._assetBundle, "ringingBell");
                UtilMenuMain.Instance.Icons.Save = Plugin.LoadAssetSafe<Material>(this._assetBundle, "save");
                UtilMenuMain.Instance.Icons.Server = Plugin.LoadAssetSafe<Material>(this._assetBundle, "server");
                UtilMenuMain.Instance.Icons.SnowCloud = Plugin.LoadAssetSafe<Material>(this._assetBundle, "snowCloud");
                UtilMenuMain.Instance.Icons.Spotify = Plugin.LoadAssetSafe<Material>(this._assetBundle, "spotify");
                UtilMenuMain.Instance.Icons.StopIcon = Plugin.LoadAssetSafe<Material>(this._assetBundle, "stopIcon");
                UtilMenuMain.Instance.Icons.TrashIcon = Plugin.LoadAssetSafe<Material>(this._assetBundle, "trashIcon");
                UtilMenuMain.Instance.Icons.Umbrella = Plugin.LoadAssetSafe<Material>(this._assetBundle, "umbrella");
                UtilMenuMain.Instance.Icons.LightningShockHead = Plugin.LoadAssetSafe<Material>(this._assetBundle, "lightningShockHead");
                UtilMenuMain.Instance.Icons.MoveArrows = Plugin.LoadAssetSafe<Material>(this._assetBundle, "moveArrows");
                UtilMenuMain.Instance.Icons.FriendsConnected = Plugin.LoadAssetSafe<Material>(this._assetBundle, "friendsConnected");
              }
            }
            ((Component) this).gameObject.AddComponent<InputManager>();
            ((Component) this).gameObject.AddComponent<HapticEngine>();
            if (FeatureToggles.ShowDesktopCasting)
              new GameObject("ControlWalkModeParent").AddComponent<SakuraaCastingMod.Desktop.ControlMode.Main.ControlMode>();
            UnityEngine.Debug.Log((object) "Loaded Camera Client Assets!");
          }
        }
        else
        {
          UnityEngine.Debug.LogError((object) "Failed to find asset bundle2!");
          assetBundle.Unload(true);
          Notification.Send("Failed to load Sakuraa Client Assets!", Color.red);
        }
      }
      else
      {
        UnityEngine.Debug.LogError((object) "Failed to load AssetBundle1!");
        Notification.Send("Failed to load CastingMod Assets!", Color.red);
      }
    }
  }

  public void UpdateTabletScale(float size)
  {
    if (((UnityEngine.Object) this.tabletObj == (UnityEngine.Object) null))
      return;
    Transform parent = ((Component) this.camera).transform.parent;
    ((Component) this.camera).transform.parent = (Transform) null;
    float num = 0.75f * size;
    this.tabletObj.transform.localScale = new Vector3(num, num, num);
    ((Component) this.camera).transform.parent = parent;
  }

  public void InitTablet()
  {
    Transform parent1 = this.tabletObj.transform.Find("SimpleOptions");
    if (((UnityEngine.Object) parent1 != (UnityEngine.Object) null))
    {
      // ISSUE: method pointer
      SetButtonListener(parent1, "1stPerspectiveBtn", new UnityAction(TabletController.Ins.FirstPersonButton));
      // ISSUE: method pointer
      SetButtonListener(parent1, "3rdPerspectiveBtn", new UnityAction(TabletController.Ins.ThirdPersonButton));
      // ISSUE: method pointer
      SetButtonListener(parent1, "GoProModeBtn", new UnityAction(TabletController.Ins.GoProModeButton));
      // ISSUE: method pointer
      SetButtonListener(parent1, "FollowModeBtn", new UnityAction(TabletController.Ins.FollowModeButton));
      // ISSUE: method pointer
      SetButtonListener(parent1, "TrackMonkeBtn", new UnityAction(TabletController.Ins.TrackMonkeButton));
      // ISSUE: method pointer
      SetButtonListener(parent1, "LockBtn", new UnityAction(TabletController.Ins.LockCameraButton));
      // ISSUE: method pointer
      SetButtonListener(parent1, "FlipBtn", new UnityAction(TabletController.Ins.FlipButton));
      // ISSUE: method pointer
      SetButtonListener(parent1, "RecorderToggleBtn", new UnityAction(TabletController.Ins.ToggleRecordingPanel));
      // ISSUE: method pointer
      SetButtonListener(parent1, "SettingsBtn", new UnityAction(TabletController.Ins.SettingsButton));
    }
    Transform parent2 = this.tabletObj.transform.Find("AdvancedOptions");
    if (((UnityEngine.Object) parent2 != (UnityEngine.Object) null))
    {
      TabletController.Ins.dynamicSlider = ((Component) parent2.Find("Dynamic_slider")).gameObject;
      // ISSUE: method pointer
      SetButtonListener(parent2.Find("Dynamic_slider"), "LeftButton", new UnityAction(TabletController.Ins.DynamicSliderLeft));
      // ISSUE: method pointer
      SetButtonListener(parent2.Find("Dynamic_slider"), "RightButton", new UnityAction(TabletController.Ins.DynamicSliderRight));
      // ISSUE: method pointer
      SetButtonListener(parent2.Find("Fov_slider"), "LeftButton", new UnityAction(TabletController.Ins.FOVSliderLeft));
      // ISSUE: method pointer
      SetButtonListener(parent2.Find("Fov_slider"), "RightButton", new UnityAction(TabletController.Ins.FOVSliderRight));
      // ISSUE: method pointer
      SetButtonListener(parent2.Find("Clipping_slider"), "LeftButton", new UnityAction(TabletController.Ins.ClippingSliderLeft));
      // ISSUE: method pointer
      SetButtonListener(parent2.Find("Clipping_slider"), "RightButton", new UnityAction(TabletController.Ins.ClippingSliderRight));
      // ISSUE: method pointer
      SetButtonListener(parent2.Find("FpvPos_slider"), "LeftButton", new UnityAction(TabletController.Ins.FpvPosSliderLeft));
      // ISSUE: method pointer
      SetButtonListener(parent2.Find("FpvPos_slider"), "RightButton", new UnityAction(TabletController.Ins.FpvPosSliderRight));
      if (((UnityEngine.Object) parent2.Find("Clamping_slider") != (UnityEngine.Object) null))
      {
        TabletController.Ins.clampingSlider = ((Component) parent2.Find("Clamping_slider")).gameObject;
        // ISSUE: method pointer
        SetButtonListener(parent2.Find("Clamping_slider"), "LeftButton", new UnityAction(TabletController.Ins.ClampingSliderLeft));
        // ISSUE: method pointer
        SetButtonListener(parent2.Find("Clamping_slider"), "RightButton", new UnityAction(TabletController.Ins.ClampingSliderRight));
      }
      // ISSUE: method pointer
      SetButtonListener(parent2, "GoProModeBtn", new UnityAction(TabletController.Ins.GoProModeButton));
      // ISSUE: method pointer
      SetButtonListener(parent2, "FollowModeBtn", new UnityAction(TabletController.Ins.FollowModeButton));
      // ISSUE: method pointer
      SetButtonListener(parent2, "1stPerspectiveBtn", new UnityAction(TabletController.Ins.FirstPersonButton));
      // ISSUE: method pointer
      SetButtonListener(parent2, "LockBtn", new UnityAction(TabletController.Ins.LockCameraButton));
      // ISSUE: method pointer
      SetButtonListener(parent2, "TrackMonkeBtn", new UnityAction(TabletController.Ins.TrackMonkeButton));
      // ISSUE: method pointer
      SetButtonListener(parent2, "3rdPerspectiveBtn", new UnityAction(TabletController.Ins.ThirdPersonButton));
      // ISSUE: method pointer
      SetButtonListener(parent2, "FlipBtn2", new UnityAction(TabletController.Ins.FlipButton));
      // ISSUE: method pointer
      SetButtonListener(parent2, "SettingsBtn2", new UnityAction(TabletController.Ins.SettingsButton));
      // ISSUE: method pointer
      SetButtonListener(parent2, "RecorderToggleBtn", new UnityAction(TabletController.Ins.ToggleRecordingPanel));
    }
    Transform parent3 = this.tabletObj.transform.Find("ExtraOptions");
    if (!((UnityEngine.Object) parent3 != (UnityEngine.Object) null))
      return;
    // ISSUE: method pointer
    SetButtonListener(parent3, "RollLockBtn", new UnityAction(TabletController.Ins.RollLockButton));
    // ISSUE: method pointer
    SetButtonListener(parent3, "CosmeticsBtn", new UnityAction(TabletController.Ins.CosmeticButton));
    // ISSUE: method pointer
    SetButtonListener(parent3.Find("Rotation_slider"), "LeftButton", new UnityAction(TabletController.Ins.RotationSliderLeft));
    // ISSUE: method pointer
    SetButtonListener(parent3.Find("Rotation_slider"), "RightButton", new UnityAction(TabletController.Ins.RotationSliderRight));
    // ISSUE: method pointer
    SetButtonListener(parent3.Find("Distance_slider"), "LeftButton", new UnityAction(TabletController.Ins.DistanceSliderLeft));
    // ISSUE: method pointer
    SetButtonListener(parent3.Find("Distance_slider"), "RightButton", new UnityAction(TabletController.Ins.DistanceSliderRight));
    // ISSUE: method pointer
    SetButtonListener(parent3.Find("Speed_slider"), "LeftButton", new UnityAction(TabletController.Ins.SpeedSliderLeft));
    // ISSUE: method pointer
    SetButtonListener(parent3.Find("Speed_slider"), "RightButton", new UnityAction(TabletController.Ins.SpeedSliderRight));
    // ISSUE: method pointer
    SetButtonListener(parent3.Find("Size_slider"), "LeftButton", new UnityAction(TabletController.Ins.SizeSliderLeft));
    // ISSUE: method pointer
    SetButtonListener(parent3.Find("Size_slider"), "RightButton", new UnityAction(TabletController.Ins.SizeSliderRight));
    // ISSUE: method pointer
    SetButtonListener(parent3, "FlipBtn3", new UnityAction(TabletController.Ins.FlipButton));
    // ISSUE: method pointer
    SetButtonListener(parent3, "RecorderToggleBtn", new UnityAction(TabletController.Ins.ToggleRecordingPanel));
    // ISSUE: method pointer
    SetButtonListener(parent3, "SettingsBtn3", new UnityAction(TabletController.Ins.SettingsButton));

    static void SetButtonListener(Transform parent, string buttonName, UnityAction action)
    {
      try
      {
        Transform transform = parent.Find(buttonName);
        if (!((UnityEngine.Object) transform == (UnityEngine.Object) null))
        {
          GameObject gameObject = ((Component) transform).gameObject;
          gameObject.layer = 18;
          TabletController.Ins.requiredLayerObjs.Add(gameObject);
          GorillaFingerButton gorillaFingerButton = gameObject.AddComponent<GorillaFingerButton>();
          gorillaFingerButton.unpressedMaterial = UtilMenuMain.Instance?.buttonMat;
          gorillaFingerButton.pressedMaterial = UtilMenuMain.Instance?.pressedButtonMat;
          UnityEvent unityEvent = new UnityEvent();
          unityEvent.AddListener(action);
          gorillaFingerButton.onPressButton = unityEvent;
        }
        else
          UnityEngine.Debug.LogWarning((object) $"Button {buttonName} not found in {((UnityEngine.Object) parent).name}");
      }
      catch (Exception ex)
      {
        UnityEngine.Debug.LogError((object) $"Error setting up button {buttonName}: {ex}");
      }
    }
  }

  public void ResetCameraObject()
  {
    ((Component) ((Component) this.camera).transform.GetChild(0)).gameObject.SetActive(false);
    ((Component) this.camera).transform.parent = this.cameraParent;
    try
    {
      UnityEngine.Object.DestroyImmediate((UnityEngine.Object) Drone.DroneAudioSource);
    }
    catch
    {
    }
    Drone.IsCrashed = false;
    try
    {
      UnityEngine.Object.DestroyImmediate((UnityEngine.Object) ((Component) ((Component) this.camera).transform).GetComponent<Rigidbody>());
    }
    catch
    {
    }
    try
    {
      UnityEngine.Object.DestroyImmediate((UnityEngine.Object) ((Component) ((Component) this.camera).transform).GetComponent<Collider>());
    }
    catch
    {
    }
    try
    {
      if (((UnityEngine.Object) Drone.DroneBodyObj != (UnityEngine.Object) null))
        UnityEngine.Object.DestroyImmediate((UnityEngine.Object) Drone.DroneBodyObj);
    }
    catch
    {
    }
    ((Component) this.camera).transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
    ((Component) this.camera).transform.localScale = new Vector3(1f, 1f, 1f);
    FreeCam.Pos = ((Component) this.camera).transform.localPosition;
    FreeCam.Rot = ((Component) this.camera).transform.localEulerAngles;
  }

  public void OnModeChange()
  {
    Sounds.PlayCasterClick(Sounds.shootSfx, 0.06f);
    this.ResetCameraObject();
    PhotonNetworkController.Instance.disableAFKKick = true;
    SpectatedCosmeticHider.Restore();
    if (Observation.ShowNests)
      Observation.ToggleNestVisibility();
    NameTags.ClearNameTags();
    Drone.DroneStarted = false;
    Cursor.lockState = (CursorLockMode) 0;
    Cursor.visible = true;
    AutoPilot.AutoPilotEnabled = false;
    foreach (KeyValuePair<string, CachedPlayerCard> playerCard in Leaderboard.PlayerCards)
      UnityEngine.Object.Destroy((UnityEngine.Object) playerCard.Value.GameObject);
    Leaderboard.PlayerCards.Clear();
    UnityEngine.Object.Destroy((UnityEngine.Object) LavaDistance.PlayerDistanceCard);
    if (this.currentCameraMode != 4)
      return;
    this.camera.fieldOfView = 90f;
  }

  private void LateUpdate()
  {
    Interpolation.ApplyInterpolationSettings();
    NameTags.LateUpdate();
  }

  private void Update()
  {
    if ((this.modDisabled ? 1 : (!this._initialized ? 1 : 0)) != 0)
      return;
    GorillaDataHandler.SafeUpdate();
    Networking.InRoom = Plugin.ReplayCompatibilityMode || NetworkSystem.Instance.InRoom;
    Networking.ScanForManagerUpdate();
    if (this._lastFrameMode != this.currentCameraMode)
      this.OnModeChange();
    if (((UnityEngine.Object) this._mirroredDisplayCam != (UnityEngine.Object) null))
    {
      this._mirroredDisplayCam.fieldOfView = this.camera.fieldOfView;
      this._mirroredDisplayCam.nearClipPlane = this.camera.nearClipPlane;
    }
    if (FeatureToggles.ShowDesktopCasting)
    {
      switch (this.currentCameraMode)
      {
        case 4:
          Drone.UpdateDroneMode();
          break;
        case 5:
          PlayerSpec.UpdatePlayerSpec();
          break;
        case 6:
          Observation.UpdateObservationSpec();
          break;
        case 7:
          Director.UpdateDirector();
          break;
      }
      Keybinds.UpdateCheck();
      FreeCam.MovementMultiplier = (Keyboard.current != null && Keyboard.current.shiftKey.isPressed) | (Gamepad.current != null && (Gamepad.current.leftStickButton.isPressed || Gamepad.current.rightStickButton.isPressed)) ? 15f : 5f;
      if (this.currentCameraMode == 2)
      {
        bool flag;
        Cursor.lockState = (flag = !MainMenus.ShowMainMenu && Mouse.current != null && Mouse.current.rightButton.isPressed) ? (CursorLockMode) 1 : (CursorLockMode) 0;
        Cursor.visible = !flag;
      }
      // Final say: if the desktop menu is open the pointer must be usable. Every camera
      // mode already intends this, but each one manages the cursor on its own and any
      // missed path leaves it captured, which makes the whole GUI unclickable.
      if (MainMenus.ShowMainMenu && Cursor.lockState != (CursorLockMode) 0)
      {
        Cursor.lockState = (CursorLockMode) 0;
        Cursor.visible = true;
      }
    }
    RewindViewer.CaptureFrame();
    RewindViewer.UpdatePlayback();
    MiniMap.UpdateMap();
    ToggleMicOverlay.UpdatePosition();
    Scoreboard.UpdateScoreboard();
    LavaDistance.UpdateDistances();
    Notification.UpdateDuration();
    if (((UnityEngine.Object) this._playerListener != (UnityEngine.Object) null))
      ((Behaviour) this._playerListener).enabled = !Plugin.listenerBool;
    if (((UnityEngine.Object) this._cameraListener != (UnityEngine.Object) null))
      ((Behaviour) this._cameraListener).enabled = Plugin.listenerBool;
    Networking.InRoom = Plugin.ReplayCompatibilityMode || NetworkSystem.Instance.InRoom;
    FpsCounter.UpdateCheck();
    this._lastFrameMode = this.currentCameraMode;
    JoinForensics.Tick(Time.unscaledDeltaTime * 1000f);
    SoundboardTriggers.Tick();
    SoundboardKeybinds.Tick();
    ModMessageHandler.CheckMessages();
  }

  public void FixedUpdate()
  {
    if ((this.modDisabled ? 1 : (!this._initialized ? 1 : 0)) != 0)
      return;
    switch (this.currentCameraMode)
    {
      case 0:
        this.FDisabledSpec();
        break;
      case 2:
        FreeCam.FUpdateFreecam();
        break;
      case 3:
        if (((UnityEngine.Object) SakuraaCastingMod.Desktop.ControlMode.Main.ControlMode.Instance != (UnityEngine.Object) null))
        {
          SakuraaCastingMod.Desktop.ControlMode.Main.ControlMode.Instance.FUpdateControlMode();
          break;
        }
        break;
      case 4:
        Drone.FixedUpdateDronePhysics();
        break;
    }
    FpsCounter.FixedUpdateCheck();
    Scoreboard.TransformScoreboards();
  }

  private void FDisabledSpec()
  {
    if (((UnityEngine.Object) ((Component) this.camera).transform.parent == (UnityEngine.Object) this.originalCameraParent))
      return;
    Transform transform;
    ((Component) (transform = ((Component) this.camera).transform).GetChild(0)).gameObject.SetActive(true);
    transform.parent = this.originalCameraParent;
    transform.localPosition = this.originalPos;
    transform.localRotation = this.originalRot;
  }

  private void OnGUI()
  {
    SakuraaCastingMod.Desktop.Ui.Framework.InputDiag.NoteGlobal(Event.current);
    Notification.DrawNotifier();
    if ((this.modDisabled ? 1 : (!this._initialized ? 1 : 0)) != 0 || !FeatureToggles.ShowDesktopCasting)
      return;
    if (FpsCounter.ShowFps)
      FpsCounter.Draw();
    if (MainMenus.ShowMainMenu)
      MainMenus.Draw();
    if (Networking.InRoom)
    {
      if (Leaderboard.ShowLeaderboard)
        Leaderboard.DrawLeaderboard();
      bool flag1;
      if (flag1 = LavaDistance.ShowDistanceCard)
      {
        bool flag2;
        switch (this.currentCameraMode)
        {
          case 5:
          case 6:
            flag2 = true;
            break;
          default:
            flag2 = false;
            break;
        }
        flag1 = flag2;
      }
      if (flag1)
        LavaDistance.DrawDistanceCard();
    }
    else if (Leaderboard.PlayerCards.Count > 0)
      Leaderboard.Clear();
    RewindViewer.DrawRewindWindow();
    LayoutEditor.Draw();
    if (!MainMenus.ShowMainMenu)
      return;
    LayoutEditor.DrawToggleButton();
  }

  private void OnApplicationQuit() => OfflinePresetStore.SaveTabletSettings();

  public abstract class CameraModules
  {
    public const int Disabled = 0;
    public const int Tablet = 1;
    public const int Freecam = 2;
    public const int Control = 3;
    public const int Drone = 4;
    public const int Player = 5;
    public const int Observe = 6;
    public const int Director = 7;
  }
}
