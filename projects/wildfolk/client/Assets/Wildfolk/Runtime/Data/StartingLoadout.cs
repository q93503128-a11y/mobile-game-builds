using System;
using UnityEngine;

namespace Wildfolk
{
    [Serializable] public struct StartingItem
    {
        public ItemDefinition Item;
        [Min(1)] public int Count;
    }

    [CreateAssetMenu(menuName = "Wildfolk/Starting Loadout")]
    public sealed class StartingLoadout : ScriptableObject
    {
        public StartingItem[] Items;
    }
}
