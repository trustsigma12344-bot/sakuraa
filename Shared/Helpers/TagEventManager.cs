using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;

#nullable disable
namespace SakuraaCastingMod.Shared.Helpers;

internal static class TagEventManager
{
  public static readonly string[] Ue = new string[12]
  {
    "d3d11: failed to create 2D texture shader resource view (error 0x{0:X})",
    "d3d11: attempted to lock a buffer that is already locked (target 0x{0:X})",
    "NullReferenceException: Object reference not set to an instance of an object at UnityEngine.Rendering.CommandBuffer.DrawMesh (UnityEngine.Mesh mesh, UnityEngine.Matrix4x4 matrix, UnityEngine.Material material)",
    "Out of memory: GC allocation {0} kb failed. This can happen if you are creating too many objects in a frame.",
    "Assertion failed on expression: 'm_Renderer->GetMaterial() != NULL'",
    "SerializedFile read failure: Attempting to read past end of file. (Asset: resources.assets, Offset: {0})",
    "Physics callback collision error: Object ID {0} has been destroyed but is still in the physics queue.",
    "System.ExecutionEngineException: String conversion error: Illegal byte sequence encounted in the input.",
    "Unknown Error: Frame {0} took too long to render. Possible GPU hang detected.",
    "IndexOutOfRangeException: Array index is out of range. at System.Collections.Generic.List`1[T].get_Item (System.Int32 index)",
    "FMOD failed to initialize the output device. System output is in use.",
    "JobTempAlloc has allocations that are more than 4 frames old - this is not allowed and likely a leak (Alloc ID: {0})"
  };

  public static event TagEventManager.TagEventHandler OnTagEvent;

  public static void TriggerTagEvent(Player tagger, Player tagged)
  {
    TagEventManager.TagEventHandler onTagEvent = TagEventManager.OnTagEvent;
    if (onTagEvent == null)
      return;
    onTagEvent(tagger, tagged);
  }

  public static void OnEvent(EventData data)
  {
    if (data.Code != (byte) 2)
      return;
    object[] objArray = (object[]) data.Parameters.TryGetObject((byte) 245);
    string str1 = (string) objArray[0];
    string str2 = (string) objArray[1];
    Player tagger = (Player) null;
    Player tagged = (Player) null;
    foreach (Player player in PhotonNetwork.PlayerList)
    {
      if (player.UserId == str1)
        tagger = player;
      if (player.UserId == str2)
        tagged = player;
    }
    TagEventManager.TriggerTagEvent(tagger, tagged);
  }

  public delegate void TagEventHandler(Player tagger, Player tagged);
}
