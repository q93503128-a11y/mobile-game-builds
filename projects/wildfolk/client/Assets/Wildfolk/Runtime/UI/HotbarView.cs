using UnityEngine;

namespace Wildfolk
{
    public sealed class HotbarView : MonoBehaviour
    {
        public PlayerInventory Owner;
        public GameplaySession Session;
        public InventorySlotView[] Slots;

        private void Start()
        {
            Owner.Model.Changed += Refresh;
            Owner.SelectionChanged += Refresh;
            Refresh();
        }
        private void OnDestroy()
        {
            if (Owner != null && Owner.Model != null) Owner.Model.Changed -= Refresh;
            if (Owner != null) Owner.SelectionChanged -= Refresh;
        }
        public void Select(int index)
        {
            if (Session.IsRunning && index >= 0 && index < PlayerInventory.HotbarSize)
                Owner.SelectSlot(index);
        }
        private void Refresh()
        {
            for (var i = 0; i < Slots.Length; i++)
                Slots[i].Show(Owner.Model.GetSlot(i), Owner.SelectedIndex == i);
        }
    }
}
