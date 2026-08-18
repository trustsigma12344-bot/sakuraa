#nullable disable
namespace SakuraaCastingMod.Features.Replay.EditJson;

public class ReplayInfo
{
  public SakuraaCastingMod.Features.Replay.ReplayJson.Replay replay { get; set; }

  public float StartOffsetTime { get; set; }

  public string ReplayFolderName { get; set; }
}
