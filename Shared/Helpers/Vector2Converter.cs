using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Shared.Helpers;

public class Vector2Converter : JsonConverter<Vector2>
{
  public override void WriteJson(
    JsonWriter writer,
    Vector2 value,
    JsonSerializer serializer)
  {
    JObject jobject = new JObject();
    jobject.Add("x", (value.x));
    jobject.Add("y", (value.y));
    ((JToken) jobject).WriteTo(writer, Array.Empty<JsonConverter>());
  }

  public override Vector2 ReadJson(
    JsonReader reader,
    Type objectType,
    Vector2 existingValue,
    bool hasExistingValue,
    JsonSerializer serializer)
  {
    JObject jobject = JObject.Load(reader);
    return new Vector2(Newtonsoft.Json.Linq.Extensions.Value<float>((IEnumerable<JToken>) jobject["x"]), Newtonsoft.Json.Linq.Extensions.Value<float>((IEnumerable<JToken>) jobject["y"]));
  }
}
