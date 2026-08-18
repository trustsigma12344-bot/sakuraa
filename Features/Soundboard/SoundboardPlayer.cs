using SakMerge.Api;
using SakuraaCastingMod.Core;
using SakuraaCastingMod.Shared.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

#nullable disable
namespace SakuraaCastingMod.Features.Soundboard;

public static class SoundboardPlayer
{
  [SavedSetting("SoundboardVolume", 0.7f)]
  public static float Volume = 0.7f;
  private static AudioSource _monitor;
  private static Coroutine _playRoutine;
  private const int CacheCap = 8;
  private static readonly Dictionary<string, AudioClip> _cache = new Dictionary<string, AudioClip>();
  private static readonly LinkedList<string> _lru = new LinkedList<string>();
  private static readonly List<AudioClip> _orphans = new List<AudioClip>();

  public static string CurrentSoundId { get; private set; }

  static SoundboardPlayer()
  {
    SoundboardManager.OnManifestChanged += new Action(SoundboardPlayer.FlushCache);
  }

  public static void Play(SoundboardManager.Sound sound)
  {
    if (sound == null)
      return;
    MonoBehaviour ins = (MonoBehaviour) Plugin.Ins;
    if (((UnityEngine.Object) ins == (UnityEngine.Object) null))
      return;
    SoundboardPlayer.Stop();
    SoundboardPlayer._playRoutine = ins.StartCoroutine(SoundboardPlayer.PlayRoutine(sound));
  }

  public static void Stop()
  {
    if ((SoundboardPlayer._playRoutine == null ? 0 : (((UnityEngine.Object) Plugin.Ins != (UnityEngine.Object) null) ? 1 : 0)) != 0)
      Plugin.Ins.StopCoroutine(SoundboardPlayer._playRoutine);
    SoundboardPlayer._playRoutine = (Coroutine) null;
    SoundboardPlayer.CurrentSoundId = (string) null;
    if (((UnityEngine.Object) SoundboardPlayer._monitor != (UnityEngine.Object) null))
      SoundboardPlayer._monitor.Stop();
    Voice.Stop();
    SoundboardPlayer.DestroyOrphans();
  }

  private static IEnumerator PlayRoutine(SoundboardManager.Sound sound)
  {
    AudioClip clip;
    if ((!SoundboardPlayer._cache.TryGetValue(sound.Id, out clip) ? 1 : (((UnityEngine.Object) clip == (UnityEngine.Object) null) ? 1 : 0)) == 0)
    {
      SoundboardPlayer._lru.Remove(sound.Id);
      SoundboardPlayer._lru.AddFirst(sound.Id);
    }
    else
    {
      string uri = "file:///" + sound.FilePath.Replace('\\', '/');
      using (UnityWebRequest req = UnityWebRequestMultimedia.GetAudioClip(uri, (AudioType) 20))
      {
        yield return (object) req.SendWebRequest();
        if (req.result == (UnityWebRequest.Result) 1)
        {
          clip = DownloadHandlerAudioClip.GetContent(req);
        }
        else
        {
          UnityEngine.Debug.Log((object) $"[Soundboard] '{sound.Name}' failed to load (deleted in loader?), skipping");
          SoundboardPlayer._playRoutine = (Coroutine) null;
          yield break;
        }
      }
      if (((UnityEngine.Object) clip == (UnityEngine.Object) null))
      {
        SoundboardPlayer._playRoutine = (Coroutine) null;
        yield break;
      }
      SoundboardPlayer.CacheAdd(sound.Id, clip);
      uri = (string) null;
    }
    SoundboardPlayer.EnsureMonitor();
    float vol = Mathf.Clamp01(SoundboardPlayer.Volume);
    if (((UnityEngine.Object) SoundboardPlayer._monitor != (UnityEngine.Object) null))
      SoundboardPlayer._monitor.PlayOneShot(clip, vol);
    Voice.Play(clip, vol);
    SoundboardPlayer.CurrentSoundId = sound.Id;
    float timeout = (float) ((double) Time.realtimeSinceStartup + (double) clip.length + 1.0);
    while ((!Voice.IsPlaying ? 0 : ((double) Time.realtimeSinceStartup < (double) timeout ? 1 : 0)) != 0)
      yield return (object) null;
    SoundboardPlayer.CurrentSoundId = (string) null;
    SoundboardPlayer._playRoutine = (Coroutine) null;
    SoundboardPlayer.DestroyOrphans();
  }

  private static void EnsureMonitor()
  {
    if ((((UnityEngine.Object) SoundboardPlayer._monitor != (UnityEngine.Object) null) ? 1 : (((UnityEngine.Object) Plugin.Ins == (UnityEngine.Object) null) ? 1 : 0)) != 0)
      return;
    SoundboardPlayer._monitor = ((Component) Plugin.Ins).gameObject.AddComponent<AudioSource>();
    SoundboardPlayer._monitor.spatialBlend = 0.0f;
  }

  private static void CacheAdd(string id, AudioClip clip)
  {
    AudioClip clip1;
    if ((!SoundboardPlayer._cache.TryGetValue(id, out clip1) || !((UnityEngine.Object) clip1 != (UnityEngine.Object) null) ? 0 : (((UnityEngine.Object) clip1 != (UnityEngine.Object) clip) ? 1 : 0)) != 0)
      SoundboardPlayer.RetireClip(clip1, id);
    SoundboardPlayer._cache[id] = clip;
    SoundboardPlayer._lru.Remove(id);
    SoundboardPlayer._lru.AddFirst(id);
    while (SoundboardPlayer._lru.Count > 8)
    {
      string str = SoundboardPlayer._lru.Last.Value;
      SoundboardPlayer._lru.RemoveLast();
      AudioClip clip2;
      if (SoundboardPlayer._cache.TryGetValue(str, out clip2))
      {
        SoundboardPlayer._cache.Remove(str);
        SoundboardPlayer.RetireClip(clip2, str);
      }
    }
  }

  private static void FlushCache()
  {
    foreach (KeyValuePair<string, AudioClip> keyValuePair in SoundboardPlayer._cache)
      SoundboardPlayer.RetireClip(keyValuePair.Value, keyValuePair.Key);
    SoundboardPlayer._cache.Clear();
    SoundboardPlayer._lru.Clear();
  }

  private static void RetireClip(AudioClip clip, string id)
  {
    if (((UnityEngine.Object) clip == (UnityEngine.Object) null))
      return;
    if ((id == null ? 0 : (id == SoundboardPlayer.CurrentSoundId ? 1 : 0)) == 0)
      UnityEngine.Object.Destroy((UnityEngine.Object) clip);
    else
      SoundboardPlayer._orphans.Add(clip);
  }

  private static void DestroyOrphans()
  {
    if ((SoundboardPlayer._orphans.Count == 0 ? 1 : (SoundboardPlayer.CurrentSoundId != null ? 1 : 0)) != 0)
      return;
    foreach (AudioClip orphan in SoundboardPlayer._orphans)
    {
      if (((UnityEngine.Object) orphan != (UnityEngine.Object) null))
        UnityEngine.Object.Destroy((UnityEngine.Object) orphan);
    }
    SoundboardPlayer._orphans.Clear();
  }
}
