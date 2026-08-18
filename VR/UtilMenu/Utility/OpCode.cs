using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

#nullable disable
namespace SakuraaCastingMod.VR.UtilMenu.Utility;

[JsonConverter(typeof (StringEnumConverter))]
public enum OpCode
{
  FRIEND_ACTION,
  FRIEND_ADD_INGAME,
  PARTY_CREATE,
  PARTY_JOIN_REQUEST,
  PARTY_LEADER_DECISION,
  PARTY_UPDATE_SETTINGS,
  PARTY_LEAVE,
  PARTY_PROMOTE,
  PARTY_KICK,
  CONFIG_SHARE_SEND,
  NOTIFICATION_NEW,
  GAME_JOIN_REQUEST,
  GET_MY_PROFILE,
  PING,
}
