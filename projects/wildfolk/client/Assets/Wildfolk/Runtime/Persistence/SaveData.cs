using System;
using UnityEngine;
namespace Wildfolk
{
    [Serializable] public sealed class SavedStack {public string Item;public int Count;}
    [Serializable] public sealed class SurvivalSnapshot
    {public float Hunger,Thirst,HungerClock,ThirstClock,RecoveryBlock,DrainPenalty;public float DrainMultiplier=1;}
    [Serializable] public sealed class SavedSkill {public string Id;public int Rank;}
    [Serializable] public sealed class GrowthSnapshot {public int Level,Points;public long CurrentXP;public SavedSkill[] Skills;}
    [Serializable] public sealed class PlayerSnapshot
    {public Vector2 Position;public int HP,Selected;public float CombatRecovery;public SurvivalSnapshot Survival;public SavedStack[] Inventory;public long XP,Gold;public string HeadArmor,BodyArmor;public GrowthSnapshot Growth;public float Stamina,StaminaWait;public string[] SeenRecipes=Array.Empty<string>();}
    [Serializable] public sealed class ResourceSnapshot
    {public string Id;public int Rule,HP;public Vector2 Position;public float Scale;public bool Destroyed;public double RespawnAt;}
    [Serializable] public sealed class AnimalSnapshot
    {public string Id,Species,GroupId;public int Rule,HP,Region;public Vector2 Position,Home;public bool Missing;public double RespawnAt;}
    [Serializable]public sealed class AnimalEcologySnapshot {public int[] InitializedRegions=Array.Empty<int>();public int Serial,GroupSerial,Cursor;public RecoveryTicket[] Pending=Array.Empty<RecoveryTicket>();}
    [Serializable]public sealed class GoblinActorSnapshot {public string Id,GroupId,CampId;public bool Ranged;public Vector2 Position,Home;public int HP;}
    [Serializable]public sealed class CampSnapshot {public string Id;public Vector2 Position;public double RefillAt;}
    [Serializable]public sealed class GoblinEcologySnapshot {public int Serial,GroupSerial;public CampSnapshot[] Camps=Array.Empty<CampSnapshot>();public GoblinActorSnapshot[] Actors=Array.Empty<GoblinActorSnapshot>();}
    [Serializable]public sealed class NightCreatureSnapshot {public string Species,Variant,Feature;public Vector2 Position;public int HP;}
    [Serializable] public sealed class ProcessingSnapshot
    {public string Recipe;public int Batch;public float Remaining;public bool Sequential;public ProcessingEntry[] Waiting=Array.Empty<ProcessingEntry>();public ProcessingEntry[] Output=Array.Empty<ProcessingEntry>();}
    [Serializable] public sealed class RecoveryTicket {public int Rule,Region;public double Due;}
    [Serializable] public sealed class HarvestExclusion {public int Rule;public Vector2 Position;public double Until;}
    [Serializable] public sealed class ResourceRecoverySnapshot {public int Serial,CandidateCursor;public RecoveryTicket[] Pending=Array.Empty<RecoveryTicket>();public HarvestExclusion[] Harvests=Array.Empty<HarvestExclusion>();}
    [Serializable] public sealed class ProcessingEntry {public string Recipe;public int Count;}
    [Serializable] public sealed class PlaceableSnapshot
    {public string Id,Item,Creator;public double SimulatedAt;public Vector2 Position;public float Rotation;public int HP;public bool DoorOpen;public SavedStack[] Inventory;public ProcessingSnapshot Processing;}
    [Serializable] public sealed class PickupSnapshot
    {public string Item;public int Count;public Vector2 Position;}
    [Serializable] public sealed class WorldFeatureSnapshot {public string Id;public bool Visited,Cleared;}
    [Serializable] public sealed class SavedGame
    {
        public int Version=7,Seed;
        public int BaseGenerationVersion=1;
        public WorldFeatureSnapshot[] Features=Array.Empty<WorldFeatureSnapshot>();
        public bool IsMultiplayer;
        public MultiplayerSnapshot Multiplayer;
        public double Elapsed;
        public PlayerSnapshot Player;
        public ResourceSnapshot[] Resources=Array.Empty<ResourceSnapshot>();
        public ResourceRecoverySnapshot Recovery;
        public AnimalSnapshot[] Animals=Array.Empty<AnimalSnapshot>();
        public AnimalEcologySnapshot AnimalEcology;
        public GoblinEcologySnapshot GoblinEcology;
        public NightCreatureSnapshot[] NightCreatures=Array.Empty<NightCreatureSnapshot>();
        public PlaceableSnapshot[] Placeables=Array.Empty<PlaceableSnapshot>();
        public PickupSnapshot[] Pickups=Array.Empty<PickupSnapshot>();
    }
}
