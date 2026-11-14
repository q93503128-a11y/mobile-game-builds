using UnityEngine;
using UnityEngine.UI;
namespace Wildfolk
{
    public sealed class InventoryEquipment : MonoBehaviour
    {
        public PlayerInventory Player;
        public PlayerEquipment Equipment;
        public InventoryPanel Panel;
        public Image HeadIcon,BodyIcon,UtilityIcon,AccessoryIcon;
        public Button UtilityButton,AccessoryButton;
        public Text[] SlotItemNames,SlotStatusLabels;
        public Image[] SlotSelectionImages;
        public Text Comparison,Details;
        public Text SelectionState;
        public Button EquipButton,UnequipButton;
        public Button HeadButton,BodyButton;
        public Text EquipLabel;
        public int SlotFilter {get;private set;}=-1;
        private bool equippedSelected;
        private ArmorSlot selectedSlot;
        private void Start(){Player.Model.Changed+=Refresh;Player.SelectionChanged+=Selection;Panel.ItemSelected+=Selection;Equipment.Changed+=Refresh;L.Changed+=Refresh;Refresh();}
        private void OnDestroy(){L.Changed-=Refresh;Player.Model.Changed-=Refresh;Player.SelectionChanged-=Selection;Panel.ItemSelected-=Selection;Equipment.Changed-=Refresh;}
        private void Selection(){equippedSelected=false;Refresh();}
        public void SelectSlot(int slot)
        {
            SlotFilter=SlotFilter==slot ? -1 : slot;selectedSlot=(ArmorSlot)slot;equippedSelected=SlotFilter>=0;
            var drag=GetComponent<InventoryDragController>();
            var selectingEquipment=equippedSelected;var selectedStack=Player.SelectedStack;
            var selectedCandidate=selectingEquipment && !selectedStack.IsEmpty && selectedStack.Item.Equippable && PlayerEquipment.Slot(selectedStack.Item)==selectedSlot;
            if(selectingEquipment && !selectedCandidate && Current()==null)
            {
                for(var i=0;i<Player.Model.Capacity;i++)
                {
                    var stack=Player.Model.GetSlot(i);
                    if(stack.IsEmpty || !stack.Item.Equippable || PlayerEquipment.Slot(stack.Item)!=selectedSlot)continue;
                    Panel.Select(i);drag?.Select(i,false);selectedCandidate=true;break;
                }
            }
            if(selectingEquipment && !selectedCandidate){drag?.Select(-1,false);equippedSelected=SlotFilter>=0;}
            else if(selectingEquipment)equippedSelected=false;
            else{drag?.Select(Player.SelectedIndex,false);equippedSelected=false;}
            Refresh();
        }
        private ItemDefinition Current()=>Equipment.Equipped(selectedSlot);
        public void Equip()
        {
            var stack=Player.SelectedStack;
            if(stack.IsEmpty){Refresh();return;}
            if(stack.Item.Telescope){Player.GetComponent<PlayerInputCommands>().Session.CloseInventory();Player.GetComponent<PlayerTelescope>().Use();return;}
            if(stack.Item.Equippable && Equipment.TryEquip(stack.Item))
            {
                selectedSlot=PlayerEquipment.Slot(stack.Item);
                SlotFilter=(int)selectedSlot;
                equippedSelected=true;
                GetComponent<InventoryDragController>()?.Select(-1,false);
            }
            Refresh();
        }
        public void Unequip(){if(equippedSelected)Equipment.Unequip(selectedSlot);else{var stack=Player.SelectedStack;if(!stack.IsEmpty && (stack.Item.Equippable))Equipment.Unequip(PlayerEquipment.Slot(stack.Item));}Refresh();}
        public void Refresh()
        {
            if(Player.Model==null)return;
            HeadIcon.sprite=Equipment.HeadArmor==null ? null : Equipment.HeadArmor.Icon;HeadIcon.enabled=HeadIcon.sprite!=null;BodyIcon.sprite=Equipment.BodyArmor==null ? null : Equipment.BodyArmor.Icon;BodyIcon.enabled=BodyIcon.sprite!=null;
            if(UtilityIcon!=null){UtilityIcon.sprite=Equipment.Utility==null ? null : Equipment.Utility.Icon;UtilityIcon.enabled=UtilityIcon.sprite!=null;}
            if(AccessoryIcon!=null){AccessoryIcon.sprite=Equipment.Accessory==null?null:Equipment.Accessory.Icon;AccessoryIcon.enabled=AccessoryIcon.sprite!=null;}
            var stack=Player.SelectedStack;var item=equippedSelected ? Current() : stack.IsEmpty ? null : stack.Item;var armor=item==null ? null : item.Armor;
            var itemSlot=item!=null && item.Equippable ? PlayerEquipment.Slot(item) : selectedSlot;
            var currentItem=item!=null && item.Equippable ? Equipment.Equipped(itemSlot) : null;
            var selectedCompatible=SlotFilter<0 || item!=null && item.Equippable && (int)itemSlot==SlotFilter;
            var alreadyEquipped=item!=null && item.Equippable && currentItem==item;
            var canEquip=item!=null && item.Equippable && selectedCompatible && !equippedSelected && !alreadyEquipped;
            var canUse=item!=null && item.Telescope && SlotFilter<0;
            EquipButton.gameObject.SetActive(canEquip || canUse);EquipButton.interactable=canEquip || canUse;
            UnequipButton.gameObject.SetActive(item!=null && item.Equippable && equippedSelected);Comparison.text=Details.text="";
            Comparison.color=new Color32(226,222,193,255);
            if(armor!=null)
            {
                var oldDefense=currentItem==null ? 0 : currentItem.Armor.Defense;
                var delta=armor.Defense-oldDefense;
                Details.text=ItemStats.Armor(armor);
                if(!equippedSelected && !alreadyEquipped && selectedCompatible)
                {
                    var direction=delta>0 ? "↑" : delta<0 ? "↓" : "→";
                    Comparison.text=L.Format("ui.inventory.compare",oldDefense,armor.Defense,direction,delta);
                    Comparison.color=delta>0 ? new Color32(159,214,145,255) : delta<0 ? new Color32(239,151,128,255) : new Color32(213,194,161,255);
                }
            }
            if(EquipLabel!=null)EquipLabel.text=L.Text(item!=null && item.Telescope ? "ui.telescope.use" : currentItem!=null && !alreadyEquipped ? "ui.action.replace" : "ui.action.equip");
            if(item!=null && item.Telescope)Details.text=L.Text("ui.telescope.aim");
            if(item!=null && item.EquippedLight!=null){Details.text=L.Text("ui.utility.info");UnequipButton.interactable=Equipment.Utility!=null;}
            if(item!=null && (item.Tool!=null || item.ExplorationUtility!=null))Details.text=ItemStats.Description(item);
            if(item!=null && item.Consumable!=null)Details.text=ItemStats.Food(item.Consumable);
            if(item!=null && item.Accessory!=null){Details.text=L.Text("item."+item.Id+".description");UnequipButton.interactable=Equipment.Accessory!=null;}
            if(item!=null && item.FishingRod)Details.text=L.Text("ui.fishing.rod");
            if(item!=null && item.Consumable!=null && L.Has("item."+item.Id+".description"))Details.text+="\n"+L.Text("item."+item.Id+".description");
            if(item!=null && item.Ammo!=AmmoKind.None)Details.text=L.Format("ui.ammo.info",item.AmmoDamage);
            if(item!=null && item.WaterCapacity>0)Details.text=stack.Charges+"/"+item.WaterCapacity+" · "+L.Text("water."+stack.Quality)+"\n"+L.Text("ui.waterskin.info");
            if(equippedSelected){Panel.SelectedName.text=item==null ? L.Text("ui.inventory.empty") : item.LocalizedName;Panel.SelectedIcon.sprite=item==null ? null : item.Icon;Panel.SelectedIcon.enabled=item!=null;Panel.SelectedCategory.text=L.Text("ui.slot."+selectedSlot);}
            if(SelectionState!=null)
            {
                var state="";var color=new Color32(226,222,193,255);
                if(equippedSelected){state=item==null ? string.Empty : L.Text("ui.inventory.equipped");color=item==null ? new Color32(220,175,124,255) : new Color32(157,212,143,255);}
                else if(SlotFilter>=0 && item!=null && !selectedCompatible){state=L.Format("ui.inventory.incompatible",L.Text("ui.slot."+(ArmorSlot)SlotFilter));color=new Color32(238,145,121,255);}
                else if(alreadyEquipped){state=L.Text("ui.inventory.already_equipped");color=new Color32(157,212,143,255);}
                else if(item!=null && item.Equippable)
                {
                    if(currentItem==null){state=L.Text("ui.inventory.equip_candidate");color=new Color32(255,204,129,255);}
                    else{state=L.Format("ui.inventory.current_item",currentItem.LocalizedName);color=new Color32(255,204,129,255);}
                }
                SelectionState.text=state;SelectionState.color=color;
            }
            for(var i=0;i<EquipmentSlots.Length;i++)
            {
                var slot=EquipmentSlots[i];
                var current=Equipment.Equipped(slot);
                var selectedStack=Player.SelectedStack;
                var matching= !selectedStack.IsEmpty && selectedStack.Item.Equippable && PlayerEquipment.Slot(selectedStack.Item)==slot;
                var selectedCurrent=SlotFilter==(int)slot && matching && current==selectedStack.Item;
                var candidate=!equippedSelected && SlotFilter==(int)slot && matching && !selectedCurrent;
                var showingCurrent=SlotFilter==(int)slot && equippedSelected && selectedSlot==slot && current!=null;
                var mismatch=!equippedSelected && SlotFilter==(int)slot && !selectedStack.IsEmpty && !matching;
                if(SlotItemNames!=null && i<SlotItemNames.Length && SlotItemNames[i]!=null)SlotItemNames[i].text=current==null ? "—" : current.LocalizedName;
                if(SlotStatusLabels!=null && i<SlotStatusLabels.Length && SlotStatusLabels[i]!=null)
                {
                    var key=candidate ? current==null ? "ui.inventory.equip_candidate" : "ui.inventory.replace_candidate" :
                        selectedCurrent || showingCurrent || mismatch ? null : current==null ? "ui.inventory.empty" : null;
                    SlotStatusLabels[i].text=key==null ? string.Empty : mismatch ? L.Format(key,L.Text("ui.slot."+slot)) : L.Text(key);
                    SlotStatusLabels[i].color=mismatch ? new Color32(238,145,121,255) : candidate ? new Color32(255,204,129,255) : current==null ?
                        new Color32(220,175,124,255) : new Color32(157,192,129,255);
                }
                if(SlotSelectionImages!=null && i<SlotSelectionImages.Length && SlotSelectionImages[i]!=null)
                {
                    var selected=SlotFilter==(int)slot;
                    SlotSelectionImages[i].enabled=selected;
                    SlotSelectionImages[i].gameObject.SetActive(selected);
                    if(selected)SlotSelectionImages[i].color=mismatch ? new Color32(238,145,121,255) :
                        selectedCurrent || showingCurrent ? new Color32(157,212,143,255) : new Color32(255,204,129,255);
                }
            }
            for(var i=0;i<Panel.Slots.Length;i++)
            {
                var owned=Player.Model.GetSlot(i);
                var compatible=SlotFilter<0 || !owned.IsEmpty && owned.Item.Equippable && (int)PlayerEquipment.Slot(owned.Item)==SlotFilter;
                Panel.Slots[i].GetComponent<Image>().color=compatible ? Color.white : new Color32(143,119,96,255);
            }
            if(!equippedSelected)Panel.RefreshSelection();
        }

        private static readonly ArmorSlot[] EquipmentSlots={ArmorSlot.Head,ArmorSlot.Body,ArmorSlot.Utility,ArmorSlot.Accessory};
    }
}
