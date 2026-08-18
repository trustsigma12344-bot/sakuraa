using System.Collections.Generic;

#nullable disable
namespace SakuraaCastingMod.Features.Replay.Recording;

public class MovementData
{
  public List<float> headPositions { get; set; }

  public List<float> leftHandPositions { get; set; }

  public List<float> rightHandPositions { get; set; }

  public List<float> headRot { get; set; }

  public List<float> leftHandRot { get; set; }

  public List<float> rightHandRot { get; set; }
}
