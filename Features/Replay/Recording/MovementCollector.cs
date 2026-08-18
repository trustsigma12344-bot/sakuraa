using System;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Features.Replay.Recording;

public static class MovementCollector
{
  public static void Capture(SakuraaCastingMod.Features.Replay.Recording.Replay replay, VRRig vrrig, NetPlayer player)
  {
    if ((((UnityEngine.Object) vrrig == (UnityEngine.Object) null) ? 1 : (player == null ? 1 : 0)) != 0)
      return;
    MovementData movementData = (MovementData) null;
    foreach (PlayerData playerData in replay.playerDatas)
    {
      if (playerData.actorNumber == player.ActorNumber)
        movementData = playerData.movementData;
    }
    if (movementData == null)
      return;
    GorillaIK componentInChildren = ((Component) ((Component) vrrig).transform).gameObject.GetComponentInChildren<GorillaIK>();
    if ((((UnityEngine.Object) componentInChildren == (UnityEngine.Object) null) || ((UnityEngine.Object) componentInChildren.headBone == (UnityEngine.Object) null) || ((UnityEngine.Object) componentInChildren.rightHand == (UnityEngine.Object) null) ? 1 : (((UnityEngine.Object) componentInChildren.leftHand == (UnityEngine.Object) null) ? 1 : 0)) != 0 || (((UnityEngine.Object) vrrig.leftHandTransform == (UnityEngine.Object) null) ? 1 : (((UnityEngine.Object) vrrig.rightHandTransform == (UnityEngine.Object) null) ? 1 : 0)) != 0)
      return;
    Quaternion quaternion1 = Quaternion.AngleAxis(-90f, Vector3.right);
    movementData.headPositions.Add((float) Math.Truncate(100.0 * -(double) componentInChildren.headBone.position.x) / 100f);
    movementData.headPositions.Add((float) Math.Truncate(100.0 * (double) componentInChildren.headBone.position.y) / 100f);
    movementData.headPositions.Add((float) Math.Truncate(100.0 * (double) componentInChildren.headBone.position.z) / 100f);
    movementData.rightHandPositions.Add((float) Math.Truncate(100.0 * -(double) componentInChildren.rightHand.position.x) / 100f);
    movementData.rightHandPositions.Add((float) Math.Truncate(100.0 * (double) componentInChildren.rightHand.position.y) / 100f);
    movementData.rightHandPositions.Add((float) Math.Truncate(100.0 * (double) componentInChildren.rightHand.position.z) / 100f);
    movementData.leftHandPositions.Add((float) Math.Truncate(100.0 * -(double) componentInChildren.leftHand.position.x) / 100f);
    movementData.leftHandPositions.Add((float) Math.Truncate(100.0 * (double) componentInChildren.leftHand.position.y) / 100f);
    movementData.leftHandPositions.Add((float) Math.Truncate(100.0 * (double) componentInChildren.leftHand.position.z) / 100f);
    Quaternion rotation = componentInChildren.headBone.rotation;
    Vector3 eulerAngles = rotation.eulerAngles;
    eulerAngles.y = -eulerAngles.y;
    Quaternion quaternion2 = (Quaternion.Euler(eulerAngles) * quaternion1);
    movementData.headRot.Add((float) Math.Truncate(10000000.0 * (double) quaternion2.x) / 1E+07f);
    movementData.headRot.Add((float) Math.Truncate(10000000.0 * (double) quaternion2.y) / 1E+07f);
    movementData.headRot.Add((float) Math.Truncate(10000000.0 * (double) quaternion2.z) / 1E+07f);
    movementData.headRot.Add((float) Math.Truncate(10000000.0 * (double) quaternion2.w) / 1E+07f);
    movementData.leftHandRot.Add((float) Math.Truncate(10000000.0 * (double) vrrig.leftHandTransform.rotation.x) / 1E+07f);
    movementData.leftHandRot.Add((float) Math.Truncate(10000000.0 * (double) vrrig.leftHandTransform.rotation.y) / 1E+07f);
    movementData.leftHandRot.Add((float) Math.Truncate(10000000.0 * (double) vrrig.leftHandTransform.rotation.z) / 1E+07f);
    movementData.leftHandRot.Add((float) Math.Truncate(10000000.0 * (double) vrrig.leftHandTransform.rotation.w) / 1E+07f);
    movementData.rightHandRot.Add((float) Math.Truncate(10000000.0 * (double) vrrig.rightHandTransform.rotation.x) / 1E+07f);
    movementData.rightHandRot.Add((float) Math.Truncate(10000000.0 * (double) vrrig.rightHandTransform.rotation.y) / 1E+07f);
    movementData.rightHandRot.Add((float) Math.Truncate(10000000.0 * (double) vrrig.rightHandTransform.rotation.z) / 1E+07f);
    movementData.rightHandRot.Add((float) Math.Truncate(10000000.0 * (double) vrrig.rightHandTransform.rotation.w) / 1E+07f);
  }
}
