using System;

#nullable disable
namespace SakuraaCastingMod.Core.Patches;

public class Events
{
  public static event EventHandler<Events.RoomJoinedArgs> RoomJoined;

  public static event EventHandler<Events.RoomJoinedArgs> RoomLeft;

  public static event EventHandler GameInitialized;

  public virtual void TriggerRoomJoin(Events.RoomJoinedArgs e)
  {
    EventHandler<Events.RoomJoinedArgs> roomJoined = Events.RoomJoined;
    if (roomJoined == null)
      return;
    roomJoined.SafeInvoke<Events.RoomJoinedArgs>((object) this, e);
  }

  public virtual void TriggerRoomLeft(Events.RoomJoinedArgs e)
  {
    EventHandler<Events.RoomJoinedArgs> roomLeft = Events.RoomLeft;
    if (roomLeft == null)
      return;
    roomLeft.SafeInvoke<Events.RoomJoinedArgs>((object) this, e);
  }

  public virtual void TriggerGameInitialized()
  {
    EventHandler gameInitialized = Events.GameInitialized;
    if (gameInitialized == null)
      return;
    gameInitialized.SafeInvoke((object) this, EventArgs.Empty);
  }

  public class RoomJoinedArgs : EventArgs
  {
    public bool IsPrivate { get; set; }

    public string Gamemode { get; set; }
  }
}
