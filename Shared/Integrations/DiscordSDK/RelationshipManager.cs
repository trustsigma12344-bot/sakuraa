using System;
using System.Runtime.InteropServices;

#nullable disable
namespace SakuraaCastingMod.Shared.Integrations.DiscordSDK;

public class RelationshipManager
{
  private readonly IntPtr MethodsPtr;
  private object MethodsStructure;

  internal RelationshipManager(
    IntPtr ptr,
    IntPtr eventsPtr,
    ref RelationshipManager.FFIEvents events)
  {
    if (eventsPtr == IntPtr.Zero)
      throw new ResultException(Result.InternalError);
    this.InitEvents(eventsPtr, ref events);
    this.MethodsPtr = ptr;
    if (this.MethodsPtr == IntPtr.Zero)
      throw new ResultException(Result.InternalError);
  }

  private RelationshipManager.FFIMethods Methods
  {
    get
    {
      if (this.MethodsStructure == null)
        this.MethodsStructure = Marshal.PtrToStructure(this.MethodsPtr, typeof (RelationshipManager.FFIMethods));
      return (RelationshipManager.FFIMethods) this.MethodsStructure;
    }
  }

  public event RelationshipManager.RefreshHandler OnRefresh;

  public event RelationshipManager.RelationshipUpdateHandler OnRelationshipUpdate;

  private void InitEvents(IntPtr eventsPtr, ref RelationshipManager.FFIEvents events)
  {
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    events.OnRefresh = new RelationshipManager.FFIEvents.RefreshHandler(RelationshipManager.OnRefreshImpl);
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    events.OnRelationshipUpdate = new RelationshipManager.FFIEvents.RelationshipUpdateHandler(RelationshipManager.OnRelationshipUpdateImpl);
    Marshal.StructureToPtr<RelationshipManager.FFIEvents>(events, eventsPtr, false);
  }

  [MonoPInvokeCallback]
  private static bool FilterCallbackImpl(IntPtr ptr, ref Relationship relationship)
  {
    return ((RelationshipManager.FilterHandler) GCHandle.FromIntPtr(ptr).Target)(ref relationship);
  }

  public void Filter(RelationshipManager.FilterHandler callback)
  {
    GCHandle gcHandle = GCHandle.Alloc((object) callback);
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    this.Methods.Filter(this.MethodsPtr, GCHandle.ToIntPtr(gcHandle), new RelationshipManager.FFIMethods.FilterCallback(RelationshipManager.FilterCallbackImpl));
    gcHandle.Free();
  }

  public int Count()
  {
    int count = 0;
    Result result = this.Methods.Count(this.MethodsPtr, ref count);
    if (result != 0)
      throw new ResultException(result);
    return count;
  }

  public Relationship Get(long userId)
  {
    Relationship relationship = new Relationship();
    Result result = this.Methods.Get(this.MethodsPtr, userId, ref relationship);
    if (result != 0)
      throw new ResultException(result);
    return relationship;
  }

  public Relationship GetAt(uint index)
  {
    Relationship relationship = new Relationship();
    Result result = this.Methods.GetAt(this.MethodsPtr, index, ref relationship);
    if (result != 0)
      throw new ResultException(result);
    return relationship;
  }

  [MonoPInvokeCallback]
  private static void OnRefreshImpl(IntPtr ptr)
  {
    Discord target = (Discord) GCHandle.FromIntPtr(ptr).Target;
    if (target.RelationshipManagerInstance.OnRefresh == null)
      return;
    target.RelationshipManagerInstance.OnRefresh();
  }

  [MonoPInvokeCallback]
  private static void OnRelationshipUpdateImpl(IntPtr ptr, ref Relationship relationship)
  {
    Discord target = (Discord) GCHandle.FromIntPtr(ptr).Target;
    if (target.RelationshipManagerInstance.OnRelationshipUpdate == null)
      return;
    target.RelationshipManagerInstance.OnRelationshipUpdate(ref relationship);
  }

  public delegate bool FilterHandler(ref Relationship relationship);

  public delegate void RefreshHandler();

  public delegate void RelationshipUpdateHandler(ref Relationship relationship);

  internal struct FFIEvents
  {
    internal RelationshipManager.FFIEvents.RefreshHandler OnRefresh;
    internal RelationshipManager.FFIEvents.RelationshipUpdateHandler OnRelationshipUpdate;

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate void RefreshHandler(IntPtr ptr);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate void RelationshipUpdateHandler(IntPtr ptr, ref Relationship relationship);
  }

  internal struct FFIMethods
  {
    internal RelationshipManager.FFIMethods.FilterMethod Filter;
    internal RelationshipManager.FFIMethods.CountMethod Count;
    internal RelationshipManager.FFIMethods.GetMethod Get;
    internal RelationshipManager.FFIMethods.GetAtMethod GetAt;

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate bool FilterCallback(IntPtr ptr, ref Relationship relationship);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate void FilterMethod(
      IntPtr methodsPtr,
      IntPtr callbackData,
      RelationshipManager.FFIMethods.FilterCallback callback);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate Result CountMethod(IntPtr methodsPtr, ref int count);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate Result GetMethod(
      IntPtr methodsPtr,
      long userId,
      ref Relationship relationship);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate Result GetAtMethod(
      IntPtr methodsPtr,
      uint index,
      ref Relationship relationship);
  }
}
