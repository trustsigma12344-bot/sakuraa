using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GorillaNetworking;
using PlayFab;
using PlayFab.ClientModels;
using SakMerge.Api;
using SakuraaCastingMod.Features.Overlays;
using SakuraaCastingMod.Shared.Helpers;
using SakuraaCastingMod.Shared.Models;
using SakuraaCastingMod.VR.Interaction;
using SakuraaCastingMod.VR.UtilMenu.Utility;
using UnityEngine;
using UnityEngine.Events;

namespace SakuraaCastingMod.VR.UtilMenu.Pages;

public class LobbyPage : BasePage
{
	public class HoldableButton : MonoBehaviour
	{
		public Action Callback;

		public float HoldTime = 2f;

		public MeshRenderer TargetRenderer;

		public Material UnpressedMat;

		public Material PressedMat;

		private bool _isHolding;

		private bool _holdingHandLeft;

		private float _timer;

		private Color _startColor;

		private Color _endColor;

		private bool _colorsInitialized;

		private void Start()
		{
			if (TargetRenderer == null)
			{
				TargetRenderer = GetComponent<MeshRenderer>();
			}
			if (UnpressedMat != null && UnpressedMat.HasProperty("_Color"))
			{
				_startColor = UnpressedMat.color;
			}
			else if (!(TargetRenderer != null))
			{
				_startColor = Color.white;
			}
			else
			{
				_startColor = TargetRenderer.sharedMaterial.color;
			}
			if (!(PressedMat != null) || !PressedMat.HasProperty("_Color"))
			{
				_endColor = Color.red;
			}
			else
			{
				_endColor = PressedMat.color;
			}
			_colorsInitialized = true;
		}

		private void Update()
		{
			if (!_colorsInitialized || TargetRenderer == null)
			{
				return;
			}
			if (!_isHolding)
			{
				if (_timer > 0f)
				{
					_timer = 0f;
					ResetHold();
				}
				return;
			}
			_timer += Time.deltaTime;
			float num = Mathf.Clamp01(_timer / HoldTime);
			TargetRenderer.material.color = Color.Lerp(_startColor, _endColor, num);
			HapticEngine.SetHoldProgress(_holdingHandLeft, num);
			if (_timer >= HoldTime)
			{
				Callback?.Invoke();
				HapticEngine.Play(HapticPreset.HoldConfirm, _holdingHandLeft);
				if (GorillaTagger.Instance != null && GorillaTagger.Instance.offlineVRRig != null)
				{
					GorillaTagger.Instance.offlineVRRig.PlayHandTapLocal(67, _holdingHandLeft, 0.25f);
				}
				ResetHold();
			}
		}

		private void OnTriggerEnter(Collider other)
		{
			GorillaTriggerColliderHandIndicator componentInParent = other.GetComponentInParent<GorillaTriggerColliderHandIndicator>();
			if (componentInParent != null)
			{
				_isHolding = true;
				_holdingHandLeft = componentInParent.isLeftHand;
				HapticEngine.Play(HapticPreset.HoldStart, _holdingHandLeft);
			}
		}

		private void OnTriggerExit(Collider other)
		{
			if ((bool)other.GetComponentInParent<GorillaTriggerColliderHandIndicator>())
			{
				if (_isHolding && _timer > 0f)
				{
					HapticEngine.Play(HapticPreset.HoldCancel, _holdingHandLeft);
				}
				ResetHold();
			}
		}

		private void ResetHold()
		{
			_isHolding = false;
			_timer = 0f;
			HapticEngine.CancelHold(_holdingHandLeft);
			if (TargetRenderer != null && UnpressedMat != null)
			{
				TargetRenderer.material = UnpressedMat;
			}
		}
	}

	private static Dictionary<string, string> _hardcodedSafeMods = new Dictionary<string, string>
	{
		{ "HP_Left", "HOLDABLEPAD" },
		{ "RGBA", "CUSTOMCOSMETICS" },
		{ "cheese is gouda", "WHOSICHEATING" },
		{ "shirtversion", "GORILLASHIRTS" },
		{ "gpronouns", "GORILLAPRONOUNS" },
		{ "gfaces", "GORILLAFACES" },
		{ "monkephone", "MONKEPHONE" },
		{ "cokecosmetics", "COKE COSMETX" },
		{ "GFaces", "gFACES" },
		{ "github.com/maroon-shadow/SimpleBoards", "SIMPLEBOARDS" },
		{ "GTrials", "gTRIALS" },
		{ "github.com/ZlothY29IQ/GorillaMediaDisplay", "GMD" },
		{ "github.com/ZlothY29IQ/TooMuchInfo", "TOOMUCHINFO" },
		{ "gorillastats", "GORILLASTATS" },
		{ "using gorilladrift", "GORILLADRIFT" },
		{ "monkehavocversion", "MONKEHAVOC" },
		{ "tictactoe", "TICTACTOE" },
		{ "github.com/ZlothY29IQ/MonkeClick", "MONKECLICK" },
		{ "pmversion", "PLAYERMODELS" },
		{ "gtrials", "GORILLATRIALS" },
		{ "imposter", "GORILLAAMONGUS" },
		{ "spectapeversion", "SPECTAPE" },
		{ "CarName", "GORILLAVEHICLES" },
		{ "FPS-Nametags for Zlothy", "FPSTAGS" },
		{ "github.com/ZlothY29IQ/RoomUtils-IW", "ROOMUTILS-IW" },
		{ "msp", "MONKESMARTPHONE" },
		{ "MP25", "MONKEPHONE" },
		{ "GorillaWatch", "GORILLAWATCH" },
		{ "InfoWatch", "GORILLAINFOWATCH" },
		{ "cats", "CATS" },
		{ "made by biotest05 :3", "DOGS" },
		{ "colour", "CUSTOMCOSMETICS" },
		{ "chainedtogether", "CHAINED TOGETHER" },
		{ "GrateVersion", "GRATE" },
		{ "BANANAOS", "BANANAOS" },
		{ "GC", "GORILLACRAFT" },
		{ "BananaPhone", "BANANAPHONE" },
		{ "GS", "OLD SHIRTS" },
		{ "CustomMaterial", "CUSTOMCOSMETICS" },
		{ "github.com/ZlothY29IQ/MonkeClick-CI", "MONKECLICK-CI" },
		{ "github.com/ZlothY29IQ/MonkeRealism", "MONKEREALISM" },
		{ "MediaPad", "MEDIAPAD" },
		{ "GorillaCinema", "gCINEMA" },
		{ "ChainedTogetherActive", "CHAINEDTOGETHER" },
		{ "GPronouns", "gPRONOUNS" },
		{ "CSVersion", "CustomSkin" },
		{ "ShirtProperties", "SHIRTS-OLD" },
		{ "GorillaShirts", "SHIRTS" }
	};

	private static Dictionary<string, string> _hardcodedCheats = new Dictionary<string, string>
	{
		{ "genesis", "GENESIS" },
		{ "Vivid", "VIVID" },
		{ "oblivionuser", "OBLIVION" },
		{ "hgrehngio889584739_hugb\n", "RESURGENCE" },
		{ "eyerock reborn", "EYEROCK" },
		{ "asteroidlite", "ASTEROID LITE" },
		{ "elux", "ELUX" },
		{ "ObsidianMC", "OBSIDIAN" },
		{ "hgrehngio889584739_hugb", "RESURGENCE" },
		{ "cronos", "CRONOS" },
		{ "ORBIT", "ORBIT" },
		{ "Violet On Top", "VIOLET" },
		{ "void_menu_open", "VOID" },
		{ "violetpaiduser", "VIOLETPAID" },
		{ "violetfree", "VIOLETFREE" },
		{ "void", "VOID" },
		{ "obsidianmc", "OBSIDIAN.LOL" },
		{ "I like cheese", "RECROOMRIG" },
		{ "emotewheel", "EMOTEWHEEL" },
		{ "dark", "SHIBAGT DARK" },
		{ "hidden menu", "HIDDEN" },
		{ "untitled", "UNTITLED" }
	};

	private static Dictionary<string, string> _hardcodedUnknownMods = new Dictionary<string, string>
	{
		{ "ccolor", "INDEX" },
		{ "6p72ly3j85pau2g9mda6ib8px", "CCMV2" },
		{ "fys cool magic mod", "FYSMAGICMOD" },
		{ "goofywalkversion", "GOOFYWALK" },
		{ "6XpyykmrCthKhFeUfkYGxv7xnXpoe2", "CCMV2" },
		{ "Body Tracking", "BODYTRACK-OLD" },
		{ "Body Estimation", "HANBodyEst" },
		{ "Gorilla Track", "BODYTRACK" },
		{ "silliness", "SILLINESS" },
		{ "github.com/ZlothY29IQ/Zloth-RecRoomRig", "ZLOTH-RRR" }
	};

	private static Dictionary<string, string> _webKnownMods = new Dictionary<string, string>();

	private static Dictionary<string, string> _webKnownCheats = new Dictionary<string, string>();

	private static Dictionary<string, float> _cosmeticSuspicionTimes = new Dictionary<string, float>();

	private UtilTab _lobbyTab;

	private UtilTab _leaderboardTab;

	private UtilTab _gunTab;

	private UtilTab _friendsTab;

	private NetPlayer _selectedPlayer;

	private string _selectedPlayerId;

	private int _playerListOffset = 0;

	private string _lastPlayerListHash = "";

	private bool _wasInRoom = false;

	private GameObject _pointerObject;

	private GameObject _selectionIndicator;

	private bool _isSelectingReport = false;

	private int _reportReasonIndex = 0;

	private readonly GorillaPlayerLineButton.ButtonType[] _reportTypes = new GorillaPlayerLineButton.ButtonType[3]
	{
		GorillaPlayerLineButton.ButtonType.HateSpeech,
		GorillaPlayerLineButton.ButtonType.Cheating,
		GorillaPlayerLineButton.ButtonType.Toxicity
	};

	private int _friendPageIndex = 0;

	private const int FriendsPerPage = 4;

	private HashSet<string> _lastOnlineFriends = new HashSet<string>();

	private bool _hasDoneInitialFriendCheck = false;

	private string _roomCodeInput = "";

	private bool _isEnteringCode = false;

	private static GameObject _cachedSelectorParent;

	private bool _viewingMods = false;

	private bool _viewingCheats = false;

	private MiniRigController _miniRigController;

	private static Dictionary<string, DateTime> _playerJoinDates = new Dictionary<string, DateTime>();

	private static Dictionary<string, bool> _playerJoinedAfterPaid = new Dictionary<string, bool>();

	private static readonly HashSet<string> _joinDateInFlight = new HashSet<string>();

	private static readonly HashSet<string> _joinDateFailed = new HashSet<string>();

	private static readonly MethodInfo _joinQueueMethod = typeof(GorillaComputer).GetMethod("JoinQueue", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[2]
	{
		typeof(string),
		typeof(bool)
	}, null);

	private static bool _lobbyHopInFlight;

	private static MenuElement _lobbyHopElement;

	public override string PageName => "LOBBY";

	public override Material PageIcon => UtilMenuMain.Instance.Icons.Radar;

	public override void Start()
	{
		base.Start();
		UtilMenuMain.Instance.StartCoroutine(FriendMonitorRoutine());
		UtilMenuMain.Instance.StartCoroutine(FetchKnownMods());
		UtilMenuMain.Instance.StartCoroutine(FetchKnownCheats());
		UtilMenuMain.Instance.StartCoroutine(CosmeticSuspicionRoutine());
		UtilMenuMain.Instance.StartCoroutine(CleanupLoop());
		if (_miniRigController == null)
		{
			GameObject gameObject = new GameObject("SakuraMiniRigHost");
			UnityEngine.Object.DontDestroyOnLoad(gameObject);
			_miniRigController = gameObject.AddComponent<MiniRigController>();
		}
		SetupHoldButtons();
	}

	private void SetupHoldButtons()
	{
		if (UtilMenuController.Instance == null)
		{
			return;
		}
		MakeButtonHoldable(UtilMenuController.Instance.cloneCosmeticsBtnObj, delegate
		{
			if (UtilMenuController.Instance.CurrentPage is LobbyPage lobbyPage)
			{
				lobbyPage.OnCloneCosmetics();
			}
		});
		if (UtilMenuController.Instance.reportMainBtn != null)
		{
			MakeButtonHoldable(UtilMenuController.Instance.reportMainBtn.gameObject, delegate
			{
				if (UtilMenuController.Instance.CurrentPage is LobbyPage lobbyPage)
				{
					lobbyPage.OnMainReportBtn();
				}
			});
		}
		if (UtilMenuController.Instance.reportSendBtn != null)
		{
			MakeButtonInstant(UtilMenuController.Instance.reportSendBtn.gameObject, delegate
			{
				if (UtilMenuController.Instance.CurrentPage is LobbyPage lobbyPage)
				{
					lobbyPage.OnSendReport();
				}
			});
		}
		if (UtilMenuController.Instance.reportSliderLeft != null)
		{
			MakeButtonInstant(UtilMenuController.Instance.reportSliderLeft.gameObject, delegate
			{
				if (UtilMenuController.Instance.CurrentPage is LobbyPage lobbyPage)
				{
					lobbyPage.OnReportSlider(left: true);
				}
			});
		}
		if (!(UtilMenuController.Instance.reportSliderRight != null))
		{
			return;
		}
		MakeButtonInstant(UtilMenuController.Instance.reportSliderRight.gameObject, delegate
		{
			if (UtilMenuController.Instance.CurrentPage is LobbyPage lobbyPage)
			{
				lobbyPage.OnReportSlider(left: false);
			}
		});
	}

	public static void MakeButtonInstant(GameObject obj, UnityAction callback)
	{
		if (!(obj == null))
		{
			HoldableButton component = obj.GetComponent<HoldableButton>();
			if ((bool)component)
			{
				UnityEngine.Object.Destroy(component);
			}
			UtilFingerButton component2 = obj.GetComponent<UtilFingerButton>();
			if ((bool)component2)
			{
				component2.enabled = true;
				component2.SetButtonListener(callback);
			}
		}
	}

	public static void MakeButtonHoldable(GameObject obj, Action callback, float duration = 1f)
	{
		if (obj == null || obj.GetComponent<HoldableButton>() != null)
		{
			return;
		}
		UtilFingerButton component = obj.GetComponent<UtilFingerButton>();
		if ((bool)component)
		{
			component.enabled = false;
		}
		GorillaFingerButton component2 = obj.GetComponent<GorillaFingerButton>();
		if ((bool)component2)
		{
			component2.enabled = false;
		}
		HoldableButton holdableButton = obj.AddComponent<HoldableButton>();
		holdableButton.Callback = callback;
		holdableButton.HoldTime = duration;
		if (!(component != null))
		{
			if (component2 != null)
			{
				holdableButton.TargetRenderer = component2.buttonRenderer;
				holdableButton.UnpressedMat = component2.unpressedMaterial;
				holdableButton.PressedMat = component2.pressedMaterial;
			}
		}
		else
		{
			holdableButton.TargetRenderer = component.buttonRenderer;
			holdableButton.UnpressedMat = component.unpressedMaterial;
			holdableButton.PressedMat = component.pressedMaterial;
		}
		if (holdableButton.TargetRenderer == null)
		{
			holdableButton.TargetRenderer = obj.GetComponent<MeshRenderer>();
		}
	}

	private IEnumerator CleanupLoop()
	{
		while (true)
		{
			if (UtilMenuController.Instance == null || !UtilMenuController.Instance.isMenuEnabled)
			{
				if (_selectionIndicator != null && _selectionIndicator.activeSelf)
				{
					_selectionIndicator.SetActive(value: false);
				}
				if (_pointerObject != null)
				{
					TogglePointer(enable: false);
				}
			}
			yield return new WaitForSeconds(0.2f);
		}
	}

	public override void BuildTabs()
	{
		_lobbyTab = new UtilTab
		{
			TabIcon = UtilMenuMain.Instance.Icons.CellTower,
			TabName = "Lobby"
		};
		Tabs.Add(_lobbyTab);
		RefreshLobbyTab();
		_leaderboardTab = new UtilTab
		{
			TabIcon = UtilMenuMain.Instance.Icons.FourPeople,
			TabName = "Leaderboard"
		};
		Tabs.Add(_leaderboardTab);
		_gunTab = new UtilTab
		{
			TabIcon = UtilMenuMain.Instance.Icons.Crosshair,
			TabName = "Gun"
		};
		Tabs.Add(_gunTab);
		_friendsTab = new UtilTab
		{
			TabIcon = UtilMenuMain.Instance.Icons.ThreeFriends,
			TabName = "Friends"
		};
		_friendsTab.Elements.Add(new MenuElement("REFRESH FRIENDS", RefreshFriendList));
		Tabs.Add(_friendsTab);
	}

	public override void RefreshPageUI()
	{
		if (UtilMenuController.Instance.CurrentTabIndex == 1)
		{
			UpdateLeaderboardUI();
			return;
		}
		if ((bool)UtilMenuController.Instance.playerSelectRoot)
		{
			UtilMenuController.Instance.playerSelectRoot.SetActive(value: false);
		}
		if ((bool)UtilMenuController.Instance.panel2Root)
		{
			UtilMenuController.Instance.panel2Root.SetActive(value: false);
		}
		if ((bool)UtilMenuController.Instance.barExtraRoot)
		{
			UtilMenuController.Instance.barExtraRoot.SetActive(value: false);
		}
	}

	public override bool ShouldHideStandardBars()
	{
		return UtilMenuController.Instance.CurrentTabIndex == 1 && NetworkSystem.Instance != null && NetworkSystem.Instance.InRoom;
	}

	public override void LateUpdate()
	{
		base.LateUpdate();
		bool flag = UtilMenuController.Instance.CurrentTabIndex == 1 && UtilMenuController.Instance.isMenuEnabled;
		bool flag2;
		if ((flag2 = NetworkSystem.Instance != null && NetworkSystem.Instance.InRoom) != _wasInRoom)
		{
			_wasInRoom = flag2;
			if (!flag2)
			{
				ResetSelection();
				if (_pointerObject != null)
				{
					TogglePointer(enable: false);
				}
			}
			if (flag)
			{
				UtilMenuController.Instance.RefreshUI();
			}
		}
		if (!flag2)
		{
			flag = false;
		}
		if (!flag)
		{
			if (_pointerObject != null && _pointerObject.activeSelf)
			{
				_pointerObject.SetActive(value: false);
			}
			if (_selectionIndicator != null)
			{
				_selectionIndicator.SetActive(value: false);
			}
		}
		else
		{
			if (_selectedPlayer != null)
			{
				if (!(_selectedPlayer.UserId != _selectedPlayerId))
				{
					VRRig rigByNetPlayer = PlayerTranslator.GetRigByNetPlayer(_selectedPlayer);
					if (rigByNetPlayer != null && !rigByNetPlayer.isLocal)
					{
						if (_selectionIndicator == null)
						{
							CreateSelectionIndicator();
						}
						_selectionIndicator.SetActive(value: true);
						_selectionIndicator.transform.position = rigByNetPlayer.headMesh.transform.position + Vector3.up * 0.4f;
						_selectionIndicator.transform.LookAt(Camera.main.transform);
					}
					else if (rigByNetPlayer == null)
					{
						ResetSelection();
					}
					else if (_selectionIndicator != null)
					{
						_selectionIndicator.SetActive(value: false);
					}
				}
				else
				{
					ResetSelection();
				}
			}
			if (NetworkSystem.Instance != null && NetworkSystem.Instance.InRoom)
			{
				NetPlayer[] allNetPlayers = NetworkSystem.Instance.AllNetPlayers;
				string text = string.Join(",", allNetPlayers.Select((NetPlayer p) => p.ActorNumber));
				if (text != _lastPlayerListHash)
				{
					_lastPlayerListHash = text;
					UpdateLeaderboardUI();
				}
			}
		}
		if (UtilMenuController.Instance.CurrentTabIndex != 0 || _lobbyTab.Elements.Count <= 3)
		{
			return;
		}
		MenuElement menuElement = _lobbyTab.Elements[0];
		string text2 = GorillaComputer.instance.currentQueue ?? "DEFAULT";
		if (menuElement.Text != "QUEUE: " + text2)
		{
			menuElement.Text = "QUEUE: " + text2;
			UtilMenuController.Instance.RefreshUI();
		}
		MenuElement menuElement2 = _lobbyTab.Elements[1];
		string text3 = GorillaComputer.instance.currentGameMode.Value ?? "INFECTION";
		if (menuElement2.Text != "MODE: " + text3.ToUpper())
		{
			menuElement2.Text = "MODE: " + text3.ToUpper();
			UtilMenuController.Instance.RefreshUI();
		}
		MenuElement menuElement3 = _lobbyTab.Elements[3];
		if (NetworkSystem.Instance.InRoom)
		{
			_isEnteringCode = false;
			if (menuElement3.Text != "DISCONNECT")
			{
				menuElement3.Text = "DISCONNECT";
				menuElement3.Type = ElementType.Button;
				UtilMenuController.Instance.RefreshUI();
			}
			return;
		}
		string text4 = (_isEnteringCode ? _roomCodeInput : (string.IsNullOrEmpty(_roomCodeInput) ? "JOIN ROOM" : ("JOINING " + _roomCodeInput + "...")));
		ElementType elementType = (_isEnteringCode ? ElementType.Input : ElementType.Button);
		if (menuElement3.Text != text4 || menuElement3.Type != elementType)
		{
			menuElement3.Text = text4;
			menuElement3.Type = elementType;
			UtilMenuController.Instance.RefreshUI();
		}
	}

	public override void OnTabSelected(int tabIndex)
	{
		switch (tabIndex)
		{
		case 0:
			RefreshLobbyTab();
			break;
		case 1:
			UpdateLeaderboardUI();
			break;
		case 2:
			UtilMenuMain.Instance.StartCoroutine(DeferredGunTabSwitch());
			break;
		case 3:
			RefreshFriendList();
			break;
		}
		if (tabIndex != 1 && tabIndex != 2)
		{
			if (_miniRigController != null)
			{
				_miniRigController.Hide();
			}
			_selectedPlayer = null;
			_selectedPlayerId = null;
			_viewingMods = false;
			_viewingCheats = false;
			if (_pointerObject != null)
			{
				TogglePointer(enable: false);
			}
			if ((bool)_selectionIndicator)
			{
				_selectionIndicator.SetActive(value: false);
			}
			if ((bool)UtilMenuController.Instance.playerSelectRoot)
			{
				UtilMenuController.Instance.playerSelectRoot.SetActive(value: false);
			}
			if ((bool)UtilMenuController.Instance.panel2Root)
			{
				UtilMenuController.Instance.panel2Root.SetActive(value: false);
			}
			if ((bool)UtilMenuController.Instance.barExtraRoot)
			{
				UtilMenuController.Instance.barExtraRoot.SetActive(value: false);
			}
		}
	}

	public override void OnPageClosed()
	{
		base.OnPageClosed();
		if (_miniRigController != null)
		{
			_miniRigController.Hide();
		}
		if (_pointerObject != null)
		{
			TogglePointer(enable: false);
		}
	}

	private void ResetSelection()
	{
		if (_miniRigController != null)
		{
			_miniRigController.Hide();
		}
		_selectedPlayer = null;
		_selectedPlayerId = null;
		_isSelectingReport = false;
		_viewingMods = false;
		_viewingCheats = false;
		if (_selectionIndicator != null)
		{
			_selectionIndicator.SetActive(value: false);
		}
		UpdateLeaderboardUI();
	}

	private void UpdateLeaderboardUI()
	{
		UtilMenuController instance = UtilMenuController.Instance;
		if (NetworkSystem.Instance == null || !NetworkSystem.Instance.InRoom)
		{
			_leaderboardTab.Description = "YOU MUST BE IN A ROOM TO USE THIS.";
			_gunTab.Description = "YOU MUST BE IN A ROOM TO USE THIS.";
			if ((bool)instance.playerSelectRoot)
			{
				instance.playerSelectRoot.SetActive(value: false);
			}
			if ((bool)instance.barExtraRoot)
			{
				instance.barExtraRoot.SetActive(value: false);
			}
			if ((bool)instance.panel2Root)
			{
				instance.panel2Root.SetActive(value: false);
			}
			_selectedPlayer = null;
			return;
		}
		_leaderboardTab.Description = null;
		_gunTab.Description = null;
		if (_selectedPlayer == null && _selectedPlayerId != null)
		{
			_selectedPlayer = PlayerTranslator.GetNetPlayerByUserID(_selectedPlayerId);
			if (_selectedPlayer == null)
			{
				_selectedPlayer = NetworkSystem.Instance.AllNetPlayers?.FirstOrDefault((NetPlayer p) => p.UserId == _selectedPlayerId);
			}
			if (_selectedPlayer == null)
			{
				_selectedPlayerId = null;
			}
		}
		if (_selectedPlayer != null && _selectedPlayerId != null)
		{
			NetPlayer[] allNetPlayers = NetworkSystem.Instance.AllNetPlayers;
			if (allNetPlayers == null || !allNetPlayers.Any((NetPlayer p) => p.UserId == _selectedPlayerId))
			{
				_selectedPlayer = null;
				_selectedPlayerId = null;
				_isSelectingReport = false;
				_viewingMods = false;
				_viewingCheats = false;
				if (_selectionIndicator != null)
				{
					_selectionIndicator.SetActive(value: false);
				}
			}
		}
		if (!instance.playerSelectRoot)
		{
			return;
		}
		instance.playerSelectRoot.SetActive(value: true);
		NetPlayer[] array = NetworkSystem.Instance.AllNetPlayers ?? Array.Empty<NetPlayer>();
		int num = array.Length;
		bool flag;
		if (!(flag = num > 10))
		{
			_playerListOffset = 0;
		}
		if (_playerListOffset >= num)
		{
			_playerListOffset = 0;
		}
		for (int num2 = 0; num2 < 10; num2++)
		{
			int num3 = _playerListOffset + num2;
			if (num3 >= num)
			{
				instance.playerButtons[num2].gameObject.SetActive(value: false);
				continue;
			}
			instance.playerButtons[num2].gameObject.SetActive(value: true);
			NetPlayer netPlayer = array[num3];
			string text = netPlayer.NickName;
			VRRig rigByNetPlayer = PlayerTranslator.GetRigByNetPlayer(netPlayer);
			string text2 = "";
			if (rigByNetPlayer != null)
			{
				List<string> detectedMods = GetDetectedMods(rigByNetPlayer);
				bool flag2 = detectedMods.Any((string d) => d.Contains("red"));
				bool flag3 = detectedMods.Count > 0;
				if (UtilMenuMain.Instance.specialVariant == 1 && GetSpecialCosmetics(rigByNetPlayer).Count > 0)
				{
					text2 = "<color=#89CFF0>";
				}
				else if (!flag2)
				{
					if (flag3)
					{
						text2 = "<color=green>";
					}
				}
				else
				{
					text2 = "<color=red>";
				}
			}
			if (!string.IsNullOrEmpty(text2))
			{
				text = text2 + text + "</color>";
			}
			instance.playerButtonTexts[num2].text = text;
			bool isOn = _selectedPlayer != null && netPlayer.UserId == _selectedPlayerId;
			if (instance.playerButtons[num2] != null)
			{
				instance.playerButtons[num2].isOn = isOn;
				instance.playerButtons[num2].UpdateColor();
			}
		}
		if ((bool)instance.barExtraRoot)
		{
			instance.barExtraRoot.SetActive(value: true);
			bool isOn2 = _pointerObject != null && _pointerObject.activeSelf;
			if ((bool)instance.gunSelectBtn)
			{
				instance.gunSelectBtn.gameObject.SetActive(!flag);
				instance.gunSelectBtn.isOn = isOn2;
				instance.gunSelectBtn.UpdateColor();
			}
			if ((bool)instance.gunSelectSmallBtn)
			{
				instance.gunSelectSmallBtn.gameObject.SetActive(flag);
				instance.gunSelectSmallBtn.isOn = isOn2;
				instance.gunSelectSmallBtn.UpdateColor();
			}
			if ((bool)instance.peSliderLeft)
			{
				instance.peSliderLeft.gameObject.SetActive(flag);
			}
			if ((bool)instance.peSliderRight)
			{
				instance.peSliderRight.gameObject.SetActive(flag);
			}
			if ((bool)instance.peSliderText)
			{
				instance.peSliderText.gameObject.SetActive(flag);
				if (flag)
				{
					instance.peSliderText.text = $"PAGE {_playerListOffset / 10 + 1}/{Mathf.CeilToInt((float)num / 10f)}";
				}
			}
		}
		if (_selectedPlayer != null && (bool)instance.panel2Root)
		{
			instance.panel2Root.SetActive(value: true);
			UpdatePanel2Info();
		}
		else if ((bool)instance.panel2Root)
		{
			instance.panel2Root.SetActive(value: false);
		}
	}

	private void UpdatePanel2Info()
	{
		UtilMenuController instance = UtilMenuController.Instance;
		if (_selectedPlayer == null)
		{
			if (_miniRigController != null)
			{
				_miniRigController.Hide();
			}
			return;
		}
		GameObject playerBackgroundObj = instance.playerBackgroundObj;
		if (_miniRigController != null && playerBackgroundObj != null)
		{
			if (!_selectedPlayer.IsLocal)
			{
				_miniRigController.SetTarget(_selectedPlayer, playerBackgroundObj.transform);
			}
			else
			{
				_miniRigController.Hide();
			}
		}
		bool flag = Players.IsMuted(_selectedPlayer.UserId);
		if ((bool)instance.volUnMuteBtn)
		{
			instance.volUnMuteBtn.isOn = !flag;
			instance.volUnMuteBtn.UpdateColor();
		}
		bool isOn = Voices.IsFocused(_selectedPlayer.UserId);
		if ((bool)instance.volPrioritizeBtn)
		{
			instance.volPrioritizeBtn.isOn = isOn;
			instance.volPrioritizeBtn.UpdateColor();
		}
		int num = 0;
		int num2 = 0;
		List<string> list = new List<string>();
		List<string> list2 = new List<string>();
		VRRig rigByNetPlayer = PlayerTranslator.GetRigByNetPlayer(_selectedPlayer);
		if (rigByNetPlayer != null)
		{
			List<string> detectedMods = GetDetectedMods(rigByNetPlayer);
			foreach (string item in detectedMods)
			{
				if (item.Contains("red"))
				{
					num++;
					list.Add(item);
				}
				else
				{
					num2++;
					list2.Add(item);
				}
			}
		}
		if ((bool)instance.modsBtnText)
		{
			instance.modsBtnText.text = $"{num2}\nMODS";
		}
		if ((bool)instance.cheatsBtnText)
		{
			instance.cheatsBtnText.text = $"{num}\nCHEATS";
		}
		if ((bool)instance.modsCheckBtn)
		{
			instance.modsCheckBtn.isOn = _viewingMods;
			instance.modsCheckBtn.UpdateColor();
		}
		if ((bool)instance.cheatsCheckBtn)
		{
			instance.cheatsCheckBtn.isOn = _viewingCheats;
			instance.cheatsCheckBtn.UpdateColor();
		}
		bool flag2 = _viewingMods || _viewingCheats;
		if ((bool)instance.volumeBarRoot)
		{
			instance.volumeBarRoot.SetActive(!flag2);
		}
		if ((bool)instance.reportBarRoot)
		{
			instance.reportBarRoot.SetActive(!flag2);
		}
		if ((bool)instance.cloneCosmeticsBtnObj)
		{
			instance.cloneCosmeticsBtnObj.SetActive(!flag2 && !_isSelectingReport);
		}
		if ((bool)instance.modAndCheatListText)
		{
			instance.modAndCheatListText.gameObject.SetActive(flag2);
			if (_viewingMods)
			{
				instance.modAndCheatListText.text = ((list2.Count > 0) ? string.Join("\n", list2) : "NONE");
			}
			else if (_viewingCheats)
			{
				instance.modAndCheatListText.text = ((list.Count > 0) ? string.Join("\n", list) : "NONE");
			}
		}
		string text = "UNKNOWN";
		if (!_playerJoinDates.TryGetValue(_selectedPlayer.UserId, out var value))
		{
			if (_joinDateFailed.Contains(_selectedPlayer.UserId))
			{
				text = "UNKNOWN";
			}
			else if (_joinDateInFlight.Contains(_selectedPlayer.UserId))
			{
				text = "FETCHING...";
			}
			else
			{
				_joinDateInFlight.Add(_selectedPlayer.UserId);
				UtilMenuMain.Instance.StartCoroutine(FetchJoinDate(_selectedPlayer.UserId));
				text = "FETCHING...";
			}
		}
		else
		{
			text = value.ToShortDateString();
		}
		int num3 = 0;
		try
		{
			FieldInfo field = typeof(VRRig).GetField("fps", BindingFlags.Instance | BindingFlags.NonPublic);
			if (field != null)
			{
				num3 = (int)field.GetValue(rigByNetPlayer);
			}
		}
		catch
		{
		}
		string arg = "?";
		bool value2;
		if (rigByNetPlayer.IsItemAllowed("S. FIRST LOGIN"))
		{
			arg = "STEAM";
		}
		else if (_playerJoinedAfterPaid.TryGetValue(_selectedPlayer.UserId, out value2))
		{
			arg = (value2 ? "OCULUS" : "?");
		}
		string text2 = ((rigByNetPlayer != null) ? $"{Mathf.RoundToInt(rigByNetPlayer.playerColor.r * 9f)} {Mathf.RoundToInt(rigByNetPlayer.playerColor.g * 9f)} {Mathf.RoundToInt(rigByNetPlayer.playerColor.b * 9f)}" : "- - -");
		string text3 = "";
		if (UtilMenuMain.Instance.specialVariant == 1 && rigByNetPlayer != null)
		{
			List<string> specialCosmetics = GetSpecialCosmetics(rigByNetPlayer);
			text3 = ((specialCosmetics.Count > 0) ? string.Join(", ", specialCosmetics) : "");
		}
		instance.userInfoText.text = _selectedPlayer.NickName.ToUpper() + "\n" + $"{arg} | {num3}FPS\n" + "COLOR " + text2 + "\n" + text3 + "\nJoined\n" + text;
		if ((bool)instance.volumeValueText)
		{
			if (!flag)
			{
				instance.volumeValueText.text = $"{Mathf.RoundToInt(Voices.GetVolume(_selectedPlayer.UserId) * 100f)}%";
			}
			else
			{
				instance.volumeValueText.text = "MUTED";
			}
		}
		if (flag2 || !instance.reportBarRoot)
		{
			return;
		}
		instance.reportBarRoot.SetActive(value: true);
		if (!_isSelectingReport)
		{
			instance.reportMainBtn.gameObject.SetActive(value: true);
			if ((bool)instance.cloneCosmeticsBtnObj)
			{
				instance.cloneCosmeticsBtnObj.SetActive(value: true);
			}
			instance.reportSendBtn.gameObject.SetActive(value: false);
			instance.reportSliderLeft.gameObject.SetActive(value: false);
			instance.reportSliderRight.gameObject.SetActive(value: false);
			instance.reportSliderText.gameObject.SetActive(value: false);
			if ((bool)instance.reportValueText)
			{
				instance.reportValueText.gameObject.SetActive(value: false);
			}
			return;
		}
		instance.reportMainBtn.gameObject.SetActive(value: false);
		if ((bool)instance.cloneCosmeticsBtnObj)
		{
			instance.cloneCosmeticsBtnObj.SetActive(value: false);
		}
		instance.reportSendBtn.gameObject.SetActive(value: true);
		instance.reportSliderLeft.gameObject.SetActive(value: true);
		instance.reportSliderRight.gameObject.SetActive(value: true);
		instance.reportSliderText.gameObject.SetActive(value: true);
		if ((bool)instance.reportSliderText)
		{
			instance.reportSliderText.text = "REPORT:";
		}
		if ((bool)instance.reportValueText)
		{
			instance.reportValueText.gameObject.SetActive(value: true);
			instance.reportValueText.text = _reportTypes[_reportReasonIndex].ToString().ToUpper();
		}
	}

	public void OnPlayerButtonPressed(int buttonIndex)
	{
		NetPlayer[] allNetPlayers = NetworkSystem.Instance.AllNetPlayers;
		int num = _playerListOffset + buttonIndex;
		if (num < allNetPlayers.Length)
		{
			NetPlayer selectedPlayer = allNetPlayers[num];
			_selectedPlayer = selectedPlayer;
			_selectedPlayerId = _selectedPlayer.UserId;
			_isSelectingReport = false;
			_viewingMods = false;
			_viewingCheats = false;
			UpdateLeaderboardUI();
		}
	}

	public void OnGunSelectBtn()
	{
		bool enable;
		if (!(enable = !(_pointerObject != null) || !_pointerObject.activeSelf) || (!(NetworkSystem.Instance == null) && NetworkSystem.Instance.InRoom))
		{
			TogglePointer(enable);
			UpdateLeaderboardUI();
		}
	}

	private IEnumerator DeferredGunTabSwitch()
	{
		yield return null;
		UtilMenuController.Instance.OnTabPress(2);
		if (NetworkSystem.Instance != null && NetworkSystem.Instance.InRoom)
		{
			TogglePointer(enable: true);
		}
		UpdateLeaderboardUI();
	}

	public void OnPlayerPageLeft()
	{
		NetPlayer[] allNetPlayers = NetworkSystem.Instance.AllNetPlayers;
		if (allNetPlayers.Length > 10)
		{
			_playerListOffset -= 10;
			if (_playerListOffset < 0)
			{
				_playerListOffset = Mathf.CeilToInt((float)allNetPlayers.Length / 10f) * 10 - 10;
			}
			if (_playerListOffset < 0)
			{
				_playerListOffset = 0;
			}
			UpdateLeaderboardUI();
		}
	}

	public void OnPlayerPageRight()
	{
		NetPlayer[] allNetPlayers = NetworkSystem.Instance.AllNetPlayers;
		if (allNetPlayers.Length > 10)
		{
			_playerListOffset += 10;
			if (_playerListOffset >= allNetPlayers.Length)
			{
				_playerListOffset = 0;
			}
			UpdateLeaderboardUI();
		}
	}

	public void OnVolumeChange(bool up)
	{
		if (_selectedPlayer != null)
		{
			Voices.StepVolume(_selectedPlayer.UserId, up);
			UpdateLeaderboardUI();
		}
	}

	public void OnPrioritizeBtn()
	{
		if (_selectedPlayer != null)
		{
			Voices.ToggleFocus(_selectedPlayer.UserId);
			UpdateLeaderboardUI();
		}
	}

	public void OnMuteToggle()
	{
		if (_selectedPlayer != null && Players.ToggleMute(_selectedPlayer.UserId))
		{
			UpdateLeaderboardUI();
		}
	}

	public void OnMainReportBtn()
	{
		_isSelectingReport = true;
		UpdateLeaderboardUI();
	}

	public void OnSendReport()
	{
		PerformReport(_reportTypes[_reportReasonIndex]);
		_isSelectingReport = false;
		UpdateLeaderboardUI();
	}

	public void OnReportSlider(bool left)
	{
		if (left)
		{
			_reportReasonIndex--;
		}
		else
		{
			_reportReasonIndex++;
		}
		if (_reportReasonIndex < 0)
		{
			_reportReasonIndex = _reportTypes.Length - 1;
		}
		if (_reportReasonIndex >= _reportTypes.Length)
		{
			_reportReasonIndex = 0;
		}
		UpdateLeaderboardUI();
	}

	public void OnCloneCosmetics()
	{
		if (_selectedPlayer == null || CosmeticsController.instance == null)
		{
			return;
		}
		VRRig rigByNetPlayer = PlayerTranslator.GetRigByNetPlayer(_selectedPlayer);
		if (rigByNetPlayer == null)
		{
			return;
		}
		CosmeticsController.instance.currentWornSet.ClearSet(CosmeticsController.instance.nullItem);
		CosmeticsController.CosmeticItem[] items = rigByNetPlayer.cosmeticSet.items;
		CosmeticsController.CosmeticItem[] items2 = CosmeticsController.instance.currentWornSet.items;
		for (int i = 0; i < items.Length; i++)
		{
			if (!items[i].isNullItem)
			{
				CosmeticsController.CosmeticItem localItem = CosmeticsController.instance.GetItemFromDict(items[i].itemName);
				bool flag = CosmeticsController.instance.unlockedCosmetics.Any((CosmeticsController.CosmeticItem x) => x.itemName == localItem.itemName);
				if ((!localItem.isNullItem & flag) && i < items2.Length)
				{
					items2[i] = localItem;
				}
			}
		}
		CosmeticsHelper.ApplyAndRefresh();
	}

	public void OnCheckMods()
	{
		if (_selectedPlayer != null)
		{
			if (!_viewingMods)
			{
				_viewingMods = true;
				_viewingCheats = false;
			}
			else
			{
				_viewingMods = false;
			}
			UpdateLeaderboardUI();
		}
	}

	public void OnCheckCheats()
	{
		if (_selectedPlayer != null)
		{
			if (_viewingCheats)
			{
				_viewingCheats = false;
			}
			else
			{
				_viewingCheats = true;
				_viewingMods = false;
			}
			UpdateLeaderboardUI();
		}
	}

	private void RefreshLobbyTab()
	{
		if (_lobbyTab == null)
		{
			return;
		}
		_lobbyTab.Elements.Clear();
		List<string> modes = new List<string>();
		try
		{
			if (_cachedSelectorParent == null)
			{
				_cachedSelectorParent = GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/TreeRoomInteractables/UI/GameModeSelector/Selector Buttons/BtnLayouts/ENABLE FOR BETA");
			}
			if (_cachedSelectorParent != null)
			{
				modes.AddRange(from Transform child in _cachedSelectorParent.transform
					select child.GetComponent<ModeSelectButton>() into btn
					where btn != null && !string.IsNullOrEmpty(btn.gameMode)
					select btn.gameMode);
			}
		}
		catch
		{
		}
		modes = modes.Distinct().ToList();
		List<string> queues = new List<string> { "DEFAULT", "MINIGAMES", "COMPETITIVE" };
		string currentQueue = GorillaComputer.instance.currentQueue ?? "DEFAULT";
		_lobbyTab.Elements.Add(new MenuElement("QUEUE: " + currentQueue, "", delegate
		{
			int num = queues.IndexOf(currentQueue);
			if (num == -1)
			{
				num = 0;
			}
			num--;
			if (num < 0)
			{
				num = queues.Count - 1;
			}
			UpdateQueue(queues[num]);
		}, delegate
		{
			int num = queues.IndexOf(currentQueue);
			if (num == -1)
			{
				num = 0;
			}
			num++;
			if (num >= queues.Count)
			{
				num = 0;
			}
			UpdateQueue(queues[num]);
		})
		{
			Type = ElementType.Slider
		});
		string currentMode = GorillaComputer.instance.currentGameMode.Value ?? "INFECTION";
		_lobbyTab.Elements.Add(new MenuElement("MODE: " + currentMode.ToUpper(), "", delegate
		{
			if (modes.Count != 0)
			{
				int num = modes.FindIndex((string m) => m.Equals(currentMode, StringComparison.OrdinalIgnoreCase));
				if (num == -1)
				{
					num = 0;
				}
				num--;
				if (num < 0)
				{
					num = modes.Count - 1;
				}
				GorillaComputer.instance.OnModeSelectButtonPress(modes[num], GorillaComputer.instance.leftHanded);
				RefreshLobbyTab();
				UtilMenuController.Instance.RefreshUI();
			}
		}, delegate
		{
			if (modes.Count != 0)
			{
				int num = modes.FindIndex((string m) => m.Equals(currentMode, StringComparison.OrdinalIgnoreCase));
				if (num == -1)
				{
					num = 0;
				}
				num++;
				if (num >= modes.Count)
				{
					num = 0;
				}
				GorillaComputer.instance.OnModeSelectButtonPress(modes[num], GorillaComputer.instance.leftHanded);
				RefreshLobbyTab();
				UtilMenuController.Instance.RefreshUI();
			}
		})
		{
			Type = ElementType.Slider
		});
		_lobbyHopElement = new MenuElement("LOBBY HOP", delegate
		{
			_roomCodeInput = "RANDOM ROOM";
			GorillaNetworkJoinTrigger currentJoinTrigger = PhotonNetworkController.Instance.currentJoinTrigger;
			if (currentJoinTrigger != null)
			{
				UtilMenuController.Instance.StartCoroutine(ProcessLobbyHop(currentJoinTrigger));
			}
		});
		_lobbyTab.Elements.Add(_lobbyHopElement);
		_lobbyTab.Elements.Add(new MenuElement("JOIN ROOM", delegate
		{
			if (!NetworkSystem.Instance.InRoom)
			{
				_isEnteringCode = true;
				_roomCodeInput = "";
				UtilMenuController.Instance.RefreshUI();
				KeyboardController.Instance.ClearInput();
				KeyboardController.Instance.OnKeyPressed = delegate(string key)
				{
					_roomCodeInput = key.ToUpper();
				};
				KeyboardController.Instance.OnEnterPressed = delegate
				{
					_isEnteringCode = false;
					UtilMenuController.Instance.RefreshUI();
					if (GorillaComputer.instance.CheckAutoBanListForName(_roomCodeInput))
					{
						if (!string.IsNullOrEmpty(_roomCodeInput))
						{
							PhotonNetworkController.Instance.AttemptToJoinSpecificRoom(_roomCodeInput, JoinType.Solo);
						}
						KeyboardController.Instance.CloseKeyboard();
					}
				};
				KeyboardController.Instance.OpenKeyboard();
			}
			else
			{
				NetworkSystem.Instance.ReturnToSinglePlayer();
				_roomCodeInput = "";
				_isEnteringCode = false;
				UtilMenuController.Instance.RefreshUI();
			}
		}, ElementType.Button));
	}

	private void UpdateQueue(string next)
	{
		GorillaComputer instance = GorillaComputer.instance;
		if (!(instance == null))
		{
			if (next == "COMPETITIVE" && !instance.allowedInCompetitive)
			{
				instance.CompQueueUnlockButtonPress();
			}
			if (!(_joinQueueMethod != null))
			{
				instance.currentQueue = next;
				BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
				typeof(GorillaComputer).GetField("troopQueueActive", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.SetValue(instance, false);
				typeof(GorillaComputer).GetField("currentTroopPopulation", bindingAttr)?.SetValue(instance, -1);
				PlayerPrefs.SetString("currentQueue", next);
				PlayerPrefs.SetInt("troopQueueActive", 0);
				PlayerPrefs.Save();
				UnityEngine.Debug.LogWarning("[LobbyPage] GorillaComputer.JoinQueue not found - used fallback queue switch.");
			}
			else
			{
				_joinQueueMethod.Invoke(instance, new object[2] { next, false });
			}
			RefreshLobbyTab();
			UtilMenuController.Instance.RefreshUI();
		}
	}

	private void RefreshFriendList()
	{
		_friendsTab.Elements.Clear();
		_friendsTab.Elements.Add(new MenuElement("FETCHING FRIENDS...", delegate
		{
		}));
		UtilMenuController.Instance.RefreshUI();
		FriendSystem.Instance.OnFriendListRefresh -= OnFriendListReceived;
		FriendSystem.Instance.OnFriendListRefresh += OnFriendListReceived;
		FriendSystem.Instance.RefreshFriendsList();
	}

	private void OnFriendListReceived(List<FriendBackendController.Friend> friends)
	{
		FriendSystem.Instance.OnFriendListRefresh -= OnFriendListReceived;
		_friendsTab.Elements.Clear();
		if (friends != null && friends.Count != 0)
		{
			foreach (FriendBackendController.Friend friend in friends)
			{
				FriendNameCache.Remember(friend.Presence?.FriendLinkId, friend.Presence?.UserName);
			}
			List<FriendBackendController.Friend> list = friends.Where(delegate(FriendBackendController.Friend friend)
			{
				string text2 = friend.Presence?.UserName;
				return (!string.IsNullOrEmpty(text2) && text2 != "Unknown") || FriendNameCache.HasCachedName(friend.Presence?.FriendLinkId);
			}).ToList();
			list.Sort((FriendBackendController.Friend a, FriendBackendController.Friend b) => (b.Presence != null && !string.IsNullOrEmpty(b.Presence.RoomId)).CompareTo(a.Presence != null && !string.IsNullOrEmpty(a.Presence.RoomId)));
			_friendsTab.Elements.Add(new MenuElement("REFRESH LIST", RefreshFriendList));
			int totalPages = Mathf.CeilToInt((float)list.Count / 4f);
			if (_friendPageIndex >= totalPages)
			{
				_friendPageIndex = 0;
			}
			int num = _friendPageIndex * 4;
			int num2 = Mathf.Min(4, list.Count - num);
			for (int num3 = 0; num3 < num2; num3++)
			{
				FriendBackendController.Friend f = list[num + num3];
				string text = FriendNameCache.Resolve(f.Presence?.FriendLinkId, f.Presence?.UserName);
				string roomId = f.Presence?.RoomId;
				bool joinable = !string.IsNullOrEmpty(roomId) && roomId != "OFFLINE";
				_friendsTab.Elements.Add(new MenuElement(text + " " + (joinable ? "[JOIN]" : "[OFFLINE]"), delegate
				{
					if (joinable)
					{
						PhotonNetworkController.Instance.AttemptToJoinSpecificRoom(roomId, (f.Presence?.IsPublic ?? true) ? JoinType.FriendStationPublic : JoinType.FriendStationPrivate);
					}
				}));
			}
			if (totalPages > 1)
			{
				_friendsTab.Elements.Add(new MenuElement($"PAGE {_friendPageIndex + 1}/{totalPages}", delegate
				{
					_friendPageIndex++;
					if (_friendPageIndex >= totalPages)
					{
						_friendPageIndex = 0;
					}
					OnFriendListReceived(friends);
				}));
			}
			UtilMenuController.Instance.RefreshUI();
		}
		else
		{
			_friendsTab.Elements.Add(new MenuElement("NO FRIENDS FOUND", RefreshFriendList));
			UtilMenuController.Instance.RefreshUI();
		}
	}

	private IEnumerator FetchJoinDate(string userId)
	{
		bool finished = false;
		PlayFabClientAPI.GetAccountInfo(new GetAccountInfoRequest
		{
			PlayFabId = userId
		}, delegate(GetAccountInfoResult res)
		{
			if (res?.AccountInfo != null)
			{
				_playerJoinDates[userId] = res.AccountInfo.Created;
				_playerJoinedAfterPaid[userId] = res.AccountInfo.Created > DateTime.Parse("12/13/2022");
			}
			_joinDateInFlight.Remove(userId);
			finished = true;
		}, delegate
		{
			_joinDateInFlight.Remove(userId);
			_joinDateFailed.Add(userId);
			finished = true;
		});
		yield return new WaitUntil(() => finished);
		UpdateLeaderboardUI();
	}

	private void PerformReport(GorillaPlayerLineButton.ButtonType reportType)
	{
		if (_selectedPlayer == null)
		{
			return;
		}
		List<GorillaPlayerScoreboardLine> list = GorillaScoreboardTotalUpdater.allScoreboardLines.Where((GorillaPlayerScoreboardLine l) => l != null && l.linePlayer != null && l.linePlayer.UserId == _selectedPlayer.UserId).ToList();
		if (list.Count == 0)
		{
			GorillaPlayerScoreboardLine.ReportPlayer(_selectedPlayer.UserId, reportType, _selectedPlayer.NickName);
			return;
		}
		list[0].SetReportState(reportState: false, reportType);
		for (int num = 1; num < list.Count; num++)
		{
			GorillaPlayerScoreboardLine gorillaPlayerScoreboardLine = list[num];
			gorillaPlayerScoreboardLine.reportedCheating |= reportType == GorillaPlayerLineButton.ButtonType.Cheating;
			gorillaPlayerScoreboardLine.reportedToxicity |= reportType == GorillaPlayerLineButton.ButtonType.Toxicity;
			gorillaPlayerScoreboardLine.reportedHateSpeech |= reportType == GorillaPlayerLineButton.ButtonType.HateSpeech;
			if (gorillaPlayerScoreboardLine.reportButton != null)
			{
				gorillaPlayerScoreboardLine.reportButton.isOn = true;
				gorillaPlayerScoreboardLine.reportButton.UpdateColor();
			}
		}
	}

	private void TogglePointer(bool enable)
	{
		if (enable)
		{
			if (_pointerObject == null)
			{
				_pointerObject = new GameObject("SakUtilPointer");
				PlayerSelectGun playerSelectGun = _pointerObject.AddComponent<PlayerSelectGun>();
				playerSelectGun.Callback = delegate(VRRig rig)
				{
					NetPlayer netPlayerByRig = PlayerTranslator.GetNetPlayerByRig(rig);
					if (netPlayerByRig != null)
					{
						_selectedPlayer = netPlayerByRig;
						_selectedPlayerId = netPlayerByRig.UserId;
						_isSelectingReport = false;
						if (UtilMenuController.Instance.CurrentPageIndex != 0)
						{
							UtilMenuController.Instance.SwitchPage(0);
						}
						if (UtilMenuController.Instance.CurrentTabIndex != 1)
						{
							UtilMenuController.Instance.OnTabPress(1);
						}
						UpdateLeaderboardUI();
					}
				};
			}
			_pointerObject.SetActive(value: true);
		}
		else
		{
			if (_pointerObject != null)
			{
				UnityEngine.Object.Destroy(_pointerObject);
			}
			_pointerObject = null;
		}
	}

	private void CreateSelectionIndicator()
	{
		_selectionIndicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		UnityEngine.Object.Destroy(_selectionIndicator.GetComponent<Collider>());
		_selectionIndicator.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
		Renderer component = _selectionIndicator.GetComponent<Renderer>();
		Material material = new Material(Shader.Find("GorillaTag/UberShader"));
		material = ExtraTools.MakeMaterialTransparent(material);
		material.color = new Color(1f, 0.41f, 0.71f, 0.7f);
		component.material = material;
	}

	private IEnumerator FriendMonitorRoutine()
	{
		yield return new WaitForSeconds(5f);
		while (true)
		{
			if (NotificationsPage.FriendNotificationsEnabled)
			{
				FriendSystem.Instance.OnFriendListRefresh -= OnBackgroundFriendListReceived;
				FriendSystem.Instance.OnFriendListRefresh += OnBackgroundFriendListReceived;
				FriendSystem.Instance.RefreshFriendsList();
			}
			yield return new WaitForSeconds(60f);
		}
	}

	private void OnBackgroundFriendListReceived(List<FriendBackendController.Friend> friends)
	{
		FriendSystem.Instance.OnFriendListRefresh -= OnBackgroundFriendListReceived;
		HashSet<string> hashSet = new HashSet<string>();
		if (friends != null)
		{
			foreach (FriendBackendController.Friend friend in friends)
			{
				FriendNameCache.Remember(friend.Presence?.FriendLinkId, friend.Presence?.UserName);
				if (friend.Presence != null && !string.IsNullOrEmpty(friend.Presence.UserName) && friend.Presence.UserName != "Unknown" && !string.IsNullOrEmpty(friend.Presence.RoomId) && friend.Presence.RoomId != "OFFLINE")
				{
					hashSet.Add(friend.Presence.UserName);
				}
			}
		}
		if (_hasDoneInitialFriendCheck && NotificationsPage.FriendNotificationsEnabled)
		{
			foreach (string item in hashSet.Where((string friend) => !_lastOnlineFriends.Contains(friend)))
			{
				Notification.Send("<color=green>[FRIEND]</color> " + item + " IS ONLINE");
			}
		}
		_lastOnlineFriends = hashSet;
		_hasDoneInitialFriendCheck = true;
	}

	private IEnumerator ProcessLobbyHop(GorillaNetworkJoinTrigger trigger)
	{
		if (_lobbyHopInFlight)
		{
			UnityEngine.Debug.LogWarning("[LobbyHop] ignored - a hop is already in flight.");
			yield break;
		}
		_lobbyHopInFlight = true;
		try
		{
			if (!(FriendNetworkController.Instance != null))
			{
				NetworkSystem.Instance.ReturnToSinglePlayer();
				yield return new WaitForSeconds(2f);
				yield return new WaitUntil(() => !NetworkSystem.Instance.InRoom);
			}
			else
			{
				yield return FriendNetworkController.Instance.SafeLeaveAndWait();
				if (!FriendNetworkController.Instance.LastReJoinReachedIdle)
				{
					UnityEngine.Debug.LogWarning("[LobbyHop] aborted, leave didn't settle cleanly; not shipping a possibly-stale ticket. Try again in a moment.");
					yield return FlashLobbyHop("TRY AGAIN");
					yield break;
				}
			}
			float hopStart = Time.realtimeSinceStartup;
			JoinForensics.NoteModJoin("<lobby-hop>", raw: false, "ProcessLobbyHop");
			trigger.OnBoxTriggered();
			float startWait = Time.realtimeSinceStartup;
			yield return new WaitUntil(() => NetworkSystem.Instance == null || NetworkSystem.Instance.netState != NetSystemState.Idle || Time.realtimeSinceStartup - startWait > 3f);
			float deadline = Time.realtimeSinceStartup + 20f;
			yield return new WaitUntil(() => NetworkSystem.Instance == null || NetworkSystem.Instance.netState == NetSystemState.InGame || NetworkSystem.Instance.netState == NetSystemState.Idle || Time.realtimeSinceStartup > deadline);
			if (!(NetworkSystem.Instance != null) || NetworkSystem.Instance.netState != NetSystemState.InGame)
			{
				string reason = ((FriendNetworkController.LastJoinFailureTime >= hopStart) ? FriendNetworkController.FriendlyJoinFailure(FriendNetworkController.LastJoinFailureCode) : "JOIN FAILED!");
				yield return FlashLobbyHop(reason);
			}
		}
		finally
		{
			_lobbyHopInFlight = false;
			if (_roomCodeInput == "RANDOM ROOM")
			{
				_roomCodeInput = "";
				if (UtilMenuController.Instance != null)
				{
					UtilMenuController.Instance.RefreshUI();
				}
			}
		}
	}

	private static IEnumerator FlashLobbyHop(string message)
	{
		MenuElement el = _lobbyHopElement;
		if (el != null)
		{
			el.Text = message;
			if (UtilMenuController.Instance != null)
			{
				UtilMenuController.Instance.RefreshUI();
			}
			yield return new WaitForSeconds(2.5f);
			el.Text = "LOBBY HOP";
			if (UtilMenuController.Instance != null)
			{
				UtilMenuController.Instance.RefreshUI();
			}
		}
	}

	public static IEnumerator FetchKnownMods()
	{
		yield break;
	}

	private static IEnumerator FetchKnownCheats()
	{
		yield break;
	}

	private IEnumerator CosmeticSuspicionRoutine()
	{
		while (true)
		{
			yield return new WaitForSeconds(0.5f);
			if (NetworkSystem.Instance == null || NetworkSystem.Instance.AllNetPlayers == null)
			{
				continue;
			}
			NetPlayer[] allNetPlayers = NetworkSystem.Instance.AllNetPlayers;
			foreach (NetPlayer player in allNetPlayers)
			{
				if (player?.IsLocal ?? true)
				{
					continue;
				}
				VRRig rig = PlayerTranslator.GetRigByNetPlayer(player);
				if (rig == null)
				{
					continue;
				}
				Vector3 pos = rig.transform.position;
				if (pos.y > 19.79f && pos.y < 27.8f && pos.z > -157.8f && pos.z < -113.45f && pos.x > -83.7203f && pos.x < -45.519f)
				{
					if (_cosmeticSuspicionTimes.ContainsKey(player.UserId))
					{
						_cosmeticSuspicionTimes.Remove(player.UserId);
					}
					continue;
				}
				bool hasUnownedCosmetic = false;
				try
				{
					CosmeticsController.CosmeticSet cosmeticSet = rig.cosmeticSet;
					if (cosmeticSet != null && cosmeticSet.items != null)
					{
						CosmeticsController.CosmeticItem[] items = rig.cosmeticSet.items;
						for (int j = 0; j < items.Length; j++)
						{
							CosmeticsController.CosmeticItem item = items[j];
							if (!item.isNullItem && !rig.IsItemAllowed(item.itemName) && (rig.tryOnSet == null || rig.tryOnSet.items == null || !rig.tryOnSet.items.Any((CosmeticsController.CosmeticItem t) => !t.isNullItem && t.itemName == item.itemName)))
							{
								hasUnownedCosmetic = true;
								break;
							}
						}
					}
				}
				catch
				{
				}
				if (hasUnownedCosmetic)
				{
					if (!_cosmeticSuspicionTimes.ContainsKey(player.UserId))
					{
						_cosmeticSuspicionTimes[player.UserId] = Time.time;
					}
				}
				else if (_cosmeticSuspicionTimes.ContainsKey(player.UserId))
				{
					_cosmeticSuspicionTimes.Remove(player.UserId);
				}
			}
		}
	}

	public static List<string> GetSpecialCosmetics(VRRig rig)
	{
		List<string> list = new List<string>();
		if (rig == null)
		{
			return list;
		}
		try
		{
			if (rig.cosmeticSet.items == null)
			{
				return list;
			}
			CosmeticsController.CosmeticItem[] items = rig.cosmeticSet.items;
			for (int i = 0; i < items.Length; i++)
			{
				CosmeticsController.CosmeticItem cosmeticItem = items[i];
				if (cosmeticItem.isNullItem)
				{
					continue;
				}
				switch (cosmeticItem.itemName)
				{
				case "LBAAD.":
					if (rig.IsItemAllowed("LBAAD."))
					{
						list.Add("ADMINISTRATOR");
					}
					break;
				case "LMAPY.":
					if (rig.IsItemAllowed("LMAPY."))
					{
						list.Add("FOREST GUIDE");
					}
					break;
				case "LBANI.":
					if (rig.IsItemAllowed("LBANI."))
					{
						list.Add("AA CREATOR");
					}
					break;
				case "LBAGS.":
					if (rig.IsItemAllowed("LBAGS."))
					{
						list.Add("ILLUSTRATOR");
					}
					break;
				case "LBADE.":
					if (rig.IsItemAllowed("LBADE."))
					{
						list.Add("FINGER PAINTER");
					}
					break;
				case "LBAAK.":
					if (rig.IsItemAllowed("LBAAK."))
					{
						list.Add("DEV STICK");
					}
					break;
				}
			}
		}
		catch
		{
		}
		return list;
	}

	public static List<string> GetDetectedMods(VRRig rig)
	{
		List<string> list = new List<string>();
		NetPlayer creator = rig.Creator;
		if (creator != null)
		{
			string userId = creator.UserId;
			if (_cosmeticSuspicionTimes.TryGetValue(userId, out var value) && Time.time - value > 5f)
			{
				list.Add("<color=red>COSMETX</color>");
			}
			Dictionary<string, object> customProps = new Dictionary<string, object>();
			try
			{
				foreach (DictionaryEntry customProperty in creator.GetPlayerRef().CustomProperties)
				{
					customProps[customProperty.Key.ToString().ToLower()] = customProperty.Value;
				}
			}
			catch
			{
				return list;
			}
			HashSet<string> processedKeys = new HashSet<string>();
			int num = 0;
			int num2 = 0;
			foreach (KeyValuePair<string, string> item in _hardcodedCheats.Where((KeyValuePair<string, string> kvp) => customProps.ContainsKey(kvp.Key.ToLower())))
			{
				list.Add("<color=red>" + item.Value.ToUpper() + "</color>");
				processedKeys.Add(item.Key.ToLower());
				num++;
			}
			foreach (KeyValuePair<string, string> item2 in _webKnownCheats.Where((KeyValuePair<string, string> kvp) => customProps.ContainsKey(kvp.Key.ToLower()) && !processedKeys.Contains(kvp.Key.ToLower())))
			{
				list.Add("<color=red>" + item2.Value.ToUpper() + "</color>");
				processedKeys.Add(item2.Key.ToLower());
				num++;
			}
			bool flag = false;
			foreach (string key in customProps.Keys)
			{
				if (key.Contains("gorillashirts"))
				{
					if (!flag)
					{
						list.Add("<color=green>GORILLA SHIRTS</color>");
						flag = true;
						num2++;
					}
					processedKeys.Add(key);
				}
			}
			bool flag2 = false;
			foreach (string key2 in customProps.Keys)
			{
				if (key2.Contains("juul"))
				{
					if (!flag2)
					{
						list.Add("<color=green>JUUL</color>");
						flag2 = true;
						num2++;
					}
					processedKeys.Add(key2);
				}
			}
			foreach (KeyValuePair<string, string> item3 in _hardcodedSafeMods.Where((KeyValuePair<string, string> kvp) => customProps.ContainsKey(kvp.Key.ToLower()) && !processedKeys.Contains(kvp.Key.ToLower())))
			{
				list.Add("<color=green>" + item3.Value.ToUpper() + "</color>");
				processedKeys.Add(item3.Key.ToLower());
				num2++;
			}
			foreach (KeyValuePair<string, string> item4 in _webKnownMods.Where((KeyValuePair<string, string> kvp) => customProps.ContainsKey(kvp.Key.ToLower()) && !processedKeys.Contains(kvp.Key.ToLower())))
			{
				list.Add("<color=green>" + item4.Value.ToUpper() + "</color>");
				processedKeys.Add(item4.Key.ToLower());
				num2++;
			}
			foreach (KeyValuePair<string, string> item5 in _hardcodedUnknownMods.Where((KeyValuePair<string, string> kvp) => customProps.ContainsKey(kvp.Key.ToLower()) && !processedKeys.Contains(kvp.Key.ToLower())))
			{
				list.Add("<color=yellow>" + item5.Value.ToUpper() + "</color>");
				processedKeys.Add(item5.Key.ToLower());
			}
			if (num >= 5 && num2 >= 10)
			{
				list.Clear();
				list.Add("<color=red>MOD SPOOFER</color>");
			}
			if (ConsoleDetector.IsConsoleUser(userId))
			{
				string label = ConsoleDetector.GetLabel(userId);
				if (!list.Any((string d) => d.Contains(label.Split(' ')[0])))
				{
					list.Add("<color=red>" + label + "</color>");
				}
			}
			return list;
		}
		return list;
	}
}
