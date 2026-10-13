using UnityEngine;

namespace Wildfolk
{
    public sealed class CraftingStation : MonoBehaviour, IPlayerInteractable
    {
        public PlaceableDefinition Definition;
        public Health Health;
        public Transform Location => transform;
        public Sprite Icon => Definition.Item.Icon;
        public PlayerActionKind ActionKind => PlayerActionKind.Interact;
        public bool CanInteract(PlayerAction actor) => CanInteract(actor.transform.position,actor.Settings.StationInteractionRange);
        public bool Use(PlayerAction actor) => actor.Crafting.OpenStation(this);
        public bool CanInteract(Vector2 actorPosition,float range) =>
            Health.IsAlive && ((Vector2)transform.position-actorPosition).sqrMagnitude<=range*range;
    }
}
