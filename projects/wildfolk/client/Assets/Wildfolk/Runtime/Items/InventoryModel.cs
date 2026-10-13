using System;

namespace Wildfolk
{
    public sealed class InventoryModel
    {
        private readonly ItemStack[] slots;
        public int Capacity => slots.Length;
        public event Action Changed;

        public InventoryModel(int capacity)
        {
            if (capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity));
            slots = new ItemStack[capacity];
        }

        public ItemStack GetSlot(int index) => slots[index];

        // Returns the amount actually accepted; the caller keeps any remainder in the world.
        public int Add(ItemDefinition item, int count)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (item.MaxStack <= 0) throw new ArgumentOutOfRangeException(nameof(item.MaxStack));
            if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count));
            var remaining = count;
            for (var i = 0; i < slots.Length && remaining > 0; i++)
            {
                var stack = slots[i];
                if (stack.IsEmpty || stack.Item.Id != item.Id) continue;
                var accepted = Math.Min(item.MaxStack - stack.Count, remaining);
                slots[i] = new ItemStack(stack.Item, stack.Count + accepted);
                remaining -= accepted;
            }
            for (var i = 0; i < slots.Length && remaining > 0; i++)
            {
                if (!slots[i].IsEmpty) continue;
                var accepted = Math.Min(item.MaxStack, remaining);
                slots[i] = new ItemStack(item, accepted);
                remaining -= accepted;
            }
            var total = count - remaining;
            if (total > 0) Changed?.Invoke();
            return total;
        }
    }
}
