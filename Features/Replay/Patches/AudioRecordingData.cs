using System.Collections.Generic;

#nullable disable
namespace SakuraaCastingMod.Features.Replay.Patches;

public class AudioRecordingData
{
  public List<float> linearBuffer = new List<float>();
  public int highestLinearPosition = -1;
  public int lastPlayhead = -1;
  public int totalWraps = 0;
  public int bufferSize = 48000;
  public int readAheadSamples = 1000;
  public int updateCount = 0;
  public float startTime = 0.0f;
  public float[] tempReadBuffer = new float[1000];
}
