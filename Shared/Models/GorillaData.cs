using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Shared.Models;

public class GorillaData
{
  public Transform BodyTransform;
  public Color Color;
  public Transform HeadTransform;
  public bool Infected;
  public string UserId;
  public string UserName;
  public VRRig Rig;

  public GorillaData()
  {
  }

  public GorillaData(
    Transform bodyTransform,
    Transform headTransform,
    Color color,
    string userName,
    string userId,
    bool infected)
  {
    this.BodyTransform = bodyTransform;
    this.HeadTransform = headTransform;
    this.Color = color;
    this.UserName = userName;
    this.UserId = userId;
    this.Infected = infected;
  }
}
