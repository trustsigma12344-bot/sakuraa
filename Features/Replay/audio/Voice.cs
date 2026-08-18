using SakuraaCastingMod.Features.Replay.ReplayJson;
using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

#nullable disable
namespace SakuraaCastingMod.Features.Replay.audio;

public class Voice
{
  private GorillaController controller;
  private AudioDatas myAudio;
  private MonoBehaviour coroutineRunner;
  public bool IsAudioGlobal = false;
  public GameObject audioObject;
  private AudioSource audioSource;
  private AudioClip audioClip;
  private bool hasStarted = false;
  private bool isLoaded = false;
  private float audioStartTime;
  public float VolumeScalar = 1f;

  private bool replayPaused => GorillaController.Paused;

  public Voice(GorillaController gorillaController, AudioDatas audioData, MonoBehaviour runner)
  {
    this.controller = gorillaController;
    this.myAudio = audioData;
    this.coroutineRunner = runner;
    this.audioStartTime = (float) this.myAudio.StartTime / 100f;
    this.CreateAudioObject();
  }

  private void CreateAudioObject()
  {
    this.audioObject = new GameObject($"Voice_{this.myAudio.actorNumberOfSpeaker}");
    this.audioSource = this.audioObject.AddComponent<AudioSource>();
    this.audioSource.playOnAwake = false;
    this.audioSource.loop = false;
    this.audioSource.volume = 1f;
    if (this.myAudio.actorNumberOfSpeaker == -1)
    {
      this.IsAudioGlobal = true;
      this.audioSource.spatialBlend = 0.0f;
    }
    else
    {
      this.audioSource.spatialBlend = 1f;
      this.audioSource.rolloffMode = (AudioRolloffMode) 1;
      this.audioSource.minDistance = 1f;
      this.audioSource.maxDistance = 999f;
    }
  }

  public void LoadAudio(string filePath)
  {
    if (!File.Exists(filePath))
      UnityEngine.Debug.LogWarning((object) ("[VOICE] Audio file not found: " + filePath));
    else
      this.coroutineRunner.StartCoroutine(this.LoadAudioCoroutine(filePath));
  }

  private IEnumerator LoadAudioCoroutine(string filePath)
  {
    string fileUri = "file:///" + filePath.Replace('\\', '/');
    using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(fileUri, (AudioType) 20))
    {
      yield return (object) www.SendWebRequest();
      if (www.result == (UnityWebRequest.Result) 1)
      {
        this.audioClip = DownloadHandlerAudioClip.GetContent(www);
        this.audioSource.clip = this.audioClip;
        this.isLoaded = true;
        UnityEngine.Debug.Log((object) $"[VOICE] Loaded audio: {Path.GetFileName(filePath)}, Duration: {this.audioClip.length}s");
      }
      else
        UnityEngine.Debug.LogError((object) ("[VOICE] Failed to load audio: " + www.error));
    }
  }

  public void Update(float currentReplayTime)
  {
    if ((!this.isLoaded ? 1 : (((UnityEngine.Object) this.audioSource == (UnityEngine.Object) null) ? 1 : 0)) != 0)
      return;
    this.audioSource.mute = VoiceEditManager.IsActorMutedAt(this.myAudio.actorNumberOfSpeaker, currentReplayTime);
    if (!this.replayPaused)
    {
      if ((this.IsAudioGlobal ? 0 : (this.myAudio.actorNumberOfSpeaker != -1 ? 1 : 0)) != 0)
      {
        Gorilla gorillaFromActorNumber = this.controller.GetGorillaFromActorNumber(this.myAudio.actorNumberOfSpeaker);
        if ((gorillaFromActorNumber == null ? 0 : (gorillaFromActorNumber.gorillaRig != null ? 1 : 0)) != 0)
          this.audioObject.transform.position = gorillaFromActorNumber.gorillaRig.Head.transform.position;
      }
      float num = currentReplayTime - this.audioStartTime;
      if (((double) num < 0.0 ? 0 : ((double) num < (double) this.audioClip.length ? 1 : 0)) != 0)
      {
        if (this.hasStarted)
        {
          if (this.audioSource.isPlaying)
          {
            if ((double) Math.Abs(num - this.audioSource.time) <= 0.10000000149011612)
              return;
            this.audioSource.time = num;
          }
          else
          {
            this.audioSource.time = num;
            this.audioSource.Play();
          }
        }
        else
        {
          this.audioSource.time = num;
          this.audioSource.Play();
          this.hasStarted = true;
        }
      }
      else
      {
        if (!this.hasStarted)
          return;
        this.Stop();
      }
    }
    else
    {
      if (!this.audioSource.isPlaying)
        return;
      this.audioSource.Pause();
    }
  }

  public void Stop()
  {
    if (!((UnityEngine.Object) this.audioSource != (UnityEngine.Object) null))
      return;
    this.audioSource.Stop();
    this.hasStarted = false;
  }

  public void Destroy()
  {
    this.Stop();
    if (!((UnityEngine.Object) this.audioObject != (UnityEngine.Object) null))
      return;
    UnityEngine.Object.Destroy((UnityEngine.Object) this.audioObject);
  }
}
