using System;
using UnityEngine;
namespace Wildfolk
{
    [Serializable] public struct LootEntry {public ItemDefinition Item;[Range(0,1)]public float Chance;[Min(1)]public int Minimum,Maximum;}
    [CreateAssetMenu(menuName="Wildfolk/Loot Table")]
    public sealed class LootTable : ScriptableObject
    {
        public LootEntry[] Items=Array.Empty<LootEntry>();
        public LootEntry[] RareItems=Array.Empty<LootEntry>();
        [Min(0)]public int Experience,Gold;
    }
}
