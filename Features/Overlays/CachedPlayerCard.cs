using TMPro;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
namespace SakuraaCastingMod.Features.Overlays;

public class CachedPlayerCard
{
  public GameObject GameObject;
  public Transform Transform;
  public GameObject RankObject;
  public Image RankImage;
  public GameObject SteamIcon;
  public GameObject MetaIcon;
  public Image BackgroundImage;
  public TMP_Text NumberText;
  public TMP_Text NameText;

  public CachedPlayerCard(GameObject obj)
  {
    this.GameObject = obj;
    this.Transform = obj.transform;
    this.RankObject = ((Component) this.Transform.Find("rank")).gameObject;
    if (((UnityEngine.Object) this.RankObject != (UnityEngine.Object) null))
      this.RankImage = this.RankObject.GetComponent<Image>();
    this.SteamIcon = ((Component) this.Transform.Find("steam")).gameObject;
    this.MetaIcon = ((Component) this.Transform.Find("meta")).gameObject;
    Transform transform1 = this.Transform.Find("Behind");
    if (((UnityEngine.Object) transform1 != (UnityEngine.Object) null))
      this.BackgroundImage = ((Component) transform1.Find("Image")).GetComponent<Image>();
    Transform transform2 = this.Transform.Find("PlayerNumberText");
    if (((UnityEngine.Object) transform2 != (UnityEngine.Object) null))
      this.NumberText = ((Component) transform2).GetComponent<TMP_Text>();
    Transform transform3 = this.Transform.Find("PlayerNameText");
    if (!((UnityEngine.Object) transform3 != (UnityEngine.Object) null))
      return;
    this.NameText = ((Component) transform3).GetComponent<TMP_Text>();
  }
}
