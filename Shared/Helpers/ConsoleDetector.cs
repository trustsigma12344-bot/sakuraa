using ExitGames.Client.Photon;
using GorillaTag;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Shared.Helpers;

internal static class ConsoleDetector
{
  private const string ConsolePrefix = "%<CONSOLE>%";
  private const string LoadVersionEventKey = "%<CONSOLE>%LoadVersion";
  private const string SyncAssetsPrefix = "%<CONSOLE>%SyncAssets";
  private const byte ConsoleByte = 68;
  private const byte PatreonByte = 63 /*0x3F*/;
  private const int MaxEventNameLen = 256 /*0x0100*/;
  private const int MaxBrandLen = 16 /*0x10*/;
  private const int MaxVersionLen = 12;
  private static readonly string[] SplitToken = new string[1]
  {
    "||"
  };
  private static readonly HashSet<string> ConsoleCommands = new HashSet<string>()
  {
    "isusing",
    "confirmusing",
    "kick",
    "silkick",
    "join",
    "kickall",
    "block",
    "crash",
    "sleep",
    "vibrate",
    "forceenable",
    "toggle",
    "togglemenu",
    "tp",
    "tpnv",
    "tpsmooth",
    "smoothtp",
    "map",
    "nocone",
    "vel",
    "controller",
    "shake",
    "scale",
    "cosmetic",
    "cosmetics",
    "strike",
    "laser",
    "notify",
    "lr",
    "platf",
    "muteall",
    "unmuteall",
    "mute",
    "unmute",
    "rigposition",
    "sb",
    "time",
    "weather",
    "setfog",
    "resetfog",
    "spatial",
    "setmaterial"
  };
  private static readonly HashSet<string> _consoleUsers = new HashSet<string>();
  private static readonly Dictionary<string, string> _consoleBrands = new Dictionary<string, string>();
  private static readonly Dictionary<string, string> _consoleVersions = new Dictionary<string, string>();
  private static bool _wiredClear;

  public static bool IsConsoleUser(string userId)
  {
    return userId != null && ConsoleDetector._consoleUsers.Contains(userId);
  }

  public static string GetVersion(string userId)
  {
    string str;
    return userId != null && ConsoleDetector._consoleVersions.TryGetValue(userId, out str) ? str : (string) null;
  }

  public static string GetLabel(string userId)
  {
    string str1;
    string str2 = userId == null || !ConsoleDetector._consoleBrands.TryGetValue(userId, out str1) ? (string) null : str1;
    string str3 = string.IsNullOrEmpty(str2) ? "CONSOLE SPYWARE" : str2;
    string version = ConsoleDetector.GetVersion(userId);
    return string.IsNullOrEmpty(version) ? str3 : $"{str3} {version}";
  }

  public static void Clear()
  {
    ConsoleDetector._consoleUsers.Clear();
    ConsoleDetector._consoleBrands.Clear();
    ConsoleDetector._consoleVersions.Clear();
  }

  public static void OnEvent(EventData data)
  {
    try
    {
      ConsoleDetector.TryWireClear();
      if (!(data.Parameters.TryGetObject((byte) 245) is object[] objArray))
        return;
      string str1 = (string) null;
      int num1 = 0;
      bool flag = false;
      foreach (object obj in objArray)
      {
        if (obj is string str2 && str1 == null && str2.Length <= 256 /*0x0100*/ && str2.StartsWith("%<CONSOLE>%"))
          str1 = str2;
        else if (obj is int num2 && !flag)
        {
          num1 = num2;
          flag = true;
        }
      }
      if (str1 != null)
      {
        string userId = ConsoleDetector.UserIdOf(data.Sender);
        ConsoleDetector.MarkConsole(userId);
        if (!(str1 == "%<CONSOLE>%LoadVersion"))
        {
          if ((!flag ? 0 : (str1.StartsWith("%<CONSOLE>%SyncAssets") ? 1 : 0)) == 0)
            return;
          string[] strArray = str1.Split(ConsoleDetector.SplitToken, StringSplitOptions.None);
          if ((strArray.Length < 4 ? 0 : (strArray[1] == "confirmusing" ? 1 : 0)) == 0)
            return;
          ConsoleDetector.RecordBrand(ConsoleDetector.UserIdOf(num1), strArray[3], strArray[2], false);
        }
        else
        {
          if (!flag)
            return;
          ConsoleDetector.RecordVersionCode(userId, num1);
        }
      }
      else
      {
        string str3 = default;
        int num3;
        if (objArray.Length >= 1)
        {
          str3 = objArray[0] as string;
          num3 = str3 != null ? 1 : 0;
        }
        else
          num3 = 0;
        if (num3 == 0)
          return;
        if ((data.Code != (byte) 68 ? 0 : (ConsoleDetector.ConsoleCommands.Contains(str3) || str3.StartsWith("asset-", StringComparison.Ordinal) ? 1 : (str3.StartsWith("game-", StringComparison.Ordinal) ? 1 : 0))) == 0)
        {
          if ((data.Code != (byte) 63 /*0x3F*/ || !(str3 == "indicator") || objArray.Length != 2 ? 0 : (objArray[1] is bool ? 1 : 0)) == 0)
            return;
          ConsoleDetector.RecordBrand(ConsoleDetector.UserIdOf(data.Sender), "seralyth", (string) null, true);
        }
        else
        {
          ConsoleDetector.MarkConsole(ConsoleDetector.UserIdOf(data.Sender));
          string menuName = default;
          string version = default;
          int num4;
          if (str3 == "confirmusing" && objArray.Length >= 3 && objArray[1] is string str4)
          {
            version = str4;
            menuName = objArray[2] as string;
            num4 = menuName != null ? 1 : 0;
          }
          else
            num4 = 0;
          if (num4 == 0)
            return;
          ConsoleDetector.RecordBrand(ConsoleDetector.UserIdOf(data.Sender), menuName, version, true);
        }
      }
    }
    catch
    {
    }
  }

  private static void MarkConsole(string userId)
  {
    if (userId == null)
      return;
    ConsoleDetector._consoleUsers.Add(userId);
  }

  private static void RecordVersionCode(string userId, int code)
  {
    if (userId == null)
      return;
    ConsoleDetector._consoleUsers.Add(userId);
    string str = ConsoleDetector.FormatVersionCode(code);
    if ((str == null ? 0 : (!ConsoleDetector._consoleVersions.ContainsKey(userId) ? 1 : 0)) == 0)
      return;
    ConsoleDetector._consoleVersions[userId] = str;
  }

  private static void RecordBrand(
    string userId,
    string menuName,
    string version,
    bool trustedPresence)
  {
    if (userId == null)
      return;
    if (trustedPresence)
      ConsoleDetector._consoleUsers.Add(userId);
    else if (!ConsoleDetector._consoleUsers.Contains(userId))
      return;
    string str1 = ConsoleDetector.SanitizeBrand(menuName);
    if ((str1 == null ? 0 : (str1 != "CONSOLE" ? 1 : 0)) != 0)
      ConsoleDetector._consoleBrands[userId] = str1;
    string str2 = ConsoleDetector.SanitizeVersion(version);
    if (str2 == null)
      return;
    ConsoleDetector._consoleVersions[userId] = str2;
  }

  private static string FormatVersionCode(int code)
  {
    return (code < 0 ? 1 : (code > 999999 ? 1 : 0)) != 0 ? (string) null : $"{code / 100}.{code / 10 % 10}.{code % 10}";
  }

  private static string SanitizeBrand(string s)
  {
    string str;
    if (!string.IsNullOrEmpty(s))
    {
      StringBuilder stringBuilder = new StringBuilder(16 /*0x10*/);
      foreach (char c in s)
      {
        if (stringBuilder.Length < 16 /*0x10*/)
        {
          if ((c >= '0' && c <= '9' || c >= 'A' && c <= 'Z' ? 1 : (c < 'a' ? 0 : (c <= 'z' ? 1 : 0))) != 0)
            stringBuilder.Append(char.ToUpperInvariant(c));
        }
        else
          break;
      }
      str = stringBuilder.Length == 0 ? (string) null : stringBuilder.ToString();
    }
    else
      str = (string) null;
    return str;
  }

  private static string SanitizeVersion(string s)
  {
    string str;
    if (string.IsNullOrEmpty(s))
    {
      str = (string) null;
    }
    else
    {
      StringBuilder stringBuilder = new StringBuilder(12);
      foreach (char ch in s)
      {
        if (stringBuilder.Length < 12)
        {
          if ((ch < '0' || ch > '9' ? (ch == '.' ? 1 : 0) : 1) != 0)
            stringBuilder.Append(ch);
        }
        else
          break;
      }
      str = stringBuilder.Length == 0 ? (string) null : stringBuilder.ToString();
    }
    return str;
  }

  private static string UserIdOf(int actor)
  {
    Player player = PhotonNetwork.CurrentRoom?.GetPlayer(actor, false);
    return string.IsNullOrEmpty(player?.UserId) ? (string) null : player.UserId;
  }

  private static void TryWireClear()
  {
    if ((ConsoleDetector._wiredClear ? 1 : (((UnityEngine.Object) NetworkSystem.Instance == (UnityEngine.Object) null) ? 1 : 0)) != 0)
      return;
    ConsoleDetector._wiredClear = true;
    DelegateListProcessor returnedToSinglePlayer = NetworkSystem.Instance.OnReturnedToSinglePlayer;
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    Action action = new Action(ConsoleDetector.Clear);
    ref Action local = ref action;
    ((ListProcessor<Action>) returnedToSinglePlayer).Add(ref local);
  }
}
