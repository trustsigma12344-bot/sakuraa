using BepInEx;
using System;
using System.IO;

#nullable disable
namespace SakuraaCastingMod.Features.Replay.Recording;

public static class RecordingPaths
{
  public static string ReplaysRoot => Path.Combine(Paths.BepInExRootPath, "replays");

  public static string Sanitize(string name)
  {
    string str;
    if (string.IsNullOrEmpty(name))
    {
      str = "Unknown";
    }
    else
    {
      foreach (char invalidFileNameChar in Path.GetInvalidFileNameChars())
        name = name.Replace(invalidFileNameChar, '-');
      str = name.Replace(" ", "-");
    }
    return str;
  }

  public static string BuildSessionFolderName(string roomName, DateTime startedAt)
  {
    return $"{startedAt.ToString("yyyy-MM-dd_HH-mm-ss")}_{RecordingPaths.Sanitize(roomName)}";
  }

  public static string GetSessionFolder(string sessionFolderName)
  {
    string path = Path.Combine(RecordingPaths.ReplaysRoot, sessionFolderName);
    Directory.CreateDirectory(path);
    return path;
  }

  public static string GetRawReplayPath(string sessionFolderName)
  {
    return Path.Combine(RecordingPaths.GetSessionFolder(sessionFolderName), sessionFolderName + ".rawReplay");
  }

  public static string GetAudioFilePath(string sessionFolderName, string userKey)
  {
    return Path.Combine(RecordingPaths.GetSessionFolder(sessionFolderName), userKey + ".wav");
  }
}
