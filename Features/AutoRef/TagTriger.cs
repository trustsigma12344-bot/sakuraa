using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Features.AutoRef;

public class TagTriger
{
  public bool ShouldTag = true;
  public readonly GameObject Obj;

  public TagTriger(Vector3 position, Quaternion rotation, Vector3 scale)
  {
    this.Obj = GameObject.CreatePrimitive((PrimitiveType) 3);
    this.Obj.layer = LayerMask.NameToLayer("Water");
    Renderer component = this.Obj.GetComponent<Renderer>();
    if (((UnityEngine.Object) component != (UnityEngine.Object) null))
      UnityEngine.Object.Destroy((UnityEngine.Object) component);
    this.Obj.transform.localScale = scale;
    this.Obj.transform.position = position;
    this.Obj.transform.rotation = rotation;
    this.Obj.GetComponent<Collider>().isTrigger = true;
    this.Obj.AddComponent<TaggerObjectScript>().tagTriger = this;
  }
}
