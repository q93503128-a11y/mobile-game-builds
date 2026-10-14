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
            foreach(var entry in Table.Items)
            {
                if(entry.Chance<1 && Random.value>=entry.Chance)continue;
                var drop=PrefabPool.Spawn(Pickup,transform.position,Quaternion.identity);
                drop.Initialize(entry.Item,Random.Range(entry.Minimum,entry.Maximum+1),Player,Settings);
            }
            if(Health.LastAttacker!=null)Health.LastAttacker.GetComponent<PlayerProgression>().Reward(Table.Experience,Table.Gold);
            PrefabPool.Release(gameObject);
        }
    }
}
