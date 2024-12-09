using Fishy.Chat;
using Fishy.Event;
using Fishy.Extensions;
using Fishy.Helper;
using Fishy.Models;
using Fishy.Models.Packets;
using Steamworks;
using Steamworks.Data;
using System.Numerics;

namespace Fishy.Utils
{
    public enum CHANNELS
    {
        ACTOR_UPDATE,
        ACTOR_ACTION,
        GAME_STATE,
        CHALK,
        GUITAR,
        ACTOR_ANIMATION,
        SPEECH,
    }

    class NetworkHandler
    {
        public static Dictionary<int, Vector3> PreviousPositions = [];
        private static int _actorUpdateCount = 30;

        public void Start()
        {
            Thread cbThread = new(RunSteamworksUpdate)
            {
                IsBackground = true
            };
            cbThread.Start();

            Thread thread = new(Listen)
            {
                IsBackground = true
            };
            thread.Start();

            static void requestPlayerPings() => new PingPacket().SendPacket("all", (int)CHANNELS.GAME_STATE);
            ScheduledTask pingTask = new(requestPlayerPings, 5000);
            pingTask.Start();

            ScheduledTask spawnTask = new(Spawner.SpawnIngameEvent, 10000);
            spawnTask.Start();

            ScheduledTask updateTask = new(Update, 100);
            updateTask.Start();
        }

        void Listen()
        {
            while (true)
            {
                for (int i = 0; i < 6; i++)
                {
                    if (!SteamNetworking.IsP2PPacketAvailable(i))
                        continue;

                    P2Packet? packet = SteamNetworking.ReadP2PPacket(i);

                    if (packet != null)
                        OnPacketReceived(packet.Value);
                }
            }
        }

        void RunSteamworksUpdate()
        {
            while (true)
                SteamClient.RunCallbacks();
        }

        public static void OnPacketReceived(P2Packet packet)
        {

            try
            {
                byte[] packetData = GZip.Decompress(packet.Data);
                Dictionary<string, object> packetInfo = FPacket.FromBytes(packetData);
                if (!packetInfo.TryGetValue("type", out object? value))
                    return;
                string packetType = (string)value;

                if (Fishy.BannedUsers.Contains(packet.SteamId.Value.ToString()))
                    return;

                switch (packetType)
                {
                    case "handshake":
                        new HandshakePacket().SendPacket("single", (int)CHANNELS.GAME_STATE, packet.SteamId);
                        break;
                    case "request_ping":
                        new PongPacket().SendPacket("single", (int)CHANNELS.ACTOR_ACTION, packet.SteamId);
                        break;
                    case "new_player_join":
                        new MessagePacket("Welcome to the server!").SendPacket("single", (int)CHANNELS.GAME_STATE, packet.SteamId);
                        new MessagePacket(Fishy.Config.JoinMessage).SendPacket("single", (int)CHANNELS.GAME_STATE, packet.SteamId);
                        if (Fishy.Config.Admins.Contains(packet.SteamId.Value.ToString()))
                            new MessagePacket("An admin has joined the lobby").SendPacket("all", (int)CHANNELS.GAME_STATE);
                        SendCanvasData(packet.SteamId);
                        new HostPacket().SendPacket("all", (int)CHANNELS.GAME_STATE);
                        break;
                    case "instance_actor":
                        Dictionary<string, object> parameters = (Dictionary<string, object>)packetInfo["params"];
                        if (parameters["actor_type"].ToString() == "player")
                        {
                            int index = Fishy.Players.FindIndex(p => p.SteamID.Equals(packet.SteamId));
                            if (index == -1)
                                break;
                            Fishy.Players[index].InstanceID = (long)parameters["actor_id"];
                        }
                        break;
                    case "actor_update":
                        int playerIndex = Fishy.Players.FindIndex(p => p.InstanceID.Equals(packetInfo["actor_id"]));
                        if (playerIndex == -1)
                            break;
                        Fishy.Players[playerIndex].Position = (Vector3)packetInfo["pos"];
                        break;

                    case "actor_action":
                        string packetAction = (string)packetInfo["action"];
                        if (packetAction == "_sync_create_bubble")
                        {
                            string Message = (string)((Dictionary<int, object>)packetInfo["params"])[0];
                            OnChat(Message, packet.SteamId);
                        }
                        if ((string)packetInfo["action"] == "_wipe_actor")
                        {
                            long actorToWipe = (long)((Dictionary<int, object>)packetInfo["params"])[0];
                            Actor serverInst = Fishy.Actors.First(i => i.InstanceID == actorToWipe);
                            if (serverInst != null)
                            {
                                RemoveServerActor(serverInst);
                            }
                        }
                        EventManager.RaiseEvent(new Event.Events.ActorActionEventArgs(packet.SteamId, packetAction));
                        break;
                    case "request_actors":
                        List<Actor> instances = Fishy.Actors;
                        foreach (Actor actor in instances)
                        {
                            new ActorSpawnPacket(actor.Type, actor.Position, actor.InstanceID).SendPacket("single", (int)CHANNELS.GAME_STATE, packet.SteamId);
                        }

                        new ActorRequestPacket().SendPacket("single", (int)CHANNELS.GAME_STATE, packet.SteamId);
                        break;
                    case "letter_recieved":
                        Dictionary<string, object> data = (Dictionary<string, object>)packetInfo["data"];
                        string body = data["body"].ToString() ?? "";
                        CommandHandler.OnMessage(packet.SteamId, body);
                        break;
                    case "chalk_packet":
                        var ChalkPacketData = (Dictionary<int, object>)packetInfo["data"];
                        int CanvasID = Convert.ToInt32(packetInfo["canvas_id"]);

                        while (Fishy.CanvasData.Count <= CanvasID)
                        {
                            Fishy.CanvasData.Add(null);
                        }

                        if (Fishy.CanvasData[CanvasID] == null)
                            Fishy.CanvasData[CanvasID] = new Dictionary<Vector2, int>();

                        foreach (var DataPointObj in ChalkPacketData.Values)
                        {
                            Dictionary<int, object> DataPoint = (Dictionary<int, object>)DataPointObj;
                            var chalkLocation = (Vector2)DataPoint[0];
                            var chalkColor = Convert.ToInt32(DataPoint[1]);

                            if (!Fishy.CanvasData[CanvasID].ContainsKey(chalkLocation))
                                Fishy.CanvasData[CanvasID].Add(chalkLocation, chalkColor);
                        }
                        break;
                    default: break;

                }
            } catch (Exception ex) {
                Console.WriteLine(DateTime.Now.ToString("dd.MM HH:mm:ss") + " Error: " + ex.Message);
            }
            
        }

        static void RemoveServerActor(Actor instance)
        {
            new ActorRemovePacket(instance.InstanceID).SendPacket("all", (int)CHANNELS.GAME_STATE);
            Fishy.Actors.Remove(instance);
        }


        static void OnChat(string message, SteamId id)
        {
            ChatLogger.Log(new ChatMessage(id, message));

            if (CommandHandler.OnMessage(id, message)) return; // Suppress message if command ran

            EventManager.RaiseEvent(new Event.Events.ChatMessageEventArgs(id,message));
        }


        public static void Update()
        {
            _actorUpdateCount++;
            foreach (Actor actor in Fishy.Actors.ToList())
            {
                actor.OnUpdate();

                if (!PreviousPositions.ContainsKey(actor.InstanceID))
                    PreviousPositions[actor.InstanceID] = Vector3.Zero;

                if (actor.Position != PreviousPositions[actor.InstanceID] && _actorUpdateCount == 20)
                {
                    PreviousPositions[actor.InstanceID] = actor.Position;
                    new ActorUpdatePacket(actor.InstanceID, actor.Position, actor.Rotation).SendPacket("all", (int)CHANNELS.GAME_STATE);
                }
            }

            if (_actorUpdateCount >= 20)
                _actorUpdateCount = 0;
        }


        static void SendCanvasData(SteamId id)
        {
            int i = 0;

            foreach(Dictionary<Vector2, int> Canvas in Fishy.CanvasData)
            {
                if (Canvas == null)
                {
                    i++;
                    continue;
                }
                
                new ChalkPacket(i, Canvas).SendPacket("single", (int)CHANNELS.CHALK, id);

                i++;
            }
        }
    }
}