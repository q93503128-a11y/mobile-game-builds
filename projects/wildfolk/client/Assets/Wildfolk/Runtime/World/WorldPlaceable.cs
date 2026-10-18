using UnityEngine;

namespace Wildfolk
{
    public sealed class WorldPlaceable : MonoBehaviour
    {
        public static readonly System.Collections.Generic.List<WorldPlaceable> Active=new System.Collections.Generic.List<WorldPlaceable>();
        public PlaceableDefinition Definition;
        public Health Health;
        public SpriteRenderer Visual;
        public string SaveId {get;set;}
        public string CreatorId {get;set;}
        private void Awake(){SaveId=System.Guid.NewGuid().ToString("N");if(Definition.TierVisual!=null)Visual.sprite=Definition.TierVisual;}
        private void OnEnable(){Active.Add(this);Health.Died+=DestroyPlaced;}
        private void OnDisable(){Active.Remove(this);Health.Died-=DestroyPlaced;}
        private void DestroyPlaced() => PrefabPool.Release(gameObject);
    }
}
