using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#nullable disable
namespace SakuraaCastingMod.VR.UtilMenu.Utility;

public class ConfigSharePayload
{
  [JsonProperty("fromUserId")]
  public string FromUserId { get; set; }

  [JsonProperty("fromName")]
  public string FromName { get; set; }

  [JsonProperty("configName")]
  public string ConfigName { get; set; }

  [JsonProperty("patch")]
  public JObject Patch { get; set; }

  [JsonProperty("configJson")]
  public string ConfigJson { get; set; }
}
