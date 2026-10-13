using UnityEngine;

namespace Wildfolk
{
    public sealed class CraftingStation : MonoBehaviour
    {
        public PlaceableDefinition Definition;
        public Health Health;
        public bool CanInteract(Vector2 actorPosition,float range) =>
            Health.IsAlive && ((Vector2)transform.position-actorPosition).sqrMagnitude<=range*range;
    }
}
