#nullable disable
namespace SakuraaCastingMod.Features.Replay.EditJson;

public class CameraKeyFrame : IKeyFrame
{
  public string ID { get; set; }

  public float Time { get; set; }

  public float PosX { get; set; }

  public float PosY { get; set; }

  public float PosZ { get; set; }

  public float RotX { get; set; }

  public float RotY { get; set; }

  public float RotZ { get; set; }

  public int CameraMovmentType { get; set; }

  public int CameraRotationType { get; set; }

  public int TargetPlayerID { get; set; }

  public float FOV { get; set; }

  public float NearClip { get; set; }

  public float FarClip { get; set; }

  public CameraMoventCurveSettings cameraMoventCurveSettings { get; set; }

  public CameraSmoothingSettings cameraSmoothingSettings { get; set; }

  public FirstPersonSettings firstPersonSettings { get; set; }
}
