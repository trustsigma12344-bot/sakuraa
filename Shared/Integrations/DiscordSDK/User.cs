using System.Runtime.InteropServices;

#nullable disable
namespace SakuraaCastingMod.Shared.Integrations.DiscordSDK;

public struct User
{
  public long Id;
  [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256 /*0x0100*/)]
  public string Username;
  [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
  public string Discriminator;
  [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128 /*0x80*/)]
  public string Avatar;
  public bool Bot;
}
