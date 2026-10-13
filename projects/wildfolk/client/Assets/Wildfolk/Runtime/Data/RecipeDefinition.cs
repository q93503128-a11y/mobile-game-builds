using System;
using UnityEngine;

namespace Wildfolk
{
    public enum StationKind { None, Workbench, Campfire }
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
        [Min(0)] public float CookingSeconds;
    }
}
