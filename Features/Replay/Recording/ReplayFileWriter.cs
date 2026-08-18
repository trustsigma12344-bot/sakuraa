using Newtonsoft.Json;
using System.IO;

#nullable disable
namespace SakuraaCastingMod.Features.Replay.Recording;

public static class ReplayFileWriter
{
  public static void Write(SakuraaCastingMod.Features.Replay.Recording.Replay replay, string sessionFolderName)
  {
    if (replay == null)
      return;
    string str = JsonConvert.SerializeObject((object) replay);
    foreach (string file in Directory.GetFiles(RecordingPaths.GetSessionFolder(sessionFolderName), "*.rawReplay"))
      File.Delete(file);
    using (StreamWriter streamWriter = new StreamWriter(RecordingPaths.GetRawReplayPath(sessionFolderName), false))
      streamWriter.Write(str);
  }
}
