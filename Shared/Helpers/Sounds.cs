using SakuraaCastingMod.Core;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Shared.Helpers;

public static class Sounds
{
  public static AudioClip boingSfx;
  public static AudioClip subtleClickSfx;
  public static AudioClip shootSfx;

  public static void PlaySound(AudioClip clip, float tapVolume = 0.15f, bool isLeftHand = false)
  {
    if ((((UnityEngine.Object) clip == (UnityEngine.Object) null) ? 1 : (((UnityEngine.Object) GorillaTagger.Instance == (UnityEngine.Object) null) ? 1 : 0)) != 0)
      return;
    AudioSource audioSource = isLeftHand ? GorillaTagger.Instance.offlineVRRig.leftHandPlayer : GorillaTagger.Instance.offlineVRRig.rightHandPlayer;
    audioSource.volume = tapVolume;
    GTAudioSourceExtensions.GTPlayOneShot(audioSource, clip, 1f);
  }

  public static void PlayCasterClick(AudioClip clip, float volume = 0.15f)
  {
    Plugin.Ins.CameraAudioSource.PlayOneShot(clip, volume);
  }

  public static void ControlModePlayKbSfx(int sound, float volume = 0.1f)
  {
    if (!((UnityEngine.Object) GorillaTagger.Instance.offlineVRRig != (UnityEngine.Object) null))
      return;
    GorillaTagger.Instance.offlineVRRig.PlayHandTapLocal(sound, false, volume);
  }
}
