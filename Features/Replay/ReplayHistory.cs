using Newtonsoft.Json;
using SakuraaCastingMod.Features.Replay.EditJson;
using SakuraaCastingMod.Features.Replay.ReplayManagers;
using System;
using System.Collections.Generic;

#nullable disable
namespace SakuraaCastingMod.Features.Replay;

internal static class ReplayHistory
{
  private const int MaxDepth = 50;
  private static readonly List<ReplayHistory.Snapshot> _undo = new List<ReplayHistory.Snapshot>();
  private static readonly List<ReplayHistory.Snapshot> _redo = new List<ReplayHistory.Snapshot>();
  public static Action OnRestored;

  public static bool CanUndo => ReplayHistory._undo.Count > 0;

  public static bool CanRedo => ReplayHistory._redo.Count > 0;

  public static void Record()
  {
    ReplayHistory.Snapshot snapshot = ReplayHistory.Snapshot.Capture();
    if (snapshot == null)
      return;
    ReplayHistory._undo.Add(snapshot);
    if (ReplayHistory._undo.Count > 50)
      ReplayHistory._undo.RemoveAt(0);
    ReplayHistory._redo.Clear();
  }

  public static void Undo()
  {
    if (ReplayHistory._undo.Count == 0)
      return;
    ReplayHistory.Snapshot snapshot1 = ReplayHistory.Snapshot.Capture();
    if (snapshot1 != null)
      ReplayHistory._redo.Add(snapshot1);
    ReplayHistory.Snapshot snapshot2 = ReplayHistory._undo[ReplayHistory._undo.Count - 1];
    ReplayHistory._undo.RemoveAt(ReplayHistory._undo.Count - 1);
    snapshot2.Restore();
    Action onRestored = ReplayHistory.OnRestored;
    if (onRestored == null)
      return;
    onRestored();
  }

  public static void Redo()
  {
    if (ReplayHistory._redo.Count == 0)
      return;
    ReplayHistory.Snapshot snapshot1 = ReplayHistory.Snapshot.Capture();
    if (snapshot1 != null)
      ReplayHistory._undo.Add(snapshot1);
    ReplayHistory.Snapshot snapshot2 = ReplayHistory._redo[ReplayHistory._redo.Count - 1];
    ReplayHistory._redo.RemoveAt(ReplayHistory._redo.Count - 1);
    snapshot2.Restore();
    Action onRestored = ReplayHistory.OnRestored;
    if (onRestored == null)
      return;
    onRestored();
  }

  public static void Clear()
  {
    ReplayHistory._undo.Clear();
    ReplayHistory._redo.Clear();
  }

  private static void ReplaceContents<T>(List<T> target, List<T> source)
  {
    if (target == null)
      return;
    target.Clear();
    if (source == null)
      return;
    target.AddRange((IEnumerable<T>) source);
  }

  private static List<T> Clone<T>(List<T> list)
  {
    return list != null ? JsonConvert.DeserializeObject<List<T>>(JsonConvert.SerializeObject((object) list)) : new List<T>();
  }

  private class Snapshot
  {
    private List<CameraKeyFrame> _cameras;
    private List<SpeedKeyframe> _speeds;
    private List<VoiceKeyFrame> _voices;

    public static ReplayHistory.Snapshot Capture()
    {
      ReplayProject replayProject = ReplayManager.replayProject;
      ReplayHistory.Snapshot snapshot;
      if (replayProject != null)
        snapshot = new ReplayHistory.Snapshot()
        {
          _cameras = ReplayHistory.Clone<CameraKeyFrame>(replayProject.CameraKeyFrames),
          _speeds = ReplayHistory.Clone<SpeedKeyframe>(replayProject.SpeedKeyframes),
          _voices = ReplayHistory.Clone<VoiceKeyFrame>(replayProject.VoiceKeyFrames)
        };
      else
        snapshot = (ReplayHistory.Snapshot) null;
      return snapshot;
    }

    public void Restore()
    {
      ReplayProject replayProject = ReplayManager.replayProject;
      if (replayProject == null)
        return;
      ReplayHistory.ReplaceContents<CameraKeyFrame>(replayProject.CameraKeyFrames, ReplayHistory.Clone<CameraKeyFrame>(this._cameras));
      ReplayHistory.ReplaceContents<SpeedKeyframe>(replayProject.SpeedKeyframes, ReplayHistory.Clone<SpeedKeyframe>(this._speeds));
      ReplayHistory.ReplaceContents<VoiceKeyFrame>(replayProject.VoiceKeyFrames, ReplayHistory.Clone<VoiceKeyFrame>(this._voices));
    }
  }
}
