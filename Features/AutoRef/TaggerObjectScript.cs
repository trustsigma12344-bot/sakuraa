using System.Collections;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Features.AutoRef;

internal class TaggerObjectScript : MonoBehaviour
{
  public TagTriger tagTriger;

  private IEnumerator CheckCollisionRoutine(Collider other)
  {
    VRRig vrrig = (VRRig) null;
    if (((UnityEngine.Object) ((Component) other).gameObject).name == "BodyTrigger")
    {
      Transform t = ((Component) other).transform;
      if ((!((UnityEngine.Object) t.parent != (UnityEngine.Object) null) || !((UnityEngine.Object) t.parent.parent != (UnityEngine.Object) null) ? 0 : (((UnityEngine.Object) t.parent.parent.parent != (UnityEngine.Object) null) ? 1 : 0)) != 0)
        vrrig = ((Component) t.parent.parent.parent).GetComponent<VRRig>();
      t = (Transform) null;
    }
    if ((!((UnityEngine.Object) vrrig != (UnityEngine.Object) null) || this.tagTriger == null ? 0 : (this.tagTriger.ShouldTag ? 1 : 0)) != 0)
    {
      if ((((UnityEngine.Object) AutoRefManager.Instance == (UnityEngine.Object) null) ? 1 : (!AutoRefManager.Instance.IsActive ? 1 : 0)) != 0 || vrrig.OwningNetPlayer == null)
        yield break;
      AutoRefManager.Instance.TryTagPlayer(vrrig.OwningNetPlayer);
    }
    yield return (object) null;
  }

  public void OnTriggerEnter(Collider other)
  {
    this.StartCoroutine(this.CheckCollisionRoutine(other));
  }
}
