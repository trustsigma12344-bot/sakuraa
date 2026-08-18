using SakuraaCastingMod.Core;
using SakuraaCastingMod.Shared.Helpers;
using SakuraaCastingMod.VR.Interaction;
using SakuraaCastingMod.VR.UtilMenu;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Features.Soundboard;

public static class SoundboardKeybinds
{
  [SavedSetting("SoundboardKeyLeftX", "")]
  public static string LeftXSound = "";
  [SavedSetting("SoundboardKeyLeftY", "")]
  public static string LeftYSound = "";
  [SavedSetting("SoundboardKeyRightA", "")]
  public static string RightASound = "";
  [SavedSetting("SoundboardKeyRightB", "")]
  public static string RightBSound = "";
  public static readonly (SoundboardKeybinds.Slot slot, string label)[] Defs = new (SoundboardKeybinds.Slot, string)[4]
  {
    (SoundboardKeybinds.Slot.LeftX, "LEFT X"),
    (SoundboardKeybinds.Slot.LeftY, "LEFT Y"),
    (SoundboardKeybinds.Slot.RightA, "RIGHT A"),
    (SoundboardKeybinds.Slot.RightB, "RIGHT B")
  };

  public static string GetSoundId(SoundboardKeybinds.Slot s)
  {
    string soundId;
    switch (s)
    {
      case SoundboardKeybinds.Slot.LeftX:
        soundId = SoundboardKeybinds.LeftXSound;
        break;
      case SoundboardKeybinds.Slot.LeftY:
        soundId = SoundboardKeybinds.LeftYSound;
        break;
      case SoundboardKeybinds.Slot.RightA:
        soundId = SoundboardKeybinds.RightASound;
        break;
      case SoundboardKeybinds.Slot.RightB:
        soundId = SoundboardKeybinds.RightBSound;
        break;
      default:
        soundId = "";
        break;
    }
    return soundId;
  }

  public static void SetSoundId(SoundboardKeybinds.Slot s, string id)
  {
    id = id ?? "";
    switch (s)
    {
      case SoundboardKeybinds.Slot.LeftX:
        SoundboardKeybinds.LeftXSound = id;
        break;
      case SoundboardKeybinds.Slot.LeftY:
        SoundboardKeybinds.LeftYSound = id;
        break;
      case SoundboardKeybinds.Slot.RightA:
        SoundboardKeybinds.RightASound = id;
        break;
      case SoundboardKeybinds.Slot.RightB:
        SoundboardKeybinds.RightBSound = id;
        break;
    }
    Configuration.SaveSettings();
  }

  public static void AllOff()
  {
    string str;
    SoundboardKeybinds.RightBSound = str = "";
    SoundboardKeybinds.RightASound = str;
    SoundboardKeybinds.LeftYSound = str;
    SoundboardKeybinds.LeftXSound = str;
    Configuration.SaveSettings();
  }

  public static bool AnyEnabled
  {
    get
    {
      return !string.IsNullOrEmpty(SoundboardKeybinds.LeftXSound) || !string.IsNullOrEmpty(SoundboardKeybinds.LeftYSound) || !string.IsNullOrEmpty(SoundboardKeybinds.RightASound) || !string.IsNullOrEmpty(SoundboardKeybinds.RightBSound);
    }
  }

  public static bool ConflictsWithMenuToggle(SoundboardKeybinds.Slot s)
  {
    bool flag;
    switch (s)
    {
      case SoundboardKeybinds.Slot.LeftX:
        flag = UtilMenuController.CurrentToggleButton == UtilMenuController.MenuToggleButton.Primary;
        break;
      case SoundboardKeybinds.Slot.LeftY:
        flag = UtilMenuController.CurrentToggleButton == UtilMenuController.MenuToggleButton.Secondary;
        break;
      default:
        flag = false;
        break;
    }
    return flag;
  }

  public static void Tick()
  {
    if (!SoundboardKeybinds.AnyEnabled)
      return;
    InputManager ins = InputManager.Ins;
    if (((UnityEngine.Object) ins == (UnityEngine.Object) null))
      return;
    if (ins.leftPrimaryBtnSingle)
      SoundboardKeybinds.Toggle(SoundboardKeybinds.Slot.LeftX);
    if (ins.leftSecondaryBtnSingle)
      SoundboardKeybinds.Toggle(SoundboardKeybinds.Slot.LeftY);
    if (ins.rightPrimaryBtnSingle)
      SoundboardKeybinds.Toggle(SoundboardKeybinds.Slot.RightA);
    if (!ins.rightSecondaryBtnSingle)
      return;
    SoundboardKeybinds.Toggle(SoundboardKeybinds.Slot.RightB);
  }

  private static void Toggle(SoundboardKeybinds.Slot s)
  {
    string soundId = SoundboardKeybinds.GetSoundId(s);
    if ((string.IsNullOrEmpty(soundId) ? 1 : (SoundboardKeybinds.ConflictsWithMenuToggle(s) ? 1 : 0)) != 0)
      return;
    if (SoundboardPlayer.CurrentSoundId == soundId)
    {
      SoundboardPlayer.Stop();
    }
    else
    {
      SoundboardManager.Sound sound = SoundboardManager.FindSound(soundId);
      if (sound == null)
        return;
      SoundboardPlayer.Play(sound);
    }
  }

  public enum Slot
  {
    LeftX,
    LeftY,
    RightA,
    RightB,
  }
}
