using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Desktop.Ui.Framework;

public static class UiAnim
{
  public const float HoverHalfLife = 0.045f;
  public const float PressHalfLife = 0.03f;
  public const float FocusHalfLife = 0.06f;
  public const float OpenSpringHz = 7f;
  public const float ResizeSpringHz = 9f;
  public const float KnobSpringHz = 9f;
  public const float ThemeFadeDuration = 0.25f;
  public const float PressScale = 0.97f;
  public const float ShadowAlpha = 0.35f;
  public static readonly Vector2 ShadowOffset = new Vector2(0.0f, 3f);
  public const float ShadowExpand = 3f;
  public const float HoverVol = 0.04f;
  public const float ClickVol = 0.15f;
  public const float MaxDt = 0.05f;

  public static float EaseOutCubic(float t)
  {
    t = Mathf.Clamp01(t);
    float num = 1f - t;
    return (float) (1.0 - (double) num * (double) num * (double) num);
  }

  public static float EaseInOutCubic(float t)
  {
    t = Mathf.Clamp01(t);
    return (double) t < 0.5 ? 4f * t * t * t : (float) (1.0 - (double) Mathf.Pow((float) (-2.0 * (double) t + 2.0), 3f) / 2.0);
  }

  public static float MoveTowardsExp(float current, float target, float halfLife, float dt)
  {
    float num1;
    if ((double) halfLife <= 0.0)
    {
      num1 = target;
    }
    else
    {
      float num2 = 1f - Mathf.Exp(-0.6931472f * dt / halfLife);
      num1 = current + (target - current) * num2;
    }
    return num1;
  }

  public static void Spring(ref float pos, ref float vel, float target, float hz, float dt)
  {
    if ((double) dt <= 0.0)
      return;
    float num1 = 6.28318548f * hz;
    float num2 = num1 * dt;
    float num3 = (float) (1.0 / (1.0 + (double) num2 + 0.47999998927116394 * (double) num2 * (double) num2 + 0.23499999940395355 * (double) num2 * (double) num2 * (double) num2));
    float num4 = pos - target;
    float num5 = (vel + num1 * num4) * dt;
    vel = (vel - num1 * num5) * num3;
    pos = target + (num4 + num5) * num3;
  }

  public static Color LerpColor(Color a, Color b, float t) => Color.Lerp(a, b, Mathf.Clamp01(t));

  public static Rect ScaleRectCentered(Rect r, float scale)
  {
    float num1 = r.width * scale;
    float num2 = r.height * scale;
    return new Rect(r.x + (float) (((double) r.width - (double) num1) * 0.5), r.y + (float) (((double) r.height - (double) num2) * 0.5), num1, num2);
  }

  public static Rect Expand(Rect r, float by)
  {
    return new Rect(r.x - by, r.y - by, r.width + by * 2f, r.height + by * 2f);
  }
}
