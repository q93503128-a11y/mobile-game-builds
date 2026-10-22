using UnityEngine;

namespace Wildfolk
{
    public sealed class WorldPlaceable : MonoBehaviour
    {
        public static readonly System.Collections.Generic.List<WorldPlaceable> Active=new System.Collections.Generic.List<WorldPlaceable>();
        public PlaceableDefinition Definition;
        public Health Health;
        public SpriteRenderer Visual;
        private string saveId;public string SaveId {get=>saveId;set{if(Definition!=null && Definition.IsBeacon){BeaconDirectory.Remove(saveId);saveId=value;GetComponent<SignalBeacon>()?.Publish();}else saveId=value;}}
        public bool PlayerBuilt {get;set;}
        public string CreatorId {get;set;}
        private void Awake(){SaveId=System.Guid.NewGuid().ToString("N");if(Definition.TierVisual!=null)Visual.sprite=Definition.TierVisual;}
        private void OnEnable(){Active.Add(this);Health.Died+=DestroyPlaced;}
        private void OnDisable(){Active.Remove(this);Health.Died-=DestroyPlaced;}
        private void DestroyPlaced(){if(Definition.IsBeacon)BeaconDirectory.Remove(SaveId);PrefabPool.Release(gameObject);}
    }
}
