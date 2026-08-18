using SakuraaCastingMod.Shared.Integrations;
using System.ComponentModel;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Api;

[Browsable(true)]
public static class Music
{
  public static void Play() => Music.Resume();

  public static void Resume()
  {
    SpotifyManager instance = SpotifyManager.Instance;
    if ((!((UnityEngine.Object) instance != (UnityEngine.Object) null) ? 0 : (instance.IsConnected ? 1 : 0)) == 0)
      SpotifyManager.NativePlayPause();
    else
      instance.Resume();
  }

  public static void Pause()
  {
    SpotifyManager instance = SpotifyManager.Instance;
    if ((!((UnityEngine.Object) instance != (UnityEngine.Object) null) ? 0 : (instance.IsConnected ? 1 : 0)) != 0)
      instance.Pause();
    else
      SpotifyManager.NativePlayPause();
  }

  public static void Next()
  {
    SpotifyManager instance = SpotifyManager.Instance;
    if ((!((UnityEngine.Object) instance != (UnityEngine.Object) null) ? 0 : (instance.IsConnected ? 1 : 0)) == 0)
      SpotifyManager.NativeNextTrack();
    else
      instance.Skip();
  }

  public static void Previous()
  {
    SpotifyManager instance = SpotifyManager.Instance;
    if ((!((UnityEngine.Object) instance != (UnityEngine.Object) null) ? 0 : (instance.IsConnected ? 1 : 0)) != 0)
      instance.Previous();
    else
      SpotifyManager.NativePreviousTrack();
  }

  public static string NowPlaying()
  {
    SpotifyManager instance = SpotifyManager.Instance;
    string str;
    if ((((UnityEngine.Object) instance == (UnityEngine.Object) null) ? 1 : (!instance.IsConnected ? 1 : 0)) != 0)
    {
      str = (string) null;
    }
    else
    {
      string currentTrackName = instance.CurrentTrackName;
      if ((string.IsNullOrEmpty(currentTrackName) || currentTrackName == "Not Connected" ? 1 : (currentTrackName.StartsWith("Connected") ? 1 : 0)) != 0)
      {
        str = (string) null;
      }
      else
      {
        string currentArtistName = instance.CurrentArtistName;
        str = string.IsNullOrEmpty(currentArtistName) || currentArtistName == "Unknown" ? currentTrackName : $"{currentTrackName} - {currentArtistName}";
      }
    }
    return str;
  }
}
