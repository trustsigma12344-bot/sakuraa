using System.Collections.Generic;

#nullable disable
namespace SakuraaCastingMod.Features.Replay.ReplayJson;

public class Replay
{
  public string ID { get; set; }

  public string FormatVersion { get; set; }

  public int FinalTime { get; set; }

  public int playerInfoLen { get; set; }

  public List<Player> players { get; set; }

  public int playerDataLen { get; set; }

  public List<PlayerData> playerDatas { get; set; }

  public List<AudioDatas> audioDatas { get; set; }
}
