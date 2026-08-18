using SakuraaCastingMod.Features.Replay.EditJson;
using SakuraaCastingMod.Features.Replay.ReplayManagers;
using SakuraaCastingMod.Features.Replay.utils;
using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Features.Replay.UI;

internal class UiManager
{
  public GorillaController gorillaController;
  internal bool UiON = false;
  internal float VW = (float) (Screen.width / 100);
  internal float VH = (float) (Screen.height / 100);
  internal float TenthSecondPxSpacing = 0.15f;
  private Dictionary<string, Texture2D> ColorTexturCache = new Dictionary<string, Texture2D>();
  private Dictionary<string, GUIStyle> GorillaGuiStyleCache = new Dictionary<string, GUIStyle>();
  internal ButtonRowFlexContainer BottomButtonRow;

  internal ReplayProject replayProject => ReplayManager.replayProject;

  internal float currentTime
  {
    get => this.gorillaController.GetCurrentRealTime();
    set => this.gorillaController.CurrentTime = value;
  }

  internal float SpeedTime
  {
    get => SpeedManager.RealTimeToSpeedTime(this.gorillaController.GetCurrentRealTime());
    set => this.gorillaController.CurrentTime = SpeedManager.SpeedTimeToRealTime(value);
  }

  public UiManager(GorillaController gorillaController)
  {
    this.gorillaController = gorillaController;
    this.BottomButtonRow = new ButtonRowFlexContainer(50f * this.VW, 100f * this.VH, 7f * this.VW, 1f * this.VW, 3f * this.VH);
  }

  internal Texture2D MakeTex(int width, int height, Color col)
  {
    string key = col.r.ToString() + col.g.ToString() + col.b.ToString() + col.a.ToString();
    Texture2D texture2D1;
    try
    {
      texture2D1 = this.ColorTexturCache[key];
      goto label_7;
    }
    catch
    {
    }
    Color[] colorArray = new Color[width * height];
    for (int index = 0; index < colorArray.Length; ++index)
      colorArray[index] = col;
    Texture2D texture2D2 = new Texture2D(width, height);
    texture2D2.SetPixels(colorArray);
    texture2D2.Apply();
    this.ColorTexturCache.Add(key, texture2D2);
    texture2D1 = texture2D2;
label_7:
    return texture2D1;
  }

  internal GUIStyle MakeGuiForGorillaBar(Color col)
  {
    string key = col.r.ToString() + col.g.ToString() + col.b.ToString() + col.a.ToString();
    GUIStyle guiStyle1;
    try
    {
      guiStyle1 = this.GorillaGuiStyleCache[key];
      goto label_6;
    }
    catch
    {
    }
    GUIStyle guiStyle2 = new GUIStyle();
    guiStyle2.normal.background = this.MakeTex(2, 2, col);
    this.GorillaGuiStyleCache.Add(key, guiStyle2);
    if (ContrastUtils.ShouldUseWhiteText(col))
      guiStyle2.normal.textColor = Color.white;
    guiStyle1 = guiStyle2;
label_6:
    return guiStyle1;
  }

  public virtual void OnGUI()
  {
    GUI.depth = 50;
    if (!this.UiON)
      return;
    this.BaseUI();
  }

  public virtual void Update()
  {
  }

  internal virtual void BaseUI()
  {
    if (this.replayProject != null)
      this.BottomButtonRow.Add(GorillaController.Paused ? "Resume" : "Pause", (Action) (() => GorillaController.Paused = !GorillaController.Paused));
    this.BottomButtonRow.RenderButtons();
  }
}
