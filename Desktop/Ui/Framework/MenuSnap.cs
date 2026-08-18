using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Desktop.Ui.Framework;

public static class MenuSnap
{
  private const float GridSize = 25f;
  private const float SnapThreshold = 12f;
  private const float SnapMargin = 20f;
  private static int _activeFrame = -10;
  private static bool _snapX;
  private static bool _snapY;
  private static float _snapLineX;
  private static float _snapLineY;
  private static Texture2D _pixel;

  public static bool Active => Time.frameCount - MenuSnap._activeFrame <= 1;

  private static Texture2D Pixel
  {
    get
    {
      if (((UnityEngine.Object) MenuSnap._pixel == (UnityEngine.Object) null))
      {
        MenuSnap._pixel = new Texture2D(1, 1);
        MenuSnap._pixel.SetPixel(0, 0, Color.white);
        MenuSnap._pixel.Apply();
      }
      return MenuSnap._pixel;
    }
  }

  public static void KeepAlive() => MenuSnap._activeFrame = Time.frameCount;

  public static void Apply(ref float x, ref float y, float w, float h)
  {
    MenuSnap._activeFrame = Time.frameCount;
    x = Mathf.Round(x / 25f) * 25f;
    y = Mathf.Round(y / 25f) * 25f;
    float width = (float) Screen.width;
    float height = (float) Screen.height;
    float num1 = width / 2f;
    float num2 = height / 2f;
    MenuSnap._snapY = false;
    MenuSnap._snapX = false;
    float[] numArray1 = new float[3]{ x, x + w / 2f, x + w };
    float[] numArray2 = new float[3]
    {
      20f,
      num1,
      width - 20f
    };
    float num3 = 12f;
    float num4 = 0.0f;
    for (int index1 = 0; index1 < 3; ++index1)
    {
      for (int index2 = 0; index2 < 3; ++index2)
      {
        float num5 = Mathf.Abs(numArray1[index1] - numArray2[index2]);
        if ((double) num5 < (double) num3)
        {
          num3 = num5;
          num4 = numArray2[index2] - numArray1[index1];
          MenuSnap._snapLineX = numArray2[index2];
          MenuSnap._snapX = true;
        }
      }
    }
    x += num4;
    float[] numArray3 = new float[3]{ y, y + h / 2f, y + h };
    float[] numArray4 = new float[3]
    {
      20f,
      num2,
      height - 20f
    };
    float num6 = 12f;
    float num7 = 0.0f;
    for (int index3 = 0; index3 < 3; ++index3)
    {
      for (int index4 = 0; index4 < 3; ++index4)
      {
        float num8 = Mathf.Abs(numArray3[index3] - numArray4[index4]);
        if ((double) num8 < (double) num6)
        {
          num6 = num8;
          num7 = numArray4[index4] - numArray3[index3];
          MenuSnap._snapLineY = numArray4[index4];
          MenuSnap._snapY = true;
        }
      }
    }
    y += num7;
  }

  public static void DrawOverlay()
  {
    if ((!MenuSnap.Active ? 1 : (Event.current.type != (EventType) 7 ? 1 : 0)) != 0)
      return;
    Color color = GUI.color;
    Texture2D pixel = MenuSnap.Pixel;
    float width = (float) Screen.width;
    float height = (float) Screen.height;
    GUI.color = new Color(0.0f, 0.0f, 0.0f, 0.22f);
    GUI.DrawTexture(new Rect(0.0f, 0.0f, width, height), (Texture) pixel);
    GUI.color = new Color(1f, 1f, 1f, 0.16f);
    for (float num1 = 0.0f; (double) num1 < (double) width; num1 += 25f)
    {
      for (float num2 = 0.0f; (double) num2 < (double) height; num2 += 25f)
        GUI.DrawTexture(new Rect(num1 - 1f, num2 - 1f, 2f, 2f), (Texture) pixel);
    }
    float num3 = width / 2f;
    float num4 = height / 2f;
    GUI.color = new Color(1f, 1f, 1f, 0.28f);
    for (float num5 = 0.0f; (double) num5 < (double) height; num5 += 16f)
      GUI.DrawTexture(new Rect(num3, num5, 1f, 6f), (Texture) pixel);
    for (float num6 = 0.0f; (double) num6 < (double) width; num6 += 16f)
      GUI.DrawTexture(new Rect(num6, num4, 6f, 1f), (Texture) pixel);
    GUI.color = new Color(1f, 1f, 1f, 0.18f);
    for (float num7 = 0.0f; (double) num7 < (double) height; num7 += 16f)
    {
      GUI.DrawTexture(new Rect(20f, num7, 1f, 6f), (Texture) pixel);
      GUI.DrawTexture(new Rect(width - 20f, num7, 1f, 6f), (Texture) pixel);
    }
    for (float num8 = 0.0f; (double) num8 < (double) width; num8 += 16f)
    {
      GUI.DrawTexture(new Rect(num8, 20f, 6f, 1f), (Texture) pixel);
      GUI.DrawTexture(new Rect(num8, height - 20f, 6f, 1f), (Texture) pixel);
    }
    Color menuOutlineColor = MenuConfig.MenuOutlineColor;
    menuOutlineColor.a = 0.7f;
    GUI.color = menuOutlineColor;
    if (MenuSnap._snapX)
      GUI.DrawTexture(new Rect(MenuSnap._snapLineX - 1f, 0.0f, 2f, height), (Texture) pixel);
    if (MenuSnap._snapY)
      GUI.DrawTexture(new Rect(0.0f, MenuSnap._snapLineY - 1f, width, 2f), (Texture) pixel);
    GUI.color = color;
  }
}
