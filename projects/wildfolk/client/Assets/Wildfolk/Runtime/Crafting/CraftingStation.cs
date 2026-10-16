using UnityEngine;

namespace Wildfolk
{
    public sealed class CraftingStation : MonoBehaviour, IPlayerInteractable
    {
        public event System.Action Changed;
        public void NotifyChanged()=>Changed?.Invoke();
        public PlaceableDefinition Definition;
        public Health Health;
        public Transform Location => transform;
        public Sprite Icon => Definition.Item.Icon;
        public bool CanInteract(PlayerInteraction actor) => CanInteract(actor.transform.position,actor.Settings.StationInteractionRange);
        public bool Use(PlayerInteraction actor) => actor.Crafting.OpenStation(this);
        public bool CanInteract(Vector2 actorPosition,float range) =>
            Health.IsAlive && ((Vector2)transform.position-actorPosition).sqrMagnitude<=range*range;
    }
}
