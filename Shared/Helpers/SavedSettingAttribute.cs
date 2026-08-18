using System;

#nullable disable
namespace SakuraaCastingMod.Shared.Helpers;

[AttributeUsage(AttributeTargets.Field)]
public class SavedSettingAttribute : Attribute
{
  public string Key { get; }

  public object DefaultValue { get; }

  public SavedSettingAttribute(string key = null, object defaultValue = null)
  {
    this.Key = key;
    this.DefaultValue = defaultValue;
  }
}
