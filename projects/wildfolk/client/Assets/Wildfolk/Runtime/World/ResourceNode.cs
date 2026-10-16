using UnityEngine;

namespace Wildfolk
{
    public sealed class ResourceNode : MonoBehaviour
    {
        public Health Health;
        public ResourceDefinition Definition;
        public WorldPickup PickupPrefab;
        public PlayerInventory Player;
        public GameplaySettings Settings;
        public SpriteRenderer Visual;
        public int Harvest(ToolDefinition tool,int damage,PlayerInventory actor)
        {
            if(!SimulationAuthority.Allows(this) || !Health.IsAlive || !Definition.CanHarvest(tool))return 0;
            return Health.ApplyDamage(damage,false,actor);
        }
        private void OnEnable() => Health.Died += Break;
        private void OnDisable() => Health.Died -= Break;

        private void Break()
        {
            var network=GetComponent<NetworkResource>();if(network!=null){network.Break();return;}
            var pickup = PrefabPool.Spawn(PickupPrefab, transform.position, Quaternion.identity);
            pickup.Initialize(Definition.DropItem, Definition.DropCount, Player, Settings);
            Destroy(gameObject);
        }
    }
}
