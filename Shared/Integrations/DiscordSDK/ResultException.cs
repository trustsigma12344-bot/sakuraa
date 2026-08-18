using System;

#nullable disable
namespace SakuraaCastingMod.Shared.Integrations.DiscordSDK;

public class ResultException(Result result) : Exception(result.ToString())
{
  public readonly Result Result;
}
