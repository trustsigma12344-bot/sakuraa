using GorillaRigUtil;
using SakuraaCastingMod.Shared.Models;
using System.Collections.Generic;

#nullable disable
namespace SakuraaCastingMod.Features.Replay.compatability;

internal class GorillaRigExposer : GorillaRigExposed
{
  private CustomRig customRig;
  private bool isInfected;
  public static List<GorillaRigExposer> exposers = new List<GorillaRigExposer>();

  public GorillaRigExposer(CustomRig rig, bool isInfected)
  {
    this.customRig = rig;
    this.isInfected = isInfected;
    this.update();
    GorillaRigExposer.exposers.Add(this);
  }

  public void update()
  {
    if (this.customRig != null)
    {
      this.RootBone = this.customRig.WorldObject;
      this.head = this.customRig.WorldObject;
      this.gorillaColor = this.customRig.color;
      this.userName = this.customRig.Name;
      this.ID = this.customRig.ID;
      this.isRigInfected = this.isInfected;
      this.isRigActive = this.customRig.isRigActive;
    }
    else
      this.isRigActive = false;
  }
}
