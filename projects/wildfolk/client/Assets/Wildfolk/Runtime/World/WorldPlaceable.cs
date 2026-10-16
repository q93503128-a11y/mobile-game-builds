using UnityEngine;

namespace Wildfolk
{
    public sealed class WorldPlaceable : MonoBehaviour
    {
        public PlaceableDefinition Definition;
        public Health Health;
        public SpriteRenderer Visual;
        public string SaveId {get;set;}
        public string CreatorId {get;set;}
        private void Awake(){SaveId=System.Guid.NewGuid().ToString("N");if(Definition.TierVisual!=null)Visual.sprite=Definition.TierVisual;}
        private void OnEnable() => Health.Died += DestroyPlaced;
        private void OnDisable() => Health.Died -= DestroyPlaced;
        private void DestroyPlaced() => PrefabPool.Release(gameObject);
    }
}
