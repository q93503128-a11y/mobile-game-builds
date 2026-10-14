using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Wildfolk
{
    public enum StationKind { None, Workbench, Campfire, Furnace }
    public enum RecipeCategory { Survival, Tools, Weapons, Armor, Building, Facilities }
    public enum ProcessingKind { None, Cooking, Smelting }
    public enum UnlockKind { PlayerLevel, Skill, DiscoveredItem, Blueprint, SpecialContent }
    [Serializable]public struct UnlockRequirement {public UnlockKind Kind;public string Id;[Min(1)]public int Rank;}
    [Serializable] public struct ItemAmount
    {
        public ItemDefinition Item;
        [Min(1)] public int Count;
    }

    [CreateAssetMenu(menuName = "Wildfolk/Recipe")]
    public sealed class RecipeDefinition : ScriptableObject
    {
        public string Id;
        public RecipeCategory Category;
        public int SortOrder=-1;
        public ItemDefinition Result;
        [Min(1)] public int ResultCount;
        public ItemAmount[] Ingredients;
        public StationKind RequiredStation;
        [FormerlySerializedAs("CookingSeconds"), Min(0)] public float ProcessingSeconds;
        public ProcessingKind Processing;
        public float LegacyProcessingSeconds;
        public EquipmentTier Tier;
        [Min(1)]public int RequiredWorkbenchTier=1;
        public UnlockRequirement[] FutureUnlocks=Array.Empty<UnlockRequirement>();
    }
}
