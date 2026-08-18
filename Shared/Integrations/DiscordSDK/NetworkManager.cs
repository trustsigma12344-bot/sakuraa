using System;
using System.Runtime.InteropServices;

#nullable disable
namespace SakuraaCastingMod.Shared.Integrations.DiscordSDK;

public class NetworkManager
{
  private readonly IntPtr MethodsPtr;
  private object MethodsStructure;

  internal NetworkManager(IntPtr ptr, IntPtr eventsPtr, ref NetworkManager.FFIEvents events)
  {
    if (eventsPtr == IntPtr.Zero)
      throw new ResultException(Result.InternalError);
    this.InitEvents(eventsPtr, ref events);
    this.MethodsPtr = ptr;
    if (this.MethodsPtr == IntPtr.Zero)
      throw new ResultException(Result.InternalError);
  }

  private NetworkManager.FFIMethods Methods
  {
    get
    {
      if (this.MethodsStructure == null)
        this.MethodsStructure = Marshal.PtrToStructure(this.MethodsPtr, typeof (NetworkManager.FFIMethods));
      return (NetworkManager.FFIMethods) this.MethodsStructure;
    }
  }

  public event NetworkManager.MessageHandler OnMessage;

  public event NetworkManager.RouteUpdateHandler OnRouteUpdate;

  private void InitEvents(IntPtr eventsPtr, ref NetworkManager.FFIEvents events)
  {
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    events.OnMessage = new NetworkManager.FFIEvents.MessageHandler(NetworkManager.OnMessageImpl);
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    events.OnRouteUpdate = new NetworkManager.FFIEvents.RouteUpdateHandler(NetworkManager.OnRouteUpdateImpl);
    Marshal.StructureToPtr<NetworkManager.FFIEvents>(events, eventsPtr, false);
  }

  public ulong GetPeerId()
  {
    ulong peerId = 0;
    this.Methods.GetPeerId(this.MethodsPtr, ref peerId);
    return peerId;
  }

  public void Flush()
  {
    Result result = this.Methods.Flush(this.MethodsPtr);
    if (result != 0)
      throw new ResultException(result);
  }

  public void OpenPeer(ulong peerId, string routeData)
  {
    Result result = this.Methods.OpenPeer(this.MethodsPtr, peerId, routeData);
    if (result != 0)
      throw new ResultException(result);
  }

  public void UpdatePeer(ulong peerId, string routeData)
  {
    Result result = this.Methods.UpdatePeer(this.MethodsPtr, peerId, routeData);
    if (result != 0)
      throw new ResultException(result);
  }

  public void ClosePeer(ulong peerId)
  {
    Result result = this.Methods.ClosePeer(this.MethodsPtr, peerId);
    if (result != 0)
      throw new ResultException(result);
  }

  public void OpenChannel(ulong peerId, byte channelId, bool reliable)
  {
    Result result = this.Methods.OpenChannel(this.MethodsPtr, peerId, channelId, reliable);
    if (result != 0)
      throw new ResultException(result);
  }

  public void CloseChannel(ulong peerId, byte channelId)
  {
    Result result = this.Methods.CloseChannel(this.MethodsPtr, peerId, channelId);
    if (result != 0)
      throw new ResultException(result);
  }

  public void SendMessage(ulong peerId, byte channelId, byte[] data)
  {
    Result result = this.Methods.SendMessage(this.MethodsPtr, peerId, channelId, data, data.Length);
    if (result != 0)
      throw new ResultException(result);
  }

  [MonoPInvokeCallback]
  private static void OnMessageImpl(
    IntPtr ptr,
    ulong peerId,
    byte channelId,
    IntPtr dataPtr,
    int dataLen)
  {
    Discord target = (Discord) GCHandle.FromIntPtr(ptr).Target;
    if (target.NetworkManagerInstance.OnMessage == null)
      return;
    byte[] numArray = new byte[dataLen];
    Marshal.Copy(dataPtr, numArray, 0, dataLen);
    target.NetworkManagerInstance.OnMessage(peerId, channelId, numArray);
  }

  [MonoPInvokeCallback]
  private static void OnRouteUpdateImpl(IntPtr ptr, string routeData)
  {
    Discord target = (Discord) GCHandle.FromIntPtr(ptr).Target;
    if (target.NetworkManagerInstance.OnRouteUpdate == null)
      return;
    target.NetworkManagerInstance.OnRouteUpdate(routeData);
  }

  public delegate void MessageHandler(ulong peerId, byte channelId, byte[] data);

  public delegate void RouteUpdateHandler(string routeData);

  internal struct FFIEvents
  {
    internal NetworkManager.FFIEvents.MessageHandler OnMessage;
    internal NetworkManager.FFIEvents.RouteUpdateHandler OnRouteUpdate;

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate void MessageHandler(
      IntPtr ptr,
      ulong peerId,
      byte channelId,
      IntPtr dataPtr,
      int dataLen);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate void RouteUpdateHandler(IntPtr ptr, [MarshalAs(UnmanagedType.LPStr)] string routeData);
  }

  internal struct FFIMethods
  {
    internal NetworkManager.FFIMethods.GetPeerIdMethod GetPeerId;
    internal NetworkManager.FFIMethods.FlushMethod Flush;
    internal NetworkManager.FFIMethods.OpenPeerMethod OpenPeer;
    internal NetworkManager.FFIMethods.UpdatePeerMethod UpdatePeer;
    internal NetworkManager.FFIMethods.ClosePeerMethod ClosePeer;
    internal NetworkManager.FFIMethods.OpenChannelMethod OpenChannel;
    internal NetworkManager.FFIMethods.CloseChannelMethod CloseChannel;
    internal NetworkManager.FFIMethods.SendMessageMethod SendMessage;

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate void GetPeerIdMethod(IntPtr methodsPtr, ref ulong peerId);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate Result FlushMethod(IntPtr methodsPtr);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate Result OpenPeerMethod(IntPtr methodsPtr, ulong peerId, [MarshalAs(UnmanagedType.LPStr)] string routeData);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate Result UpdatePeerMethod(IntPtr methodsPtr, ulong peerId, [MarshalAs(UnmanagedType.LPStr)] string routeData);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate Result ClosePeerMethod(IntPtr methodsPtr, ulong peerId);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate Result OpenChannelMethod(
      IntPtr methodsPtr,
      ulong peerId,
      byte channelId,
      bool reliable);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate Result CloseChannelMethod(IntPtr methodsPtr, ulong peerId, byte channelId);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate Result SendMessageMethod(
      IntPtr methodsPtr,
      ulong peerId,
      byte channelId,
      byte[] data,
      int dataLen);
  }
}
