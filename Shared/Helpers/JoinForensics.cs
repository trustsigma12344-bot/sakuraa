using Photon.Pun;
using System;
using System.IO;
using System.Text;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Shared.Helpers;

public static class JoinForensics
{
  private const int EventCapacity = 128 /*0x80*/;
  private const int FrameCapacity = 512 /*0x0200*/;
  private const float FrameWindowSec = 5f;
  private const float HitchThresholdMs = 250f;
  private const float WatchdogSec = 15f;
  private const float IncidentCooldownSec = 30f;
  private static readonly JoinForensics.Event[] _events = new JoinForensics.Event[128 /*0x80*/];
  private static int _evHead;
  private static int _evCount;
  private static readonly JoinForensics.FrameSample[] _frames = new JoinForensics.FrameSample[512 /*0x0200*/];
  private static int _frHead;
  private static int _frCount;
  private static string _lastNetState = "";
  private static float _lastHitchRecordT = -999f;
  private static bool _joinInFlight;
  private static float _joinDeadline;
  private static float _lastJoinAt = -999f;
  private static string _lastJoinCode = "";
  private static bool _lastJoinRaw;
  private static string _lastJoinVia = "";
  private static string _nonceBaseline = "";
  public static Func<string> NonceProvider;
  private static bool _incidentWritten;
  private static float _incidentWrittenAt = -999f;
  public static bool LoggingEnabled;

  private static float Now => Time.realtimeSinceStartup;

  public static void Record(string evt)
  {
    try
    {
      JoinForensics._events[JoinForensics._evHead] = new JoinForensics.Event(JoinForensics.Now, evt ?? "");
      JoinForensics._evHead = (JoinForensics._evHead + 1) % 128 /*0x80*/;
      if (JoinForensics._evCount >= 128 /*0x80*/)
        return;
      ++JoinForensics._evCount;
    }
    catch
    {
    }
  }

  public static void Tick(float frameMs)
  {
    try
    {
      float now = JoinForensics.Now;
      JoinForensics._frames[JoinForensics._frHead] = new JoinForensics.FrameSample(now, frameMs);
      JoinForensics._frHead = (JoinForensics._frHead + 1) % 512 /*0x0200*/;
      if (JoinForensics._frCount < 512 /*0x0200*/)
        ++JoinForensics._frCount;
      if (((double) frameMs <= 250.0 ? 0 : ((double) now - (double) JoinForensics._lastHitchRecordT > 1.0 ? 1 : 0)) != 0)
      {
        JoinForensics._lastHitchRecordT = now;
        JoinForensics.Record($"FRAME HITCH {frameMs:F0}ms");
      }
      string str = ((UnityEngine.Object) NetworkSystem.Instance != (UnityEngine.Object) null) ? NetworkSystem.Instance.netState.ToString() : "<no NetworkSystem>";
      if (str != JoinForensics._lastNetState)
      {
        JoinForensics.Record($"netState {(JoinForensics._lastNetState.Length == 0 ? "<init>" : JoinForensics._lastNetState)} -> {str}");
        JoinForensics._lastNetState = str;
      }
      if ((!JoinForensics._joinInFlight ? 0 : ((double) now > (double) JoinForensics._joinDeadline ? 1 : 0)) == 0)
        return;
      if ((str == "InGame" ? 1 : (str == "Idle" ? 1 : 0)) != 0)
      {
        JoinForensics._joinInFlight = false;
      }
      else
      {
        JoinForensics._joinInFlight = false;
        JoinForensics.FlushIncident("join-watchdog-hang", (string) null);
      }
    }
    catch
    {
    }
  }

  public static void NoteModJoin(string code, bool raw, string via)
  {
    JoinForensics._joinInFlight = true;
    JoinForensics._joinDeadline = JoinForensics.Now + 15f;
    JoinForensics._lastJoinAt = JoinForensics.Now;
    JoinForensics._lastJoinCode = code ?? "";
    JoinForensics._lastJoinRaw = raw;
    JoinForensics._lastJoinVia = via ?? "";
    JoinForensics.Record($"mod-join issued code={JoinForensics._lastJoinCode} raw={raw} via={via}");
  }

  public static void NoteLeaveBaseline(string nonce)
  {
    JoinForensics._nonceBaseline = nonce ?? "";
    JoinForensics.Record($"leave: baseline nonce len={JoinForensics._nonceBaseline.Length}");
  }

  public static void NoteJoinResolved(string how, bool success)
  {
    JoinForensics._joinInFlight = false;
    JoinForensics.Record($"join resolved: {how} success={success}");
    if (!success)
      return;
    JoinForensics._incidentWritten = false;
    JoinForensics._nonceBaseline = "";
  }

  public static void FlushIncident(string trigger, string disconnectCause)
  {
    try
    {
      if (!JoinForensics.LoggingEnabled)
        return;
      float now = JoinForensics.Now;
      if ((!JoinForensics._incidentWritten ? 0 : ((double) now - (double) JoinForensics._incidentWrittenAt < 30.0 ? 1 : 0)) != 0)
        return;
      JoinForensics._incidentWritten = true;
      JoinForensics._incidentWrittenAt = now;
      string cause = disconnectCause ?? "<none>";
      string str1 = JoinForensics.SafeNonce();
      bool nonceKnown;
      bool nonceChanged = (nonceKnown = JoinForensics._nonceBaseline.Length > 0 && str1 != null) && str1 != JoinForensics._nonceBaseline;
      float worstAgeSec;
      float worstMs = JoinForensics.WorstFrameMsInWindow(now, 5f, out worstAgeSec);
      bool recentMod = JoinForensics._joinInFlight || (double) now - (double) JoinForensics._lastJoinAt < 10.0;
      string str2 = JoinForensics.Verdict(cause, recentMod, nonceKnown, nonceChanged, worstMs);
      StringBuilder sb = new StringBuilder();
      sb.AppendLine($"JoinIncident @ {DateTime.Now:O}");
      sb.AppendLine("trigger:        " + trigger);
      sb.AppendLine("LIKELY CAUSE:   " + str2);
      sb.AppendLine("----------------------------------------------------------------");
      sb.AppendLine("disconnectCause: " + cause);
      sb.AppendLine("netState:        " + (((UnityEngine.Object) NetworkSystem.Instance != (UnityEngine.Object) null) ? NetworkSystem.Instance.netState.ToString() : "<no NetworkSystem>"));
      sb.AppendLine($"photonState:     {PhotonNetwork.NetworkClientState}");
      sb.AppendLine($"inRoom:          {PhotonNetwork.InRoom}");
      sb.AppendLine($"modJoinInFlight: {JoinForensics._joinInFlight}");
      sb.AppendLine($"lastModJoin:     code={JoinForensics._lastJoinCode} raw={JoinForensics._lastJoinRaw} via={JoinForensics._lastJoinVia} ({(ValueType) (float) ((double) now - (double) JoinForensics._lastJoinAt):F2}s ago)");
      sb.AppendLine("nonceBaseline:   " + (JoinForensics._nonceBaseline.Length == 0 ? "<none>" : JoinForensics._nonceBaseline.Length.ToString() + " chars"));
      sb.AppendLine("nonceNow:        " + (str1 == null ? "<null>" : str1.Length.ToString() + " chars"));
      sb.AppendLine("nonceChanged:    " + (nonceKnown ? nonceChanged.ToString() : "<unknown>"));
      sb.AppendLine($"worstFrame(5s):  {worstMs:F0}ms ({worstAgeSec:F2}s ago)");
      sb.AppendLine("----------------------------------------------------------------");
      sb.AppendLine($"timeline (last {JoinForensics._evCount} events, oldest first):");
      JoinForensics.AppendTimeline(sb, now);
      string contents = sb.ToString();
      UnityEngine.Debug.LogError((object) $"[JoinForensics] INCIDENT ({str2})\n{contents}");
      try
      {
        string str3 = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "BepInEx", "config", "SakuraaCameraClient");
        if (!Directory.Exists(str3))
          Directory.CreateDirectory(str3);
        File.WriteAllText(Path.Combine(str3, $"JoinIncident_{DateTime.Now:yyyyMMdd_HHmmss}.txt"), contents);
      }
      catch (Exception ex)
      {
        UnityEngine.Debug.LogError((object) ("[JoinForensics] file backup failed (player.log still has it): " + ex.Message));
      }
    }
    catch (Exception ex)
    {
      try
      {
        UnityEngine.Debug.LogError((object) ("[JoinForensics] failed to capture incident: " + ex.Message));
      }
      catch
      {
      }
    }
  }

  private static string SafeNonce()
  {
    try
    {
      return JoinForensics.NonceProvider != null ? JoinForensics.NonceProvider() : (string) null;
    }
    catch
    {
      return (string) null;
    }
  }

  private static string Verdict(
    string cause,
    bool recentMod,
    bool nonceKnown,
    bool nonceChanged,
    float worstMs)
  {
    bool flag1 = cause == "CustomAuthenticationFailed" || cause == "AuthenticationTicketExpired" || cause == "InvalidAuthentication";
    bool flag2 = cause == "ClientTimeout" || cause == "ServerTimeout";
    return (flag1 ? 1 : (!(recentMod & nonceKnown) ? 0 : (!nonceChanged ? 1 : 0))) == 0 ? ((flag2 ? 1 : ((double) worstMs > 500.0 ? 1 : 0)) == 0 ? (cause == "DisconnectByServerLogic" ? "SERVER-LOGIC kick - inspect timeline (not a clean A or B)" : "UNKNOWN - see timeline") : "STALL (B) - main-thread hitch starved Photon keepalives") : "AUTH (A) - stale/rejected Steam nonce on re-auth";
  }

  private static float WorstFrameMsInWindow(float now, float windowSec, out float worstAgeSec)
  {
    float num1 = 0.0f;
    float num2 = now;
    for (int index1 = 0; index1 < JoinForensics._frCount; ++index1)
    {
      int index2 = (JoinForensics._frHead - 1 - index1 + 512 /*0x0200*/) % 512 /*0x0200*/;
      JoinForensics.FrameSample frame = JoinForensics._frames[index2];
      if ((double) now - (double) frame.T <= (double) windowSec)
      {
        if ((double) frame.Ms > (double) num1)
        {
          num1 = frame.Ms;
          num2 = frame.T;
        }
      }
      else
        break;
    }
    worstAgeSec = now - num2;
    return num1;
  }

  private static void AppendTimeline(StringBuilder sb, float now)
  {
    for (int index1 = 0; index1 < JoinForensics._evCount; ++index1)
    {
      int index2 = (JoinForensics._evHead - JoinForensics._evCount + index1 + 128 /*0x80*/) % 128 /*0x80*/;
      JoinForensics.Event @event = JoinForensics._events[index2];
      sb.AppendLine($"  T-{(ValueType) (float) ((double) now - (double) @event.T):F2}s  {@event.Msg}");
    }
  }

  private readonly struct Event(float t, string msg)
  {
    public readonly float T = t;
    public readonly string Msg = msg;
  }

  private readonly struct FrameSample(float t, float ms)
  {
    public readonly float T = t;
    public readonly float Ms = ms;
  }
}
