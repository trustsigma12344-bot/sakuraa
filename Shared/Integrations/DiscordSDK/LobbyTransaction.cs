using System;
using System.Runtime.InteropServices;

#nullable disable
namespace SakuraaCastingMod.Shared.Integrations.DiscordSDK;

public struct LobbyTransaction
{
  internal IntPtr MethodsPtr;
  internal object MethodsStructure;

  private LobbyTransaction.FFIMethods Methods
  {
    get
    {
      if (this.MethodsStructure == null)
        this.MethodsStructure = Marshal.PtrToStructure(this.MethodsPtr, typeof (LobbyTransaction.FFIMethods));
      return (LobbyTransaction.FFIMethods) this.MethodsStructure;
    }
  }

  public void SetType(LobbyType type)
  {
    if (!(this.MethodsPtr != IntPtr.Zero))
      return;
    Result result = this.Methods.SetType(this.MethodsPtr, type);
    if (result != 0)
      throw new ResultException(result);
  }

  public void SetOwner(long ownerId)
  {
    if (!(this.MethodsPtr != IntPtr.Zero))
      return;
    Result result = this.Methods.SetOwner(this.MethodsPtr, ownerId);
    if (result != 0)
      throw new ResultException(result);
  }

  public void SetCapacity(uint capacity)
  {
    if (!(this.MethodsPtr != IntPtr.Zero))
      return;
    Result result = this.Methods.SetCapacity(this.MethodsPtr, capacity);
    if (result != 0)
      throw new ResultException(result);
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

  public void SetLocked(bool locked)
  {
    if (!(this.MethodsPtr != IntPtr.Zero))
      return;
    Result result = this.Methods.SetLocked(this.MethodsPtr, locked);
    if (result != 0)
      throw new ResultException(result);
  }

  internal struct FFIMethods
  {
    internal LobbyTransaction.FFIMethods.SetTypeMethod SetType;
    internal LobbyTransaction.FFIMethods.SetOwnerMethod SetOwner;
    internal LobbyTransaction.FFIMethods.SetCapacityMethod SetCapacity;
    internal LobbyTransaction.FFIMethods.SetMetadataMethod SetMetadata;
    internal LobbyTransaction.FFIMethods.DeleteMetadataMethod DeleteMetadata;
    internal LobbyTransaction.FFIMethods.SetLockedMethod SetLocked;

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate Result SetTypeMethod(IntPtr methodsPtr, LobbyType type);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate Result SetOwnerMethod(IntPtr methodsPtr, long ownerId);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate Result SetCapacityMethod(IntPtr methodsPtr, uint capacity);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate Result SetMetadataMethod(IntPtr methodsPtr, [MarshalAs(UnmanagedType.LPStr)] string key, [MarshalAs(UnmanagedType.LPStr)] string value);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate Result DeleteMetadataMethod(IntPtr methodsPtr, [MarshalAs(UnmanagedType.LPStr)] string key);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate Result SetLockedMethod(IntPtr methodsPtr, bool locked);
  }
}
