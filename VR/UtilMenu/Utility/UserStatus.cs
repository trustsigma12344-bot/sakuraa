using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

#nullable disable
namespace SakuraaCastingMod.VR.UtilMenu.Utility;

[JsonConverter(typeof (StringEnumConverter))]
public enum UserStatus
{
  IN_LAUNCHER,
  AWAY,
  IN_GAME,
  OFFLINE,
}
