using SakMerge.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

#nullable disable
namespace SakuraaCastingMod.Features.AutoRef;

public class RefVoiceManager : MonoBehaviour
{
  public static RefVoiceManager Instance;
  private readonly Queue<string> _messageQueue = new Queue<string>();
  private const string TtsService = "Streamlabs";
  private const string TtsVoice = "Brian";
  private string _ttsCachePath;
  private AudioSource _localAudioSource;

  public bool IsSpeaking { get; private set; }

  private void Awake()
  {
    RefVoiceManager.Instance = this;
    this._ttsCachePath = Path.Combine(Application.persistentDataPath, "SakuraaAutoRef", "TTSCache");
    if (!Directory.Exists(this._ttsCachePath))
      Directory.CreateDirectory(this._ttsCachePath);
    this._localAudioSource = ((Component) this).gameObject.AddComponent<AudioSource>();
    this._localAudioSource.spatialBlend = 0.0f;
  }

  public void ResetState()
  {
    this.StopAllCoroutines();
    this._messageQueue.Clear();
    this.IsSpeaking = false;
    Voice.Stop();
  }

  public void PreCache(string text)
  {
    if (string.IsNullOrEmpty(text))
      return;
    string str = Path.Combine(this._ttsCachePath, RefVoiceManager.Hash(text + "Brian") + ".mp3");
    if (File.Exists(str))
      return;
    this.StartCoroutine(this.DownloadTTS(text, str, false));
  }

  public void Speak(string text)
  {
    if (string.IsNullOrEmpty(text) || this._messageQueue.Contains(text))
      return;
    this._messageQueue.Enqueue(text);
    if (this.IsSpeaking)
      return;
    this.StartCoroutine(this.ProcessQueue());
  }

  private IEnumerator ProcessQueue()
  {
    this.IsSpeaking = true;
    while (this._messageQueue.Count > 0)
    {
      string text = this._messageQueue.Dequeue();
      yield return (object) this.StartCoroutine(this.GenerateAndPlayTTS(text));
      yield return (object) new WaitForSeconds(0.25f);
      text = (string) null;
    }
    this.IsSpeaking = false;
  }

  private IEnumerator GenerateAndPlayTTS(string text)
  {
    string filePath = Path.Combine(this._ttsCachePath, RefVoiceManager.Hash(text + "Brian") + ".mp3");
    if (!File.Exists(filePath))
      yield return (object) this.StartCoroutine(this.DownloadTTS(text, filePath, true));
    else
      yield return (object) this.StartCoroutine(this.PlayLocalFile(filePath));
  }

  private IEnumerator DownloadTTS(string text, string filePath, bool playAfter) { yield break; }

  private IEnumerator PlayLocalFile(string filePath)
  {
    if (!File.Exists(filePath))
      yield break;
    using (UnityWebRequest req = UnityWebRequestMultimedia.GetAudioClip("file://" + filePath, (AudioType) 13))
    {
      yield return (object) req.SendWebRequest();
      if (req.result != (UnityWebRequest.Result) 2 && req.result != (UnityWebRequest.Result) 3)
      {
        AudioClip clip = DownloadHandlerAudioClip.GetContent(req);
        if ((UnityEngine.Object) clip == (UnityEngine.Object) null)
          yield break;
        yield return (object) this.StartCoroutine(this.InjectAudio(clip));
      }
    }
  }

  private IEnumerator InjectAudio(AudioClip clip)
  {
    if (((UnityEngine.Object) this._localAudioSource != (UnityEngine.Object) null))
      this._localAudioSource.PlayOneShot(clip);
    Voice.Play(clip, 1f);
    float timeout = (float) ((double) Time.time + (double) clip.length + 1.0);
    while ((!Voice.IsPlaying ? 0 : ((double) Time.time < (double) timeout ? 1 : 0)) != 0)
      yield return (object) null;
    if (Voice.IsPlaying)
      Voice.Stop();
  }

  private static string Hash(string value)
  {
    using (SHA256 shA256 = SHA256.Create())
    {
      byte[] hash = shA256.ComputeHash(Encoding.UTF8.GetBytes(value));
      StringBuilder stringBuilder = new StringBuilder(hash.Length * 2);
      foreach (byte num in hash)
        stringBuilder.Append(num.ToString("x2"));
      return stringBuilder.ToString();
    }
  }
}
