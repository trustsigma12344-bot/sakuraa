using HarmonyLib;
using System.Reflection;

#nullable disable
namespace SakuraaCastingMod.Core;

public class HarmonyPatches
{
  public const string InstanceId = "com.sakuraa.gorillatag.sakuraacastingmod";
  private static Harmony instance;

  public static bool IsPatched { get; private set; }

  internal static void ApplyHarmonyPatches()
  {
    if (HarmonyPatches.IsPatched)
      return;
    if (HarmonyPatches.instance == null)
      HarmonyPatches.instance = new Harmony("com.sakuraa.gorillatag.sakuraacastingmod");
    HarmonyPatches.instance.PatchAll(Assembly.GetExecutingAssembly());
    HarmonyPatches.IsPatched = true;
  }

  internal static void RemoveHarmonyPatches()
  {
    if ((HarmonyPatches.instance == null ? 0 : (HarmonyPatches.IsPatched ? 1 : 0)) == 0)
      return;
    HarmonyPatches.instance.UnpatchSelf();
    HarmonyPatches.IsPatched = false;
  }
}
