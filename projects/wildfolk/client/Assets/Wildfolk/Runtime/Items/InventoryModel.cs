using System;

namespace Wildfolk
{
    public enum ExchangeResult { Success, MissingMaterials, NoSpace }

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
        public long CountItem(ItemDefinition item)
        {
            long total = 0;
            foreach (var slot in slots) if (!slot.IsEmpty && slot.Item.Id == item.Id) total += slot.Count;
            return total;
        }
        public int Add(ItemDefinition item, int count)
        {
            Validate(item, count);
            var accepted = AddTo(slots, item, count);
            if (accepted > 0) Changed?.Invoke();
            return accepted;
        }
        public bool CanMoveStack(int from, int to)
        {
            var source = slots[from]; var target = slots[to];
            return from != to && !source.IsEmpty &&
                (target.IsEmpty || target.Item.Id != source.Item.Id || target.Count < target.Item.MaxStack);
        }
        public bool MoveStack(int from, int to)
        {
            if (!CanMoveStack(from, to)) return false;
            var source = slots[from]; var target = slots[to];
            if (!target.IsEmpty && target.Item.Id == source.Item.Id)
            {
                var moved = Math.Min(source.Count, target.Item.MaxStack - target.Count);
                slots[to] = new ItemStack(target.Item, target.Count + moved);
                slots[from] = moved == source.Count ? default : new ItemStack(source.Item, source.Count - moved);
            }
            else { slots[to] = source; slots[from] = target; }
            Changed?.Invoke();
            return true;
        }
        public bool ConsumeSlot(int index, ItemDefinition expected, int count)
        {
            Validate(expected, count);
            var slot = slots[index];
            if (slot.IsEmpty || slot.Item.Id != expected.Id || slot.Count < count) return false;
            slots[index] = slot.Count == count ? default : new ItemStack(slot.Item, slot.Count - count);
            Changed?.Invoke();
            return true;
        }
        public ExchangeResult GetExchangeStatus(ItemAmount[] costs, ItemDefinition output, int count)
            => PrepareExchange(costs, output, count, out _);

        public ExchangeResult TryExchange(ItemAmount[] costs, ItemDefinition output, int count)
        {
            var result = PrepareExchange(costs, output, count, out var next);
            if (result != ExchangeResult.Success) return result;
            Array.Copy(next, slots, slots.Length);
            Changed?.Invoke();
            return result;
        }
        private ExchangeResult PrepareExchange(ItemAmount[] costs, ItemDefinition output, int count, out ItemStack[] next)
        {
            Validate(output, count);
            if (costs == null) throw new ArgumentNullException(nameof(costs));
            next = (ItemStack[])slots.Clone();
            if (!RemoveCosts(next,costs)) return ExchangeResult.MissingMaterials;
            return AddTo(next, output, count) == count ? ExchangeResult.Success : ExchangeResult.NoSpace;
        }
        public bool TryConsumeCosts(ItemAmount[] costs)
        {
            if(costs==null) throw new ArgumentNullException(nameof(costs));
            var next=(ItemStack[])slots.Clone();
            if(!RemoveCosts(next,costs)) return false;
            Array.Copy(next,slots,slots.Length);Changed?.Invoke();return true;
        }
        private static bool RemoveCosts(ItemStack[] next,ItemAmount[] costs)
        {
            foreach (var cost in costs)
            {
                Validate(cost.Item, cost.Count);
                var remaining = cost.Count;
                for (var i = 0; i < next.Length && remaining > 0; i++)
                {
                    var slot = next[i];
                    if (slot.IsEmpty || slot.Item.Id != cost.Item.Id) continue;
                    var consumed = Math.Min(slot.Count, remaining);
                    remaining -= consumed;
                    next[i] = slot.Count == consumed ? default : new ItemStack(slot.Item, slot.Count - consumed);
                }
                if (remaining > 0) return false;
            }
            return true;
        }
        private static int AddTo(ItemStack[] target, ItemDefinition item, int count)
        {
            var remaining = count;
            for (var i = 0; i < target.Length && remaining > 0; i++)
            {
                var slot = target[i];
                if (slot.IsEmpty || slot.Item.Id != item.Id) continue;
                var accepted = Math.Min(item.MaxStack - slot.Count, remaining);
                target[i] = new ItemStack(slot.Item, slot.Count + accepted);
                remaining -= accepted;
            }
            for (var i = 0; i < target.Length && remaining > 0; i++)
            {
                if (!target[i].IsEmpty) continue;
                var accepted = Math.Min(item.MaxStack, remaining);
                target[i] = new ItemStack(item, accepted);
                remaining -= accepted;
            }
            return count - remaining;
        }
        private static void Validate(ItemDefinition item, int count)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (item.MaxStack <= 0) throw new ArgumentOutOfRangeException(nameof(item.MaxStack));
            if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count));
        }
    }
}
