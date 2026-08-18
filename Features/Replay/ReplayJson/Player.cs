using System.Collections.Generic;

#nullable disable
namespace SakuraaCastingMod.Features.Replay.ReplayJson;

public class Player
{
  public string id { get; set; }

  public int actornumber { get; set; }

  public List<float> color { get; set; }

  public string Name { get; set; }

  public List<int> JoinTimes { get; set; }

  public List<int> LeaveTimes { get; set; }
}
