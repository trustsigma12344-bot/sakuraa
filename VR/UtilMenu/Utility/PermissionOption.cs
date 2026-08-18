using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

#nullable disable
namespace SakuraaCastingMod.VR.UtilMenu.Utility;

[JsonConverter(typeof (StringEnumConverter))]
public enum PermissionOption
{
  FRIENDS,
  FAVORITES,
  DISABLE,
}
