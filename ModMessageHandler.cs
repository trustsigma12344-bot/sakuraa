using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SakuraaCastingMod.Features.Soundboard;
using SakuraaCastingMod.VR.UtilMenu.Utility;
using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod;

public static class ModMessageHandler
{
  private static Dictionary<string, Action<JObject>> _listeners = new Dictionary<string, Action<JObject>>();
  private static bool _initialized = false;

  public static void Initialize()
  {
    if (ModMessageHandler._initialized)
      return;
    ModMessageHandler._initialized = true;
    Console.WriteLine("[ModMessageHandler] Initialized");
    if (ModMessageHandler.IsConnected())
      Console.WriteLine("[ModMessageHandler] WebSocket connected!");
    else
      Console.WriteLine("[ModMessageHandler] WebSocket not connected");
  }

  public static bool IsConnected()
  {
    try
    {
      return WebSocketBridge.IsConnected();
    }
    catch (Exception ex)
    {
      return false;
    }
  }

  public static bool Send(string action, object data = null)
  {
    bool flag;
    if (ModMessageHandler.IsConnected())
    {
      try
      {
        var data1 = new
        {
          handler = "mod",
          action = action,
          data = data
        };
        flag = WebSocketBridge.SendMessage(JsonConvert.SerializeObject((object) data1));
      }
      catch (Exception ex)
      {
        Console.WriteLine("[ModMessageHandler] Error sending message: " + ex.Message);
        flag = false;
      }
    }
    else
      flag = false;
    return flag;
  }

  public static void On(string action, Action<JObject> callback)
  {
    ModMessageHandler._listeners[action.ToLower()] = callback;
  }

  public static void CheckMessages()
  {
    if (!ModMessageHandler.IsConnected())
      return;
    try
    {
      string[] messages = WebSocketBridge.GetMessages();
      if ((messages == null ? 1 : (messages.Length == 0 ? 1 : 0)) != 0)
        return;
      for (int index = 0; index < messages.Length; ++index)
      {
        string json = messages[index];
        switch (json)
        {
          case null:
            Console.WriteLine($"[ModMessageHandler] Bridge delivered NULL entry at index {index} (batch={messages.Length}), frame dropped");
            break;
          case "":
            Console.WriteLine($"[ModMessageHandler] Bridge delivered EMPTY entry at index {index} (batch={messages.Length}), frame dropped");
            break;
          default:
            if ((!json.Contains("SOUNDBOARD_MANIFEST") ? 0 : (ModMessageHandler.TryHandleSoundboardManifest(json) ? 1 : 0)) == 0)
            {
              if (((UnityEngine.Object) FriendNetworkController.Instance != (UnityEngine.Object) null))
                FriendNetworkController.Instance.OnMessageReceived(json);
              ModMessageHandler.ProcessMessage(json);
              break;
            }
            break;
        }
      }
    }
    catch (Exception ex)
    {
      Console.WriteLine("[ModMessageHandler] Error checking messages: " + ex.Message);
    }
  }

  private static bool TryHandleSoundboardManifest(string json)
  {
    try
    {
      JObject jobject = JObject.Parse(json);
      if (jobject["op"]?.ToString() != "SOUNDBOARD_MANIFEST")
        return false;
      SoundboardManager.HandleManifest(jobject["d"]);
      return true;
    }
    catch (Exception ex)
    {
      Console.WriteLine("[ModMessageHandler] Soundboard manifest parse failed: " + ex.Message);
      return true;
    }
  }

  private static void ProcessMessage(string json)
  {
    try
    {
      JObject jobject = JObject.Parse(json);
      if (jobject["op"] != null || jobject["action"] == null)
        return;
      string lower = Extensions.Value<string>((IEnumerable<JToken>) jobject["action"])?.ToLower();
      if (!ModMessageHandler._listeners.ContainsKey(lower))
        return;
      ModMessageHandler._listeners[lower](jobject["data"] as JObject);
    }
    catch
    {
    }
  }
}
