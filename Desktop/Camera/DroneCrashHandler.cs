using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Desktop.Camera;

public class DroneCrashHandler : MonoBehaviour
{
  private const float CrashSpeedThreshold = 15f;

  private void OnCollisionEnter(Collision collision)
  {
    if ((!Drone.DroneStarted ? 1 : (Drone.IsCrashed ? 1 : 0)) != 0)
      return;
    Vector3 relativeVelocity = collision.relativeVelocity;
    if ((double) relativeVelocity.magnitude < 15.0)
      return;
    relativeVelocity = collision.relativeVelocity;
    Drone.TriggerCrash(relativeVelocity.magnitude);
  }
}
