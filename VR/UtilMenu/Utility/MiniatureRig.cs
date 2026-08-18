using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.VR.UtilMenu.Utility;

public class MiniatureRig
{
  public GameObject GorillaHolder;
  public bool IsActive = false;
  public SkinnedMeshRenderer Skin;
  public Renderer _miniFaceRenderer;
  private Dictionary<string, Transform> _nameToMiniBone = new Dictionary<string, Transform>();
  private List<MiniatureRig.BonePair> _cachedBoneMap = new List<MiniatureRig.BonePair>();
  private List<GameObject> _activeCosmetics = new List<GameObject>();
  private VRRig _currentTarget;
  private bool _skeletonBuilt = false;

  public void SetTarget(VRRig targetRig, int targetLayer)
  {
    this._currentTarget = targetRig;
    if (!this._skeletonBuilt)
      this.BuildSkeleton(targetLayer);
    this.ClearCosmetics();
    this.MapBones(targetRig);
    this.UpdateVisuals(targetRig, targetLayer);
  }

  private void BuildSkeleton(int targetLayer)
  {
    if (((UnityEngine.Object) this.GorillaHolder != (UnityEngine.Object) null))
      UnityEngine.Object.Destroy((UnityEngine.Object) this.GorillaHolder);
    this.GorillaHolder = new GameObject("MiniGorillaHolder");
    this.GorillaHolder.layer = targetLayer;
    this._nameToMiniBone.Clear();
    GameObject gameObject1 = new GameObject("body");
    GameObject gameObject2 = new GameObject("head");
    GameObject gameObject3 = new GameObject("head_end");
    Transform transform1 = gameObject1.transform;
    Transform transform2 = gameObject2.transform;
    transform2.parent = transform1;
    gameObject3.transform.parent = transform2;
    Transform bone1 = CreateBone("shoulder.L", transform1);
    Transform bone2 = CreateBone("upper_arm.L", bone1);
    Transform bone3 = CreateBone("forearm.L", bone2);
    Transform bone4 = CreateBone("hand.L", bone3);
    Transform bone5 = CreateBone("shoulder.R", transform1);
    Transform bone6 = CreateBone("upper_arm.R", bone5);
    Transform bone7 = CreateBone("forearm.R", bone6);
    Transform bone8 = CreateBone("hand.R", bone7);
    Transform bone9 = CreateBone("palm.01.L", bone4);
    CreateBone("f_index.03.L_end", CreateBone("f_index.03.L", CreateBone("f_index.02.L", CreateBone("f_index.01.L", bone9))));
    CreateBone("thumb.03.L_end", CreateBone("thumb.03.L", CreateBone("thumb.02.L", CreateBone("thumb.01.L", bone9))));
    CreateBone("f_middle.03.L_end", CreateBone("f_middle.03.L", CreateBone("f_middle.02.L", CreateBone("f_middle.01.L", CreateBone("palm.02.L", bone4)))));
    Transform bone10 = CreateBone("palm.01.R", bone8);
    CreateBone("f_index.03.R_end", CreateBone("f_index.03.R", CreateBone("f_index.02.R", CreateBone("f_index.01.R", bone10))));
    CreateBone("thumb.03.R_end", CreateBone("thumb.03.R", CreateBone("thumb.02.R", CreateBone("thumb.01.R", bone10))));
    CreateBone("f_middle.03.R_end", CreateBone("f_middle.03.R", CreateBone("f_middle.02.R", CreateBone("f_middle.01.R", CreateBone("palm.02.R", bone8)))));
    CreateBone("Offset", bone3);
    CreateBone("CosmeticsForearm.L", bone3);
    CreateBone("CosmeticsUpperArm.L", bone2);
    CreateBone("Offset", bone7);
    CreateBone("CosmeticsForearm.R", bone7);
    CreateBone("CosmeticsUpperArm.R", bone6);
    CreateBone("TransferrableItemRightShoulder", transform1);
    CreateBone("TransferrableItemLeftShoulder", transform1);
    Transform bone11 = CreateBone("Cosmetics", transform2);
    new GameObject("Holdables").transform.parent = this.GorillaHolder.transform;
    transform1.parent = this.GorillaHolder.transform;
    transform2.localPosition = new Vector3(0.0f, 0.3974f, -6.258488E-07f);
    gameObject3.transform.localPosition = new Vector3(0.0f, 0.1842833f, 0.0f);
    bone1.localPosition = new Vector3(-0.01830291f, 0.3432287f, 0.0790565f);
    bone1.localRotation = new Quaternion(-0.539560437f, 0.350375772f, 0.234246314f, 0.728862166f);
    bone5.localPosition = new Vector3(0.0183029287f, 0.3432287f, 0.0790565f);
    bone5.localRotation = new Quaternion(-0.539560437f, -0.350375772f, -0.234246314f, 0.728862166f);
    bone2.localPosition = new Vector3(-0.0002577677f, 0.145488486f, -0.0259815753f);
    bone2.localRotation = new Quaternion(-0.366254866f, -0.6022762f, 0.6076133f, 0.365960151f);
    bone6.localPosition = new Vector3(0.0002577528f, 0.145488471f, -0.02598156f);
    bone6.localRotation = new Quaternion(0.366254866f, -0.6022762f, 0.6076133f, -0.365960151f);
    bone3.localPosition = new Vector3(4.20422339E-06f, 0.40616706f, -1.04308128E-06f);
    bone3.localRotation = new Quaternion(-0.0621675327f, 0.933610141f, 0.156085163f, 0.316456437f);
    bone7.localPosition = new Vector3(-4.20434E-06f, 0.4061671f, -1.04308128E-06f);
    bone7.localRotation = new Quaternion(0.0621675625f, 0.933610439f, 0.156084955f, -0.316455662f);
    bone4.localPosition = new Vector3(3.0733645E-08f, 0.381689519f, 1.11758709E-08f);
    bone4.localRotation = new Quaternion(-0.171266124f, 0.9682555f, 0.07243276f, -0.167040914f);
    bone8.localPosition = new Vector3(1.39698386E-08f, 0.381689548f, 3.7252903E-09f);
    bone8.localRotation = new Quaternion(0.171266049f, 0.9682557f, 0.0724326149f, 0.167040154f);
    bone11.localPosition = new Vector3(0.0f, -0.171972573f, 0.0344753973f);
    bone11.localRotation = new Quaternion(-0.7524642f, 0.0f, 0.0f, 0.6586332f);
    foreach (Transform componentsInChild in this.GorillaHolder.GetComponentsInChildren<Transform>(true))
    {
      this._nameToMiniBone[((UnityEngine.Object) componentsInChild).name] = componentsInChild;
      ((Component) componentsInChild).gameObject.layer = targetLayer;
    }
    this.Skin = new GameObject("gorilla")
    {
      transform = {
        parent = this.GorillaHolder.transform
      },
      layer = targetLayer
    }.AddComponent<SkinnedMeshRenderer>();
    this.Skin.rootBone = transform1;
    this.Skin.updateWhenOffscreen = true;
    string[] strArray = new string[32 /*0x20*/]
    {
      "body",
      "head",
      "shoulder.L",
      "upper_arm.L",
      "forearm.L",
      "hand.L",
      "palm.01.L",
      "f_index.01.L",
      "f_index.02.L",
      "f_index.03.L",
      "thumb.01.L",
      "thumb.02.L",
      "thumb.03.L",
      "palm.02.L",
      "f_middle.01.L",
      "f_middle.02.L",
      "f_middle.03.L",
      "shoulder.R",
      "upper_arm.R",
      "forearm.R",
      "hand.R",
      "palm.01.R",
      "f_index.01.R",
      "f_index.02.R",
      "f_index.03.R",
      "thumb.01.R",
      "thumb.02.R",
      "thumb.03.R",
      "palm.02.R",
      "f_middle.01.R",
      "f_middle.02.R",
      "f_middle.03.R"
    };
    Transform[] transformArray = new Transform[strArray.Length];
    for (int index = 0; index < strArray.Length; ++index)
      transformArray[index] = this._nameToMiniBone.ContainsKey(strArray[index]) ? this._nameToMiniBone[strArray[index]] : transform1;
    this.Skin.bones = transformArray;
    this._skeletonBuilt = true;

    static Transform CreateBone(string name, Transform parent)
    {
      return new GameObject(name)
      {
        transform = {
          parent = parent
        }
      }.transform;
    }
  }

  private void MapBones(VRRig target)
  {
    this._cachedBoneMap.Clear();
    if (((UnityEngine.Object) target == (UnityEngine.Object) null))
      return;
    this.MapRecursive(((Component) target).transform);
  }

  private void MapRecursive(Transform currentTargetBone)
  {
    Transform transform;
    if (this._nameToMiniBone.TryGetValue(((UnityEngine.Object) currentTargetBone).name, out transform))
      this._cachedBoneMap.Add(new MiniatureRig.BonePair()
      {
        Source = currentTargetBone,
        Mini = transform
      });
    foreach (Transform currentTargetBone1 in currentTargetBone)
      this.MapRecursive(currentTargetBone1);
  }

  private void UpdateVisuals(VRRig target, int targetLayer)
  {
    if ((!((UnityEngine.Object) this.Skin != (UnityEngine.Object) null) ? 0 : (((UnityEngine.Object) target.mainSkin != (UnityEngine.Object) null) ? 1 : 0)) != 0)
    {
      this.Skin.sharedMesh = target.mainSkin.sharedMesh;
      ((Renderer) this.Skin).sharedMaterials = ((Renderer) target.mainSkin).sharedMaterials;
      if (((Renderer) this.Skin).material.HasProperty("_Color"))
        ((Renderer) this.Skin).material.color = target.playerColor;
      if ((!((UnityEngine.Object) this.Skin.sharedMesh != (UnityEngine.Object) null) ? 0 : (this.Skin.bones.Length != this.Skin.sharedMesh.bindposes.Length ? 1 : 0)) != 0)
      {
        Transform[] transformArray = new Transform[this.Skin.sharedMesh.bindposes.Length];
        for (int index = 0; index < transformArray.Length; ++index)
          transformArray[index] = index < this.Skin.bones.Length ? this.Skin.bones[index] : (this._nameToMiniBone.ContainsKey("body") ? this._nameToMiniBone["body"] : this.Skin.rootBone);
        this.Skin.bones = transformArray;
      }
    }
    this.UpdateFace(target, targetLayer);
    this.CloneTargetCosmetics(target, targetLayer);
  }

  private void UpdateFace(VRRig target, int targetLayer)
  {
    if (!this._nameToMiniBone.ContainsKey("head"))
      return;
    Renderer renderer = (Renderer) null;
    Mesh mesh = (Mesh) null;
    try
    {
      FieldInfo field = typeof (VRRig).GetField("faceSkin", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
      if (field != (FieldInfo) null)
        renderer = field.GetValue((object) target) as Renderer;
    }
    catch
    {
    }
    if ((!((UnityEngine.Object) renderer == (UnityEngine.Object) null) ? 0 : (((UnityEngine.Object) target.headMesh != (UnityEngine.Object) null) ? 1 : 0)) != 0)
      renderer = target.headMesh.GetComponent<Renderer>();
    if (((UnityEngine.Object) renderer != (UnityEngine.Object) null))
    {
      if (renderer is SkinnedMeshRenderer skinnedMeshRenderer)
      {
        mesh = skinnedMeshRenderer.sharedMesh;
      }
      else
      {
        MeshFilter component = ((Component) renderer).GetComponent<MeshFilter>();
        if (((UnityEngine.Object) component != (UnityEngine.Object) null))
          mesh = component.sharedMesh;
      }
    }
    if (!((UnityEngine.Object) mesh != (UnityEngine.Object) null))
      return;
    if ((((UnityEngine.Object) this._miniFaceRenderer == (UnityEngine.Object) null) ? 1 : (((UnityEngine.Object) ((Component) this._miniFaceRenderer).gameObject == (UnityEngine.Object) null) ? 1 : 0)) != 0)
    {
      GameObject gameObject = new GameObject("MiniGorillaFace")
      {
        layer = targetLayer
      };
      gameObject.transform.SetParent(this._nameToMiniBone["head"], false);
      gameObject.transform.localPosition = new Vector3(0.0f, -1.64209533f, 0.230855629f);
      gameObject.transform.localRotation = new Quaternion(-0.7524642f, 6.1760006E-09f, 3.54931773E-09f, 0.6586331f);
      gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
      this._miniFaceRenderer = (Renderer) gameObject.AddComponent<MeshRenderer>();
      this._activeCosmetics.Add(gameObject);
    }
    else
    {
      MeshFilter component = ((Component) this._miniFaceRenderer).GetComponent<MeshFilter>();
      if (((UnityEngine.Object) component != (UnityEngine.Object) null))
        component.sharedMesh = mesh;
    }
    if ((!((UnityEngine.Object) this._miniFaceRenderer != (UnityEngine.Object) null) ? 0 : (((UnityEngine.Object) renderer != (UnityEngine.Object) null) ? 1 : 0)) == 0)
      return;
    this._miniFaceRenderer.sharedMaterials = renderer.sharedMaterials;
  }

  private void CloneTargetCosmetics(VRRig target, int targetLayer)
  {
    if (target.cosmetics == null)
      return;
    foreach (GameObject cosmetic in target.cosmetics)
    {
      if ((!((UnityEngine.Object) cosmetic != (UnityEngine.Object) null) ? 0 : (cosmetic.activeSelf ? 1 : 0)) != 0)
      {
        Transform transform1 = cosmetic.transform;
        string key = "";
        int num = 0;
        List<Transform> transformList = new List<Transform>();
        for (; (!((UnityEngine.Object) transform1 != (UnityEngine.Object) null) || !((UnityEngine.Object) transform1 != (UnityEngine.Object) ((Component) target).transform) ? 0 : (num < 15 ? 1 : 0)) != 0; ++num)
        {
          transformList.Add(transform1);
          if (!this._nameToMiniBone.ContainsKey(((UnityEngine.Object) transform1).name))
          {
            transform1 = transform1.parent;
          }
          else
          {
            key = ((UnityEngine.Object) transform1).name;
            break;
          }
        }
        if (!string.IsNullOrEmpty(key))
        {
          Transform transform2 = this._nameToMiniBone[key];
          transformList.Reverse();
          GameObject gameObject1 = ((Component) transform2).gameObject;
          for (int index = 1; index < transformList.Count; ++index)
          {
            Transform realParent = transformList[index];
            GameObject gameObject2 = new GameObject(((UnityEngine.Object) realParent).name)
            {
              layer = targetLayer,
              transform = {
                parent = gameObject1.transform,
                localPosition = realParent.localPosition,
                localRotation = realParent.localRotation,
                localScale = realParent.localScale
              }
            };
            MeshFilter component1 = ((Component) realParent).GetComponent<MeshFilter>();
            if ((!((UnityEngine.Object) component1) ? 0 : (((UnityEngine.Object) component1.sharedMesh) ? 1 : 0)) != 0)
              gameObject2.AddComponent<MeshFilter>().sharedMesh = component1.sharedMesh;
            Renderer component2 = ((Component) realParent).GetComponent<Renderer>();
            if (((UnityEngine.Object) component2))
            {
              Material[] sharedMaterials = component2.sharedMaterials;
              switch (component2)
              {
                case SkinnedMeshRenderer skinnedMeshRenderer2:
                  SkinnedMeshRenderer skinnedMeshRenderer1 = gameObject2.AddComponent<SkinnedMeshRenderer>();
                  skinnedMeshRenderer1.sharedMesh = skinnedMeshRenderer2.sharedMesh;
                  ((Renderer) skinnedMeshRenderer1).sharedMaterials = sharedMaterials;
                  break;
                case MeshRenderer _:
                  ((Renderer) gameObject2.AddComponent<MeshRenderer>()).sharedMaterials = sharedMaterials;
                  break;
              }
            }
            this._activeCosmetics.Add(gameObject2);
            gameObject1 = gameObject2;
            if (index == transformList.Count - 1)
              this.CloneSubCosmetics(realParent, gameObject2.transform, targetLayer);
          }
        }
      }
    }
  }

  private void CloneSubCosmetics(Transform realParent, Transform miniParent, int targetLayer)
  {
    foreach (Transform realParent1 in realParent)
    {
      if ((((UnityEngine.Object) realParent1).name.Contains("NameTag") || ((UnityEngine.Object) realParent1).name.Contains("Speaker") || ((UnityEngine.Object) realParent1).name.Contains("Audio") ? 1 : (((UnityEngine.Object) realParent1).name.Contains("Voice") ? 1 : 0)) == 0)
      {
        MeshFilter component1 = ((Component) realParent1).GetComponent<MeshFilter>();
        if ((!((UnityEngine.Object) component1 != (UnityEngine.Object) null) ? 0 : (((UnityEngine.Object) component1.sharedMesh != (UnityEngine.Object) null) ? 1 : 0)) != 0)
        {
          string name = ((UnityEngine.Object) component1.sharedMesh).name;
          if ((((UnityEngine.Object) realParent1).name.Contains("Anchor") || name.Contains("Cylinder") || name.Contains("Capsule") ? 1 : (name.Contains("Cube") ? 1 : 0)) != 0)
            continue;
        }
        Renderer component2 = ((Component) realParent1).GetComponent<Renderer>();
        int num;
        if (((UnityEngine.Object) component2 != (UnityEngine.Object) null))
        {
          Bounds bounds = component2.bounds;
          Vector3 size = bounds.size;
          num = (double) size.magnitude > 2.0 ? 1 : 0;
        }
        else
          num = 0;
        if (num == 0)
        {
          GameObject gameObject = new GameObject(((UnityEngine.Object) realParent1).name);
          gameObject.layer = targetLayer;
          gameObject.transform.parent = miniParent;
          gameObject.transform.localPosition = realParent1.localPosition;
          gameObject.transform.localRotation = realParent1.localRotation;
          gameObject.transform.localScale = realParent1.localScale;
          gameObject.SetActive(((Component) realParent1).gameObject.activeSelf);
          if (((UnityEngine.Object) component1 != (UnityEngine.Object) null))
            gameObject.AddComponent<MeshFilter>().sharedMesh = component1.sharedMesh;
          if (((UnityEngine.Object) component2 != (UnityEngine.Object) null))
          {
            switch (component2)
            {
              case SkinnedMeshRenderer skinnedMeshRenderer2:
                SkinnedMeshRenderer skinnedMeshRenderer1 = gameObject.AddComponent<SkinnedMeshRenderer>();
                skinnedMeshRenderer1.sharedMesh = skinnedMeshRenderer2.sharedMesh;
                ((Renderer) skinnedMeshRenderer1).sharedMaterials = ((Renderer) skinnedMeshRenderer2).sharedMaterials;
                break;
              case MeshRenderer meshRenderer:
                ((Renderer) gameObject.AddComponent<MeshRenderer>()).sharedMaterials = ((Renderer) meshRenderer).sharedMaterials;
                break;
            }
          }
          this._activeCosmetics.Add(gameObject);
          this.CloneSubCosmetics(realParent1, gameObject.transform, targetLayer);
        }
      }
    }
  }

  public void ClearCosmetics()
  {
    foreach (GameObject activeCosmetic in this._activeCosmetics)
    {
      if (((UnityEngine.Object) activeCosmetic != (UnityEngine.Object) null))
        UnityEngine.Object.Destroy((UnityEngine.Object) activeCosmetic);
    }
    this._activeCosmetics.Clear();
    this._miniFaceRenderer = (Renderer) null;
  }

  public void SyncBonesAndColors()
  {
    if (((UnityEngine.Object) this._currentTarget == (UnityEngine.Object) null))
      return;
    int count = this._cachedBoneMap.Count;
    for (int index = 0; index < count; ++index)
    {
      MiniatureRig.BonePair cachedBone = this._cachedBoneMap[index];
      if ((!((UnityEngine.Object) cachedBone.Source != (UnityEngine.Object) null) ? 0 : (((UnityEngine.Object) cachedBone.Mini != (UnityEngine.Object) null) ? 1 : 0)) != 0)
      {
        string name = ((UnityEngine.Object) cachedBone.Mini).name;
        if ((name == "head" || name.Contains("shoulder") || name.Contains("arm") || name.StartsWith("hand") ? 1 : (name.StartsWith("palm") ? 1 : 0)) == 0)
        {
          cachedBone.Mini.localRotation = cachedBone.Source.localRotation;
          if (name == "body")
            cachedBone.Mini.localPosition = Vector3.zero;
        }
      }
    }
    if ((!((UnityEngine.Object) this.Skin != (UnityEngine.Object) null) ? 0 : (((UnityEngine.Object) this._currentTarget.mainSkin != (UnityEngine.Object) null) ? 1 : 0)) == 0 || !((Renderer) this.Skin).material.HasProperty("_Color"))
      return;
    ((Renderer) this.Skin).material.color = this._currentTarget.playerColor;
  }

  public void Activate()
  {
    this.IsActive = true;
    if (!((UnityEngine.Object) this.GorillaHolder != (UnityEngine.Object) null))
      return;
    this.GorillaHolder.SetActive(true);
  }

  public void DeActivate()
  {
    this.IsActive = false;
    if (!((UnityEngine.Object) this.GorillaHolder != (UnityEngine.Object) null))
      return;
    this.GorillaHolder.SetActive(false);
  }

  public void DestroyRig()
  {
    if (((UnityEngine.Object) this.GorillaHolder != (UnityEngine.Object) null))
      UnityEngine.Object.Destroy((UnityEngine.Object) this.GorillaHolder);
    this._nameToMiniBone.Clear();
    this._cachedBoneMap.Clear();
    this._activeCosmetics.Clear();
    this._skeletonBuilt = false;
  }

  private struct BonePair
  {
    public Transform Source;
    public Transform Mini;
  }
}
