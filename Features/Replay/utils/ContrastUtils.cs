using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Features.Replay.utils;

public static class ContrastUtils
{
  public static bool ShouldUseWhiteText(Color bg)
  {
    float r = bg.r;
    float g = bg.g;
    float b = bg.b;
    float num1;
    float num2;
    float num3;
    if (QualitySettings.activeColorSpace != 0)
    {
      num1 = Mathf.Clamp01(r);
      num2 = Mathf.Clamp01(g);
      num3 = Mathf.Clamp01(b);
    }
    else
    {
      num1 = ContrastUtils.SrgbToLinear(Mathf.Clamp01(r));
      num2 = ContrastUtils.SrgbToLinear(Mathf.Clamp01(g));
      num3 = ContrastUtils.SrgbToLinear(Mathf.Clamp01(b));
    }
    float num4 = (float) (0.2125999927520752 * (double) num1 + 0.71520000696182251 * (double) num2 + 0.0722000002861023 * (double) num3);
    return 1.0499999523162842 / ((double) num4 + 0.05000000074505806) >= ((double) num4 + 0.05000000074505806) / 0.05000000074505806;
  }

  public static Color IdealTextColor(Color bg)
  {
    return !ContrastUtils.ShouldUseWhiteText(bg) ? Color.black : Color.white;
  }

  private static float SrgbToLinear(float c)
  {
    return (double) c > 0.040449999272823334 ? Mathf.Pow((float) (((double) c + 0.054999999701976776) / 1.0549999475479126), 2.4f) : c / 12.92f;
  }
}
