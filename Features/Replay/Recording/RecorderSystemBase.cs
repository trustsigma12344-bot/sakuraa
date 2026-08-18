using GorillaTag;
using HarmonyLib;
using Photon.Pun;
using Photon.Voice.Unity;
using SakuraaCastingMod.Features.Replay.Patches;
using SakuraaCastingMod.Features.Replay.Recording.UI;
using SakuraaCastingMod.Shared.Helpers;
using System;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Features.Replay.Recording;

public class RecorderSystemBase : MonoBehaviour
{
  public static RecorderSystemBase Instance;
  [SavedSetting("RecorderEnabled", false)]
  public static bool RecorderEnabled = false;
  [SavedSetting("RecorderAutoRecordOnMatch", true)]
  public static bool AutoRecordOnMatch = true;
  [SavedSetting("RecorderRecordVoice", true)]
  public static bool RecordVoice = true;
  [SavedSetting("RecorderCaptureHz", 20f)]
  public static float CaptureHz = 20f;
  public ReplayRecorder recorder = new ReplayRecorder();
  private bool _wiredNetworkCallbacks;

  public bool IsRecording => this.recorder != null && this.recorder.isRecording;

  public int ReplayTime => this.recorder == null ? 0 : this.recorder.ReplayTime;

  public string ReplayContainerFolder
  {
    get => this.recorder == null ? "" : this.recorder.SessionFolderName;
  }

  private void Awake() => RecorderSystemBase.Instance = this;

  private void Start() => this.ApplyPatches();

  private void ApplyPatches()
  {
    Harmony harmony = new Harmony("com.sakuraa.replayrecorder");
    MethodBase methodBase1 = (MethodBase) AccessTools.Method(typeof (GorillaGameManager), "OnPlayerEnteredRoom", new Type[1]
    {
      typeof (NetPlayer)
    }, (Type[]) null);
    if (methodBase1 != (MethodBase) null)
      harmony.Patch(methodBase1, new HarmonyMethod(AccessTools.Method(typeof (RecorderPlayerEnteredPatch), "Prefix", (Type[]) null, (Type[]) null)), (HarmonyMethod) null, (HarmonyMethod) null, (HarmonyMethod) null, (HarmonyMethod) null);
    MethodBase methodBase2 = (MethodBase) AccessTools.Method(typeof (GorillaGameManager), "OnPlayerLeftRoom", new Type[1]
    {
      typeof (NetPlayer)
    }, (Type[]) null);
    if (methodBase2 != (MethodBase) null)
      harmony.Patch(methodBase2, new HarmonyMethod(AccessTools.Method(typeof (RecorderPlayerLeftPatch), "Prefix", (Type[]) null, (Type[]) null)), (HarmonyMethod) null, (HarmonyMethod) null, (HarmonyMethod) null, (HarmonyMethod) null);
    MethodBase methodBase3 = (MethodBase) AccessTools.Method(typeof (UnityAudioOut), "OutWrite", new Type[2]
    {
      typeof (float[]),
      typeof (int)
    }, (Type[]) null);
    if (!(methodBase3 != (MethodBase) null))
      return;
    harmony.Patch(methodBase3, new HarmonyMethod(AccessTools.Method(typeof (AudioRecordingPatch), "Prefix", (Type[]) null, (Type[]) null)), (HarmonyMethod) null, (HarmonyMethod) null, (HarmonyMethod) null, (HarmonyMethod) null);
  }

  private void Update()
  {
    if ((this.recorder == null || !this.recorder.isRecording ? 0 : (!PhotonNetwork.InRoom ? 1 : 0)) != 0)
      this.recorder.StopRecording();
    if (!RecorderSystemBase.RecorderEnabled || this.recorder == null)
      return;
    this.recorder.Tick();
    if ((!((UnityEngine.Object) NetworkSystem.Instance != (UnityEngine.Object) null) ? 0 : (!this._wiredNetworkCallbacks ? 1 : 0)) == 0)
      return;
    this._wiredNetworkCallbacks = true;
    DelegateListProcessor multiplayerStarted = NetworkSystem.Instance.OnMultiplayerStarted;
    Action action = (Action) (() =>
    {
      if ((!RecorderSystemBase.RecorderEnabled || !RecorderSystemBase.AutoRecordOnMatch ? 0 : (this.recorder != null ? 1 : 0)) == 0)
        return;
      this.recorder.StartRecording();
    });
    ref Action local1 = ref action;
    ((ListProcessor<Action>) multiplayerStarted).Add(ref local1);
    DelegateListProcessor returnedToSinglePlayer = NetworkSystem.Instance.OnReturnedToSinglePlayer;
    action = (Action) (() =>
    {
      if (this.recorder == null)
        return;
      this.recorder.StopRecording();
    });
    ref Action local2 = ref action;
    ((ListProcessor<Action>) returnedToSinglePlayer).Add(ref local2);
  }

  private void OnGUI() => RecorderMenu.Draw();

  public void OnApplicationQuit()
  {
    if (this.recorder == null)
      return;
    this.recorder.Save();
  }

  public void ManualStart()
  {
    if ((!RecorderSystemBase.RecorderEnabled ? 0 : (this.recorder != null ? 1 : 0)) == 0)
      return;
    this.recorder.StartRecording();
  }

  public void ManualStop()
  {
    if (this.recorder == null)
      return;
    this.recorder.StopRecording();
  }

  public void HandleJoiningPlayer(NetPlayer player)
  {
    if ((!RecorderSystemBase.RecorderEnabled ? 0 : (this.recorder != null ? 1 : 0)) == 0)
      return;
    this.recorder.HandleJoiningPlayer(player);
  }

  public void HandleLeavingPlayer(NetPlayer player)
  {
    if ((!RecorderSystemBase.RecorderEnabled ? 0 : (this.recorder != null ? 1 : 0)) == 0)
      return;
    this.recorder.HandleLeavingPlayer(player);
  }
}
