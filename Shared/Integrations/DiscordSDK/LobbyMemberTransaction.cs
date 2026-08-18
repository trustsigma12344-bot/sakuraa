using System;
using System.Runtime.InteropServices;

#nullable disable
namespace SakuraaCastingMod.Shared.Integrations.DiscordSDK;

public struct LobbyMemberTransaction
{
  internal IntPtr MethodsPtr;
  internal object MethodsStructure;

  private LobbyMemberTransaction.FFIMethods Methods
  {
    get
    {
      if (this.MethodsStructure == null)
        this.MethodsStructure = Marshal.PtrToStructure(this.MethodsPtr, typeof (LobbyMemberTransaction.FFIMethods));
      return (LobbyMemberTransaction.FFIMethods) this.MethodsStructure;
    }
  }

  public void SetMetadata(string key, string value)
  {
    if (!(this.MethodsPtr != IntPtr.Zero))
      return;
    Result result = this.Methods.SetMetadata(this.MethodsPtr, key, value);
    if (result != 0)
      throw new ResultException(result);
  }

  public void DeleteMetadata(string key)
  {
    if (!(this.MethodsPtr != IntPtr.Zero))
      return;
    Result result = this.Methods.DeleteMetadata(this.MethodsPtr, key);
    if (result != 0)
      throw new ResultException(result);
  }

  internal struct FFIMethods
  {
    internal LobbyMemberTransaction.FFIMethods.SetMetadataMethod SetMetadata;
    internal LobbyMemberTransaction.FFIMethods.DeleteMetadataMethod DeleteMetadata;

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate Result SetMetadataMethod(IntPtr methodsPtr, [MarshalAs(UnmanagedType.LPStr)] string key, [MarshalAs(UnmanagedType.LPStr)] string value);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate Result DeleteMetadataMethod(IntPtr methodsPtr, [MarshalAs(UnmanagedType.LPStr)] string key);
  }
}
