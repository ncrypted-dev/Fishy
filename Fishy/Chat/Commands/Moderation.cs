using Fishy.Models;
using Fishy.Models.Packets;
using Fishy.Utils;
using Steamworks;
using System.Text;

namespace Fishy.Chat.Commands
{
    internal class StopCommand : Command
    {
        public override string Name => "stop";
        public override string Description => "Shuts down the server";
        public override PermissionLevel PermissionLevel => PermissionLevel.Admin;
        public override string[] Aliases => ["halt"];
        public override string Help => "!stop";

        public override void OnUse(SteamId executor, string[] arguments)
        {
            Console.WriteLine("Server was halted by " + executor);

            new ServerClosePacket().SendPacket("all", (int)CHANNELS.GAME_STATE);
            Fishy.SteamHandler.Lobby.SetJoinable(false); // Prevent takeover of lobby
            foreach (var pl in Fishy.Players) // Kick all players
                Punish.KickPlayer(pl);
            Fishy.SteamHandler.Lobby.Leave();
            Environment.Exit(0);
        }
    }

    internal class KickCommand : Command
    {
        public override string Name => "kick";
        public override string Description => "Kick a player";
        public override PermissionLevel PermissionLevel => PermissionLevel.Admin;
        public override string[] Aliases => [];
        public override string Help => "!kick player";

        public override void OnUse(SteamId executor, string[] arguments)
        {
            if (arguments.Length == 0) { ChatUtils.SendChat(executor, Help); return; }

            Player? playerToKick = ChatUtils.FindPlayer(arguments[0]);

            if (playerToKick == null)
                return;

            Punish.KickPlayer(playerToKick);

            ChatUtils.SendChat(executor, $"Kicked player {playerToKick.Name} {playerToKick.SteamID}");
        }
    }
    internal class BanCommand : Command
    {
        public override string Name => "ban";
        public override string Description => "Ban a player";
        public override PermissionLevel PermissionLevel => PermissionLevel.Admin;
        public override string[] Aliases => [];
        public override string Help => "!ban player";

        public override void OnUse(SteamId executor, string[] arguments)
        {
            if (arguments.Length == 0) { ChatUtils.SendChat(executor, Help); return; }

            Player? playerToBan = ChatUtils.FindPlayer(arguments[0]);

            if (playerToBan == null)
                return;

            Punish.BanPlayer(playerToBan);
            ChatUtils.SendChat(executor, $"Banned player {playerToBan.Name} {playerToBan.SteamID}");
        }
    }
    internal class SpawnCommand : Command
    {
        public override string Name => "spawn";
        public override string Description => "Spawn an entity";
        public override PermissionLevel PermissionLevel => PermissionLevel.Admin;
        public override string[] Aliases => [];
        public override string Help
        {
            get
            {
                StringBuilder result = new StringBuilder();
                result.AppendLine("!spawn type [force]\nAvailable default types: ");

                foreach (string typeName in Actor.ActorTypesByName.Keys)
                {
                    result.AppendLine(typeName);
                }

                return result.ToString();
            }
        }


        public override void OnUse(SteamId executor, string[] arguments)
        {
            if (arguments.Length == 0) { ChatUtils.SendChat(executor, Help); return; }

            string actorTypeName = arguments[0];

            if (Actor.ActorTypesByName.ContainsKey(actorTypeName))
            {
                Spawner.VanillaSpawn(Actor.ActorTypesByName[actorTypeName]);
                ChatUtils.SendChat(executor, $"A {actorTypeName} has been spawned!");
            }
            else if (arguments.Length > 1 && arguments[1] == "force")
            {
                Player? player = ChatUtils.FindPlayer(arguments[0]);
                Spawner.SpawnActor(new Actor(Spawner.GetFreeId(), actorTypeName, player.Position));
                ChatUtils.SendChat(executor, $"A {actorTypeName} has been spawned!");
            }
            else
            {
                ChatUtils.SendChat(executor, $"No actor with the name {actorTypeName} is known. Add the 'force' argument to try anyway.");
            }
        }
    }

    internal class CodeOnlyCommand : Command
    {
        public override string Name => "codeonly";
        public override string Description =>"sets lobby type";
        public override PermissionLevel PermissionLevel => PermissionLevel.Admin;
        public override string[] Aliases => [];
        public override string Help => "!codeonly true/false";

        public override void OnUse(SteamId executor, string[] arguments)
        {
            if (arguments.Length == 0) { ChatUtils.SendChat(executor, Help); return; }

            string type = arguments[0] == "true" ? "code_only" : "public";

            Fishy.SteamHandler.Lobby.SetData("type", type);

            ChatUtils.SendChat(executor, "The lobby type has been set to: " + type);
        }
    }

    internal class ReportCommand : Command
    {
        public override string Name => "report";
        public override string Description => "Report a player";
        public override PermissionLevel PermissionLevel => PermissionLevel.Player;
        public override string[] Aliases => [];
        public override string Help => "!report player reason";

        public override void OnUse(SteamId executor, string[] arguments)
        {
            if (arguments.Length < 2) { ChatUtils.SendChat(executor, Help); return; }

            string reportPath = Path.Combine(AppContext.BaseDirectory, Fishy.Config.ReportFolder, DateTime.Now.ToString("ddMMyyyyHHmmss") + arguments[0] + ".txt");
            string report = "Report for user: " + arguments[0];

            report += "\nReason: " + String.Join(" ", arguments[1..]);
            report += "\nChat Log:\n\n";

            string chatLog = String.Empty;
            Player? player = Fishy.Players.FirstOrDefault(p => p.Name.Equals(arguments[0]));

            if (player != null)
                chatLog = ChatLogger.GetLog(player.SteamID);
            else
                chatLog = ChatLogger.GetLog();

            File.WriteAllText(reportPath, report + ChatLogger.GetLog());
            ChatUtils.SendChat(executor, Fishy.Config.ReportResponse, "b30000");
        }
    }
    internal class IssueCommand : Command
    {
        public override string Name => "issue";
        public override string Description =>"Report an issue";
        public override PermissionLevel PermissionLevel => PermissionLevel.Player;
        public override string Help => "!issue description";
        public override string[] Aliases => [];

        public override void OnUse(SteamId executor, string[] arguments)
        {
            if (arguments.Length == 0)  { ChatUtils.SendChat(executor, Help); return; };

            string issuePath = Path.Combine(AppContext.BaseDirectory, Fishy.Config.ReportFolder, DateTime.Now.ToString("ddMMyyyyHHmmss") + "issueReport.txt");
            string issueReport = "Issue Report\n" + String.Join(" ", arguments[0..]);

            File.WriteAllText(issuePath, issueReport);
            ChatUtils.SendChat(executor, "Your issues has been received and will be looked at as soon as possible.", "b30000");
        }
    }
    internal class SetAdmin : Command
    {
        public override string Name => "setadmin";
        public override string Description => "set a player to admin temporally (CONSOLE ONLY)";
        public override PermissionLevel PermissionLevel => PermissionLevel.Server;
        public override string Help => "!setadmin name/steamid";
        public override string[] Aliases => [];

        public override void OnUse(SteamId executor, string[] arguments)
        {
            if (arguments.Length == 0) { ChatUtils.SendChat(executor, Help); return; };
            Player? player = ChatUtils.FindPlayer(arguments[0]);
            if (player == null)
            {
                ChatUtils.SendChat(executor, $"Couldn't find player\"{arguments[0]}\"!");
                return;
            }
            if (Fishy.Config.Admins.Contains(player.SteamID.ToString()))
            {
                ChatUtils.SendChat(executor, $"{player.Name} ({player.SteamID}) is already an admin!", "ffaaaa");
                return;
            }
            Fishy.Config.Admins.Add(player.SteamID.ToString());
            // Todo save config, might require some config rework too :(
            ChatUtils.SendChat(executor, $"Set {player.Name} ({player.SteamID}) to admin!", "aaffaa");
            ChatUtils.BroadcastChat($"{player.Name} ({player.SteamID}) was promoted to admin!", "aaffaa");
        }
    }
    internal class RevokeAdmin : Command
    {
        public override string Name => "revokeadmin";
        public override string Description => "revoke a player's admin access (CONSOLE ONLY)";
        public override PermissionLevel PermissionLevel => PermissionLevel.Server;
        public override string Help => "!revokeadmin steamid";
        public override string[] Aliases => [];

        public override void OnUse(SteamId executor, string[] arguments)
        {
            if (arguments.Length == 0) { ChatUtils.SendChat(executor, Help); return; };
            Player? player = ChatUtils.FindPlayer(arguments[0]);
            if (player == null)
            {
                ChatUtils.SendChat(executor, $"Couldn't find player\"{arguments[0]}\"!");
                return;
            }
            if (!Fishy.Config.Admins.Contains(player.SteamID.ToString()))
            {
                ChatUtils.SendChat(executor, $"{player.Name} {player.SteamID} is not an admin!", "ffaaaa");
                return;
            }
            Fishy.Config.Admins.Remove(player.SteamID.ToString());
            // Todo save config, might require some config rework too :(
            ChatUtils.SendChat(executor, $"Revoked {player.Name} ({player.SteamID})'s admin!", "aaffaa");
            ChatUtils.BroadcastChat($"{player.Name} ({player.SteamID}) was demoted from admin!", "ffaaaa");
        }

      
    }
    
}
