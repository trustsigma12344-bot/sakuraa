using System;
using System.Runtime.InteropServices;

#nullable disable
namespace SakuraaCastingMod.Shared.Integrations.DiscordSDK;

public class ActivityManager
{
  private readonly IntPtr MethodsPtr;
  private object MethodsStructure;

  public void RegisterCommand() => this.RegisterCommand((string) null);

  internal ActivityManager(IntPtr ptr, IntPtr eventsPtr, ref ActivityManager.FFIEvents events)
  {
    if (eventsPtr == IntPtr.Zero)
      throw new ResultException(Result.InternalError);
    this.InitEvents(eventsPtr, ref events);
    this.MethodsPtr = ptr;
    if (this.MethodsPtr == IntPtr.Zero)
      throw new ResultException(Result.InternalError);
  }

  private ActivityManager.FFIMethods Methods
  {
    get
    {
      if (this.MethodsStructure == null)
        this.MethodsStructure = Marshal.PtrToStructure(this.MethodsPtr, typeof (ActivityManager.FFIMethods));
      return (ActivityManager.FFIMethods) this.MethodsStructure;
    }
  }

  public event ActivityManager.ActivityJoinHandler OnActivityJoin;

  public event ActivityManager.ActivitySpectateHandler OnActivitySpectate;

  public event ActivityManager.ActivityJoinRequestHandler OnActivityJoinRequest;

  public event ActivityManager.ActivityInviteHandler OnActivityInvite;

  private void InitEvents(IntPtr eventsPtr, ref ActivityManager.FFIEvents events)
  {
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    events.OnActivityJoin = new ActivityManager.FFIEvents.ActivityJoinHandler(ActivityManager.OnActivityJoinImpl);
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    events.OnActivitySpectate = new ActivityManager.FFIEvents.ActivitySpectateHandler(ActivityManager.OnActivitySpectateImpl);
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    events.OnActivityJoinRequest = new ActivityManager.FFIEvents.ActivityJoinRequestHandler(ActivityManager.OnActivityJoinRequestImpl);
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    events.OnActivityInvite = new ActivityManager.FFIEvents.ActivityInviteHandler(ActivityManager.OnActivityInviteImpl);
    Marshal.StructureToPtr<ActivityManager.FFIEvents>(events, eventsPtr, false);
  }

  public void RegisterCommand(string command)
  {
    Result result = this.Methods.RegisterCommand(this.MethodsPtr, command);
    if (result != 0)
      throw new ResultException(result);
  }

  public void RegisterSteam(uint steamId)
  {
    Result result = this.Methods.RegisterSteam(this.MethodsPtr, steamId);
    if (result != 0)
      throw new ResultException(result);
  }

  [MonoPInvokeCallback]
  private static void UpdateActivityCallbackImpl(IntPtr ptr, Result result)
  {
    GCHandle gcHandle = GCHandle.FromIntPtr(ptr);
    ActivityManager.UpdateActivityHandler target = (ActivityManager.UpdateActivityHandler) gcHandle.Target;
    gcHandle.Free();
    target(result);
  }

  public void UpdateActivity(Activity activity, ActivityManager.UpdateActivityHandler callback)
  {
    GCHandle gcHandle = GCHandle.Alloc((object) callback);
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    this.Methods.UpdateActivity(this.MethodsPtr, ref activity, GCHandle.ToIntPtr(gcHandle), new ActivityManager.FFIMethods.UpdateActivityCallback(ActivityManager.UpdateActivityCallbackImpl));
  }

  [MonoPInvokeCallback]
  private static void ClearActivityCallbackImpl(IntPtr ptr, Result result)
  {
    GCHandle gcHandle = GCHandle.FromIntPtr(ptr);
    ActivityManager.ClearActivityHandler target = (ActivityManager.ClearActivityHandler) gcHandle.Target;
    gcHandle.Free();
    target(result);
  }

  public void ClearActivity(ActivityManager.ClearActivityHandler callback)
  {
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    this.Methods.ClearActivity(this.MethodsPtr, GCHandle.ToIntPtr(GCHandle.Alloc((object) callback)), new ActivityManager.FFIMethods.ClearActivityCallback(ActivityManager.ClearActivityCallbackImpl));
  }

  [MonoPInvokeCallback]
  private static void SendRequestReplyCallbackImpl(IntPtr ptr, Result result)
  {
    GCHandle gcHandle = GCHandle.FromIntPtr(ptr);
    ActivityManager.SendRequestReplyHandler target = (ActivityManager.SendRequestReplyHandler) gcHandle.Target;
    gcHandle.Free();
    target(result);
  }

  public void SendRequestReply(
    long userId,
    ActivityJoinRequestReply reply,
    ActivityManager.SendRequestReplyHandler callback)
  {
    GCHandle gcHandle = GCHandle.Alloc((object) callback);
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    this.Methods.SendRequestReply(this.MethodsPtr, userId, reply, GCHandle.ToIntPtr(gcHandle), new ActivityManager.FFIMethods.SendRequestReplyCallback(ActivityManager.SendRequestReplyCallbackImpl));
  }

  [MonoPInvokeCallback]
  private static void SendInviteCallbackImpl(IntPtr ptr, Result result)
  {
    GCHandle gcHandle = GCHandle.FromIntPtr(ptr);
    ActivityManager.SendInviteHandler target = (ActivityManager.SendInviteHandler) gcHandle.Target;
    gcHandle.Free();
    target(result);
  }

  public void SendInvite(
    long userId,
    ActivityActionType type,
    string content,
    ActivityManager.SendInviteHandler callback)
  {
    GCHandle gcHandle = GCHandle.Alloc((object) callback);
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    this.Methods.SendInvite(this.MethodsPtr, userId, type, content, GCHandle.ToIntPtr(gcHandle), new ActivityManager.FFIMethods.SendInviteCallback(ActivityManager.SendInviteCallbackImpl));
  }

  [MonoPInvokeCallback]
  private static void AcceptInviteCallbackImpl(IntPtr ptr, Result result)
  {
    GCHandle gcHandle = GCHandle.FromIntPtr(ptr);
    ActivityManager.AcceptInviteHandler target = (ActivityManager.AcceptInviteHandler) gcHandle.Target;
    gcHandle.Free();
    target(result);
  }

  public void AcceptInvite(long userId, ActivityManager.AcceptInviteHandler callback)
  {
    GCHandle gcHandle = GCHandle.Alloc((object) callback);
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    this.Methods.AcceptInvite(this.MethodsPtr, userId, GCHandle.ToIntPtr(gcHandle), new ActivityManager.FFIMethods.AcceptInviteCallback(ActivityManager.AcceptInviteCallbackImpl));
  }

  [MonoPInvokeCallback]
  private static void OnActivityJoinImpl(IntPtr ptr, string secret)
  {
    Discord target = (Discord) GCHandle.FromIntPtr(ptr).Target;
    if (target.ActivityManagerInstance.OnActivityJoin == null)
      return;
    target.ActivityManagerInstance.OnActivityJoin(secret);
  }

  [MonoPInvokeCallback]
  private static void OnActivitySpectateImpl(IntPtr ptr, string secret)
  {
    Discord target = (Discord) GCHandle.FromIntPtr(ptr).Target;
    if (target.ActivityManagerInstance.OnActivitySpectate == null)
      return;
    target.ActivityManagerInstance.OnActivitySpectate(secret);
  }

  [MonoPInvokeCallback]
  private static void OnActivityJoinRequestImpl(IntPtr ptr, ref User user)
  {
    Discord target = (Discord) GCHandle.FromIntPtr(ptr).Target;
    if (target.ActivityManagerInstance.OnActivityJoinRequest == null)
      return;
    target.ActivityManagerInstance.OnActivityJoinRequest(ref user);
  }

  [MonoPInvokeCallback]
  private static void OnActivityInviteImpl(
    IntPtr ptr,
    ActivityActionType type,
    ref User user,
    ref Activity activity)
  {
    Discord target = (Discord) GCHandle.FromIntPtr(ptr).Target;
    if (target.ActivityManagerInstance.OnActivityInvite == null)
      return;
    target.ActivityManagerInstance.OnActivityInvite(type, ref user, ref activity);
  }

  public delegate void AcceptInviteHandler(Result result);

  public delegate void ActivityInviteHandler(
    ActivityActionType type,
    ref User user,
    ref Activity activity);

  public delegate void ActivityJoinHandler(string secret);

  public delegate void ActivityJoinRequestHandler(ref User user);

  public delegate void ActivitySpectateHandler(string secret);

  public delegate void ClearActivityHandler(Result result);

  public delegate void SendInviteHandler(Result result);

  public delegate void SendRequestReplyHandler(Result result);

  public delegate void UpdateActivityHandler(Result result);

  internal struct FFIEvents
  {
    internal ActivityManager.FFIEvents.ActivityJoinHandler OnActivityJoin;
    internal ActivityManager.FFIEvents.ActivitySpectateHandler OnActivitySpectate;
    internal ActivityManager.FFIEvents.ActivityJoinRequestHandler OnActivityJoinRequest;
    internal ActivityManager.FFIEvents.ActivityInviteHandler OnActivityInvite;

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate void ActivityJoinHandler(IntPtr ptr, [MarshalAs(UnmanagedType.LPStr)] string secret);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate void ActivitySpectateHandler(IntPtr ptr, [MarshalAs(UnmanagedType.LPStr)] string secret);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate void ActivityJoinRequestHandler(IntPtr ptr, ref User user);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate void ActivityInviteHandler(
      IntPtr ptr,
      ActivityActionType type,
      ref User user,
      ref Activity activity);
  }

  internal struct FFIMethods
  {
    internal ActivityManager.FFIMethods.RegisterCommandMethod RegisterCommand;
    internal ActivityManager.FFIMethods.RegisterSteamMethod RegisterSteam;
    internal ActivityManager.FFIMethods.UpdateActivityMethod UpdateActivity;
    internal ActivityManager.FFIMethods.ClearActivityMethod ClearActivity;
    internal ActivityManager.FFIMethods.SendRequestReplyMethod SendRequestReply;
    internal ActivityManager.FFIMethods.SendInviteMethod SendInvite;
    internal ActivityManager.FFIMethods.AcceptInviteMethod AcceptInvite;

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate Result RegisterCommandMethod(IntPtr methodsPtr, [MarshalAs(UnmanagedType.LPStr)] string command);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate Result RegisterSteamMethod(IntPtr methodsPtr, uint steamId);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate void UpdateActivityCallback(IntPtr ptr, Result result);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate void UpdateActivityMethod(
      IntPtr methodsPtr,
      ref Activity activity,
      IntPtr callbackData,
      ActivityManager.FFIMethods.UpdateActivityCallback callback);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate void ClearActivityCallback(IntPtr ptr, Result result);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate void ClearActivityMethod(
      IntPtr methodsPtr,
      IntPtr callbackData,
      ActivityManager.FFIMethods.ClearActivityCallback callback);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate void SendRequestReplyCallback(IntPtr ptr, Result result);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate void SendRequestReplyMethod(
      IntPtr methodsPtr,
      long userId,
      ActivityJoinRequestReply reply,
      IntPtr callbackData,
      ActivityManager.FFIMethods.SendRequestReplyCallback callback);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate void SendInviteCallback(IntPtr ptr, Result result);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate void SendInviteMethod(
      IntPtr methodsPtr,
      long userId,
      ActivityActionType type,
      [MarshalAs(UnmanagedType.LPStr)] string content,
      IntPtr callbackData,
      ActivityManager.FFIMethods.SendInviteCallback callback);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate void AcceptInviteCallback(IntPtr ptr, Result result);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate void AcceptInviteMethod(
      IntPtr methodsPtr,
      long userId,
      IntPtr callbackData,
      ActivityManager.FFIMethods.AcceptInviteCallback callback);
  }
}
