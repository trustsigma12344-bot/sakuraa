using Photon.Realtime;
using SakuraaCastingMod.Core;
using SakuraaCastingMod.Shared.Helpers;
using SakuraaCastingMod.Shared.Models;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Desktop.Camera;

public static class Director
{
  [SavedSetting("DirectorMinShotSeconds", 4.5f)]
  public static float MinShotSeconds = 4.5f;
  [SavedSetting("DirectorMaxShotSeconds", 12f)]
  public static float MaxShotSeconds = 12f;
  [SavedSetting("DirectorTagBeatSeconds", 2.6f)]
  public static float TagBeatSeconds = 2.6f;
  [SavedSetting("DirectorDistance", 3.4f)]
  public static float Distance = 3.4f;
  [SavedSetting("DirectorHeight", 1.2f)]
  public static float Height = 1.2f;
  [SavedSetting("DirectorPosSmooth", 0.5f)]
  public static float PosSmoothTime = 0.5f;
  [SavedSetting("DirectorPanSpeed", 110f)]
  public static float MaxPanSpeed = 110f;
  [SavedSetting("DirectorCutOnTags", true)]
  public static bool CutOnTags = true;
  [SavedSetting("DirectorWallAvoid", true)]
  public static bool WallAvoid = true;
  [SavedSetting("DirectorLeadShots", true)]
  public static bool LeadShots = true;
  [SavedSetting("DirectorWideShots", true)]
  public static bool WideShots = true;
  [SavedSetting("DirectorIncludeLocal", false)]
  public static bool IncludeLocal;
  private const float ActionThreshold = 0.65f;
  private const float EvalInterval = 0.7f;
  private const float VelocityWindow = 0.25f;
  private const float FlowTurnDegPerSec = 50f;
  private const float CutTravelDistance = 10f;
  private const float TagBeatCooldown = 4f;
  private const int StealStreak = 2;
  private static readonly Dictionary<string, Director.Motion> Tracked = new Dictionary<string, Director.Motion>();
  private static readonly Dictionary<string, float> RecentPrimaries = new Dictionary<string, float>();
  private static Director.ShotType _shot = Director.ShotType.Wide;
  private static string _primaryId;
  private static string _secondaryId;
  private static float _shotStartTime;
  private static float _sideSign = 1f;
  private static Vector3 _shotFlow = Vector3.forward;
  private static float _orbitYaw;
  private static float _wideYaw;
  private static float _wideRadius = 6f;
  private static float _tagBeatUntil;
  private static float _nextTagBeatTime;
  private static float _nextEvalTime;
  private static float _nextPurgeTime;
  private static string _challengerId;
  private static int _challengerStreak;
  private static bool _forceCut;
  private static bool _snapNext;
  private static bool _hasPose;
  private static Vector3 _camPos;
  private static Vector3 _camVel;
  private static Quaternion _camRot = Quaternion.identity;
  private static Vector3 _focus;
  private static Vector3 _focusVel;
  private static float _wallDist = -1f;
  private static string _pendingTagPrimary;
  private static string _pendingTagSecondary;
  private static bool _subscribed;
  private static int _lastUpdateFrame = -10;

  public static string CurrentShotLabel { get; private set; } = "...";

  public static void UpdateDirector()
  {
    if ((((UnityEngine.Object) Plugin.Ins == (UnityEngine.Object) null) ? 1 : (((UnityEngine.Object) Plugin.Ins.camera == (UnityEngine.Object) null) ? 1 : 0)) != 0)
      return;
    Director.EnsureSubscribed();
    bool flag = Time.frameCount - Director._lastUpdateFrame > 2;
    Director._lastUpdateFrame = Time.frameCount;
    if (flag)
      Director.ResetState();
    List<GorillaData> gorillaDataList = GorillaDataHandler.GorillaDataList;
    Director.UpdateMotion(gorillaDataList);
    Director.PurgeStale(gorillaDataList);
    if (Director._pendingTagPrimary != null)
    {
      if ((double) Time.time >= (double) Director._nextTagBeatTime)
      {
        Director.BeginShot(Director.ShotType.TagBeat, Director._pendingTagPrimary, Director._pendingTagSecondary, gorillaDataList);
        Director._tagBeatUntil = Time.time + Director.TagBeatSeconds;
        Director._nextTagBeatTime = Director._tagBeatUntil + 4f;
      }
      Director._pendingTagPrimary = (string) null;
      Director._pendingTagSecondary = (string) null;
    }
    else
      Director.EvaluateShot(gorillaDataList);
    Director.ComposeAndApply(gorillaDataList);
  }

  public static void ForceCut() => Director._forceCut = true;

  private static void ResetState()
  {
    Director._shot = Director.ShotType.Wide;
    Director._primaryId = (string) null;
    Director._secondaryId = (string) null;
    Director._shotStartTime = Time.time;
    Director._forceCut = true;
    Director._snapNext = false;
    Director._nextEvalTime = 0.0f;
    Director._challengerId = (string) null;
    Director._challengerStreak = 0;
    Director._pendingTagPrimary = (string) null;
    Director._pendingTagSecondary = (string) null;
    Director._tagBeatUntil = 0.0f;
    Director._wallDist = -1f;
    Director._camVel = Vector3.zero;
    Director._focusVel = Vector3.zero;
    Transform transform = ((Component) Plugin.Ins.camera).transform;
    Director._camPos = transform.position;
    Director._camRot = transform.rotation;
    Director._focus = (transform.position + (transform.forward * 5f));
    Director._hasPose = true;
    Director.CurrentShotLabel = "...";
  }

  private static void EnsureSubscribed()
  {
    if (Director._subscribed)
      return;
    Director._subscribed = true;
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    TagEventManager.OnTagEvent += new TagEventManager.TagEventHandler(Director.OnTag);
  }

  private static void OnTag(Player tagger, Player tagged)
  {
    if (!Director.CutOnTags || (((UnityEngine.Object) Plugin.Ins == (UnityEngine.Object) null) ? 1 : (Plugin.Ins.currentCameraMode != 7 ? 1 : 0)) != 0)
      return;
    string userId1 = tagged?.UserId;
    string userId2 = tagger?.UserId;
    if ((userId1 != null ? 0 : (userId2 == null ? 1 : 0)) != 0)
      return;
    Director._pendingTagPrimary = userId1 ?? userId2;
    Director._pendingTagSecondary = userId1 != null ? userId2 : (string) null;
  }

  private static void UpdateMotion(List<GorillaData> list)
  {
    float time = Time.time;
    for (int index = 0; index < list.Count; ++index)
    {
      GorillaData gorillaData = list[index];
      if ((gorillaData == null || ((UnityEngine.Object) gorillaData.BodyTransform == (UnityEngine.Object) null) ? 1 : (string.IsNullOrEmpty(gorillaData.UserId) ? 1 : 0)) == 0)
      {
        Vector3 position = gorillaData.BodyTransform.position;
        Director.Motion motion;
        if (!Director.Tracked.TryGetValue(gorillaData.UserId, out motion))
        {
          Director.Tracked[gorillaData.UserId] = new Director.Motion()
          {
            SamplePos = position,
            SampleTime = time
          };
        }
        else
        {
          float num = time - motion.SampleTime;
          if ((double) num >= 0.25)
          {
            Vector3 vector3 = ((position - motion.SamplePos) / num);
            if ((double) vector3.sqrMagnitude > 900.0)
              vector3 = motion.Velocity;
            motion.Velocity = Vector3.Lerp(motion.Velocity, vector3, 0.5f);
            motion.SamplePos = position;
            motion.SampleTime = time;
          }
        }
      }
    }
  }

  private static void PurgeStale(List<GorillaData> list)
  {
    if ((double) Time.time < (double) Director._nextPurgeTime)
      return;
    Director._nextPurgeTime = Time.time + 15f;
    HashSet<string> stringSet = new HashSet<string>();
    for (int index = 0; index < list.Count; ++index)
    {
      if ((list[index] == null ? 0 : (!string.IsNullOrEmpty(list[index].UserId) ? 1 : 0)) != 0)
        stringSet.Add(list[index].UserId);
    }
    List<string> stringList1 = new List<string>();
    foreach (string key in Director.Tracked.Keys)
    {
      if (!stringSet.Contains(key))
        stringList1.Add(key);
    }
    foreach (string key in stringList1)
      Director.Tracked.Remove(key);
    List<string> stringList2 = new List<string>();
    foreach (KeyValuePair<string, float> recentPrimary in Director.RecentPrimaries)
    {
      if ((double) Time.time - (double) recentPrimary.Value > 30.0)
        stringList2.Add(recentPrimary.Key);
    }
    foreach (string key in stringList2)
      Director.RecentPrimaries.Remove(key);
  }

  private static void EvaluateShot(List<GorillaData> list)
  {
    if (Director._shot == Director.ShotType.TagBeat)
    {
      if (((double) Time.time >= (double) Director._tagBeatUntil ? 0 : (Director.Find(list, Director._primaryId) != null ? 1 : 0)) != 0)
        return;
      Director._forceCut = true;
    }
    float num1 = Time.time - Director._shotStartTime;
    bool flag1 = Director._shot != Director.ShotType.Wide && Director.Find(list, Director._primaryId) == null;
    if ((Director._forceCut ? 0 : (!flag1 ? 1 : 0)) != 0 && ((double) num1 < (double) Director.MinShotSeconds ? 1 : ((double) Time.time < (double) Director._nextEvalTime ? 1 : 0)) != 0)
      return;
    Director._nextEvalTime = Time.time + 0.7f;
    float currentLive;
    Director.Pick pick = Director.PickBest(list, out currentLive);
    bool flag2;
    if (!(flag2 = Director._forceCut | flag1 || (double) num1 >= (double) Director.MaxShotSeconds))
    {
      if ((pick.PrimaryId == null ? 1 : (pick.PrimaryId == Director._primaryId ? 1 : 0)) != 0)
      {
        if ((!(pick.PrimaryId == Director._primaryId) ? 0 : (Director._shot != Director.ShotType.Wide ? 1 : 0)) != 0)
          Director._secondaryId = pick.SecondaryId;
        Director._challengerId = (string) null;
        Director._challengerStreak = 0;
        return;
      }
      if ((double) pick.Score <= (double) currentLive * 1.5 + 0.25)
      {
        Director._challengerId = (string) null;
        Director._challengerStreak = 0;
        return;
      }
      if (!(pick.PrimaryId == Director._challengerId))
      {
        Director._challengerId = pick.PrimaryId;
        Director._challengerStreak = 1;
      }
      else
        ++Director._challengerStreak;
      if (Director._challengerStreak < 2)
        return;
    }
    Director._challengerId = (string) null;
    Director._challengerStreak = 0;
    if (pick.PrimaryId != null)
    {
      Vector3 vector3 = Director.VelocityOf(pick.PrimaryId);
      float magnitude = vector3.magnitude;
      Director.ShotType type = ((double) pick.Score < 0.64999997615814209 ? 0 : (pick.SecondaryId != null ? 1 : 0)) == 0 ? ((!Director.WideShots ? 0 : (Director.CountValid(list) >= 3 ? 1 : 0)) != 0 ? Director.ShotType.Wide : Director.ShotType.Orbit) : (!Director.LeadShots || (double) magnitude <= 2.5 || (double) UnityEngine.Random.value >= 0.2199999988079071 ? Director.ShotType.Chase : Director.ShotType.Lead);
      if ((!flag2 || !(pick.PrimaryId == Director._primaryId) ? 0 : (type == Director._shot ? 1 : 0)) != 0)
      {
        int num2;
        switch (type)
        {
          case Director.ShotType.Chase:
            type = !Director.LeadShots || (double) magnitude <= 2.5 ? Director.ShotType.Orbit : Director.ShotType.Lead;
            goto label_24;
          case Director.ShotType.Lead:
            num2 = 1;
            break;
          default:
            num2 = type == Director.ShotType.Orbit ? 1 : 0;
            break;
        }
        if (num2 == 0)
        {
          Director._forceCut = false;
          Director._shotStartTime = Time.time;
          return;
        }
        type = Director.ShotType.Chase;
      }
label_24:
      Director.BeginShot(type, pick.PrimaryId, pick.SecondaryId, list);
    }
    else if ((Director.CountValid(list) < 1 ? 0 : (Director._shot != Director.ShotType.Wide ? 1 : 0)) != 0)
    {
      Director.BeginShot(Director.ShotType.Wide, (string) null, (string) null, list);
    }
    else
    {
      Director._forceCut = false;
      Director._shotStartTime = Time.time;
    }
  }

  private static Director.Pick PickBest(List<GorillaData> list, out float currentLive)
  {
    Director.Pick pick = new Director.Pick();
    currentLive = 0.0f;
    string userId = NetworkSystem.Instance?.LocalPlayer?.UserId;
    bool flag = false;
    for (int index = 0; index < list.Count; ++index)
    {
      if ((list[index] == null ? 0 : (list[index].Infected ? 1 : 0)) != 0)
      {
        flag = true;
        break;
      }
    }
    for (int index1 = 0; index1 < list.Count; ++index1)
    {
      GorillaData g1 = list[index1];
      if (Director.IsUsable(g1, userId) && (!flag ? 0 : (g1.Infected ? 1 : 0)) == 0)
      {
        Vector3 position1 = g1.BodyTransform.position;
        Vector3 vector3_1 = Director.VelocityOf(g1.UserId);
        for (int index2 = 0; index2 < list.Count; ++index2)
        {
          if (index2 != index1)
          {
            GorillaData g2 = list[index2];
            if (Director.IsUsable(g2, userId) && (!flag ? 0 : (!g2.Infected ? 1 : 0)) == 0 && (flag ? 0 : (index2 < index1 ? 1 : 0)) == 0)
            {
              Vector3 position2 = g2.BodyTransform.position;
              Vector3 vector3_2 = Director.VelocityOf(g2.UserId);
              float num1 = Vector3.Distance(position1, position2);
              if ((double) num1 <= 40.0)
              {
                Vector3 vector3_3 = (position1 - position2);
                Vector3 normalized = vector3_3.normalized;
                float num2 = Vector3.Dot((vector3_2 - vector3_1), normalized);
                float num3 = (float) (1.0 / (1.0 + (double) num1 * 0.34999999403953552));
                float num4 = Mathf.Clamp(num2, 0.0f, 8f) * 0.35f;
                float num5 = Mathf.Clamp((float) (((double) vector3_1.magnitude + (double) vector3_2.magnitude) * 0.5), 0.0f, 10f) * 0.08f;
                Vector3 vector3_4 = ((position1 + position2) * 0.5f);
                int num6 = 0;
                for (int index3 = 0; index3 < list.Count; ++index3)
                {
                  if ((index3 == index1 ? 1 : (index3 == index2 ? 1 : 0)) == 0)
                  {
                    GorillaData gorillaData = list[index3];
                    if ((gorillaData == null ? 1 : (((UnityEngine.Object) gorillaData.BodyTransform == (UnityEngine.Object) null) ? 1 : 0)) == 0 && (double) Vector3.Distance(gorillaData.BodyTransform.position, vector3_4) < 6.0)
                      ++num6;
                  }
                }
                float num7 = (float) ((double) num3 * 2.2000000476837158 + (double) num4 + (double) num5 + (double) num6 * 0.11999999731779099);
                string key = g1.UserId;
                string str = g2.UserId;
                if (!flag)
                {
                  if ((g1.UserId == Director._primaryId ? 1 : (g2.UserId == Director._primaryId ? 1 : 0)) == 0)
                  {
                    if ((double) vector3_2.sqrMagnitude > (double) vector3_1.sqrMagnitude)
                    {
                      key = g2.UserId;
                      str = g1.UserId;
                    }
                  }
                  else
                  {
                    key = Director._primaryId;
                    str = g1.UserId == Director._primaryId ? g2.UserId : g1.UserId;
                  }
                }
                if (((Director._primaryId == null || !(g1.UserId == Director._primaryId) && !(g2.UserId == Director._primaryId) ? 0 : (Director._secondaryId == null || g1.UserId == Director._secondaryId ? 1 : (g2.UserId == Director._secondaryId ? 1 : 0))) == 0 ? 0 : ((double) num7 > (double) currentLive ? 1 : 0)) != 0)
                  currentLive = num7;
                float num8 = num7;
                float num9;
                if ((!Director.RecentPrimaries.TryGetValue(key, out num9) ? 0 : ((double) Time.time - (double) num9 < 8.0 ? 1 : 0)) != 0)
                  num8 *= 0.55f;
                if ((double) num8 > (double) pick.Score)
                {
                  pick.PrimaryId = key;
                  pick.SecondaryId = str;
                  pick.Score = num8;
                }
              }
            }
          }
        }
      }
    }
    if (pick.PrimaryId == null)
    {
      for (int index = 0; index < list.Count; ++index)
      {
        if (Director.IsUsable(list[index], userId))
        {
          pick.PrimaryId = list[index].UserId;
          pick.Score = 0.25f;
          break;
        }
      }
    }
    if (pick.PrimaryId == null)
    {
      for (int index = 0; index < list.Count; ++index)
      {
        if (Director.IsUsable(list[index], (string) null))
        {
          pick.PrimaryId = list[index].UserId;
          pick.Score = 0.25f;
          break;
        }
      }
    }
    return pick;
  }

  private static bool IsUsable(GorillaData g, string localId)
  {
    return (g == null || ((UnityEngine.Object) g.BodyTransform == (UnityEngine.Object) null) ? 1 : (string.IsNullOrEmpty(g.UserId) ? 1 : 0)) == 0 && (Director.IncludeLocal || localId == null ? 0 : (g.UserId == localId ? 1 : 0)) == 0;
  }

  private static int CountValid(List<GorillaData> list)
  {
    int num = 0;
    for (int index = 0; index < list.Count; ++index)
    {
      if ((list[index] == null ? 0 : (((UnityEngine.Object) list[index].BodyTransform != (UnityEngine.Object) null) ? 1 : 0)) != 0)
        ++num;
    }
    return num;
  }

  private static void BeginShot(
    Director.ShotType type,
    string primaryId,
    string secondaryId,
    List<GorillaData> list)
  {
    if ((Director._primaryId == null ? 0 : (Director._primaryId != primaryId ? 1 : 0)) != 0)
      Director.RecentPrimaries[Director._primaryId] = Time.time;
    Director._shot = type;
    Director._primaryId = primaryId;
    Director._secondaryId = secondaryId;
    Director._shotStartTime = Time.time;
    Director._forceCut = false;
    Director._sideSign = -Director._sideSign;
    Director._wallDist = -1f;
    Director._nextEvalTime = Time.time + Mathf.Max(0.7f, 1.2f);
    GorillaData gorillaData1 = Director.Find(list, primaryId);
    GorillaData gorillaData2 = Director.Find(list, secondaryId);
    if (gorillaData1 != null)
    {
      Vector3 vector3_1 = Director.VelocityOf(primaryId);
      if ((double) vector3_1.sqrMagnitude < 1.0)
        vector3_1 = gorillaData2 != null ? (gorillaData1.BodyTransform.position - gorillaData2.BodyTransform.position) : gorillaData1.BodyTransform.forward;
      Vector3 vector3_2 = Vector3.ProjectOnPlane(vector3_1, Vector3.up);
      if ((double) vector3_2.sqrMagnitude > 0.0099999997764825821)
        Director._shotFlow = vector3_2.normalized;
      Vector3 vector3_3 = (Director._camPos - gorillaData1.BodyTransform.position);
      Director._orbitYaw = Mathf.Atan2(vector3_3.x, vector3_3.z) * 57.29578f;
    }
    Vector3 ideal;
    if (Director.ComputeShotPose(list, out ideal, out Vector3 _) && (type == Director.ShotType.TagBeat ? 1 : ((double) Vector3.Distance(Director._camPos, ideal) > 10.0 ? 1 : 0)) != 0)
      Director._snapNext = true;
    string str1 = gorillaData1 == null || string.IsNullOrEmpty(gorillaData1.UserName) ? "crowd" : gorillaData1.UserName;
    string str2;
    switch (type)
    {
      case Director.ShotType.Chase:
        str2 = "chase " + str1;
        break;
      case Director.ShotType.Lead:
        str2 = "lead " + str1;
        break;
      case Director.ShotType.Orbit:
        str2 = "orbit " + str1;
        break;
      case Director.ShotType.Wide:
        str2 = "wide";
        break;
      default:
        str2 = "tag! " + str1;
        break;
    }
    Director.CurrentShotLabel = str2;
  }

  private static void ComposeAndApply(List<GorillaData> list)
  {
    float deltaTime = Time.deltaTime;
    GorillaData gorillaData = Director.Find(list, Director._primaryId);
    if ((Director._shot == Director.ShotType.Chase || Director._shot == Director.ShotType.Lead ? (gorillaData != null ? 1 : 0) : 0) != 0)
    {
      Vector3 vector3_1 = Vector3.ProjectOnPlane(Director.VelocityOf(Director._primaryId), Vector3.up);
      if ((double) vector3_1.magnitude > 1.2000000476837158)
      {
        Vector3 vector3_2 = Vector3.RotateTowards(Director._shotFlow, vector3_1.normalized, 0.87266463f * deltaTime, 0.0f);
        Director._shotFlow = vector3_2.normalized;
      }
    }
    if (Director._shot == Director.ShotType.Orbit)
      Director._orbitYaw += 10f * deltaTime;
    if ((Director._shot == Director.ShotType.Wide ? 1 : (gorillaData == null ? 1 : 0)) != 0)
      Director._wideYaw += 2f * deltaTime;
    Vector3 ideal;
    Vector3 focusTarget;
    if (!Director.ComputeShotPose(list, out ideal, out focusTarget))
      return;
    if (Director.WallAvoid)
      ideal = Director.ApplyWallPull(focusTarget, ideal, deltaTime);
    if ((Director._snapNext ? 1 : (!Director._hasPose ? 1 : 0)) != 0)
    {
      Director._camPos = ideal;
      Director._camVel = Vector3.zero;
      Director._focus = focusTarget;
      Director._focusVel = Vector3.zero;
      Vector3 vector3 = (focusTarget - ideal);
      if ((double) vector3.sqrMagnitude > 9.9999997473787516E-05)
        Director._camRot = Quaternion.LookRotation(vector3.normalized);
      Director._snapNext = false;
      Director._hasPose = true;
    }
    else
    {
      Director._focus = Vector3.SmoothDamp(Director._focus, focusTarget, ref Director._focusVel, Mathf.Max(0.1f, Director.PosSmoothTime * 0.55f));
      Vector3 vector3_3 = Director.VelocityOf(Director._primaryId);
      float num1 = Mathf.Max(12f, vector3_3.magnitude * 1.8f);
      Director._camPos = Vector3.SmoothDamp(Director._camPos, ideal, ref Director._camVel, Mathf.Max(0.12f, Director.PosSmoothTime), num1);
      Vector3 vector3_4 = (Director._focus - Director._camPos);
      if ((double) vector3_4.sqrMagnitude > 9.9999997473787516E-05)
      {
        Quaternion quaternion = Quaternion.LookRotation(vector3_4.normalized);
        float num2 = Mathf.Min((float) (8.0 + (double) Quaternion.Angle(Director._camRot, quaternion) * 3.2000000476837158), Director.MaxPanSpeed);
        Director._camRot = Quaternion.RotateTowards(Director._camRot, quaternion, num2 * deltaTime);
      }
    }
    Transform transform = ((Component) Plugin.Ins.camera).transform;
    transform.position = Director._camPos;
    transform.rotation = Director._camRot;
  }

  private static bool ComputeShotPose(
    List<GorillaData> list,
    out Vector3 ideal,
    out Vector3 focusTarget)
  {
    GorillaData g1 = Director.Find(list, Director._primaryId);
    GorillaData g2 = Director.Find(list, Director._secondaryId);
    bool shotPose;
    if ((Director._shot == Director.ShotType.Wide ? 1 : (g1 == null ? 1 : 0)) != 0)
    {
      shotPose = Director.ComputeWide(list, out ideal, out focusTarget);
    }
    else
    {
      Vector3 position = g1.BodyTransform.position;
      Vector3 vector3_1 = Director.VelocityOf(Director._primaryId);
      Vector3 vector3_2 = Director.HeadPos(g1);
      Vector3 vector3_3 = (Vector3.Cross(Vector3.up, Director._shotFlow) * Director._sideSign);
      switch (Director._shot)
      {
        case Director.ShotType.Lead:
          ideal = (((position + (Director._shotFlow * Director.Distance * 1.15f)) + (vector3_3 * Director.Distance * 0.35f)) + (Vector3.up * Director.Height * 0.75f));
          focusTarget = g2 != null ? Vector3.Lerp(vector3_2, Director.HeadPos(g2), 0.3f) : vector3_2;
          break;
        case Director.ShotType.Orbit:
          Vector3 vector3_4 = (Quaternion.Euler(0.0f, Director._orbitYaw, 0.0f) * Vector3.forward);
          ideal = ((position + (vector3_4 * Director.Distance * 1.15f)) + (Vector3.up * Director.Height));
          focusTarget = vector3_2;
          break;
        case Director.ShotType.TagBeat:
          Vector3 vector3_5 = g2 != null ? g2.BodyTransform.position : (position + (Director._shotFlow * 1.5f));
          Vector3 vector3_6 = ((position + vector3_5) * 0.5f);
          Vector3 vector3_7 = (vector3_5 - position);
          float magnitude = vector3_7.magnitude;
          Vector3 vector3_8 = (double) magnitude > 0.05000000074505806 ? (Vector3.Cross(Vector3.up, (vector3_7 / magnitude)) * Director._sideSign) : vector3_3;
          ideal = ((vector3_6 + (vector3_8 * (float) ((double) magnitude * 0.89999997615814209 + 2.0))) + (Vector3.up * (Director.Height + 0.4f)));
          focusTarget = (vector3_6 + (Vector3.up * 0.25f));
          break;
        default:
          ideal = (((position - (Director._shotFlow * Director.Distance)) + (vector3_3 * Director.Distance * 0.45f)) + (Vector3.up * Director.Height));
          focusTarget = (vector3_2 + (Vector3.ClampMagnitude(vector3_1, 6f) * 0.2f));
          if (g2 != null)
          {
            focusTarget = Vector3.Lerp(focusTarget, Director.HeadPos(g2), 0.25f);
            break;
          }
          break;
      }
      shotPose = true;
    }
    return shotPose;
  }

  private static bool ComputeWide(
    List<GorillaData> list,
    out Vector3 ideal,
    out Vector3 focusTarget)
  {
    ideal = Vector3.zero;
    focusTarget = Vector3.zero;
    Vector3 vector3_1 = Vector3.zero;
    int num1 = 0;
    for (int index = 0; index < list.Count; ++index)
    {
      GorillaData gorillaData = list[index];
      if ((gorillaData == null ? 1 : (((UnityEngine.Object) gorillaData.BodyTransform == (UnityEngine.Object) null) ? 1 : 0)) == 0)
      {
        vector3_1 = (vector3_1 + gorillaData.BodyTransform.position);
        ++num1;
      }
    }
    bool wide;
    if (num1 != 0)
    {
      Vector3 vector3_2 = (vector3_1 / (float) num1);
      float num2 = 0.0f;
      for (int index = 0; index < list.Count; ++index)
      {
        GorillaData gorillaData = list[index];
        if ((gorillaData == null ? 1 : (((UnityEngine.Object) gorillaData.BodyTransform == (UnityEngine.Object) null) ? 1 : 0)) == 0)
        {
          float num3 = Vector3.Distance(gorillaData.BodyTransform.position, vector3_2);
          if ((double) num3 > (double) num2)
            num2 = num3;
        }
      }
      Director._wideRadius = Mathf.Lerp(Director._wideRadius, num2, 1f - Mathf.Exp(-1.2f * Time.deltaTime));
      Vector3 vector3_3 = (Quaternion.Euler(0.0f, Director._wideYaw, 0.0f) * Vector3.forward);
      ideal = ((vector3_2 + (vector3_3 * Mathf.Max(Director._wideRadius * 1.7f, 6f))) + (Vector3.up * Mathf.Max(Director._wideRadius * 0.9f, 3.5f)));
      focusTarget = vector3_2;
      wide = true;
    }
    else
      wide = false;
    return wide;
  }

  private static Vector3 ApplyWallPull(Vector3 focus, Vector3 ideal, float dt)
  {
    Vector3 vector3_1 = (ideal - focus);
    float magnitude = vector3_1.magnitude;
    Vector3 vector3_2;
    if ((double) magnitude >= 0.60000002384185791)
    {
      Vector3 vector3_3 = (vector3_1 / magnitude);
      float num = magnitude;
      RaycastHit raycastHit = default;
      if ((!Physics.SphereCast((focus + (vector3_3 * 0.45f)), 0.15f, vector3_3, out raycastHit, magnitude - 0.45f, -5, (QueryTriggerInteraction) 1) || !((UnityEngine.Object) raycastHit.collider != (UnityEngine.Object) null) ? 0 : (((UnityEngine.Object) raycastHit.collider.attachedRigidbody == (UnityEngine.Object) null) ? 1 : 0)) != 0)
        num = (float) (0.44999998807907104 + (double) raycastHit.distance - 0.20000000298023224);
      if ((double) Director._wallDist < 0.0)
        Director._wallDist = num;
      Director._wallDist = (double) num < (double) Director._wallDist ? num : Mathf.Min(Director._wallDist + 3f * dt, num);
      Director._wallDist = Mathf.Clamp(Director._wallDist, 0.6f, magnitude);
      vector3_2 = (focus + (vector3_3 * Director._wallDist));
    }
    else
    {
      Director._wallDist = magnitude;
      vector3_2 = ideal;
    }
    return vector3_2;
  }

  private static GorillaData Find(List<GorillaData> list, string id)
  {
    GorillaData gorillaData1;
    if (!string.IsNullOrEmpty(id))
    {
      for (int index = 0; index < list.Count; ++index)
      {
        GorillaData gorillaData2 = list[index];
        if ((gorillaData2 == null || !((UnityEngine.Object) gorillaData2.BodyTransform != (UnityEngine.Object) null) ? 0 : (gorillaData2.UserId == id ? 1 : 0)) != 0)
        {
          gorillaData1 = gorillaData2;
          goto label_8;
        }
      }
      gorillaData1 = (GorillaData) null;
    }
    else
      gorillaData1 = (GorillaData) null;
label_8:
    return gorillaData1;
  }

  private static Vector3 VelocityOf(string id)
  {
    Director.Motion motion;
    return id == null || !Director.Tracked.TryGetValue(id, out motion) ? Vector3.zero : motion.Velocity;
  }

  private static Vector3 HeadPos(GorillaData g)
  {
    return ((UnityEngine.Object) g.HeadTransform != (UnityEngine.Object) null) ? g.HeadTransform.position : (g.BodyTransform.position + (Vector3.up * 0.3f));
  }

  private enum ShotType
  {
    Chase,
    Lead,
    Orbit,
    Wide,
    TagBeat,
  }

  private class Motion
  {
    public Vector3 SamplePos;
    public float SampleTime;
    public Vector3 Velocity;
  }

  private struct Pick
  {
    public string PrimaryId;
    public string SecondaryId;
    public float Score;
  }
}
