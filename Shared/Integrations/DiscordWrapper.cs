using SakuraaCastingMod.Shared.Integrations.DiscordSDK;
using System;
using System.Diagnostics;

#nullable disable
namespace SakuraaCastingMod.Shared.Integrations;

public static class DiscordWrapper
{
  private static Discord discord;
  private static ActivityManager activityManager;
  private static Activity activity;
  private static volatile bool toUpload;
  private static volatile bool running;
  private static float lastUploadTime;
  private static readonly Stopwatch uploadTimer;

  public static void Construct()
  {
  }

  public static void Shutdown()
  {
  }

  private static void RegisterDiscord()
  {
  }

  public static void SetActivity(Func<Activity, Activity> modificationFunc)
  {
  }

  public static void UpdateActivity()
  {
  }
}
