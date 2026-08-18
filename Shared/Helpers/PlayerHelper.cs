using SakMerge.Api;

#nullable disable
namespace SakuraaCastingMod.Shared.Helpers;

public static class PlayerHelper
{
  public static void UpdateColor(float r, float g, float b) => Colors.Set(r, g, b);

  public static void UpdateName(string newName) => Rigs.SetName(newName);
}
