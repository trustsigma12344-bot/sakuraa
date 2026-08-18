using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Shared.Helpers;

public static class Extensions
{
  public static T GetOrAddComponent<T>(this GameObject obj) where T : Component
  {
    T component = obj.GetComponent<T>();
    return ((UnityEngine.Object) (object) component) ? component : obj.AddComponent<T>();
  }

  public static void Obliterate(this Component self) => UnityEngine.Object.Destroy((UnityEngine.Object) self);

  public static float Distance(this Vector3 self, Vector3 other) => Vector3.Distance(self, other);

  public static float Map(float x, float a1, float a2, float b1, float b2)
  {
    return b1 + (float) (((double) x - (double) a1) / ((double) a2 - (double) a1) * ((double) b2 - (double) b1));
  }
}
