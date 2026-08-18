using HarmonyLib;
using Photon.Voice.Unity;
using SakuraaCastingMod.Features.Replay.Recording;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Features.Replay.Patches;

internal class AudioRecordingPatch
{
  public static Dictionary<UnityAudioOut, AudioRecordingData> audioRecordingData = new Dictionary<UnityAudioOut, AudioRecordingData>();
  public static Dictionary<UnityAudioOut, CachedSpeakerData> speakerDataCache = new Dictionary<UnityAudioOut, CachedSpeakerData>();
  public static Dictionary<string, WavWriter> userAudioWriters = new Dictionary<string, WavWriter>();
  private static readonly FieldInfo SourceField = AccessTools.Field(typeof (UnityAudioOut), "source");
  private static readonly FieldInfo ClipField = AccessTools.Field(typeof (UnityAudioOut), "clip");
  private static readonly FieldInfo SpeakerAudioOutputField = AccessTools.Field(typeof (Speaker), "audioOutput");

  public static void Prefix(UnityAudioOut __instance, float[] data, int offsetSamples)
  {
    if ((!RecorderSystemBase.RecorderEnabled ? 1 : (!RecorderSystemBase.RecordVoice ? 1 : 0)) != 0 || (((UnityEngine.Object) RecorderSystemBase.Instance == (UnityEngine.Object) null) ? 1 : (!RecorderSystemBase.Instance.IsRecording ? 1 : 0)) != 0)
      return;
    ReplayRecorder recorder = RecorderSystemBase.Instance.recorder;
    if ((recorder == null ? 1 : (recorder.replay == null ? 1 : 0)) != 0 || (AudioRecordingPatch.SourceField == (FieldInfo) null ? 1 : (AudioRecordingPatch.ClipField == (FieldInfo) null ? 1 : 0)) != 0)
      return;
    AudioSource audioSource = AudioRecordingPatch.SourceField.GetValue((object) __instance) as AudioSource;
    if (((UnityEngine.Object) audioSource == (UnityEngine.Object) null))
      return;
    AudioClip audioClip = AudioRecordingPatch.ClipField.GetValue((object) __instance) as AudioClip;
    if (((UnityEngine.Object) audioClip == (UnityEngine.Object) null))
      return;
    int frequency = audioClip.frequency;
    int channels = audioClip.channels;
    int samples = audioClip.samples;
    if ((frequency <= 0 || channels <= 0 ? 1 : (samples <= 0 ? 1 : 0)) != 0 || !AudioRecordingPatch.speakerDataCache.ContainsKey(__instance) && !AudioRecordingPatch.CacheSpeakerData(__instance))
      return;
    CachedSpeakerData cachedSpeakerData = AudioRecordingPatch.speakerDataCache[__instance];
    string userKey = cachedSpeakerData.UserKey;
    if (!AudioRecordingPatch.audioRecordingData.ContainsKey(__instance))
    {
      AudioRecordingData audioRecordingData = new AudioRecordingData()
      {
        startTime = Time.realtimeSinceStartup,
        bufferSize = samples
      };
      audioRecordingData.tempReadBuffer = new float[audioRecordingData.readAheadSamples * channels];
      if (recorder.replay.audioDatas == null)
        recorder.replay.audioDatas = new List<AudioDatas>();
      recorder.replay.audioDatas.Add(new AudioDatas()
      {
        actorNumberOfSpeaker = cachedSpeakerData.ActorNumber,
        StartTime = recorder.ReplayTime,
        FileName = userKey + ".wav"
      });
      AudioRecordingPatch.audioRecordingData[__instance] = audioRecordingData;
    }
    AudioRecordingData audioRecordingData1 = AudioRecordingPatch.audioRecordingData[__instance];
    int timeSamples = audioSource.timeSamples;
    if (audioRecordingData1.lastPlayhead != -1 && (timeSamples >= audioRecordingData1.lastPlayhead ? 0 : (audioRecordingData1.lastPlayhead > audioRecordingData1.bufferSize - 2000 ? 1 : 0)) != 0)
      ++audioRecordingData1.totalWraps;
    int readAheadSamples = audioRecordingData1.readAheadSamples;
    int num1 = timeSamples;
    if ((num1 < 0 ? 1 : (num1 >= samples ? 1 : 0)) != 0)
      return;
    int length = readAheadSamples * channels;
    if ((audioRecordingData1.tempReadBuffer == null ? 1 : (audioRecordingData1.tempReadBuffer.Length < length ? 1 : 0)) != 0)
      audioRecordingData1.tempReadBuffer = new float[length];
    if (num1 + readAheadSamples > samples)
    {
      int num2 = samples - num1;
      int num3 = readAheadSamples - num2;
      if ((num2 <= 0 ? 1 : (num3 <= 0 ? 1 : 0)) != 0)
        return;
      float[] sourceArray1 = new float[num2 * channels];
      audioClip.GetData(sourceArray1, num1);
      float[] sourceArray2 = new float[num3 * channels];
      audioClip.GetData(sourceArray2, 0);
      if (sourceArray1.Length + sourceArray2.Length > audioRecordingData1.tempReadBuffer.Length)
        return;
      Array.Copy((Array) sourceArray1, 0, (Array) audioRecordingData1.tempReadBuffer, 0, sourceArray1.Length);
      Array.Copy((Array) sourceArray2, 0, (Array) audioRecordingData1.tempReadBuffer, sourceArray1.Length, sourceArray2.Length);
    }
    else
      audioClip.GetData(audioRecordingData1.tempReadBuffer, num1);
    int num4 = audioRecordingData1.totalWraps * audioRecordingData1.bufferSize + timeSamples;
    int num5 = num4 + readAheadSamples;
    while (audioRecordingData1.linearBuffer.Count < num5)
      audioRecordingData1.linearBuffer.Add(0.0f);
    for (int index1 = 0; index1 < readAheadSamples; ++index1)
    {
      int index2 = num4 + index1;
      int index3 = index1 * channels;
      if (index3 < audioRecordingData1.tempReadBuffer.Length)
      {
        if (index2 < audioRecordingData1.linearBuffer.Count)
          audioRecordingData1.linearBuffer[index2] = audioRecordingData1.tempReadBuffer[index3];
      }
      else
        break;
    }
    audioRecordingData1.highestLinearPosition = Math.Max(audioRecordingData1.highestLinearPosition, num4 + readAheadSamples - 1);
    ++audioRecordingData1.updateCount;
    audioRecordingData1.lastPlayhead = timeSamples;
    if (AudioRecordingPatch.userAudioWriters.ContainsKey(userKey))
      return;
    string audioFilePath = RecordingPaths.GetAudioFilePath(RecorderSystemBase.Instance.ReplayContainerFolder, userKey);
    AudioRecordingPatch.userAudioWriters[userKey] = new WavWriter(audioFilePath, frequency);
  }

  private static bool CacheSpeakerData(UnityAudioOut audioOut)
  {
    bool flag;
    if (AudioRecordingPatch.SpeakerAudioOutputField == (FieldInfo) null)
    {
      flag = false;
    }
    else
    {
      foreach (Speaker speaker in UnityEngine.Object.FindObjectsOfType<Speaker>())
      {
        if (!((UnityEngine.Object) speaker == (UnityEngine.Object) null) && AudioRecordingPatch.SpeakerAudioOutputField.GetValue((object) speaker) is UnityAudioOut unityAudioOut && (audioOut != unityAudioOut ? 0 : (speaker.Actor != null ? 1 : 0)) != 0)
        {
          CachedSpeakerData cachedSpeakerData = new CachedSpeakerData()
          {
            ActorNumber = speaker.Actor.ActorNumber,
            UserId = speaker.Actor.UserId,
            UserKey = speaker.Actor.ActorNumber.ToString(),
            NickName = speaker.Actor.NickName
          };
          AudioRecordingPatch.speakerDataCache[audioOut] = cachedSpeakerData;
          flag = true;
          goto label_8;
        }
      }
      flag = false;
    }
label_8:
    return flag;
  }

  public static void SaveAllRecordings()
  {
    if (AudioRecordingPatch.audioRecordingData == null)
      return;
    foreach (KeyValuePair<UnityAudioOut, AudioRecordingData> keyValuePair in AudioRecordingPatch.audioRecordingData)
    {
      UnityAudioOut key = keyValuePair.Key;
      AudioRecordingData audioRecordingData = keyValuePair.Value;
      if (AudioRecordingPatch.speakerDataCache.ContainsKey(key))
      {
        string userKey = AudioRecordingPatch.speakerDataCache[key].UserKey;
        if (AudioRecordingPatch.userAudioWriters.ContainsKey(userKey))
        {
          WavWriter userAudioWriter = AudioRecordingPatch.userAudioWriters[userKey];
          if (userAudioWriter != null)
          {
            int num = Math.Min(audioRecordingData.highestLinearPosition + 1, audioRecordingData.linearBuffer.Count);
            for (int index = 0; index < num; ++index)
              userAudioWriter.WriteSample(audioRecordingData.linearBuffer[index]);
          }
        }
      }
    }
    foreach (WavWriter wavWriter in AudioRecordingPatch.userAudioWriters.Values)
      wavWriter?.Dispose();
    AudioRecordingPatch.audioRecordingData.Clear();
    AudioRecordingPatch.userAudioWriters.Clear();
    AudioRecordingPatch.speakerDataCache.Clear();
  }

  public static void ClearAllData()
  {
    foreach (WavWriter wavWriter in AudioRecordingPatch.userAudioWriters.Values)
      wavWriter?.Dispose();
    AudioRecordingPatch.audioRecordingData.Clear();
    AudioRecordingPatch.userAudioWriters.Clear();
    AudioRecordingPatch.speakerDataCache.Clear();
  }
}
