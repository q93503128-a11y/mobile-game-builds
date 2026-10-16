using System;
using UnityEngine;

namespace Wildfolk
{
    public sealed class PlayerInventory : MonoBehaviour
    {
        public const int HotbarSize = 9;
        public GameplaySettings Settings;
        public InventoryModel Model { get; private set; }
        public IInventoryAuthority Authority {get;set;}
        public bool TransferSlots(int from,int to,int amount)=>Authority!=null && !Authority.IsApplying ? Authority.Transfer(from,to,amount) : Model.MoveStackAmount(from,to,amount);
        public int SelectedIndex { get; private set; }
        public ItemStack SelectedStack => Model.GetSlot(SelectedIndex);
        public event Action SelectionChanged;

        private void Awake()
        {
            Model = new InventoryModel(Settings.InventorySlots);
        }

        public void SelectSlot(int index)
        {if(Authority!=null && !Authority.IsApplying){Authority.Select(index);return;}ApplySelection(index);}
        internal void ApplySelection(int index)
        {
            if (index < 0 || index >= Model.Capacity) throw new ArgumentOutOfRangeException(nameof(index));
            if (index == SelectedIndex) return;
            SelectedIndex = index;
            SelectionChanged?.Invoke();
        }
    }
}
