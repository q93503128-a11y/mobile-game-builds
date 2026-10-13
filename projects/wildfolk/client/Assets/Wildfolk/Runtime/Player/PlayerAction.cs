using System;
using UnityEngine;

namespace Wildfolk
{
    public enum PlayerActionKind { Attack, Place, Interact }

    public sealed class PlayerAction : MonoBehaviour
    {
        public PlayerEquipment Equipment;
        public PlayerCombat Combat;
        public PlayerPlacement Placement;
        public PlayerCrafting Crafting;
        public GameplaySession Session;
        public GameplaySettings Settings;
        public CharacterFacing Facing;
        public PlayerActionKind Kind { get; private set; }
        public CraftingStation TargetStation { get; private set; }
        public event Action Changed;
        private ItemDefinition previousItem;
        private readonly Collider2D[] candidates = new Collider2D[16];

        private void Update() => RefreshContext();
        private void RefreshContext()
        {
            CraftingStation station = null;
            if (Equipment.ActivePlaceable == null)
            {
                var count = Physics2D.OverlapCircle(transform.position, Settings.StationInteractionRange,
                    new ContactFilter2D {useTriggers=true}, candidates);
                var nearest = float.MaxValue;
                for (var i=0;i<count;i++)
                {
                    var candidate = candidates[i].GetComponentInParent<CraftingStation>();
                    if (candidate == null || !candidate.CanInteract(transform.position,Settings.StationInteractionRange)) continue;
                    var delta = (Vector2)candidate.transform.position-(Vector2)transform.position;
                    if (Vector2.Dot(Facing.Direction, delta.normalized) < .35f || delta.sqrMagnitude >= nearest) continue;
                    station=candidate;nearest=delta.sqrMagnitude;
                }
            }
            var kind = Equipment.ActivePlaceable != null ? PlayerActionKind.Place :
                station != null ? PlayerActionKind.Interact : PlayerActionKind.Attack;
            if (kind == Kind && station == TargetStation && previousItem == Equipment.SelectedItem) return;
            Kind=kind;TargetStation=station;previousItem=Equipment.SelectedItem;
            Changed?.Invoke();
        }
        public bool TryUse()
        {
            if (!Session.IsRunning) return false;
            RefreshContext();
            switch (Kind)
            {
                case PlayerActionKind.Place: return Placement.TryPlace();
                case PlayerActionKind.Interact: return Crafting.OpenStation(TargetStation);
                default: return Combat.TryAttack();
            }
        }
        public Sprite ActionIcon(Sprite fist)
        {
            if (Kind == PlayerActionKind.Interact && TargetStation != null) return TargetStation.Definition.Item.Icon;
            var item=Equipment.SelectedItem;
            return item != null && (item.Tool != null || item.Placement != null) ? item.Icon : fist;
        }
    }
}
