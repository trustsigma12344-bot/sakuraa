using GorillaLocomotion;
using GorillaNetworking;
using SakuraaCastingMod.Features.Overlays;
using SakuraaCastingMod.Shared.Helpers;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Features.World;

public static class SakTeleporter
{
  [SavedSetting("SakTeleporterShowMenu", false)]
  public static bool ShowMenu;

  private static void TeleportPlayer(Vector3 targetPosition, Quaternion targetRotation)
  {
    GorillaTagger.Instance.rigidbody.velocity = Vector3.zero;
    Transform parent = ((Component) ((Component) GTPlayer.Instance).GetComponent<Rigidbody>()).transform.parent;
    parent.position = targetPosition;
    parent.rotation = targetRotation;
    ((Component) ((Component) GTPlayer.Instance).GetComponent<Rigidbody>()).transform.localPosition = Vector3.zero;
    GTPlayer.Instance.InitializeValues();
    foreach (GameObject gameObject in PhotonNetworkController.Instance.disableOnStartup)
      gameObject.SetActive(false);
    foreach (GameObject gameObject in PhotonNetworkController.Instance.enableOnStartup)
      gameObject.SetActive(true);
  }

  public static void AttemptToTeleportTo(Vector3 pos, Quaternion rot)
  {
    if (NetworkSystem.Instance.InRoom)
      Notification.Send("You must leave the room before teleporting!", Color.magenta);
    else
      SakTeleporter.TeleportPlayer(pos, rot);
  }
}
