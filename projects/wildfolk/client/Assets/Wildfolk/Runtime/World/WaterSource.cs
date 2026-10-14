using UnityEngine;
namespace Wildfolk
{
    public sealed class WaterSource : MonoBehaviour, IPlayerInteractable
    {
        public WaterSourceDefinition Definition;
        private float availableAt;
        public Transform Location => transform;
        public Sprite Icon => Definition.Container.Raw.Icon;
        public bool CanInteract(PlayerInteraction actor) => CanInteract(actor.transform.position);
        public bool Use(PlayerInteraction actor) => Collect(actor.Equipment.Inventory);
        public bool CanInteract(Vector2 actor) => Vector2.Distance(actor,transform.position)<=Definition.InteractionRange;
        public bool Collect(PlayerInventory player)
        {
            if(!CanInteract(player.transform.position) || Time.time<availableAt) return false;
            if(player.Model.CountItem(Definition.Container.Empty)<1){PlayerFeedback.ShowKey("feedback.empty_cup");return false;}
            var result=player.Model.TryExchange(new[]{new ItemAmount{Item=Definition.Container.Empty,Count=1}},Definition.Container.Raw,1);
            if(result!=ExchangeResult.Success){PlayerFeedback.ShowKey("feedback.bag_full");return false;}
            availableAt=Time.time+Definition.CollectionCooldown;return true;
        }
    }
}
