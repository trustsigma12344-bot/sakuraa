using SakuraaCastingMod.Features.Overlays;
using System.ComponentModel;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Api;

[Browsable(true)]
public static class Notifications
{
  public static void Send(string message) => Notification.Send(message);

  public static void Send(string message, Color color) => Notification.Send(message, color);
}
