using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Wildfolk
{
    public enum StationKind { None, Workbench, Campfire, Furnace }
    public enum ProcessingKind { None, Cooking, Smelting }
    [Serializable] public struct ItemAmount
    {
        public ItemDefinition Item;
        [Min(1)] public int Count;
    }

    [CreateAssetMenu(menuName = "Wildfolk/Recipe")]
    public sealed class RecipeDefinition : ScriptableObject
    {
        public string Id;
        public ItemDefinition Result;
        [Min(1)] public int ResultCount;
        public ItemAmount[] Ingredients;
        public StationKind RequiredStation;
        [FormerlySerializedAs("CookingSeconds"), Min(0)] public float ProcessingSeconds;
        public ProcessingKind Processing;
        public EquipmentTier Tier;
    }
}
