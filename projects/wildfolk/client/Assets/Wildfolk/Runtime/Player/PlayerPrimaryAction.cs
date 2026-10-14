using System;
using UnityEngine;
namespace Wildfolk
{
    public enum PrimaryActionKind { Attack, Place, Consume }
    public sealed class PlayerPrimaryAction : MonoBehaviour
    {
        public PlayerEquipment Equipment;
        public PlayerCombat Combat;
        public PlayerPlacement Placement;
        public PlayerSurvival Survival;
        public GameplaySession Session;
        public PrimaryActionKind Kind => Equipment.ActivePlaceable!=null ? PrimaryActionKind.Place : Equipment.SelectedItem!=null && Equipment.SelectedItem.Consumable!=null ? PrimaryActionKind.Consume : PrimaryActionKind.Attack;
        public event Action Changed;
        private ItemDefinition previous;
        private void Update() {if(previous==Equipment.SelectedItem)return;previous=Equipment.SelectedItem;Changed?.Invoke();}
        public bool TryUse()
        {
            if(!Session.IsRunning)return false;
            if(Kind==PrimaryActionKind.Place)return Placement.TryPlace();
            if(Kind==PrimaryActionKind.Consume)return Survival.TryConsume();
            return Combat.TryAttack();
        }
        public Sprite ActionIcon(Sprite fist)
        {
            var item=Equipment.SelectedItem;
            return item!=null && (item.Tool!=null || item.Placement!=null || item.Consumable!=null) ? item.Icon : fist;
        }
    }
}
