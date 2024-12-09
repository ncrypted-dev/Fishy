using Fishy.Models;
using Fishy.Utils;
using Steamworks;

namespace Fishy.Event
{
   
    public class EventManager
    {
        public static event EventHandler<Events.ChatMessageEventArgs> OnChatMessage;
        internal static void RaiseEvent(Events.ChatMessageEventArgs eventArgs)  => OnChatMessage.Invoke(null, eventArgs);


        public static event EventHandler<Events.PlayerJoinEventArgs> OnPlayerJoin;
        internal static void RaiseEvent(Events.PlayerJoinEventArgs  eventArgs)  => OnPlayerJoin.Invoke(null, eventArgs);


        public static event EventHandler<Events.PlayerLeaveEventArgs> OnPlayerLeave;
        internal static void RaiseEvent(Events.PlayerLeaveEventArgs eventArgs)  => OnPlayerLeave.Invoke(null, eventArgs);


        public static event EventHandler<Events.ActorSpawnEventArgs> OnActorSpawn;
        internal static void RaiseEvent(Events.ActorSpawnEventArgs  eventArgs)  => OnActorSpawn.Invoke(null, eventArgs);


        public static event EventHandler<Events.ActorDespawnEventArgs> OnActorDespawn;
        internal static void RaiseEvent(Events.ActorDespawnEventArgs eventArgs) => OnActorDespawn.Invoke(null, eventArgs);


        public static event EventHandler<Events.ActorActionEventArgs> OnActorAction;
        internal static void RaiseEvent(Events.ActorActionEventArgs eventArgs) => OnActorAction.Invoke(null, eventArgs);
    }
}
namespace Fishy.Event.Events
{
    public class ActorActionEventArgs(SteamId steamId, string packetAction) : EventArgs
    {
        public SteamId SteamId { get; set; } = steamId;
        public string PacketAction { get; set; } = packetAction;
    }

    public class ActorSpawnEventArgs(Actor actor)
    {
        public Actor Actor { get; set; } = actor;
    }

    public class ActorDespawnEventArgs(Actor actor)
    {
        public Actor Actor { get; set; } = actor;
    }

    public class ChatMessageEventArgs(SteamId steamId, string message) : EventArgs
    {
        public SteamId SteamId { get; set; } = steamId;
        public string Message { get; set; } = message;

        public ChatMessage ChatMessage { get; set; } = new ChatMessage(steamId, message);
    }

    public class PlayerJoinEventArgs(Player player) : EventArgs
    {
        public Player Player { get; set; } = player;
    }
    public class PlayerLeaveEventArgs(Player player) : EventArgs
    {
        public Player Player { get; set; } = player;
    }
}
