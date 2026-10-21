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
        private void Update() => Refresh();
        private void Refresh()
        {
            IPlayerInteractable target=null;var nearest=float.MaxValue;
            var count=Physics2D.OverlapCircle(transform.position,Settings.StationInteractionRange,new ContactFilter2D{useTriggers=true},candidates);
            for(var i=0;i<count;i++)
            {
                var candidate=candidates[i].GetComponentInParent<IPlayerInteractable>();
                if(candidate==null || !candidate.CanInteract(this))continue;
                if(GetComponent<NetworkPlayer>()!=null && candidate.Location.GetComponent<NetworkBuilding>()==null && candidate.Location.GetComponentInParent<NetworkCamp>()==null && candidate.Location.GetComponentInParent<NetworkPoiSite>()==null)continue;
                var delta=(Vector2)candidate.Location.position-(Vector2)transform.position;
                if(delta.sqrMagnitude>.001f && Vector2.Dot(Facing.Direction,delta.normalized)<.35f)continue;
                if(delta.sqrMagnitude>=nearest)continue;nearest=delta.sqrMagnitude;target=candidate;
            }
            if(target==Target)return;Target=target;Changed?.Invoke();
        }
        public bool CanEquipSelected=>Equipment.SelectedItem!=null && (Equipment.SelectedItem.Armor!=null || Equipment.SelectedItem.EquippedLight!=null);
        public bool TryInteract() {if(!Session.IsRunning || GetComponent<PlayerInputCommands>().IsScouting)return false;Refresh();if(Target is PoiSite site && site.Kind==WorldAnchorKind.Well)return site.Use(this);if(Equipment.SelectedItem!=null && Equipment.SelectedItem.WaterCapacity>0){var net=GetComponent<NetworkContainer>();return net==null ? GetComponent<PlayerWaterskin>().Fill() : net.Fill();}if(CanEquipSelected)return Equipment.TryEquipSelectedArmor();Refresh();return Target!=null && Target.Use(this);}
        public void Use() => TryInteract();
    }
}
