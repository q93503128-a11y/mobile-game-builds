using UnityEngine;
using UnityEngine.UI;

namespace Wildfolk
{
    public sealed class InventoryPanel : MonoBehaviour
    {
        public PlayerInventory Owner;
        public PlayerStorage Storage;
        public GameplaySession Session;
        public GameObject View;
        public PlayerCrafting Crafting;
        public InventorySlotView[] Slots;
        public Image SelectedIcon;
        public Text SelectedName;
        public Text SelectedCategory;
        public Button CraftingButton;

        private void Start()
        {
            Owner.Model.Changed+=Refresh;
            Owner.SelectionChanged+=Refresh;
            Session.StateChanged+=Refresh;if(Crafting!=null)Crafting.StationChanged+=Refresh;L.Changed+=Refresh;if(Storage!=null)Storage.Changed+=Refresh;
            Refresh();
        }

        private void OnDestroy()
        {
            if(Owner != null && Owner.Model != null) Owner.Model.Changed-=Refresh;
            if(Owner != null) Owner.SelectionChanged-=Refresh;
            if(Session != null) Session.StateChanged-=Refresh;if(Crafting!=null)Crafting.StationChanged-=Refresh;L.Changed-=Refresh;
            if(Storage!=null) Storage.Changed-=Refresh;
        }

        public event System.Action ItemSelected;
        public void Select(int index){Owner.SelectSlot(index);Refresh();ItemSelected?.Invoke();}

        private void Refresh()
        {
            View.SetActive(!CampLootPanel.IsOpen && !TierThreeHud.IsFocused && Session.State==SessionState.Inventory && (Storage==null || !Storage.IsOpen) && (Crafting==null || !Crafting.ScreenOpen));
            var selected=Owner.SelectedIndex;
            for(var i=0;i<Slots.Length;i++) Slots[i].Show(Owner.Model.GetSlot(i),i==selected);
            var stack=Owner.SelectedStack;
            SelectedIcon.enabled=!stack.IsEmpty;
            SelectedIcon.sprite=stack.IsEmpty ? null : stack.Item.Icon;
            SelectedName.text=stack.IsEmpty ? L.Text("ui.inventory.select") : stack.Item.LocalizedName;
            SelectedCategory.text=stack.IsEmpty ? string.Empty : L.Text("ui.item_category."+stack.Item.Category);
        }
    }
}
