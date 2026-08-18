#nullable disable
namespace SakuraaCastingMod.Features.Replay.EditJson;

public class VoiceKeyFrame : IKeyFrame
{
  public float Time { get; set; }

  public int ActorNumber { get; set; }

  public bool Muted { get; set; }
}
