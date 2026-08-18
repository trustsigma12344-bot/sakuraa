using Newtonsoft.Json;

#nullable disable
namespace SakuraaCastingMod.VR.UtilMenu.Utility;

public class FriendModel
{
  [JsonProperty("userId")]
  public string UserId { get; set; }

  [JsonProperty("discordName")]
  public string Username { get; set; }

  [JsonProperty("status")]
  public UserStatus Status { get; set; }

  [JsonProperty("roomCode")]
  public string RoomCode { get; set; }

  [JsonProperty("inRoom")]
  public bool InRoom { get; set; }

  [JsonProperty("photonId")]
  public string PhotonId { get; set; }

  [JsonProperty("partyId")]
  public string PartyId { get; set; }
}
