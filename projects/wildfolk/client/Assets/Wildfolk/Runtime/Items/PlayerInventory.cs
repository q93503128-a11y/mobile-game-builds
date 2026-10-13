using System;
using UnityEngine;

namespace Wildfolk
{
    public sealed class PlayerInventory : MonoBehaviour
    {
        public const int HotbarSize = 9;
        public GameplaySettings Settings;
        public StartingLoadout Loadout;
        public InventoryModel Model { get; private set; }
        public int SelectedIndex { get; private set; }
        public ItemStack SelectedStack => Model.GetSlot(SelectedIndex);
        public event Action SelectionChanged;

        private void Awake()
        {
            Model = new InventoryModel(Settings.InventorySlots);
            if (Loadout != null)
                foreach (var entry in Loadout.Items) Model.Add(entry.Item, entry.Count);
        }

        public void SelectSlot(int index)
        {
            if (index < 0 || index >= Model.Capacity) throw new ArgumentOutOfRangeException(nameof(index));
            if (index == SelectedIndex) return;
            SelectedIndex = index;
            SelectionChanged?.Invoke();
        }
    }
}
