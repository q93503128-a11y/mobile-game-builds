using UnityEngine;
namespace Wildfolk
{
    public sealed class PlayerSurvival : MonoBehaviour
    {
        public SurvivalDefinition Definition;
        public Health Health;
        public GameplaySession Session;
        public PlayerInventory Inventory;
        public SurvivalModel Model { get; private set; }
        private void Awake() => Model=new SurvivalModel(Definition);
        private void Update()
        {
            if(!Health.IsAlive || !Session.WorldIsRunning) return;
            var damage=Model.Advance(Time.deltaTime);
            if(damage>0) Health.ApplyDamage(damage);
        }
        public bool TryConsume()
        {
            var stack=Inventory.SelectedStack;
            if(!Session.IsRunning || stack.IsEmpty || !Model.CanRestore(stack.Item.Consumable)) return false;
            var food=stack.Item.Consumable;
            if(!Inventory.Model.ConsumeSlot(Inventory.SelectedIndex,stack.Item,1)) return false;
            Model.Restore(food);return true;
        }
    }
}
