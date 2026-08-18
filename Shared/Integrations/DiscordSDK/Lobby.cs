using System.Runtime.InteropServices;

#nullable disable
namespace SakuraaCastingMod.Shared.Integrations.DiscordSDK;

public struct Lobby
{
  public long Id;
  public LobbyType Type;
  public long OwnerId;
  [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128 /*0x80*/)]
  public string Secret;
  public uint Capacity;
  public bool Locked;
}
