using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Shared.Models;

public class UtilTab
{
  public string TabName;
  public Material TabIcon;
  public string Description;
  public List<MenuElement> Elements = new List<MenuElement>();
}
