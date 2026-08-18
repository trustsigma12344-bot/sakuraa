using System;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Core.Patches;

public static class EventHandlerExtensions
{
  public static void SafeInvoke(this EventHandler handler, object sender, EventArgs e)
  {
    foreach (EventHandler invocation in handler?.GetInvocationList())
    {
      try
      {
        if (invocation != null)
          invocation(sender, e);
      }
      catch (Exception ex)
      {
        UnityEngine.Debug.LogException(ex);
      }
    }
  }

  public static void SafeInvoke<T>(this EventHandler<T> handler, object sender, T e) where T : EventArgs
  {
    foreach (EventHandler<T> invocation in handler?.GetInvocationList())
    {
      try
      {
        if (invocation != null)
          invocation(sender, e);
      }
      catch (Exception ex)
      {
        UnityEngine.Debug.LogException(ex);
      }
    }
  }
}
