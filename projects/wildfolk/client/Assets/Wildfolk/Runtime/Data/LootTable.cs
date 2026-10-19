using System;
using System.Collections.Generic;
using UnityEngine;
namespace Wildfolk
{
    public readonly struct PendingLoot
    {
        public readonly ItemDefinition Item;public readonly int Count;
        public PendingLoot(ItemDefinition item,int count){Item=item;Count=count;}
    }
    [Serializable] public struct LootEntry {public ItemDefinition Item;[Range(0,1)]public float Chance;[Min(1)]public int Minimum,Maximum;}
    [CreateAssetMenu(menuName="Wildfolk/Loot Table")]
    public sealed class LootTable : ScriptableObject
    {
        public LootEntry[] Items=Array.Empty<LootEntry>();
        public LootEntry[] RareItems=Array.Empty<LootEntry>();
        [Min(0)]public int Experience,Gold;
        public List<PendingLoot> Roll()
        {var result=new List<PendingLoot>();Roll(Items,result);Roll(RareItems,result);return result;}
        private static void Roll(LootEntry[] entries,List<PendingLoot> result)
        {foreach(var e in entries)if(e.Chance>=1 || UnityEngine.Random.value<e.Chance)result.Add(new PendingLoot(e.Item,UnityEngine.Random.Range(e.Minimum,e.Maximum+1)));}
    }
}
