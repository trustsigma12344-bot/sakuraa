using Newtonsoft.Json.Linq;
using Photon.Pun;
using Photon.Realtime;
using SakuraaCastingMod.VR.UtilMenu;
using SakuraaCastingMod.VR.UtilMenu.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Shared.Helpers;

public class SecureNetworkManager : MonoBehaviourPunCallbacks
{
  public static SecureNetworkManager Instance;
  [SavedSetting("AnonymousMode", false)]
  public static bool AnonymousMode;
  public GameObject remoteMenuPrefab;
  private bool _scmLocalLastMenuOpen;
  private readonly Dictionary<Player, RemoteModRepresentation> _remoteObjects = new Dictionary<Player, RemoteModRepresentation>();
  private readonly Queue<RemoteModRepresentation> _repPool = new Queue<RemoteModRepresentation>();
  private readonly Dictionary<string, SecureNetworkManager.ModStateData> _pendingByPhotonId = new Dictionary<string, SecureNetworkManager.ModStateData>();
  private readonly Dictionary<string, byte> _remoteTiers = new Dictionary<string, byte>();
  private bool _hasBroadcast;
  private bool _lastM;
  private bool _lastK;
  private bool _lastP2;
  private float _lastMpOffX;
  private float _lastMpOffY;
  private float _lastMpOffZ;
  private float _lastMrOffX;
  private float _lastMrOffY;
  private float _lastMrOffZ;
  private Color _lastCBtn;
  private Color _lastCPrs;
  private Color _lastCInr;
  private Color _lastCSel;
  private Color _lastCPnl;

  public bool TryGetRemoteTier(string userId, out byte tier)
  {
    if (!string.IsNullOrEmpty(userId) && this._remoteTiers.TryGetValue(userId, out tier))
      return true;
    tier = (byte) 0;
    return false;
  }

  private void Awake() => SecureNetworkManager.Instance = this;

  private void Start() => ((MonoBehaviour) this).StartCoroutine(this.SyncLoop());

  public bool IsPlayerSecured(Player p) => this._remoteObjects.ContainsKey(p);

  public override void OnJoinedRoom()
  {
    this._hasBroadcast = false;
    this._pendingByPhotonId.Clear();
  }

  public override void OnPlayerEnteredRoom(Player newPlayer)
  {
    if ((newPlayer == null ? 1 : (string.IsNullOrEmpty(newPlayer.UserId) ? 1 : 0)) != 0)
      return;
    this._hasBroadcast = false;
    SecureNetworkManager.ModStateData state;
    if (!this._pendingByPhotonId.TryGetValue(newPlayer.UserId, out state))
      return;
    this._pendingByPhotonId.Remove(newPlayer.UserId);
    this.UpdateRemoteRepresentation(newPlayer, state);
  }

  public override void OnPlayerLeftRoom(Player otherPlayer)
  {
    if ((otherPlayer == null ? 0 : (!string.IsNullOrEmpty(otherPlayer.UserId) ? 1 : 0)) != 0)
    {
      this._remoteTiers.Remove(otherPlayer.UserId);
      this._pendingByPhotonId.Remove(otherPlayer.UserId);
    }
    RemoteModRepresentation modRepresentation;
    if (!this._remoteObjects.TryGetValue(otherPlayer, out modRepresentation))
      return;
    this._remoteObjects.Remove(otherPlayer);
    if (!((UnityEngine.Object) modRepresentation != (UnityEngine.Object) null))
      return;
    modRepresentation.UnbindOwner();
    this._repPool.Enqueue(modRepresentation);
  }

  public override void OnLeftRoom()
  {
    this._hasBroadcast = false;
    this._pendingByPhotonId.Clear();
    foreach (Component component in this._remoteObjects.Values.Where<RemoteModRepresentation>((Func<RemoteModRepresentation, bool>) (rep => ((UnityEngine.Object) rep != (UnityEngine.Object) null))))
      UnityEngine.Object.Destroy((UnityEngine.Object) component.gameObject);
    this._remoteObjects.Clear();
    this._remoteTiers.Clear();
    while (this._repPool.Count > 0)
    {
      RemoteModRepresentation modRepresentation = this._repPool.Dequeue();
      if (((UnityEngine.Object) modRepresentation != (UnityEngine.Object) null))
        UnityEngine.Object.Destroy((UnityEngine.Object) ((Component) modRepresentation).gameObject);
    }
  }

  private IEnumerator SyncLoop()
  {
    WaitForSeconds wait = new WaitForSeconds(0.25f);
    while (true)
    {
      if (PhotonNetwork.InRoom)
      {
        this.BroadcastLocalState();
        this.DrainPending();
      }
      yield return (object) wait;
    }
  }

  private void BroadcastLocalState()
  {
    if (SecureNetworkManager.AnonymousMode)
    {
      if (!this._hasBroadcast)
        return;
      ModMessageHandler.Send("mod_state_clear");
      this._hasBroadcast = false;
    }
    else
    {
      bool flag1 = ((UnityEngine.Object) UtilMenuController.Instance != (UnityEngine.Object) null) && UtilMenuController.Instance.isMenuEnabled;
      bool flag2 = false;
      bool flag3 = false;
      if ((!flag1 ? 0 : (((UnityEngine.Object) UtilMenuController.Instance != (UnityEngine.Object) null) ? 1 : 0)) != 0)
      {
        flag2 = ((UnityEngine.Object) KeyboardController.Instance != (UnityEngine.Object) null) && KeyboardController.Instance.IsKeyboardActive;
        flag3 = ((UnityEngine.Object) UtilMenuController.Instance.panel2Root != (UnityEngine.Object) null) && UtilMenuController.Instance.panel2Root.activeSelf;
        Player localPlayer = PhotonNetwork.LocalPlayer;
        string str1;
        if (localPlayer == null)
        {
          str1 = (string) null;
        }
        else
        {
          str1 = localPlayer.UserId;
          if (str1 != null)
            goto label_8;
        }
        str1 = "";
label_8:
        string str2 = str1;
        if (((str2 == "D6971CA01F82A975" || str2 == "38C7AFBF5FC3014C" ? 1 : (str2 == "9AF07DD7384734AC" ? 1 : 0)) == 0 ? 0 : (!this._scmLocalLastMenuOpen ? 1 : 0)) != 0)
          this.LogMenuPoseCapture();
      }
      this._scmLocalLastMenuOpen = flag1;
      Color customColor1 = ThemeManager.GetCustomColor("BUTTON");
      Color customColor2 = ThemeManager.GetCustomColor("PRESSED");
      Color customColor3 = ThemeManager.GetCustomColor("INNER");
      Color customColor4 = ThemeManager.GetCustomColor("SELECTED");
      Color customColor5 = ThemeManager.GetCustomColor("PANEL");
      float offsetPosX = UtilMenuController.OffsetPosX;
      float offsetPosY = UtilMenuController.OffsetPosY;
      float offsetPosZ = UtilMenuController.OffsetPosZ;
      float offsetRotX = UtilMenuController.OffsetRotX;
      float offsetRotY = UtilMenuController.OffsetRotY;
      float offsetRotZ = UtilMenuController.OffsetRotZ;
      if ((!this._hasBroadcast || this._lastM != flag1 || this._lastK != flag2 || this._lastP2 != flag3 || (double) this._lastMpOffX != (double) offsetPosX || (double) this._lastMpOffY != (double) offsetPosY || (double) this._lastMpOffZ != (double) offsetPosZ || (double) this._lastMrOffX != (double) offsetRotX || (double) this._lastMrOffY != (double) offsetRotY || (double) this._lastMrOffZ != (double) offsetRotZ || !(this._lastCBtn == customColor1) || !(this._lastCPrs == customColor2) || !(this._lastCInr == customColor3) || !(this._lastCSel == customColor4) ? 0 : ((this._lastCPnl == customColor5) ? 1 : 0)) != 0)
        return;
      SecureNetworkManager.ModStateData data = new SecureNetworkManager.ModStateData();
      data.m = flag1;
      data.k = flag2;
      data.p2 = flag3;
      SecureNetworkManager.ModStateData modStateData1 = data;
      float[] numArray1;
      if (!flag1)
        numArray1 = (float[]) null;
      else
        numArray1 = new float[3]
        {
          offsetPosX,
          offsetPosY,
          offsetPosZ
        };
      modStateData1.mpOff = numArray1;
      SecureNetworkManager.ModStateData modStateData2 = data;
      float[] numArray2;
      if (!flag1)
        numArray2 = (float[]) null;
      else
        numArray2 = new float[3]
        {
          offsetRotX,
          offsetRotY,
          offsetRotZ
        };
      modStateData2.mrOff = numArray2;
      data.tc = new float[20]
      {
        customColor1.r,
        customColor1.g,
        customColor1.b,
        customColor1.a,
        customColor2.r,
        customColor2.g,
        customColor2.b,
        customColor2.a,
        customColor3.r,
        customColor3.g,
        customColor3.b,
        customColor3.a,
        customColor4.r,
        customColor4.g,
        customColor4.b,
        customColor4.a,
        customColor5.r,
        customColor5.g,
        customColor5.b,
        customColor5.a
      };
      if (!ModMessageHandler.Send("mod_state_update", (object) data))
        return;
      this._hasBroadcast = true;
      this._lastM = flag1;
      this._lastK = flag2;
      this._lastP2 = flag3;
      this._lastMpOffX = offsetPosX;
      this._lastMpOffY = offsetPosY;
      this._lastMpOffZ = offsetPosZ;
      this._lastMrOffX = offsetRotX;
      this._lastMrOffY = offsetRotY;
      this._lastMrOffZ = offsetRotZ;
      this._lastCBtn = customColor1;
      this._lastCPrs = customColor2;
      this._lastCInr = customColor3;
      this._lastCSel = customColor4;
      this._lastCPnl = customColor5;
    }
  }

  private void LogMenuPoseCapture()
  {
    Transform transform1 = ((Component) UtilMenuController.Instance).transform;
    Vector3 position = transform1.position;
    Quaternion rotation = transform1.rotation;
    Transform parent = transform1.parent;
    string str1 = ((UnityEngine.Object) parent != (UnityEngine.Object) null) ? ((UnityEngine.Object) parent).name : "<null parent>";
    StringBuilder stringBuilder = new StringBuilder("[SCM-LOCAL-OPEN] menu hierarchy: ");
    Transform transform2 = transform1;
    while (((UnityEngine.Object) transform2 != (UnityEngine.Object) null))
    {
      stringBuilder.Append(((UnityEngine.Object) transform2).name);
      transform2 = transform2.parent;
      if (((UnityEngine.Object) transform2 != (UnityEngine.Object) null))
        stringBuilder.Append(" <- ");
    }
    UnityEngine.Debug.Log((object) stringBuilder.ToString());
    Vector3 localPosition1 = transform1.localPosition;
    Quaternion localRotation = transform1.localRotation;
    Vector3 eulerAngles1 = localRotation.eulerAngles;
    string str2 = str1;
    UnityEngine.Debug.Log((object) $"[SCM-LOCAL-OPEN] menu transform.localPos={localPosition1:F4} localRotEuler={eulerAngles1:F3} parent='{str2}'");
    VRRig offlineVrRig = ((UnityEngine.Object) GorillaTagger.Instance != (UnityEngine.Object) null) ? GorillaTagger.Instance.offlineVRRig : (VRRig) null;
    Transform leftHandTransform = ((UnityEngine.Object) offlineVrRig != (UnityEngine.Object) null) ? offlineVrRig.leftHandTransform : (Transform) null;
    Transform rightHandTransform = ((UnityEngine.Object) offlineVrRig != (UnityEngine.Object) null) ? offlineVrRig.rightHandTransform : (Transform) null;
    Transform transform3 = !((UnityEngine.Object) offlineVrRig != (UnityEngine.Object) null) || !((UnityEngine.Object) offlineVrRig.headMesh != (UnityEngine.Object) null) ? (Transform) null : offlineVrRig.headMesh.transform;
    Transform transform4 = !((UnityEngine.Object) offlineVrRig != (UnityEngine.Object) null) || !((UnityEngine.Object) offlineVrRig.mainCamera != (UnityEngine.Object) null) ? (Transform) null : offlineVrRig.mainCamera.transform;
    Vector3 vector3_1 = ((UnityEngine.Object) leftHandTransform != (UnityEngine.Object) null) ? leftHandTransform.InverseTransformPoint(position) : Vector3.zero;
    Quaternion quaternion;
    Vector3 vector3_2;
    if (!((UnityEngine.Object) leftHandTransform != (UnityEngine.Object) null))
    {
      vector3_2 = Vector3.zero;
    }
    else
    {
      quaternion = (Quaternion.Inverse(leftHandTransform.rotation) * rotation);
      vector3_2 = quaternion.eulerAngles;
    }
    Vector3 vector3_3 = vector3_2;
    Vector3 vector3_4 = ((UnityEngine.Object) rightHandTransform != (UnityEngine.Object) null) ? rightHandTransform.InverseTransformPoint(position) : Vector3.zero;
    Vector3 vector3_5;
    if (!((UnityEngine.Object) rightHandTransform != (UnityEngine.Object) null))
    {
      vector3_5 = Vector3.zero;
    }
    else
    {
      quaternion = (Quaternion.Inverse(rightHandTransform.rotation) * rotation);
      vector3_5 = quaternion.eulerAngles;
    }
    Vector3 vector3_6 = vector3_5;
    Vector3 vector3_7 = ((UnityEngine.Object) transform3 != (UnityEngine.Object) null) ? transform3.InverseTransformPoint(position) : Vector3.zero;
    Vector3 vector3_8;
    if (!((UnityEngine.Object) transform3 != (UnityEngine.Object) null))
    {
      vector3_8 = Vector3.zero;
    }
    else
    {
      quaternion = (Quaternion.Inverse(transform3.rotation) * rotation);
      vector3_8 = quaternion.eulerAngles;
    }
    Vector3 vector3_9 = vector3_8;
    Vector3 vector3_10 = ((UnityEngine.Object) transform4 != (UnityEngine.Object) null) ? transform4.InverseTransformPoint(position) : Vector3.zero;
    Vector3 vector3_11;
    if (!((UnityEngine.Object) transform4 != (UnityEngine.Object) null))
    {
      vector3_11 = Vector3.zero;
    }
    else
    {
      quaternion = (Quaternion.Inverse(transform4.rotation) * rotation);
      vector3_11 = quaternion.eulerAngles;
    }
    Vector3 vector3_12 = vector3_11;
    string[] strArray = new string[9];
    strArray[0] = "[SCM-LOCAL] parent='";
    strArray[1] = str1;
    strArray[2] = "' ";
    Vector3 localPosition2 = transform1.localPosition;
    quaternion = transform1.localRotation;
    Vector3 eulerAngles2 = quaternion.eulerAngles;
    strArray[3] = $"menuLocalPos={localPosition2:F4} menuLocalRotEuler={eulerAngles2:F3} ";
    strArray[4] = $"| LH-FRAME pos={vector3_1:F4} rotEuler={vector3_3:F3} ";
    strArray[5] = $"| RH-FRAME pos={vector3_4:F4} rotEuler={vector3_6:F3} ";
    strArray[6] = $"| HEAD-FRAME pos={vector3_7:F4} rotEuler={vector3_9:F3} ";
    strArray[7] = $"| CAM-FRAME pos={vector3_10:F4} rotEuler={vector3_12:F3} ";
    strArray[8] = $"| menuWorld={position:F4} menuWorldRotEuler={rotation.eulerAngles:F3}";
    UnityEngine.Debug.Log((object) string.Concat(strArray));
  }

  public void HandleModStatePush(JObject d)
  {
    if (d == null)
      return;
    JToken jtoken = d["photonId"];
    string str = jtoken != null ? Newtonsoft.Json.Linq.Extensions.Value<string>((IEnumerable<JToken>) jtoken) : (string) null;
    if (string.IsNullOrEmpty(str))
      return;
    SecureNetworkManager.ModStateData state = d["state"]?.ToObject<SecureNetworkManager.ModStateData>();
    if (!SecureNetworkManager.ValidateState(state))
      return;
    if ((state.t < (byte) 1 ? 0 : (state.t <= (byte) 3 ? 1 : 0)) != 0)
      this._remoteTiers[str] = state.t;
    Player playerByPhotonId = SecureNetworkManager.FindPlayerByPhotonId(str);
    if (playerByPhotonId == null)
    {
      this._pendingByPhotonId[str] = state;
    }
    else
    {
      this._pendingByPhotonId.Remove(str);
      this.UpdateRemoteRepresentation(playerByPhotonId, state);
    }
  }

  public void HandleModStateClear(JObject d)
  {
    if (d == null)
      return;
    JToken jtoken = d["photonId"];
    string str = jtoken != null ? Newtonsoft.Json.Linq.Extensions.Value<string>((IEnumerable<JToken>) jtoken) : (string) null;
    if (string.IsNullOrEmpty(str))
      return;
    this._pendingByPhotonId.Remove(str);
    this._remoteTiers.Remove(str);
    Player playerByPhotonId = SecureNetworkManager.FindPlayerByPhotonId(str);
    if (playerByPhotonId == null)
      return;
    this.RetractRemote(playerByPhotonId);
  }

  private void DrainPending()
  {
    if (this._pendingByPhotonId.Count == 0)
      return;
    foreach (string str in this._pendingByPhotonId.Keys.ToList<string>())
    {
      Player playerByPhotonId = SecureNetworkManager.FindPlayerByPhotonId(str);
      SecureNetworkManager.ModStateData state;
      if (playerByPhotonId != null && this._pendingByPhotonId.TryGetValue(str, out state))
      {
        this._pendingByPhotonId.Remove(str);
        this.UpdateRemoteRepresentation(playerByPhotonId, state);
      }
    }
  }

  private static Player FindPlayerByPhotonId(string photonId)
  {
    Player playerByPhotonId;
    if ((string.IsNullOrEmpty(photonId) ? 1 : (PhotonNetwork.CurrentRoom == null ? 1 : 0)) == 0)
    {
      foreach (Player player in PhotonNetwork.CurrentRoom.Players.Values)
      {
        if ((player == null ? 1 : (player.IsLocal ? 1 : 0)) == 0 && player.UserId == photonId)
        {
          playerByPhotonId = player;
          goto label_9;
        }
      }
      playerByPhotonId = (Player) null;
    }
    else
      playerByPhotonId = (Player) null;
label_9:
    return playerByPhotonId;
  }

  private void RetractRemote(Player p)
  {
    if (p == null)
      return;
    if (!string.IsNullOrEmpty(p.UserId))
      this._remoteTiers.Remove(p.UserId);
    RemoteModRepresentation modRepresentation;
    if (!this._remoteObjects.TryGetValue(p, out modRepresentation))
      return;
    this._remoteObjects.Remove(p);
    if (!((UnityEngine.Object) modRepresentation != (UnityEngine.Object) null))
      return;
    modRepresentation.UnbindOwner();
    this._repPool.Enqueue(modRepresentation);
  }

  private static bool ValidateState(SecureNetworkManager.ModStateData state) => state != null;

  private void UpdateRemoteRepresentation(Player p, SecureNetworkManager.ModStateData state)
  {
    RemoteModRepresentation modRepresentation;
    if (!this._remoteObjects.TryGetValue(p, out modRepresentation))
    {
      if (((UnityEngine.Object) this.remoteMenuPrefab == (UnityEngine.Object) null))
      {
        UnityEngine.Debug.LogWarning((object) ("[SCM] Remote menu prefab not assigned - skipping remote representation for " + p.NickName));
        return;
      }
      if (!state.m)
        return;
      if (this._repPool.Count > 0)
      {
        modRepresentation = this._repPool.Dequeue();
        if (((UnityEngine.Object) modRepresentation != (UnityEngine.Object) null))
        {
          modRepresentation.Rebind(p);
          this._remoteObjects[p] = modRepresentation;
        }
      }
      if (((UnityEngine.Object) modRepresentation == (UnityEngine.Object) null))
      {
        modRepresentation = new GameObject("RemoteMod_" + p.NickName).AddComponent<RemoteModRepresentation>();
        modRepresentation.Initialize(this.remoteMenuPrefab, p);
        this._remoteObjects[p] = modRepresentation;
      }
    }
    Vector3 menuPosOffset = state.mpOff == null || state.mpOff.Length < 3 ? Vector3.zero : new Vector3(state.mpOff[0], state.mpOff[1], state.mpOff[2]);
    Vector3 menuRotOffset = state.mrOff == null || state.mrOff.Length < 3 ? Vector3.zero : new Vector3(state.mrOff[0], state.mrOff[1], state.mrOff[2]);
    modRepresentation.UpdateState(state.m, state.k, state.p2, state.tc, menuPosOffset, menuRotOffset);
  }

  [Serializable]
  private class ModStateData
  {
    public bool m;
    public bool k;
    public bool p2;
    public float[] mpOff;
    public float[] mrOff;
    public float[] tc;
    public byte t;
  }
}
