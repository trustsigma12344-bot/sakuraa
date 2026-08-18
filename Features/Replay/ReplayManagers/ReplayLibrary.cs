using BepInEx;
using Newtonsoft.Json;
using SakuraaCastingMod.Features.Replay.EditJson;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

#nullable disable
namespace SakuraaCastingMod.Features.Replay.ReplayManagers;

public static class ReplayLibrary
{
  public static string ReplaysRoot => Path.Combine(Paths.BepInExRootPath, "replays");

  public static List<ReplayEntry> Scan()
  {
    List<ReplayEntry> replayEntryList1 = new List<ReplayEntry>();
    List<ReplayEntry> replayEntryList2;
    if (Directory.Exists(ReplayLibrary.ReplaysRoot))
    {
      foreach (string directory in Directory.GetDirectories(ReplayLibrary.ReplaysRoot))
      {
        string[] files = Directory.GetFiles(directory, "*.rawReplay");
        if (files.Length != 0)
        {
          ReplayEntry replayEntry = new ReplayEntry();
          replayEntry.FolderPath = directory;
          replayEntry.RawReplayPath = files[0];
          string room;
          DateTime when;
          ReplayLibrary.ParseFolderName(directory, out room, out when);
          replayEntry.DisplayName = room;
          replayEntry.SortKey = when;
          replayEntry.DateText = when == DateTime.MinValue ? "" : when.ToString("yyyy-MM-dd  HH:mm");
          replayEntryList1.Add(replayEntry);
        }
      }
      replayEntryList1.Sort((Comparison<ReplayEntry>) ((a, b) => b.SortKey.CompareTo(a.SortKey)));
      replayEntryList2 = replayEntryList1;
    }
    else
      replayEntryList2 = replayEntryList1;
    return replayEntryList2;
  }

  public static void LoadDetails(ReplayEntry entry)
  {
    if (entry.DetailsLoaded)
      return;
    entry.DetailsLoaded = true;
    try
    {
      SakuraaCastingMod.Features.Replay.ReplayJson.Replay replay = JsonConvert.DeserializeObject<SakuraaCastingMod.Features.Replay.ReplayJson.Replay>(File.ReadAllText(entry.RawReplayPath));
      if (replay == null)
        return;
      entry.PlayerCount = replay.players != null ? replay.players.Count : 0;
      entry.DurationSeconds = (float) replay.FinalTime / 100f;
    }
    catch
    {
    }
  }

  public static void Delete(ReplayEntry entry)
  {
    if ((entry == null ? 1 : (string.IsNullOrEmpty(entry.FolderPath) ? 1 : 0)) != 0)
      return;
    try
    {
      ReplayProject replayProject = ReplayManager.replayProject;
      if ((replayProject == null ? 0 : (replayProject.replayInfos != null ? 1 : 0)) != 0)
      {
        foreach (ReplayInfo replayInfo in replayProject.replayInfos)
        {
          if (replayInfo.ReplayFolderName == entry.FolderPath)
          {
            ReplayManager.UnloadProject();
            break;
          }
        }
      }
      if (!Directory.Exists(entry.FolderPath))
        return;
      Directory.Delete(entry.FolderPath, true);
    }
    catch
    {
    }
  }

  private static void ParseFolderName(string folder, out string room, out DateTime when)
  {
    string fileName = Path.GetFileName(folder);
    DateTime result;
    if (fileName.Length >= 19 && DateTime.TryParseExact(fileName.Substring(0, 19), "yyyy-MM-dd_HH-mm-ss", (IFormatProvider) CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
    {
      when = result;
      room = fileName.Length > 20 ? fileName.Substring(20) : fileName;
    }
    else
    {
      room = fileName;
      try
      {
        when = Directory.GetLastWriteTime(folder);
      }
      catch
      {
        when = DateTime.MinValue;
      }
    }
  }
}
