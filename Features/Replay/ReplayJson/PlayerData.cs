using System.Collections.Generic;

#nullable disable
namespace SakuraaCastingMod.Features.Replay.ReplayJson;

public class PlayerData
{
  public int actorNumber { get; set; }

  public int taggedLen { get; set; }

  public List<int> tagstimes { get; set; }

  public MovementData movementData { get; set; }

  public List<CosmeticsData> cosmeticsDatas { get; set; }

  public List<float> talkingtimes { get; set; }
}
