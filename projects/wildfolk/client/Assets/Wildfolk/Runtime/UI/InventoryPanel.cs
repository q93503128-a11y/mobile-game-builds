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
        public InventorySlotView[] Slots;
        public Image SelectedIcon;
        public Text SelectedName;
        public Text SelectedCategory;

        private void Start()
        {
            Owner.Model.Changed+=Refresh;
            Owner.SelectionChanged+=Refresh;
            Session.StateChanged+=Refresh;Storage.Changed+=Refresh;
            Refresh();
        }

        private void OnDestroy()
        {
            if(Owner != null && Owner.Model != null) Owner.Model.Changed-=Refresh;
            if(Owner != null) Owner.SelectionChanged-=Refresh;
            if(Session != null) Session.StateChanged-=Refresh;
            if(Storage!=null) Storage.Changed-=Refresh;
        }

        public void Select(int index) => Owner.SelectSlot(index);

        private void Refresh()
        {
            View.SetActive(Session.State==SessionState.Inventory && !Storage.IsOpen);
            var selected=Owner.SelectedIndex;
            for(var i=0;i<Slots.Length;i++) Slots[i].Show(Owner.Model.GetSlot(i),i==selected);
            var stack=Owner.SelectedStack;
            SelectedIcon.enabled=!stack.IsEmpty;
            SelectedIcon.sprite=stack.IsEmpty ? null : stack.Item.Icon;
            SelectedName.text=stack.IsEmpty ? "Select an item" : stack.Item.DisplayName;
            SelectedCategory.text=stack.IsEmpty ? string.Empty : stack.Item.Category.ToString();
        }
    }
}
