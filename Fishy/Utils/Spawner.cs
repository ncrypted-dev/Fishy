using Fishy.Models.Packets;
using Fishy.Models;
using System.Numerics;

namespace Fishy.Utils
{
    public static class Spawner
    {
        static float _rainChance = 0.0f;
        static int _alienCooldown = 16;
        static readonly string[] _baseTypes = ["fish_spawn", "none"];
        static readonly Random _random = new();

        // Ported from world.gd
        public static void Spawn()
        {
            foreach (Actor instance in Fishy.Actors.ToList())
            {
                float instanceAge = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - instance.SpawnTime.ToUnixTimeSeconds();
                if ((instance.Type == "fish_spawn_alien" && instanceAge > 120)
                    || (instance.Type == "fish_spawn" && instanceAge > 80)
                    || (instance.Type == "raincloud" && instanceAge > 550)
                    || (instance.Type == "void_portal" && instanceAge > 600))
                {
                    new ActorRemovePacket(instance.InstanceID).SendPacket("all", (int)CHANNELS.GAME_STATE);
                    Fishy.Actors.Remove(instance);
                }
            }

            int count = Fishy.Actors.Where(a => a.Type == "metal_spawn").Count();
            if (count < 7)
                SpawnMetalSpot();

            string type = _baseTypes[_random.Next(0, 2)];

            _alienCooldown -= 1;

            if (_random.NextSingle() < 0.01 && _random.NextSingle() < 0.4 && _alienCooldown <= 0)
            {
                type = "fish_spawn_alien";
                _alienCooldown = 16;
            }

            if (_random.NextSingle() < _rainChance && _random.NextSingle() < .12f)
            {
                type = "raincloud";
                _rainChance = 0;
            }
            else
                if (_random.NextSingle() < .75f) _rainChance += .001f;

            if (_random.NextSingle() < 0.01f && _random.NextSingle() < 0.25)
                type = "void_portal";

            switch (type)
            {
                case "none":
                    return;
                case "fish_spawn":
                    if (Fishy.Actors.Count > 15)
                        return;
                    SpawnFish();
                    break;
                case "fish_spawn_alien":
                    SpawnFish(type);
                    break;
                case "raincloud":
                    SpawnRainCloud();
                    break;
                case "void_portal":
                    SpawnVoidPortal();
                    break;
            }
        }

        public static void SpawnFish(string type = "fish_spawn")
        {
            int id = _random.Next();
            Vector3 pos = Fishy.World.FishSpawns[_random.Next(Fishy.World.FishSpawns.Count)];
            new ActorSpawnPacket(type, pos, id).SendPacket("all", (int)CHANNELS.GAME_STATE);
            Fishy.Actors.Add(new Actor(id, type, pos));
            if (type != "fish_spawn")
                new ActorRemovePacket(id) { Action = "_ready" }.SendPacket("all", (int)CHANNELS.GAME_STATE);
        }

        public static void SpawnRainCloud()
        {
            int id = _random.Next();
            Vector3 pos = new(_random.Next(-100, 150), 42f, _random.Next(-150, 100));
            SpawnActor(new RainCloud(id, pos));
        }

        public static void SpawnMetalSpot()
        {
            int id = _random.Next();
            Vector3 pos = Fishy.World.TrashPoints[_random.Next(Fishy.World.TrashPoints.Count)];

            if (_random.NextSingle() < .15f)
                pos = Fishy.World.ShorelinePoints[_random.Next(Fishy.World.ShorelinePoints.Count)];

            SpawnActor(new Actor(id, "metal_spawn", pos));
        }

        public static void SpawnVoidPortal()
        {
            int id = _random.Next();
            Vector3 pos = Fishy.World.HiddenSpots[_random.Next(Fishy.World.HiddenSpots.Count)];
            SpawnActor(new Actor(id, "void_portal", pos));
        }

        public static void SpawnActor(Actor actor)
        {
            new ActorSpawnPacket(actor.Type, actor.Position, actor.InstanceID).SendPacket("all", (int)CHANNELS.GAME_STATE);
            Fishy.Actors.Add(actor);
            if (actor.Rotation == default)
                return;
            new ActorUpdatePacket(actor.InstanceID, actor.Position, actor.Rotation).SendPacket("all", (int)CHANNELS.GAME_STATE);

        }
        public static void SpawnActor(int ID, string Type, Vector3 position, Vector3 entRot = default)
        {
            SpawnActor( new Actor(ID, Type, position, entRot));
        }
    }
}
