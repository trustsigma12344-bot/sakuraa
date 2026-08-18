using System.Collections.Generic;

#nullable disable
namespace SakuraaCastingMod.VR.UtilMenu.Pages;

public class SpammerProfile
{
  public string Name { get; set; } = "New Profile";

  public List<string> Ids { get; set; } = new List<string>();

  public string Hand { get; set; } = "Both";
}
