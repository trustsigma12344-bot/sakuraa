using System.Runtime.InteropServices;

#nullable disable
namespace SakuraaCastingMod.Shared.Integrations.DiscordSDK;

public struct UserAchievement
{
  public long UserId;
  public long AchievementId;
  public byte PercentComplete;
  [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64 /*0x40*/)]
  public string UnlockedAt;
}
