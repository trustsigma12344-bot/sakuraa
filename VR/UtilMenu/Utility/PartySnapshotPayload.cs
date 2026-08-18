using Newtonsoft.Json;
using System.Collections.Generic;

#nullable disable
namespace SakuraaCastingMod.VR.UtilMenu.Utility;

public class PartySnapshotPayload
{
  [JsonProperty("partyId")]
  public string PartyId { get; set; }

  [JsonProperty("leaderId")]
  public string LeaderId { get; set; }

  [JsonProperty("mode")]
  public PartyJoinMode Mode { get; set; }

  [JsonProperty("members")]
  public List<FriendModel> Members { get; set; }

  [JsonProperty("youAreLeader")]
  public bool YouAreLeader { get; set; }
}
