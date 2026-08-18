#nullable disable
namespace SakuraaCastingMod;

public static class WebSocketBridge
{
  public static bool SendMessage(string message) => false;

  public static string[] GetMessages() => new string[0];

  public static bool IsConnected() => false;

  public static bool GetSettingBool(string key) => false;

  public static string GetSettingString(string key) => "";
}
