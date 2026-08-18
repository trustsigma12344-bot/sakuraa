using System.Collections.Generic;

#nullable disable
namespace SakuraaCastingMod.Features.Replay.Recording;

public class Replay
{
  public string FormatVersion { get; set; }

  public int FinalTime { get; set; }

  public List<Player> players { get; set; }

  public List<PlayerData> playerDatas { get; set; }

  public List<AudioDatas> audioDatas { get; set; }
}
