using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

#nullable disable
namespace SakuraaCastingMod.VR.UtilMenu.Utility;

[JsonConverter(typeof (StringEnumConverter))]
public enum FriendActionType
{
  REQUEST_ACCEPT,
  REQUEST_DECLINE,
  REMOVE,
}
