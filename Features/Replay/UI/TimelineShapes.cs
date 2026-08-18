using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Features.Replay.UI;

internal static class TimelineShapes
{
  private static Texture2D _circle;
  private static readonly Color Edge = new Color(0.0f, 0.0f, 0.0f, 0.55f);

  private static Texture2D Circle
  {
    get
    {
      if (((UnityEngine.Object) TimelineShapes._circle == (UnityEngine.Object) null))
        TimelineShapes._circle = TimelineShapes.MakeCircle(48 /*0x30*/);
      return TimelineShapes._circle;
    }
  }

  private static Texture2D MakeCircle(int size)
  {
    Texture2D texture2D = new Texture2D(size, size, (TextureFormat) 5, false);
    ((Texture) texture2D).wrapMode = (TextureWrapMode) 1;
    float num1 = (float) size / 2f;
    for (int index1 = 0; index1 < size; ++index1)
    {
      for (int index2 = 0; index2 < size; ++index2)
      {
        float num2 = (float) index2 + 0.5f - num1;
        float num3 = (float) index1 + 0.5f - num1;
        float num4 = Mathf.Sqrt((float) ((double) num2 * (double) num2 + (double) num3 * (double) num3));
        float num5 = Mathf.Clamp01(num1 - num4);
        texture2D.SetPixel(index2, index1, new Color(1f, 1f, 1f, num5));
      }
    }
    texture2D.Apply();
    return texture2D;
  }

  public static void Square(Rect rect, Color fill)
  {
    Color color = GUI.color;
    GUI.color = TimelineShapes.Edge;
    GUI.DrawTexture(rect, (Texture) Texture2D.whiteTexture);
    GUI.color = fill;
    GUI.DrawTexture(new Rect(rect.x + 1.5f, rect.y + 1.5f, rect.width - 3f, rect.height - 3f), (Texture) Texture2D.whiteTexture);
    GUI.color = color;
  }

  public static void Dot(Rect rect, Color fill)
  {
    Color color = GUI.color;
    GUI.color = TimelineShapes.Edge;
    GUI.DrawTexture(rect, (Texture) TimelineShapes.Circle);
    GUI.color = fill;
    GUI.DrawTexture(new Rect(rect.x + 1.5f, rect.y + 1.5f, rect.width - 3f, rect.height - 3f), (Texture) TimelineShapes.Circle);
    GUI.color = color;
  }

  public static void Line(Rect rect, Color color)
  {
    Color color1 = GUI.color;
    GUI.color = color;
    GUI.DrawTexture(rect, (Texture) Texture2D.whiteTexture);
    GUI.color = color1;
  }
}
