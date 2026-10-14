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
    {public string Id;public int Rule,HP;public Vector2 Position,Home;public bool Missing;public double RespawnAt;}
    [Serializable] public sealed class ProcessingSnapshot
    {public string Recipe;public int Batch;public float Remaining;}
    [Serializable] public sealed class PlaceableSnapshot
    {public string Id,Item;public Vector2 Position;public float Rotation;public int HP;public bool DoorOpen;public SavedStack[] Inventory;public ProcessingSnapshot Processing;}
    [Serializable] public sealed class PickupSnapshot
    {public string Item;public int Count;public Vector2 Position;}
    [Serializable] public sealed class SavedGame
    {
        public int Version=2,Seed;
        public double Elapsed;
        public PlayerSnapshot Player;
        public ResourceSnapshot[] Resources=Array.Empty<ResourceSnapshot>();
        public AnimalSnapshot[] Animals=Array.Empty<AnimalSnapshot>();
        public PlaceableSnapshot[] Placeables=Array.Empty<PlaceableSnapshot>();
        public PickupSnapshot[] Pickups=Array.Empty<PickupSnapshot>();
    }
}
