using SakuraaCastingMod.Desktop.Ui.Framework;
using SakuraaCastingMod.Desktop.Ui.Framework.Menus;
using SakuraaCastingMod.Shared.Helpers;
using SakuraaCastingMod.VR.UtilMenu.Utility;
using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Features.Overlays;

public static class LayoutEditor
{
  public static bool IsEditing;
  [SavedSetting("LeaderboardPosX", 170f)]
  public static float LeaderboardPosX = 170f;
  [SavedSetting("LeaderboardPosY", -120f)]
  public static float LeaderboardPosY = -120f;
  [SavedSetting("KillFeedOffsetX", 0.0f)]
  public static float KillFeedOffsetX;
  [SavedSetting("KillFeedOffsetY", 0.0f)]
  public static float KillFeedOffsetY;
  [SavedSetting("FpsOffsetX", 0.0f)]
  public static float FpsOffsetX;
  [SavedSetting("FpsOffsetY", 0.0f)]
  public static float FpsOffsetY;
  [SavedSetting("NotificationPosX", 0.0f)]
  public static float NotificationPosX;
  [SavedSetting("NotificationPosY", 0.0f)]
  public static float NotificationPosY;
  private const float GridSize = 25f;
  private const float CornerHitSize = 14f;
  private const float SnapThreshold = 12f;
  private const float SnapMargin = 20f;
  private static List<LayoutEditor.OverlayHandle> _handles;
  private static int _dragIndex = -1;
  private static LayoutEditor.DragType _dragType = LayoutEditor.DragType.None;
  private static Vector2 _dragStart;
  private static Vector2 _accumDelta;
  private static string _copiedNotice;
  private static float _copiedNoticeTimer;
  private static bool _snapX;
  private static bool _snapY;
  private static float _snapLineX;
  private static float _snapLineY;
  private static Texture2D _pixelTex;
  private static GUIStyle _handleLabelStyle;
  private static GUIStyle _titleStyle;
  private static GUIStyle _hintStyle;
  private static GUIStyle _toggleBtnStyle;
  private static GUIStyle _toggleBtnActiveStyle;
  private static bool _themeHooked;

  private static Color Accent(float alpha)
  {
    return new Color(MenuConfig.MenuOutlineColor.r, MenuConfig.MenuOutlineColor.g, MenuConfig.MenuOutlineColor.b, alpha);
  }

  public static void Toggle()
  {
    LayoutEditor.IsEditing = !LayoutEditor.IsEditing;
    LayoutEditor._dragIndex = -1;
    LayoutEditor._dragType = LayoutEditor.DragType.None;
    LayoutEditor._accumDelta = Vector2.zero;
    Sounds.PlayCasterClick(Sounds.boingSfx);
  }

  private static void EnsureInit()
  {
    if (((UnityEngine.Object) LayoutEditor._pixelTex == (UnityEngine.Object) null))
    {
      LayoutEditor._pixelTex = new Texture2D(1, 1);
      LayoutEditor._pixelTex.SetPixel(0, 0, Color.white);
      LayoutEditor._pixelTex.Apply();
    }
    if (LayoutEditor._handles != null)
      return;
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    LayoutEditor._handles = new List<LayoutEditor.OverlayHandle>()
    {
      new LayoutEditor.OverlayHandle()
      {
        Name = "Leaderboard",
        HandleColor = MenuConfig.MenuOutlineColor,
        IsActive = (Func<bool>) (() => Leaderboard.ShowLeaderboard),
        GetRect = new Func<Rect>(LayoutEditor.GetLeaderboardRect),
        ApplyDelta = (Action<Vector2>) (delta =>
        {
          LayoutEditor.LeaderboardPosX += delta.x;
          LayoutEditor.LeaderboardPosY -= delta.y;
        })
      },
      new LayoutEditor.OverlayHandle()
      {
        Name = "MiniMap",
        HandleColor = MenuConfig.MenuOutlineColor,
        IsActive = (Func<bool>) (() => MiniMap.MiniMapEnabled),
        GetRect = new Func<Rect>(LayoutEditor.GetMiniMapRect),
        ApplyDelta = (Action<Vector2>) (delta =>
        {
          Vector2 miniMapPosition = MiniMap.MiniMapPosition;
          miniMapPosition.x += delta.x;
          miniMapPosition.y -= delta.y;
          MiniMap.MiniMapPosition = miniMapPosition;
        })
      },
      new LayoutEditor.OverlayHandle()
      {
        Name = "Kill Feed",
        HandleColor = MenuConfig.MenuOutlineColor,
        IsActive = (Func<bool>) (() => KillFeed.KillFeedEnabled),
        GetRect = new Func<Rect>(LayoutEditor.GetKillFeedRect),
        ApplyDelta = (Action<Vector2>) (delta =>
        {
          LayoutEditor.KillFeedOffsetX += delta.x;
          LayoutEditor.KillFeedOffsetY -= delta.y;
        })
      },
      new LayoutEditor.OverlayHandle()
      {
        Name = "Scoreboard",
        HandleColor = MenuConfig.MenuOutlineColor,
        IsActive = (Func<bool>) (() => Scoreboard.ShowScoreboard),
        GetRect = new Func<Rect>(LayoutEditor.GetScoreboardRect),
        ApplyDelta = (Action<Vector2>) (delta =>
        {
          Vector2 scorePos = Scoreboard._scorePos;
          scorePos.x += delta.x;
          scorePos.y -= delta.y;
          Scoreboard._scorePos = scorePos;
          Scoreboard.TransformScoreboards();
        }),
        CanScale = true,
        ApplyScale = (Action<float>) (delta =>
        {
          Scoreboard.ScoreScale = Mathf.Max(0.2f, Scoreboard.ScoreScale + delta);
          Scoreboard.TransformScoreboards();
        })
      },
      new LayoutEditor.OverlayHandle()
      {
        Name = "FPS Counter",
        HandleColor = MenuConfig.MenuOutlineColor,
        IsActive = (Func<bool>) (() => FpsCounter.ShowFps),
        GetRect = new Func<Rect>(LayoutEditor.GetFpsRect),
        ApplyDelta = (Action<Vector2>) (delta =>
        {
          LayoutEditor.FpsOffsetX += delta.x;
          LayoutEditor.FpsOffsetY += delta.y;
        })
      },
      new LayoutEditor.OverlayHandle()
      {
        Name = "Tag Distance",
        HandleColor = MenuConfig.MenuOutlineColor,
        IsActive = (Func<bool>) (() => LavaDistance.ShowDistanceCard),
        GetRect = new Func<Rect>(LayoutEditor.GetDistanceRect),
        ApplyDelta = (Action<Vector2>) (delta =>
        {
          LavaDistance.DistanceOffsetX += delta.x;
          LavaDistance.DistanceOffsetY -= delta.y;
        })
      },
      new LayoutEditor.OverlayHandle()
      {
        Name = "Mic Toggle",
        HandleColor = MenuConfig.MenuOutlineColor,
        IsActive = (Func<bool>) (() => ToggleMicOverlay.ShowToggleMicIcon),
        GetRect = (Func<Rect>) (() => ToggleMicOverlay.GetDisplayRect()),
        ApplyDelta = (Action<Vector2>) (delta =>
        {
          ToggleMicOverlay.ToggleMicOffsetX += delta.x;
          ToggleMicOverlay.ToggleMicOffsetY -= delta.y;
        })
      },
      new LayoutEditor.OverlayHandle()
      {
        Name = "Notifications",
        HandleColor = MenuConfig.MenuOutlineColor,
        IsActive = (Func<bool>) (() => true),
        GetRect = (Func<Rect>) (() => Notification.GetDisplayRect()),
        ApplyDelta = (Action<Vector2>) (delta =>
        {
          LayoutEditor.NotificationPosX += delta.x;
          LayoutEditor.NotificationPosY += delta.y;
        })
      },
      new LayoutEditor.OverlayHandle()
      {
        Name = "Rewind",
        HandleColor = MenuConfig.MenuOutlineColor,
        IsActive = (Func<bool>) (() => RewindViewer.ShowEditorMenu),
        GetRect = (Func<Rect>) (() => RewindViewer.GetDisplayRect()),
        ApplyDelta = (Action<Vector2>) (delta =>
        {
          RewindViewer.RewindPosX += delta.x;
          RewindViewer.RewindPosY += delta.y;
        }),
        CanScale = true,
        ApplyScale = (Action<float>) (delta => RewindViewer.RewindScale = Mathf.Clamp(RewindViewer.RewindScale + delta, 0.5f, 3f))
      },
      new LayoutEditor.OverlayHandle()
      {
        Name = "Util Menu",
        HandleColor = MenuConfig.MenuOutlineColor,
        IsActive = (Func<bool>) (() => UtilMenuViewer.ShowViewer),
        GetRect = (Func<Rect>) (() => UtilMenuViewer.GetDisplayRect()),
        ApplyDelta = (Action<Vector2>) (delta =>
        {
          UtilMenuViewer.PosX += delta.x;
          UtilMenuViewer.PosY += delta.y;
        }),
        CanScale = true,
        ApplyScale = (Action<float>) (delta => UtilMenuViewer.ViewerScale = Mathf.Clamp(UtilMenuViewer.ViewerScale + delta, 0.5f, 2.5f))
      }
    };
  }

  private static void EnsureStyles()
  {
    if (!LayoutEditor._themeHooked)
    {
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      ThemeManager.OnThemeApplied += new Action(LayoutEditor.InvalidateStyles);
      LayoutEditor._themeHooked = true;
    }
    if (LayoutEditor._handleLabelStyle != null)
      return;
    LayoutEditor._handleLabelStyle = new GUIStyle(GUI.skin.label)
    {
      fontSize = 12,
      fontStyle = (FontStyle) 1,
      alignment = (TextAnchor) 4,
      normal = {
        textColor = Color.white
      }
    };
    LayoutEditor._titleStyle = new GUIStyle(GUI.skin.label)
    {
      fontSize = 20,
      fontStyle = (FontStyle) 1,
      alignment = (TextAnchor) 4,
      normal = {
        textColor = MenuConfig.MenuOutlineColor
      }
    };
    LayoutEditor._hintStyle = new GUIStyle(GUI.skin.label)
    {
      fontSize = 12,
      alignment = (TextAnchor) 4,
      normal = {
        textColor = new Color(1f, 1f, 1f, 0.5f)
      }
    };
    LayoutEditor._toggleBtnStyle = new GUIStyle(MenuConfig.GetButtonStyle())
    {
      fontSize = 12,
      alignment = (TextAnchor) 4,
      padding = new RectOffset(8, 8, 4, 4)
    };
    LayoutEditor._toggleBtnActiveStyle = new GUIStyle(LayoutEditor._toggleBtnStyle)
    {
      normal = {
        textColor = MenuConfig.MenuOutlineColor
      }
    };
  }

  private static void InvalidateStyles()
  {
    LayoutEditor._handleLabelStyle = (GUIStyle) null;
    LayoutEditor._titleStyle = (GUIStyle) null;
    LayoutEditor._hintStyle = (GUIStyle) null;
    LayoutEditor._toggleBtnStyle = (GUIStyle) null;
    LayoutEditor._toggleBtnActiveStyle = (GUIStyle) null;
  }

  private static Rect GetCalibratedRect(LayoutEditor.OverlayHandle handle)
  {
    Rect rect = handle.GetRect();
    return new Rect(rect.x + handle.CalibX, rect.y + handle.CalibY, rect.width + handle.CalibW, rect.height + handle.CalibH);
  }

  private static LayoutEditor.DragType HitTestCorners(Rect rect, Vector2 mouse)
  {
    float num = 14f;
    Rect rect1 = new Rect(rect.x - 7f, rect.y - 7f, num, num);
    LayoutEditor.DragType dragType;
    if (!rect1.Contains(mouse))
    {
      Rect rect2 = new Rect(rect.xMax - num / 2f, rect.y - num / 2f, num, num);
      if (rect2.Contains(mouse))
      {
        dragType = LayoutEditor.DragType.TopRight;
      }
      else
      {
        Rect rect3 = new Rect(rect.x - num / 2f, rect.yMax - num / 2f, num, num);
        if (rect3.Contains(mouse))
        {
          dragType = LayoutEditor.DragType.BottomLeft;
        }
        else
        {
          Rect rect4 = new Rect(rect.xMax - num / 2f, rect.yMax - num / 2f, num, num);
          dragType = !rect4.Contains(mouse) ? LayoutEditor.DragType.None : LayoutEditor.DragType.BottomRight;
        }
      }
    }
    else
      dragType = LayoutEditor.DragType.TopLeft;
    return dragType;
  }

  private static Rect GetLeaderboardRect()
  {
    int num1 = GorillaDataHandler.GorillaDataList.Count;
    if (num1 == 0)
      num1 = 5;
    float num2 = (float) num1 * 36f + LayoutEditor.LeaderboardPosY;
    float num3 = 36f + LayoutEditor.LeaderboardPosY;
    float num4 = (float) ((double) Screen.height - (double) num2 - 28.05000114440918 - 104.0);
    float num5 = (float) ((double) Screen.height - (double) num3 + 28.05000114440918 - 119.0);
    return new Rect((float) ((double) LayoutEditor.LeaderboardPosX - 181.90000915527344 + 20.0), num4, 361.800018f, num5 - num4);
  }

  private static Rect GetMiniMapRect()
  {
    float num = 101f * MiniMap.MiniMapScale;
    Vector2 miniMapPosition = MiniMap.MiniMapPosition;
    return new Rect(miniMapPosition.x - num / 2f, (float) ((double) Screen.height - (double) miniMapPosition.y - (double) num / 2.0), num, num);
  }

  private static Rect GetKillFeedRect()
  {
    int num1 = Mathf.Max(1, KillFeed.ActiveItemCount);
    float num2 = (float) Screen.width - 210f + LayoutEditor.KillFeedOffsetX;
    float num3 = (float) Screen.height + LayoutEditor.KillFeedOffsetY;
    if (Scoreboard.CurrentScoreboard == 2)
      num3 -= 180f;
    float num4 = (float) Screen.height - (num3 - 40f);
    float num5 = (float) ((double) Screen.height - ((double) num3 - (double) num1 * 40.0) + 40.0);
    return new Rect(num2 - 175f, num4, 350f, num5 - num4);
  }

  private static Rect GetScoreboardRect()
  {
    float num1;
    float num2;
    switch (Scoreboard.CurrentScoreboard)
    {
      case 0:
        num1 = 1120f * Scoreboard.ScoreScale;
        num2 = 111f * Scoreboard.ScoreScale;
        break;
      case 2:
        num1 = 800f * Scoreboard.ScoreScale;
        num2 = 400f * Scoreboard.ScoreScale;
        break;
      default:
        num1 = 350f * Scoreboard.ScoreScale;
        num2 = 60f * Scoreboard.ScoreScale;
        break;
    }
    return new Rect(Scoreboard._scorePos.x - num1 / 2f, (float) ((double) Screen.height - (double) Scoreboard._scorePos.y - (double) num2 / 2.0), num1, num2);
  }

  private static Rect GetFpsRect()
  {
    return new Rect((float) ((double) Screen.width - 100.0 - 10.0) + LayoutEditor.FpsOffsetX, 10f + LayoutEditor.FpsOffsetY, 100f, 25f);
  }

  private static Rect GetDistanceRect()
  {
    return new Rect((float) ((double) ((float) Screen.width / 2f + LavaDistance.DistanceOffsetX) - 181.90000915527344 - 43.0), (float) ((double) Screen.height - (double) LavaDistance.DistanceOffsetY - 24.650001525878906 - 29.0), 363.800018f, 49.3000031f);
  }

  public static void DrawToggleButton()
  {
    if (!LayoutEditor.IsEditing)
      return;
    LayoutEditor.EnsureInit();
    LayoutEditor.EnsureStyles();
    float num1 = 100f;
    float num2 = 28f;
    float num3 = 10f;
    Rect rect;
    // ISSUE: explicit constructor call
    rect = new Rect(num3, num3, num1, num2);
    if (!GUI.Button(rect, "Return", LayoutEditor._toggleBtnActiveStyle))
      return;
    LayoutEditor.Toggle();
  }

  public static void Draw()
  {
    if (!LayoutEditor.IsEditing)
      return;
    LayoutEditor.EnsureInit();
    LayoutEditor.EnsureStyles();
    Event current = Event.current;
    bool shift;
    LayoutEditor.DrawEditorOverlay(shift = current.shift);
    float num = 50f;
    Color color = GUI.color;
    GUI.color = new Color(0.0f, 0.0f, 0.0f, 0.55f);
    GUI.DrawTexture(new Rect(0.0f, 0.0f, (float) Screen.width, num), (Texture) LayoutEditor._pixelTex);
    GUI.color = LayoutEditor.Accent(0.6f);
    GUI.DrawTexture(new Rect(0.0f, num, (float) Screen.width, 1f), (Texture) LayoutEditor._pixelTex);
    GUI.color = color;
    GUI.Label(new Rect(0.0f, 4f, (float) Screen.width, 24f), "LAYOUT EDITOR", LayoutEditor._titleStyle);
    GUI.Label(new Rect(0.0f, 26f, (float) Screen.width, 18f), shift ? $"Grid Snap: {(ValueType) 25f}px  |  Snaps to edges & center  |  Release Shift for free movement" : "Drag overlays to reposition  |  Hold Shift for grid snap + edge/center snapping", LayoutEditor._hintStyle);
    LayoutEditor.DrawNormalMode(current, shift);
    if ((double) LayoutEditor._copiedNoticeTimer <= 0.0)
      return;
    LayoutEditor._copiedNoticeTimer -= Time.unscaledDeltaTime;
    GUI.Label(new Rect(0.0f, (float) (Screen.height - 50), (float) Screen.width, 20f), LayoutEditor._copiedNotice, LayoutEditor._titleStyle);
  }

  private static void DrawNormalMode(Event evt, bool shiftHeld)
  {
    for (int index = 0; index < LayoutEditor._handles.Count; ++index)
    {
      LayoutEditor.OverlayHandle handle = LayoutEditor._handles[index];
      if (handle.IsActive())
      {
        Rect calibratedRect = LayoutEditor.GetCalibratedRect(handle);
        bool isDragging = LayoutEditor._dragIndex == index;
        LayoutEditor.DrawHandleBox(calibratedRect, handle.HandleColor, isDragging, false);
        LayoutEditor.DrawHandleLabel(calibratedRect, handle.Name, handle.HandleColor);
        if (handle.CanScale)
          LayoutEditor.DrawScaleCornerHandles(calibratedRect, handle.HandleColor);
        if (((int) evt.type != 0 ? 0 : (evt.button == 0 ? 1 : 0)) != 0)
        {
          if (handle.CanScale)
          {
            LayoutEditor.DragType dragType = LayoutEditor.HitTestCorners(calibratedRect, evt.mousePosition);
            if (dragType != 0)
            {
              LayoutEditor._dragIndex = index;
              LayoutEditor._dragType = dragType;
              LayoutEditor._dragStart = evt.mousePosition;
              LayoutEditor._accumDelta = Vector2.zero;
              evt.Use();
              continue;
            }
          }
          if (calibratedRect.Contains(evt.mousePosition))
          {
            LayoutEditor._dragIndex = index;
            LayoutEditor._dragType = LayoutEditor.DragType.Body;
            LayoutEditor._dragStart = evt.mousePosition;
            LayoutEditor._accumDelta = Vector2.zero;
            evt.Use();
          }
        }
      }
    }
    if ((LayoutEditor._dragIndex < 0 || LayoutEditor._dragIndex >= LayoutEditor._handles.Count ? 0 : (LayoutEditor._dragType != 0 ? 1 : 0)) != 0)
    {
      if (evt.type != (EventType) 3)
      {
        if ((evt.type != (EventType) 1 ? 0 : (evt.button == 0 ? 1 : 0)) != 0)
        {
          LayoutEditor._dragIndex = -1;
          LayoutEditor._dragType = LayoutEditor.DragType.None;
          LayoutEditor._accumDelta = Vector2.zero;
          LayoutEditor._snapY = false;
          LayoutEditor._snapX = false;
          evt.Use();
        }
      }
      else
      {
        Vector2 vector2_1 = (evt.mousePosition - LayoutEditor._dragStart);
        LayoutEditor._dragStart = evt.mousePosition;
        if (LayoutEditor._dragType == LayoutEditor.DragType.Body)
        {
          if (shiftHeld)
          {
            LayoutEditor._accumDelta = (LayoutEditor._accumDelta + vector2_1);
            Vector2 vector2_2;
            // ISSUE: explicit constructor call
            vector2_2 = new Vector2(Mathf.Round(LayoutEditor._accumDelta.x / 25f) * 25f, Mathf.Round(LayoutEditor._accumDelta.y / 25f) * 25f);
            if ((vector2_2 != Vector2.zero))
            {
              LayoutEditor._handles[LayoutEditor._dragIndex].ApplyDelta(vector2_2);
              LayoutEditor._accumDelta = (LayoutEditor._accumDelta - vector2_2);
            }
            LayoutEditor.ApplyEdgeSnap(LayoutEditor._handles[LayoutEditor._dragIndex]);
          }
          else
          {
            LayoutEditor._snapY = false;
            LayoutEditor._snapX = false;
            LayoutEditor._handles[LayoutEditor._dragIndex].ApplyDelta(vector2_1);
          }
        }
        else if ((!LayoutEditor._handles[LayoutEditor._dragIndex].CanScale ? 0 : (LayoutEditor._handles[LayoutEditor._dragIndex].ApplyScale != null ? 1 : 0)) != 0)
        {
          float num = (float) (((double) vector2_1.x + (double) vector2_1.y) * 0.004999999888241291);
          LayoutEditor._handles[LayoutEditor._dragIndex].ApplyScale(num);
        }
        evt.Use();
      }
    }
    if ((LayoutEditor._dragIndex < 0 ? 0 : (LayoutEditor._dragIndex < LayoutEditor._handles.Count ? 1 : 0)) == 0)
      return;
    LayoutEditor.DrawSnapGuides();
    LayoutEditor.OverlayHandle handle1 = LayoutEditor._handles[LayoutEditor._dragIndex];
    Rect calibratedRect1 = LayoutEditor.GetCalibratedRect(handle1);
    LayoutEditor.DrawStatusBar($"{handle1.Name}   X: {Mathf.Round(calibratedRect1.x)}  Y: {Mathf.Round(calibratedRect1.y)}   " + $"{Mathf.Round(calibratedRect1.width)} x {Mathf.Round(calibratedRect1.height)}");
  }

  private static void ApplyEdgeSnap(LayoutEditor.OverlayHandle handle)
  {
    Rect calibratedRect = LayoutEditor.GetCalibratedRect(handle);
    float width = (float) Screen.width;
    float height = (float) Screen.height;
    float num1 = width / 2f;
    float num2 = height / 2f;
    LayoutEditor._snapX = false;
    LayoutEditor._snapY = false;
    float num3 = calibratedRect.x + calibratedRect.width / 2f;
    float num4 = calibratedRect.y + calibratedRect.height / 2f;
    float num5 = float.MaxValue;
    float num6 = float.MaxValue;
    float num7 = 0.0f;
    float num8 = 0.0f;
    float[] numArray1 = new float[3]
    {
      calibratedRect.x,
      num3,
      calibratedRect.xMax
    };
    float[] numArray2 = new float[3]
    {
      20f,
      num1,
      width - 20f
    };
    for (int index1 = 0; index1 < 3; ++index1)
    {
      for (int index2 = 0; index2 < 3; ++index2)
      {
        float num9 = Mathf.Abs(numArray1[index1] - numArray2[index2]);
        if (((double) num9 >= 12.0 ? 0 : ((double) num9 < (double) num5 ? 1 : 0)) != 0)
        {
          num5 = num9;
          num7 = numArray2[index2] - numArray1[index1];
          LayoutEditor._snapLineX = numArray2[index2];
          LayoutEditor._snapX = true;
        }
      }
    }
    float[] numArray3 = new float[3]
    {
      calibratedRect.y,
      num4,
      calibratedRect.yMax
    };
    float[] numArray4 = new float[3]
    {
      20f,
      num2,
      height - 20f
    };
    for (int index3 = 0; index3 < 3; ++index3)
    {
      for (int index4 = 0; index4 < 3; ++index4)
      {
        float num10 = Mathf.Abs(numArray3[index3] - numArray4[index4]);
        if (((double) num10 >= 12.0 ? 0 : ((double) num10 < (double) num6 ? 1 : 0)) != 0)
        {
          num6 = num10;
          num8 = numArray4[index4] - numArray3[index3];
          LayoutEditor._snapLineY = numArray4[index4];
          LayoutEditor._snapY = true;
        }
      }
    }
    if ((LayoutEditor._snapX ? 1 : (LayoutEditor._snapY ? 1 : 0)) == 0)
      return;
    Vector2 vector2;
    // ISSUE: explicit constructor call
    vector2 = new Vector2(LayoutEditor._snapX ? num7 : 0.0f, LayoutEditor._snapY ? num8 : 0.0f);
    handle.ApplyDelta(vector2);
  }

  private static void DrawSnapGuides()
  {
    if ((LayoutEditor._snapX ? 0 : (!LayoutEditor._snapY ? 1 : 0)) != 0)
      return;
    Color color = GUI.color;
    GUI.color = LayoutEditor.Accent(0.5f);
    if (LayoutEditor._snapX)
      GUI.DrawTexture(new Rect(LayoutEditor._snapLineX, 0.0f, 1f, (float) Screen.height), (Texture) LayoutEditor._pixelTex);
    if (LayoutEditor._snapY)
      GUI.DrawTexture(new Rect(0.0f, LayoutEditor._snapLineY, (float) Screen.width, 1f), (Texture) LayoutEditor._pixelTex);
    GUI.color = color;
  }

  private static void DrawStatusBar(string info)
  {
    float num = (float) Screen.height - 28f;
    Color color = GUI.color;
    GUI.color = new Color(0.0f, 0.0f, 0.0f, 0.55f);
    GUI.DrawTexture(new Rect(0.0f, num, (float) Screen.width, 28f), (Texture) LayoutEditor._pixelTex);
    GUI.color = LayoutEditor.Accent(0.6f);
    GUI.DrawTexture(new Rect(0.0f, num, (float) Screen.width, 1f), (Texture) LayoutEditor._pixelTex);
    GUI.color = color;
    GUI.Label(new Rect(0.0f, num + 4f, (float) Screen.width, 20f), info, LayoutEditor._hintStyle);
  }

  private static void DrawEditorOverlay(bool showGrid)
  {
    Color color = GUI.color;
    float num1 = 80f;
    GUI.color = new Color(0.0f, 0.0f, 0.0f, 0.25f);
    GUI.DrawTexture(new Rect(0.0f, 0.0f, (float) Screen.width, num1), (Texture) LayoutEditor._pixelTex);
    GUI.DrawTexture(new Rect(0.0f, (float) Screen.height - num1, (float) Screen.width, num1), (Texture) LayoutEditor._pixelTex);
    GUI.DrawTexture(new Rect(0.0f, num1, num1, (float) Screen.height - 160f), (Texture) LayoutEditor._pixelTex);
    GUI.DrawTexture(new Rect((float) Screen.width - num1, num1, num1, (float) Screen.height - 160f), (Texture) LayoutEditor._pixelTex);
    if (showGrid)
    {
      GUI.color = new Color(1f, 1f, 1f, 0.08f);
      float num2 = 2f;
      for (float num3 = 0.0f; (double) num3 < (double) Screen.width; num3 += 25f)
      {
        for (float num4 = 0.0f; (double) num4 < (double) Screen.height; num4 += 25f)
          GUI.DrawTexture(new Rect(num3 - num2 / 2f, num4 - num2 / 2f, num2, num2), (Texture) LayoutEditor._pixelTex);
      }
      float num5 = (float) Screen.width / 2f;
      float num6 = (float) Screen.height / 2f;
      float num7 = 6f;
      float num8 = 10f;
      GUI.color = new Color(1f, 1f, 1f, 0.1f);
      for (float num9 = 0.0f; (double) num9 < (double) Screen.height; num9 += num7 + num8)
        GUI.DrawTexture(new Rect(num5, num9, 1f, num7), (Texture) LayoutEditor._pixelTex);
      for (float num10 = 0.0f; (double) num10 < (double) Screen.width; num10 += num7 + num8)
        GUI.DrawTexture(new Rect(num10, num6, num7, 1f), (Texture) LayoutEditor._pixelTex);
      GUI.color = new Color(1f, 1f, 1f, 0.06f);
      for (float num11 = 0.0f; (double) num11 < (double) Screen.height; num11 += num7 + num8)
      {
        GUI.DrawTexture(new Rect(20f, num11, 1f, num7), (Texture) LayoutEditor._pixelTex);
        GUI.DrawTexture(new Rect((float) Screen.width - 20f, num11, 1f, num7), (Texture) LayoutEditor._pixelTex);
      }
      for (float num12 = 0.0f; (double) num12 < (double) Screen.width; num12 += num7 + num8)
      {
        GUI.DrawTexture(new Rect(num12, 20f, num7, 1f), (Texture) LayoutEditor._pixelTex);
        GUI.DrawTexture(new Rect(num12, (float) Screen.height - 20f, num7, 1f), (Texture) LayoutEditor._pixelTex);
      }
    }
    float num13 = 1f;
    GUI.color = LayoutEditor.Accent(0.3f);
    GUI.DrawTexture(new Rect(0.0f, 0.0f, (float) Screen.width, num13), (Texture) LayoutEditor._pixelTex);
    GUI.DrawTexture(new Rect(0.0f, (float) Screen.height - num13, (float) Screen.width, num13), (Texture) LayoutEditor._pixelTex);
    GUI.DrawTexture(new Rect(0.0f, 0.0f, num13, (float) Screen.height), (Texture) LayoutEditor._pixelTex);
    GUI.DrawTexture(new Rect((float) Screen.width - num13, 0.0f, num13, (float) Screen.height), (Texture) LayoutEditor._pixelTex);
    GUI.color = color;
  }

  private static void DrawHandleBox(Rect rect, Color color, bool isDragging, bool devMode)
  {
    Color color1 = GUI.color;
    GUI.color = new Color(color.r, color.g, color.b, isDragging ? 0.18f : 0.06f);
    GUI.DrawTexture(rect, (Texture) LayoutEditor._pixelTex);
    float num1 = isDragging ? 2f : 1f;
    float num2 = isDragging ? 1f : 0.6f;
    GUI.color = new Color(color.r, color.g, color.b, num2);
    GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, num1), (Texture) LayoutEditor._pixelTex);
    GUI.DrawTexture(new Rect(rect.x, rect.yMax - num1, rect.width, num1), (Texture) LayoutEditor._pixelTex);
    GUI.DrawTexture(new Rect(rect.x, rect.y, num1, rect.height), (Texture) LayoutEditor._pixelTex);
    GUI.DrawTexture(new Rect(rect.xMax - num1, rect.y, num1, rect.height), (Texture) LayoutEditor._pixelTex);
    if (!devMode)
    {
      float num3 = isDragging ? 8f : 6f;
      GUI.DrawTexture(new Rect(rect.x - 1f, rect.y - 1f, num3, num3), (Texture) LayoutEditor._pixelTex);
      GUI.DrawTexture(new Rect((float) ((double) rect.xMax - (double) num3 + 1.0), rect.y - 1f, num3, num3), (Texture) LayoutEditor._pixelTex);
      GUI.DrawTexture(new Rect(rect.x - 1f, (float) ((double) rect.yMax - (double) num3 + 1.0), num3, num3), (Texture) LayoutEditor._pixelTex);
      GUI.DrawTexture(new Rect((float) ((double) rect.xMax - (double) num3 + 1.0), (float) ((double) rect.yMax - (double) num3 + 1.0), num3, num3), (Texture) LayoutEditor._pixelTex);
    }
    if (isDragging)
    {
      float num4 = rect.x + rect.width / 2f;
      float num5 = rect.y + rect.height / 2f;
      float num6 = 10f;
      GUI.color = new Color(color.r, color.g, color.b, 0.5f);
      GUI.DrawTexture(new Rect(num4 - num6, num5, 20f, 1f), (Texture) LayoutEditor._pixelTex);
      GUI.DrawTexture(new Rect(num4, num5 - num6, 1f, 20f), (Texture) LayoutEditor._pixelTex);
    }
    GUI.color = color1;
  }

  private static void DrawScaleCornerHandles(Rect rect, Color color)
  {
    Color color1 = GUI.color;
    float num1 = 14f;
    float num2 = 7f;
    GUI.color = new Color(color.r, color.g, color.b, 0.8f);
    GUI.DrawTexture(new Rect(rect.x - num2, rect.y - num2, num1, num1), (Texture) LayoutEditor._pixelTex);
    GUI.DrawTexture(new Rect(rect.xMax - num2, rect.y - num2, num1, num1), (Texture) LayoutEditor._pixelTex);
    GUI.DrawTexture(new Rect(rect.x - num2, rect.yMax - num2, num1, num1), (Texture) LayoutEditor._pixelTex);
    GUI.DrawTexture(new Rect(rect.xMax - num2, rect.yMax - num2, num1, num1), (Texture) LayoutEditor._pixelTex);
    GUI.color = color1;
  }

  private static void DrawHandleLabel(Rect rect, string name, Color color)
  {
    float num1 = (float) ((double) name.Length * 7.5 + 20.0);
    float num2 = 20f;
    float num3 = rect.x + (float) (((double) rect.width - (double) num1) / 2.0);
    float num4 = (float) ((double) rect.y - (double) num2 - 6.0);
    if ((double) num4 < 52.0)
      num4 = rect.yMax + 6f;
    Color color1 = GUI.color;
    GUI.color = new Color(0.0f, 0.0f, 0.0f, 0.75f);
    GUI.DrawTexture(new Rect(num3, num4, num1, num2), (Texture) LayoutEditor._pixelTex);
    GUI.color = color;
    GUI.DrawTexture(new Rect(num3, num4, 3f, num2), (Texture) LayoutEditor._pixelTex);
    GUI.color = color1;
    LayoutEditor._handleLabelStyle.normal.textColor = color;
    GUI.Label(new Rect(num3 + 6f, num4, num1 - 6f, num2), name, LayoutEditor._handleLabelStyle);
  }

  private class OverlayHandle
  {
    public string Name;
    public Func<bool> IsActive;
    public Func<Rect> GetRect;
    public Action<Vector2> ApplyDelta;
    public Color HandleColor;
    public bool CanScale;
    public Action<float> ApplyScale;
    public float CalibX;
    public float CalibY;
    public float CalibW;
    public float CalibH;
  }

  private enum DragType
  {
    None,
    Body,
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight,
  }
}
