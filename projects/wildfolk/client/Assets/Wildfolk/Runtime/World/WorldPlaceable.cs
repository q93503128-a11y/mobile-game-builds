using UnityEngine;

namespace Wildfolk
{
    public sealed class WorldPlaceable : MonoBehaviour
    {
        public PlaceableDefinition Definition;
        public Health Health;
        public SpriteRenderer Visual;
        private void OnEnable() => Health.Died += DestroyPlaced;
        private void OnDisable() => Health.Died -= DestroyPlaced;
        private void DestroyPlaced() => Destroy(gameObject);
    }
}
