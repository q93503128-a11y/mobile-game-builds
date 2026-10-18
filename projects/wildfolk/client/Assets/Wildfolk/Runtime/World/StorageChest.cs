using UnityEngine;
namespace Wildfolk
{
    public sealed class StorageChest : MonoBehaviour, IPlayerInteractable
    {
        public WorldPlaceable Placeable;
        public WorldPickup RecoveryPickup;
        public GameplaySettings Settings;
        public InventoryModel Model { get; private set; }
        public Transform Location => transform;
        public Sprite Icon => Placeable.Definition.Item.Icon;
        private PlayerInventory visitor;
        private void Awake() => Model=new InventoryModel(Placeable.Definition.StorageSlots);
        private void OnEnable() => Placeable.Health.Died+=DropContents;
        private void OnDisable() => Placeable.Health.Died-=DropContents;
        public bool CanInteract(PlayerInteraction actor) => InRange(actor.transform.position,actor.Settings.StationInteractionRange);
        public bool InRange(Vector2 actor,float range) => Placeable.Health.IsAlive && Vector2.Distance(actor,transform.position)<=range;
        public bool Use(PlayerInteraction actor) => actor.Storage.OpenChest(this);
        public void BindVisitor(PlayerInventory player) => visitor=player;
        private void DropContents()
        {
            for(var i=0;i<Model.Capacity;i++)
            {
                var stack=Model.GetSlot(i);if(stack.IsEmpty) continue;
                var pickup=PrefabPool.Spawn(RecoveryPickup,transform.position,Quaternion.identity);
                pickup.Initialize(stack.Item,stack.Count,visitor,Settings,false,stack.Charges,stack.Quality);
                Model.ConsumeSlot(i,stack.Item,stack.Count);
            }
        }
    }
}
