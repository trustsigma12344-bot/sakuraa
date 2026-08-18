#nullable disable
namespace SakuraaCastingMod.Features.Replay.EditJson;

public class CameraPathSmoothingSettings
{
  public float PercentToStrightStart { get; set; }

  public float PercentToStrightEnd { get; set; }

  public float PercentToCurveStart { get; set; }

  public float PercentToCurveEnd { get; set; }

  public bool UseMainCurveStart { get; set; }

  public bool UseMainCurveEnd { get; set; }
}
