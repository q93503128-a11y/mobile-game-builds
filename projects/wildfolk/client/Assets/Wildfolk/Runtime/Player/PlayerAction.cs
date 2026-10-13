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
        public PlayerStorage Storage;
        public PlayerSurvival Survival;
        public GameplaySession Session;
        public GameplaySettings Settings;
        public CharacterFacing Facing;
        public PlayerActionKind Kind { get; private set; }
        public IPlayerInteractable Target { get; private set; }
        public CraftingStation TargetStation => Target as CraftingStation;
        public WaterSource TargetWater => Target as WaterSource;
        public event Action Changed;
        private ItemDefinition previousItem;
        private readonly Collider2D[] candidates=new Collider2D[32];
        private void Update() => RefreshContext();
        private void RefreshContext()
        {
            IPlayerInteractable target=null;
            if(Equipment.ActivePlaceable==null && (Equipment.SelectedItem==null || Equipment.SelectedItem.Consumable==null))
            {
                var count=Physics2D.OverlapCircle(transform.position,Settings.StationInteractionRange,new ContactFilter2D {useTriggers=true},candidates);
                var nearest=float.MaxValue;
                for(var i=0;i<count;i++)
                {
                    var candidate=candidates[i].GetComponentInParent<IPlayerInteractable>();
                    if(candidate==null || !candidate.CanInteract(this)) continue;
                    var delta=(Vector2)candidate.Location.position-(Vector2)transform.position;
                    if(Vector2.Dot(Facing.Direction,delta.normalized)<.35f || delta.sqrMagnitude>=nearest) continue;
                    target=candidate;nearest=delta.sqrMagnitude;
                }

            }
            var kind=Equipment.ActivePlaceable!=null ? PlayerActionKind.Place :
                Equipment.SelectedItem!=null && Equipment.SelectedItem.Consumable!=null ? PlayerActionKind.Consume :
                target!=null ? target.ActionKind : PlayerActionKind.Attack;
            if(kind==Kind && target==Target && previousItem==Equipment.SelectedItem) return;
            Kind=kind;Target=target;previousItem=Equipment.SelectedItem;Changed?.Invoke();
        }
        public bool TryUse()
        {
            if(!Session.IsRunning) return false;
            RefreshContext();
            switch(Kind)
            {
                case PlayerActionKind.Place: return Placement.TryPlace();
                case PlayerActionKind.Consume: return Survival.TryConsume();
                case PlayerActionKind.Interact: case PlayerActionKind.CollectWater: return Target!=null && Target.Use(this);
                default: return Combat.TryAttack();
            }
        }
        public Sprite ActionIcon(Sprite fist)
        {
            if(Target!=null && (Kind==PlayerActionKind.Interact || Kind==PlayerActionKind.CollectWater)) return Target.Icon;
            var item=Equipment.SelectedItem;
            return item!=null && (item.Tool!=null || item.Placement!=null || item.Consumable!=null) ? item.Icon : fist;
        }
    }
}
