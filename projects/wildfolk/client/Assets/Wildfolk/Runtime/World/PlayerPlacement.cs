using UnityEngine;

namespace Wildfolk
{
    public sealed class PlayerPlacement : MonoBehaviour
    {
        public PlayerEquipment Equipment;
        public PlayerInventory Inventory;
        public GameplaySession Session;
        public CharacterFacing Facing;
        public GameplaySettings Settings;
        public SpriteRenderer Ground;
        public WorldNavigation Terrain;
        public GameObject Preview;
        public SpriteRenderer PreviewVisual;
        public SpriteRenderer PreviewFootprint;
        public Vector2 PreviewPosition { get; private set; }
        public bool PreviewValid { get; private set; }
        private readonly Collider2D[] overlaps = new Collider2D[16];

        private void LateUpdate()
        {
            var definition = Equipment.ActivePlaceable;
            if(Preview==null)return;Preview.SetActive(definition != null && Session.IsRunning);
            if (definition == null) return;
            PreviewPosition = (Vector2)transform.position + Facing.Direction * Settings.PlacementDistance;
            PreviewValid = CanPlaceAt(definition, PreviewPosition);
            Preview.transform.SetPositionAndRotation(PreviewPosition,RotationFor(definition));
            PreviewVisual.sprite = definition.Prefab.Visual.sprite;
            PreviewVisual.transform.localScale = definition.Prefab.Visual.transform.localScale;
            PreviewFootprint.transform.localScale = new Vector3(definition.Footprint.x/.08f, definition.Footprint.y/.08f, 1);
            var color = PreviewValid ? new Color(.55f,.76f,.49f,.58f) : new Color(.75f,.34f,.25f,.58f);
            PreviewVisual.color = color; color.a = .18f; PreviewFootprint.color = color;
        }
        public bool CanPlaceAt(PlaceableDefinition definition, Vector2 position)
        {
            if (definition == null || definition.Prefab == null) return false;
            if (Vector2.Distance(transform.position, position) > definition.MaximumDistance) return false;
            var angle=RotationFor(definition).eulerAngles.z;
            var footprint=angle>45 && angle<135 ? new Vector2(definition.Footprint.y,definition.Footprint.x) : definition.Footprint;
            var half = footprint*.5f;
            var bounds = Terrain!=null && Terrain.World.Map.Streamed ? Terrain.World.Map.RegionBounds(position) : Ground.bounds;
            if (!bounds.Contains(position-half) || !bounds.Contains(position+half)) return false;
            if(Terrain!=null)for(var y=-1;y<=1;y++)for(var x=-1;x<=1;x++){var sample=position+new Vector2(half.x*x,half.y*y);if(Terrain.Surface(sample)!=TerrainSurface.Land || !Terrain.World.Map.IsWalkable(sample,0))return false;}
            var filter = new ContactFilter2D { useTriggers = true };
            return Physics2D.OverlapBox(position, definition.Footprint, angle, filter, overlaps) == 0;
        }
        public Quaternion RotationFor(PlaceableDefinition definition) => definition.IsBarrier && Mathf.Abs(Facing.Direction.x)>Mathf.Abs(Facing.Direction.y) ? Quaternion.Euler(0,0,90) : Quaternion.identity;
        public bool TryPlace()
        {
            var network=GetComponent<NetworkConstruction>();if(network!=null)return network.Place();
            if (!Session.IsRunning) return false;
            var definition = Equipment.ActivePlaceable;
            var position = (Vector2)transform.position + Facing.Direction * Settings.PlacementDistance;
            if (!CanPlaceAt(definition, position)) return false;
            WorldPlaceable placed=null;
            try{return Inventory.Model.TryDropSlot(Inventory.SelectedIndex,definition.Item,1,()=>{placed=Instantiate(definition.Prefab,position,RotationFor(definition));if(placed!=null)placed.PlayerBuilt=true;return placed!=null;});}
            catch(System.Exception){if(placed!=null)Destroy(placed.gameObject);return false;}
        }
    }
}
