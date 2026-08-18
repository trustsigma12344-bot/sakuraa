using System;
using System.Runtime.InteropServices;

#nullable disable
namespace SakuraaCastingMod.Shared.Integrations.DiscordSDK;

public class AchievementManager
{
  private readonly IntPtr MethodsPtr;
  private object MethodsStructure;

  internal AchievementManager(
    IntPtr ptr,
    IntPtr eventsPtr,
    ref AchievementManager.FFIEvents events)
  {
    if (eventsPtr == IntPtr.Zero)
      throw new ResultException(Result.InternalError);
    this.InitEvents(eventsPtr, ref events);
    this.MethodsPtr = ptr;
    if (this.MethodsPtr == IntPtr.Zero)
      throw new ResultException(Result.InternalError);
  }

  private AchievementManager.FFIMethods Methods
  {
    get
    {
      if (this.MethodsStructure == null)
        this.MethodsStructure = Marshal.PtrToStructure(this.MethodsPtr, typeof (AchievementManager.FFIMethods));
      return (AchievementManager.FFIMethods) this.MethodsStructure;
    }
  }

  public event AchievementManager.UserAchievementUpdateHandler OnUserAchievementUpdate;

  private void InitEvents(IntPtr eventsPtr, ref AchievementManager.FFIEvents events)
  {
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    events.OnUserAchievementUpdate = new AchievementManager.FFIEvents.UserAchievementUpdateHandler(AchievementManager.OnUserAchievementUpdateImpl);
    Marshal.StructureToPtr<AchievementManager.FFIEvents>(events, eventsPtr, false);
  }

  [MonoPInvokeCallback]
  private static void SetUserAchievementCallbackImpl(IntPtr ptr, Result result)
  {
    GCHandle gcHandle = GCHandle.FromIntPtr(ptr);
    AchievementManager.SetUserAchievementHandler target = (AchievementManager.SetUserAchievementHandler) gcHandle.Target;
    gcHandle.Free();
    target(result);
  }

  public void SetUserAchievement(
    long achievementId,
    byte percentComplete,
    AchievementManager.SetUserAchievementHandler callback)
  {
    GCHandle gcHandle = GCHandle.Alloc((object) callback);
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    this.Methods.SetUserAchievement(this.MethodsPtr, achievementId, percentComplete, GCHandle.ToIntPtr(gcHandle), new AchievementManager.FFIMethods.SetUserAchievementCallback(AchievementManager.SetUserAchievementCallbackImpl));
  }

  [MonoPInvokeCallback]
  private static void FetchUserAchievementsCallbackImpl(IntPtr ptr, Result result)
  {
    GCHandle gcHandle = GCHandle.FromIntPtr(ptr);
    AchievementManager.FetchUserAchievementsHandler target = (AchievementManager.FetchUserAchievementsHandler) gcHandle.Target;
    gcHandle.Free();
    target(result);
  }

  public void FetchUserAchievements(
    AchievementManager.FetchUserAchievementsHandler callback)
  {
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    this.Methods.FetchUserAchievements(this.MethodsPtr, GCHandle.ToIntPtr(GCHandle.Alloc((object) callback)), new AchievementManager.FFIMethods.FetchUserAchievementsCallback(AchievementManager.FetchUserAchievementsCallbackImpl));
  }

  public int CountUserAchievements()
  {
    int count = 0;
    this.Methods.CountUserAchievements(this.MethodsPtr, ref count);
    return count;
  }

  public UserAchievement GetUserAchievement(long userAchievementId)
  {
    UserAchievement userAchievement = new UserAchievement();
    Result result = this.Methods.GetUserAchievement(this.MethodsPtr, userAchievementId, ref userAchievement);
    if (result != 0)
      throw new ResultException(result);
    return userAchievement;
  }

  public UserAchievement GetUserAchievementAt(int index)
  {
    UserAchievement userAchievement = new UserAchievement();
    Result result = this.Methods.GetUserAchievementAt(this.MethodsPtr, index, ref userAchievement);
    if (result != 0)
      throw new ResultException(result);
    return userAchievement;
  }

  [MonoPInvokeCallback]
  private static void OnUserAchievementUpdateImpl(IntPtr ptr, ref UserAchievement userAchievement)
  {
    Discord target = (Discord) GCHandle.FromIntPtr(ptr).Target;
    if (target.AchievementManagerInstance.OnUserAchievementUpdate == null)
      return;
    target.AchievementManagerInstance.OnUserAchievementUpdate(ref userAchievement);
  }

  public delegate void FetchUserAchievementsHandler(Result result);

  public delegate void SetUserAchievementHandler(Result result);

  public delegate void UserAchievementUpdateHandler(ref UserAchievement userAchievement);

  internal struct FFIEvents
  {
    internal AchievementManager.FFIEvents.UserAchievementUpdateHandler OnUserAchievementUpdate;

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate void UserAchievementUpdateHandler(
      IntPtr ptr,
      ref UserAchievement userAchievement);
  }

  internal struct FFIMethods
  {
    internal AchievementManager.FFIMethods.SetUserAchievementMethod SetUserAchievement;
    internal AchievementManager.FFIMethods.FetchUserAchievementsMethod FetchUserAchievements;
    internal AchievementManager.FFIMethods.CountUserAchievementsMethod CountUserAchievements;
    internal AchievementManager.FFIMethods.GetUserAchievementMethod GetUserAchievement;
    internal AchievementManager.FFIMethods.GetUserAchievementAtMethod GetUserAchievementAt;

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate void SetUserAchievementCallback(IntPtr ptr, Result result);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate void SetUserAchievementMethod(
      IntPtr methodsPtr,
      long achievementId,
      byte percentComplete,
      IntPtr callbackData,
      AchievementManager.FFIMethods.SetUserAchievementCallback callback);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate void FetchUserAchievementsCallback(IntPtr ptr, Result result);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate void FetchUserAchievementsMethod(
      IntPtr methodsPtr,
      IntPtr callbackData,
      AchievementManager.FFIMethods.FetchUserAchievementsCallback callback);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate void CountUserAchievementsMethod(IntPtr methodsPtr, ref int count);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate Result GetUserAchievementMethod(
      IntPtr methodsPtr,
      long userAchievementId,
      ref UserAchievement userAchievement);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate Result GetUserAchievementAtMethod(
      IntPtr methodsPtr,
      int index,
      ref UserAchievement userAchievement);
  }
}
