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
        private void OnEnable() => Health.Died += Break;
        private void OnDisable() => Health.Died -= Break;

        private void Break()
        {
            var pickup = PrefabPool.Spawn(PickupPrefab, transform.position, Quaternion.identity);
            pickup.Initialize(Definition.DropItem, Definition.DropCount, Player, Settings);
            Destroy(gameObject);
        }
    }
}
