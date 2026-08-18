using Newtonsoft.Json;
using SakuraaCastingMod.Features.Replay.EditJson;

#nullable disable
namespace SakuraaCastingMod.Features.Replay.utils;

public class CameraKeyFrameUtils
{
  public static bool AreEqual(CameraKeyFrame key1, CameraKeyFrame key2)
  {
    return (!(key1.ID == key2.ID) || (double) key1.PosX != (double) key2.PosX || (double) key1.PosY != (double) key2.PosY || (double) key1.PosZ != (double) key2.PosZ || (double) key1.RotX != (double) key2.RotX || (double) key1.RotY != (double) key2.RotY || (double) key1.RotZ != (double) key2.RotZ || key1.cameraMoventCurveSettings.UseMainCurve != key2.cameraMoventCurveSettings.UseMainCurve || key1.cameraMoventCurveSettings.UseMainCurveY != key2.cameraMoventCurveSettings.UseMainCurveY || (double) key1.Time != (double) key2.Time || key1.CameraMovmentType != key2.CameraMovmentType ? 0 : (key1.CameraRotationType == key2.CameraRotationType ? 1 : 0)) != 0;
  }

  public static CameraKeyFrame MakeNewCopy(CameraKeyFrame cameraKey)
  {
    return JsonConvert.DeserializeObject<CameraKeyFrame>(JsonConvert.SerializeObject((object) cameraKey));
  }
}
