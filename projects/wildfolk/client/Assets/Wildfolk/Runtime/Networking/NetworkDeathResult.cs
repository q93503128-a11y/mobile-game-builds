using System;
using System.Collections.Generic;
using UnityEngine;
namespace Wildfolk
{
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
            Items=table.Roll().AsReadOnly();
        }
    }
}
