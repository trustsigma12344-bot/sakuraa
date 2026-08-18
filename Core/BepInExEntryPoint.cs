using BepInEx;

#nullable disable
namespace SakuraaCastingMod.Core;

[BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
public class BepInExEntryPoint : BaseUnityPlugin
{
  private void Awake() => Loader.ErmWhatTheSigma.Load();
}
