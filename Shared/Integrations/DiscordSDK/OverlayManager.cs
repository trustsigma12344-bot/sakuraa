using System;
using System.Runtime.InteropServices;

#nullable disable
namespace SakuraaCastingMod.Shared.Integrations.DiscordSDK;

public class OverlayManager
{
  private readonly IntPtr MethodsPtr;
  private object MethodsStructure;

  internal OverlayManager(IntPtr ptr, IntPtr eventsPtr, ref OverlayManager.FFIEvents events)
  {
    if (eventsPtr == IntPtr.Zero)
      throw new ResultException(Result.InternalError);
    this.InitEvents(eventsPtr, ref events);
    this.MethodsPtr = ptr;
    if (this.MethodsPtr == IntPtr.Zero)
      throw new ResultException(Result.InternalError);
  }

  private OverlayManager.FFIMethods Methods
  {
    get
    {
      if (this.MethodsStructure == null)
        this.MethodsStructure = Marshal.PtrToStructure(this.MethodsPtr, typeof (OverlayManager.FFIMethods));
      return (OverlayManager.FFIMethods) this.MethodsStructure;
    }
  }

  public event OverlayManager.ToggleHandler OnToggle;

  private void InitEvents(IntPtr eventsPtr, ref OverlayManager.FFIEvents events)
  {
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    events.OnToggle = new OverlayManager.FFIEvents.ToggleHandler(OverlayManager.OnToggleImpl);
    Marshal.StructureToPtr<OverlayManager.FFIEvents>(events, eventsPtr, false);
  }

  public bool IsEnabled()
  {
    bool enabled = false;
    this.Methods.IsEnabled(this.MethodsPtr, ref enabled);
    return enabled;
  }

  public bool IsLocked()
  {
    bool locked = false;
    this.Methods.IsLocked(this.MethodsPtr, ref locked);
    return locked;
  }

  [MonoPInvokeCallback]
  private static void SetLockedCallbackImpl(IntPtr ptr, Result result)
  {
    GCHandle gcHandle = GCHandle.FromIntPtr(ptr);
    OverlayManager.SetLockedHandler target = (OverlayManager.SetLockedHandler) gcHandle.Target;
    gcHandle.Free();
    target(result);
  }

  public void SetLocked(bool locked, OverlayManager.SetLockedHandler callback)
  {
    GCHandle gcHandle = GCHandle.Alloc((object) callback);
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    this.Methods.SetLocked(this.MethodsPtr, locked, GCHandle.ToIntPtr(gcHandle), new OverlayManager.FFIMethods.SetLockedCallback(OverlayManager.SetLockedCallbackImpl));
  }

  [MonoPInvokeCallback]
  private static void OpenActivityInviteCallbackImpl(IntPtr ptr, Result result)
  {
    GCHandle gcHandle = GCHandle.FromIntPtr(ptr);
    OverlayManager.OpenActivityInviteHandler target = (OverlayManager.OpenActivityInviteHandler) gcHandle.Target;
    gcHandle.Free();
    target(result);
  }

  public void OpenActivityInvite(
    ActivityActionType type,
    OverlayManager.OpenActivityInviteHandler callback)
  {
    GCHandle gcHandle = GCHandle.Alloc((object) callback);
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    this.Methods.OpenActivityInvite(this.MethodsPtr, type, GCHandle.ToIntPtr(gcHandle), new OverlayManager.FFIMethods.OpenActivityInviteCallback(OverlayManager.OpenActivityInviteCallbackImpl));
  }

  [MonoPInvokeCallback]
  private static void OpenGuildInviteCallbackImpl(IntPtr ptr, Result result)
  {
    GCHandle gcHandle = GCHandle.FromIntPtr(ptr);
    OverlayManager.OpenGuildInviteHandler target = (OverlayManager.OpenGuildInviteHandler) gcHandle.Target;
    gcHandle.Free();
    target(result);
  }

  public void OpenGuildInvite(string code, OverlayManager.OpenGuildInviteHandler callback)
  {
    GCHandle gcHandle = GCHandle.Alloc((object) callback);
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    this.Methods.OpenGuildInvite(this.MethodsPtr, code, GCHandle.ToIntPtr(gcHandle), new OverlayManager.FFIMethods.OpenGuildInviteCallback(OverlayManager.OpenGuildInviteCallbackImpl));
  }

  [MonoPInvokeCallback]
  private static void OpenVoiceSettingsCallbackImpl(IntPtr ptr, Result result)
  {
    GCHandle gcHandle = GCHandle.FromIntPtr(ptr);
    OverlayManager.OpenVoiceSettingsHandler target = (OverlayManager.OpenVoiceSettingsHandler) gcHandle.Target;
    gcHandle.Free();
    target(result);
  }

  public void OpenVoiceSettings(OverlayManager.OpenVoiceSettingsHandler callback)
  {
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    this.Methods.OpenVoiceSettings(this.MethodsPtr, GCHandle.ToIntPtr(GCHandle.Alloc((object) callback)), new OverlayManager.FFIMethods.OpenVoiceSettingsCallback(OverlayManager.OpenVoiceSettingsCallbackImpl));
  }

  public void InitDrawingDxgi(IntPtr swapchain, bool useMessageForwarding)
  {
    Result result = this.Methods.InitDrawingDxgi(this.MethodsPtr, swapchain, useMessageForwarding);
    if (result != 0)
      throw new ResultException(result);
  }

  public void OnPresent() => this.Methods.OnPresent(this.MethodsPtr);

  public void ForwardMessage(IntPtr message)
  {
    this.Methods.ForwardMessage(this.MethodsPtr, message);
  }

  public void KeyEvent(bool down, string keyCode, KeyVariant variant)
  {
    this.Methods.KeyEvent(this.MethodsPtr, down, keyCode, variant);
  }

  public void CharEvent(string character) => this.Methods.CharEvent(this.MethodsPtr, character);

  public void MouseButtonEvent(byte down, int clickCount, MouseButton which, int x, int y)
  {
    this.Methods.MouseButtonEvent(this.MethodsPtr, down, clickCount, which, x, y);
  }

  public void MouseMotionEvent(int x, int y)
  {
    this.Methods.MouseMotionEvent(this.MethodsPtr, x, y);
  }

  public void ImeCommitText(string text) => this.Methods.ImeCommitText(this.MethodsPtr, text);

  public void ImeSetComposition(string text, ImeUnderline underlines, int from, int to)
  {
    this.Methods.ImeSetComposition(this.MethodsPtr, text, ref underlines, from, to);
  }

  public void ImeCancelComposition() => this.Methods.ImeCancelComposition(this.MethodsPtr);

  [MonoPInvokeCallback]
  private static void SetImeCompositionRangeCallbackCallbackImpl(
    IntPtr ptr,
    int from,
    int to,
    ref Rect bounds)
  {
    GCHandle gcHandle = GCHandle.FromIntPtr(ptr);
    OverlayManager.SetImeCompositionRangeCallbackHandler target = (OverlayManager.SetImeCompositionRangeCallbackHandler) gcHandle.Target;
    gcHandle.Free();
    target(from, to, ref bounds);
  }

  public void SetImeCompositionRangeCallback(
    OverlayManager.SetImeCompositionRangeCallbackHandler callback)
  {
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    this.Methods.SetImeCompositionRangeCallback(this.MethodsPtr, GCHandle.ToIntPtr(GCHandle.Alloc((object) callback)), new OverlayManager.FFIMethods.SetImeCompositionRangeCallbackCallback(OverlayManager.SetImeCompositionRangeCallbackCallbackImpl));
  }

  [MonoPInvokeCallback]
  private static void SetImeSelectionBoundsCallbackCallbackImpl(
    IntPtr ptr,
    Rect anchor,
    Rect focus,
    bool isAnchorFirst)
  {
    GCHandle gcHandle = GCHandle.FromIntPtr(ptr);
    OverlayManager.SetImeSelectionBoundsCallbackHandler target = (OverlayManager.SetImeSelectionBoundsCallbackHandler) gcHandle.Target;
    gcHandle.Free();
    target(anchor, focus, isAnchorFirst);
  }

  public void SetImeSelectionBoundsCallback(
    OverlayManager.SetImeSelectionBoundsCallbackHandler callback)
  {
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    this.Methods.SetImeSelectionBoundsCallback(this.MethodsPtr, GCHandle.ToIntPtr(GCHandle.Alloc((object) callback)), new OverlayManager.FFIMethods.SetImeSelectionBoundsCallbackCallback(OverlayManager.SetImeSelectionBoundsCallbackCallbackImpl));
  }

  public bool IsPointInsideClickZone(int x, int y)
  {
    return this.Methods.IsPointInsideClickZone(this.MethodsPtr, x, y);
  }

  [MonoPInvokeCallback]
  private static void OnToggleImpl(IntPtr ptr, bool locked)
  {
    Discord target = (Discord) GCHandle.FromIntPtr(ptr).Target;
    if (target.OverlayManagerInstance.OnToggle == null)
      return;
    target.OverlayManagerInstance.OnToggle(locked);
  }

  public delegate void OpenActivityInviteHandler(Result result);

  public delegate void OpenGuildInviteHandler(Result result);

  public delegate void OpenVoiceSettingsHandler(Result result);

  public delegate void SetImeCompositionRangeCallbackHandler(int from, int to, ref Rect bounds);

  public delegate void SetImeSelectionBoundsCallbackHandler(
    Rect anchor,
    Rect focus,
    bool isAnchorFirst);

  public delegate void SetLockedHandler(Result result);

  public delegate void ToggleHandler(bool locked);

  internal struct FFIEvents
  {
    internal OverlayManager.FFIEvents.ToggleHandler OnToggle;

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate void ToggleHandler(IntPtr ptr, bool locked);
  }

  internal struct FFIMethods
  {
    internal OverlayManager.FFIMethods.IsEnabledMethod IsEnabled;
    internal OverlayManager.FFIMethods.IsLockedMethod IsLocked;
    internal OverlayManager.FFIMethods.SetLockedMethod SetLocked;
    internal OverlayManager.FFIMethods.OpenActivityInviteMethod OpenActivityInvite;
    internal OverlayManager.FFIMethods.OpenGuildInviteMethod OpenGuildInvite;
    internal OverlayManager.FFIMethods.OpenVoiceSettingsMethod OpenVoiceSettings;
    internal OverlayManager.FFIMethods.InitDrawingDxgiMethod InitDrawingDxgi;
    internal OverlayManager.FFIMethods.OnPresentMethod OnPresent;
    internal OverlayManager.FFIMethods.ForwardMessageMethod ForwardMessage;
    internal OverlayManager.FFIMethods.KeyEventMethod KeyEvent;
    internal OverlayManager.FFIMethods.CharEventMethod CharEvent;
    internal OverlayManager.FFIMethods.MouseButtonEventMethod MouseButtonEvent;
    internal OverlayManager.FFIMethods.MouseMotionEventMethod MouseMotionEvent;
    internal OverlayManager.FFIMethods.ImeCommitTextMethod ImeCommitText;
    internal OverlayManager.FFIMethods.ImeSetCompositionMethod ImeSetComposition;
    internal OverlayManager.FFIMethods.ImeCancelCompositionMethod ImeCancelComposition;
    internal OverlayManager.FFIMethods.SetImeCompositionRangeCallbackMethod SetImeCompositionRangeCallback;
    internal OverlayManager.FFIMethods.SetImeSelectionBoundsCallbackMethod SetImeSelectionBoundsCallback;
    internal OverlayManager.FFIMethods.IsPointInsideClickZoneMethod IsPointInsideClickZone;

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate void IsEnabledMethod(IntPtr methodsPtr, ref bool enabled);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate void IsLockedMethod(IntPtr methodsPtr, ref bool locked);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate void SetLockedCallback(IntPtr ptr, Result result);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate void SetLockedMethod(
      IntPtr methodsPtr,
      bool locked,
      IntPtr callbackData,
      OverlayManager.FFIMethods.SetLockedCallback callback);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate void OpenActivityInviteCallback(IntPtr ptr, Result result);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate void OpenActivityInviteMethod(
      IntPtr methodsPtr,
      ActivityActionType type,
      IntPtr callbackData,
      OverlayManager.FFIMethods.OpenActivityInviteCallback callback);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate void OpenGuildInviteCallback(IntPtr ptr, Result result);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate void OpenGuildInviteMethod(
      IntPtr methodsPtr,
      [MarshalAs(UnmanagedType.LPStr)] string code,
      IntPtr callbackData,
      OverlayManager.FFIMethods.OpenGuildInviteCallback callback);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate void OpenVoiceSettingsCallback(IntPtr ptr, Result result);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate void OpenVoiceSettingsMethod(
      IntPtr methodsPtr,
      IntPtr callbackData,
      OverlayManager.FFIMethods.OpenVoiceSettingsCallback callback);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate Result InitDrawingDxgiMethod(
      IntPtr methodsPtr,
      IntPtr swapchain,
      bool useMessageForwarding);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate void OnPresentMethod(IntPtr methodsPtr);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate void ForwardMessageMethod(IntPtr methodsPtr, IntPtr message);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate void KeyEventMethod(
      IntPtr methodsPtr,
      bool down,
      [MarshalAs(UnmanagedType.LPStr)] string keyCode,
      KeyVariant variant);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate void CharEventMethod(IntPtr methodsPtr, [MarshalAs(UnmanagedType.LPStr)] string character);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate void MouseButtonEventMethod(
      IntPtr methodsPtr,
      byte down,
      int clickCount,
      MouseButton which,
      int x,
      int y);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate void MouseMotionEventMethod(IntPtr methodsPtr, int x, int y);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate void ImeCommitTextMethod(IntPtr methodsPtr, [MarshalAs(UnmanagedType.LPStr)] string text);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate void ImeSetCompositionMethod(
      IntPtr methodsPtr,
      [MarshalAs(UnmanagedType.LPStr)] string text,
      ref ImeUnderline underlines,
      int from,
      int to);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate void ImeCancelCompositionMethod(IntPtr methodsPtr);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate void SetImeCompositionRangeCallbackCallback(
      IntPtr ptr,
      int from,
      int to,
      ref Rect bounds);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate void SetImeCompositionRangeCallbackMethod(
      IntPtr methodsPtr,
      IntPtr callbackData,
      OverlayManager.FFIMethods.SetImeCompositionRangeCallbackCallback callback);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate void SetImeSelectionBoundsCallbackCallback(
      IntPtr ptr,
      Rect anchor,
      Rect focus,
      bool isAnchorFirst);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate void SetImeSelectionBoundsCallbackMethod(
      IntPtr methodsPtr,
      IntPtr callbackData,
      OverlayManager.FFIMethods.SetImeSelectionBoundsCallbackCallback callback);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate bool IsPointInsideClickZoneMethod(IntPtr methodsPtr, int x, int y);
  }
}
