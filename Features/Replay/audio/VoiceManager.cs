using SakuraaCastingMod.Features.Replay.ReplayJson;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Features.Replay.audio;

public class VoiceManager
{
  private GorillaController controller;
  private Dictionary<SakuraaCastingMod.Features.Replay.ReplayJson.Replay, List<Voice>> voicesForReplay = new Dictionary<SakuraaCastingMod.Features.Replay.ReplayJson.Replay, List<Voice>>();
  private MonoBehaviour coroutineRunner;

  public VoiceManager(GorillaController gorillaController, MonoBehaviour runner)
  {
    this.controller = gorillaController;
    this.coroutineRunner = runner;
  }

  public void SpawnReplayVoices(SakuraaCastingMod.Features.Replay.ReplayJson.Replay replay, string replayFolderName)
  {
    if (replay.audioDatas == null)
      return;
    if (!this.voicesForReplay.ContainsKey(replay))
      this.voicesForReplay[replay] = new List<Voice>();
    foreach (AudioDatas audioData in replay.audioDatas)
    {
      Voice voice = new Voice(this.controller, audioData, this.coroutineRunner);
      voice.LoadAudio(Path.Combine(replayFolderName, audioData.FileName));
      this.voicesForReplay[replay].Add(voice);
    }
    UnityEngine.Debug.Log((object) $"[VOICE-MANAGER] Spawned {replay.audioDatas.Count} voice sources");
  }

  public void Update(float currentReplayTime)
  {
    foreach (KeyValuePair<SakuraaCastingMod.Features.Replay.ReplayJson.Replay, List<Voice>> keyValuePair in this.voicesForReplay)
    {
      foreach (Voice voice in keyValuePair.Value)
        voice.Update(currentReplayTime);
    }
  }

  public void StopAllVoices()
  {
    foreach (KeyValuePair<SakuraaCastingMod.Features.Replay.ReplayJson.Replay, List<Voice>> keyValuePair in this.voicesForReplay)
    {
      foreach (Voice voice in keyValuePair.Value)
        voice.Stop();
    }
  }

  public void CleanUp()
  {
    this.StopAllVoices();
    foreach (KeyValuePair<SakuraaCastingMod.Features.Replay.ReplayJson.Replay, List<Voice>> keyValuePair in this.voicesForReplay)
    {
      foreach (Voice voice in keyValuePair.Value)
        voice.Destroy();
    }
    this.voicesForReplay.Clear();
  }
}
