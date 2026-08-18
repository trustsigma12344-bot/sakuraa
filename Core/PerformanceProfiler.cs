using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

#nullable disable
namespace SakuraaCastingMod.Core;

public class PerformanceProfiler : MonoBehaviour
{
  public static PerformanceProfiler Instance;
  private const int HistorySize = 300;
  public PerformanceProfiler.ProfilerMode CurrentMode = PerformanceProfiler.ProfilerMode.FlatTotal;
  public PerformanceProfiler.ProfilerSettings Settings = new PerformanceProfiler.ProfilerSettings();
  private static readonly Dictionary<string, PerformanceProfiler.Node> GlobalRegistry = new Dictionary<string, PerformanceProfiler.Node>();
  private static readonly Dictionary<string, string> FrameEdges = new Dictionary<string, string>();
  private readonly List<PerformanceProfiler.Node> _renderRoots = new List<PerformanceProfiler.Node>();
  private List<PerformanceProfiler.Node> _renderFlat = new List<PerformanceProfiler.Node>();
  private static readonly ThreadLocal<Stack<PerformanceProfiler.FrameCall>> CallStack = new ThreadLocal<Stack<PerformanceProfiler.FrameCall>>((Func<Stack<PerformanceProfiler.FrameCall>>) (() => new Stack<PerformanceProfiler.FrameCall>()));
  private List<PerformanceProfiler.Node> _spikeSnapshot = new List<PerformanceProfiler.Node>();
  private double _spikeMaxTime = 0.0;
  private string _spikeCause = "";
  private readonly List<float> _frameHistory = new List<float>((IEnumerable<float>) new float[300]);
  private Harmony _harmony;
  private bool _isVisible = false;
  private Rect _windowRect = new Rect(20f, 20f, 900f, 800f);
  private Vector2 _scrollPos;
  private Texture2D _whiteTex;
  private double _currentFrameTotal;

  private void Awake()
  {
    PerformanceProfiler.Instance = this;
    this._whiteTex = new Texture2D(1, 1);
    this._whiteTex.SetPixel(0, 0, Color.white);
    this._whiteTex.Apply();
    this._harmony = new Harmony("com.sakuraa.profiler_ultimate_v3");
    this.PatchMethods();
  }

  private void OnDestroy() => this._harmony?.UnpatchSelf();

  private void PatchMethods()
  {
    IEnumerable<MethodInfo> methodInfos = ((IEnumerable<Type>) typeof (Plugin).Assembly.GetTypes()).Where<Type>((Func<Type, bool>) (t => t.Namespace != null && t.Namespace.StartsWith("SakuraaCastingMod") && !t.Name.Contains("DisplayClass") && t != typeof (PerformanceProfiler))).SelectMany<Type, MethodInfo>((Func<Type, IEnumerable<MethodInfo>>) (t => (IEnumerable<MethodInfo>) t.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static))).Where<MethodInfo>((Func<MethodInfo, bool>) (m => !m.IsSpecialName && !m.IsGenericMethod));
    MethodInfo methodInfo1 = AccessTools.Method(typeof (PerformanceProfiler), "Prefix", (Type[]) null, (Type[]) null);
    MethodInfo methodInfo2 = AccessTools.Method(typeof (PerformanceProfiler), "Postfix", (Type[]) null, (Type[]) null);
    foreach (MethodInfo methodInfo3 in methodInfos)
    {
      try
      {
        this._harmony.Patch((MethodBase) methodInfo3, new HarmonyMethod(methodInfo1), new HarmonyMethod(methodInfo2), (HarmonyMethod) null, (HarmonyMethod) null, (HarmonyMethod) null);
      }
      catch
      {
      }
    }
  }

  private static void Prefix(out PerformanceProfiler.FrameCall __state, MethodBase __originalMethod)
  {
    string key1 = $"{__originalMethod.DeclaringType?.Name}.{__originalMethod.Name}";
    __state = new PerformanceProfiler.FrameCall()
    {
      Id = key1,
      StartTick = Stopwatch.GetTimestamp(),
      ChildrenDuration = 0.0
    };
    lock (PerformanceProfiler.GlobalRegistry)
    {
      if (!PerformanceProfiler.GlobalRegistry.ContainsKey(key1))
      {
        Dictionary<string, PerformanceProfiler.Node> globalRegistry = PerformanceProfiler.GlobalRegistry;
        string key2 = key1;
        PerformanceProfiler.Node node = new PerformanceProfiler.Node();
        node.Name = __originalMethod.Name;
        Type declaringType = __originalMethod.DeclaringType;
        string str;
        if ((object) declaringType == null)
        {
          str = (string) null;
        }
        else
        {
          str = declaringType.Name;
          if (str != null)
            goto label_6;
        }
        str = "Unknown";
label_6:
        node.ClassName = str;
        node.Id = key1;
        globalRegistry[key2] = node;
      }
    }
    PerformanceProfiler.CallStack.Value.Push(__state);
  }

  private static void Postfix(PerformanceProfiler.FrameCall __state)
  {
    double num1 = (double) (Stopwatch.GetTimestamp() - __state.StartTick) * 1000.0 / (double) Stopwatch.Frequency;
    Stack<PerformanceProfiler.FrameCall> frameCallStack = PerformanceProfiler.CallStack.Value;
    if (frameCallStack.Count > 0)
      frameCallStack.Pop();
    string str = (string) null;
    if (frameCallStack.Count > 0)
    {
      PerformanceProfiler.FrameCall frameCall = frameCallStack.Pop();
      frameCall.ChildrenDuration += num1;
      str = frameCall.Id;
      frameCallStack.Push(frameCall);
    }
    double num2 = Math.Max(0.0, num1 - __state.ChildrenDuration);
    lock (PerformanceProfiler.GlobalRegistry)
    {
      PerformanceProfiler.Node node;
      if (PerformanceProfiler.GlobalRegistry.TryGetValue(__state.Id, out node))
      {
        node.FrameTotal += num1;
        node.FrameSelf += num2;
        ++node.FrameCalls;
        ++node.Calls;
      }
      if (str == null)
        PerformanceProfiler.FrameEdges.Remove(__state.Id);
      else
        PerformanceProfiler.FrameEdges[__state.Id] = str;
    }
  }

  private void Update()
  {
    if (((ButtonControl) Keyboard.current.f5Key).wasPressedThisFrame)
      this._isVisible = !this._isVisible;
    if (!this._isVisible || this.Settings.Paused)
      return;
    lock (PerformanceProfiler.GlobalRegistry)
    {
      this.RebuildVisualTree();
      this._currentFrameTotal = 0.0;
      foreach (PerformanceProfiler.Node renderRoot in this._renderRoots)
        this._currentFrameTotal += renderRoot.FrameTotal;
      this._frameHistory.Add((float) this._currentFrameTotal);
      if (this._frameHistory.Count > 300)
        this._frameHistory.RemoveAt(0);
      if (this._currentFrameTotal > this._spikeMaxTime)
      {
        this._spikeMaxTime = this._currentFrameTotal;
        this.CaptureSpikeSnapshot();
      }
      float num = Mathf.Clamp01(1f - this.Settings.Smoothing);
      foreach (PerformanceProfiler.Node node in PerformanceProfiler.GlobalRegistry.Values)
      {
        node.AvgTotalTime = (double) Mathf.Lerp((float) node.AvgTotalTime, (float) node.FrameTotal, num);
        node.AvgSelfTime = (double) Mathf.Lerp((float) node.AvgSelfTime, (float) node.FrameSelf, num);
        node.FrameTotal = 0.0;
        node.FrameSelf = 0.0;
        node.FrameCalls = 0;
      }
      PerformanceProfiler.FrameEdges.Clear();
      this._renderFlat = PerformanceProfiler.GlobalRegistry.Values.Where<PerformanceProfiler.Node>((Func<PerformanceProfiler.Node, bool>) (n => n.AvgTotalTime > this.Settings.MinDisplayMs || n.AvgSelfTime > this.Settings.MinDisplayMs)).ToList<PerformanceProfiler.Node>();
    }
  }

  private void RebuildVisualTree()
  {
    this._renderRoots.Clear();
    foreach (PerformanceProfiler.Node node in PerformanceProfiler.GlobalRegistry.Values)
      node.Children.Clear();
    foreach (PerformanceProfiler.Node node1 in PerformanceProfiler.GlobalRegistry.Values)
    {
      if (node1.FrameCalls != 0)
      {
        string key;
        PerformanceProfiler.Node node2 = default;
        if ((!PerformanceProfiler.FrameEdges.TryGetValue(node1.Id, out key) ? 0 : (PerformanceProfiler.GlobalRegistry.TryGetValue(key, out node2) ? 1 : 0)) != 0)
          node2.Children.Add(node1);
        else
          this._renderRoots.Add(node1);
      }
    }
  }

  private void CaptureSpikeSnapshot()
  {
    this._spikeSnapshot.Clear();
    this._spikeCause = "Analyzing...";
    foreach (PerformanceProfiler.Node renderRoot in this._renderRoots)
    {
      if (renderRoot.FrameTotal > this.Settings.MinDisplayMs)
        this._spikeSnapshot.Add(Clone(renderRoot));
    }
    PerformanceProfiler.Node node1 = (PerformanceProfiler.Node) null;
    foreach (PerformanceProfiler.Node node2 in PerformanceProfiler.GlobalRegistry.Values)
    {
      if ((node2.FrameSelf <= 0.0 ? 0 : (node1 == null ? 1 : (node2.FrameSelf > node1.FrameSelf ? 1 : 0))) != 0)
        node1 = node2;
    }
    if (node1 == null)
      return;
    this._spikeCause = $"Heaviest: {node1.ClassName}.{node1.Name} ({node1.FrameSelf:F2}ms self)";

    static PerformanceProfiler.Node Clone(PerformanceProfiler.Node original)
    {
      PerformanceProfiler.Node node = new PerformanceProfiler.Node()
      {
        Name = original.Name,
        ClassName = original.ClassName,
        Id = original.Id,
        AvgTotalTime = original.FrameTotal,
        AvgSelfTime = original.FrameSelf,
        Expanded = false
      };
      foreach (PerformanceProfiler.Node child in original.Children)
        node.Children.Add(Clone(child));
      return node;
    }
  }

  private void OnGUI()
  {
    if (!this._isVisible)
      return;
    GUI.skin.label.richText = true;
    GUI.skin.window.normal.background = this._whiteTex;
    GUI.backgroundColor = new Color(0.1f, 0.1f, 0.12f, 0.98f);
    GUI.contentColor = Color.white;
    // ISSUE: method pointer
    this._windowRect = GUILayout.Window(9005, this._windowRect, new GUI.WindowFunction(this.DrawWindow), "Sakuraa Profiler Ultimate", Array.Empty<GUILayoutOption>());
  }

  private void DrawWindow(int id)
  {
    GUILayout.BeginVertical(Array.Empty<GUILayoutOption>());
    GUILayout.BeginHorizontal(GUI.skin.box, Array.Empty<GUILayoutOption>());
    GUILayout.Label($"<b>FPS: {(ValueType) (float) (1.0 / (double) Time.unscaledDeltaTime):F0}</b>", new GUILayoutOption[1]
    {
      GUILayout.Width(70f)
    });
    GUILayout.Label($"<b>Load: {this._currentFrameTotal:F2}ms</b>", new GUILayoutOption[1]
    {
      GUILayout.Width(100f)
    });
    GUILayout.Label($"<b>Spike: {this._spikeMaxTime:F1}ms</b>", new GUILayoutOption[1]
    {
      GUILayout.Width(100f)
    });
    GUI.backgroundColor = this.Settings.Paused ? Color.red : Color.green;
    if (GUILayout.Button(this.Settings.Paused ? "RESUME" : "PAUSE", Array.Empty<GUILayoutOption>()))
      this.Settings.Paused = !this.Settings.Paused;
    GUI.backgroundColor = new Color(0.1f, 0.1f, 0.12f, 0.98f);
    if (GUILayout.Button("RESET", Array.Empty<GUILayoutOption>()))
      this.ResetData();
    GUILayout.EndHorizontal();
    this.DrawSettingsPanel();
    this.DrawHistoryGraph();
    GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
    this.DrawTab("Flat (Total)", PerformanceProfiler.ProfilerMode.FlatTotal);
    this.DrawTab("Flat (Self)", PerformanceProfiler.ProfilerMode.FlatSelf);
    this.DrawTab("Hierarchy", PerformanceProfiler.ProfilerMode.Hierarchy);
    this.DrawTab("Flame Graph", PerformanceProfiler.ProfilerMode.FlameGraph);
    this.DrawTab("Spike Analysis", PerformanceProfiler.ProfilerMode.SpikeIsolator);
    GUILayout.EndHorizontal();
    GUILayout.BeginVertical(GUI.skin.box, Array.Empty<GUILayoutOption>());
    this._scrollPos = GUILayout.BeginScrollView(this._scrollPos, Array.Empty<GUILayoutOption>());
    switch (this.CurrentMode)
    {
      case PerformanceProfiler.ProfilerMode.FlatTotal:
        this.DrawFlat(true);
        break;
      case PerformanceProfiler.ProfilerMode.FlatSelf:
        this.DrawFlat(false);
        break;
      case PerformanceProfiler.ProfilerMode.Hierarchy:
        this.DrawHierarchy(this._renderRoots, false);
        break;
      case PerformanceProfiler.ProfilerMode.FlameGraph:
        this.DrawStableFlameGraph();
        break;
      case PerformanceProfiler.ProfilerMode.SpikeIsolator:
        GUILayout.Label($"<color=orange><b>{this._spikeCause}</b></color>", Array.Empty<GUILayoutOption>());
        this.DrawHierarchy(this._spikeSnapshot, true);
        break;
    }
    GUILayout.EndScrollView();
    GUILayout.EndVertical();
    GUILayout.EndVertical();
    GUI.DragWindow();
  }

  private void DrawSettingsPanel()
  {
    GUILayout.BeginVertical(GUI.skin.box, Array.Empty<GUILayoutOption>());
    GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
    GUILayout.Label("Smoothing:", new GUILayoutOption[1]
    {
      GUILayout.Width(80f)
    });
    this.Settings.Smoothing = GUILayout.HorizontalSlider(this.Settings.Smoothing, 0.0f, 0.99f, new GUILayoutOption[1]
    {
      GUILayout.Width(100f)
    });
    GUILayout.Space(10f);
    GUILayout.Label($"Min: {this.Settings.MinDisplayMs:F3}ms", new GUILayoutOption[1]
    {
      GUILayout.Width(90f)
    });
    this.Settings.MinDisplayMs = (double) GUILayout.HorizontalSlider((float) this.Settings.MinDisplayMs, 0.0f, 1f, new GUILayoutOption[1]
    {
      GUILayout.Width(100f)
    });
    GUILayout.Space(10f);
    this.Settings.ShowClassNames = GUILayout.Toggle(this.Settings.ShowClassNames, "Class Names", Array.Empty<GUILayoutOption>());
    this.Settings.ColorIntensity = GUILayout.Toggle(this.Settings.ColorIntensity, "Heat Map", Array.Empty<GUILayoutOption>());
    GUILayout.EndHorizontal();
    GUILayout.EndVertical();
  }

  private void DrawTab(string text, PerformanceProfiler.ProfilerMode mode)
  {
    GUI.backgroundColor = this.CurrentMode == mode ? new Color(0.3f, 0.6f, 1f) : Color.gray;
    if (GUILayout.Button(text, new GUILayoutOption[1]
    {
      GUILayout.Height(25f)
    }))
      this.CurrentMode = mode;
    GUI.backgroundColor = new Color(0.1f, 0.1f, 0.12f, 0.98f);
  }

  private void DrawFlat(bool sortByTotal)
  {
    List<PerformanceProfiler.Node> renderFlat = this._renderFlat;
    if (sortByTotal)
      renderFlat.Sort((Comparison<PerformanceProfiler.Node>) ((a, b) => b.AvgTotalTime.CompareTo(a.AvgTotalTime)));
    else
      renderFlat.Sort((Comparison<PerformanceProfiler.Node>) ((a, b) => b.AvgSelfTime.CompareTo(a.AvgSelfTime)));
    double max = renderFlat.Count > 0 ? (sortByTotal ? renderFlat[0].AvgTotalTime : renderFlat[0].AvgSelfTime) : 1.0;
    if (max <= 0.0)
      max = 1.0;
    foreach (PerformanceProfiler.Node n in renderFlat)
    {
      double val = sortByTotal ? n.AvgTotalTime : n.AvgSelfTime;
      if (val >= this.Settings.MinDisplayMs)
        this.DrawRow(this.GetDisplayName(n), val, max, n.Calls, 0);
    }
  }

  private void DrawHierarchy(List<PerformanceProfiler.Node> nodes, bool isSnapshot, int depth = 0)
  {
    if ((nodes == null ? 1 : (nodes.Count == 0 ? 1 : 0)) != 0)
      return;
    List<PerformanceProfiler.Node> nodeList = new List<PerformanceProfiler.Node>((IEnumerable<PerformanceProfiler.Node>) nodes);
    nodeList.Sort((Comparison<PerformanceProfiler.Node>) ((a, b) => b.AvgTotalTime.CompareTo(a.AvgTotalTime)));
    double max = isSnapshot ? (this._spikeMaxTime > 0.0 ? this._spikeMaxTime : 1.0) : (this._currentFrameTotal > 0.0 ? this._currentFrameTotal : 1.0);
    foreach (PerformanceProfiler.Node n in nodeList)
    {
      double avgTotalTime = n.AvgTotalTime;
      if (avgTotalTime >= this.Settings.MinDisplayMs)
      {
        GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
        GUILayout.Space((float) (depth * 15));
        if (GUILayout.Button((n.Children.Count > 0 ? (n.Expanded ? "▼ " : "▶ ") : "• ") + this.GetDisplayName(n), GUI.skin.label, new GUILayoutOption[1]
        {
          GUILayout.Width((float) (300 - depth * 15))
        }))
          n.Expanded = !n.Expanded;
        this.DrawBar(GUILayoutUtility.GetRect(200f, 16f), avgTotalTime, max, this.GetHeatColor(avgTotalTime));
        GUILayout.Label($"{avgTotalTime:F2}ms", new GUILayoutOption[1]
        {
          GUILayout.Width(60f)
        });
        double avgSelfTime = n.AvgSelfTime;
        if (avgSelfTime > this.Settings.MinDisplayMs)
          GUILayout.Label($"<color=#888888>(self: {avgSelfTime:F2}ms)</color>", new GUILayoutOption[1]
          {
            GUILayout.Width(110f)
          });
        GUILayout.EndHorizontal();
        if ((!n.Expanded ? 0 : (n.Children.Count > 0 ? 1 : 0)) != 0)
          this.DrawHierarchy(n.Children, isSnapshot, depth + 1);
      }
    }
  }

  private void DrawStableFlameGraph()
  {
    Rect rect = GUILayoutUtility.GetRect((float) (Screen.width - 60), 400f);
    GUI.Box(rect, "");
    if ((this._currentFrameTotal <= 0.0 ? 1 : (this._renderRoots.Count == 0 ? 1 : 0)) != 0)
      return;
    List<PerformanceProfiler.Node> nodeList = new List<PerformanceProfiler.Node>((IEnumerable<PerformanceProfiler.Node>) this._renderRoots);
    nodeList.Sort((Comparison<PerformanceProfiler.Node>) ((a, b) => b.AvgTotalTime.CompareTo(a.AvgTotalTime)));
    float x = rect.x;
    float height = 20f;
    foreach (PerformanceProfiler.Node node in nodeList)
    {
      if (node.AvgTotalTime >= this.Settings.MinDisplayMs)
      {
        this.DrawFlameRecursive(node, x, rect.y, rect.width, height, this._currentFrameTotal);
        x += (float) (node.AvgTotalTime / this._currentFrameTotal) * rect.width;
      }
    }
  }

  private void DrawFlameRecursive(
    PerformanceProfiler.Node node,
    float x,
    float y,
    float fullWidth,
    float height,
    double totalFrameTime)
  {
    if ((node.AvgTotalTime < this.Settings.MinDisplayMs ? 1 : (totalFrameTime <= 0.0 ? 1 : 0)) != 0)
      return;
    float num = (float) (node.AvgTotalTime / totalFrameTime) * fullWidth;
    if ((double) num >= 1.0)
    {
      Rect rect;
      // ISSUE: explicit constructor call
      rect = new Rect(x, y, num, height);
      GUI.color = this.GetHeatColor(node.AvgTotalTime);
      GUI.DrawTexture(rect, (Texture) this._whiteTex);
      GUI.color = new Color(0.0f, 0.0f, 0.0f, 0.5f);
      GUI.DrawTexture(new Rect(rect.x, rect.y, 1f, rect.height), (Texture) this._whiteTex);
      if ((double) num > 30.0)
      {
        GUI.color = Color.black;
        string str = node.Name;
        if ((double) num > 100.0)
          str = this.GetDisplayName(node);
        GUI.Label(rect, str);
      }
      if (node.Children.Count > 0)
      {
        List<PerformanceProfiler.Node> nodeList = new List<PerformanceProfiler.Node>((IEnumerable<PerformanceProfiler.Node>) node.Children);
        nodeList.Sort((Comparison<PerformanceProfiler.Node>) ((a, b) => b.AvgTotalTime.CompareTo(a.AvgTotalTime)));
        float x1 = x;
        foreach (PerformanceProfiler.Node node1 in nodeList)
        {
          this.DrawFlameRecursive(node1, x1, y + height, fullWidth, height, totalFrameTime);
          x1 += (float) (node1.AvgTotalTime / totalFrameTime) * fullWidth;
        }
      }
    }
    GUI.color = Color.white;
  }

  private string GetDisplayName(PerformanceProfiler.Node n)
  {
    return !this.Settings.ShowClassNames ? n.Name : $"{n.ClassName}.{n.Name}";
  }

  private void DrawRow(string name, double val, double max, long calls, int indent)
  {
    GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
    GUILayout.Space((float) indent);
    GUILayout.Label(name, new GUILayoutOption[1]
    {
      GUILayout.Width(300f)
    });
    this.DrawBar(GUILayoutUtility.GetRect(200f, 16f), val, max, this.GetHeatColor(val));
    GUILayout.Label($"{val:F2} ms", new GUILayoutOption[1]
    {
      GUILayout.Width(70f)
    });
    GUILayout.Label($"x{calls}", new GUILayoutOption[1]
    {
      GUILayout.Width(50f)
    });
    GUILayout.EndHorizontal();
  }

  private void DrawBar(Rect r, double val, double max, Color c)
  {
    if (max <= 0.0)
      max = 1.0;
    float num = (float) (val / max) * r.width;
    GUI.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);
    GUI.DrawTexture(r, (Texture) this._whiteTex);
    GUI.color = c;
    GUI.DrawTexture(new Rect(r.x, r.y, num, r.height), (Texture) this._whiteTex);
    GUI.color = Color.white;
  }

  private void DrawHistoryGraph()
  {
    Rect rect = GUILayoutUtility.GetRect(0.0f, 40f, new GUILayoutOption[1]
    {
      GUILayout.ExpandWidth(true)
    });
    GUI.color = new Color(0.0f, 0.0f, 0.0f, 0.3f);
    GUI.DrawTexture(rect, (Texture) this._whiteTex);
    if (this._frameHistory.Count < 2)
      return;
    float num1 = this._frameHistory.Max();
    if ((double) num1 < 1.0)
      num1 = 1f;
    float num2 = rect.width / (float) this._frameHistory.Count;
    for (int index = 0; index < this._frameHistory.Count; ++index)
    {
      float num3 = this._frameHistory[index] / num1 * rect.height;
      GUI.color = this.GetHeatColor((double) this._frameHistory[index]);
      GUI.DrawTexture(new Rect(rect.x + (float) index * num2, rect.y + rect.height - num3, num2, num3), (Texture) this._whiteTex);
    }
    GUI.color = Color.white;
  }

  private Color GetHeatColor(double ms)
  {
    return !this.Settings.ColorIntensity ? new Color(0.4f, 0.6f, 1f) : (ms >= 0.2 ? (ms < 1.0 ? Color.green : (ms < 4.0 ? Color.yellow : (ms < 8.0 ? new Color(1f, 0.5f, 0.0f) : (ms >= 16.0 ? Color.magenta : Color.red)))) : Color.cyan);
  }

  private void ResetData()
  {
    lock (PerformanceProfiler.GlobalRegistry)
    {
      PerformanceProfiler.GlobalRegistry.Clear();
      PerformanceProfiler.FrameEdges.Clear();
      this._renderRoots.Clear();
      this._renderFlat.Clear();
      this._frameHistory.Clear();
      this._spikeSnapshot.Clear();
      this._spikeMaxTime = 0.0;
      this._currentFrameTotal = 0.0;
    }
  }

  public enum ProfilerMode
  {
    FlatTotal,
    FlatSelf,
    Hierarchy,
    FlameGraph,
    SpikeIsolator,
  }

  [Serializable]
  public class ProfilerSettings
  {
    public float Smoothing = 0.5f;
    public double MinDisplayMs = 0.01;
    public bool Paused = false;
    public bool ColorIntensity = true;
    public bool ShowClassNames = true;
  }

  public class Node
  {
    public string Name;
    public string ClassName;
    public string Id;
    public double AvgTotalTime;
    public double AvgSelfTime;
    public long Calls;
    public double FrameTotal;
    public double FrameSelf;
    public int FrameCalls;
    public List<PerformanceProfiler.Node> Children = new List<PerformanceProfiler.Node>();
    public bool Expanded = false;
  }

  private struct FrameCall
  {
    public string Id;
    public long StartTick;
    public double ChildrenDuration;
  }
}
