using SakuraaCastingMod.Core;
using SakuraaCastingMod.Desktop.Camera;
using System;

#nullable disable
namespace SakuraaCastingMod.Desktop.Ui.Framework.Menus;

public static class DirectorMenu
{
  public static MenuBuilder _directorMenu;

  public static bool IsInitialized => DirectorMenu._directorMenu != null;

  public static void Draw()
  {
    if (DirectorMenu._directorMenu == null)
      DirectorMenu.Initialize();
    DirectorMenu._directorMenu.Draw();
  }

  public static void Initialize()
  {
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    DirectorMenu._directorMenu = new MenuBuilder("Auto Director", 250f).AddSpace(25f).AddDynamicButton((Func<string>) (() => "Shot: " + Director.CurrentShotLabel), new Action(Director.ForceCut), "Shows the current shot; click to cut to the best shot right now").AddSlider("FOV:", Plugin.fov, 30f, 160f, (Action<float>) (value => Plugin.fov = value), 1, "Adjust field of view").AddSpace().AddSlider("Min Shot:", Director.MinShotSeconds, 1f, 10f, (Action<float>) (v => Director.MinShotSeconds = v), 1, "Shortest time a shot holds before the director may cut away").AddSlider("Max Shot:", Director.MaxShotSeconds, 4f, 30f, (Action<float>) (v => Director.MaxShotSeconds = v), 1, "Longest time a shot holds before a cut is forced for variety").AddSlider("Distance:", Director.Distance, 1.5f, 8f, (Action<float>) (v => Director.Distance = v), 1, "How far the camera sits from the subject").AddSlider("Height:", Director.Height, 0.0f, 4f, (Action<float>) (v => Director.Height = v), 1, "How high above the subject the camera rides").AddSlider("Smoothing:", Director.PosSmoothTime, 0.15f, 1.2f, (Action<float>) (v => Director.PosSmoothTime = v), 2, "How floaty the camera moves; higher is slower and steadier").AddSlider("Pan Speed:", Director.MaxPanSpeed, 40f, 240f, (Action<float>) (v => Director.MaxPanSpeed = v), description: "Fastest the camera is allowed to pan, in degrees per second").AddSpace().AddDynamicToggle("Cut On Tags", (Func<bool>) (() => Director.CutOnTags), (Action<bool>) (v => Director.CutOnTags = v), "Hard-cut to every tag the moment it happens").AddSlider("Tag Beat:", Director.TagBeatSeconds, 1f, 6f, (Action<float>) (v => Director.TagBeatSeconds = v), 1, "How long the tag shot holds before resuming", (Func<bool>) (() => Director.CutOnTags)).AddDynamicToggle("Lead Shots", (Func<bool>) (() => Director.LeadShots), (Action<bool>) (v => Director.LeadShots = v), "Occasionally film chases head-on from in front of the runner").AddDynamicToggle("Wide Shots", (Func<bool>) (() => Director.WideShots), (Action<bool>) (v => Director.WideShots = v), "Pull out to a crowd overview when nothing is happening").AddDynamicToggle("Wall Avoid", (Func<bool>) (() => Director.WallAvoid), (Action<bool>) (v => Director.WallAvoid = v), "Pull the camera in front of geometry that blocks the subject").AddDynamicToggle("Include Local", (Func<bool>) (() => Director.IncludeLocal), (Action<bool>) (v => Director.IncludeLocal = v), "Allow the director to film the casting account too");
  }
}
