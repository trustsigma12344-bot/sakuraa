using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Shared.Helpers;

public static class Networking
{
  public static bool InRoom;
  public static GorillaTagManager GtagManager;

  public static void ScanForManagerUpdate()
  {
    if (((UnityEngine.Object) NetworkSystem.Instance == (UnityEngine.Object) null))
      return;
    if (!NetworkSystem.Instance.InRoom)
    {
      Networking.GtagManager = (GorillaTagManager) null;
    }
    else
    {
      if (((UnityEngine.Object) Networking.GtagManager != (UnityEngine.Object) null))
        return;
      GorillaGameManager instance = GorillaGameManager.instance;
      if ((!((UnityEngine.Object) instance != (UnityEngine.Object) null) ? 0 : (instance.GetType() == typeof (GorillaTagManager) ? 1 : 0)) == 0)
        return;
      Networking.GtagManager = (GorillaTagManager) instance;
    }
  }

  public static VRRig MyRig
  {
    get
    {
      return !NetworkSystem.Instance.InRoom ? VRRigCache.Instance.localRig.Rig : PlayerTranslator.vrrigDict[NetworkSystem.Instance.LocalPlayer];
    }
  }

  public static void LeaveRoom()
  {
    if (!Networking.InRoom)
      return;
    NetworkSystem.Instance.ReturnToSinglePlayer();
  }

  public static bool CheckInRoom() => Networking.InRoom;

  public static bool CheckNotInRoom() => !Networking.InRoom;
}
