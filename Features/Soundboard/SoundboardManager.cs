using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Features.Soundboard;

public static class SoundboardManager
{
  public const int MaxSounds = 200;
  public const int MaxPages = 5;
  public const int TilesPerView = 10;
  public static readonly List<SoundboardManager.Page> Pages = new List<SoundboardManager.Page>();
  public static readonly List<SoundboardManager.Group> Groups = new List<SoundboardManager.Group>();
  public static readonly List<SoundboardManager.TileView> Views = new List<SoundboardManager.TileView>();
  private static readonly Dictionary<string, SoundboardManager.Sound> _byId = new Dictionary<string, SoundboardManager.Sound>();
  private static readonly List<SoundboardManager.Sound> _allSounds = new List<SoundboardManager.Sound>();

  public static event Action OnManifestChanged;

  public static bool HasManifest { get; private set; }

  public static int SoundCount { get; private set; }

  public static bool HasContent
  {
    get => SoundboardManager.SoundCount > 0 && SoundboardManager.Views.Count > 0;
  }

  public static IReadOnlyList<SoundboardManager.Sound> AllSounds
  {
    get => (IReadOnlyList<SoundboardManager.Sound>) SoundboardManager._allSounds;
  }

  public static SoundboardManager.Sound FindSound(string id)
  {
    SoundboardManager.Sound sound = default;
    return !string.IsNullOrEmpty(id) && SoundboardManager._byId.TryGetValue(id, out sound) ? sound : (SoundboardManager.Sound) null;
  }

  public static void HandleManifest(JToken d)
  {
    try
    {
      SoundboardManager.Pages.Clear();
      SoundboardManager.Groups.Clear();
      SoundboardManager.Views.Clear();
      SoundboardManager._byId.Clear();
      SoundboardManager._allSounds.Clear();
      SoundboardManager.SoundCount = 0;
      SoundboardManager.HasManifest = true;
      string path1 = d?[(object) "folder"]?.ToString();
      Dictionary<string, SoundboardManager.Sound> dictionary1 = new Dictionary<string, SoundboardManager.Sound>();
      if (d?[(object) "sounds"] is JArray jarray1)
      {
        foreach (JToken jtoken1 in jarray1)
        {
          string path2 = jtoken1?[(object) "file"]?.ToString();
          if ((string.IsNullOrEmpty(path2) ? 1 : (string.IsNullOrEmpty(path1) ? 1 : 0)) == 0)
          {
            string key = path2.EndsWith(".wav", StringComparison.OrdinalIgnoreCase) ? path2.Substring(0, path2.Length - 4) : path2;
            SoundboardManager.Sound sound1 = new SoundboardManager.Sound();
            sound1.Id = key;
            string str;
            if (jtoken1 == null)
            {
              str = (string) null;
            }
            else
            {
              JToken jtoken2 = jtoken1[(object) "name"];
              if (jtoken2 == null)
              {
                str = (string) null;
              }
              else
              {
                str = jtoken2.ToString();
                if (str != null)
                  goto label_11;
              }
            }
            str = "Sound";
label_11:
            sound1.Name = str;
            sound1.DurationSec = (jtoken1?[(object) "durationMs"]?.ToObject<float?>().GetValueOrDefault() / 1000f) ?? 0f;
            sound1.FilePath = Path.Combine(path1, path2);
            SoundboardManager.Sound sound2 = sound1;
            dictionary1[key] = sound2;
          }
        }
      }
      SoundboardManager.SoundCount = dictionary1.Count;
      foreach (SoundboardManager.Sound sound in dictionary1.Values)
      {
        SoundboardManager._byId[sound.Id] = sound;
        SoundboardManager._allSounds.Add(sound);
      }
      List<(int, SoundboardManager.Page)> valueTupleList = new List<(int, SoundboardManager.Page)>();
      if (d?[(object) "pages"] is JArray jarray3)
      {
        foreach (JToken jtoken3 in jarray3)
        {
          if (valueTupleList.Count < 5)
          {
            SoundboardManager.Page page1 = new SoundboardManager.Page();
            string str;
            if (jtoken3 == null)
            {
              str = (string) null;
            }
            else
            {
              JToken jtoken4 = jtoken3[(object) "name"];
              if (jtoken4 == null)
              {
                str = (string) null;
              }
              else
              {
                str = jtoken4.ToString();
                if (str != null)
                  goto label_31;
              }
            }
            str = "PAGE";
label_31:
            page1.Name = str;
            SoundboardManager.Page page2 = page1;
            if (jtoken3?[(object) "soundIds"] is JArray jarray2)
            {
              foreach (JToken jtoken5 in jarray2)
              {
                Dictionary<string, SoundboardManager.Sound> dictionary2 = dictionary1;
                string key;
                if (jtoken5 == null)
                {
                  key = (string) null;
                }
                else
                {
                  key = jtoken5.ToString();
                  if (key != null)
                    goto label_38;
                }
                key = "";
label_38:
                if (dictionary2.TryGetValue(key, out SoundboardManager.Sound sound))
                  page2.Sounds.Add(sound);
              }
            }
            valueTupleList.Add((jtoken3?[(object) "order"]?.ToObject<int?>().GetValueOrDefault() ?? 0, page2));
          }
          else
            break;
        }
      }
      valueTupleList.Sort((Comparison<(int, SoundboardManager.Page)>) ((a, b) => a.Item1.CompareTo(b.Item1)));
      foreach ((int _, SoundboardManager.Page page) in valueTupleList)
        SoundboardManager.Pages.Add(page);
      SoundboardManager.BuildViews();
      int count1 = d?[(object) "sounds"] is JArray jarray4 ? ((JContainer) jarray4).Count : 0;
      int count2 = d?[(object) "pages"] is JArray jarray5 ? ((JContainer) jarray5).Count : 0;
      UnityEngine.Debug.Log((object) $"{$"[Soundboard] Manifest applied: {SoundboardManager.SoundCount}/{count1} sounds, "}{$"{SoundboardManager.Pages.Count}/{count2} pages, {SoundboardManager.Groups.Count} groups, {SoundboardManager.Views.Count} views. "}Pages: [{SoundboardManager.PageSummary()}]");
    }
    catch (Exception ex)
    {
      UnityEngine.Debug.LogError((object) ("[Soundboard] Failed to apply manifest: " + ex.Message));
    }
    Action onManifestChanged = SoundboardManager.OnManifestChanged;
    if (onManifestChanged == null)
      return;
    onManifestChanged();
  }

  private static string PageSummary()
  {
    List<string> values = new List<string>(SoundboardManager.Pages.Count);
    foreach (SoundboardManager.Page page in SoundboardManager.Pages)
      values.Add($"{page.Name}({page.Sounds.Count})");
    return string.Join(", ", (IEnumerable<string>) values);
  }

  private static void BuildViews()
  {
    if (SoundboardManager._allSounds.Count > 0)
      SoundboardManager.AddGroup("ALL SOUNDS", SoundboardManager._allSounds);
    foreach (SoundboardManager.Page page in SoundboardManager.Pages)
      SoundboardManager.AddGroup(page.Name, page.Sounds);
  }

  private static void AddGroup(string name, List<SoundboardManager.Sound> sounds)
  {
    SoundboardManager.Group group = new SoundboardManager.Group()
    {
      Name = string.IsNullOrEmpty(name) ? "PAGE" : name,
      SoundTotal = sounds.Count,
      FirstView = SoundboardManager.Views.Count
    };
    int num1 = Mathf.Max(1, Mathf.CeilToInt((float) sounds.Count / 10f));
    for (int index1 = 0; index1 < num1; ++index1)
    {
      SoundboardManager.TileView tileView = new SoundboardManager.TileView()
      {
        Label = num1 > 1 ? $"{group.Name.ToUpper()} {index1 + 1}/{num1}" : group.Name.ToUpper()
      };
      int num2 = index1 * 10;
      int num3 = Mathf.Min(10, sounds.Count - num2);
      for (int index2 = 0; index2 < num3; ++index2)
        tileView.Tiles.Add(sounds[num2 + index2]);
      SoundboardManager.Views.Add(tileView);
    }
    group.ViewCount = num1;
    SoundboardManager.Groups.Add(group);
  }

  public static int FindViewOf(string id)
  {
    int viewOf;
    if (!string.IsNullOrEmpty(id))
    {
      for (int index = 0; index < SoundboardManager.Views.Count; ++index)
      {
        foreach (SoundboardManager.Sound tile in SoundboardManager.Views[index].Tiles)
        {
          if (tile.Id == id)
          {
            viewOf = index;
            goto label_12;
          }
        }
      }
      viewOf = 0;
    }
    else
      viewOf = 0;
label_12:
    return viewOf;
  }

  public class Sound
  {
    public string Id;
    public string Name;
    public float DurationSec;
    public string FilePath;
  }

  public class Page
  {
    public string Name;
    public readonly List<SoundboardManager.Sound> Sounds = new List<SoundboardManager.Sound>();
  }

  public class TileView
  {
    public string Label;
    public readonly List<SoundboardManager.Sound> Tiles = new List<SoundboardManager.Sound>(10);
  }

  public class Group
  {
    public string Name;
    public int SoundTotal;
    public int FirstView;
    public int ViewCount;
  }
}
