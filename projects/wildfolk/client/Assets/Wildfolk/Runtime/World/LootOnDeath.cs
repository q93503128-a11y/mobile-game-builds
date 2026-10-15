using UnityEngine;
namespace Wildfolk
{
    public sealed class LootOnDeath : MonoBehaviour
    {
        public Health Health;
        public LootTable Table;
        public PlayerInventory Player;
        public WorldPickup Pickup;
        public GameplaySettings Settings;
        private void OnEnable()=>Health.Died+=Drop;
        private void OnDisable()=>Health.Died-=Drop;
        private void Drop()
        {
            DropItems(Table.Items);DropItems(Table.RareItems);
            if(Health.LastAttacker!=null){var reward=Health.LastAttacker.GetComponent<PlayerProgression>();if(reward!=null)reward.Reward(Table.Experience,Table.Gold);}
            PrefabPool.Release(gameObject);
        }
        private void DropItems(LootEntry[] entries)
        {
            foreach(var entry in entries)
            {
                if(entry.Chance<1 && Random.value>=entry.Chance)continue;
                var drop=PrefabPool.Spawn(Pickup,transform.position,Quaternion.identity);
                drop.Initialize(entry.Item,Random.Range(entry.Minimum,entry.Maximum+1),Player,Settings);
            }
        }
    }
}
