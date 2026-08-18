using System;

#nullable disable
namespace SakuraaCastingMod.Shared.Helpers;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
public sealed class IntroducedInAttribute : Attribute
{
  public string Version { get; }

  public IntroducedInAttribute(string version) => this.Version = version;
}
