using System;
using System.Collections;
using System.Text;
using BepInEx.Bootstrap;
using Photon.Pun;
using SakuraaCastingMod.Shared.Helpers;
using SakuraaCastingMod.Shared.Integrations;
using SakuraaCastingMod.Shared.Models;
using SakuraaCastingMod.VR.Interaction;
using SakuraaCastingMod.VR.UtilMenu.Pages;
using SakuraaCastingMod.VR.UtilMenu.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.XR;

namespace SakuraaCastingMod.VR.UtilMenu;

public class UtilMenuMain : MonoBehaviour
{
	public static UtilMenuMain Instance;

	public int specialVariant = 0;

	public GameObject menu;

	public Material buttonMat;

	public Material pressedButtonMat;

	public Material innerBtnMat;

	public Material selectedBtnMat;

	public Material panelMat;

	public IconCollection Icons = new IconCollection();

	private void Start()
	{
		Instance = this;
		StartCoroutine(InitWithDelay());
	}

	private IEnumerator InitWithDelay()
	{
		UnityEngine.Debug.Log("[Util Menu] Waiting 3 seconds before initializing...");
		yield return new WaitForSeconds(3f);
		_ = XRSettings.isDeviceActive;
		if (1 == 0)
		{
			UnityEngine.Debug.Log("[Util Menu] VR Not Detected. Aborting load of UtilMenu.");
			yield break;
		}
		if (Chainloader.PluginInfos.ContainsKey("com.sakuraa.gorillatag.sheesh-client"))
		{
			UnityEngine.Debug.LogError("[Util Menu] Incompatible mod detected (com.sakuraa.gorillatag.sheesh-client)! The Util Menu will be disabled.");
			if (menu != null)
			{
				menu.SetActive(value: false);
			}
			yield break;
		}
		UnityEngine.Debug.Log("[Util Menu] Loading Util Menu now!");
		specialVariant = 0;
		if (base.gameObject.GetComponent<FriendNetworkController>() == null)
		{
			base.gameObject.AddComponent<FriendNetworkController>();
		}
		InitMenu();
		if (UtilMenuController.Instance != null)
		{
			UtilMenuController.Instance.LoadPages();
			UtilMenuController.Instance.RefreshUI();
		}
		ThemeManager.Initialize();
		StartCoroutine(LobbyPage.FetchKnownMods());
		base.gameObject.AddComponent<SpotifyManager>();
		StartCoroutine(CheckAndApplyTheme());
		UnityEngine.Debug.Log("[Util Menu] Initialization Complete.");
	}

	private IEnumerator CheckAndApplyTheme()
	{
		UnityEngine.Debug.Log("[Util Menu] Waiting for Photon LocalPlayer ID...");
		while (PhotonNetwork.LocalPlayer == null || string.IsNullOrEmpty(PhotonNetwork.LocalPlayer.UserId))
		{
			yield return new WaitForSeconds(1f);
		}
		string id = PhotonNetwork.LocalPlayer.UserId;
		UnityEngine.Debug.Log("[Util Menu] ID Found: " + id);
		if (id == "D6971CA01F82A975" || id == "38C7AFBF5FC3014C" || id == "9AF07DD7384734AC")
		{
			UnityEngine.Debug.Log("[Util Menu] Special ID detected! Enabling theme.");
			specialVariant = 1;
			JoinForensics.LoggingEnabled = true;
			UpdateThemeVisuals();
		}
	}

	public void UpdateThemeVisuals()
	{
		if (menu == null)
		{
			return;
		}
		Transform transform = menu.transform.Find("panel1/screenButtons");
		if (transform != null)
		{
			Transform transform2 = transform.Find("MenuTitleText");
			Transform transform3 = transform.Find("CustomMenuTitleText");
			if (specialVariant == 1)
			{
				if ((bool)transform2)
				{
					transform2.gameObject.SetActive(value: false);
				}
				if ((bool)transform3)
				{
					transform3.gameObject.SetActive(value: true);
					if ((bool)UtilMenuController.Instance)
					{
						UtilMenuController.Instance.menuTitle = transform3.GetComponent<TextMeshPro>();
					}
				}
			}
			else
			{
				if ((bool)transform3)
				{
					transform3.gameObject.SetActive(value: false);
				}
				if ((bool)transform2)
				{
					transform2.gameObject.SetActive(value: true);
					if ((bool)UtilMenuController.Instance)
					{
						UtilMenuController.Instance.menuTitle = transform2.GetComponent<TextMeshPro>();
					}
				}
			}
		}
		if (UtilMenuController.Instance != null)
		{
			UtilMenuController.Instance.LoadPages();
			UtilMenuController.Instance.RefreshUI();
		}
	}

	private void Update()
	{
		if (!(KeyboardController.Instance == null))
		{
			if (Keyboard.current.f1Key.wasPressedThisFrame)
			{
				KeyboardController.Instance.OpenKeyboard();
			}
			if (Keyboard.current.f2Key.wasPressedThisFrame)
			{
				KeyboardController.Instance.CloseKeyboard();
			}
		}
	}

	public void InitMenu()
	{
		if (menu == null)
		{
			UnityEngine.Debug.LogError("[SakUtil] Menu object is null!");
			return;
		}
		if (UtilMenuController.Instance == null)
		{
			UnityEngine.Debug.LogError("[SakUtil] UtilMenuController.Instance is NULL! Ensure the component is added in Plugin.cs");
			return;
		}
		Transform transform = menu.transform.Find("panel1");
		Transform transform5;
		Transform transform16;
		UtilMenuController instance;
		object obj;
		if (!(transform == null))
		{
			Transform transform2 = transform.Find("pageButtons");
			if (transform2 != null)
			{
				UtilMenuController.Instance.bottomBtnList.Clear();
				UtilFingerButton utilFingerButton = SetButtonListener(transform2, "menuUp", applyMats: true, delegate
				{
					UtilMenuController.Instance.MenuBackButton();
				}, isNavigation: true);
				if ((bool)utilFingerButton)
				{
					UtilMenuController.Instance.menuBackBtnObj = utilFingerButton.gameObject;
					Transform transform3 = utilFingerButton.transform.Find("Icon");
					if ((bool)transform3 && (bool)Icons.Forward)
					{
						transform3.GetComponent<MeshRenderer>().material = Icons.Forward;
					}
				}
				UtilFingerButton utilFingerButton2 = SetButtonListener(transform2, "menuBtn1", applyMats: true, delegate
				{
					UtilMenuController.Instance.Menu1Button();
				}, isNavigation: true);
				if ((bool)utilFingerButton2)
				{
					UtilMenuController.Instance.bottomBtnList.Add(utilFingerButton2.gameObject);
				}
				UtilFingerButton utilFingerButton3 = SetButtonListener(transform2, "menuBtn2", applyMats: true, delegate
				{
					UtilMenuController.Instance.Menu2Button();
				}, isNavigation: true);
				if ((bool)utilFingerButton3)
				{
					UtilMenuController.Instance.bottomBtnList.Add(utilFingerButton3.gameObject);
				}
				UtilFingerButton utilFingerButton4 = SetButtonListener(transform2, "menuBtn3", applyMats: true, delegate
				{
					UtilMenuController.Instance.Menu3Button();
				}, isNavigation: true);
				if ((bool)utilFingerButton4)
				{
					UtilMenuController.Instance.bottomBtnList.Add(utilFingerButton4.gameObject);
				}
				UtilFingerButton utilFingerButton5 = SetButtonListener(transform2, "menuBtn4", applyMats: true, delegate
				{
					UtilMenuController.Instance.Menu4Button();
				}, isNavigation: true);
				if ((bool)utilFingerButton5)
				{
					UtilMenuController.Instance.bottomBtnList.Add(utilFingerButton5.gameObject);
				}
				UtilFingerButton utilFingerButton6 = SetButtonListener(transform2, "menuBtn5", applyMats: true, delegate
				{
					UtilMenuController.Instance.Menu5Button();
				}, isNavigation: true);
				if ((bool)utilFingerButton6)
				{
					UtilMenuController.Instance.bottomBtnList.Add(utilFingerButton6.gameObject);
				}
				UtilFingerButton utilFingerButton7 = SetButtonListener(transform2, "menuDown", applyMats: true, delegate
				{
					UtilMenuController.Instance.MenuForwardButtton();
				}, isNavigation: true);
				if ((bool)utilFingerButton7)
				{
					UtilMenuController.Instance.menuForwardBtnObj = utilFingerButton7.gameObject;
					Transform transform4 = utilFingerButton7.transform.Find("Icon");
					if ((bool)transform4 && (bool)Icons.Backward)
					{
						transform4.GetComponent<MeshRenderer>().material = Icons.Backward;
					}
				}
			}
			transform5 = transform.Find("screenButtons");
			if (transform5 != null)
			{
				Transform transform6;
				if (specialVariant == 0)
				{
					transform6 = transform5.Find("MenuTitleText");
					if ((bool)transform5.Find("CustomMenuTitleText"))
					{
						transform5.Find("CustomMenuTitleText").gameObject.SetActive(value: false);
					}
				}
				else
				{
					transform6 = transform5.Find("CustomMenuTitleText");
					if ((bool)transform5.Find("MenuTitleText"))
					{
						transform5.Find("MenuTitleText").gameObject.SetActive(value: false);
					}
				}
				if (transform6 != null)
				{
					transform6.gameObject.SetActive(value: true);
					UtilMenuController.Instance.menuTitle = transform6.GetComponent<TextMeshPro>();
				}
				Transform transform7 = transform5.Find("DescriptionText");
				UtilMenuController.Instance.descriptionText = transform7.GetComponent<TextMeshPro>();
				UtilMenuController.Instance.BarList.Clear();
				for (int num = 1; num <= 6; num++)
				{
					int index = num;
					string n = "bar" + num;
					Transform transform8 = transform5.Find(n);
					if (!(transform8 != null))
					{
						continue;
					}
					UtilBarModel utilBarModel = new UtilBarModel
					{
						BarTransform = transform8,
						ScreenButton = transform8.Find("screenBtn"),
						SliderLeftButton = transform8.Find("sliderBtnLeft"),
						SliderRightButton = transform8.Find("sliderBtnRight")
					};
					if (utilBarModel.ScreenButton != null)
					{
						Transform transform9 = utilBarModel.ScreenButton.Find("ButtonText");
						if (transform9 != null)
						{
							utilBarModel.ButtonTextTransform = transform9;
							utilBarModel.ButtonText = transform9.GetComponent<TextMeshPro>();
						}
					}
					Transform transform10 = transform8.Find("SliderText");
					if (transform10 != null)
					{
						utilBarModel.SliderTextTransform = transform10;
						utilBarModel.SliderText = transform10.GetComponent<TextMeshPro>();
						Transform transform11 = transform10.Find("ValueText");
						if (transform11 != null)
						{
							utilBarModel.ValueText = transform11.GetComponent<TextMeshPro>();
						}
					}
					Transform transform12 = transform8.Find("TextInputBorder");
					if (transform12 != null)
					{
						utilBarModel.TextInputBorder = transform12;
						Transform transform13 = transform12.Find("InputText");
						if (transform13 != null)
						{
							utilBarModel.InputText = transform13.GetComponent<TextMeshPro>();
						}
					}
					UtilMenuController.Instance.BarList.Add(utilBarModel);
					if ((bool)utilBarModel.ScreenButton)
					{
						SetButtonListener(transform8, "screenBtn", applyMats: false, delegate
						{
							if ((bool)UtilMenuController.Instance)
							{
								UtilMenuController.Instance.OnScreenButtonPress(index);
							}
						});
					}
					if ((bool)utilBarModel.SliderLeftButton)
					{
						SetButtonListener(transform8, "sliderBtnLeft", applyMats: false, delegate
						{
							if ((bool)UtilMenuController.Instance)
							{
								UtilMenuController.Instance.OnSliderLeftPress(index);
							}
						});
					}
					if (!utilBarModel.SliderRightButton)
					{
						continue;
					}
					SetButtonListener(transform8, "sliderBtnRight", applyMats: false, delegate
					{
						if ((bool)UtilMenuController.Instance)
						{
							UtilMenuController.Instance.OnSliderRightPress(index);
						}
					});
				}
				Transform transform14 = transform5.Find("PlayerSelect");
				if (transform14 != null)
				{
					UtilMenuController.Instance.playerSelectRoot = transform14.gameObject;
					UtilMenuController.Instance.playerButtons.Clear();
					UtilMenuController.Instance.playerButtonTexts.Clear();
					for (int num2 = 1; num2 <= 10; num2++)
					{
						int pIndex = num2;
						UtilFingerButton utilFingerButton8 = SetButtonListener(transform14, $"playerBtn ({num2})", applyMats: true, delegate
						{
							if (UtilMenuController.Instance.CurrentPage is LobbyPage lobbyPage)
							{
								lobbyPage.OnPlayerButtonPressed(pIndex - 1);
							}
							else if (UtilMenuController.Instance.CurrentPage is SoundboardPage soundboardPage)
							{
								soundboardPage.OnTilePressed(pIndex - 1);
							}
						});
						if (utilFingerButton8 != null)
						{
							UtilMenuController.Instance.playerButtons.Add(utilFingerButton8);
							Transform transform15 = utilFingerButton8.transform.Find("ButtonText");
							if ((bool)transform15)
							{
								UtilMenuController.Instance.playerButtonTexts.Add(transform15.GetComponent<TextMeshPro>());
							}
						}
					}
					transform16 = transform14.Find("barExtra");
					if (transform16 != null)
					{
						UtilMenuController.Instance.barExtraRoot = transform16.gameObject;
						Transform transform17 = transform16.Find("GunSelectBtn");
						if (transform17 == null)
						{
							transform17 = transform16.Find("GunSelectButton");
						}
						instance = UtilMenuController.Instance;
						if ((object)transform17 == null)
						{
							obj = null;
						}
						else
						{
							obj = transform17.name;
							if (obj != null)
							{
								goto IL_0776;
							}
						}
						obj = "GunSelectBtn";
						goto IL_0776;
					}
				}
				goto IL_0899;
			}
			goto IL_0b9e;
		}
		UnityEngine.Debug.LogError("Panel1 not found");
		return;
		IL_0776:
		instance.gunSelectBtn = SetButtonListener(transform16, (string)obj, applyMats: true, delegate
		{
			if (UtilMenuController.Instance.CurrentPage is LobbyPage lobbyPage)
			{
				lobbyPage.OnGunSelectBtn();
			}
		});
		UtilMenuController.Instance.gunSelectSmallBtn = SetButtonListener(transform16, "gunSelectSmallBtn", applyMats: true, delegate
		{
			if (UtilMenuController.Instance.CurrentPage is LobbyPage lobbyPage)
			{
				lobbyPage.OnGunSelectBtn();
			}
		});
		UtilMenuController.Instance.peSliderLeft = SetButtonListener(transform16, "sliderBtnLeft", applyMats: false, delegate
		{
			if (!(UtilMenuController.Instance.CurrentPage is LobbyPage lobbyPage))
			{
				if (UtilMenuController.Instance.CurrentPage is SoundboardPage soundboardPage)
				{
					soundboardPage.OnTilePageLeft();
				}
			}
			else
			{
				lobbyPage.OnPlayerPageLeft();
			}
		});
		UtilMenuController.Instance.peSliderRight = SetButtonListener(transform16, "sliderBtnRight", applyMats: false, delegate
		{
			if (!(UtilMenuController.Instance.CurrentPage is LobbyPage lobbyPage))
			{
				if (UtilMenuController.Instance.CurrentPage is SoundboardPage soundboardPage)
				{
					soundboardPage.OnTilePageRight();
				}
			}
			else
			{
				lobbyPage.OnPlayerPageRight();
			}
		});
		Transform transform18 = transform16.Find("SliderText");
		if ((bool)transform18)
		{
			UtilMenuController.Instance.peSliderText = transform18.GetComponent<TextMeshPro>();
			Transform transform19 = transform18.Find("ValueText");
			if ((bool)transform19)
			{
				UtilMenuController.Instance.peValueText = transform19.GetComponent<TextMeshPro>();
			}
		}
		goto IL_0899;
		IL_0b9e:
		Transform transform20 = menu.transform.Find("panel2");
		if (transform20 != null)
		{
			UtilMenuController.Instance.panel2Root = transform20.gameObject;
			Transform transform21 = transform20.Find("UserInfoText");
			if ((bool)transform21)
			{
				UtilMenuController.Instance.userInfoText = transform21.GetComponent<TextMeshPro>();
			}
			Transform transform22 = transform20.Find("PlayerBackground");
			if ((bool)transform22)
			{
				UtilMenuController.Instance.playerBackgroundObj = transform22.gameObject;
			}
			Transform transform23 = transform20.Find("ModAndCheatListText");
			if ((bool)transform23)
			{
				UtilMenuController.Instance.modAndCheatListText = transform23.GetComponent<TextMeshPro>();
			}
			Transform transform24 = transform20.Find("volumeBar");
			if (transform24 != null)
			{
				UtilMenuController.Instance.volumeBarRoot = transform24.gameObject;
				UtilMenuController.Instance.volDownBtn = SetButtonListener(transform24, "VolumeDownBtn", applyMats: true, delegate
				{
					if (UtilMenuController.Instance.CurrentPage is LobbyPage lobbyPage)
					{
						lobbyPage.OnVolumeChange(up: false);
					}
				});
				UtilMenuController.Instance.volUpBtn = SetButtonListener(transform24, "VolumeUpBtn", applyMats: true, delegate
				{
					if (UtilMenuController.Instance.CurrentPage is LobbyPage lobbyPage)
					{
						lobbyPage.OnVolumeChange(up: true);
					}
				});
				UtilMenuController.Instance.volPrioritizeBtn = SetButtonListener(transform24, "PrioritizeBtn", applyMats: true, delegate
				{
					if (UtilMenuController.Instance.CurrentPage is LobbyPage lobbyPage)
					{
						lobbyPage.OnPrioritizeBtn();
					}
				});
				UtilMenuController.Instance.volUnMuteBtn = SetButtonListener(transform24, "UnMutedBtn", applyMats: true, delegate
				{
					if (UtilMenuController.Instance.CurrentPage is LobbyPage lobbyPage)
					{
						lobbyPage.OnMuteToggle();
					}
				});
				Transform transform25 = transform24.Find("VolumeText/ValueText");
				if ((bool)transform25)
				{
					UtilMenuController.Instance.volumeValueText = transform25.GetComponent<TextMeshPro>();
				}
			}
			Transform transform26 = transform20.Find("reportBar");
			if (transform26 != null)
			{
				UtilMenuController.Instance.reportBarRoot = transform26.gameObject;
				UtilMenuController.Instance.reportMainBtn = SetButtonListener(transform26, "MainReportBtn", applyMats: true, delegate
				{
					if (UtilMenuController.Instance.CurrentPage is LobbyPage lobbyPage)
					{
						lobbyPage.OnMainReportBtn();
					}
				});
				UtilMenuController.Instance.reportSendBtn = SetButtonListener(transform26, "SendReportBtn", applyMats: true, delegate
				{
					if (UtilMenuController.Instance.CurrentPage is LobbyPage lobbyPage)
					{
						lobbyPage.OnSendReport();
					}
				});
				UtilMenuController.Instance.reportSliderLeft = SetButtonListener(transform26, "sliderBtnLeft", applyMats: false, delegate
				{
					if (UtilMenuController.Instance.CurrentPage is LobbyPage lobbyPage)
					{
						lobbyPage.OnReportSlider(left: true);
					}
				});
				UtilMenuController.Instance.reportSliderRight = SetButtonListener(transform26, "sliderBtnRight", applyMats: false, delegate
				{
					if (UtilMenuController.Instance.CurrentPage is LobbyPage lobbyPage)
					{
						lobbyPage.OnReportSlider(left: false);
					}
				});
				Transform transform27 = transform26.Find("SliderText");
				if ((bool)transform27)
				{
					UtilMenuController.Instance.reportSliderText = transform27.GetComponent<TextMeshPro>();
				}
				Transform transform28 = transform27?.Find("ValueText");
				if ((bool)transform28)
				{
					UtilMenuController.Instance.reportValueText = transform28.GetComponent<TextMeshPro>();
				}
			}
			UtilMenuController.Instance.cloneCosmeticsBtnObj = SetButtonListener(transform20, "CloneCosmeticsBtn", applyMats: true, delegate
			{
				if (UtilMenuController.Instance.CurrentPage is LobbyPage lobbyPage)
				{
					lobbyPage.OnCloneCosmetics();
				}
			})?.gameObject;
			Transform transform29 = transform20.Find("ModCheckBar");
			if (transform29 != null)
			{
				UtilMenuController.Instance.modCheckBarRoot = transform29.gameObject;
				UtilFingerButton utilFingerButton9 = SetButtonListener(transform29, "modsBtn", applyMats: true, delegate
				{
					if (UtilMenuController.Instance.CurrentPage is LobbyPage lobbyPage)
					{
						lobbyPage.OnCheckMods();
					}
				});
				UtilMenuController.Instance.modsCheckBtn = utilFingerButton9;
				if ((bool)utilFingerButton9)
				{
					UtilMenuController.Instance.modsBtnText = utilFingerButton9.transform.Find("ButtonText")?.GetComponent<TextMeshPro>();
				}
				UtilFingerButton utilFingerButton10 = SetButtonListener(transform29, "cheatsBtn", applyMats: true, delegate
				{
					if (UtilMenuController.Instance.CurrentPage is LobbyPage lobbyPage)
					{
						lobbyPage.OnCheckCheats();
					}
				});
				UtilMenuController.Instance.cheatsCheckBtn = utilFingerButton10;
				if ((bool)utilFingerButton10)
				{
					UtilMenuController.Instance.cheatsBtnText = utilFingerButton10.transform.Find("ButtonText")?.GetComponent<TextMeshPro>();
				}
			}
		}
		Transform transform30 = transform.Find("sidebar");
		if (transform30 != null)
		{
			UtilMenuController.Instance.sideBtnList.Clear();
			for (int num3 = 1; num3 <= 6; num3++)
			{
				int index2 = num3;
				string text = "sideBtn" + num3;
				Transform transform31 = transform30.Find(text);
				if (!(transform31 != null))
				{
					continue;
				}
				UtilMenuController.Instance.sideBtnList.Add(transform31.gameObject);
				SetButtonListener(transform30, text, applyMats: true, delegate
				{
					if ((bool)UtilMenuController.Instance)
					{
						UtilMenuController.Instance.OnTabPress(index2);
					}
				}, isNavigation: true);
			}
		}
		UtilMenuController.Instance.RefreshUI();
		return;
		IL_0899:
		Transform transform32 = transform5.Find("Spotify");
		if (transform32 == null)
		{
			foreach (Transform item in transform5)
			{
				if (string.Equals(item.name, "Spotify", StringComparison.OrdinalIgnoreCase))
				{
					transform32 = item;
					break;
				}
			}
			if (transform32 == null)
			{
				StringBuilder stringBuilder = new StringBuilder("[SCM] Spotify GameObject not found under screenButtons. Children:");
				foreach (Transform item2 in transform5)
				{
					stringBuilder.Append(" '").Append(item2.name).Append("'");
				}
				UnityEngine.Debug.LogWarning(stringBuilder.ToString());
			}
		}
		if (transform32 != null)
		{
			UtilMenuController.Instance.spotifyRoot = transform32.gameObject;
			Transform transform35 = transform32.Find("spotifyBackground");
			if ((bool)transform35)
			{
				UtilMenuController.Instance.spotifyAlbumCoverRenderer = transform35.GetComponent<Renderer>();
			}
			Transform transform36 = transform32.Find("SongTitleText");
			if ((bool)transform36)
			{
				UtilMenuController.Instance.spotifySongTitleText = transform36.GetComponent<TextMeshPro>();
				Transform transform37 = transform36.Find("ArtistNameText");
				if ((bool)transform37)
				{
					UtilMenuController.Instance.spotifyArtistNameText = transform37.GetComponent<TextMeshPro>();
				}
			}
			Transform transform38 = transform32.Find("VolumeText");
			if ((bool)transform38)
			{
				Transform transform39 = transform38.Find("VolumeValueText");
				if ((bool)transform39)
				{
					UtilMenuController.Instance.spotifyVolumeValueText = transform39.GetComponent<TextMeshPro>();
				}
			}
			UnityAction action = delegate
			{
			};
			UtilMenuController.Instance.spotifyVolumeLeftBtn = SetButtonListener(transform32, "volumeSliderBtnLeft", applyMats: true, action);
			UtilMenuController.Instance.spotifyVolumeRightBtn = SetButtonListener(transform32, "volumeSliderBtnRight", applyMats: true, action);
			UtilMenuController.Instance.spotifyShuffleBtn = SetButtonListener(transform32, "ShuffleBtn", applyMats: true, action);
			UtilMenuController.Instance.spotifyRepeatBtn = SetButtonListener(transform32, "RepeatBtn", applyMats: true, action);
			UtilMenuController.Instance.spotifyPausePlayBtn = SetButtonListener(transform32, "PausePlayBtn", applyMats: true, action);
			UtilMenuController.Instance.spotifyRewindBtn = SetButtonListener(transform32, "RewindBtn", applyMats: true, action);
			UtilMenuController.Instance.spotifySkipBtn = SetButtonListener(transform32, "SkipBtn", applyMats: true, action);
			Transform transform40 = transform32.Find("PausePlayBtn");
			if ((bool)transform40)
			{
				Transform transform41 = transform40.Find("PauseIcon");
				if ((bool)transform41)
				{
					UtilMenuController.Instance.spotifyPauseIcon = transform41.gameObject;
				}
				Transform transform42 = transform40.Find("PlayIcon");
				if ((bool)transform42)
				{
					UtilMenuController.Instance.spotifyPlayIcon = transform42.gameObject;
				}
			}
		}
		goto IL_0b9e;
		UtilFingerButton SetButtonListener(Transform parent, string buttonName, bool applyMats, UnityAction call, bool isNavigation = false)
		{
			try
			{
				if (parent == null)
				{
					return null;
				}
				Transform transform43 = parent.Find(buttonName);
				if (!(transform43 == null))
				{
					GameObject gameObject = transform43.gameObject;
					gameObject.layer = 18;
					UtilMenuController.Instance.requiredLayerObjs.Add(gameObject);
					if (gameObject.GetComponent<Collider>() == null)
					{
						BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
						boxCollider.isTrigger = true;
					}
					UtilFingerButton utilFingerButton11 = gameObject.GetComponent<UtilFingerButton>();
					if (utilFingerButton11 == null)
					{
						utilFingerButton11 = gameObject.AddComponent<UtilFingerButton>();
					}
					utilFingerButton11.isNavigationButton = isNavigation;
					if (applyMats)
					{
						utilFingerButton11.unpressedMaterial = (isNavigation ? buttonMat : innerBtnMat);
						utilFingerButton11.pressedMaterial = pressedButtonMat;
					}
					if (utilFingerButton11.onPressButton == null)
					{
						utilFingerButton11.onPressButton = new UnityEvent();
					}
					utilFingerButton11.onPressButton.RemoveAllListeners();
					utilFingerButton11.onPressButton.AddListener(call);
					utilFingerButton11.UpdateColor();
					return utilFingerButton11;
				}
				return null;
			}
			catch (Exception ex)
			{
				UnityEngine.Debug.LogError("[SakUtil-Client] Error setting listener for " + buttonName + ": " + ex.Message);
				return null;
			}
		}
	}
}
