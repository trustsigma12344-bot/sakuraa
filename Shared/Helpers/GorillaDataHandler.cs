using SakuraaCastingMod.Core;
using SakuraaCastingMod.Shared.Models;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Shared.Helpers;

public static class GorillaDataHandler
{
  public static List<GorillaData> GorillaDataList = new List<GorillaData>();
  public static Dictionary<string, GorillaData> GorillaDataDict = new Dictionary<string, GorillaData>();
  private static readonly HashSet<string> ActiveIds = new HashSet<string>();
  private static readonly List<string> IdsToRemove = new List<string>();
  private static readonly HashSet<GorillaData> InfectedSet = new HashSet<GorillaData>();
  private static float _lastFailureLogTime = -999f;

  /// <summary>
  /// Plugin.Update() calls this first, and everything after it (keybinds, camera modes,
  /// minimap, scoreboard, notifications) is skipped if it throws. Never let a single bad
  /// frame take the rest of the mod down with it; log at most once every 5 seconds so a
  /// recurring fault can't spam a stack trace to Player.log every frame.
  /// </summary>
  public static void SafeUpdate()
  {
    try
    {
      GorillaDataHandler.UpdateGorillaData();
    }
    catch (System.Exception ex)
    {
      if ((double) (Time.realtimeSinceStartup - GorillaDataHandler._lastFailureLogTime) > 5.0)
      {
        GorillaDataHandler._lastFailureLogTime = Time.realtimeSinceStartup;
        UnityEngine.Debug.LogWarning((object) ("[Sakuraa] UpdateGorillaData failed, skipping this frame: " + ex.Message));
      }
    }
  }

  public static void UpdateGorillaData()
  {
    GorillaDataHandler.ActiveIds.Clear();
    GorillaDataHandler.IdsToRemove.Clear();
    GorillaDataHandler.InfectedSet.Clear();
    if (Plugin.ReplayCompatibilityMode)
    {
      foreach (GorillaRigExposed exposedRig in ReplayModExposer.exposedRigs)
      {
        if (string.IsNullOrEmpty(exposedRig.ID))
          continue;
        if (exposedRig.isRigActive)
        {
          GorillaData gorillaData;
          if (!GorillaDataHandler.GorillaDataDict.TryGetValue(exposedRig.ID, out gorillaData))
          {
            GorillaDataHandler.GorillaDataDict.Add(exposedRig.ID, new GorillaData()
            {
              BodyTransform = exposedRig.RootBone.transform,
              HeadTransform = exposedRig.head.transform,
              Color = exposedRig.gorillaColor,
              UserName = exposedRig.userName,
              UserId = exposedRig.ID,
              Infected = exposedRig.isRigInfected
            });
          }
          else
          {
            gorillaData.BodyTransform = exposedRig.RootBone.transform;
            gorillaData.HeadTransform = exposedRig.head.transform;
            gorillaData.Color = exposedRig.gorillaColor;
            gorillaData.UserName = exposedRig.userName;
            gorillaData.UserId = exposedRig.ID;
            gorillaData.Infected = exposedRig.isRigInfected;
          }
        }
        else if (GorillaDataHandler.GorillaDataDict.ContainsKey(exposedRig.ID))
          GorillaDataHandler.GorillaDataDict.Remove(exposedRig.ID);
      }
    }
    else if (Networking.InRoom)
    {
      GorillaTagManager gtagManager = Networking.GtagManager;
      if ((!((UnityEngine.Object) gtagManager != (UnityEngine.Object) null) ? 0 : (((GorillaGameManager) gtagManager).GameModeName() == "INFECTION" ? 1 : 0)) != 0)
      {
        foreach (NetPlayer player in gtagManager.currentInfected)
        {
          if (player != null)
          {
            GorillaData gorillaByNetPlayer = PlayerTranslator.GetGorillaByNetPlayer(player);
            if (gorillaByNetPlayer != null)
              GorillaDataHandler.InfectedSet.Add(gorillaByNetPlayer);
          }
        }
      }
      foreach (VRRig vrrig in PlayerTranslator.vrrigs)
      {
        // A rig can exist before Photon has assigned its owner a UserId (mid-join),
        // and a null key throws ArgumentNullException out of Update() every frame.
        if (vrrig != null && vrrig.OwningNetPlayer != null && !string.IsNullOrEmpty(vrrig.OwningNetPlayer.UserId))
        {
          string userId = vrrig.OwningNetPlayer.UserId;
          GorillaDataHandler.ActiveIds.Add(userId);
          Color color;
          if ((vrrig.setMatIndex != 0 ? 0 : (vrrig.materialsToChangeTo.Length != 0 ? 1 : 0)) == 0)
          {
            // ISSUE: explicit constructor call
            color = new Color(0.55f, 0.1f, 0.0f);
          }
          else
            color = vrrig.materialsToChangeTo[0].color;
          GorillaData gorillaData;
          if (!GorillaDataHandler.GorillaDataDict.TryGetValue(userId, out gorillaData))
          {
            GorillaDataHandler.GorillaDataDict.Add(userId, new GorillaData()
            {
              Color = color,
              UserName = vrrig.OwningNetPlayer.NickName,
              UserId = userId,
              BodyTransform = ((Component) vrrig).transform,
              HeadTransform = vrrig.headMesh.transform,
              Rig = vrrig,
              Infected = false
            });
          }
          else
          {
            gorillaData.Color = color;
            gorillaData.UserName = vrrig.OwningNetPlayer.NickName;
            gorillaData.UserId = userId;
            gorillaData.BodyTransform = ((Component) vrrig).transform;
            gorillaData.HeadTransform = vrrig.headMesh.transform;
            gorillaData.Rig = vrrig;
            int num = !((UnityEngine.Object) gtagManager != (UnityEngine.Object) null) ? 0 : (((GorillaGameManager) gtagManager).GameModeName() == "INFECTION" ? 1 : 0);
            gorillaData.Infected = num != 0 && GorillaDataHandler.InfectedSet.Contains(gorillaData);
          }
        }
      }
      foreach (string key in GorillaDataHandler.GorillaDataDict.Keys)
      {
        if (!GorillaDataHandler.ActiveIds.Contains(key))
          GorillaDataHandler.IdsToRemove.Add(key);
      }
      foreach (string key in GorillaDataHandler.IdsToRemove)
      {
        GorillaData gorillaData;
        if (GorillaDataHandler.GorillaDataDict.TryGetValue(key, out gorillaData))
          UnityEngine.Debug.Log((object) (gorillaData.UserName + " is invalid, removing"));
        GorillaDataHandler.GorillaDataDict.Remove(key);
      }
    }
    else
    {
      VRRig offlineVrRig = GorillaTagger.Instance.offlineVRRig;
      if (offlineVrRig != null && NetworkSystem.Instance.LocalPlayer != null && !string.IsNullOrEmpty(NetworkSystem.Instance.LocalPlayer.UserId))
      {
        string userId = NetworkSystem.Instance.LocalPlayer.UserId;
        string nickName = NetworkSystem.Instance.LocalPlayer.NickName;
        Color color;
        if ((offlineVrRig.setMatIndex != 0 ? 0 : (offlineVrRig.materialsToChangeTo.Length != 0 ? 1 : 0)) != 0)
        {
          color = offlineVrRig.materialsToChangeTo[0].color;
        }
        else
        {
          // ISSUE: explicit constructor call
          color = new Color(0.55f, 0.1f, 0.0f);
        }
        GorillaData gorillaData;
        if (!GorillaDataHandler.GorillaDataDict.TryGetValue(userId, out gorillaData))
        {
          GorillaDataHandler.GorillaDataDict.Add(userId, new GorillaData()
          {
            Color = color,
            UserName = nickName,
            UserId = userId,
            BodyTransform = ((Component) offlineVrRig).transform,
            HeadTransform = offlineVrRig.headMesh.transform,
            Infected = false
          });
        }
        else
        {
          gorillaData.Color = color;
          gorillaData.UserName = nickName;
          gorillaData.UserId = userId;
          gorillaData.BodyTransform = ((Component) offlineVrRig).transform;
          gorillaData.HeadTransform = offlineVrRig.headMesh.transform;
          gorillaData.Infected = false;
        }
        foreach (string key in GorillaDataHandler.GorillaDataDict.Keys)
        {
          if (key != userId)
            GorillaDataHandler.IdsToRemove.Add(key);
        }
        foreach (string key in GorillaDataHandler.IdsToRemove)
          GorillaDataHandler.GorillaDataDict.Remove(key);
      }
    }
    GorillaDataHandler.GorillaDataList.Clear();
    GorillaDataHandler.GorillaDataList.AddRange((IEnumerable<GorillaData>) GorillaDataHandler.GorillaDataDict.Values);
  }
}
