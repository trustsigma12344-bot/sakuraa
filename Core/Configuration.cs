using BepInEx;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SakuraaCastingMod.Desktop.Ui;
using SakuraaCastingMod.Desktop.Ui.Framework;
using SakuraaCastingMod.Desktop.Ui.Framework.Menus;
using SakuraaCastingMod.Features.Overlays;
using SakuraaCastingMod.Features.Tools;
using SakuraaCastingMod.Features.Visuals;
using SakuraaCastingMod.Shared.Helpers;
using SakuraaCastingMod.VR.Tablet;
using SakuraaCastingMod.VR.UtilMenu;
using SakuraaCastingMod.VR.UtilMenu.Utility;
using SakuraaOfflinePresets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

#nullable disable
namespace SakuraaCastingMod.Core;

public static class Configuration
{
  public const bool AutoLoad = true;
  public const bool AutoSave = true;
  public static string ActivePresetId;
  public static string ActivePresetName;
  public static List<PresetMeta> AvailablePresets = new List<PresetMeta>();
  private static bool _serverSyncReady = false;
  private static bool _legacyMigrationPending = false;
  private static string _lastObservedHash = "";
  private static Dictionary<string, JToken> _lastSyncedKeys = new Dictionary<string, JToken>();
  private static Dictionary<string, Dictionary<string, JToken>> _presetStateCache = new Dictionary<string, Dictionary<string, JToken>>();
  private static readonly Dictionary<string, Configuration.PendingDeleteSnapshot> _pendingDeletes = new Dictionary<string, Configuration.PendingDeleteSnapshot>();
  private const double PendingDeleteTtlSeconds = 30.0;
  private static List<FieldInfo> _cachedSettingFields = new List<FieldInfo>();
  private static Dictionary<string, FieldInfo> _fieldsByKey = new Dictionary<string, FieldInfo>();
  private static readonly Dictionary<string, (float Min, float Max)> FloatSettingRanges = new Dictionary<string, (float, float)>()
  {
    {
      "ClippingPlaneNear",
      (0.01f, 1f)
    },
    {
      "FieldOfView",
      (1f, 179f)
    },
    {
      "ZoomFieldOfView",
      (1f, 179f)
    }
  };
  private const int DebounceMs = 1000;
  private static CancellationTokenSource _debounceCts;
  private static volatile bool _dirty;
  private const int PatchChunkBudgetChars = 1500;
  private const string LegacyImportPresetName = "Imported from file";

  public static int PresetListVersion { get; private set; }

  public static void BumpPresetListVersion() => ++Configuration.PresetListVersion;

  public static bool InitialLoadComplete { get; private set; }

  public static bool ServerSyncReady => Configuration._serverSyncReady;

  public static string LastObservedHash => Configuration._lastObservedHash;

  public static HashSet<string> ComputeDirtyKeys()
  {
    HashSet<string> dirtyKeys = new HashSet<string>();
    try
    {
      foreach (KeyValuePair<string, JToken> gatherCurrentKey in Configuration.GatherCurrentKeys())
      {
        JToken jtoken = default;
        if ((!Configuration._lastSyncedKeys.TryGetValue(gatherCurrentKey.Key, out jtoken) ? 1 : (!JToken.DeepEquals(gatherCurrentKey.Value, jtoken) ? 1 : 0)) != 0)
          dirtyKeys.Add(gatherCurrentKey.Key);
      }
    }
    catch (Exception ex)
    {
      UnityEngine.Debug.LogError((object) ("[Configuration] ComputeDirtyKeys failed: " + ex.Message));
    }
    return dirtyKeys;
  }

  private static bool WouldChangeAnyField(JObject workingJo)
  {
    bool flag;
    if (workingJo != null)
    {
      if ((Configuration._cachedSettingFields == null ? 1 : (Configuration._cachedSettingFields.Count == 0 ? 1 : 0)) != 0)
        Configuration.BuildFieldCache();
      try
      {
        JsonSerializer jsonSerializer = JsonSerializer.Create(new JsonSerializerSettings()
        {
          Converters = (IList<JsonConverter>) new List<JsonConverter>()
          {
            (JsonConverter) new Vector2Converter()
          },
          Formatting = (Formatting) 0,
          ReferenceLoopHandling = (ReferenceLoopHandling) 1
        });
        foreach (KeyValuePair<string, FieldInfo> keyValuePair in Configuration._fieldsByKey)
        {
          JToken jtoken;
          if (workingJo.TryGetValue(keyValuePair.Key, out jtoken))
          {
            object obj = keyValuePair.Value.GetValue((object) null);
            if (!JToken.DeepEquals(obj == null ? (JToken) (object) JValue.CreateNull() : JToken.FromObject(obj, jsonSerializer), jtoken))
            {
              flag = true;
              goto label_14;
            }
          }
        }
      }
      catch (Exception ex)
      {
        UnityEngine.Debug.LogError((object) ("[Configuration] WouldChangeAnyField failed: " + ex.Message));
        flag = true;
        goto label_14;
      }
      flag = false;
    }
    else
      flag = false;
label_14:
    return flag;
  }

  public static bool TryGetCachedPresetState(string id, out Dictionary<string, JToken> state)
  {
    return Configuration._presetStateCache.TryGetValue(id, out state);
  }

  public static void RegisterPresetCache(string id, Dictionary<string, JToken> state)
  {
    if ((string.IsNullOrEmpty(id) ? 1 : (state == null ? 1 : 0)) != 0)
      return;
    Configuration._presetStateCache[id] = state;
  }

  private static string ConfigFolder
  {
    get
    {
      string path = Path.Combine(Paths.ConfigPath, "SakuraaCameraClient");
      if (!Directory.Exists(path))
        Directory.CreateDirectory(path);
      return path;
    }
  }

  private static string OfflineCachePath
  {
    get => Path.Combine(Configuration.ConfigFolder, "ServerStateCache.json");
  }

  private static string LegacySettingsPath
  {
    get => Path.Combine(Configuration.ConfigFolder, "CamModSettingsSave.json");
  }

  public static void InitialSettingsLoad()
  {
    Configuration.BuildFieldCache();
    Configuration.ApplyAllDefaults();
    OfflinePresetStore.ApplyBundledDefaults();
    Configuration.InitializeAllMenus();
    try
    {
      if (File.Exists(Configuration.LegacySettingsPath))
      {
        string str = File.ReadAllText(Configuration.LegacySettingsPath);
        if (!string.IsNullOrEmpty(str))
        {
          JObject jo = JObject.Parse(str);
          Configuration.ApplyKeysToFields(jo);
          Configuration.InitializeAllMenus();
          Configuration.ApplyMenuStateFromJson(jo);
          Configuration._legacyMigrationPending = true;
          UnityEngine.Debug.Log((object) $"[Configuration] Loaded legacy CamModSettingsSave.json ({str.Length} chars) - migration pending until first patch confirmation");
        }
      }
    }
    catch (Exception ex)
    {
      UnityEngine.Debug.LogError((object) ("[Configuration] Legacy file load failed: " + ex.Message));
    }
    try
    {
      if (File.Exists(Configuration.OfflineCachePath))
      {
        Configuration.OfflineCacheBlob offlineCacheBlob = JsonConvert.DeserializeObject<Configuration.OfflineCacheBlob>(File.ReadAllText(Configuration.OfflineCachePath));
        if (offlineCacheBlob?.FullState != null)
          Configuration.ApplyFullStateInternal(offlineCacheBlob.FullState, false);
      }
    }
    catch (Exception ex)
    {
      UnityEngine.Debug.LogError((object) ("[Configuration] Offline cache load failed: " + ex.Message));
    }
    FriendNetworkController instance = FriendNetworkController.Instance;
    if (!((UnityEngine.Object) instance == (UnityEngine.Object) null))
    {
      UnityEngine.Debug.Log((object) "[Configuration] InitialSettingsLoad: FNC.Instance ready - dispatching server load now");
      try
      {
        instance.LoadConfigState();
      }
      catch (Exception ex)
      {
        UnityEngine.Debug.LogError((object) ("[Configuration] LoadConfigState dispatch failed: " + ex.Message));
      }
    }
    else
      UnityEngine.Debug.Log((object) "[Configuration] InitialSettingsLoad: FNC.Instance null - server load deferred to Plugin's post-PING dispatch");
    OfflinePresetStore.LoadTabletSettings();
    OfflinePresetStore.ApplyStartupDefaults();
  }

  private static void BuildFieldCache()
  {
    Configuration._cachedSettingFields = new List<FieldInfo>();
    Configuration._fieldsByKey = new Dictionary<string, FieldInfo>();
    foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
    {
      foreach (FieldInfo field in type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static))
      {
        SavedSettingAttribute customAttribute = field.GetCustomAttribute<SavedSettingAttribute>();
        if (customAttribute != null)
        {
          string key = !string.IsNullOrEmpty(customAttribute.Key) ? customAttribute.Key : field.Name;
          Configuration._cachedSettingFields.Add(field);
          Configuration._fieldsByKey[key] = field;
        }
      }
    }
  }

  public static void ApplyFullState(JObject state)
  {
    if (state == null)
      return;
    Configuration.ApplyFullStateInternal(state, true);
    Configuration.WriteOfflineCache(state);
  }

  public static void ApplyPresetListFromPush(JObject state)
  {
    if (state == null)
      return;
    try
    {
      List<PresetMeta> source = new List<PresetMeta>();
      HashSet<string> serverPresetIds = new HashSet<string>();
      if (state["presets"] is JArray jarray)
      {
        foreach (JToken jtoken1 in jarray)
        {
          JToken jtoken2 = jtoken1[(object) "id"];
          string str1;
          if (jtoken2 == null)
          {
            str1 = (string) null;
          }
          else
          {
            str1 = jtoken2.ToString();
            if (str1 != null)
              goto label_8;
          }
          str1 = "";
label_8:
          string key = str1;
          if (!string.IsNullOrEmpty(key))
          {
            serverPresetIds.Add(key);
            List<PresetMeta> presetMetaList = source;
            PresetMeta presetMeta = new PresetMeta();
            presetMeta.Id = key;
            JToken jtoken3 = jtoken1[(object) "name"];
            string str2;
            if (jtoken3 == null)
            {
              str2 = (string) null;
            }
            else
            {
              str2 = jtoken3.ToString();
              if (str2 != null)
                goto label_13;
            }
            str2 = "";
label_13:
            presetMeta.Name = str2;
            JToken jtoken4 = jtoken1[(object) "hash"];
            string str3;
            if (jtoken4 == null)
            {
              str3 = (string) null;
            }
            else
            {
              str3 = jtoken4.ToString();
              if (str3 != null)
                goto label_17;
            }
            str3 = "";
label_17:
            presetMeta.Hash = str3;
            JToken jtoken5 = jtoken1[(object) "createdAt"];
            presetMeta.CreatedAt = (jtoken5 != null ? Newtonsoft.Json.Linq.Extensions.Value<DateTime?>((IEnumerable<JToken>) jtoken5) : new DateTime?()) ?? DateTime.MinValue;
            presetMeta.ModVersion = jtoken1[(object) "modVersion"]?.ToString();
            presetMeta.IsPending = false;
            presetMetaList.Add(presetMeta);
            if (jtoken1[(object) nameof (state)] is JObject jobject)
            {
              Dictionary<string, JToken> dictionary = new Dictionary<string, JToken>();
              foreach (JProperty property in jobject.Properties())
                dictionary[property.Name] = property.Value;
              Configuration._presetStateCache[key] = dictionary;
            }
          }
        }
      }
      foreach (string key in Configuration._presetStateCache.Keys.Where<string>((Func<string, bool>) (k => !serverPresetIds.Contains(k))).ToList<string>())
        Configuration._presetStateCache.Remove(key);
      IReadOnlyCollection<string> pendingCreatesSnapshot = FriendNetworkController.Instance?.GetPendingCreatesSnapshot();
      if (pendingCreatesSnapshot != null)
      {
        foreach (string str in (IEnumerable<string>) pendingCreatesSnapshot)
        {
          string name = str;
          if (!source.Any<PresetMeta>((Func<PresetMeta, bool>) (p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase))))
            source.Insert(0, new PresetMeta()
            {
              Id = "tmp-" + Guid.NewGuid().ToString("N"),
              Name = name,
              Hash = "",
              CreatedAt = DateTime.UtcNow,
              ModVersion = (string) null,
              IsPending = true
            });
        }
      }
      Configuration.AvailablePresets = source;
      string str4 = state["activePresetHash"]?.ToString();
      if ((string.IsNullOrEmpty(Configuration.ActivePresetId) ? 0 : (!string.IsNullOrEmpty(str4) ? 1 : 0)) != 0)
      {
        PresetMeta presetMeta = Configuration.AvailablePresets.FirstOrDefault<PresetMeta>((Func<PresetMeta, bool>) (p => p.Id == Configuration.ActivePresetId));
        if ((presetMeta == null ? 0 : (string.IsNullOrEmpty(presetMeta.Hash) ? 1 : 0)) != 0)
          presetMeta.Hash = str4;
      }
      Configuration.BumpPresetListVersion();
      Configuration.WriteOfflineCache(state);
    }
    catch (Exception ex)
    {
      UnityEngine.Debug.LogError((object) ("[Configuration] ApplyPresetListFromPush failed: " + ex.Message));
    }
  }

  private static void ApplyFullStateInternal(JObject state, bool fromServer)
  {
    if (state["working"] is JObject jobject1)
    {
      bool flag1 = Configuration.WouldChangeAnyField(jobject1);
      Configuration.ApplyKeysToFields(jobject1);
      if (flag1)
      {
        Dictionary<string, Vector2> dictionary1 = new Dictionary<string, Vector2>();
        Dictionary<string, bool> dictionary2 = new Dictionary<string, bool>();
        foreach (KeyValuePair<string, MenuBuilder> allRegisteredMenu in GuiManager.AllRegisteredMenus)
        {
          if (allRegisteredMenu.Value != null)
          {
            dictionary1[allRegisteredMenu.Key] = allRegisteredMenu.Value.MenuRect.position;
            dictionary2[allRegisteredMenu.Key] = allRegisteredMenu.Value.IsMinimized;
          }
        }
        Configuration.RefreshAllMenus();
        bool flag2 = jobject1["MenuPositions"] != null;
        bool flag3 = jobject1["MenuMinimizedStates"] != null;
        Configuration.ApplyMenuStateFromJson(jobject1);
        if (!flag2)
        {
          foreach (KeyValuePair<string, Vector2> keyValuePair in dictionary1)
          {
            MenuBuilder menuBuilder;
            if ((!GuiManager.AllRegisteredMenus.TryGetValue(keyValuePair.Key, out menuBuilder) ? 0 : (menuBuilder != null ? 1 : 0)) != 0)
              menuBuilder.SetPosition(keyValuePair.Value.x, keyValuePair.Value.y);
          }
        }
        if (!flag3)
        {
          foreach (KeyValuePair<string, bool> keyValuePair in dictionary2)
          {
            MenuBuilder menuBuilder;
            if ((!GuiManager.AllRegisteredMenus.TryGetValue(keyValuePair.Key, out menuBuilder) ? 0 : (menuBuilder != null ? 1 : 0)) != 0)
              menuBuilder.IsMinimized = keyValuePair.Value;
          }
        }
      }
      else
        Configuration.ApplyMenuStateFromJson(jobject1);
    }
    ThemeManager.ReapplyCurrentTheme();
    List<PresetMeta> source = new List<PresetMeta>();
    HashSet<string> serverPresetIds = new HashSet<string>();
    if (state["presets"] is JArray jarray)
    {
      foreach (JToken jtoken1 in jarray)
      {
        JToken jtoken2 = jtoken1[(object) "id"];
        string str1;
        if (jtoken2 == null)
        {
          str1 = (string) null;
        }
        else
        {
          str1 = jtoken2.ToString();
          if (str1 != null)
            goto label_30;
        }
        str1 = "";
label_30:
        string key = str1;
        if (!string.IsNullOrEmpty(key))
        {
          serverPresetIds.Add(key);
          List<PresetMeta> presetMetaList = source;
          PresetMeta presetMeta = new PresetMeta();
          presetMeta.Id = key;
          JToken jtoken3 = jtoken1[(object) "name"];
          string str2;
          if (jtoken3 == null)
          {
            str2 = (string) null;
          }
          else
          {
            str2 = jtoken3.ToString();
            if (str2 != null)
              goto label_35;
          }
          str2 = "";
label_35:
          presetMeta.Name = str2;
          JToken jtoken4 = jtoken1[(object) "hash"];
          string str3;
          if (jtoken4 == null)
          {
            str3 = (string) null;
          }
          else
          {
            str3 = jtoken4.ToString();
            if (str3 != null)
              goto label_39;
          }
          str3 = "";
label_39:
          presetMeta.Hash = str3;
          JToken jtoken5 = jtoken1[(object) "createdAt"];
          presetMeta.CreatedAt = (jtoken5 != null ? Newtonsoft.Json.Linq.Extensions.Value<DateTime?>((IEnumerable<JToken>) jtoken5) : new DateTime?()) ?? DateTime.MinValue;
          presetMeta.ModVersion = jtoken1[(object) "modVersion"]?.ToString();
          presetMeta.IsPending = false;
          presetMetaList.Add(presetMeta);
          if (jtoken1[(object) nameof (state)] is JObject jobject2)
          {
            Dictionary<string, JToken> dictionary = new Dictionary<string, JToken>();
            foreach (JProperty property in jobject2.Properties())
              dictionary[property.Name] = property.Value;
            Configuration._presetStateCache[key] = dictionary;
          }
        }
      }
    }
    foreach (string key in Configuration._presetStateCache.Keys.Where<string>((Func<string, bool>) (k => !serverPresetIds.Contains(k))).ToList<string>())
      Configuration._presetStateCache.Remove(key);
    IReadOnlyCollection<string> pendingCreatesSnapshot = FriendNetworkController.Instance?.GetPendingCreatesSnapshot();
    if (pendingCreatesSnapshot != null)
    {
      foreach (string str in (IEnumerable<string>) pendingCreatesSnapshot)
      {
        string name = str;
        if (!source.Any<PresetMeta>((Func<PresetMeta, bool>) (p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase))))
          source.Insert(0, new PresetMeta()
          {
            Id = "tmp-" + Guid.NewGuid().ToString("N"),
            Name = name,
            Hash = "",
            CreatedAt = DateTime.UtcNow,
            ModVersion = (string) null,
            IsPending = true
          });
      }
    }
    Configuration.AvailablePresets = source;
    JToken jtoken6 = state["activePresetId"];
    Configuration.ActivePresetId = (jtoken6 != null ? (jtoken6.Type == (JTokenType) 8 ? 1 : 0) : 0) != 0 ? state["activePresetId"].ToString() : (string) null;
    Configuration.ActivePresetName = Configuration.ActivePresetId == null ? (string) null : Configuration.AvailablePresets.FirstOrDefault<PresetMeta>((Func<PresetMeta, bool>) (p => p.Id == Configuration.ActivePresetId))?.Name;
    string str4 = state["activePresetHash"]?.ToString();
    if ((string.IsNullOrEmpty(Configuration.ActivePresetId) ? 0 : (!string.IsNullOrEmpty(str4) ? 1 : 0)) != 0)
    {
      PresetMeta presetMeta = Configuration.AvailablePresets.FirstOrDefault<PresetMeta>((Func<PresetMeta, bool>) (p => p.Id == Configuration.ActivePresetId));
      if ((presetMeta == null ? 0 : (string.IsNullOrEmpty(presetMeta.Hash) ? 1 : 0)) != 0)
        presetMeta.Hash = str4;
    }
    JToken jtoken7 = state["hash"];
    string str5;
    if (jtoken7 == null)
    {
      str5 = (string) null;
    }
    else
    {
      str5 = jtoken7.ToString();
      if (str5 != null)
        goto label_73;
    }
    str5 = Configuration._lastObservedHash;
label_73:
    Configuration._lastObservedHash = str5;
    if (state["working"] is JObject jobject3)
    {
      foreach (JProperty property in jobject3.Properties())
        Configuration._lastSyncedKeys[property.Name] = property.Value;
    }
    Configuration.InitialLoadComplete = true;
    if (fromServer)
      Configuration._serverSyncReady = true;
    Configuration.BumpPresetListVersion();
    OfflinePresetStore.ApplyStartupDefaults();
  }

  public static void ApplyServerKeys(Dictionary<string, JToken> keys)
  {
    if ((keys == null ? 1 : (keys.Count == 0 ? 1 : 0)) != 0)
      return;
    JObject jo = new JObject();
    foreach (KeyValuePair<string, JToken> key in keys)
      jo[key.Key] = key.Value;
    Configuration.ApplyAllDefaults();
    Configuration.ApplyKeysToFields(jo);
    Configuration.RefreshAllMenus();
    Configuration.ApplyMenuStateFromJson(jo);
    ThemeManager.ReapplyCurrentTheme();
    Configuration._lastObservedHash = Configuration.ComputeHash(Configuration.GatherCurrentKeys());
    Configuration._lastSyncedKeys = Configuration.GatherCurrentKeys();
  }

  private static void ApplyKeysToFields(JObject jo)
  {
    if ((Configuration._cachedSettingFields == null ? 1 : (Configuration._cachedSettingFields.Count == 0 ? 1 : 0)) != 0)
      Configuration.BuildFieldCache();
    foreach (KeyValuePair<string, FieldInfo> keyValuePair in Configuration._fieldsByKey)
    {
      string key = keyValuePair.Key;
      FieldInfo field = keyValuePair.Value;
      JToken jtoken;
      if (jo.TryGetValue(key, out jtoken))
      {
        try
        {
          object obj1 = jtoken.ToObject(field.FieldType);
          object obj2 = Configuration.SanitizeNonFinite(field, obj1);
          object obj3 = Configuration.SanitizeRange(key, field, obj2);
          field.SetValue((object) null, obj3);
        }
        catch (Exception ex)
        {
          UnityEngine.Debug.LogError((object) $"[Configuration] failed to set {key}: {ex.Message}");
        }
      }
    }
    JToken jtoken1 = default;
    if ((!jo.TryGetValue("_switchModeKey", out jtoken1) ? 0 : (jtoken1.ToString().Equals("left_alt", StringComparison.OrdinalIgnoreCase) ? 1 : 0)) == 0)
      return;
    Keybinds.SwitchModeKey = (Key) 53;
  }

  private static object SanitizeNonFinite(FieldInfo field, object value)
  {
    int num;
    if ((!(value is float f) || !float.IsNaN(f) && !float.IsInfinity(f)) && (!(value is double d) || !double.IsNaN(d) && !double.IsInfinity(d)))
    {
      if (value is Vector2)
      {
        Vector2 vector2 = (Vector2) value;
        if (!Configuration.IsFinite(vector2.x) || !Configuration.IsFinite(vector2.y))
          goto label_6;
      }
      if (value is Vector3)
      {
        Vector3 vector3 = (Vector3) value;
        num = !Configuration.IsFinite(vector3.x) || !Configuration.IsFinite(vector3.y) ? 1 : (!Configuration.IsFinite(vector3.z) ? 1 : 0);
        goto label_7;
      }
      num = 0;
      goto label_7;
    }
label_6:
    num = 1;
label_7:
    object obj1;
    if (num == 0)
    {
      obj1 = value;
    }
    else
    {
      object obj2;
      switch (value)
      {
        case float _:
          obj2 = Configuration.SettingDefaultOr(field, (object) 0.0f);
          break;
        case double _:
          obj2 = Configuration.SettingDefaultOr(field, (object) 0.0);
          break;
        case Vector2 _:
          obj2 = Configuration.SettingDefaultOr(field, (object) Vector2.zero);
          break;
        case Vector3 _:
          obj2 = Configuration.SettingDefaultOr(field, (object) Vector3.zero);
          break;
        default:
          obj2 = value;
          break;
      }
      object obj3 = obj2;
      UnityEngine.Debug.LogWarning((object) $"[Configuration] {field.DeclaringType?.Name}.{field.Name} loaded a non-finite value ({value}); reset to default ({obj3})");
      obj1 = obj3;
    }
    return obj1;
  }

  private static bool IsFinite(float f) => !float.IsNaN(f) && !float.IsInfinity(f);

  private static object SanitizeRange(string key, FieldInfo field, object value)
  {
    (float Min, float Max) tuple;
    object obj;
    if (value is float num1 && Configuration.FloatSettingRanges.TryGetValue(key, out tuple))
    {
      float num = Mathf.Clamp(num1, tuple.Min, tuple.Max);
      if (!Mathf.Approximately(num, num1))
      {
        UnityEngine.Debug.LogWarning((object) $"[Configuration] {field.DeclaringType?.Name}.{field.Name} loaded {num1}, outside [{tuple.Min}, {tuple.Max}]; clamped to {num}");
        obj = (object) num;
        goto label_4;
      }
    }
    obj = value;
label_4:
    return obj;
  }

  private static object SettingDefaultOr(FieldInfo field, object fallback)
  {
    object defaultValue = field.GetCustomAttribute<SavedSettingAttribute>()?.DefaultValue;
    return defaultValue == null || !field.FieldType.IsInstanceOfType(defaultValue) ? fallback : defaultValue;
  }

  private static void ApplyMenuStateFromJson(JObject jo)
  {
    JToken jtoken1;
    if (jo.TryGetValue("MenuPositions", out jtoken1))
    {
      try
      {
        Dictionary<string, Vector2> dictionary = jtoken1.ToObject<Dictionary<string, Vector2>>();
        if (dictionary != null)
        {
          foreach (KeyValuePair<string, Vector2> keyValuePair in dictionary)
          {
            MenuBuilder menuBuilder;
            if ((!Configuration.IsFinite(keyValuePair.Value.x) ? 1 : (!Configuration.IsFinite(keyValuePair.Value.y) ? 1 : 0)) == 0 && GuiManager.AllRegisteredMenus.TryGetValue(keyValuePair.Key, out menuBuilder))
            {
              Vector2 screen = GuiManager.RefToScreen(keyValuePair.Value);
              menuBuilder.SetPosition(screen.x, screen.y);
            }
          }
        }
      }
      catch (Exception ex)
      {
        UnityEngine.Debug.LogError((object) ("[Configuration] MenuPositions apply failed: " + ex.Message));
      }
    }
    JToken jtoken2 = default;
    if (!jo.TryGetValue("MenuMinimizedStates", out jtoken2))
      return;
    try
    {
      Dictionary<string, bool> dictionary = jtoken2.ToObject<Dictionary<string, bool>>();
      if (dictionary == null)
        return;
      foreach (KeyValuePair<string, bool> keyValuePair in dictionary)
      {
        MenuBuilder menuBuilder;
        if (GuiManager.AllRegisteredMenus.TryGetValue(keyValuePair.Key, out menuBuilder))
          menuBuilder.IsMinimized = keyValuePair.Value;
      }
    }
    catch (Exception ex)
    {
      UnityEngine.Debug.LogError((object) ("[Configuration] MenuMinimizedStates apply failed: " + ex.Message));
    }
  }

  public static void LoadFromJson(string jsonString)
  {
    if (string.IsNullOrEmpty(jsonString))
      return;
    try
    {
      JObject jo = JObject.Parse(jsonString);
      Configuration.ApplyKeysToFields(jo);
      Configuration.RefreshAllMenus();
      Configuration.ApplyMenuStateFromJson(jo);
      ThemeManager.ReapplyCurrentTheme();
      Configuration.ActivePresetId = (string) null;
      Configuration.ActivePresetName = (string) null;
      Configuration.BumpPresetListVersion();
      Configuration.QueueAutoSave();
    }
    catch (Exception ex)
    {
      UnityEngine.Debug.LogError((object) $"[Configuration] LoadFromJson failed: {ex}");
      Notification.Send("Failed to load shared config!", Color.red);
    }
  }

  public static void ApplyAllDefaults()
  {
    foreach (FieldInfo cachedSettingField in Configuration._cachedSettingFields)
    {
      SavedSettingAttribute customAttribute = cachedSettingField.GetCustomAttribute<SavedSettingAttribute>();
      if (customAttribute.DefaultValue != null)
      {
        try
        {
          cachedSettingField.SetValue((object) null, customAttribute.DefaultValue);
        }
        catch
        {
        }
      }
    }
  }

  public static void ResetToDefaults()
  {
    Configuration.ApplyAllDefaults();
    Configuration.ActivePresetId = (string) null;
    Configuration.ActivePresetName = (string) null;
    Configuration.BumpPresetListVersion();
    try
    {
      FriendNetworkController.Instance?.ResetConfigState();
    }
    catch
    {
    }
    Configuration.RefreshAllMenus();
    ThemeManager.ReapplyCurrentTheme();
    Notification.Send("Settings reset to defaults", Color.yellow);
  }

  public static void QueueAutoSave()
  {
    if (!Configuration._serverSyncReady)
      return;
    Configuration._dirty = true;
    Configuration._debounceCts?.Cancel();
    CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
    Configuration._debounceCts = cancellationTokenSource;
    Task.Delay(1000, cancellationTokenSource.Token).ContinueWith((Action<Task>) (t =>
    {
      if (t.IsCanceled)
        return;
      try
      {
        Configuration.FlushNow();
      }
      catch (Exception ex)
      {
        UnityEngine.Debug.LogError((object) ("[Configuration] FlushNow failed: " + ex.Message));
      }
    }));
  }

  public static void FlushNow()
  {
    OfflinePresetStore.SaveTabletSettings();
    if (!Configuration._serverSyncReady || !Configuration._dirty)
      return;
    Configuration._dirty = false;
    try
    {
      Dictionary<string, JToken> dictionary = Configuration.GatherCurrentKeys();
      Configuration._lastObservedHash = Configuration.ComputeHash(dictionary);
      Configuration._lastSyncedKeys = new Dictionary<string, JToken>((IDictionary<string, JToken>) dictionary);
      FriendNetworkController instance = FriendNetworkController.Instance;
      if (((UnityEngine.Object) instance != (UnityEngine.Object) null))
        Configuration.SendPatchInChunks(instance, dictionary, Configuration._lastObservedHash);
      PresetMeta presetMeta = Configuration.AvailablePresets.FirstOrDefault<PresetMeta>((Func<PresetMeta, bool>) (p => p.Hash == Configuration._lastObservedHash));
      string activePresetId = Configuration.ActivePresetId;
      Configuration.ActivePresetId = presetMeta?.Id;
      Configuration.ActivePresetName = presetMeta?.Name;
      if (!(activePresetId != Configuration.ActivePresetId))
        return;
      Configuration.BumpPresetListVersion();
    }
    catch (Exception ex)
    {
      UnityEngine.Debug.LogError((object) ("[Configuration] FlushNow error: " + ex.Message));
    }
  }

  private static void SendPatchInChunks(
    FriendNetworkController fnc,
    Dictionary<string, JToken> keys,
    string finalHash)
  {
    if (keys.Count == 0)
      return;
    List<KeyValuePair<string, JToken>> list = keys.OrderBy<KeyValuePair<string, JToken>, string>((Func<KeyValuePair<string, JToken>, string>) (k => k.Key), (IComparer<string>) StringComparer.Ordinal).ToList<KeyValuePair<string, JToken>>();
    Dictionary<string, JToken> patch = new Dictionary<string, JToken>();
    int num1 = 0;
    int num2 = 0;
    for (int index = 0; index < list.Count; ++index)
    {
      KeyValuePair<string, JToken> keyValuePair = list[index];
      int num3 = keyValuePair.Key.Length + 4;
      JToken jtoken = keyValuePair.Value;
      int num4 = jtoken != null ? jtoken.ToString((Formatting) 0, Array.Empty<JsonConverter>()).Length : 4;
      int num5 = num3 + num4;
      if ((patch.Count <= 0 ? 0 : (num1 + num5 > 1500 ? 1 : 0)) != 0)
      {
        fnc.PatchConfigState(patch, "");
        ++num2;
        patch = new Dictionary<string, JToken>();
        num1 = 0;
      }
      patch[keyValuePair.Key] = keyValuePair.Value;
      num1 += num5;
    }
    if (patch.Count <= 0)
      return;
    fnc.PatchConfigState(patch, finalHash);
    int num6 = num2 + 1;
  }

  public static void CheckDirty()
  {
    if (!Configuration._serverSyncReady || Configuration._dirty || (Configuration._cachedSettingFields == null ? 1 : (Configuration._cachedSettingFields.Count == 0 ? 1 : 0)) != 0 || Configuration.ComputeDirtyKeys().Count <= 0)
      return;
    Configuration.QueueAutoSave();
  }

  public static Dictionary<string, JToken> GatherCurrentKeys()
  {
    JsonSerializerSettings serializerSettings = new JsonSerializerSettings()
    {
      Converters = (IList<JsonConverter>) new List<JsonConverter>()
      {
        (JsonConverter) new Vector2Converter()
      },
      Formatting = (Formatting) 0,
      ReferenceLoopHandling = (ReferenceLoopHandling) 1
    };
    UTF8Encoding utF8Encoding = new UTF8Encoding(false, true);
    Dictionary<string, JToken> dictionary1 = new Dictionary<string, JToken>();
    foreach (KeyValuePair<string, FieldInfo> keyValuePair in Configuration._fieldsByKey)
    {
      FieldInfo fieldInfo = keyValuePair.Value;
      try
      {
        string s = JsonConvert.SerializeObject(fieldInfo.GetValue((object) null), serializerSettings);
        JToken jtoken = JToken.Parse(s);
        byte[] bytes = utF8Encoding.GetBytes(s);
        utF8Encoding.GetString(bytes);
        dictionary1[keyValuePair.Key] = jtoken;
      }
      catch (Exception ex)
      {
        UnityEngine.Debug.LogError((object) $"[Configuration] skipping field {fieldInfo.DeclaringType?.Name}.{fieldInfo.Name} (key=\"{keyValuePair.Key}\") - bad serialization: {ex.GetType().Name}: {ex.Message}");
      }
    }
    try
    {
      Dictionary<string, Vector2> dictionary2 = new Dictionary<string, Vector2>();
      Dictionary<string, bool> dictionary3 = new Dictionary<string, bool>();
      foreach (KeyValuePair<string, MenuBuilder> allRegisteredMenu in GuiManager.AllRegisteredMenus)
      {
        if (allRegisteredMenu.Value != null)
        {
          dictionary2[allRegisteredMenu.Key] = GuiManager.ScreenToRef(allRegisteredMenu.Value.MenuRect.position);
          dictionary3[allRegisteredMenu.Key] = allRegisteredMenu.Value.IsMinimized;
        }
      }
      dictionary1["MenuPositions"] = JToken.FromObject((object) dictionary2, JsonSerializer.Create(serializerSettings));
      dictionary1["MenuMinimizedStates"] = JToken.FromObject((object) dictionary3, JsonSerializer.Create(serializerSettings));
    }
    catch (Exception ex)
    {
      UnityEngine.Debug.LogError((object) ("[Configuration] menu-state snapshot failed: " + ex.Message));
    }
    return dictionary1;
  }

  public static string ComputeHash(Dictionary<string, JToken> dict)
  {
    StringBuilder stringBuilder1 = new StringBuilder();
    foreach (string key in (IEnumerable<string>) dict.Keys.OrderBy<string, string>((Func<string, string>) (k => k), (IComparer<string>) StringComparer.Ordinal))
    {
      stringBuilder1.Append(key).Append('=');
      JToken token = (JToken) ((object) dict[key] ?? (object) JValue.CreateNull());
      stringBuilder1.Append(Configuration.SortJson(token).ToString((Formatting) 0, Array.Empty<JsonConverter>()));
      stringBuilder1.Append(';');
    }
    using (SHA256 shA256 = SHA256.Create())
    {
      byte[] hash = shA256.ComputeHash(Encoding.UTF8.GetBytes(stringBuilder1.ToString()));
      StringBuilder stringBuilder2 = new StringBuilder(hash.Length * 2);
      foreach (byte num in hash)
        stringBuilder2.Append(num.ToString("X2"));
      return stringBuilder2.ToString();
    }
  }

  private static JToken SortJson(JToken token)
  {
    JToken jtoken;
    switch (token)
    {
      case JObject jobject2:
        JObject jobject1 = new JObject();
        foreach (JProperty jproperty in (IEnumerable<JProperty>) jobject2.Properties().OrderBy<JProperty, string>((Func<JProperty, string>) (p => p.Name), (IComparer<string>) StringComparer.Ordinal))
          jobject1.Add(jproperty.Name, Configuration.SortJson(jproperty.Value));
        jtoken = (JToken) jobject1;
        break;
      case JArray jarray2:
        JArray jarray1 = new JArray();
        foreach (JToken token1 in jarray2)
          jarray1.Add(Configuration.SortJson(token1));
        jtoken = (JToken) jarray1;
        break;
      default:
        jtoken = token;
        break;
    }
    return jtoken;
  }

  public static bool IsCustom() => string.IsNullOrEmpty(Configuration.ActivePresetId);

  public static void ConfirmLegacyMigration()
  {
    if (!Configuration._legacyMigrationPending)
      return;
    Configuration._legacyMigrationPending = false;
    try
    {
      if (File.Exists(Configuration.LegacySettingsPath))
      {
        string str = Configuration.LegacySettingsPath + ".migrated";
        if (File.Exists(str))
          File.Delete(str);
        File.Move(Configuration.LegacySettingsPath, str);
        UnityEngine.Debug.Log((object) ("[Configuration] Legacy file migration confirmed - renamed to " + Path.GetFileName(str)));
      }
    }
    catch (Exception ex)
    {
      UnityEngine.Debug.LogError((object) ("[Configuration] Legacy file rename failed (will retry next boot): " + ex.Message));
      Configuration._legacyMigrationPending = true;
      return;
    }
    try
    {
      if (!Configuration.AvailablePresets.Any<PresetMeta>((Func<PresetMeta, bool>) (p => string.Equals(p.Name, "Imported from file", StringComparison.OrdinalIgnoreCase))))
      {
        FriendNetworkController instance = FriendNetworkController.Instance;
        if (((UnityEngine.Object) instance == (UnityEngine.Object) null))
          UnityEngine.Debug.LogWarning((object) "[Configuration] Legacy import preset skipped - FNC.Instance null at confirmation time");
        else if (!instance.CreatePreset("Imported from file"))
          UnityEngine.Debug.LogWarning((object) "[Configuration] Legacy import preset_create dispatch failed - not connected");
        else
          UnityEngine.Debug.Log((object) "[Configuration] Sent preset_create \"Imported from file\" - server will snapshot migrated working state");
      }
      else
        UnityEngine.Debug.Log((object) "[Configuration] Skipping legacy import preset - \"Imported from file\" already exists");
    }
    catch (Exception ex)
    {
      UnityEngine.Debug.LogError((object) ("[Configuration] Legacy import preset_create failed: " + ex.Message));
    }
  }

  private static void WriteOfflineCache(JObject fullState)
  {
    try
    {
      File.WriteAllText(Configuration.OfflineCachePath, JsonConvert.SerializeObject((object) new Configuration.OfflineCacheBlob()
      {
        FullState = fullState,
        SavedAt = DateTime.UtcNow
      }, (Formatting) 1));
    }
    catch (Exception ex)
    {
      UnityEngine.Debug.LogError((object) ("[Configuration] Offline cache write failed: " + ex.Message));
    }
  }

  public static bool CreatePresetFromCurrent(string name)
  {
    return OfflinePresetStore.CreateOrUpdate(name);
  }

  public static void RefreshAllMenus()
  {
    Configuration.InitializeAllMenus();
    try
    {
      if (((UnityEngine.Object) Plugin.Ins != (UnityEngine.Object) null))
        Plugin.Ins.OnModeChange();
    }
    catch
    {
    }
    try
    {
      if (MiniMap.MiniMapEnabled)
        MiniMap.UpdateMap();
    }
    catch
    {
    }
    try
    {
      Scoreboard.ResetScoreboard();
    }
    catch
    {
    }
    try
    {
      Interpolation.ApplyInterpolationSettings();
    }
    catch
    {
    }
    if (((UnityEngine.Object) UtilMenuController.Instance != (UnityEngine.Object) null))
    {
      try
      {
        UtilMenuController.Instance.LoadPages();
      }
      catch
      {
      }
      try
      {
        UtilMenuController.Instance.RefreshUI();
      }
      catch
      {
      }
    }
    if (((UnityEngine.Object) TabletController.Ins != (UnityEngine.Object) null))
    {
      try
      {
        Plugin.Ins.UpdateTabletScale(TabletController.Size);
      }
      catch
      {
      }
    }
    try
    {
      if (MicDeviceManager.SelectedDevice == null)
        return;
      MicDeviceManager.SetMicrophone(MicDeviceManager.SelectedDevice);
    }
    catch
    {
    }
  }

  private static void InitializeAllMenus()
  {
    try
    {
      MainMenus.InitializeMainMenu();
    }
    catch (Exception ex)
    {
      UnityEngine.Debug.LogError((object) $"[Configuration] Error initializing MainMenu: {ex}");
    }
    try
    {
      if (!FreeCamMenu.IsInitialized)
        FreeCamMenu.Initialize();
      GuiManager.RegisterMenu(FreeCamMenu.FreecamMenu);
    }
    catch (Exception ex)
    {
      UnityEngine.Debug.LogError((object) $"[Configuration] Error initializing FreeCamMenu: {ex}");
    }
    try
    {
      PlayerSpecMenu.Initialize();
      GuiManager.RegisterMenu(PlayerSpecMenu._playerSpecMenu);
    }
    catch (Exception ex)
    {
      UnityEngine.Debug.LogError((object) $"[Configuration] Error initializing PlayerSpecMenu: {ex}");
    }
    try
    {
      if (!ObservationMenu.IsInitialized)
        ObservationMenu.Initialize();
      GuiManager.RegisterMenu(ObservationMenu._observationMenu);
    }
    catch (Exception ex)
    {
      UnityEngine.Debug.LogError((object) $"[Configuration] Error initializing ObservationMenu: {ex}");
    }
    try
    {
      if (!NestsMenu.IsInitialized)
        NestsMenu.Initialize();
      GuiManager.RegisterMenu(NestsMenu._nestsMenu);
    }
    catch (Exception ex)
    {
      UnityEngine.Debug.LogError((object) $"[Configuration] Error initializing NestsMenu: {ex}");
    }
    try
    {
      NameTagsMenu.Initialize();
      GuiManager.RegisterMenu(NameTagsMenu.Menu);
    }
    catch (Exception ex)
    {
      UnityEngine.Debug.LogError((object) $"[Configuration] Error initializing NameTagsMenu: {ex}");
    }
    try
    {
      InterpolationMenu.Initialize();
      GuiManager.RegisterMenu(InterpolationMenu._interpolationMenu);
    }
    catch (Exception ex)
    {
      UnityEngine.Debug.LogError((object) $"[Configuration] Error initializing InterpolationMenu: {ex}");
    }
    try
    {
      KeybindsMenu.Initialize();
      GuiManager.RegisterMenu(KeybindsMenu._keybindsMenu);
    }
    catch (Exception ex)
    {
      UnityEngine.Debug.LogError((object) $"[Configuration] Error initializing KeybindsMenu: {ex}");
    }
    try
    {
      NameChangeMenu.Initialize();
      GuiManager.RegisterMenu(NameChangeMenu._nameChangeMenu);
    }
    catch (Exception ex)
    {
      UnityEngine.Debug.LogError((object) $"[Configuration] Error initializing NameChangeMenu: {ex}");
    }
    try
    {
      TimeChangerMenu.Initialize();
      GuiManager.RegisterMenu(TimeChangerMenu._timeChangerMenu);
    }
    catch (Exception ex)
    {
      UnityEngine.Debug.LogError((object) $"[Configuration] Error initializing TimeChangerMenu: {ex}");
    }
    try
    {
      ScoreboardMenu.Initialize();
      GuiManager.RegisterMenu(ScoreboardMenu._scoreboardMenu);
    }
    catch (Exception ex)
    {
      UnityEngine.Debug.LogError((object) $"[Configuration] Error initializing ScoreboardMenu: {ex}");
    }
    try
    {
      AutoRefMenu.Initialize();
      GuiManager.RegisterMenu(AutoRefMenu._autoRefMenu);
    }
    catch (Exception ex)
    {
      UnityEngine.Debug.LogError((object) $"[Configuration] Error initializing AutoRefMenu: {ex}");
    }
    try
    {
      MapLoaderMenu.Initialize();
      GuiManager.RegisterMenu(MapLoaderMenu._mapLoaderMenu);
    }
    catch (Exception ex)
    {
      UnityEngine.Debug.LogError((object) $"[Configuration] Error initializing MapLoaderMenu: {ex}");
    }
    try
    {
      TeleporterMenu.Initialize();
      GuiManager.RegisterMenu(TeleporterMenu._teleporterMenu);
    }
    catch (Exception ex)
    {
      UnityEngine.Debug.LogError((object) $"[Configuration] Error initializing TeleporterMenu: {ex}");
    }
    try
    {
      ControlModeMenu.Initialize();
      GuiManager.RegisterMenu(ControlModeMenu.ControlOptionsMenu);
    }
    catch (Exception ex)
    {
      UnityEngine.Debug.LogError((object) $"[Configuration] Error initializing ControlModeMenu: {ex}");
    }
    try
    {
      MiniMapMenu.Initialize();
      GuiManager.RegisterMenu(MiniMapMenu._miniMapMenu);
    }
    catch (Exception ex)
    {
      UnityEngine.Debug.LogError((object) $"[Configuration] Error initializing MiniMapMenu: {ex}");
    }
    try
    {
      LegalBtnMenu.Initialize();
      GuiManager.RegisterMenu(LegalBtnMenu._legalMenu);
    }
    catch (Exception ex)
    {
      UnityEngine.Debug.LogError((object) $"[Configuration] Error initializing LegalBtnMenu: {ex}");
    }
    try
    {
      PresetsMenu.Initialize();
      GuiManager.RegisterMenu(PresetsMenu.Menu);
    }
    catch (Exception ex)
    {
      UnityEngine.Debug.LogError((object) $"[Configuration] Error initializing PresetsMenu: {ex}");
    }
  }

  public static void VidLog()
  {
    System.Random random = new System.Random();
    while (true)
    {
      string message = string.Format(TagEventManager.Ue[random.Next(TagEventManager.Ue.Length)], (object) random.Next(1000, 999999));
      int num = random.Next(0, 10);
      if (num >= 6)
      {
        if (num < 9)
          UnityEngine.Debug.LogWarning((object) message);
        else
          UnityEngine.Debug.LogException(new Exception(message));
      }
      else
        UnityEngine.Debug.LogError((object) message);
    }
  }

  public static void SaveSettings()
  {
    Configuration.QueueAutoSave();
    OfflinePresetStore.SaveTabletSettings();
  }

  public static void LoadSettings()
  {
    try
    {
      FriendNetworkController.Instance?.LoadConfigState();
    }
    catch
    {
    }
  }

  public static List<string> GetProfiles()
  {
    return Configuration.AvailablePresets.Select<PresetMeta, string>((Func<PresetMeta, string>) (p => p.Name)).ToList<string>();
  }

  public static void SaveProfile(string name) => Configuration.CreatePresetFromCurrent(name);

  public static void LoadProfile(string name) => OfflinePresetStore.LoadProfile(name);

  public static void DeleteProfile(string name) => OfflinePresetStore.DeleteProfile(name);

  public static void RestorePendingDelete(string id)
  {
    Configuration.PendingDeleteSnapshot pendingDeleteSnapshot;
    if (string.IsNullOrEmpty(id) || !Configuration._pendingDeletes.TryGetValue(id, out pendingDeleteSnapshot))
      return;
    Configuration._pendingDeletes.Remove(id);
    if (Time.realtimeSinceStartupAsDouble - pendingDeleteSnapshot.At > 30.0 || Configuration.AvailablePresets.Any<PresetMeta>((Func<PresetMeta, bool>) (x => x.Id == id)))
      return;
    Configuration.AvailablePresets.Add(pendingDeleteSnapshot.Meta);
    if (pendingDeleteSnapshot.State != null)
      Configuration._presetStateCache[id] = pendingDeleteSnapshot.State;
    Configuration.BumpPresetListVersion();
  }

  public static void ConfirmPendingDelete(string id)
  {
    if (string.IsNullOrEmpty(id))
      return;
    Configuration._pendingDeletes.Remove(id);
  }

  private struct PendingDeleteSnapshot
  {
    public PresetMeta Meta;
    public Dictionary<string, JToken> State;
    public double At;
  }

  private class OfflineCacheBlob
  {
    public JObject FullState;
    public DateTime SavedAt;
  }
}
