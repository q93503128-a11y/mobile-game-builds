using UnityEngine;
using UnityEngine.UI;

namespace Wildfolk
{
    public sealed class InventoryPanel : MonoBehaviour
    {
        public PlayerInventory Owner;
        public GameplaySession Session;
        public GameObject View;
        public InventorySlotView[] Slots;
        public Image SelectedIcon;
        public Text SelectedName;
        public Text SelectedCategory;
        private int selected=-1;

        private void Start()
        {
            Owner.Model.Changed+=Refresh;
            Session.StateChanged+=Refresh;
            Refresh();
        }

        private void OnDestroy()
        {
            if(Owner != null && Owner.Model != null) Owner.Model.Changed-=Refresh;
            if(Session != null) Session.StateChanged-=Refresh;
        }

        public void Select(int index) { selected=index; Refresh(); }

        private void Refresh()
        {
            View.SetActive(Session.State==SessionState.Inventory);
            for(var i=0;i<Slots.Length;i++) Slots[i].Show(Owner.Model.GetSlot(i),i==selected);
            var stack=selected>=0 ? Owner.Model.GetSlot(selected) : default;
            SelectedIcon.enabled=!stack.IsEmpty;
            SelectedIcon.sprite=stack.IsEmpty ? null : stack.Item.Icon;
            SelectedName.text=stack.IsEmpty ? "Select an item" : stack.Item.DisplayName;
            SelectedCategory.text=stack.IsEmpty ? string.Empty : "Material";
        }
    }
}
