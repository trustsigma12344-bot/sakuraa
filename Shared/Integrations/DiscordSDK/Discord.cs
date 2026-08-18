using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#nullable disable
namespace SakuraaCastingMod.Shared.Integrations.DiscordSDK;

public class Discord : IDisposable
{
  private AchievementManager.FFIEvents AchievementEvents;
  private readonly IntPtr AchievementEventsPtr;
  internal AchievementManager AchievementManagerInstance;
  private ActivityManager.FFIEvents ActivityEvents;
  private readonly IntPtr ActivityEventsPtr;
  internal ActivityManager ActivityManagerInstance;
  private ApplicationManager.FFIEvents ApplicationEvents;
  private readonly IntPtr ApplicationEventsPtr;
  internal ApplicationManager ApplicationManagerInstance;
  private readonly Discord.FFIEvents Events;
  private readonly IntPtr EventsPtr;
  private ImageManager.FFIEvents ImageEvents;
  private readonly IntPtr ImageEventsPtr;
  internal ImageManager ImageManagerInstance;
  private LobbyManager.FFIEvents LobbyEvents;
  private readonly IntPtr LobbyEventsPtr;
  internal LobbyManager LobbyManagerInstance;
  private readonly IntPtr MethodsPtr;
  private object MethodsStructure;
  private NetworkManager.FFIEvents NetworkEvents;
  private readonly IntPtr NetworkEventsPtr;
  internal NetworkManager NetworkManagerInstance;
  private OverlayManager.FFIEvents OverlayEvents;
  private readonly IntPtr OverlayEventsPtr;
  internal OverlayManager OverlayManagerInstance;
  private RelationshipManager.FFIEvents RelationshipEvents;
  private readonly IntPtr RelationshipEventsPtr;
  internal RelationshipManager RelationshipManagerInstance;
  private GCHandle SelfHandle;
  private GCHandle? setLogHook;
  private StorageManager.FFIEvents StorageEvents;
  private readonly IntPtr StorageEventsPtr;
  internal StorageManager StorageManagerInstance;
  private StoreManager.FFIEvents StoreEvents;
  private readonly IntPtr StoreEventsPtr;
  internal StoreManager StoreManagerInstance;
  private UserManager.FFIEvents UserEvents;
  private readonly IntPtr UserEventsPtr;
  internal UserManager UserManagerInstance;
  private VoiceManager.FFIEvents VoiceEvents;
  private readonly IntPtr VoiceEventsPtr;
  internal VoiceManager VoiceManagerInstance;

  public Discord(long clientId, ulong flags)
  {
    Discord.FFICreateParams createParams;
    createParams.ClientId = clientId;
    createParams.Flags = flags;
    this.Events = new Discord.FFIEvents();
    this.EventsPtr = Marshal.AllocHGlobal(Marshal.SizeOf<Discord.FFIEvents>(this.Events));
    createParams.Events = this.EventsPtr;
    this.SelfHandle = GCHandle.Alloc((object) this);
    createParams.EventData = GCHandle.ToIntPtr(this.SelfHandle);
    this.ApplicationEvents = new ApplicationManager.FFIEvents();
    this.ApplicationEventsPtr = Marshal.AllocHGlobal(Marshal.SizeOf<ApplicationManager.FFIEvents>(this.ApplicationEvents));
    createParams.ApplicationEvents = this.ApplicationEventsPtr;
    createParams.ApplicationVersion = 1U;
    this.UserEvents = new UserManager.FFIEvents();
    this.UserEventsPtr = Marshal.AllocHGlobal(Marshal.SizeOf<UserManager.FFIEvents>(this.UserEvents));
    createParams.UserEvents = this.UserEventsPtr;
    createParams.UserVersion = 1U;
    this.ImageEvents = new ImageManager.FFIEvents();
    this.ImageEventsPtr = Marshal.AllocHGlobal(Marshal.SizeOf<ImageManager.FFIEvents>(this.ImageEvents));
    createParams.ImageEvents = this.ImageEventsPtr;
    createParams.ImageVersion = 1U;
    this.ActivityEvents = new ActivityManager.FFIEvents();
    this.ActivityEventsPtr = Marshal.AllocHGlobal(Marshal.SizeOf<ActivityManager.FFIEvents>(this.ActivityEvents));
    createParams.ActivityEvents = this.ActivityEventsPtr;
    createParams.ActivityVersion = 1U;
    this.RelationshipEvents = new RelationshipManager.FFIEvents();
    this.RelationshipEventsPtr = Marshal.AllocHGlobal(Marshal.SizeOf<RelationshipManager.FFIEvents>(this.RelationshipEvents));
    createParams.RelationshipEvents = this.RelationshipEventsPtr;
    createParams.RelationshipVersion = 1U;
    this.LobbyEvents = new LobbyManager.FFIEvents();
    this.LobbyEventsPtr = Marshal.AllocHGlobal(Marshal.SizeOf<LobbyManager.FFIEvents>(this.LobbyEvents));
    createParams.LobbyEvents = this.LobbyEventsPtr;
    createParams.LobbyVersion = 1U;
    this.NetworkEvents = new NetworkManager.FFIEvents();
    this.NetworkEventsPtr = Marshal.AllocHGlobal(Marshal.SizeOf<NetworkManager.FFIEvents>(this.NetworkEvents));
    createParams.NetworkEvents = this.NetworkEventsPtr;
    createParams.NetworkVersion = 1U;
    this.OverlayEvents = new OverlayManager.FFIEvents();
    this.OverlayEventsPtr = Marshal.AllocHGlobal(Marshal.SizeOf<OverlayManager.FFIEvents>(this.OverlayEvents));
    createParams.OverlayEvents = this.OverlayEventsPtr;
    createParams.OverlayVersion = 2U;
    this.StorageEvents = new StorageManager.FFIEvents();
    this.StorageEventsPtr = Marshal.AllocHGlobal(Marshal.SizeOf<StorageManager.FFIEvents>(this.StorageEvents));
    createParams.StorageEvents = this.StorageEventsPtr;
    createParams.StorageVersion = 1U;
    this.StoreEvents = new StoreManager.FFIEvents();
    this.StoreEventsPtr = Marshal.AllocHGlobal(Marshal.SizeOf<StoreManager.FFIEvents>(this.StoreEvents));
    createParams.StoreEvents = this.StoreEventsPtr;
    createParams.StoreVersion = 1U;
    this.VoiceEvents = new VoiceManager.FFIEvents();
    this.VoiceEventsPtr = Marshal.AllocHGlobal(Marshal.SizeOf<VoiceManager.FFIEvents>(this.VoiceEvents));
    createParams.VoiceEvents = this.VoiceEventsPtr;
    createParams.VoiceVersion = 1U;
    this.AchievementEvents = new AchievementManager.FFIEvents();
    this.AchievementEventsPtr = Marshal.AllocHGlobal(Marshal.SizeOf<AchievementManager.FFIEvents>(this.AchievementEvents));
    createParams.AchievementEvents = this.AchievementEventsPtr;
    createParams.AchievementVersion = 1U;
    this.InitEvents(this.EventsPtr, ref this.Events);
    Result result = Discord.DiscordCreate(3U, ref createParams, out this.MethodsPtr);
    if (result != 0)
    {
      this.Dispose();
      throw new ResultException(result);
    }
  }

  private Discord.FFIMethods Methods
  {
    get
    {
      if (this.MethodsStructure == null)
        this.MethodsStructure = Marshal.PtrToStructure(this.MethodsPtr, typeof (Discord.FFIMethods));
      return (Discord.FFIMethods) this.MethodsStructure;
    }
  }

  public void Dispose()
  {
    if (this.MethodsPtr != IntPtr.Zero)
      this.Methods.Destroy(this.MethodsPtr);
    this.SelfHandle.Free();
    Marshal.FreeHGlobal(this.EventsPtr);
    Marshal.FreeHGlobal(this.ApplicationEventsPtr);
    Marshal.FreeHGlobal(this.UserEventsPtr);
    Marshal.FreeHGlobal(this.ImageEventsPtr);
    Marshal.FreeHGlobal(this.ActivityEventsPtr);
    Marshal.FreeHGlobal(this.RelationshipEventsPtr);
    Marshal.FreeHGlobal(this.LobbyEventsPtr);
    Marshal.FreeHGlobal(this.NetworkEventsPtr);
    Marshal.FreeHGlobal(this.OverlayEventsPtr);
    Marshal.FreeHGlobal(this.StorageEventsPtr);
    Marshal.FreeHGlobal(this.StoreEventsPtr);
    Marshal.FreeHGlobal(this.VoiceEventsPtr);
    Marshal.FreeHGlobal(this.AchievementEventsPtr);
    if (!this.setLogHook.HasValue)
      return;
    this.setLogHook.Value.Free();
  }

  [MethodImpl(MethodImplOptions.PreserveSig)]
  private static Result DiscordCreate(
    uint version,
    ref Discord.FFICreateParams createParams,
    out IntPtr manager)
  {
    manager = IntPtr.Zero;
    return Result.ServiceUnavailable;
  }

  private void InitEvents(IntPtr eventsPtr, ref Discord.FFIEvents events)
  {
    Marshal.StructureToPtr<Discord.FFIEvents>(events, eventsPtr, false);
  }

  public void RunCallbacks()
  {
    Result result = this.Methods.RunCallbacks(this.MethodsPtr);
    if (result != 0)
      throw new ResultException(result);
  }

  [MonoPInvokeCallback]
  private static void SetLogHookCallbackImpl(IntPtr ptr, LogLevel level, string message)
  {
    ((Discord.SetLogHookHandler) GCHandle.FromIntPtr(ptr).Target)(level, message);
  }

  public void SetLogHook(LogLevel minLevel, Discord.SetLogHookHandler callback)
  {
    if (this.setLogHook.HasValue)
      this.setLogHook.Value.Free();
    this.setLogHook = new GCHandle?(GCHandle.Alloc((object) callback));
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    this.Methods.SetLogHook(this.MethodsPtr, minLevel, GCHandle.ToIntPtr(this.setLogHook.Value), new Discord.FFIMethods.SetLogHookCallback(Discord.SetLogHookCallbackImpl));
  }

  public ApplicationManager GetApplicationManager()
  {
    if (this.ApplicationManagerInstance == null)
      this.ApplicationManagerInstance = new ApplicationManager(this.Methods.GetApplicationManager(this.MethodsPtr), this.ApplicationEventsPtr, ref this.ApplicationEvents);
    return this.ApplicationManagerInstance;
  }

  public UserManager GetUserManager()
  {
    if (this.UserManagerInstance == null)
      this.UserManagerInstance = new UserManager(this.Methods.GetUserManager(this.MethodsPtr), this.UserEventsPtr, ref this.UserEvents);
    return this.UserManagerInstance;
  }

  public ImageManager GetImageManager()
  {
    if (this.ImageManagerInstance == null)
      this.ImageManagerInstance = new ImageManager(this.Methods.GetImageManager(this.MethodsPtr), this.ImageEventsPtr, ref this.ImageEvents);
    return this.ImageManagerInstance;
  }

  public ActivityManager GetActivityManager()
  {
    if (this.ActivityManagerInstance == null)
      this.ActivityManagerInstance = new ActivityManager(this.Methods.GetActivityManager(this.MethodsPtr), this.ActivityEventsPtr, ref this.ActivityEvents);
    return this.ActivityManagerInstance;
  }

  public RelationshipManager GetRelationshipManager()
  {
    if (this.RelationshipManagerInstance == null)
      this.RelationshipManagerInstance = new RelationshipManager(this.Methods.GetRelationshipManager(this.MethodsPtr), this.RelationshipEventsPtr, ref this.RelationshipEvents);
    return this.RelationshipManagerInstance;
  }

  public LobbyManager GetLobbyManager()
  {
    if (this.LobbyManagerInstance == null)
      this.LobbyManagerInstance = new LobbyManager(this.Methods.GetLobbyManager(this.MethodsPtr), this.LobbyEventsPtr, ref this.LobbyEvents);
    return this.LobbyManagerInstance;
  }

  public NetworkManager GetNetworkManager()
  {
    if (this.NetworkManagerInstance == null)
      this.NetworkManagerInstance = new NetworkManager(this.Methods.GetNetworkManager(this.MethodsPtr), this.NetworkEventsPtr, ref this.NetworkEvents);
    return this.NetworkManagerInstance;
  }

  public OverlayManager GetOverlayManager()
  {
    if (this.OverlayManagerInstance == null)
      this.OverlayManagerInstance = new OverlayManager(this.Methods.GetOverlayManager(this.MethodsPtr), this.OverlayEventsPtr, ref this.OverlayEvents);
    return this.OverlayManagerInstance;
  }

  public StorageManager GetStorageManager()
  {
    if (this.StorageManagerInstance == null)
      this.StorageManagerInstance = new StorageManager(this.Methods.GetStorageManager(this.MethodsPtr), this.StorageEventsPtr, ref this.StorageEvents);
    return this.StorageManagerInstance;
  }

  public StoreManager GetStoreManager()
  {
    if (this.StoreManagerInstance == null)
      this.StoreManagerInstance = new StoreManager(this.Methods.GetStoreManager(this.MethodsPtr), this.StoreEventsPtr, ref this.StoreEvents);
    return this.StoreManagerInstance;
  }

  public VoiceManager GetVoiceManager()
  {
    if (this.VoiceManagerInstance == null)
      this.VoiceManagerInstance = new VoiceManager(this.Methods.GetVoiceManager(this.MethodsPtr), this.VoiceEventsPtr, ref this.VoiceEvents);
    return this.VoiceManagerInstance;
  }

  public AchievementManager GetAchievementManager()
  {
    if (this.AchievementManagerInstance == null)
      this.AchievementManagerInstance = new AchievementManager(this.Methods.GetAchievementManager(this.MethodsPtr), this.AchievementEventsPtr, ref this.AchievementEvents);
    return this.AchievementManagerInstance;
  }

  public delegate void SetLogHookHandler(LogLevel level, string message);

  [StructLayout(LayoutKind.Sequential, Size = 1)]
  internal struct FFIEvents
  {
  }

  internal struct FFIMethods
  {
    internal Discord.FFIMethods.DestroyHandler Destroy;
    internal Discord.FFIMethods.RunCallbacksMethod RunCallbacks;
    internal Discord.FFIMethods.SetLogHookMethod SetLogHook;
    internal Discord.FFIMethods.GetApplicationManagerMethod GetApplicationManager;
    internal Discord.FFIMethods.GetUserManagerMethod GetUserManager;
    internal Discord.FFIMethods.GetImageManagerMethod GetImageManager;
    internal Discord.FFIMethods.GetActivityManagerMethod GetActivityManager;
    internal Discord.FFIMethods.GetRelationshipManagerMethod GetRelationshipManager;
    internal Discord.FFIMethods.GetLobbyManagerMethod GetLobbyManager;
    internal Discord.FFIMethods.GetNetworkManagerMethod GetNetworkManager;
    internal Discord.FFIMethods.GetOverlayManagerMethod GetOverlayManager;
    internal Discord.FFIMethods.GetStorageManagerMethod GetStorageManager;
    internal Discord.FFIMethods.GetStoreManagerMethod GetStoreManager;
    internal Discord.FFIMethods.GetVoiceManagerMethod GetVoiceManager;
    internal Discord.FFIMethods.GetAchievementManagerMethod GetAchievementManager;

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate void DestroyHandler(IntPtr MethodsPtr);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate Result RunCallbacksMethod(IntPtr methodsPtr);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate void SetLogHookCallback(IntPtr ptr, LogLevel level, [MarshalAs(UnmanagedType.LPStr)] string message);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate void SetLogHookMethod(
      IntPtr methodsPtr,
      LogLevel minLevel,
      IntPtr callbackData,
      Discord.FFIMethods.SetLogHookCallback callback);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate IntPtr GetApplicationManagerMethod(IntPtr discordPtr);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate IntPtr GetUserManagerMethod(IntPtr discordPtr);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate IntPtr GetImageManagerMethod(IntPtr discordPtr);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate IntPtr GetActivityManagerMethod(IntPtr discordPtr);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate IntPtr GetRelationshipManagerMethod(IntPtr discordPtr);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate IntPtr GetLobbyManagerMethod(IntPtr discordPtr);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate IntPtr GetNetworkManagerMethod(IntPtr discordPtr);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate IntPtr GetOverlayManagerMethod(IntPtr discordPtr);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate IntPtr GetStorageManagerMethod(IntPtr discordPtr);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate IntPtr GetStoreManagerMethod(IntPtr discordPtr);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate IntPtr GetVoiceManagerMethod(IntPtr discordPtr);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate IntPtr GetAchievementManagerMethod(IntPtr discordPtr);
  }

  internal struct FFICreateParams
  {
    internal long ClientId;
    internal ulong Flags;
    internal IntPtr Events;
    internal IntPtr EventData;
    internal IntPtr ApplicationEvents;
    internal uint ApplicationVersion;
    internal IntPtr UserEvents;
    internal uint UserVersion;
    internal IntPtr ImageEvents;
    internal uint ImageVersion;
    internal IntPtr ActivityEvents;
    internal uint ActivityVersion;
    internal IntPtr RelationshipEvents;
    internal uint RelationshipVersion;
    internal IntPtr LobbyEvents;
    internal uint LobbyVersion;
    internal IntPtr NetworkEvents;
    internal uint NetworkVersion;
    internal IntPtr OverlayEvents;
    internal uint OverlayVersion;
    internal IntPtr StorageEvents;
    internal uint StorageVersion;
    internal IntPtr StoreEvents;
    internal uint StoreVersion;
    internal IntPtr VoiceEvents;
    internal uint VoiceVersion;
    internal IntPtr AchievementEvents;
    internal uint AchievementVersion;
  }
}
