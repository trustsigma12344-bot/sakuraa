using System;
using System.Text;

#nullable disable
namespace SakuraaCastingMod.Features.Replay.utils;

public class RandomUtils
{
  private static Random random = new Random();

  public static string GenerateRandomString(int length)
  {
    StringBuilder stringBuilder = new StringBuilder(length);
    for (int index = 0; index < length; ++index)
      stringBuilder.Append("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz"[RandomUtils.random.Next("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz".Length)]);
    return stringBuilder.ToString();
  }

  public static string GenerateRandomNumberString(int length)
  {
    StringBuilder stringBuilder = new StringBuilder(length);
    for (int index = 0; index < length; ++index)
      stringBuilder.Append("123456789"[RandomUtils.random.Next("123456789".Length)]);
    return stringBuilder.ToString();
  }
}
