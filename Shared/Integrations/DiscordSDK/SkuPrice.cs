using System.Runtime.InteropServices;

#nullable disable
namespace SakuraaCastingMod.Shared.Integrations.DiscordSDK;

public struct SkuPrice
{
  public uint Amount;
  [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16 /*0x10*/)]
  public string Currency;
}
