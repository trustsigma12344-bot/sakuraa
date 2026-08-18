using System.Runtime.InteropServices;

#nullable disable
namespace SakuraaCastingMod.Shared.Integrations.DiscordSDK;

public struct OAuth2Token
{
  [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128 /*0x80*/)]
  public string AccessToken;
  [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1024 /*0x0400*/)]
  public string Scopes;
  public long Expires;
}
