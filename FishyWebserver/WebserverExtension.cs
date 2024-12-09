using Fishy.Extensions;

namespace Fishy.Webserver
{
    class WebserverExtension : FishyExtension
    {
        readonly static Dashboard dashboard = new();

        public static string GetUsername()
            => GetConfigValue("webserver")["username"].ToString() ?? "";
        public static string GetPassword()
            => GetConfigValue("webserver")["password"].ToString() ?? "";
        public static void OnChatMessage(object? sender, Event.Events.ChatMessageEventArgs args)
            => dashboard.MessageToSync.Add(args.ChatMessage);
        public static void OnPlayerJoin(object? sender,Event.Events.PlayerJoinEventArgs args)
            => dashboard.PlayersToSync.TryAdd(args.Player, "join");
        public static void OnPlayerLeave(object? sender, Event.Events.PlayerLeaveEventArgs args)
            => dashboard.PlayersToSync.TryAdd(args.Player, "leave");
        public override void OnInit()
        { 
            dashboard.Initalize();

            // Subscribe to events
            Event.EventManager.OnChatMessage += OnChatMessage;
            Event.EventManager.OnPlayerJoin += OnPlayerJoin;
            Event.EventManager.OnPlayerLeave += OnPlayerLeave;

        }
    }
}
