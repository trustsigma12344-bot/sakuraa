using Newtonsoft.Json;

#nullable disable
namespace SakuraaCastingMod.VR.UtilMenu.Utility;

public class WsPacket
{
  [JsonProperty("op")]
  public OpCode Op { get; set; }

  [JsonProperty("d")]
  public object Data { get; set; }
}
