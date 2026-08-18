using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Shared.Helpers;

public static class FriendNameCache
{
  private const string FileName = "friend_names.dat";
  private const string UnknownLiteral = "Unknown";
  private static Dictionary<string, string> _names;
  private static bool _loaded;
  private static bool _writePending;

  private static string CachePath
  {
    get => Path.Combine(Application.persistentDataPath, "friend_names.dat");
  }

  private static void EnsureLoaded()
  {
    if (FriendNameCache._loaded)
      return;
    FriendNameCache._loaded = true;
    FriendNameCache._names = new Dictionary<string, string>((IEqualityComparer<string>) StringComparer.Ordinal);
    try
    {
      if (!File.Exists(FriendNameCache.CachePath))
        return;
      string str = File.ReadAllText(FriendNameCache.CachePath);
      if (string.IsNullOrWhiteSpace(str))
        return;
      Dictionary<string, string> dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(str);
      if (dictionary == null)
        return;
      FriendNameCache._names = new Dictionary<string, string>((IDictionary<string, string>) dictionary, (IEqualityComparer<string>) StringComparer.Ordinal);
    }
    catch (Exception ex)
    {
      UnityEngine.Debug.LogError((object) ("[FriendNameCache] Load failed: " + ex.Message));
    }
  }

  public static void Remember(string friendLinkId, string userName)
  {
    if (string.IsNullOrEmpty(friendLinkId) || (string.IsNullOrEmpty(userName) ? 1 : (userName == "Unknown" ? 1 : 0)) != 0)
      return;
    FriendNameCache.EnsureLoaded();
    string str;
    if ((!FriendNameCache._names.TryGetValue(friendLinkId, out str) ? 0 : (str == userName ? 1 : 0)) != 0)
      return;
    FriendNameCache._names[friendLinkId] = userName;
    FriendNameCache.ScheduleSave();
  }

  public static string Resolve(string friendLinkId, string liveName, string fallback = "Unknown")
  {
    string str1;
    if ((string.IsNullOrEmpty(liveName) ? 1 : (liveName == "Unknown" ? 1 : 0)) != 0)
    {
      FriendNameCache.EnsureLoaded();
      string str2 = default;
      str1 = (string.IsNullOrEmpty(friendLinkId) || !FriendNameCache._names.TryGetValue(friendLinkId, out str2) ? 0 : (!string.IsNullOrEmpty(str2) ? 1 : 0)) != 0 ? str2 : fallback;
    }
    else
      str1 = liveName;
    return str1;
  }

  public static bool HasCachedName(string friendLinkId)
  {
    bool flag;
    if (string.IsNullOrEmpty(friendLinkId))
    {
      flag = false;
    }
    else
    {
      FriendNameCache.EnsureLoaded();
      string str;
      flag = FriendNameCache._names.TryGetValue(friendLinkId, out str) && !string.IsNullOrEmpty(str);
    }
    return flag;
  }

  private static void ScheduleSave()
  {
    if (FriendNameCache._writePending)
      return;
    FriendNameCache._writePending = true;
    try
    {
      File.WriteAllText(FriendNameCache.CachePath, JsonConvert.SerializeObject((object) FriendNameCache._names, (Formatting) 1));
    }
    catch (Exception ex)
    {
      UnityEngine.Debug.LogError((object) ("[FriendNameCache] Save failed: " + ex.Message));
    }
    finally
    {
      FriendNameCache._writePending = false;
    }
  }
}
