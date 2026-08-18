using SakuraaCastingMod.Core;
using UnityEngine;

#nullable disable
namespace Loader;

public class ErmWhatTheSigma
{
  private static bool _loaded;

  public static void Load()
  {
    if (ErmWhatTheSigma._loaded)
      return;
    ErmWhatTheSigma._loaded = true;
    EmbeddedAssemblyResolver.Hook();
    GameObject gameObject = new GameObject();
    UnityEngine.Object.DontDestroyOnLoad((UnityEngine.Object) gameObject);
    gameObject.AddComponent<Plugin>();
  }
}
