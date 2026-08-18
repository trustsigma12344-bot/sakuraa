using SakuraaCastingMod.Shared.Helpers;
using SakuraaCastingMod.VR.UtilMenu.Pages;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Api;

[Browsable(true)]
public static class ModCheck
{
  public static List<string> GetMods(VRRig rig)
  {
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    return ((UnityEngine.Object) rig == (UnityEngine.Object) null) ? new List<string>() : LobbyPage.GetDetectedMods(rig).Select<string, string>(new Func<string, string>(ModCheck.Clean)).Where<string>((Func<string, bool>) (s => !string.IsNullOrEmpty(s))).ToList<string>();
  }

  public static List<string> GetMods(NetPlayer player)
  {
    return player == null ? new List<string>() : ModCheck.GetMods(PlayerTranslator.GetRigByNetPlayer(player));
  }

  public static List<string> GetMods(string playerName)
  {
    return ModCheck.GetMods(ApiLookup.FindRigByName(playerName));
  }

  public static void CountMods(VRRig rig, out int cheats, out int mods)
  {
    cheats = 0;
    mods = 0;
    if (((UnityEngine.Object) rig == (UnityEngine.Object) null))
      return;
    foreach (string detectedMod in LobbyPage.GetDetectedMods(rig))
    {
      if (!string.IsNullOrEmpty(ModCheck.Clean(detectedMod)))
      {
        if (!detectedMod.Contains("red"))
          ++mods;
        else
          ++cheats;
      }
    }
  }

  public static void CountMods(NetPlayer player, out int cheats, out int mods)
  {
    ModCheck.CountMods(player == null ? (VRRig) null : PlayerTranslator.GetRigByNetPlayer(player), out cheats, out mods);
  }

  private static string Clean(string s) => Regex.Replace(s ?? "", "<.*?>", "").Trim();
}
