using SakuraaCastingMod.Shared.Helpers;
using System;
using System.Collections.Generic;

#nullable disable
namespace SakuraaCastingMod.Api;

internal static class ApiLookup
{
  internal static VRRig FindRigByName(string name)
  {
    VRRig rigByName;
    if (!string.IsNullOrWhiteSpace(name))
    {
      List<GorillaPlayerScoreboardLine> allScoreboardLines = GorillaScoreboardTotalUpdater.allScoreboardLines;
      if (allScoreboardLines == null)
      {
        rigByName = (VRRig) null;
      }
      else
      {
        string str = name.Trim();
        foreach (GorillaPlayerScoreboardLine playerScoreboardLine in allScoreboardLines)
        {
          NetPlayer linePlayer = playerScoreboardLine?.linePlayer;
          if ((linePlayer == null || linePlayer.NickName == null ? 0 : (linePlayer.NickName.IndexOf(str, StringComparison.OrdinalIgnoreCase) >= 0 ? 1 : 0)) != 0)
          {
            rigByName = PlayerTranslator.GetRigByNetPlayer(linePlayer);
            goto label_11;
          }
        }
        rigByName = (VRRig) null;
      }
    }
    else
      rigByName = (VRRig) null;
label_11:
    return rigByName;
  }
}
