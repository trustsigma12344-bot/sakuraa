#nullable disable
namespace SakuraaCastingMod.Shared.Integrations.DiscordSDK;

public enum LobbySearchComparison
{
  LessThanOrEqual = -2, // 0xFFFFFFFE
  LessThan = -1, // 0xFFFFFFFF
  Equal = 0,
  GreaterThan = 1,
  GreaterThanOrEqual = 2,
  NotEqual = 3,
}
