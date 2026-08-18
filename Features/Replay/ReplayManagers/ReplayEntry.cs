using System;

#nullable disable
namespace SakuraaCastingMod.Features.Replay.ReplayManagers;

public class ReplayEntry
{
  public string FolderPath;
  public string RawReplayPath;
  public string DisplayName;
  public string DateText;
  public DateTime SortKey;
  public bool DetailsLoaded;
  public int PlayerCount;
  public float DurationSeconds;
}
