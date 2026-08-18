using Newtonsoft.Json;

#nullable disable
namespace SakuraaCastingMod.VR.UtilMenu.Utility;

public class PendingFriendRequest
{
  [JsonProperty("fromId")]
  public string UserId { get; set; }

  [JsonProperty("discordName")]
  public string Username { get; set; }
}
