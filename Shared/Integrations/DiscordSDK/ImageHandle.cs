#nullable disable
namespace SakuraaCastingMod.Shared.Integrations.DiscordSDK;

public struct ImageHandle
{
  public ImageType Type;
  public long Id;
  public uint Size;

  public static ImageHandle User(long id) => ImageHandle.User(id, 128U /*0x80*/);

  public static ImageHandle User(long id, uint size)
  {
    return new ImageHandle()
    {
      Type = ImageType.User,
      Id = id,
      Size = size
    };
  }
}
