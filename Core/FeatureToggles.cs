#nullable disable
namespace SakuraaCastingMod.Core;

public static class FeatureToggles
{
  private static readonly string[] AllKeys = new string[5]
  {
    nameof (ShowTablet),
    nameof (ShowDesktopCasting),
    nameof (ShowCameraTablet),
    nameof (ShowReplaySystem),
    nameof (DiscordRpc)
  };

  public static bool ShowTablet { get; private set; } = true;

  public static bool ShowDesktopCasting { get; private set; } = true;

  public static bool ShowCameraTablet { get; private set; } = true;

  public static bool ShowReplaySystem { get; private set; } = true;

  public static bool DiscordRpc { get; private set; } = false;

  public static void Refresh()
  {
  }

  private static bool SafeGetBool(string key, bool fallback = true)
  {
    try
    {
      return WebSocketBridge.GetSettingBool(key);
    }
    catch
    {
      return fallback;
    }
  }
}
