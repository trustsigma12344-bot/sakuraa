using SakuraaCastingMod.Core;
using System.ComponentModel;

#nullable disable
namespace SakuraaCastingMod.Api;

[Browsable(true)]
public static class Config
{
  public static System.Collections.Generic.List<string> List() => Configuration.GetProfiles();

  public static void Load(string name) => Configuration.LoadProfile(name);
}
