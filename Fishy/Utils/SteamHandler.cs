using Fishy.Models;
using Steamworks;
using Steamworks.Data;

namespace Fishy.Utils
{
    class SteamHandler
    {
        public Lobby Lobby { get; set; }

        public SteamHandler()
        {
            SteamMatchmaking.OnLobbyCreated += SteamMatchmaking_OnLobbyCreated;
            SteamMatchmaking.OnLobbyMemberJoined += SteamMatchmaking_OnLobbyMemberJoined;
            SteamMatchmaking.OnLobbyMemberLeave += SteamMatchmaking_OnLobbyMemberLeave;
            SteamNetworking.OnP2PSessionRequest += SteamMatchmaking_OnP2PSessionRequest;
        }

        public static string Init()
        {
            try
            {
                SteamClient.Init(3146520, false);
                return "";
            }
            catch (Exception e) { return e.Message; }
        }

        public static void CreateLobby()
        {
            SteamMatchmaking.CreateLobbyAsync(Fishy.Config.MaxPlayers);
        }

        void SteamMatchmaking_OnLobbyCreated(Result result, Lobby Lobby)
        {
            Lobby.SetJoinable(true); 
            Lobby.SetData("ref", "webfishing_gamelobby");
            Lobby.SetData("name", Fishy.Config.ServerName);
            Lobby.SetData("version", Fishy.Config.GameVersion);
            Lobby.SetData("code", Fishy.Config.LobbyCode);
            Lobby.SetData("type", Fishy.Config.CodeOnly ? "code_only" : "public");
            Lobby.SetData("public", "true");
            Lobby.SetData("banned_players", "");
            Lobby.SetData("age_limit", Fishy.Config.AgeRestricted.ToString().ToLower());
            Lobby.SetData("cap", Fishy.Config.MaxPlayers.ToString());
            Lobby.SetData("lurefilter", "dedicated");

            SteamNetworking.AllowP2PPacketRelay(true);

            Lobby.SetData("server_browser_value", "0");

            Console.WriteLine("Lobby Created!");
            Console.WriteLine($"Lobby Code: {Lobby.GetData("code")}");

            this.Lobby = Lobby;
            UpdatePlayerCount();
        }

        void SteamMatchmaking_OnLobbyMemberJoined(Lobby Lobby, Friend userJoining)
        {
            // Try to stop spammers from spam joining, spammer get scammed!
            foreach (var pl in Fishy.Players)
                if (pl.SteamID == userJoining.Id) return;
            if (Punish.IsBanned(userJoining.Id)) // Well, that's the best I can do
                return;
            UpdatePlayerCount();
            Player player = new(userJoining.Id, userJoining.Name);

            Console.WriteLine(DateTime.Now.ToString("dd.MM HH:mm:ss") + $" A Player Joined: {userJoining.Name}");

            Fishy.Players.Add(player);

            Event.EventManager.RaiseEvent(new Event.EventArgs.PlayerJoinEventArgs(player));

            Console.Title = $"Fishy Server - There are currently {Fishy.Players.Count} Players playing";
        }

        void SteamMatchmaking_OnLobbyMemberLeave(Lobby Lobby, Friend userLeaving)
        {
            UpdatePlayerCount();
            Console.WriteLine(DateTime.Now.ToString("dd.MM HH:mm:ss") + $" A Player Left: {userLeaving.Name}");

            Event.EventManager.RaiseEvent(new Event.EventArgs.PlayerLeaveEventArgs(Fishy.Players.First(player => player.SteamID.Equals(userLeaving.Id))));

            Fishy.Players.RemoveAll(player => player.SteamID.Equals(userLeaving.Id));
            Console.Title = $"Fishy Server - There are currently {Fishy.Players.Count} Players playing";
        }

        void SteamMatchmaking_OnP2PSessionRequest(SteamId id)
        {
            if (Punish.IsBanned(id)) return;
            if (Lobby.Members.Any(player => player.Id.Value.Equals(id)))
                SteamNetworking.AcceptP2PSessionWithUser(id);
        }


        private void UpdatePlayerCount()
        {
            Lobby.SetData("name", Fishy.Config.ServerName);
            Lobby.SetData("lobby_name", Fishy.Config.ServerName);
        }

        public void UpdateSteamBanList(string bannedPlayerSteamID)
        {
            if (string.IsNullOrEmpty(bannedPlayerSteamID)) return;

            string banList = Lobby.GetData("banned_players");
            banList = String.IsNullOrEmpty(banList) ? bannedPlayerSteamID : $"{bannedPlayerSteamID},{banList}";
            Lobby.SetData("banned_players", banList);
        }

        public void SetSteamBanList(List<string> bannedPlayers)
        {
            if (bannedPlayers.Count == 0) return;
            string banList = String.Join(",", bannedPlayers);
            Lobby.SetData("banned_players", banList);
        }
    }
}
