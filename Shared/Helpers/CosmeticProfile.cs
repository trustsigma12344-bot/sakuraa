using System;
using System.Collections.Generic;

#nullable disable
namespace SakuraaCastingMod.Shared.Helpers;

[Serializable]
public class CosmeticProfile
{
  public string ProfileName;
  public List<string> ItemIds = new List<string>();
  public List<SavedCosmeticItem> SidedItems = new List<SavedCosmeticItem>();
  public float[] Color;
}
