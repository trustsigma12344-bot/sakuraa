using SakuraaCastingMod.Features.Replay.audio;
using SakuraaCastingMod.Features.Replay.compatability;
using SakuraaCastingMod.Features.Replay.EditJson;
using SakuraaCastingMod.Features.Replay.ReplayJson;
using SakuraaCastingMod.Features.Replay.ReplayManagers;
using SakuraaCastingMod.Features.Replay.UI;
using SakuraaCastingMod.Shared.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Features.Replay;

public class GorillaController
{
  private bool _isLoading;
  public List<Gorilla> Gorillas = new List<Gorilla>();
  public float CurrentTime;
  private float? LastCheckTime = new float?();
  public float SlowValue = 1f;
  public static bool Paused = true;
  public List<List<Gorilla>> GorillasByReplay = new List<List<Gorilla>>();
  private VoiceManager voiceManager;

  public GorillaController()
  {
    this.voiceManager = new VoiceManager(this, (MonoBehaviour) ReplaySystemBase.Instance);
  }

  private void ApplyLoadedProject()
  {
    if (this.replayProject == null)
      return;
    this.ClearGorillas();
    this.GorillasByReplay.Clear();
    this.SetupGorillas();
    this.ExposeGorillasToPublic();
    this.StartReplay();
  }

  public void LoadReplayFromPath(string filePath) => this.BeginLoad(filePath, false);

  private void BeginLoad(string filePath, bool fromDialog)
  {
    if (this._isLoading)
      return;
    ReplaySystemBase instance = ReplaySystemBase.Instance;
    if (!((UnityEngine.Object) instance == (UnityEngine.Object) null))
    {
      instance.StartCoroutine(this.LoadReplayRoutine(filePath, fromDialog));
    }
    else
    {
      if (!fromDialog)
        ReplayManager.LoadFromPath(filePath);
      else
        ReplayManager.LoadFile();
      this.ApplyLoadedProject();
    }
  }

  private IEnumerator LoadReplayRoutine(string filePath, bool fromDialog)
  {
    this._isLoading = true;
    ReplayLoadingOverlay.Begin();
    yield return (object) null;
    if (ReplayManager.BlockedByRoom())
    {
      ReplayLoadingOverlay.Cancel();
      this._isLoading = false;
    }
    else
    {
      if (fromDialog)
      {
        ReplayLoadingOverlay.SetStage("Choosing file…");
        yield return (object) null;
        filePath = FileSelector.GetFilePathFromUser();
      }
      if (string.IsNullOrEmpty(filePath))
      {
        ReplayLoadingOverlay.Cancel();
        this._isLoading = false;
      }
      else
      {
        ReplayLoadingOverlay.SetStage("Reading replay…");
        ReplayLoadingOverlay.SetIndeterminate(true);
        ReplayProject parsed = (ReplayProject) null;
        bool wasProjectFile = false;
        Exception parseError = (Exception) null;
        string path = filePath;
        Task task = Task.Run((Action) (() =>
        {
          try
          {
            parsed = ReplayManager.ParseProjectFromPath(path, out wasProjectFile);
          }
          catch (Exception ex)
          {
            parseError = ex;
          }
        }));
        while (!task.IsCompleted)
          yield return (object) null;
        if ((parseError != null ? 1 : (parsed == null ? 1 : 0)) != 0)
        {
          UnityEngine.Debug.LogError((object) ("[Replay] Failed to read replay: " + parseError?.ToString()));
          ReplayLoadingOverlay.Fail("Couldn't read replay file");
          this._isLoading = false;
        }
        else
        {
          ReplayManager.ApplyParsed(parsed, wasProjectFile);
          ReplayLoadingOverlay.SetIndeterminate(false);
          ReplayLoadingOverlay.SetStage("Building players…");
          ReplayLoadingOverlay.SetProgress(0.2f);
          this.StopReplay();
          this.ClearGorillas();
          this.GorillasByReplay.Clear();
          yield return (object) null;
          ReplayProject proj = this.replayProject;
          if ((proj == null ? 0 : (proj.replayInfos != null ? 1 : 0)) != 0)
          {
            int total = Mathf.Max(1, proj.replayInfos.Count);
            int done = 0;
            foreach (ReplayInfo replayInfo in proj.replayInfos)
            {
              this.BuildGorillasForReplay(replayInfo);
              ++done;
              ReplayLoadingOverlay.SetProgress((float) (0.20000000298023224 + 0.699999988079071 * ((double) done / (double) total)));
              yield return (object) null;
            }
          }
          ReplayLoadingOverlay.SetStage("Loading audio…");
          ReplayLoadingOverlay.SetProgress(0.96f);
          this.ExposeGorillasToPublic();
          this.SpawnAllVoices();
          this.StartReplay();
          ReplayLoadingOverlay.Finish();
          this._isLoading = false;
        }
      }
    }
  }

  public ReplayProject replayProject => ReplayManager.replayProject;

  public void Update()
  {
    if (this._isLoading)
      return;
    if ((!this.LastCheckTime.HasValue ? 1 : ((double) this.CurrentTime < 0.0 ? 1 : 0)) != 0)
      this.LastCheckTime = new float?(Time.realtimeSinceStartup);
    this.HandleTimeCalc();
    if ((double) this.CurrentTime >= 0.0)
    {
      this.UpdateGorillas();
      if (this.voiceManager != null)
        this.voiceManager.Update(this.GetCurrentRealTime());
    }
    this.ExposeGorillasToPublic();
  }

  public void HandleTimeCalc()
  {
    if ((GorillaController.Paused ? 1 : ((double) this.CurrentTime > (double) ReplayManager.FindGreatestEndTime() ? 1 : 0)) != 0)
    {
      this.LastCheckTime = new float?(Time.realtimeSinceStartup);
    }
    else
    {
      this.CurrentTime += (Time.realtimeSinceStartup - this.LastCheckTime.Value) * SpeedManager.GetCurrentSpeed(this.CurrentTime) * this.SlowValue;
      this.LastCheckTime = new float?(Time.realtimeSinceStartup);
    }
  }

  public void ReloadFile() => this.BeginLoad((string) null, true);

  public void ExposeGorillasToPublic()
  {
    ReplayModExposer.exposedRigs.Clear();
    GorillaRigExposer.exposers.Clear();
    foreach (Gorilla gorilla in this.Gorillas)
      ReplayModExposer.exposedRigs.Add((GorillaRigExposed) new GorillaRigExposer(gorilla.gorillaRig, gorilla.IsTagged));
  }

  public void StartReplay() => this.CurrentTime = 0.0f;

  public void StopReplay()
  {
    this.CurrentTime = -1f;
    if (this.voiceManager == null)
      return;
    this.voiceManager.StopAllVoices();
  }

  public float GetCurrentRealTime() => (double) this.CurrentTime >= 0.0 ? this.CurrentTime : 0.0f;

  private void ClearGorillas()
  {
    foreach (Gorilla gorilla in this.Gorillas)
      gorilla.ActivateSelf();
    if (this.voiceManager != null)
      this.voiceManager.CleanUp();
    this.Gorillas.Clear();
  }

  public static string GetReplayFilePath() => FileSelector.GetFilePathFromUser();

  private string GetRawFileString() => File.ReadAllText(GorillaController.GetReplayFilePath());

  private void ExpandIkLimit(int newLimit)
  {
    object[] parameters = new object[3]
    {
      (object) newLimit,
      (object) (Allocator) 4,
      (object) (NativeArrayOptions) 1
    };
    Type nestedType = GorillaIKMgr.Instance.GetType().GetNestedType("IKJob", BindingFlags.NonPublic);
    object obj1 = typeof (GorillaIKMgr).GetField("job", BindingFlags.Instance | BindingFlags.NonPublic).GetValue((object) GorillaIKMgr.Instance);
    FieldInfo field1 = nestedType.GetField("input", BindingFlags.Instance | BindingFlags.Public);
    object obj2 = typeof (NativeArray<>).MakeGenericType(GorillaIKMgr.Instance.GetType().GetNestedType("IKInput", BindingFlags.NonPublic)).GetConstructor(new Type[3]
    {
      typeof (int),
      typeof (Allocator),
      typeof (NativeArrayOptions)
    }).Invoke(parameters);
    field1.SetValue(obj1, obj2);
    FieldInfo field2 = nestedType.GetField("output", BindingFlags.Instance | BindingFlags.Public);
    object obj3 = typeof (NativeArray<>).MakeGenericType(GorillaIKMgr.Instance.GetType().GetNestedType("IKOutput", BindingFlags.NonPublic)).GetConstructor(new Type[3]
    {
      typeof (int),
      typeof (Allocator),
      typeof (NativeArrayOptions)
    }).Invoke(parameters);
    field2.SetValue(obj1, obj3);
  }

  private List<Vector3>[] GetPosLists(PlayerData playerData)
  {
    List<Vector3>[] posLists = new List<Vector3>[3];
    List<float> leftHandPositions = playerData.movementData.leftHandPositions;
    List<float> rightHandPositions = playerData.movementData.rightHandPositions;
    List<float> headPositions = playerData.movementData.headPositions;
    List<Vector3> vector3List1 = new List<Vector3>(leftHandPositions.Count / 3);
    List<Vector3> vector3List2 = new List<Vector3>(rightHandPositions.Count / 3);
    List<Vector3> vector3List3 = new List<Vector3>(headPositions.Count / 3);
    for (int index = 0; index + 2 < leftHandPositions.Count; index += 3)
      vector3List1.Add(new Vector3(-leftHandPositions[index], leftHandPositions[index + 1], leftHandPositions[index + 2]));
    for (int index = 0; index + 2 < rightHandPositions.Count; index += 3)
      vector3List2.Add(new Vector3(-rightHandPositions[index], rightHandPositions[index + 1], rightHandPositions[index + 2]));
    for (int index = 0; index + 2 < headPositions.Count; index += 3)
      vector3List3.Add(new Vector3(-headPositions[index], headPositions[index + 1], headPositions[index + 2]));
    posLists[0] = vector3List1;
    posLists[1] = vector3List2;
    posLists[2] = vector3List3;
    return posLists;
  }

  private List<Vector3>[] GetRotLists(PlayerData playerData)
  {
    List<Vector3>[] rotLists = new List<Vector3>[3];
    List<float> leftHandRot = playerData.movementData.leftHandRot;
    List<float> rightHandRot = playerData.movementData.rightHandRot;
    List<float> headRot = playerData.movementData.headRot;
    List<Vector3> vector3List1 = new List<Vector3>(leftHandRot.Count / 4);
    List<Vector3> vector3List2 = new List<Vector3>(rightHandRot.Count / 4);
    List<Vector3> vector3List3 = new List<Vector3>(headRot.Count / 4);
    for (int index = 0; index + 3 < leftHandRot.Count; index += 4)
    {
      List<Vector3> vector3List4 = vector3List1;
      Quaternion quaternion = new Quaternion(leftHandRot[index], leftHandRot[index + 1], leftHandRot[index + 2], leftHandRot[index + 3]);
      Vector3 eulerAngles = quaternion.eulerAngles;
      vector3List4.Add(eulerAngles);
    }
    for (int index = 0; index + 3 < rightHandRot.Count; index += 4)
    {
      List<Vector3> vector3List5 = vector3List2;
      Quaternion quaternion = new Quaternion(rightHandRot[index], rightHandRot[index + 1], rightHandRot[index + 2], rightHandRot[index + 3]);
      Vector3 eulerAngles = quaternion.eulerAngles;
      vector3List5.Add(eulerAngles);
    }
    Quaternion quaternion1 = Quaternion.AngleAxis(90f, Vector3.right);
    for (int index = 0; index + 3 < headRot.Count; index += 4)
    {
      Quaternion quaternion2 = (new Quaternion(headRot[index], headRot[index + 1], headRot[index + 2], headRot[index + 3]) * quaternion1);
      Vector3 eulerAngles = quaternion2.eulerAngles;
      eulerAngles.y = -eulerAngles.y;
      vector3List3.Add(eulerAngles);
    }
    rotLists[0] = vector3List1;
    rotLists[1] = vector3List2;
    rotLists[2] = vector3List3;
    return rotLists;
  }

  private float GetGorillaStartTime(PlayerData data, ReplayInfo replay)
  {
    UnityEngine.Debug.Log((object) ("Start AC: " + data.actorNumber.ToString()));
    return (float) ReplayManager.GetPlayerInfoFromData(data, replay.replay).JoinTimes[0];
  }

  private float GetGorillaEndTime(PlayerData data, ReplayInfo replay)
  {
    Player playerInfoFromData = ReplayManager.GetPlayerInfoFromData(data, replay.replay);
    return playerInfoFromData.LeaveTimes.Count > 0 ? (float) playerInfoFromData.LeaveTimes[0] : (float) replay.replay.FinalTime;
  }

  private List<float> GetGorillaColor(PlayerData data, ReplayInfo replay)
  {
    return ReplayManager.GetPlayerInfoFromData(data, replay.replay).color;
  }

  public Gorilla GetGorillaFromID(int ac)
  {
    Gorilla gorillaFromId;
    foreach (Gorilla gorilla in this.Gorillas)
    {
      if (gorilla.ID == ac.ToString())
      {
        gorillaFromId = gorilla;
        goto label_7;
      }
    }
    gorillaFromId = (Gorilla) null;
label_7:
    return gorillaFromId;
  }

  public Gorilla GetGorillaFromActorNumber(int actorNumber)
  {
    Gorilla gorillaFromActorNumber;
    foreach (Gorilla gorilla in this.Gorillas)
    {
      if (gorilla.actorNumber == actorNumber)
      {
        gorillaFromActorNumber = gorilla;
        goto label_7;
      }
    }
    gorillaFromActorNumber = (Gorilla) null;
label_7:
    return gorillaFromActorNumber;
  }

  private List<CosmeticsData> GetCosmeticsDatas(PlayerData data) => data.cosmeticsDatas;

  private string GetPlayerName(PlayerData data, ReplayInfo replay)
  {
    return ReplayManager.GetPlayerInfoFromData(data, replay.replay).Name;
  }

  private void UpdateGorillas()
  {
    foreach (Gorilla gorilla in this.Gorillas)
      gorilla.Update(this.GetCurrentRealTime());
  }

  private void SetupGorillas()
  {
    if (this.replayProject == null)
      return;
    foreach (ReplayInfo replayInfo in this.replayProject.replayInfos)
      this.BuildGorillasForReplay(replayInfo);
    this.SpawnAllVoices();
  }

  private void BuildGorillasForReplay(ReplayInfo replayInfo)
  {
    List<Gorilla> gorillaList = new List<Gorilla>();
    SakuraaCastingMod.Features.Replay.ReplayJson.Replay replay = replayInfo.replay;
    foreach (PlayerData playerData in replay.playerDatas)
    {
      if (playerData.actorNumber != -1)
      {
        string str = playerData.actorNumber.ToString() + replay.ID;
        if (this.GetGorillaFromID(int.Parse(str)) == null)
        {
          Player playerInfoFromData = ReplayManager.GetPlayerInfoFromData(playerData, replayInfo.replay);
          if (playerInfoFromData != null)
          {
            List<Vector3>[] posLists = this.GetPosLists(playerData);
            List<Vector3>[] rotLists = this.GetRotLists(playerData);
            float joinTime = (float) playerInfoFromData.JoinTimes[0];
            float EndTime = playerInfoFromData.LeaveTimes.Count > 0 ? (float) playerInfoFromData.LeaveTimes[0] : (float) replayInfo.replay.FinalTime;
            Gorilla gorilla = new Gorilla(str, joinTime, EndTime, posLists[1], posLists[0], posLists[2], rotLists[1], rotLists[0], rotLists[2], playerData.tagstimes, playerInfoFromData.color, playerData.cosmeticsDatas, replayInfo.StartOffsetTime, playerData.actorNumber, playerInfoFromData.Name);
            this.Gorillas.Add(gorilla);
            gorillaList.Add(gorilla);
          }
        }
      }
    }
    this.GorillasByReplay.Add(gorillaList);
  }

  private void SpawnAllVoices()
  {
    if (this.replayProject == null)
      return;
    foreach (ReplayInfo replayInfo in this.replayProject.replayInfos)
      this.voiceManager.SpawnReplayVoices(replayInfo.replay, replayInfo.ReplayFolderName);
  }
}
