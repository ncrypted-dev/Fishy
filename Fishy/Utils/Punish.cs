using Fishy.Models;
using Fishy.Models.Packets;
using Steamworks;

namespace Fishy.Utils
{
    class Punish
    {
        internal static void BanPlayer(Player playerToBan)
        {
            new BanPacket().SendPacket("single", (int)CHANNELS.GAME_STATE, playerToBan.SteamID);
            new ForceDisconnectPacket(playerToBan.SteamID.Value.ToString()).SendPacket("all", (int)CHANNELS.GAME_STATE);
            Fishy.SteamHandler.UpdateSteamBanList(playerToBan.SteamID.Value.ToString());
            using StreamWriter writer = new(Path.Combine(AppContext.BaseDirectory, "bans.txt"), true);
            writer.WriteLine(playerToBan.SteamID.Value.ToString());
            Fishy.BannedUsers.Add(playerToBan.SteamID.Value.ToString());
        }

        internal static void KickPlayer(Player playerToKick)
            => new KickPacket().SendPacket("single", (int)CHANNELS.GAME_STATE, playerToKick.SteamID);

        internal static bool IsBanned(SteamId id)
        {
            return Fishy.BannedUsers.Contains(id.Value.ToString());
        }
        
    }
}
