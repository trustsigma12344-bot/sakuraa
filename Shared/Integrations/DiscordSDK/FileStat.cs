using System.Runtime.InteropServices;

#nullable disable
namespace SakuraaCastingMod.Shared.Integrations.DiscordSDK;

public struct FileStat
{
  [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
  public string Filename;
  public ulong Size;
  public ulong LastModified;
}
