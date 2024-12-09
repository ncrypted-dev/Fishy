using Fishy.Models.Packets;
using Fishy.Models;
using System.Numerics;

namespace Fishy.Utils
{
    public static class Spawner
    {
        static float _rainChance = 0.0f;
        static int _alienCooldown = 16;
        static readonly ActorType[] _baseTypes = [ActorType.FISH_SPAWN, ActorType.NONE];
        static readonly Random _random = new();

        // Vanilla spawn routine, ported from world.gd
        public static void SpawnIngameEvent()
        {
            foreach (Actor instance in Fishy.Actors.ToList())
            {
                float instanceAge = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - instance.SpawnTime.ToUnixTimeSeconds();
                if ((instance.Type == ActorType.FISH_SPAWN_ALIEN && instanceAge > 120)
                    || (instance.Type == ActorType.FISH_SPAWN && instanceAge > 80)
                    || (instance.Type == ActorType.RAINCLOUD && instanceAge > 550)
                    || (instance.Type == ActorType.VOID_PORTAL && instanceAge > 600))
                {
                    RemoveActor(instance);
                }
            }

            int count = Fishy.Actors.Where(a => a.Type == ActorType.METAL_SPAWN).Count();
            if (count < 7)
                VanillaSpawn(ActorType.METAL_SPAWN);

            ActorType type = _baseTypes[_random.Next(0, 2)];

            _alienCooldown -= 1;

            if (_random.NextSingle() < 0.01 && _random.NextSingle() < 0.4 && _alienCooldown <= 0)
            {
                type = ActorType.FISH_SPAWN_ALIEN;
                _alienCooldown = 16;
            }

            if (_random.NextSingle() < _rainChance && _random.NextSingle() < .12f)
            {
                type = ActorType.RAINCLOUD;
                _rainChance = 0;
            }
            else
                if (_random.NextSingle() < .75f) _rainChance += .001f;

            if (_random.NextSingle() < 0.01f && _random.NextSingle() < 0.25)
                type = ActorType.VOID_PORTAL;

            if (type == ActorType.NONE || (type == ActorType.FISH_SPAWN && Fishy.Actors.Count > 15))
                return;

            VanillaSpawn(type);
        }

        // Spawns an actor according to vanilla spawning mechanics
        public static void VanillaSpawn(ActorType type)
        {
            int id = GetFreeId();
            Vector3 pos;

            switch (type)
            {
                case ActorType.FISH_SPAWN:
                case ActorType.FISH_SPAWN_ALIEN:
                    pos = Fishy.World.FishSpawns[_random.Next(Fishy.World.FishSpawns.Count)];
                    SpawnActor(new Actor(id, type, pos));

                    if (type != ActorType.FISH_SPAWN)
                        new ActorRemovePacket(id) { Action = "_ready" }.SendPacket("all", (int)CHANNELS.GAME_STATE);

                    break;

                case ActorType.RAINCLOUD:
                    pos = new(_random.Next(-100, 150), 42f, _random.Next(-150, 100));
                    SpawnActor(new RainCloud(GetFreeId(), pos));
                    break;

                case ActorType.METAL_SPAWN:
                    pos = Fishy.World.TrashPoints[_random.Next(Fishy.World.TrashPoints.Count)];
                    if (_random.NextSingle() < .15f)
                        pos = Fishy.World.ShorelinePoints[_random.Next(Fishy.World.ShorelinePoints.Count)];

                    SpawnActor(new Actor(GetFreeId(), ActorType.METAL_SPAWN, pos));
                    break;

                case ActorType.VOID_PORTAL:
                    pos = Fishy.World.HiddenSpots[_random.Next(Fishy.World.HiddenSpots.Count)];
                    SpawnActor(new Actor(GetFreeId(), ActorType.VOID_PORTAL, pos));
                    break;

                default:
                    break;
            }
        }

        public static int GetFreeId()
        {
            int id = _random.Next();
            while (Fishy.Actors.Select(f => f.InstanceID).Contains(id))
                id = _random.Next();

            return id;
        }

        public static void SpawnActor(Actor actor)
        {
            new ActorSpawnPacket(actor.Type, actor.Position, actor.InstanceID).SendPacket("all", (int)CHANNELS.GAME_STATE);
            if (!Fishy.Actors.Contains(actor))
                Fishy.Actors.Add(actor);
            Event.EventManager.RaiseEvent(new Event.Events.ActorSpawnEventArgs(actor));
            if (actor.Rotation == default)
                return;
            new ActorUpdatePacket(actor.InstanceID, actor.Position, actor.Rotation).SendPacket("all", (int)CHANNELS.GAME_STATE);

        }
        public static Actor SpawnActor(ActorType type, Vector3 position, Vector3 entRot = default)
        {
            return SpawnActor(type.ToString().ToLower(), position, entRot);
        }
        public static Actor SpawnActor(string type, Vector3 position, Vector3 entRot = default)
        {
            var actor = new Actor(GetFreeId(), type, position, entRot);
            SpawnActor(actor);
            return actor;
        }

        public static void RemoveActor(Actor actor)
        {
            new ActorRemovePacket(actor.InstanceID).SendPacket("all", (int)CHANNELS.GAME_STATE);
            Event.EventManager.RaiseEvent(new Event.Events.ActorDespawnEventArgs(actor));
            Fishy.Actors.Remove(actor);
        }

        public static void RemoveActor(int ID)
        {
            new ActorRemovePacket(ID).SendPacket("all", (int)CHANNELS.GAME_STATE);
            for (int i=0; i<Fishy.Actors.Count; i++)
            {
                if (i >= Fishy.Actors.Count)
                    return;
                var actor = Fishy.Actors[i];
                if (actor.InstanceID != ID)
                    continue;
                Fishy.Actors.Remove(actor);
                return;
            }
        }
    }
 }
