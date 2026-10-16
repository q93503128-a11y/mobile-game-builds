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
        public bool TryEquipSelectedArmor()=>TryEquipFromSlot(Inventory.SelectedIndex);
        public bool TryEquip(ItemDefinition item)
        {
            if(item==null)return false;
            for(var i=0;i<Inventory.Model.Capacity;i++){var stack=Inventory.Model.GetSlot(i);if(!stack.IsEmpty && stack.Item==item)return TryEquipFromSlot(i);}return false;
        }
        public bool TryEquipFromSlot(int index)
        {
            if(Inventory.Authority!=null && !Inventory.Authority.IsApplying)return Inventory.Authority.Equip(index);
            var stack=Inventory.Model.GetSlot(index);var item=stack.IsEmpty ? null : stack.Item;
            if(item==null || item.Armor==null)return false;
            var previous=item.Armor.Slot==ArmorSlot.Head ? HeadArmor : BodyArmor;
            if(previous==item)return false;
            var accepted=previous==null ? Inventory.Model.ConsumeSlot(index,item,1) : Inventory.Model.ReplaceConsumedSlot(index,item,previous);
            if(!accepted){PlayerFeedback.ShowKey("feedback.bag_full");return false;}
            if(item.Armor.Slot==ArmorSlot.Head)HeadArmor=item;else BodyArmor=item;RefreshArmor();return true;
        }
        public bool Unequip(ArmorSlot slot,int destination=-1)
        {
            if(Inventory.Authority!=null && !Inventory.Authority.IsApplying)return Inventory.Authority.Unequip(slot,destination);
            var item=slot==ArmorSlot.Head ? HeadArmor : BodyArmor;if(item==null)return false;
            if(!(destination<0 ? Inventory.Model.Add(item,1)==1 : Inventory.Model.TryAddAt(destination,item))){PlayerFeedback.ShowKey("feedback.bag_full");return false;}
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

        public void ApplyReplicatedAppearance(ItemDefinition head,ItemDefinition body,ItemDefinition held)
        {RestoreArmor(head,body);ShowHeldItem(held);}

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
            ShowHeldItem(item);
        }
        private void ShowHeldItem(ItemDefinition item)
        {
            var next = item==null ? null : item.Tool;
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
