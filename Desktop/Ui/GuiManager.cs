using SakuraaCastingMod.Desktop.Ui.Framework;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Desktop.Ui;

public static class GuiManager
{
  public static Dictionary<string, MenuBuilder> AllRegisteredMenus = new Dictionary<string, MenuBuilder>();
  public const float RefWidth = 1920f;
  public const float RefHeight = 1080f;
  public static float DragBoxSize = 20f;
  private static Dictionary<string, GuiManager.MenuInfo> _menuInfoDict = new Dictionary<string, GuiManager.MenuInfo>();
  private static readonly Dictionary<Color, Texture2D> TextureCache = new Dictionary<Color, Texture2D>();
  private static List<GuiManager.SnowFlake> _snowFlakes;
  private static Texture2D _snowTex;
  private const int SnowCount = 100;

  public static Vector2 RefToScreen(Vector2 refPos)
  {
    return new Vector2(refPos.x / 1920f * (float) Screen.width, refPos.y / 1080f * (float) Screen.height);
  }

  public static Vector2 ScreenToRef(Vector2 screenPos)
  {
    return new Vector2((float) ((double) screenPos.x / (double) Screen.width * 1920.0), (float) ((double) screenPos.y / (double) Screen.height * 1080.0));
  }

  public static void RegisterMenu(string id, MenuBuilder menu)
  {
    if ((menu == null ? 0 : (!string.IsNullOrEmpty(id) ? 1 : 0)) == 0)
      return;
    GuiManager.AllRegisteredMenus[id] = menu;
  }

  public static void RegisterMenu(MenuBuilder menu)
  {
    if ((menu == null ? 0 : (!string.IsNullOrEmpty(menu._title) ? 1 : 0)) == 0)
      return;
    GuiManager.RegisterMenu(menu._title, menu);
  }

  public static void UnregisterMenu(string id)
  {
    if (string.IsNullOrEmpty(id))
      return;
    GuiManager.AllRegisteredMenus.Remove(id);
  }

  public static void UnregisterMenu(MenuBuilder menu)
  {
    if ((menu == null ? 0 : (!string.IsNullOrEmpty(menu._title) ? 1 : 0)) == 0)
      return;
    GuiManager.AllRegisteredMenus.Remove(menu._title);
  }

  public static GuiManager.MenuInfo GetMenuInfo(string menuStr)
  {
    GuiManager.MenuInfo menuInfo;
    if (!GuiManager._menuInfoDict.TryGetValue(menuStr, out menuInfo))
    {
      menuInfo = new GuiManager.MenuInfo()
      {
        MenuName = menuStr
      };
      GuiManager._menuInfoDict.Add(menuStr, menuInfo);
    }
    return menuInfo;
  }

  public static void ImpDraggable(Rect menuDragBoxRect, GuiManager.MenuInfo mI)
  {
    Event current = Event.current;
    GUI.Box(menuDragBoxRect, "", GUI.skin.box);
    if (menuDragBoxRect.Contains(current.mousePosition) && ((int) current.type != 0 ? 0 : (current.button == 0 ? 1 : 0)) != 0)
    {
      mI.IsDraggingMenu = true;
      mI.MenuDragOffset = (current.mousePosition - mI.MenuPosition);
      current.Use();
    }
    if (!mI.IsDraggingMenu)
      return;
    mI.MenuPosition.x = current.mousePosition.x - mI.MenuDragOffset.x;
    mI.MenuPosition.y = current.mousePosition.y - mI.MenuDragOffset.y;
    if ((current.type != (EventType) 1 ? 1 : (current.button != 0 ? 1 : 0)) != 0)
      return;
    mI.IsDraggingMenu = false;
    current.Use();
  }

  public static void DrawSnowEffect()
  {
    if (GuiManager._snowFlakes == null)
    {
      GuiManager._snowFlakes = new List<GuiManager.SnowFlake>();
      GuiManager._snowTex = GuiManager.MakeTex(2, 2, Color.white);
      for (int index = 0; index < 100; ++index)
        GuiManager._snowFlakes.Add(GuiManager.CreateFlake(true));
    }
    if (Event.current.type != (EventType) 7)
      return;
    Color color = GUI.color;
    float deltaTime = Time.deltaTime;
    for (int index = 0; index < GuiManager._snowFlakes.Count; ++index)
    {
      GuiManager.SnowFlake snowFlake = GuiManager._snowFlakes[index];
      snowFlake.Y += snowFlake.Speed * deltaTime;
      snowFlake.X += Mathf.Sin(Time.time * 0.5f + (float) index) * 0.5f;
      if ((double) snowFlake.Y > (double) Screen.height)
        snowFlake = GuiManager.CreateFlake(false);
      GuiManager._snowFlakes[index] = snowFlake;
      GUI.color = new Color(1f, 1f, 1f, snowFlake.Alpha);
      GUI.DrawTexture(new Rect(snowFlake.X, snowFlake.Y, snowFlake.Size, snowFlake.Size), (Texture) GuiManager._snowTex);
    }
    GUI.color = color;
  }

  private static GuiManager.SnowFlake CreateFlake(bool randomY)
  {
    return new GuiManager.SnowFlake()
    {
      X = (float) UnityEngine.Random.Range(0, Screen.width),
      Y = randomY ? (float) UnityEngine.Random.Range(0, Screen.height) : -10f,
      Size = UnityEngine.Random.Range(5f, 5f),
      Speed = UnityEngine.Random.Range(30f, 80f),
      Alpha = UnityEngine.Random.Range(0.3f, 0.8f)
    };
  }

  private static Texture2D MakeTex(int width, int height, Color color)
  {
    Texture2D texture2D1;
    Texture2D texture2D2;
    if (!GuiManager.TextureCache.TryGetValue(color, out texture2D1))
    {
      Color[] colorArray = new Color[width * height];
      for (int index = 0; index < colorArray.Length; ++index)
        colorArray[index] = color;
      Texture2D texture2D3 = new Texture2D(width, height);
      texture2D3.SetPixels(colorArray);
      texture2D3.Apply();
      GuiManager.TextureCache[color] = texture2D3;
      texture2D2 = texture2D3;
    }
    else
      texture2D2 = texture2D1;
    return texture2D2;
  }

  private struct SnowFlake
  {
    public float X;
    public float Y;
    public float Size;
    public float Speed;
    public float Alpha;
  }

  public class MenuInfo
  {
    public bool IsDraggingMenu;
    public Vector2 MenuDragOffset = Vector2.zero;
    public string MenuName;
    public Vector2 MenuPosition = new Vector2(0.0f, 0.0f);
  }
}
