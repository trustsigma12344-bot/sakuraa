using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ExitGames.Client.Photon;
using GorillaNetworking;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Photon.Pun;
using Photon.Realtime;
using SakuraaCastingMod.Core;
using SakuraaCastingMod.Features.Overlays;
using SakuraaCastingMod.Features.Visuals;
using SakuraaCastingMod.Shared.Helpers;
using SakuraaCastingMod.VR.UtilMenu.Pages;
using UnityEngine;

namespace SakuraaCastingMod.VR.UtilMenu.Utility;

public class FriendNetworkController : MonoBehaviourPunCallbacks
{
	public static FriendNetworkController Instance;

	public List<NotificationModel> _notifications = new List<NotificationModel>();

	public List<FriendModel> Friends = new List<FriendModel>();

	public List<PendingFriendRequest> PendingRequests = new List<PendingFriendRequest>();

	public bool UI_NewNotification = false;

	public UserStatus CurrentStatus = UserStatus.IN_LAUNCHER;

	public ShareRoomOption ShareRoom = ShareRoomOption.ALL;

	public PermissionOption AllowFastJoins = PermissionOption.FRIENDS;

	public PermissionOption AllowRequests = PermissionOption.FRIENDS;

	public bool InParty = false;

	public bool IsPartyLeader = false;

	public string PartyId = "";

	public string PartyLeaderId = "";

	public PartyJoinMode PartyJoinMode = PartyJoinMode.NONE;

	public List<FriendModel> PartyMembers = new List<FriendModel>();

	private bool _isReservingForParty = false;

	private bool _isLeavingForJoin = false;

	private string _currentRoomCode = "";

	private string _capturedNonceBeforeLeave = "";

	private string _queuedJoinTarget;

	private bool _queuedJoinIsRaw;

	private readonly HashSet<string> _pendingCreates = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private readonly Dictionary<string, DateTime> _pendingCreatedAt = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);

	private const int PendingCreateTimeoutSec = 30;

	private static bool _localSpoofLatched;

	private const int MaxRoomCodeLength = 12;

	private static readonly string[] _specialStringList = new string[6] { "LBAAD.", "LBAAK.", "LBADE.", "LBANI.", "LBAGS.", "LMAPY." };

	private static readonly FieldInfo? _ownedSetField = typeof(VRRig).GetField("_playerOwnedCosmetics", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

	private static string[]? _cacIds;

	private static readonly FieldInfo? _concatStringField = typeof(CosmeticsController).GetField("concatStringCosmeticsAllowed", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

	private static readonly HashSet<string> _ignoredPropertyKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "didTutorial" };

	public static DisconnectCause LastDisconnectCause = DisconnectCause.None;

	public static float LastDisconnectTime = -999f;

	private float _lastDisconnectUtc = -1f;

	public const int ForceModeMemberCap = 10;

	private int _configStateApplyCount;

	public string UserId => (PhotonNetwork.LocalPlayer != null) ? PhotonNetwork.LocalPlayer.UserId : "";

	public string MyFriendCode { get; private set; } = "";

	public string MyDiscordName { get; private set; } = "";

	public static short LastJoinFailureCode { get; private set; }

	public static string LastJoinFailureMessage { get; private set; }

	public static float LastJoinFailureTime { get; private set; } = -999f;

	public bool LastReJoinReachedIdle { get; private set; } = true;

	public IReadOnlyCollection<string> GetPendingCreatesSnapshot()
	{
		return _pendingCreates.ToArray();
	}

	private void Awake()
	{
		Instance = this;
		JoinForensics.NonceProvider = ReadCurrentNonceFromAuthValues;
		StartCoroutine(NativeFriendsLoop());
		StartCoroutine(ConfigDirtyCheckLoop());
		StartCoroutine(PendingCreateTimeoutLoop());
	}

	private void OnApplicationQuit()
	{
		try
		{
			Configuration.FlushNow();
		}
		catch
		{
		}
	}

	private new void OnDisable()
	{
		try
		{
			Configuration.FlushNow();
		}
		catch
		{
		}
	}

	private IEnumerator NativeFriendsLoop()
	{
		yield return new WaitForSeconds(10f);
		while (true)
		{
			if (ModMessageHandler.IsConnected())
			{
				try
				{
					RefreshNatives();
				}
				catch
				{
				}
			}
			yield return new WaitForSeconds(60f);
		}
	}

	private IEnumerator ConfigDirtyCheckLoop()
	{
		yield return new WaitForSeconds(5f);
		while (true)
		{
			try
			{
				Configuration.CheckDirty();
			}
			catch
			{
			}
			yield return new WaitForSeconds(1f);
		}
	}

	private IEnumerator PendingCreateTimeoutLoop()
	{
		yield return new WaitForSeconds(30f);
		while (true)
		{
			try
			{
				DateTime now = DateTime.UtcNow;
				List<string> stale = (from kv in _pendingCreatedAt
					where (now - kv.Value).TotalSeconds > 30.0
					select kv.Key).ToList();
				bool anyStale = stale.Count > 0;
				foreach (string name in stale)
				{
					_pendingCreates.Remove(name);
					_pendingCreatedAt.Remove(name);
					PresetMeta tmpEntry = Configuration.AvailablePresets.FirstOrDefault((PresetMeta p) => p.IsPending && string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
					if (tmpEntry != null)
					{
						Configuration.AvailablePresets.Remove(tmpEntry);
						Configuration.BumpPresetListVersion();
						Notification.Send("[Preset] No confirmation for \"" + name + "\", refreshing", Color.yellow);
					}
				}
				if (anyStale)
				{
					try
					{
						LoadConfigState();
					}
					catch
					{
					}
				}
			}
			catch
			{
			}
			yield return new WaitForSeconds(2f);
		}
	}

	public void SendFriggenPacketYo(OpCode op, object data)
	{
		string action = op.ToString().ToLower();
		ModMessageHandler.Send(action, data);
	}

	private bool ShouldSuppressReports()
	{
		if (_localSpoofLatched)
		{
			return true;
		}
		try
		{
			Player localPlayer = PhotonNetwork.LocalPlayer;
			if (localPlayer == null)
			{
				return false;
			}
			string[] playerCos = GetPlayerCos(localPlayer);
			if (playerCos != null && Array.IndexOf(playerCos, "LBAAD.") >= 0)
			{
				_localSpoofLatched = true;
				return true;
			}
		}
		catch
		{
		}
		return false;
	}

	private void SyncFriendData(string action, object data)
	{
		if (ModMessageHandler.IsConnected() && !ShouldSuppressReports())
		{
			ModMessageHandler.Send(action, data);
		}
	}

	private void SyncRoomPresence()
	{
		if (!PhotonNetwork.InRoom)
		{
			return;
		}
		Room currentRoom = PhotonNetwork.CurrentRoom;
		object obj;
		if (currentRoom == null)
		{
			obj = null;
		}
		else
		{
			obj = currentRoom.Name;
			if (obj != null)
			{
				goto IL_0028;
			}
		}
		obj = "";
		goto IL_0028;
		IL_0028:
		string text = (string)obj;
		if (string.IsNullOrEmpty(text) || text.Length > 12)
		{
			return;
		}
		_currentRoomCode = text;
		object obj2 = currentRoom.CustomProperties["gameMode"];
		object obj3;
		if (obj2 == null)
		{
			obj3 = null;
		}
		else
		{
			obj3 = obj2.ToString();
			if (obj3 != null)
			{
				goto IL_006f;
			}
		}
		obj3 = "Unknown";
		goto IL_006f;
		IL_006f:
		string gameMode = (string)obj3;
		List<object> list = new List<object>();
		Player[] playerList = PhotonNetwork.PlayerList;
		foreach (Player player in playerList)
		{
			list.Add(new
			{
				inGameId = player.UserId,
				username = (player.NickName ?? "Unknown"),
				cosmetics = GetPlayerCos(player),
				properties = GetPlayerPro(player)
			});
		}
		string currentRoomCode = _currentRoomCode;
		Player localPlayer = PhotonNetwork.LocalPlayer;
		object obj4;
		if (localPlayer == null)
		{
			obj4 = null;
		}
		else
		{
			obj4 = localPlayer.UserId;
			if (obj4 != null)
			{
				goto IL_00f0;
			}
		}
		obj4 = "";
		goto IL_00f0;
		IL_00f0:
		SyncFriendData("game.room_enter", new
		{
			roomCode = currentRoomCode,
			gameMode = gameMode,
			selfInGameId = (string)obj4,
			players = list,
			nativeFriends = BuildNativesList()
		});
	}

	private void ClearRoomPresence()
	{
		if (!string.IsNullOrEmpty(_currentRoomCode))
		{
			SyncFriendData("game.room_leave", new
			{
				roomCode = _currentRoomCode
			});
			_currentRoomCode = "";
		}
	}

	private void NotiPJ(Player newPlayer)
	{
		SyncFriendData("game.player_join", new
		{
			inGameId = (newPlayer.UserId ?? newPlayer.ActorNumber.ToString()),
			username = (newPlayer.NickName ?? "Unknown"),
			cosmetics = GetPlayerCos(newPlayer),
			properties = GetPlayerPro(newPlayer)
		});
	}

	private void NotifPL(Player otherPlayer)
	{
		SyncFriendData("game.player_leave", new
		{
			inGameId = (otherPlayer.UserId ?? otherPlayer.ActorNumber.ToString())
		});
	}

	public void RefreshNatives()
	{
		SyncFriendData("game.native_friends_update", new
		{
			friends = BuildNativesList()
		});
	}

	private List<object> BuildNativesList()
	{
		List<object> list = new List<object>();
		if (FriendBackendController.Instance?.FriendsList == null)
		{
			return list;
		}
		foreach (FriendBackendController.Friend friends in FriendBackendController.Instance.FriendsList)
		{
			if (friends == null)
			{
				continue;
			}
			FriendBackendController.FriendPresence presence = friends.Presence;
			object obj;
			if (presence == null)
			{
				obj = null;
			}
			else
			{
				obj = presence.FriendLinkId;
				if (obj != null)
				{
					goto IL_0069;
				}
			}
			obj = "";
			goto IL_0069;
			IL_00bf:
			string text;
			object username;
			object obj2;
			list.Add(new
			{
				inGameId = text,
				username = (string)username,
				roomCode = (string)obj2
			});
			continue;
			IL_0069:
			text = (string)obj;
			FriendBackendController.FriendPresence presence2 = friends.Presence;
			object obj3;
			if (presence2 == null)
			{
				obj3 = null;
			}
			else
			{
				obj3 = presence2.UserName;
				if (obj3 != null)
				{
					goto IL_0087;
				}
			}
			obj3 = "";
			goto IL_0087;
			IL_0087:
			string text2 = (string)obj3;
			FriendNameCache.Remember(text, text2);
			username = FriendNameCache.Resolve(text, text2, "");
			FriendBackendController.FriendPresence presence3 = friends.Presence;
			if (presence3 == null)
			{
				obj2 = null;
			}
			else
			{
				obj2 = presence3.RoomId;
				if (obj2 != null)
				{
					goto IL_00bf;
				}
			}
			obj2 = "";
			goto IL_00bf;
		}
		return list;
	}

	private static string[] GetCAC()
	{
		if (_cacIds != null && _cacIds.Length != 0)
		{
			return _cacIds;
		}
		CosmeticsController instance = CosmeticsController.instance;
		if (instance?.allCosmetics != null)
		{
			try
			{
				_cacIds = (from it in instance.allCosmetics
					where !it.isNullItem && !string.IsNullOrEmpty(it.itemName)
					select it.itemName).ToArray();
				return _cacIds;
			}
			catch
			{
				return Array.Empty<string>();
			}
		}
		return Array.Empty<string>();
	}

	private string[] GetPlayerCos(Player player)
	{
		VRRig rigByNetPlayer;
		try
		{
			rigByNetPlayer = PlayerTranslator.GetRigByNetPlayer(player);
		}
		catch
		{
			return Array.Empty<string>();
		}
		if (!(rigByNetPlayer == null))
		{
			HashSet<string> hashSet = new HashSet<string>(StringComparer.Ordinal);
			try
			{
				CosmeticsController.CosmeticItem[] items = rigByNetPlayer.cosmeticSet.items;
				if (items != null)
				{
					CosmeticsController.CosmeticItem[] array = items;
					for (int i = 0; i < array.Length; i++)
					{
						CosmeticsController.CosmeticItem cosmeticItem = array[i];
						try
						{
							if (!cosmeticItem.isNullItem)
							{
								string itemName = cosmeticItem.itemName;
								if (!string.IsNullOrEmpty(itemName))
								{
									hashSet.Add(itemName);
								}
							}
						}
						catch
						{
						}
					}
				}
			}
			catch
			{
			}
			if (NetworkSystem.Instance?.LocalPlayer != null && player != null && string.Equals(player.UserId, NetworkSystem.Instance.LocalPlayer.UserId, StringComparison.Ordinal))
			{
				try
				{
					List<CosmeticsController.CosmeticItem> list = CosmeticsController.instance?.unlockedCosmetics;
					if (list != null)
					{
						foreach (CosmeticsController.CosmeticItem item in list)
						{
							try
							{
								if (!item.isNullItem)
								{
									string itemName2 = item.itemName;
									if (!string.IsNullOrEmpty(itemName2))
									{
										hashSet.Add(itemName2);
									}
								}
							}
							catch
							{
							}
						}
					}
				}
				catch
				{
				}
				try
				{
					if (_ownedSetField?.GetValue(rigByNetPlayer) is HashSet<string> hashSet2)
					{
						foreach (string item2 in hashSet2)
						{
							if (!string.IsNullOrEmpty(item2))
							{
								hashSet.Add(item2);
							}
						}
					}
				}
				catch
				{
				}
				try
				{
					if (_concatStringField?.GetValue(CosmeticsController.instance) is string text && !string.IsNullOrEmpty(text))
					{
						string[] cAC = GetCAC();
						foreach (string text2 in cAC)
						{
							if (text.Length >= text2.Length && text.Contains(text2))
							{
								hashSet.Add(text2);
							}
						}
					}
				}
				catch
				{
				}
			}
			try
			{
				string[] specialStringList = _specialStringList;
				foreach (string text3 in specialStringList)
				{
					try
					{
						if (rigByNetPlayer.IsItemAllowed(text3))
						{
							hashSet.Add(text3);
						}
					}
					catch
					{
					}
				}
			}
			catch
			{
			}
			return hashSet.ToArray();
		}
		return Array.Empty<string>();
	}

	private string[] GetPlayerPro(Player player)
	{
		try
		{
			ExitGames.Client.Photon.Hashtable customProperties = player.CustomProperties;
			if (customProperties != null && customProperties.Count != 0)
			{
				List<string> list = new List<string>(customProperties.Count);
				foreach (DictionaryEntry item in customProperties)
				{
					string text = item.Key?.ToString();
					if (!string.IsNullOrEmpty(text) && !_ignoredPropertyKeys.Contains(text))
					{
						list.Add(text);
					}
				}
				return list.ToArray();
			}
			return Array.Empty<string>();
		}
		catch
		{
			return Array.Empty<string>();
		}
	}

	private static void NoteJoinFailure(short returnCode, string message)
	{
		LastJoinFailureCode = returnCode;
		LastJoinFailureMessage = message;
		LastJoinFailureTime = Time.realtimeSinceStartup;
	}

	public static string FriendlyJoinFailure(short code)
	{
		return (code == 32765) ? "ROOM FULL!" : "JOIN FAILED!";
	}

	public override void OnJoinedRoom()
	{
		base.OnJoinedRoom();
		SyncRoomPresence();
		bool forceJoin;
		object obj;
		if (InParty && IsPartyLeader)
		{
			forceJoin = PartyJoinMode == PartyJoinMode.AUTO && PartyMembers.Count <= 10;
			Room currentRoom = PhotonNetwork.CurrentRoom;
			if (currentRoom == null)
			{
				obj = null;
			}
			else
			{
				obj = currentRoom.Name;
				if (obj != null)
				{
					goto IL_005c;
				}
			}
			obj = "";
			goto IL_005c;
		}
		goto IL_0068;
		IL_005c:
		ModMessageHandler.Send("party_photon_sync", new
		{
			roomCode = (string)obj,
			forceJoin = forceJoin
		});
		goto IL_0068;
		IL_0068:
		_isReservingForParty = false;
		_isLeavingForJoin = false;
		JoinForensics.NoteJoinResolved("OnJoinedRoom", success: true);
		RankVisuals.OnPhotonRoomJoined();
	}

	public override void OnLeftRoom()
	{
		base.OnLeftRoom();
		ClearRoomPresence();
	}

	public override void OnPlayerEnteredRoom(Player newPlayer)
	{
		base.OnPlayerEnteredRoom(newPlayer);
		NotiPJ(newPlayer);
		RankVisuals.OnPhotonPlayerEntered(newPlayer);
	}

	public override void OnPlayerLeftRoom(Player otherPlayer)
	{
		base.OnPlayerLeftRoom(otherPlayer);
		NotifPL(otherPlayer);
	}

	public override void OnDisconnected(DisconnectCause cause)
	{
		base.OnDisconnected(cause);
		_isReservingForParty = false;
		_isLeavingForJoin = false;
		_lastDisconnectUtc = Time.realtimeSinceStartup;
		LastDisconnectCause = cause;
		LastDisconnectTime = Time.realtimeSinceStartup;
		JoinForensics.Record($"OnDisconnected cause={cause}");
		if (cause != DisconnectCause.DisconnectByClientLogic && cause != DisconnectCause.None)
		{
			JoinForensics.FlushIncident("abnormal-disconnect", cause.ToString());
		}
	}

	public override void OnJoinRoomFailed(short returnCode, string message)
	{
		base.OnJoinRoomFailed(returnCode, message);
		_isReservingForParty = false;
		_isLeavingForJoin = false;
		NoteJoinFailure(returnCode, message);
		JoinForensics.NoteJoinResolved($"OnJoinRoomFailed code={returnCode} msg={message}", success: false);
	}

	public override void OnCreateRoomFailed(short returnCode, string message)
	{
		base.OnCreateRoomFailed(returnCode, message);
		_isReservingForParty = false;
		_isLeavingForJoin = false;
		NoteJoinFailure(returnCode, message);
		JoinForensics.NoteJoinResolved($"OnCreateRoomFailed code={returnCode} msg={message}", success: false);
	}

	public override void OnCustomAuthenticationFailed(string debugMessage)
	{
		base.OnCustomAuthenticationFailed(debugMessage);
		JoinForensics.Record("OnCustomAuthenticationFailed: " + debugMessage);
		JoinForensics.FlushIncident("custom-auth-failed", "CustomAuthenticationFailed");
	}

	public void SetShareRoom(ShareRoomOption s)
	{
		ShareRoom = s;
		SyncPrivacySettings();
	}

	public void SetFastJoin(PermissionOption p)
	{
		AllowFastJoins = p;
		SyncPrivacySettings();
	}

	public void SetRequests(PermissionOption p)
	{
		AllowRequests = p;
		SyncPrivacySettings();
	}

	private void SyncPrivacySettings()
	{
		ModMessageHandler.Send("set_privacy", new
		{
			shareRoom = (ShareRoom != ShareRoomOption.DISABLE),
			appearOffline = (CurrentStatus == UserStatus.OFFLINE)
		});
	}

	private static bool IsRateLimitError(string error)
	{
		if (string.IsNullOrEmpty(error))
		{
			return false;
		}
		return error.StartsWith("Wait ", StringComparison.OrdinalIgnoreCase) || error.IndexOf("too many", StringComparison.OrdinalIgnoreCase) >= 0 || error.IndexOf("slow down", StringComparison.OrdinalIgnoreCase) >= 0;
	}

	public void OnMessageReceived(string json)
	{
		try
		{
			JObject jObject;
			object obj;
			if (!string.IsNullOrEmpty(json))
			{
				jObject = JObject.Parse(json);
				if (jObject["handler"] != null)
				{
					JToken? jToken = jObject["action"];
					if (jToken == null)
					{
						obj = null;
					}
					else
					{
						obj = jToken.ToString();
						if (obj != null)
						{
							goto IL_0046;
						}
					}
					obj = "";
					goto IL_0046;
				}
				goto IL_02cf;
			}
			UnityEngine.Debug.LogError($"[FNC] OnMessageReceived got null/empty payload, bridge dropped a frame (len={((json == null) ? (-1) : 0)})");
			return;
			IL_00a4:
			object obj2;
			string text = (string)obj2;
			UnityEngine.Debug.LogError("[FNC] response with no action: error=" + text + ", likely server is missing a handler this client expects");
			goto IL_00bc;
			IL_0314:
			object obj3;
			string text2 = (string)obj3;
			string text3;
			JObject jObject4;
			object obj5;
			object obj9;
			JObject jObject2;
			object obj4;
			string text4;
			JToken? jToken3;
			object obj6;
			JToken? jToken4;
			object obj7;
			string text6;
			string text7;
			switch (text3)
			{
			case "FRIEND_REMOVED":
			{
				JObject jObject3 = JObject.Parse(text2);
				string removedId = jObject3["byId"]?.ToString();
				if (!string.IsNullOrEmpty(removedId))
				{
					Friends.RemoveAll((FriendModel f) => f.UserId == removedId);
					if (UtilMenuController.Instance != null)
					{
						UtilMenuController.Instance.RefreshUI();
					}
				}
				break;
			}
			case "FRIEND_LIST":
				Friends = JsonConvert.DeserializeObject<List<FriendModel>>(text2) ?? new List<FriendModel>();
				if (UtilMenuController.Instance != null)
				{
					UtilMenuController.Instance.RefreshUI();
				}
				break;
			case "NOTIFICATION_NEW":
			{
				NotificationModel notif = JsonConvert.DeserializeObject<NotificationModel>(text2);
				AddNotification(notif);
				break;
			}
			case "FRIEND_REQUEST_IN":
			{
				jObject4 = JObject.Parse(text2);
				JToken? jToken5 = jObject4["discordName"];
				if (jToken5 == null)
				{
					obj5 = null;
				}
				else
				{
					obj5 = jToken5.ToString();
					if (obj5 != null)
					{
						goto IL_04b1;
					}
				}
				obj5 = "Unknown";
				goto IL_04b1;
			}
			case "FRIEND_REQUEST_ACCEPTED":
			{
				JObject jObject6 = JObject.Parse(text2);
				JToken? jToken6 = jObject6["discordName"];
				if (jToken6 == null)
				{
					obj9 = null;
				}
				else
				{
					obj9 = jToken6.ToString();
					if (obj9 != null)
					{
						goto IL_05c9;
					}
				}
				obj9 = "Someone";
				goto IL_05c9;
			}
			case "FRIEND_PENDING":
				PendingRequests = JsonConvert.DeserializeObject<List<PendingFriendRequest>>(text2) ?? new List<PendingFriendRequest>();
				UI_NewNotification = true;
				break;
			case "CONFIG_STATE":
			{
				JObject jObject5 = JObject.Parse(text2);
				JObject state = (jObject5["state"] as JObject) ?? jObject5;
				Configuration.ApplyPresetListFromPush(state);
				try
				{
					ThemeManager.RefreshTheme();
					UtilMenuController.Instance?.RefreshUI();
					break;
				}
				catch
				{
					break;
				}
			}
			case "MOD_STATE_CLEAR":
				SecureNetworkManager.Instance?.HandleModStateClear(JObject.Parse(text2));
				break;
			case "MOD_STATE":
				SecureNetworkManager.Instance?.HandleModStatePush(JObject.Parse(text2));
				break;
			case "CONFIG_SHARE_RECEIVE":
			{
				ConfigSharePayload configSharePayload = JsonConvert.DeserializeObject<ConfigSharePayload>(text2);
				string text5 = ((!string.IsNullOrEmpty(configSharePayload.FromName)) ? configSharePayload.FromName : "a friend");
				string payloadJson = ((configSharePayload.Patch != null) ? configSharePayload.Patch.ToString(Formatting.None) : configSharePayload.ConfigJson);
				AddNotification(new NotificationModel
				{
					Title = "CONFIG",
					Description = configSharePayload.ConfigName + " from " + text5,
					Type = "CONFIG_RECEIVE",
					PayloadJson = payloadJson
				});
				break;
			}
			case "PARTY_LEADER_APPROVAL_REQ":
			{
				PartyApprovalRequestPayload partyApprovalRequestPayload = JsonConvert.DeserializeObject<PartyApprovalRequestPayload>(text2);
				AddNotification(new NotificationModel
				{
					Title = "PARTY REQUEST",
					Description = partyApprovalRequestPayload.RequesterUsername + " wants to join.",
					Type = "PARTY_APPROVAL",
					PayloadJson = partyApprovalRequestPayload.RequestId
				});
				break;
			}
			case "PARTY_SNAPSHOT":
				UpdatePartyState(JsonConvert.DeserializeObject<PartySnapshotPayload>(text2));
				break;
			case "PARTY_PHOTON_SYNC":
			{
				PartyPhotonSyncPayload partyPhotonSyncPayload = JsonConvert.DeserializeObject<PartyPhotonSyncPayload>(text2);
				if (!partyPhotonSyncPayload.ForceJoin)
				{
					AddNotification(new NotificationModel
					{
						Title = "PARTY MOVE",
						Description = "Moving to " + partyPhotonSyncPayload.RoomCode,
						Type = "GAME_INVITE",
						PayloadJson = partyPhotonSyncPayload.RoomCode
					});
				}
				else
				{
					StartCoroutine(JoinRoomAfterLeave(partyPhotonSyncPayload.RoomCode));
				}
				break;
			}
			case "FRIEND_PRESENCE":
				HandleFriendPresence(JObject.Parse(text2));
				break;
			case "MY_PROFILE":
				{
					jObject2 = JObject.Parse(text2);
					JToken? jToken2 = jObject2["friendCode"];
					if (jToken2 == null)
					{
						obj4 = null;
					}
					else
					{
						obj4 = jToken2.ToString();
						if (obj4 != null)
						{
							goto IL_0980;
						}
					}
					obj4 = "";
					goto IL_0980;
				}
				IL_04b1:
				text4 = (string)obj5;
				jToken3 = jObject4["friendCode"];
				if (jToken3 == null)
				{
					obj6 = null;
				}
				else
				{
					obj6 = jToken3.ToString();
					if (obj6 != null)
					{
						goto IL_04d4;
					}
				}
				obj6 = "";
				goto IL_04d4;
				IL_0980:
				MyFriendCode = (string)obj4;
				jToken4 = jObject2["discordName"];
				if (jToken4 == null)
				{
					obj7 = null;
				}
				else
				{
					obj7 = jToken4.ToString();
					if (obj7 != null)
					{
						goto IL_09a7;
					}
				}
				obj7 = "";
				goto IL_09a7;
				IL_09a7:
				MyDiscordName = (string)obj7;
				break;
				IL_04d4:
				text6 = (string)obj6;
				PendingRequests.Add(new PendingFriendRequest
				{
					UserId = jObject4["fromId"]?.ToString(),
					Username = text4
				});
				AddNotification(new NotificationModel
				{
					Title = "FRIEND REQUEST",
					Description = text4 + " (" + text6 + ")",
					Type = "FRIEND_REQ",
					PayloadJson = jObject4["fromId"]?.ToString()
				});
				break;
				IL_05c9:
				text7 = (string)obj9;
				AddNotification(new NotificationModel
				{
					Title = "FRIEND ACCEPTED",
					Description = text7 + " accepted your request",
					Type = "INFO"
				});
				RefreshFriends();
				break;
			}
			return;
			IL_02cf:
			if (jObject["op"] == null)
			{
				return;
			}
			text3 = jObject["op"].ToString();
			JToken? jToken7 = jObject["d"];
			if (jToken7 == null)
			{
				obj3 = null;
			}
			else
			{
				obj3 = jToken7.ToString();
				if (obj3 != null)
				{
					goto IL_0314;
				}
			}
			obj3 = "";
			goto IL_0314;
			IL_00bc:
			string text8;
			JObject jObject7;
			bool flag2;
			if (text8 == "config_state_load" || text8 == "preset_load" || text8 == "config_state_reset")
			{
				bool flag = jObject7?["state"] is JObject;
				UnityEngine.Debug.Log($"[DEBUG CFG-LOAD] '{text8}' response arrived,  success={flag2} hasState={flag} totalSize={json.Length}B");
				if (flag2 && jObject7?["state"] is JObject state2)
				{
					Configuration.ApplyFullState(state2);
					if (text8 == "config_state_load")
					{
						_configStateApplyCount++;
					}
				}
			}
			if (((text8 == "preset_create") & flag2) && jObject7?["preset"] is JObject presetObj)
			{
				HandlePresetCreateResponse(presetObj);
			}
			if (((text8 == "preset_state_pull") & flag2) && jObject7 != null)
			{
				HandlePresetStatePullResponse(jObject7);
			}
			if (((text8 == "config_state_patch") & flag2) && jObject7 != null)
			{
				try
				{
					Configuration.ConfirmLegacyMigration();
				}
				catch
				{
				}
			}
			if (((text8 == "preset_delete") & flag2) && jObject7 != null)
			{
				string text9 = jObject7["id"]?.ToString();
				if (!string.IsNullOrEmpty(text9))
				{
					Configuration.ConfirmPendingDelete(text9);
				}
			}
			if (!flag2 && jObject["error"] != null)
			{
				string text10 = jObject["error"].ToString();
				if (!IsRateLimitError(text10))
				{
					if (text8 == "preset_create" || text8 == "preset_load" || text8 == "preset_delete")
					{
						HandlePresetActionFailure(text8, text10, jObject);
					}
				}
				else
				{
					Notification.Send("<color=yellow>[SLOW DOWN]</color> " + text10);
				}
			}
			goto IL_02cf;
			IL_0046:
			text8 = (string)obj;
			flag2 = jObject["success"]?.Value<bool>() ?? false;
			jObject7 = jObject["data"] as JObject;
			if (string.IsNullOrEmpty(text8) && !flag2)
			{
				JToken? jToken8 = jObject["error"];
				if (jToken8 == null)
				{
					obj2 = null;
				}
				else
				{
					obj2 = jToken8.ToString();
					if (obj2 != null)
					{
						goto IL_00a4;
					}
				}
				obj2 = "<no error>";
				goto IL_00a4;
			}
			goto IL_00bc;
		}
		catch (Exception ex)
		{
			UnityEngine.Debug.LogError("[NetError] " + ex.Message);
		}
	}

	private void UpdatePartyState(PartySnapshotPayload data)
	{
		if (data == null)
		{
			InParty = false;
			PartyMembers.Clear();
			PartyId = "";
		}
		else
		{
			InParty = true;
			PartyId = data.PartyId;
			PartyLeaderId = data.LeaderId;
			PartyJoinMode = data.Mode;
			PartyMembers = data.Members ?? new List<FriendModel>();
			IsPartyLeader = data.YouAreLeader;
		}
		if (UtilMenuController.Instance != null)
		{
			UtilMenuController.Instance.RefreshUI();
		}
	}

	private void HandleFriendPresence(JObject data)
	{
		string userId = data["userId"]?.ToString();
		if (string.IsNullOrEmpty(userId))
		{
			return;
		}
		FriendModel friendModel = Friends.FirstOrDefault((FriendModel f) => f.UserId == userId);
		if (friendModel == null)
		{
			return;
		}
		if (data["status"] != null && Enum.TryParse<UserStatus>(data["status"].ToString(), out var result))
		{
			friendModel.Status = result;
		}
		JToken? jToken = data["roomCode"];
		object obj;
		if (jToken == null)
		{
			obj = null;
		}
		else
		{
			obj = jToken.ToString();
			if (obj != null)
			{
				goto IL_00a5;
			}
		}
		obj = "";
		goto IL_00a5;
		IL_00a5:
		friendModel.RoomCode = (string)obj;
		friendModel.InRoom = data["inRoom"]?.Value<bool?>() == true;
		if (UtilMenuController.Instance != null)
		{
			UtilMenuController.Instance.RefreshUI();
		}
	}

	public void HandleZoneTrigger(GorillaNetworkJoinTrigger trigger)
	{
		if (!_isReservingForParty)
		{
			StartCoroutine(PartyJoinRoutine(trigger));
		}
	}

	private IEnumerator PartyJoinRoutine(GorillaNetworkJoinTrigger trigger)
	{
		_isReservingForParty = true;
		string gameModeName = trigger.GetFullDesiredGameModeString();
		yield return SafeLeaveAndWait();
		object obj;
		if (!(NetworkSystem.Instance != null) || !NetworkSystem.Instance.InRoom)
		{
			JoinForensics.NoteModJoin(gameModeName, raw: false, "PartyJoinRoutine");
			bool forceMode = PartyJoinMode == PartyJoinMode.AUTO && PartyMembers.Count <= 10;
			if (PartyJoinMode == PartyJoinMode.AUTO && !forceMode)
			{
				UnityEngine.Debug.LogWarning($"[FNC] Party too large for FORCE ({PartyMembers.Count} > {10}), falling back to INVITE for this join");
			}
			if (forceMode)
			{
				Player localPlayer = PhotonNetwork.LocalPlayer;
				if (localPlayer == null)
				{
					obj = null;
				}
				else
				{
					obj = localPlayer.UserId;
					if (obj != null)
					{
						goto IL_00f3;
					}
				}
				obj = "";
				goto IL_00f3;
			}
			if (PhotonNetworkController.Instance != null)
			{
				try
				{
					PhotonNetworkController.Instance.AttemptToJoinPublicRoom(trigger);
					yield break;
				}
				catch
				{
					_isReservingForParty = false;
					yield break;
				}
			}
			_isReservingForParty = false;
			yield break;
		}
		UnityEngine.Debug.LogWarning("[FNC] Party: leave did not complete within hardCap, aborting");
		_isReservingForParty = false;
		yield break;
		IL_00f3:
		string localPhotonId = (string)obj;
		string[] expectedUsers = (from m in PartyMembers
			where !string.IsNullOrEmpty(m.PhotonId) && m.PhotonId != localPhotonId
			select m.PhotonId).ToArray();
		ExitGames.Client.Photon.Hashtable roomProps = new ExitGames.Client.Photon.Hashtable { { "gameMode", gameModeName } };
		bool joinSent = PhotonNetwork.JoinRandomRoom(roomProps, 10, MatchmakingMode.FillRoom, TypedLobby.Default, null, expectedUsers);
		UnityEngine.Debug.Log(string.Format("[FNC] Party AUTO: JoinRandomRoom sent={0} expectedUsers=[{1}]", joinSent, string.Join(",", expectedUsers)));
		if (joinSent)
		{
			yield break;
		}
		UnityEngine.Debug.LogWarning("[FNC] Party AUTO: JoinRandomRoom returned false - falling back to AttemptToJoinPublicRoom");
		if (PhotonNetworkController.Instance != null)
		{
			try
			{
				PhotonNetworkController.Instance.AttemptToJoinPublicRoom(trigger);
			}
			catch
			{
				_isReservingForParty = false;
			}
		}
		else
		{
			_isReservingForParty = false;
		}
	}

	public override void OnJoinRandomFailed(short returnCode, string message)
	{
		base.OnJoinRandomFailed(returnCode, message);
		NoteJoinFailure(returnCode, message);
		JoinForensics.Record($"OnJoinRandomFailed code={returnCode} msg={message}");
		if (_isReservingForParty)
		{
			PhotonNetwork.CreateRoom(null, new RoomOptions
			{
				MaxPlayers = 10
			});
		}
	}

	public void RefreshFriends()
	{
		ModMessageHandler.Send("friend_list_update");
	}

	public void RefreshPendingRequests()
	{
		ModMessageHandler.Send("friend_pending_requests");
	}

	public void GetMyProfile()
	{
		SendFriggenPacketYo(OpCode.GET_MY_PROFILE, null);
	}

	public void AddFriendInGame(string photonId)
	{
		SendFriggenPacketYo(OpCode.FRIEND_ADD_INGAME, new { photonId });
	}

	public void AcceptFriendRequest(string userId)
	{
		ProcessFriendAction(userId, FriendActionType.REQUEST_ACCEPT);
	}

	public void DeclineFriendRequest(string userId)
	{
		ProcessFriendAction(userId, FriendActionType.REQUEST_DECLINE);
	}

	public void RemoveFriend(string userId)
	{
		ProcessFriendAction(userId, FriendActionType.REMOVE);
	}

	private void ProcessFriendAction(string targetId, FriendActionType action)
	{
		SendFriggenPacketYo(OpCode.FRIEND_ACTION, new
		{
			targetUserId = targetId,
			action = action
		});
	}

	public void CreateParty(PartyJoinMode mode = PartyJoinMode.INVITE)
	{
		SendFriggenPacketYo(OpCode.PARTY_CREATE, new { mode });
	}

	public void LeaveParty()
	{
		SendFriggenPacketYo(OpCode.PARTY_LEAVE, null);
		InParty = false;
		PartyId = "";
		PartyMembers.Clear();
	}

	public void SetPartyMode(PartyJoinMode mode)
	{
		if (mode != PartyJoinMode.AUTO || PartyMembers.Count <= 10)
		{
			SendFriggenPacketYo(OpCode.PARTY_UPDATE_SETTINGS, new { mode });
		}
		else
		{
			Notification.Send($"<color=red>FORCE mode needs {10} or fewer members</color>");
		}
	}

	public void InviteToParty(string userId)
	{
		SendFriggenPacketYo(OpCode.NOTIFICATION_NEW, new
		{
			targetUserId = userId,
			type = "PARTY_INVITE",
			payloadJson = PartyId
		});
	}

	public void RequestJoinParty(string friendId)
	{
		SendFriggenPacketYo(OpCode.PARTY_JOIN_REQUEST, new
		{
			targetUserId = friendId
		});
	}

	public void KickFromParty(string userId)
	{
		SendFriggenPacketYo(OpCode.PARTY_KICK, new
		{
			targetUserId = userId
		});
	}

	public void PromoteToLeader(string userId)
	{
		SendFriggenPacketYo(OpCode.PARTY_PROMOTE, new
		{
			targetUserId = userId
		});
	}

	public void InviteToRoom(string userId)
	{
		if (PhotonNetwork.InRoom)
		{
			SendFriggenPacketYo(OpCode.NOTIFICATION_NEW, new
			{
				targetUserId = userId,
				type = "GAME_INVITE",
				payloadJson = PhotonNetwork.CurrentRoom.Name
			});
		}
	}

	public void RequestJoinGame(string userId)
	{
		SendFriggenPacketYo(OpCode.GAME_JOIN_REQUEST, new
		{
			targetUserId = userId
		});
	}

	public bool IsMemberInParty(string userId)
	{
		return PartyMembers != null && PartyMembers.Exists((FriendModel m) => m.UserId == userId);
	}

	public void JoinFriendRoom(FriendModel friend)
	{
		if (!string.IsNullOrEmpty(friend.RoomCode))
		{
			StartCoroutine(JoinRoomAfterLeave(friend.RoomCode));
		}
	}

	public void StartJoinRoom(string roomCode)
	{
		StartCoroutine(JoinRoomAfterLeave(roomCode));
	}

	private static string ReadCurrentNonceFromAuthValues()
	{
		AuthenticationValues authValues = PhotonNetwork.AuthValues;
		if (authValues == null)
		{
			return null;
		}
		if (!(authValues.AuthPostData is Dictionary<string, object> dictionary))
		{
			return null;
		}
		object value;
		return dictionary.TryGetValue("Nonce", out value) ? (value as string) : null;
	}

	public IEnumerator SafeLeaveAndWait(float hardCap = 8f)
	{
		if (NetworkSystem.Instance != null && NetworkSystem.Instance.InRoom)
		{
			_capturedNonceBeforeLeave = ReadCurrentNonceFromAuthValues() ?? "";
			JoinForensics.NoteLeaveBaseline(_capturedNonceBeforeLeave);
			JoinForensics.Record("SafeLeaveAndWait: ReturnToSinglePlayer");
			NetworkSystem.Instance.ReturnToSinglePlayer();
			yield return WaitForSafeReJoin(hardCap);
		}
	}

	private IEnumerator WaitForSafeReJoin(float hardCap = 8f)
	{
		float deadline = Time.realtimeSinceStartup + hardCap;
		bool reachedIdle = false;
		while (Time.realtimeSinceStartup < deadline)
		{
			if (!(NetworkSystem.Instance == null) && NetworkSystem.Instance.netState != NetSystemState.Idle)
			{
				yield return null;
				continue;
			}
			reachedIdle = true;
			break;
		}
		LastReJoinReachedIdle = reachedIdle;
		if (reachedIdle)
		{
			JoinForensics.Record("WaitForSafeReJoin: reached Idle (nonce refresh done)");
			yield return null;
		}
		else
		{
			UnityEngine.Debug.LogWarning("[FNC] netState did not reach Idle within " + hardCap + "s after ReturnToSinglePlayer - proceeding anyway; next auth may fail.");
			JoinForensics.Record($"WaitForSafeReJoin: TIMEOUT after {hardCap}s - netState not Idle, proceeding with possibly-stale nonce");
		}
	}

	private void QueueOrDispatch(string roomCode, bool raw)
	{
		if (!_isLeavingForJoin)
		{
			if (!raw)
			{
				StartCoroutine(JoinRoomAfterLeave(roomCode));
			}
			else
			{
				StartCoroutine(JoinRoomAfterLeaveRaw(roomCode));
			}
		}
		else
		{
			_queuedJoinTarget = roomCode;
			_queuedJoinIsRaw = raw;
		}
	}

	private void FlushQueuedJoin()
	{
		if (!string.IsNullOrEmpty(_queuedJoinTarget))
		{
			string queuedJoinTarget = _queuedJoinTarget;
			bool queuedJoinIsRaw = _queuedJoinIsRaw;
			_queuedJoinTarget = null;
			QueueOrDispatch(queuedJoinTarget, queuedJoinIsRaw);
		}
	}

	public void StartJoinRoomRaw(string roomCode)
	{
		QueueOrDispatch(roomCode, raw: true);
	}

	private IEnumerator JoinRoomAfterLeaveRaw(string roomCode)
	{
		if (!string.IsNullOrEmpty(roomCode))
		{
			if (!_isLeavingForJoin)
			{
				_isLeavingForJoin = true;
				try
				{
					yield return SafeLeaveAndWait();
					UnityEngine.Debug.Log("[FNC] Raw-joining room " + roomCode);
					JoinForensics.NoteModJoin(roomCode, raw: true, "JoinRoomAfterLeaveRaw");
					PhotonNetwork.JoinRoom(roomCode);
					yield break;
				}
				finally
				{
					_isLeavingForJoin = false;
					FlushQueuedJoin();
				}
			}
			_queuedJoinTarget = roomCode;
			_queuedJoinIsRaw = true;
		}
		else
		{
			UnityEngine.Debug.LogWarning("[FNC] JoinRoomAfterLeaveRaw called with empty roomCode, ignoring");
		}
	}

	private IEnumerator JoinRoomAfterLeave(string roomCode)
	{
		if (!string.IsNullOrEmpty(roomCode))
		{
			if (!_isLeavingForJoin)
			{
				_isLeavingForJoin = true;
				try
				{
					yield return SafeLeaveAndWait();
					UnityEngine.Debug.Log("[FNC] Joining room " + roomCode);
					JoinForensics.NoteModJoin(roomCode, raw: false, "JoinRoomAfterLeave");
					if (PhotonNetworkController.Instance != null)
					{
						PhotonNetworkController.Instance.AttemptToJoinSpecificRoom(roomCode, JoinType.Solo);
					}
					else
					{
						PhotonNetwork.JoinRoom(roomCode);
					}
					yield break;
				}
				finally
				{
					_isLeavingForJoin = false;
					FlushQueuedJoin();
				}
			}
			_queuedJoinTarget = roomCode;
			_queuedJoinIsRaw = false;
		}
		else
		{
			UnityEngine.Debug.LogWarning("[FNC] JoinRoomAfterLeave called with empty roomCode - ignoring");
		}
	}

	public void ShareConfig(string targetId, string configName)
	{
		SendFriggenPacketYo(OpCode.CONFIG_SHARE_SEND, new
		{
			targetUserId = targetId,
			configName = configName
		});
	}

	public void ShareConfig(string targetId, string configName, Dictionary<string, JToken> patch)
	{
		ShareConfig(targetId, configName);
	}

	public void ShareConfig(string targetId, string configName, string json)
	{
		ShareConfig(targetId, configName);
	}

	public void LoadConfigState()
	{
		bool flag = ModMessageHandler.IsConnected();
		int configStateApplyCount = _configStateApplyCount;
		bool flag2 = ModMessageHandler.Send("config_state_load", new { });
		UnityEngine.Debug.Log($"[FNC] LoadConfigState dispatch: connected={flag} sent={flag2}");
		StartCoroutine(LoadConfigStateRetryLoop(configStateApplyCount));
	}

	private IEnumerator LoadConfigStateRetryLoop(int beforeCount)
	{
		int[] earlyDelaysSec = new int[3] { 3, 6, 12 };
		int totalElapsedSec = 0;
		for (int i = 0; i < earlyDelaysSec.Length; i++)
		{
			yield return new WaitForSeconds(earlyDelaysSec[i]);
			totalElapsedSec += earlyDelaysSec[i];
			if (_configStateApplyCount <= beforeCount)
			{
				UnityEngine.Debug.Log($"[FNC] config_state_load response not received after {totalElapsedSec}s - retrying");
				ModMessageHandler.Send("config_state_load", new { });
				continue;
			}
			yield break;
		}
		while (_configStateApplyCount <= beforeCount)
		{
			yield return new WaitForSeconds(30f);
			if (_configStateApplyCount > beforeCount)
			{
				break;
			}
			UnityEngine.Debug.Log("[FNC] config_state_load still pending - slow-retrying every 30s");
			ModMessageHandler.Send("config_state_load", new { });
		}
	}

	public void PatchConfigState(Dictionary<string, JToken> patch, string hash)
	{
		ModMessageHandler.Send("config_state_patch", new { patch, hash });
	}

	public void ResetConfigState()
	{
		ModMessageHandler.Send("config_state_reset");
	}

	public void SetConfigFlag(string key, bool value)
	{
		ModMessageHandler.Send("config_state_flag_set", new { key, value });
	}

	public bool CreatePreset(string name, string modVersion = null)
	{
		if (!string.IsNullOrWhiteSpace(name))
		{
			name = name.Trim();
			if (!_pendingCreates.Contains(name))
			{
				_pendingCreates.Add(name);
				_pendingCreatedAt[name] = DateTime.UtcNow;
				Configuration.AvailablePresets.Insert(0, new PresetMeta
				{
					Id = "tmp-" + Guid.NewGuid().ToString("N"),
					Name = name,
					Hash = "",
					CreatedAt = DateTime.UtcNow,
					ModVersion = modVersion,
					IsPending = true
				});
				Configuration.BumpPresetListVersion();
			}
			return ModMessageHandler.Send("preset_create", new { name, modVersion });
		}
		return false;
	}

	public void LoadPreset(string id)
	{
		ModMessageHandler.Send("preset_load", new { id });
	}

	public void DeletePreset(string id)
	{
		ModMessageHandler.Send("preset_delete", new { id });
	}

	public void PullPresetState(string id)
	{
		ModMessageHandler.Send("preset_state_pull", new { id });
	}

	private void HandlePresetStatePullResponse(JObject dataObj)
	{
		JToken? jToken = dataObj["id"];
		object obj;
		if (jToken == null)
		{
			obj = null;
		}
		else
		{
			obj = jToken.ToString();
			if (obj != null)
			{
				goto IL_0027;
			}
		}
		obj = "";
		goto IL_0027;
		IL_004c:
		string id;
		if (string.IsNullOrEmpty(id) || !(dataObj["state"] is JObject jObject))
		{
			return;
		}
		Dictionary<string, JToken> dictionary = new Dictionary<string, JToken>();
		foreach (JProperty item in jObject.Properties())
		{
			dictionary[item.Name] = item.Value;
		}
		Configuration.RegisterPresetCache(id, dictionary);
		if (!(Configuration.ActivePresetId == id))
		{
			return;
		}
		Configuration.ApplyServerKeys(dictionary);
		Configuration.BumpPresetListVersion();
		PresetMeta presetMeta = Configuration.AvailablePresets.FirstOrDefault((PresetMeta p) => p.Id == id);
		object obj2;
		if (presetMeta == null)
		{
			obj2 = null;
		}
		else
		{
			obj2 = presetMeta.Name;
			if (obj2 != null)
			{
				goto IL_0122;
			}
		}
		obj2 = id;
		goto IL_0122;
		IL_0122:
		Notification.Send("<color=green>[PRESET]</color> Loaded \"" + (string)obj2 + "\"");
		return;
		IL_0027:
		id = (string)obj;
		JToken? jToken2 = dataObj["hash"];
		object obj3;
		if (jToken2 == null)
		{
			obj3 = null;
		}
		else
		{
			obj3 = jToken2.ToString();
			if (obj3 != null)
			{
				goto IL_004c;
			}
		}
		obj3 = "";
		goto IL_004c;
	}

	private void HandlePresetCreateResponse(JObject presetObj)
	{
		JToken? jToken = presetObj["id"];
		object obj;
		if (jToken == null)
		{
			obj = null;
		}
		else
		{
			obj = jToken.ToString();
			if (obj != null)
			{
				goto IL_0026;
			}
		}
		obj = "";
		goto IL_0026;
		IL_0048:
		object obj2;
		string name = (string)obj2;
		JToken? jToken2 = presetObj["hash"];
		object obj3;
		if (jToken2 == null)
		{
			obj3 = null;
		}
		else
		{
			obj3 = jToken2.ToString();
			if (obj3 != null)
			{
				goto IL_006d;
			}
		}
		obj3 = "";
		goto IL_006d;
		IL_006d:
		string hash = (string)obj3;
		DateTime createdAt = presetObj["createdAt"]?.Value<DateTime?>() ?? DateTime.UtcNow;
		string modVersion = presetObj["modVersion"]?.ToString();
		_pendingCreates.Remove(name);
		_pendingCreatedAt.Remove(name);
		PresetMeta presetMeta = Configuration.AvailablePresets.FirstOrDefault((PresetMeta p) => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
		string text;
		if (presetMeta != null)
		{
			presetMeta.Id = text;
			presetMeta.Hash = hash;
			presetMeta.CreatedAt = createdAt;
			presetMeta.ModVersion = modVersion;
			presetMeta.IsPending = false;
		}
		else
		{
			Configuration.AvailablePresets.Insert(0, new PresetMeta
			{
				Id = text,
				Name = name,
				Hash = hash,
				CreatedAt = createdAt,
				ModVersion = modVersion,
				IsPending = false
			});
		}
		if (presetObj["state"] is JObject jObject)
		{
			Dictionary<string, JToken> dictionary = new Dictionary<string, JToken>();
			foreach (JProperty item in jObject.Properties())
			{
				dictionary[item.Name] = item.Value;
			}
			Configuration.RegisterPresetCache(text, dictionary);
		}
		else
		{
			Configuration.RegisterPresetCache(text, Configuration.GatherCurrentKeys());
		}
		Configuration.ActivePresetId = text;
		Configuration.ActivePresetName = name;
		Configuration.BumpPresetListVersion();
		Notification.Send("[Preset] Saved \"" + name + "\"", Color.green, UtilMenuMain.Instance?.Icons?.Save);
		return;
		IL_0026:
		text = (string)obj;
		JToken? jToken3 = presetObj["name"];
		if (jToken3 == null)
		{
			obj2 = null;
		}
		else
		{
			obj2 = jToken3.ToString();
			if (obj2 != null)
			{
				goto IL_0048;
			}
		}
		obj2 = "";
		goto IL_0048;
	}

	private void HandlePresetActionFailure(string action, string error, JObject root)
	{
		string text = error switch
		{
			"NAME_EXISTS" => "A preset with that name already exists.", 
			"NAME_TOO_LONG" => "Preset name is too long (64 char max).", 
			"NOT_FOUND" => "Preset not found on the server.", 
			"TIER_LIMIT" => "You've hit your preset cap for your tier.", 
			"NAME_BLANK" => "Preset name cannot be empty.", 
			_ => "Preset operation failed: " + error, 
		};
		if (action == "preset_delete")
		{
			string text2 = ((!(root["data"] is JObject jObject)) ? null : jObject["id"]?.ToString());
			if (!string.IsNullOrEmpty(text2))
			{
				if (error == "NOT_FOUND")
				{
					Configuration.ConfirmPendingDelete(text2);
					return;
				}
				Configuration.RestorePendingDelete(text2);
			}
		}
		object obj;
		if (action == "preset_create")
		{
			JObject jObject2 = root["data"] as JObject;
			if (jObject2 == null)
			{
				obj = null;
			}
			else
			{
				JToken? jToken = jObject2["name"];
				if (jToken == null)
				{
					obj = null;
				}
				else
				{
					obj = jToken.ToString();
					if (obj != null)
					{
						goto IL_0135;
					}
				}
			}
			obj = "";
			goto IL_0135;
		}
		goto IL_020a;
		IL_020a:
		Notification.Send("[Preset] " + text, Color.red);
		UnityEngine.Debug.LogError("[FNC] " + action + " failed: " + error);
		return;
		IL_0135:
		string nameHint = (string)obj;
		if (string.IsNullOrEmpty(nameHint))
		{
			PresetMeta presetMeta = Configuration.AvailablePresets.FirstOrDefault((PresetMeta p) => p.IsPending);
			if (presetMeta != null)
			{
				nameHint = presetMeta.Name;
			}
		}
		if (!string.IsNullOrEmpty(nameHint))
		{
			_pendingCreates.Remove(nameHint);
			_pendingCreatedAt.Remove(nameHint);
			PresetMeta presetMeta2 = Configuration.AvailablePresets.FirstOrDefault((PresetMeta p) => p.IsPending && string.Equals(p.Name, nameHint, StringComparison.OrdinalIgnoreCase));
			if (presetMeta2 != null)
			{
				Configuration.AvailablePresets.Remove(presetMeta2);
			}
			Configuration.BumpPresetListVersion();
		}
		if (error == "NAME_EXISTS")
		{
			try
			{
				LoadConfigState();
			}
			catch
			{
			}
		}
		goto IL_020a;
	}

	public void AddNotification(NotificationModel notif, bool sendToast = true)
	{
		if (notif != null)
		{
			if (notif.Timestamp == 0L)
			{
				notif.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
			}
			if (string.IsNullOrEmpty(notif.NotificationId))
			{
				notif.NotificationId = BuildDedupKey(notif);
			}
			string dedupKey = BuildDedupKey(notif);
			_notifications.RemoveAll((NotificationModel n) => BuildDedupKey(n) == dedupKey);
			_notifications.Insert(0, notif);
			if (_notifications.Count > 30)
			{
				_notifications.RemoveAt(_notifications.Count - 1);
			}
			if (sendToast && NotificationsPage.FriendNotificationsEnabled)
			{
				Notification.Send("<color=cyan>[NEW]</color> " + notif.Title);
			}
			UI_NewNotification = true;
		}
	}

	private static string BuildDedupKey(NotificationModel n)
	{
		string text = n.Type ?? "";
		string text2 = n.PayloadJson ?? "";
		string text3 = n.Title ?? "";
		return string.IsNullOrEmpty(text2) ? (text + "|" + text3) : (text + "|" + text2);
	}

	public void RemoveNotification(NotificationModel n)
	{
		if (_notifications.Contains(n))
		{
			_notifications.Remove(n);
		}
	}

	public void HandleNotificationAction(NotificationModel notif)
	{
		switch (notif.Type)
		{
		case "GAME_INVITE":
			StartCoroutine(JoinRoomAfterLeave(notif.PayloadJson));
			break;
		case "GAME_JOIN_REQUEST":
			if (PhotonNetwork.InRoom && !string.IsNullOrEmpty(notif.PayloadJson))
			{
				InviteToRoom(notif.PayloadJson);
			}
			break;
		case "FRIEND_REQ":
			if (!string.IsNullOrEmpty(notif.PayloadJson))
			{
				AcceptFriendRequest(notif.PayloadJson);
			}
			else
			{
				RefreshPendingRequests();
			}
			break;
		case "CONFIG_RECEIVE":
			try
			{
				Configuration.LoadFromJson(notif.PayloadJson);
				try
				{
					ThemeManager.RefreshTheme();
					UtilMenuController.Instance.RefreshUI();
				}
				catch
				{
				}
				UnityEngine.Debug.Log("Config imported successfully: " + notif.PayloadJson);
				Notification.Send("<color=green>[SUCCESS]</color> Config Loaded!");
			}
			catch (Exception ex)
			{
				UnityEngine.Debug.LogError("Failed to load shared config: " + ex.Message);
			}
			break;
		case "PARTY_APPROVAL":
			SendFriggenPacketYo(OpCode.PARTY_LEADER_DECISION, new
			{
				requestId = notif.PayloadJson,
				accept = true
			});
			break;
		case "PARTY_INVITE":
			SendFriggenPacketYo(OpCode.PARTY_JOIN_REQUEST, new
			{
				partyId = notif.PayloadJson
			});
			break;
		}
		RemoveNotification(notif);
	}

	public void ClearHistory()
	{
		_notifications.Clear();
	}

	public List<NotificationModel> GetNotificationHistory()
	{
		CleanupExpired();
		return _notifications;
	}

	private static int GetTtlSeconds(string type)
	{
		return type switch
		{
			"GAME_INVITE" => 30, 
			"PARTY_APPROVAL" => 60, 
			"SPECIAL_ALERT" => 1800, 
			"FRIEND_REQ" => 0, 
			"CHEATER_ALERT" => 1800, 
			"PARTY_INVITE" => 300, 
			"INFO" => 120, 
			"CONFIG_RECEIVE" => 600, 
			"GAME_JOIN_REQUEST" => 60, 
			"MODDER_ALERT" => 1800, 
			_ => 300, 
		};
	}

	public void CleanupExpired()
	{
		long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
		int count = _notifications.Count;
		_notifications.RemoveAll(delegate(NotificationModel n)
		{
			int ttlSeconds = GetTtlSeconds(n.Type ?? "");
			return ttlSeconds > 0 && now - n.Timestamp > ttlSeconds;
		});
		if (_notifications.Count != count)
		{
			UI_NewNotification = true;
		}
	}
}
