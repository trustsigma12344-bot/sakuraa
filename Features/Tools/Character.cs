using GorillaNetworking;
using HarmonyLib;
using SakMerge.Api;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Features.Tools;

public static class Character
{
  private static string _newName = "USERNAME";
  private static Traverse _computerTraverse;
  private static Traverse _colorCursorLine;
  private static readonly Dictionary<string, GorillaKeyboardButton> KeyDict = new Dictionary<string, GorillaKeyboardButton>();

  public static string GetCurrentName() => Character._newName;

  public static void SetNewName(string name) => Character._newName = name;

  public static void TriggerUpdateName() => Character.UpdateName();

  public static void InitFakeComputer()
  {
    Character._computerTraverse = Traverse.Create((object) GorillaComputer.instance);
    Character._colorCursorLine = Character._computerTraverse.Field("colorCursorLine");
    foreach (GorillaKeyboardButton gorillaKeyboardButton in UnityEngine.Object.FindObjectsOfType<GorillaKeyboardButton>())
    {
      string key = ((GorillaKeyButton<GorillaKeyboardBindings>) gorillaKeyboardButton).characterString;
      if (!Character.KeyDict.ContainsKey(key))
        Character.KeyDict.Add(key, gorillaKeyboardButton);
    }
  }

  private static void UpdateName()
  {
    Character._newName = Character._newName.ToUpper();
    Rigs.SetName(Character._newName);
  }
}
