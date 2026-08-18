using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Shared.Helpers;

public static class ExtraTools
{
  public static Material MakeMaterialTransparent(Material material)
  {
    material.shader = Shader.Find("GorillaTag/UberShader");
    material.SetInt("_SrcBlend", 5);
    material.SetInt("_DstBlend", 10);
    material.SetInt("_ZWrite", 0);
    material.renderQueue = 3000;
    return material;
  }

  public static string[] CustomSplit(string str, string delimiter)
  {
    string[] strArray;
    if ((string.IsNullOrEmpty(str) ? 1 : (string.IsNullOrEmpty(delimiter) ? 1 : 0)) != 0)
    {
      strArray = new string[1]{ str };
    }
    else
    {
      List<string> stringList = new List<string>();
      int startIndex;
      int num;
      for (startIndex = 0; (num = str.IndexOf(delimiter, startIndex, StringComparison.Ordinal)) != -1; startIndex = num + delimiter.Length)
        stringList.Add(str.Substring(startIndex, num - startIndex));
      stringList.Add(str.Substring(startIndex));
      strArray = stringList.ToArray();
    }
    return strArray;
  }

  public static string UpperFirst(string text)
  {
    return char.ToUpper(text[0]).ToString() + (text.Length > 1 ? text.Substring(1).ToLower() : string.Empty);
  }

  public static int Clamp(int value, int min, int max)
  {
    return value >= min ? (value <= max ? value : max) : min;
  }

  public static float Clamp(float value, float min, float max)
  {
    return (double) value >= (double) min ? ((double) value > (double) max ? max : value) : min;
  }

  public static double Clamp(double value, double min, double max)
  {
    return value >= min ? (value <= max ? value : max) : min;
  }
}
