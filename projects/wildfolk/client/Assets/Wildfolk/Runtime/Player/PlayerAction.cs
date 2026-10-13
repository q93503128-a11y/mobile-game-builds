using System;
using UnityEngine;
namespace Wildfolk
{
    public enum PlayerActionKind { Attack, Place, Interact, Consume, CollectWater }
    public sealed class PlayerAction : MonoBehaviour
    {
        public PlayerEquipment Equipment;
        public PlayerCombat Combat;
        public PlayerPlacement Placement;
        public PlayerCrafting Crafting;
        public PlayerSurvival Survival;
        public GameAudio Audio;
        public GameplaySession Session;
        public GameplaySettings Settings;
        public CharacterFacing Facing;
        public PlayerActionKind Kind { get; private set; }
        public CraftingStation TargetStation { get; private set; }
        public WaterSource TargetWater { get; private set; }
        public event Action Changed;
        private ItemDefinition previousItem;
        private readonly Collider2D[] candidates=new Collider2D[16];
        private void Update() => RefreshContext();
        private void RefreshContext()
        {
            CraftingStation station=null;WaterSource water=null;
            if(Equipment.ActivePlaceable==null && (Equipment.SelectedItem==null || Equipment.SelectedItem.Consumable==null))
            {
                var count=Physics2D.OverlapCircle(transform.position,Settings.StationInteractionRange,new ContactFilter2D {useTriggers=true},candidates);
                var nearest=float.MaxValue;
                for(var i=0;i<count;i++)
                {
                    var s=candidates[i].GetComponentInParent<CraftingStation>();
                    var w=candidates[i].GetComponentInParent<WaterSource>();
                    if(s==null && w==null) continue;
                    if(s!=null && !s.CanInteract(transform.position,Settings.StationInteractionRange)) continue;
                    if(w!=null && !w.CanInteract(transform.position)) continue;
                    var target=s!=null ? s.transform : w.transform;
                    var delta=(Vector2)target.position-(Vector2)transform.position;
                    if(Vector2.Dot(Facing.Direction,delta.normalized)<.35f || delta.sqrMagnitude>=nearest) continue;
                    station=s;water=w;nearest=delta.sqrMagnitude;
                }
            }
            var kind=Equipment.ActivePlaceable!=null ? PlayerActionKind.Place :
                Equipment.SelectedItem!=null && Equipment.SelectedItem.Consumable!=null ? PlayerActionKind.Consume :
                station!=null ? PlayerActionKind.Interact : water!=null ? PlayerActionKind.CollectWater : PlayerActionKind.Attack;
            if(kind==Kind && station==TargetStation && water==TargetWater && previousItem==Equipment.SelectedItem) return;
            Kind=kind;TargetStation=station;TargetWater=water;previousItem=Equipment.SelectedItem;Changed?.Invoke();
        }
        public bool TryUse()
        {
            if(!Session.IsRunning) return false;
            RefreshContext();
            var success=UseCurrent();
            if(success && Audio!=null) Audio.PlaySfx();
            return success;
        }
        private bool UseCurrent()
        {
            switch(Kind)
            {
                case PlayerActionKind.Place: return Placement.TryPlace();
                case PlayerActionKind.Interact: return Crafting.OpenStation(TargetStation);
                case PlayerActionKind.Consume: return Survival.TryConsume();
                case PlayerActionKind.CollectWater: return TargetWater.Collect(Equipment.Inventory);
                default: return Combat.TryAttack();
            }
        }
        public Sprite ActionIcon(Sprite fist)
        {
            if(Kind==PlayerActionKind.Interact && TargetStation!=null) return TargetStation.Definition.Item.Icon;
            if(Kind==PlayerActionKind.CollectWater && TargetWater!=null) return TargetWater.Definition.Water.Icon;
            var item=Equipment.SelectedItem;
            return item!=null && (item.Tool!=null || item.Placement!=null || item.Consumable!=null) ? item.Icon : fist;
        }
    }
}
