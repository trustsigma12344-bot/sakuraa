using Photon.Pun;
using Photon.Realtime;

#nullable disable
namespace SakuraaCastingMod.Features.Visuals;

public class NameTagCleanupBridge : MonoBehaviourPunCallbacks
{
  public override void OnPlayerLeftRoom(Player otherPlayer)
  {
    base.OnPlayerLeftRoom(otherPlayer);
    if (otherPlayer == null)
      return;
    NameTags.RemoveNameTagFor(otherPlayer.UserId);
  }
}
