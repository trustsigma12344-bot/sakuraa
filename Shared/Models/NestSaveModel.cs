using System;
using System.Collections.Generic;

#nullable disable
namespace SakuraaCastingMod.Shared.Models;

[Serializable]
public class NestSaveModel
{
  public List<GameObjectModel> gameObjectDataList = new List<GameObjectModel>();
}
