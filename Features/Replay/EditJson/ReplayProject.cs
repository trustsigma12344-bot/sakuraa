using System.Collections.Generic;

#nullable disable
namespace SakuraaCastingMod.Features.Replay.EditJson;

public class ReplayProject
{
  public string FormatVersion { get; set; }

  public List<ReplayInfo> replayInfos { get; set; }

  public List<CameraKeyFrame> CameraKeyFrames { get; set; }

  public List<SpeedKeyframe> SpeedKeyframes { get; set; }

  public List<VoiceKeyFrame> VoiceKeyFrames { get; set; }
}
