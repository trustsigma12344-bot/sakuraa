using GorillaRigUtil;
using SakuraaCastingMod.Features.Replay.ReplayJson;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Features.Replay;

public class Gorilla
{
  private string _ID;
  public float StartTime;
  public float EndTime;
  private List<Vector3> PositionsListRighthand;
  private List<Vector3> PositionsListLefthand;
  private List<Vector3> PositionsListHead;
  private List<Vector3> RotationsListRightHand;
  private List<Vector3> RotationsListLeftHand;
  private List<Vector3> RotationsListHead;
  private List<int> TaggedList;
  private float BaseHeadDiff = 0.3973999f;
  public CustomRig gorillaRig;
  public Color gorillaColor;
  private bool LastTagState = false;
  private List<SakuraaCastingMod.Features.Replay.ReplayJson.CosmeticsData> CosmeticsData;
  private float LastChangeTime;
  private List<CosmeticItem> activlyActiveCosmetics = new List<CosmeticItem>();
  private float LittleTestCounter = 0.0f;

  public string ID => this._ID;

  public bool IsTagged => this.LastTagState;

  public string userName { get; private set; }

  public int actorNumber { get; private set; }

  public float offsetStartTime { get; private set; }

  public Gorilla(
    string ID,
    float StartTime,
    float EndTime,
    List<Vector3> LRPos,
    List<Vector3> LLPos,
    List<Vector3> LHPos,
    List<Vector3> LRRot,
    List<Vector3> LLRot,
    List<Vector3> LHRot,
    List<int> TaggedList,
    List<float> Color,
    List<SakuraaCastingMod.Features.Replay.ReplayJson.CosmeticsData> cosmeticsDatas,
    float offsetStartTime,
    int actorNumber,
    string userName = "gorilla")
  {
    this.userName = userName;
    this.actorNumber = actorNumber;
    this.gorillaColor = new Color(Color[0], Color[1], Color[2]);
    this._ID = ID;
    this.StartTime = StartTime;
    this.EndTime = EndTime;
    this.PositionsListRighthand = LRPos;
    this.PositionsListLefthand = LLPos;
    this.PositionsListHead = LHPos;
    this.RotationsListRightHand = LRRot;
    this.RotationsListLeftHand = LLRot;
    this.RotationsListHead = LHRot;
    this.CosmeticsData = cosmeticsDatas;
    this.TaggedList = TaggedList;
    this.offsetStartTime = offsetStartTime;
  }

  public static string[] CustomSplit(string str, string delimiter)
  {
    string[] strArray;
    if ((string.IsNullOrEmpty(str) ? 1 : (string.IsNullOrEmpty(delimiter) ? 1 : 0)) == 0)
    {
      List<string> stringList = new List<string>();
      int startIndex;
      int num;
      for (startIndex = 0; (num = str.IndexOf(delimiter, startIndex)) != -1; startIndex = num + delimiter.Length)
        stringList.Add(str.Substring(startIndex, num - startIndex));
      stringList.Add(str.Substring(startIndex));
      strArray = stringList.ToArray();
    }
    else
      strArray = new string[1]{ str };
    return strArray;
  }

  private void MakeGorilla()
  {
    this.gorillaRig = new CustomRig(this.ID, this.userName, this.gorillaColor);
    this.UpdateIKLenForLemming();
    this.gorillaRig.SetColor(this.gorillaColor);
  }

  private void UpdateGorillaRightHandPos(Vector3 ChangeTo)
  {
    this.gorillaRig.RightHand.transform.position = ChangeTo;
  }

  private void UpdateGorillaLeftHandPos(Vector3 ChangeTo)
  {
    this.gorillaRig.LeftHand.transform.position = ChangeTo;
  }

  private void UpdateGorillaHeadPos(Vector3 ChangeTo)
  {
    this.gorillaRig.WorldObject.transform.position = new Vector3(ChangeTo.x, ChangeTo.y, ChangeTo.z);
    this.gorillaRig.Head.transform.position = ChangeTo;
  }

  private void UpdateGorillaRightHandRot(Quaternion ChangeTo)
  {
    this.gorillaRig.RightHand.transform.rotation = ChangeTo;
  }

  private void UpdateGorillaLeftHandRot(Quaternion ChangeTo)
  {
    this.gorillaRig.LeftHand.transform.rotation = ChangeTo;
  }

  private void UpdateGorillaHeadRot(Quaternion ChangeTo)
  {
    this.gorillaRig.Head.transform.rotation = ChangeTo;
  }

  private void UpdateGorillaBody(Vector3 Rotation)
  {
    this.gorillaRig.WorldObject.transform.rotation = Quaternion.Euler(new Vector3(0.0f, Rotation.y, 0.0f));
  }

  private Vector3 FindInterpolatedPos(Vector3 CurrentPos, Vector3 NextPos, float PercentDone)
  {
    Vector3 vector3 = new Vector3();
    return Vector3.Lerp(CurrentPos, NextPos, PercentDone);
  }

  private Vector3 CustomSlerpXYZ(Vector3 startRotation, Vector3 endRotation, float t)
  {
    for (int index = 0; index < 3; ++index)
    {
      if ((double) endRotation[index] - (double) startRotation[index] <= 180.0)
      {
        if ((double) endRotation[index] - (double) startRotation[index] < -180.0)
        {
          ref Vector3 local = ref endRotation;
          int num = index;
          local[num] = local[num] + 360f;
        }
      }
      else
      {
        ref Vector3 local = ref endRotation;
        int num = index;
        local[num] = local[num] - 360f;
      }
    }
    Vector3 vector3;
    // ISSUE: explicit constructor call
    vector3 = new Vector3(Mathf.Lerp(startRotation.x, endRotation.x, t), Mathf.Lerp(startRotation.y, endRotation.y, t), Mathf.Lerp(startRotation.z, endRotation.z, t));
    return vector3;
  }

  private Quaternion FindInterpolatedRot(Vector3 CurrentPos, Vector3 NextPos, float PercentDone)
  {
    return Quaternion.Euler(this.CustomSlerpXYZ(CurrentPos, NextPos, PercentDone));
  }

  private Vector3 FindInterpolatedBody(Vector3 CurrentPos, Vector3 NextPos, float PercentDone)
  {
    Vector3 vector3 = new Vector3();
    return this.CustomSlerpXYZ(CurrentPos, NextPos, PercentDone);
  }

  private void UpdatePosRot(float Rtime)
  {
    try
    {
      float num = Rtime / 0.05f;
      int index1 = (int) num;
      int index2 = index1 + 1;
      float PercentDone = num - (float) index1;
      Vector3 CurrentPos1 = this.PositionsListRighthand[index1];
      Vector3 NextPos1 = this.PositionsListRighthand[index2];
      Vector3 CurrentPos2 = this.PositionsListLefthand[index1];
      Vector3 NextPos2 = this.PositionsListLefthand[index2];
      Vector3 CurrentPos3 = this.PositionsListHead[index1];
      Vector3 NextPos3 = this.PositionsListHead[index2];
      Vector3 CurrentPos4 = this.RotationsListRightHand[index1];
      Vector3 NextPos4 = this.RotationsListRightHand[index2];
      Vector3 CurrentPos5 = this.RotationsListLeftHand[index1];
      Vector3 NextPos5 = this.RotationsListLeftHand[index2];
      Vector3 CurrentPos6 = this.RotationsListHead[index1];
      Vector3 NextPos6 = this.RotationsListHead[index2];
      this.UpdateGorillaBody(this.FindInterpolatedBody(CurrentPos6, NextPos6, PercentDone));
      this.UpdateGorillaRightHandPos(this.FindInterpolatedPos(CurrentPos1, NextPos1, PercentDone));
      this.UpdateGorillaLeftHandPos(this.FindInterpolatedPos(CurrentPos2, NextPos2, PercentDone));
      this.UpdateGorillaHeadPos(this.FindInterpolatedPos(CurrentPos3, NextPos3, PercentDone));
      this.UpdateGorillaRightHandRot(this.FindInterpolatedRot(CurrentPos4, NextPos4, PercentDone));
      this.UpdateGorillaLeftHandRot(this.FindInterpolatedRot(CurrentPos5, NextPos5, PercentDone));
      this.UpdateGorillaHeadRot(this.FindInterpolatedRot(CurrentPos6, NextPos6, PercentDone));
    }
    catch
    {
    }
  }

  private void UpdateTaggedState(float Gtime)
  {
    int num1 = 0;
    int num2 = 0;
    for (int index = 0; index < this.TaggedList.Count; index += 2)
    {
      if ((this.TaggedList[index] <= num1 ? 0 : ((double) (this.TaggedList[index] / 100) < (double) Gtime ? 1 : 0)) != 0)
      {
        num1 = this.TaggedList[index];
        num2 = index;
      }
    }
    bool flag = false;
    if (this.TaggedList[num2 + 1] == 1)
      flag = true;
    if (flag == this.LastTagState)
      return;
    this.LastTagState = flag;
    if (!flag)
    {
      this.gorillaRig.SetMaterial(0);
      this.gorillaRig.SetColor(this.gorillaColor);
    }
    else
      this.gorillaRig.SetMaterial(2);
  }

  private void UpdateCosmeticsState(float GTime)
  {
    ++this.LittleTestCounter;
    int num = 0;
    int index1 = 0;
    for (int index2 = 0; index2 < this.CosmeticsData.Count; ++index2)
    {
      if ((this.CosmeticsData[index2].time <= num ? 0 : ((double) (this.CosmeticsData[index2].time / 100) < (double) GTime ? 1 : 0)) != 0)
      {
        num = this.CosmeticsData[index2].time;
        index1 = index2;
      }
    }
    if ((double) this.LastChangeTime == (double) num)
      return;
    this.LastChangeTime = (float) num;
    foreach (CosmeticItem activlyActiveCosmetic in this.activlyActiveCosmetics)
    {
      if (activlyActiveCosmetic != null)
      {
        activlyActiveCosmetic.Disable();
        if (activlyActiveCosmetic.isHoldable)
          activlyActiveCosmetic.AttachToPosition((TransferrableObject.PositionState) 0);
      }
    }
    this.activlyActiveCosmetics.Clear();
    foreach (CosmeticData cosmeticsData in this.CosmeticsData[index1].cosmeticsDatas)
    {
      if (this.gorillaRig.CosmeticIDToCosmeticItem.ContainsKey(cosmeticsData.displayName))
      {
        CosmeticItem cosmeticItem = this.gorillaRig.CosmeticIDToCosmeticItem[cosmeticsData.displayName];
        if (cosmeticItem != null)
        {
          this.activlyActiveCosmetics.Add(cosmeticItem);
          cosmeticItem.Enable((CosmeticLocation) 3);
          if ((!cosmeticsData.holdable ? 1 : (!cosmeticItem.isHoldable ? 1 : 0)) == 0)
            cosmeticItem.AttachToPosition((TransferrableObject.PositionState) cosmeticsData.state);
        }
      }
    }
  }

  public static string GetGameObjectPath(GameObject obj)
  {
    string gameObjectPath = "/" + ((UnityEngine.Object) obj).name;
    while (((UnityEngine.Object) obj.transform.parent != (UnityEngine.Object) null))
    {
      obj = ((Component) obj.transform.parent).gameObject;
      gameObjectPath = $"/{((UnityEngine.Object) obj).name}{gameObjectPath}";
    }
    return gameObjectPath;
  }

  private void UpdateIKLenForLemming()
  {
    Type type = GorillaIKMgr.Instance.GetType();
    List<GorillaIK> gorillaIkList = (List<GorillaIK>) type.GetField("ikList", BindingFlags.Instance | BindingFlags.NonPublic).GetValue((object) GorillaIKMgr.Instance);
    type.GetField("actualListSz", BindingFlags.Instance | BindingFlags.NonPublic).SetValue((object) GorillaIKMgr.Instance, (object) (gorillaIkList.Count * 2));
  }

  public void Update(float GlobalRealTime)
  {
    GlobalRealTime -= this.offsetStartTime;
    if (this.gorillaRig == null)
      this.MakeGorilla();
    if (this.gorillaRig == null)
      return;
    if (((double) GlobalRealTime * 100.0 <= (double) this.EndTime || !this.gorillaRig.isRigActive ? ((double) GlobalRealTime * 100.0 >= (double) this.StartTime ? 0 : (this.gorillaRig.isRigActive ? 1 : 0)) : 1) != 0)
    {
      this.InternalDeactivateSelf();
    }
    else
    {
      if ((this.gorillaRig.isRigActive || (double) GlobalRealTime * 100.0 > (double) this.EndTime ? 0 : ((double) GlobalRealTime * 100.0 > (double) this.StartTime ? 1 : 0)) != 0)
        this.InternalActivateSelf();
      float Rtime = GlobalRealTime - this.StartTime / 100f;
      this.UpdateTaggedState(GlobalRealTime);
      this.UpdateCosmeticsState(GlobalRealTime);
      this.UpdatePosRot(Rtime);
    }
  }

  private void InternalActivateSelf()
  {
    ((Behaviour) this.gorillaRig.myIK).enabled = true;
    this.gorillaRig.ActivateRig();
  }

  private void InternalDeactivateSelf()
  {
    ((Behaviour) this.gorillaRig.myIK).enabled = false;
    this.UpdateIKLenForLemming();
    this.gorillaRig.DeactivateRig();
  }

  public void ActivateSelf() => this.InternalDeactivateSelf();
}
