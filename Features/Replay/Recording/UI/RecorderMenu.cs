using Photon.Pun;
using SakuraaCastingMod.Core;
using SakuraaCastingMod.Desktop.Ui;
using SakuraaCastingMod.Desktop.Ui.Framework;
using System;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Features.Replay.Recording.UI;

internal static class RecorderMenu
{
  private static MenuBuilder _menu;

  public static void Draw()
  {
    if (!FeatureToggles.ShowReplaySystem || !RecorderSystemBase.RecorderEnabled || !MainMenus.ShowMainMenu)
      return;
    if (RecorderMenu._menu == null)
    {
      RecorderMenu._menu = new MenuBuilder("Recorder", 260f).SetPositionRef(1060f, 400f);
      RecorderMenu.Build();
    }
    RecorderMenu._menu.Draw();
  }

  private static void Build()
  {
    RecorderMenu._menu.AddToggle("Auto-record on join", RecorderSystemBase.AutoRecordOnMatch, (Action<bool>) (v => RecorderSystemBase.AutoRecordOnMatch = v), "Automatically start recording when you join a multiplayer room.");
    RecorderMenu._menu.AddToggle("Record Voice", RecorderSystemBase.RecordVoice, (Action<bool>) (v => RecorderSystemBase.RecordVoice = v), "Capture player voices in room to the replay.");
    RecorderMenu._menu.AddSlider("Capture Rate:", RecorderSystemBase.CaptureHz, 5f, 30f, (Action<float>) (v => RecorderSystemBase.CaptureHz = v), description: "Movement snapshots per second (default 20). Higher = smoother but larger files.");
    RecorderMenu._menu.AddSection("Manual");
    RecorderMenu._menu.AddHoldButton("Start Recording", 0.5f, (Action) (() =>
    {
      if (!((UnityEngine.Object) RecorderSystemBase.Instance != (UnityEngine.Object) null))
        return;
      RecorderSystemBase.Instance.ManualStart();
    }), "Hold to start recording (you must be in a room).", (Func<bool>) (() => ((UnityEngine.Object) RecorderSystemBase.Instance != (UnityEngine.Object) null) && !RecorderSystemBase.Instance.IsRecording && PhotonNetwork.InRoom));
    RecorderMenu._menu.AddHoldButton("Stop Recording", 0.5f, (Action) (() =>
    {
      if (!((UnityEngine.Object) RecorderSystemBase.Instance != (UnityEngine.Object) null))
        return;
      RecorderSystemBase.Instance.ManualStop();
    }), "Hold to stop and save the current recording.", (Func<bool>) (() => ((UnityEngine.Object) RecorderSystemBase.Instance != (UnityEngine.Object) null) && RecorderSystemBase.Instance.IsRecording));
    RecorderMenu._menu.AddDynamicLabel((Func<string>) (() =>
    {
      RecorderSystemBase instance = RecorderSystemBase.Instance;
      return ((UnityEngine.Object) instance == (UnityEngine.Object) null) ? "Status: unavailable" : (instance.IsRecording ? $"Recording  {(ValueType) ((float) instance.ReplayTime / 100f):0.0}s" : "Status: idle");
    }));
    RecorderMenu._menu.AddDynamicLabel((Func<string>) (() =>
    {
      RecorderSystemBase instance = RecorderSystemBase.Instance;
      string replayContainerFolder = ((UnityEngine.Object) instance != (UnityEngine.Object) null) ? instance.ReplayContainerFolder : "";
      return string.IsNullOrEmpty(replayContainerFolder) ? "Output: BepInEx/replays" : "Out: " + replayContainerFolder;
    }));
  }
}
