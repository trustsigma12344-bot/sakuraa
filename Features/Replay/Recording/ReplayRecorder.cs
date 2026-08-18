using Photon.Pun;
using SakuraaCastingMod.Core;
using SakuraaCastingMod.Features.Overlays;
using SakuraaCastingMod.Features.Replay.Patches;
using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Features.Replay.Recording;

public class ReplayRecorder
{
  public SakuraaCastingMod.Features.Replay.Recording.Replay replay;
  public bool isRecording = false;
  public int ReplayTime = 0;
  public string SessionFolderName = "";
  private float LastCheckedToAddFrame = 0.0f;
  private float realTimeSinceReplayStart;
  private string roomNameCache = "";
  private List<string> useridsThatjustJoined = new List<string>();
  private GorillaTagManager gtm;
  private readonly CosmeticCollector cosmetics = new CosmeticCollector();
  private readonly TagTracker tags = new TagTracker();

  public void Tick()
  {
    if (!RecorderSystemBase.RecorderEnabled)
    {
      this.AcquireGtm();
    }
    else
    {
      float num = (double) RecorderSystemBase.CaptureHz > 0.0 ? 1f / RecorderSystemBase.CaptureHz : 0.05f;
      if ((!this.isRecording ? 0 : ((double) this.LastCheckedToAddFrame + (double) num < (double) Time.realtimeSinceStartup ? 1 : 0)) != 0)
      {
        this.LastCheckedToAddFrame = Time.realtimeSinceStartup;
        if (this.replay == null)
          this.InitNewReplay();
        this.HandleReplayTime();
        this.HandleAddDataToReplay();
      }
      this.AcquireGtm();
    }
  }

  private void AcquireGtm()
  {
    if (!((UnityEngine.Object) this.gtm == (UnityEngine.Object) null))
      return;
    if ((((UnityEngine.Object) GorillaGameManager.instance == (UnityEngine.Object) null) ? 1 : (GorillaGameManager.instance.GetType() != typeof (GorillaTagManager) ? 1 : 0)) == 0)
    {
      if (!(GorillaGameManager.instance.GetType() == typeof (GorillaTagManager)))
        return;
      this.gtm = GorillaGameManager.instance as GorillaTagManager;
    }
    else
      this.gtm = (GorillaTagManager) null;
  }

  private void HandleReplayTime()
  {
    this.ReplayTime = (int) Math.Round((double) (Time.realtimeSinceStartup - this.realTimeSinceReplayStart) * 100.0 / 5.0) * 5;
  }

  public void StartRecording()
  {
    if (!FeatureToggles.ShowReplaySystem || this.isRecording || ((UnityEngine.Object) NetworkSystem.Instance == (UnityEngine.Object) null) || PhotonNetwork.CurrentRoom == null)
      return;
    AudioRecordingPatch.ClearAllData();
    this.realTimeSinceReplayStart = Time.realtimeSinceStartup;
    this.replay = (SakuraaCastingMod.Features.Replay.Recording.Replay) null;
    this.ReplayTime = 0;
    this.tags.Reset();
    this.SessionFolderName = RecordingPaths.BuildSessionFolderName(NetworkSystem.Instance.RoomName, DateTime.Now);
    this.roomNameCache = PhotonNetwork.CurrentRoom.Name;
    this.isRecording = true;
    Notification.Send("Recording started.", Color.green);
  }

  public void StopRecording()
  {
    if (!this.isRecording)
      return;
    this.isRecording = false;
    this.WriteReplayToFile();
    AudioRecordingPatch.SaveAllRecordings();
    this.replay = (SakuraaCastingMod.Features.Replay.Recording.Replay) null;
    this.ReplayTime = 0;
    this.tags.Reset();
    Notification.Send("Recording saved.", Color.green);
  }

  public void Save()
  {
    if ((!this.isRecording ? 0 : (this.replay != null ? 1 : 0)) == 0)
      return;
    this.WriteReplayToFile();
    AudioRecordingPatch.SaveAllRecordings();
  }

  public void WriteReplayToFile() => ReplayFileWriter.Write(this.replay, this.SessionFolderName);

  public void HandleLeavingPlayer(NetPlayer player)
  {
    if ((this.replay == null ? 1 : (this.replay.players == null ? 1 : 0)) != 0)
      return;
    foreach (Player player1 in this.replay.players)
    {
      if (player1.id == player.UserId)
        player1.LeaveTimes.Add(this.ReplayTime);
    }
  }

  public void HandleJoiningPlayer(NetPlayer player)
  {
    this.useridsThatjustJoined.Add(player.UserId);
  }

  private void InitNewReplay()
  {
    this.replay = new SakuraaCastingMod.Features.Replay.Recording.Replay();
    this.replay.FormatVersion = "v3.1";
  }

  private void HandleAddDataToReplay()
  {
    this.replay.FinalTime = this.ReplayTime;
    if ((!VRRigCache.isInitialized ? 0 : (VRRigCache.ActiveRigContainers != null ? 1 : 0)) != 0)
    {
      foreach (RigContainer activeRigContainer in (IEnumerable<RigContainer>) VRRigCache.ActiveRigContainers)
      {
        if (!((UnityEngine.Object) activeRigContainer == (UnityEngine.Object) null))
        {
          VRRig rig = activeRigContainer.Rig;
          NetPlayer creator = activeRigContainer.Creator;
          if ((((UnityEngine.Object) rig == (UnityEngine.Object) null) ? 1 : (creator == null ? 1 : 0)) == 0)
          {
            this.AddPlayerInfoInit(rig, creator);
            this.AddPlayerDataInit(rig, creator);
            MovementCollector.Capture(this.replay, rig, creator);
            this.cosmetics.Store(this.replay, rig, creator, this.ReplayTime);
          }
        }
      }
    }
    this.tags.Update(this.replay, this.gtm, this.ReplayTime);
  }

  private void AddPlayerDataInit(VRRig rig, NetPlayer netPlayer)
  {
    if (this.replay.playerDatas == null)
      this.replay.playerDatas = new List<PlayerData>();
    foreach (PlayerData playerData in this.replay.playerDatas)
    {
      if (playerData.actorNumber == netPlayer.ActorNumber)
        return;
    }
    this.replay.playerDatas.Add(new PlayerData()
    {
      actorNumber = netPlayer.ActorNumber,
      tagstimes = new List<int>(),
      movementData = new MovementData()
      {
        headPositions = new List<float>(),
        leftHandPositions = new List<float>(),
        rightHandPositions = new List<float>(),
        headRot = new List<float>(),
        leftHandRot = new List<float>(),
        rightHandRot = new List<float>()
      },
      cosmeticsDatas = new List<CosmeticsData>()
    });
  }

  private void AddPlayerInfoInit(VRRig rig, NetPlayer netPlayer)
  {
    if (this.replay.players == null)
      this.replay.players = new List<Player>();
    foreach (Player player in this.replay.players)
    {
      if (player.id == netPlayer.UserId)
      {
        player.color = new List<float>()
        {
          rig.playerColor.r,
          rig.playerColor.g,
          rig.playerColor.b
        };
        return;
      }
    }
    this.replay.players.Add(new Player()
    {
      id = netPlayer.UserId,
      actornumber = netPlayer.ActorNumber,
      color = new List<float>()
      {
        rig.playerColor.r,
        rig.playerColor.g,
        rig.playerColor.b
      },
      Name = netPlayer.NickName,
      JoinTimes = new List<int>() { this.ReplayTime },
      LeaveTimes = new List<int>()
    });
  }
}
