using System.Collections.Generic;

#nullable disable
namespace SakuraaCastingMod.Features.Replay.ReplayJson;

public class MovementData
{
  public int headLen { get; set; }

  public List<float> headPositions { get; set; }

  public int leftHandPosLen { get; set; }

  public List<float> leftHandPositions { get; set; }

  public int rightHandPosLen { get; set; }

  public List<float> rightHandPositions { get; set; }

  public int headRotLen { get; set; }

  public List<float> headRot { get; set; }

  public int leftHandRotLen { get; set; }

  public List<float> leftHandRot { get; set; }

  public int rightHandRotLen { get; set; }

  public List<float> rightHandRot { get; set; }
}
