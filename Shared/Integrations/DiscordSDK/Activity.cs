using System.Runtime.InteropServices;

#nullable disable
namespace SakuraaCastingMod.Shared.Integrations.DiscordSDK;

public struct Activity
{
  public ActivityType Type;
  public long ApplicationId;
  [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128 /*0x80*/)]
  public string Name;
  [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128 /*0x80*/)]
  public string State;
  [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128 /*0x80*/)]
  public string Details;
  public ActivityTimestamps Timestamps;
  public ActivityAssets Assets;
  public ActivityParty Party;
  public ActivitySecrets Secrets;
  public bool Instance;
  public uint SupportedPlatforms;
}
