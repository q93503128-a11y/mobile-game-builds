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
        public Text Comparison,Details;
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
            if(equippedSelected && Current()==null)for(var i=0;i<Player.Model.Capacity;i++){var stack=Player.Model.GetSlot(i);if(!stack.IsEmpty && (stack.Item.Equippable) && PlayerEquipment.Slot(stack.Item)==selectedSlot){Panel.Select(i);break;}}
            Refresh();
        }
        private ItemDefinition Current()=>Equipment.Equipped(selectedSlot);
        public void Equip(){var stack=Player.SelectedStack;if(!stack.IsEmpty && stack.Item.Telescope){Player.GetComponent<PlayerInputCommands>().Session.CloseInventory();Player.GetComponent<PlayerTelescope>().Use();return;}if(!stack.IsEmpty && (stack.Item.Equippable))Equipment.TryEquip(stack.Item);Refresh();}
        public void Unequip(){if(equippedSelected)Equipment.Unequip(selectedSlot);else{var stack=Player.SelectedStack;if(!stack.IsEmpty && (stack.Item.Equippable))Equipment.Unequip(PlayerEquipment.Slot(stack.Item));}Refresh();}
        public void Refresh()
        {
            if(Player.Model==null)return;
            HeadIcon.sprite=Equipment.HeadArmor==null ? null : Equipment.HeadArmor.Icon;HeadIcon.enabled=HeadIcon.sprite!=null;BodyIcon.sprite=Equipment.BodyArmor==null ? null : Equipment.BodyArmor.Icon;BodyIcon.enabled=BodyIcon.sprite!=null;
            if(UtilityIcon!=null){UtilityIcon.sprite=Equipment.Utility==null ? null : Equipment.Utility.Icon;UtilityIcon.enabled=UtilityIcon.sprite!=null;}
            if(AccessoryIcon!=null){AccessoryIcon.sprite=Equipment.Accessory==null?null:Equipment.Accessory.Icon;AccessoryIcon.enabled=AccessoryIcon.sprite!=null;}
            var stack=Player.SelectedStack;var item=equippedSelected ? Current() : stack.IsEmpty ? null : stack.Item;var armor=item==null ? null : item.Armor;
            EquipButton.gameObject.SetActive(item!=null && (item.Equippable || item.Telescope) && !equippedSelected);UnequipButton.gameObject.SetActive(item!=null && (item.Equippable) && equippedSelected);Comparison.text=Details.text="";
            if(armor!=null){var current=armor.Slot==ArmorSlot.Head ? Equipment.HeadArmor : Equipment.BodyArmor;var oldDefense=current==null ? 0 : current.Armor.Defense;Details.text=ItemStats.Armor(armor);Comparison.text=L.Format("ui.inventory.compare",oldDefense,armor.Defense,armor.Defense-oldDefense);UnequipButton.interactable=current!=null;}
            if(EquipLabel!=null)EquipLabel.text=L.Text(item!=null && item.Telescope ? "ui.telescope.use" : "ui.action.equip");
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
            for(var i=0;i<Panel.Slots.Length;i++){var owned=Player.Model.GetSlot(i);var compatible=SlotFilter<0 || !owned.IsEmpty && owned.Item.Equippable && (int)PlayerEquipment.Slot(owned.Item)==SlotFilter;Panel.Slots[i].GetComponent<Image>().color=compatible ? Color.white : new Color(.52f,.58f,.52f,1);}
        }
    }
}
