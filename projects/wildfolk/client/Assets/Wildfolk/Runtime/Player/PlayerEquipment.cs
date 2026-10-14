using System;
using UnityEngine;

namespace Wildfolk
{
    public sealed class PlayerEquipment : MonoBehaviour
    {
        public PlayerInventory Inventory;
        public PlayerCombat Combat;
        public SpriteRenderer HeldVisual;
        public SpriteRenderer ArmorVisual;
        public SpriteRenderer HeadVisual,LeftHandGear,RightHandGear;
        public ItemDefinition BodyArmor {get;private set;}
        public ItemDefinition HeadArmor {get;private set;}
        public int Defense=>(BodyArmor==null ? 0 : BodyArmor.Armor.Defense)+(HeadArmor==null ? 0 : HeadArmor.Armor.Defense);
        public bool TryEquipSelectedArmor()=>TryEquip(Inventory.SelectedStack.IsEmpty ? null : Inventory.SelectedStack.Item);
        public bool TryEquip(ItemDefinition item)
        {
            if(item==null || item.Armor==null)return false;
            var previous=item.Armor.Slot==ArmorSlot.Head ? HeadArmor : BodyArmor;
            if(previous==item)return false;
            if(previous==null){if(!Inventory.Model.TryConsumeCosts(new[]{new ItemAmount{Item=item,Count=1}}))return false;}
            else if(Inventory.Model.TryExchange(new[]{new ItemAmount{Item=item,Count=1}},previous,1)!=ExchangeResult.Success)return false;
            if(item.Armor.Slot==ArmorSlot.Head)HeadArmor=item;else BodyArmor=item;RefreshArmor();return true;
        }
        public bool Unequip(ArmorSlot slot)
        {
            var item=slot==ArmorSlot.Head ? HeadArmor : BodyArmor;if(item==null)return false;
            if(Inventory.Model.Add(item,1)!=1){PlayerFeedback.Show("Make room in your bag");return false;}
            if(slot==ArmorSlot.Head)HeadArmor=null;else BodyArmor=null;RefreshArmor();return true;
        }
        public void RestoreArmor(ItemDefinition head,ItemDefinition body)
        {if(head!=null && (head.Armor==null || head.Armor.Slot!=ArmorSlot.Head) || body!=null && (body.Armor==null || body.Armor.Slot!=ArmorSlot.Body))throw new System.ArgumentException();HeadArmor=head;BodyArmor=body;RefreshArmor();}
        private void RefreshArmor()
        {
            GetComponent<Health>().DamageReduction=Defense;
            ArmorVisual.sprite=BodyArmor==null ? null : BodyArmor.Armor.WornSprite;ArmorVisual.enabled=ArmorVisual.sprite!=null;
            HeadVisual.sprite=HeadArmor==null ? null : HeadArmor.Armor.WornSprite;HeadVisual.enabled=HeadVisual.sprite!=null;
            LeftHandGear.sprite=RightHandGear.sprite=BodyArmor==null ? null : BodyArmor.Armor.HandSprite;
            LeftHandGear.enabled=RightHandGear.enabled=LeftHandGear.sprite!=null;Changed?.Invoke();
        }
        public ToolDefinition ActiveTool { get; private set; }
        public ItemDefinition SelectedItem { get; private set; }
        public PlaceableDefinition ActivePlaceable => SelectedItem == null ? null : SelectedItem.Placement;
        public event Action Changed;

        private void Start()
        {
            Inventory.Model.Changed += Refresh;
            Inventory.SelectionChanged += Refresh;
            Refresh();
        }
        private void OnDestroy()
        {
            if (Inventory != null && Inventory.Model != null) Inventory.Model.Changed -= Refresh;
            if (Inventory != null) Inventory.SelectionChanged -= Refresh;
        }
        private void Refresh()
        {
            var stack = Inventory.SelectedStack;
            var item = stack.IsEmpty ? null : stack.Item;
            var next = stack.IsEmpty ? null : stack.Item.Tool;
            if (item == SelectedItem && next == ActiveTool && HeldVisual.enabled == (next != null)) return;
            Combat.CancelAttack();
            SelectedItem = item;
            ActiveTool = next;
            HeldVisual.sprite = ActiveTool == null ? null : ActiveTool.HeldSprite;
            HeldVisual.enabled = ActiveTool != null;
            Changed?.Invoke();
        }
    }
}
