using System;
using System.Runtime.InteropServices;

#nullable disable
namespace SakuraaCastingMod.Shared.Integrations.DiscordSDK;

public class ImageManager
{
  private readonly IntPtr MethodsPtr;
  private object MethodsStructure;

  internal ImageManager(IntPtr ptr, IntPtr eventsPtr, ref ImageManager.FFIEvents events)
  {
    if (eventsPtr == IntPtr.Zero)
      throw new ResultException(Result.InternalError);
    this.InitEvents(eventsPtr, ref events);
    this.MethodsPtr = ptr;
    if (this.MethodsPtr == IntPtr.Zero)
      throw new ResultException(Result.InternalError);
  }

  private ImageManager.FFIMethods Methods
  {
    get
    {
      if (this.MethodsStructure == null)
        this.MethodsStructure = Marshal.PtrToStructure(this.MethodsPtr, typeof (ImageManager.FFIMethods));
      return (ImageManager.FFIMethods) this.MethodsStructure;
    }
  }

  private void InitEvents(IntPtr eventsPtr, ref ImageManager.FFIEvents events)
  {
    Marshal.StructureToPtr<ImageManager.FFIEvents>(events, eventsPtr, false);
  }

  [MonoPInvokeCallback]
  private static void FetchCallbackImpl(IntPtr ptr, Result result, ImageHandle handleResult)
  {
    GCHandle gcHandle = GCHandle.FromIntPtr(ptr);
    ImageManager.FetchHandler target = (ImageManager.FetchHandler) gcHandle.Target;
    gcHandle.Free();
    target(result, handleResult);
  }

  public void Fetch(ImageHandle handle, bool refresh, ImageManager.FetchHandler callback)
  {
    GCHandle gcHandle = GCHandle.Alloc((object) callback);
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    this.Methods.Fetch(this.MethodsPtr, handle, refresh, GCHandle.ToIntPtr(gcHandle), new ImageManager.FFIMethods.FetchCallback(ImageManager.FetchCallbackImpl));
  }

  public ImageDimensions GetDimensions(ImageHandle handle)
  {
    ImageDimensions dimensions = new ImageDimensions();
    Result result = this.Methods.GetDimensions(this.MethodsPtr, handle, ref dimensions);
    if (result != 0)
      throw new ResultException(result);
    return dimensions;
  }

  public void GetData(ImageHandle handle, byte[] data)
  {
    Result result = this.Methods.GetData(this.MethodsPtr, handle, data, data.Length);
    if (result != 0)
      throw new ResultException(result);
  }

  public void Fetch(ImageHandle handle, ImageManager.FetchHandler callback)
  {
    this.Fetch(handle, false, callback);
  }

  public byte[] GetData(ImageHandle handle)
  {
    ImageDimensions dimensions = this.GetDimensions(handle);
    byte[] data = new byte[(int) dimensions.Width * (int) dimensions.Height * 4];
    this.GetData(handle, data);
    return data;
  }

  public delegate void FetchHandler(Result result, ImageHandle handleResult);

  [StructLayout(LayoutKind.Sequential, Size = 1)]
  internal struct FFIEvents
  {
  }

  internal struct FFIMethods
  {
    internal ImageManager.FFIMethods.FetchMethod Fetch;
    internal ImageManager.FFIMethods.GetDimensionsMethod GetDimensions;
    internal ImageManager.FFIMethods.GetDataMethod GetData;

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate void FetchCallback(IntPtr ptr, Result result, ImageHandle handleResult);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate void FetchMethod(
      IntPtr methodsPtr,
      ImageHandle handle,
      bool refresh,
      IntPtr callbackData,
      ImageManager.FFIMethods.FetchCallback callback);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate Result GetDimensionsMethod(
      IntPtr methodsPtr,
      ImageHandle handle,
      ref ImageDimensions dimensions);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate Result GetDataMethod(
      IntPtr methodsPtr,
      ImageHandle handle,
      byte[] data,
      int dataLen);
  }
}
