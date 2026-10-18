using System;
using UnityEngine;
namespace Wildfolk
{
    public enum PrimaryActionKind { Attack, Place, Consume,Equip, Fish, Ranged, Water }
    public sealed class PlayerPrimaryAction : MonoBehaviour
    {
        public PlayerEquipment Equipment;
        public PlayerCombat Combat;
        public PlayerPlacement Placement;
        public PlayerSurvival Survival;
        public GameplaySession Session;
        public PrimaryActionKind Kind => Equipment.SelectedItem!=null && (Equipment.SelectedItem.Armor!=null || Equipment.SelectedItem.EquippedLight!=null) ? PrimaryActionKind.Equip : Equipment.SelectedItem!=null && Equipment.SelectedItem.WaterCapacity>0 ? PrimaryActionKind.Water : Equipment.SelectedItem!=null && Equipment.SelectedItem.FishingRod ? PrimaryActionKind.Fish : Equipment.SelectedItem!=null && Equipment.SelectedItem.Ranged!=null ? PrimaryActionKind.Ranged : Equipment.ActivePlaceable!=null ? PrimaryActionKind.Place : Equipment.SelectedItem!=null && Equipment.SelectedItem.Consumable!=null ? PrimaryActionKind.Consume : PrimaryActionKind.Attack;
        public event Action Changed;
        private ItemDefinition previous;
        private void Update() {if(previous==Equipment.SelectedItem)return;previous=Equipment.SelectedItem;Changed?.Invoke();}
        public bool TryUse()
        {
            if(!Session.IsRunning)return false;
            if(Kind==PrimaryActionKind.Water)return Equipment.Inventory.SelectedStack.Charges==0 ? GetComponent<PlayerWaterskin>().Fill() : GetComponent<PlayerWaterskin>().Drink();
            if(Kind==PrimaryActionKind.Fish)return GetComponent<PlayerFishing>().Use();
            if(Kind==PrimaryActionKind.Ranged)return GetComponent<PlayerRanged>().Begin();
            if(Kind==PrimaryActionKind.Equip)return Equipment.TryEquipSelectedArmor();
            if(Kind==PrimaryActionKind.Place)return Placement.TryPlace();
            if(Kind==PrimaryActionKind.Consume)return Survival.TryConsume();
            return Combat.TryAttack();
        }
        public Sprite ActionIcon(Sprite fist)
        {
            var item=Equipment.SelectedItem;
            return item!=null && (item.Tool!=null || item.Placement!=null || item.Consumable!=null || item.Armor!=null || item.FishingRod || item.WaterCapacity>0 || item.EquippedLight!=null) ? item.Icon : fist;
        }
    }
}
