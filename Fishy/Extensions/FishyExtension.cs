using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Fishy.Models;
using Fishy.Models.Packets;
using Fishy.Utils;
using Steamworks;

namespace Fishy.Extensions
{
    public abstract class FishyExtension
    {
        public static List<Player> Players { get => Fishy.Players; }
        public static List<string> BannedPlayers { get => Fishy.BannedUsers; }
        public static List<Actor> Actors { get => Fishy.Actors; }
        public static List<ChatMessage> ChatLog { get => ChatLogger.ChatLogs; }

        public virtual void OnInit() { }


        public static Dictionary<string, object> GetConfigValue(string table)
            => Fishy.Config.GetConfigValue(table);

        public static void SendPacketToAll(FPacket packet)
            => packet.SendPacket("all", (int)CHANNELS.GAME_STATE);

        public static void SendPacketToPlayer(FPacket packet, SteamId player)
            => packet.SendPacket("single", (int)CHANNELS.GAME_STATE, player);

		/// <summary>
        /// Spawns an actor using the vanilla spawning algorithm.
		/// </summary>
        /// <remarks>
        /// Note that because the spawning algorithm is reimplemented by Fishy,
        /// any actors not yet implemented in Fishy will not have a vanilla
        /// spawning algorithm. These will need to be spawned with the
        /// <c>SpawnActor(string, Vector3, Vector3)</c> overload.
        /// </remarks>
		public static void SpawnActor(ActorType type)
        {
            Spawner.VanillaSpawn(type);
        }

        /// <summary>
        /// Spawns an actor by its name.
        /// </summary>
        /// <remarks>
        /// Bypasses checking whether the actor type is valid, for use in cases
        /// where an extension wants to spawn something that hasn't been added
        /// to the ActorType enum.
        /// </remarks>
		public static void SpawnActor(string type, Vector3 position, Vector3 rotation = default)
        {
            Spawner.SpawnActor(new Actor(Spawner.GetFreeId(), type, position, rotation));
        }

        public static void KickPlayer(Player player)
            => Punish.KickPlayer(player);
        public static void BanPlayer(Player player)
            => Punish.BanPlayer(player);
    }
}
