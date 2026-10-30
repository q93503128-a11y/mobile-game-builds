using System;
using UnityEngine;
namespace Wildfolk
{
    public sealed class PlayerInteraction : MonoBehaviour
    {
        public PlayerEquipment Equipment;
        public PlayerCrafting Crafting;
        public PlayerStorage Storage;
        public GameplaySession Session;
        public GameplaySettings Settings;
        public CharacterFacing Facing;
        public IPlayerInteractable Target { get; private set; }
        public CraftingStation TargetStation => Target as CraftingStation;
        public WaterSource TargetWater => Target as WaterSource;
        public event Action Changed;
        private readonly Collider2D[] candidates=new Collider2D[64];
        private WorldGenerator world;
        private void Update() => Refresh();
        private void Refresh()
        {
            IPlayerInteractable target=null;var nearest=float.MaxValue;var waterRange=-1f;var facesWater=false;
            var count=Physics2D.OverlapCircle(transform.position,Settings.StationInteractionRange,new ContactFilter2D{useTriggers=true},candidates);
            for(var i=0;i<count;i++)
            {
                var candidate=candidates[i].GetComponentInParent<IPlayerInteractable>();
                if(candidate==null || !candidate.CanInteract(this))continue;
                if(GetComponent<NetworkPlayer>()!=null && candidate.Location.GetComponent<NetworkBuilding>()==null && candidate.Location.GetComponentInParent<NetworkCamp>()==null && candidate.Location.GetComponentInParent<NetworkPoiSite>()==null)continue;
                var delta=(Vector2)candidate.Location.position-(Vector2)transform.position;
                if(candidate is WaterSource water)
                {if(waterRange!=water.Definition.InteractionRange){waterRange=water.Definition.InteractionRange;facesWater=FacesWater(waterRange);}if(!facesWater)continue;}
                else if(delta.sqrMagnitude>.001f && Vector2.Dot(Facing.Direction,delta.normalized)<.35f)continue;
                if(delta.sqrMagnitude>=nearest)continue;nearest=delta.sqrMagnitude;target=candidate;
            }
            if(target==Target)return;Target=target;Changed?.Invoke();
        }
        private bool FacesWater(float range)
        {
            if(world==null)world=FindFirstObjectByType<WorldGenerator>();
            if(world==null || world.Map==null)return false;
            var origin=(Vector2)transform.position;if(world.Map.IsWater(origin))return true;
            // Shore triggers sit on land; face the nearby water boundary, not a trigger behind the player.
            var gradient=new Vector2(world.Map.WaterDistance(origin+Vector2.right*.25f)-world.Map.WaterDistance(origin-Vector2.right*.25f),
                world.Map.WaterDistance(origin+Vector2.up*.25f)-world.Map.WaterDistance(origin-Vector2.up*.25f));
            var direction=-gradient.normalized;if(direction.sqrMagnitude<.001f || Vector2.Dot(Facing.Direction,direction)<.35f)return false;
            for(var distance=.25f;distance<range;distance+=.25f)
            {var point=origin+direction*distance;if(world.Map.Contains(point) && world.Map.IsWater(point))return true;}
            var end=origin+direction*range;return world.Map.Contains(end) && world.Map.IsWater(end);
        }
        public bool CanEquipSelected=>Equipment.SelectedItem!=null && (Equipment.SelectedItem.Armor!=null || Equipment.SelectedItem.EquippedLight!=null);
        public bool UsesSelectedWaterContainer=>Equipment.SelectedItem!=null && Equipment.SelectedItem.WaterCapacity>0 && (Target==null || Target is WaterSource);
        public bool TryInteract() {if(GetComponent<PlayerInputCommands>().HasDownedTeammate)return GetComponent<PlayerInputCommands>().HoldInteract(true);if(!Session.IsRunning || GetComponent<PlayerInputCommands>().IsScouting)return false;Refresh();if(Target is PoiSite site && site.Kind==WorldAnchorKind.Well)return site.Use(this);if(UsesSelectedWaterContainer){var net=GetComponent<NetworkContainer>();return net==null ? GetComponent<PlayerWaterskin>().Fill() : net.Fill();}if(CanEquipSelected)return Equipment.TryEquipSelectedArmor();Refresh();return Target!=null && Target.Use(this);}
        public void Use() => TryInteract();
    }
}
