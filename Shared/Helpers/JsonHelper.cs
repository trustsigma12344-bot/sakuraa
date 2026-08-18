using Newtonsoft.Json;

#nullable disable
namespace SakuraaCastingMod.Shared.Helpers;

public static class JsonHelper
{
  public static T[] FromJson<T>(string json)
  {
    return JsonConvert.DeserializeObject<JsonHelper.Wrapper<T>>($"{{ \"items\": {json}}}").items;
  }

  private class Wrapper<T>
  {
    public T[] items;
  }
}
