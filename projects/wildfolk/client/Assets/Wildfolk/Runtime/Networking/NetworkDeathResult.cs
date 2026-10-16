using System;
using System.Collections.Generic;
using UnityEngine;
namespace Wildfolk
{
    public readonly struct PendingLoot
    {
        public readonly ItemDefinition Item;
        public readonly int Count;
        public PendingLoot(ItemDefinition item,int count){Item=item;Count=count;}
    }
    public sealed class NetworkDeathResult
    {
        private bool committed;
        internal bool TryCommit(){if(committed)return false;committed=true;return true;}
        public ulong CreatureId {get;}
        public ulong AttackerId {get;}
        public IReadOnlyList<PendingLoot> Items {get;}
        public int Experience {get;}
        public int Gold {get;}
        public NetworkDeathResult(ulong creature,ulong attacker,LootTable table)
        {
            CreatureId=creature;AttackerId=attacker;Experience=table.Experience;Gold=table.Gold;
            var items=new List<PendingLoot>();Roll(table.Items,items);Roll(table.RareItems,items);Items=items.AsReadOnly();
        }
        private static void Roll(LootEntry[] entries,List<PendingLoot> result)
        {foreach(var e in entries)if(e.Chance>=1 || UnityEngine.Random.value<e.Chance)result.Add(new PendingLoot(e.Item,UnityEngine.Random.Range(e.Minimum,e.Maximum+1)));}
    }
}
