using Newtonsoft.Json;

#nullable disable
namespace SakuraaCastingMod.VR.UtilMenu.Utility;

public class PartyApprovalRequestPayload
{
  [JsonProperty("requestId")]
  public string RequestId { get; set; }

  [JsonProperty("requesterUserId")]
  public string RequesterUserId { get; set; }

  [JsonProperty("requesterUsername")]
  public string RequesterUsername { get; set; }
}
