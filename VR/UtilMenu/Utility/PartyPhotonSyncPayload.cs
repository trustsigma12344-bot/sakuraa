using Newtonsoft.Json;

#nullable disable
namespace SakuraaCastingMod.VR.UtilMenu.Utility;

public class PartyPhotonSyncPayload
{
  [JsonProperty("roomCode")]
  public string RoomCode { get; set; }

  [JsonProperty("forceJoin")]
  public bool ForceJoin { get; set; }
}
