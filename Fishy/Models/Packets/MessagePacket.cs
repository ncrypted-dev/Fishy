using System.Numerics;
using Fishy.Utils;
using Steamworks;

namespace Fishy.Models.Packets
{
    public class MessagePacket(string message, string color = "ffffff") : FPacket
    {
        public override string Type { get; set; } = "message";
        public string Message { get; set; } = message;
        public string Color { get; set; } = color;
        public bool Local { get; set; } = false;
        public Vector3 Position { get; set; } = new Vector3(0f, 0f, 0f);
        public string Zone { get; set; } = "main_zone";
        public int Zone_Owner { get; set; } = 1;

        public override void SendPacket(string target = "all", int channel = 0, SteamId steamId = default)
        {
            base.SendPacket(target, channel, steamId);
            if (target != "all") return;
            ChatMessage chatMessage = new(SteamClient.SteamId, Message);
            ChatLogger.Log(chatMessage);
            Event.EventManager.RaiseEvent(new Event.EventArgs.ChatMessageEventArgs(SteamClient.SteamId, Message));
        }
    }

}
