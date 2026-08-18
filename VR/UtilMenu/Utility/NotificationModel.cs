using Newtonsoft.Json;

#nullable disable
namespace SakuraaCastingMod.VR.UtilMenu.Utility;

public class NotificationModel
{
  [JsonProperty("id")]
  public string NotificationId { get; set; }

  [JsonProperty("title")]
  public string Title { get; set; }

  [JsonProperty("description")]
  public string Description { get; set; }

  [JsonProperty("timestamp")]
  public long Timestamp { get; set; }

  [JsonProperty("type")]
  public string Type { get; set; }

  [JsonProperty("payloadJson")]
  public string PayloadJson { get; set; }
}
