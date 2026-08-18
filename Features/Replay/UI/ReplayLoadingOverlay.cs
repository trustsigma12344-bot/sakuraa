using SakuraaCastingMod.Desktop.Ui.Framework;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Features.Replay.UI;

internal static class ReplayLoadingOverlay
{
  private static ReplayLoadingOverlay.Phase _phase = ReplayLoadingOverlay.Phase.Idle;
  private static float _target;
  private static float _shown;
  private static string _stage = "";
  private static string _title = "Loading Replay";
  private static bool _indeterminate;
  private static float _alpha;
  private static float _sweep;
  private static float _holdTimer;
  private static double _lastTime = -1.0;
  private static GUIStyle _titleStyle;
  private static GUIStyle _stageStyle;
  private static GUIStyle _pctStyle;

  public static bool Active
  {
    get
    {
      return ReplayLoadingOverlay._phase != ReplayLoadingOverlay.Phase.Idle || (double) ReplayLoadingOverlay._alpha > 1.0 / 1000.0;
    }
  }

  public static void Begin(string title = "Loading Replay")
  {
    ReplayLoadingOverlay._phase = ReplayLoadingOverlay.Phase.Loading;
    ReplayLoadingOverlay._title = title;
    ReplayLoadingOverlay._stage = "Preparing…";
    ReplayLoadingOverlay._target = 0.0f;
    ReplayLoadingOverlay._shown = 0.0f;
    ReplayLoadingOverlay._indeterminate = false;
    ReplayLoadingOverlay._sweep = 0.0f;
    ReplayLoadingOverlay._holdTimer = 0.0f;
    ReplayLoadingOverlay._lastTime = -1.0;
  }

  public static void SetStage(string stage) => ReplayLoadingOverlay._stage = stage;

  public static void SetIndeterminate(bool on) => ReplayLoadingOverlay._indeterminate = on;

  public static void SetProgress(float p) => ReplayLoadingOverlay._target = Mathf.Clamp01(p);

  public static void Finish()
  {
    ReplayLoadingOverlay._target = 1f;
    ReplayLoadingOverlay._indeterminate = false;
    ReplayLoadingOverlay._stage = "Ready";
    ReplayLoadingOverlay._holdTimer = 0.4f;
    ReplayLoadingOverlay._phase = ReplayLoadingOverlay.Phase.Finishing;
  }

  public static void Fail(string message)
  {
    ReplayLoadingOverlay._indeterminate = false;
    ReplayLoadingOverlay._stage = message;
    ReplayLoadingOverlay._holdTimer = 1.6f;
    ReplayLoadingOverlay._phase = ReplayLoadingOverlay.Phase.Failing;
  }

  public static void Cancel() => ReplayLoadingOverlay._phase = ReplayLoadingOverlay.Phase.Idle;

  public static void Draw()
  {
    if (!ReplayLoadingOverlay.Active)
      return;
    if (Event.current.type == (EventType) 7)
    {
      double sinceStartupAsDouble = Time.realtimeSinceStartupAsDouble;
      if (ReplayLoadingOverlay._lastTime < 0.0)
        ReplayLoadingOverlay._lastTime = sinceStartupAsDouble;
      float dt = Mathf.Min((float) (sinceStartupAsDouble - ReplayLoadingOverlay._lastTime), 0.05f);
      ReplayLoadingOverlay._lastTime = sinceStartupAsDouble;
      ReplayLoadingOverlay.Advance(dt);
    }
    if ((double) ReplayLoadingOverlay._alpha <= 1.0 / 1000.0)
      return;
    ReplayLoadingOverlay.DrawOverlay();
  }

  private static void Advance(float dt)
  {
    bool flag = ReplayLoadingOverlay._phase == ReplayLoadingOverlay.Phase.Idle;
    ReplayLoadingOverlay._alpha = Mathf.MoveTowards(ReplayLoadingOverlay._alpha, flag ? 0.0f : 1f, dt / 0.18f);
    ReplayLoadingOverlay._sweep += dt * 0.85f;
    if ((double) ReplayLoadingOverlay._sweep > 1.0)
      --ReplayLoadingOverlay._sweep;
    if (ReplayLoadingOverlay._indeterminate)
    {
      ReplayLoadingOverlay._shown = Mathf.MoveTowards(ReplayLoadingOverlay._shown, 0.1f, dt * 0.4f);
    }
    else
    {
      ReplayLoadingOverlay._shown = Mathf.Lerp(ReplayLoadingOverlay._shown, ReplayLoadingOverlay._target, 1f - Mathf.Exp(-12f * dt));
      if ((double) Mathf.Abs(ReplayLoadingOverlay._target - ReplayLoadingOverlay._shown) < 0.0040000001899898052)
        ReplayLoadingOverlay._shown = ReplayLoadingOverlay._target;
    }
    switch (ReplayLoadingOverlay._phase)
    {
      case ReplayLoadingOverlay.Phase.Finishing:
        if ((double) ReplayLoadingOverlay._shown < 0.99900001287460327)
          break;
        ReplayLoadingOverlay._holdTimer -= dt;
        if ((double) ReplayLoadingOverlay._holdTimer > 0.0)
          break;
        ReplayLoadingOverlay._phase = ReplayLoadingOverlay.Phase.Idle;
        break;
      case ReplayLoadingOverlay.Phase.Failing:
        ReplayLoadingOverlay._holdTimer -= dt;
        if ((double) ReplayLoadingOverlay._holdTimer > 0.0)
          break;
        ReplayLoadingOverlay._phase = ReplayLoadingOverlay.Phase.Idle;
        break;
    }
  }

  private static void DrawOverlay()
  {
    ReplayLoadingOverlay.EnsureStyles();
    float a = Mathf.Clamp01(ReplayLoadingOverlay._alpha);
    Color color = GUI.color;
    GUI.color = Color.white;
    int width = Screen.width;
    int height = Screen.height;
    ReplayLoadingOverlay.Fill(new Rect(0.0f, 0.0f, (float) width, (float) height), new Color(0.0f, 0.0f, 0.0f, 0.55f * a), 0.0f);
    float num1 = Mathf.Min(460f, (float) width - 40f);
    float num2 = 152f;
    Rect r1;
    // ISSUE: explicit constructor call
    r1 = new Rect((float) (((double) width - (double) num1) * 0.5), (float) (((double) height - (double) num2) * 0.5), num1, num2);
    int cornerRadius = MenuConfig.CornerRadius;
    ReplayLoadingOverlay.Fill(new Rect(r1.x - 2f, r1.y + 5f, r1.width + 4f, r1.height + 4f), new Color(0.0f, 0.0f, 0.0f, 0.35f * a), (float) (cornerRadius + 2));
    ReplayLoadingOverlay.Fill(r1, ReplayLoadingOverlay.WithA(MenuConfig.OutlineColorNow, 0.95f * a), (float) cornerRadius);
    Color fillColorNow = MenuConfig.FillColorNow;
    fillColorNow.a = 0.96f * a;
    ReplayLoadingOverlay.Fill(ReplayLoadingOverlay.Inset(r1, 1.6f), fillColorNow, Mathf.Max(0.0f, (float) cornerRadius - 1.6f));
    float num3 = 26f;
    float num4 = r1.width - 52f;
    ReplayLoadingOverlay.SetColor(ReplayLoadingOverlay._titleStyle, ReplayLoadingOverlay.WithA(MenuConfig.TextColor, a));
    GUI.Label(new Rect(r1.x + num3, r1.y + 20f, num4, 26f), ReplayLoadingOverlay._title, ReplayLoadingOverlay._titleStyle);
    ReplayLoadingOverlay.SetColor(ReplayLoadingOverlay._stageStyle, ReplayLoadingOverlay.WithA(MenuConfig.TextColor, 0.72f * a));
    GUI.Label(new Rect(r1.x + num3, r1.y + 52f, num4, 22f), ReplayLoadingOverlay._stage ?? "", ReplayLoadingOverlay._stageStyle);
    float num5 = 12f;
    Rect r2;
    // ISSUE: explicit constructor call
    r2 = new Rect(r1.x + num3, (float) ((double) r1.y + (double) r1.height - 44.0), num4, num5);
    float radius = 6f;
    ReplayLoadingOverlay.Fill(r2, ReplayLoadingOverlay.WithA(MenuConfig.SliderBgColor, a), radius);
    Color hoverColorNow = MenuConfig.HoverColorNow;
    if (ReplayLoadingOverlay._indeterminate)
    {
      GUI.BeginGroup(r2);
      float num6 = r2.width * 0.3f;
      ReplayLoadingOverlay.Fill(new Rect(ReplayLoadingOverlay._sweep * (r2.width + num6) - num6, 0.0f, num6, num5), ReplayLoadingOverlay.WithA(hoverColorNow, 0.9f * a), radius);
      GUI.EndGroup();
    }
    else
    {
      float num7 = r2.width * Mathf.Clamp01(ReplayLoadingOverlay._shown);
      if ((double) num7 > 0.5)
        ReplayLoadingOverlay.Fill(new Rect(r2.x, r2.y, Mathf.Max(num5, num7), num5), ReplayLoadingOverlay.WithA(hoverColorNow, a), radius);
      ReplayLoadingOverlay.SetColor(ReplayLoadingOverlay._pctStyle, ReplayLoadingOverlay.WithA(MenuConfig.TextColor, 0.85f * a));
      GUI.Label(new Rect(r1.x + num3, r2.y - 23f, num4, 20f), Mathf.RoundToInt(Mathf.Clamp01(ReplayLoadingOverlay._shown) * 100f).ToString() + "%", ReplayLoadingOverlay._pctStyle);
    }
    GUI.color = color;
  }

  private static void EnsureStyles()
  {
    if (ReplayLoadingOverlay._titleStyle != null)
      return;
    ReplayLoadingOverlay._titleStyle = new GUIStyle(MenuConfig.GetHeaderStyle())
    {
      alignment = (TextAnchor) 0,
      wordWrap = false
    };
    ReplayLoadingOverlay._stageStyle = new GUIStyle(MenuConfig.GetLabelStyle());
    ReplayLoadingOverlay._pctStyle = new GUIStyle(MenuConfig.GetLabelStyle())
    {
      alignment = (TextAnchor) 5
    };
  }

  private static void SetColor(GUIStyle s, Color c) => s.normal.textColor = c;

  private static Color WithA(Color c, float a)
  {
    c.a *= a;
    return c;
  }

  private static Rect Inset(Rect r, float n)
  {
    return new Rect(r.x + n, r.y + n, r.width - n * 2f, r.height - n * 2f);
  }

  private static void Fill(Rect r, Color color, float radius)
  {
    if ((double) color.a <= 1.0 / 1000.0)
      return;
    GUI.DrawTexture(r, (Texture) Texture2D.whiteTexture, (ScaleMode) 0, true, 0.0f, color, 0.0f, radius);
  }

  private enum Phase
  {
    Idle,
    Loading,
    Finishing,
    Failing,
  }
}
