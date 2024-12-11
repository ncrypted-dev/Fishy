using Fishy.Models;
using Fishy.Utils;
using Steamworks;

namespace Fishy.Event
{
   
    public class EventManager
    {
        public static event EventHandler<EventArgs.ChatMessageEventArgs> OnChatMessage;
        internal static void RaiseEvent(EventArgs.ChatMessageEventArgs eventArgs)  => OnChatMessage.Invoke(null, eventArgs);


        public static event EventHandler<EventArgs.PlayerJoinEventArgs> OnPlayerJoin;
        internal static void RaiseEvent(EventArgs.PlayerJoinEventArgs  eventArgs)  => OnPlayerJoin.Invoke(null, eventArgs);


        public static event EventHandler<EventArgs.PlayerLeaveEventArgs> OnPlayerLeave;
        internal static void RaiseEvent(EventArgs.PlayerLeaveEventArgs eventArgs)  => OnPlayerLeave.Invoke(null, eventArgs);


        public static event EventHandler<EventArgs.ActorSpawnEventArgs> OnActorSpawn;
        internal static void RaiseEvent(EventArgs.ActorSpawnEventArgs  eventArgs)  => OnActorSpawn.Invoke(null, eventArgs);


        public static event EventHandler<EventArgs.ActorDespawnEventArgs> OnActorDespawn;
        internal static void RaiseEvent(EventArgs.ActorDespawnEventArgs eventArgs) => OnActorDespawn.Invoke(null, eventArgs);


        public static event EventHandler<EventArgs.ActorActionEventArgs> OnActorAction;
        internal static void RaiseEvent(EventArgs.ActorActionEventArgs eventArgs)  => OnActorAction.Invoke(null, eventArgs);
    }
}
namespace Fishy.Event.EventArgs
{
    public class ActorActionEventArgs(SteamId steamId, string packetAction, Dictionary<string, object> packetInfo) : System.EventArgs
    {
        public SteamId SteamId { get; set; } = steamId;
        public string PacketAction { get; set; } = packetAction;

        public Dictionary<string, object> PacketInfo { get; set; } = packetInfo;

        public Dictionary<int, object> GetParams()
        {
            if (PacketInfo.ContainsKey("params"))
                return (Dictionary<int,object>)PacketInfo["params"];
            return new Dictionary<int, object>();
        }
    }

    public class ActorSpawnEventArgs(Actor actor)
    {
        public Actor Actor { get; set; } = actor;
    }

    public class ActorDespawnEventArgs(Actor actor)
    {
        public Actor Actor { get; set; } = actor;
    }

    public class ChatMessageEventArgs(SteamId steamId, string message) : System.EventArgs
    {
        public SteamId SteamId { get; set; } = steamId;
        public string Message { get; set; } = message;

        public ChatMessage ChatMessage { get; set; } = new ChatMessage(steamId, message);
    }

    public class PlayerJoinEventArgs(Player player) : System.EventArgs
    {
        public Player Player { get; set; } = player;
    }
    public class PlayerLeaveEventArgs(Player player) : System.EventArgs
    {
        public Player Player { get; set; } = player;
    }
}
