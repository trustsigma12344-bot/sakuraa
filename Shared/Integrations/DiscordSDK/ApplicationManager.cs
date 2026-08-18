using System;
using System.Runtime.InteropServices;
using System.Text;

#nullable disable
namespace SakuraaCastingMod.Shared.Integrations.DiscordSDK;

public class ApplicationManager
{
  private readonly IntPtr MethodsPtr;
  private object MethodsStructure;

  internal ApplicationManager(
    IntPtr ptr,
    IntPtr eventsPtr,
    ref ApplicationManager.FFIEvents events)
  {
    if (eventsPtr == IntPtr.Zero)
      throw new ResultException(Result.InternalError);
    this.InitEvents(eventsPtr, ref events);
    this.MethodsPtr = ptr;
    if (this.MethodsPtr == IntPtr.Zero)
      throw new ResultException(Result.InternalError);
  }

  private ApplicationManager.FFIMethods Methods
  {
    get
    {
      if (this.MethodsStructure == null)
        this.MethodsStructure = Marshal.PtrToStructure(this.MethodsPtr, typeof (ApplicationManager.FFIMethods));
      return (ApplicationManager.FFIMethods) this.MethodsStructure;
    }
  }

  private void InitEvents(IntPtr eventsPtr, ref ApplicationManager.FFIEvents events)
  {
    Marshal.StructureToPtr<ApplicationManager.FFIEvents>(events, eventsPtr, false);
  }

  [MonoPInvokeCallback]
  private static void ValidateOrExitCallbackImpl(IntPtr ptr, Result result)
  {
    GCHandle gcHandle = GCHandle.FromIntPtr(ptr);
    ApplicationManager.ValidateOrExitHandler target = (ApplicationManager.ValidateOrExitHandler) gcHandle.Target;
    gcHandle.Free();
    target(result);
  }

  public void ValidateOrExit(ApplicationManager.ValidateOrExitHandler callback)
  {
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    this.Methods.ValidateOrExit(this.MethodsPtr, GCHandle.ToIntPtr(GCHandle.Alloc((object) callback)), new ApplicationManager.FFIMethods.ValidateOrExitCallback(ApplicationManager.ValidateOrExitCallbackImpl));
  }

  public string GetCurrentLocale()
  {
    StringBuilder locale = new StringBuilder(128 /*0x80*/);
    this.Methods.GetCurrentLocale(this.MethodsPtr, locale);
    return locale.ToString();
  }

  public string GetCurrentBranch()
  {
    StringBuilder branch = new StringBuilder(4096 /*0x1000*/);
    this.Methods.GetCurrentBranch(this.MethodsPtr, branch);
    return branch.ToString();
  }

  [MonoPInvokeCallback]
  private static void GetOAuth2TokenCallbackImpl(
    IntPtr ptr,
    Result result,
    ref OAuth2Token oauth2Token)
  {
    GCHandle gcHandle = GCHandle.FromIntPtr(ptr);
    ApplicationManager.GetOAuth2TokenHandler target = (ApplicationManager.GetOAuth2TokenHandler) gcHandle.Target;
    gcHandle.Free();
    target(result, ref oauth2Token);
  }

  public void GetOAuth2Token(ApplicationManager.GetOAuth2TokenHandler callback)
  {
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    this.Methods.GetOAuth2Token(this.MethodsPtr, GCHandle.ToIntPtr(GCHandle.Alloc((object) callback)), new ApplicationManager.FFIMethods.GetOAuth2TokenCallback(ApplicationManager.GetOAuth2TokenCallbackImpl));
  }

  [MonoPInvokeCallback]
  private static void GetTicketCallbackImpl(IntPtr ptr, Result result, ref string data)
  {
    GCHandle gcHandle = GCHandle.FromIntPtr(ptr);
    ApplicationManager.GetTicketHandler target = (ApplicationManager.GetTicketHandler) gcHandle.Target;
    gcHandle.Free();
    target(result, ref data);
  }

  public void GetTicket(ApplicationManager.GetTicketHandler callback)
  {
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    this.Methods.GetTicket(this.MethodsPtr, GCHandle.ToIntPtr(GCHandle.Alloc((object) callback)), new ApplicationManager.FFIMethods.GetTicketCallback(ApplicationManager.GetTicketCallbackImpl));
  }

  public delegate void GetOAuth2TokenHandler(Result result, ref OAuth2Token oauth2Token);

  public delegate void GetTicketHandler(Result result, ref string data);

  public delegate void ValidateOrExitHandler(Result result);

  [StructLayout(LayoutKind.Sequential, Size = 1)]
  internal struct FFIEvents
  {
  }

  internal struct FFIMethods
  {
    internal ApplicationManager.FFIMethods.ValidateOrExitMethod ValidateOrExit;
    internal ApplicationManager.FFIMethods.GetCurrentLocaleMethod GetCurrentLocale;
    internal ApplicationManager.FFIMethods.GetCurrentBranchMethod GetCurrentBranch;
    internal ApplicationManager.FFIMethods.GetOAuth2TokenMethod GetOAuth2Token;
    internal ApplicationManager.FFIMethods.GetTicketMethod GetTicket;

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate void ValidateOrExitCallback(IntPtr ptr, Result result);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate void ValidateOrExitMethod(
      IntPtr methodsPtr,
      IntPtr callbackData,
      ApplicationManager.FFIMethods.ValidateOrExitCallback callback);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate void GetCurrentLocaleMethod(IntPtr methodsPtr, StringBuilder locale);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate void GetCurrentBranchMethod(IntPtr methodsPtr, StringBuilder branch);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate void GetOAuth2TokenCallback(
      IntPtr ptr,
      Result result,
      ref OAuth2Token oauth2Token);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate void GetOAuth2TokenMethod(
      IntPtr methodsPtr,
      IntPtr callbackData,
      ApplicationManager.FFIMethods.GetOAuth2TokenCallback callback);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate void GetTicketCallback(IntPtr ptr, Result result, [MarshalAs(UnmanagedType.LPStr)] ref string data);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate void GetTicketMethod(
      IntPtr methodsPtr,
      IntPtr callbackData,
      ApplicationManager.FFIMethods.GetTicketCallback callback);
  }
}
