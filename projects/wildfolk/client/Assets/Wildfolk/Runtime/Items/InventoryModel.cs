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
        public bool CanMoveStack(int from,int to) => CanTransfer(this,from,this,to);
        public bool MoveStack(int from,int to) => Transfer(this,from,this,to);
        public static bool CanTransfer(InventoryModel source,int from,InventoryModel target,int to,int amount=0)
        {
            if(source==null || target==null || (source==target && from==to)) return false;
            var stack=source.slots[from];var destination=target.slots[to];
            if(amount<0 || amount>stack.Count || amount>0 && !destination.IsEmpty && destination.Item.Id!=stack.Item.Id && amount!=stack.Count)return false;
            return !stack.IsEmpty && (destination.IsEmpty || destination.Item.Id!=stack.Item.Id || destination.Count<destination.Item.MaxStack);
        }
        public static bool Transfer(InventoryModel source,int from,InventoryModel target,int to,int amount=0)
        {
            if(!CanTransfer(source,from,target,to,amount)) return false;
            var stack=source.slots[from];var destination=target.slots[to];
            var requested=amount==0 ? stack.Count : amount;
            if(!destination.IsEmpty && destination.Item.Id==stack.Item.Id)
            {
                var moved=Math.Min(requested,destination.Item.MaxStack-destination.Count);
                target.slots[to]=new ItemStack(destination.Item,destination.Count+moved);
                source.slots[from]=moved==stack.Count ? default : new ItemStack(stack.Item,stack.Count-moved);
            }
            else if(destination.IsEmpty){target.slots[to]=new ItemStack(stack.Item,requested);source.slots[from]=requested==stack.Count ? default : new ItemStack(stack.Item,stack.Count-requested);}
            else { target.slots[to]=stack;source.slots[from]=destination; }
            source.Changed?.Invoke();if(target!=source) target.Changed?.Invoke();return true;
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
        public bool SplitHalf(int from)
        {
            var stack=slots[from];if(stack.IsEmpty || stack.Count<2)return false;
            for(var to=0;to<slots.Length;to++)if(slots[to].IsEmpty)return Transfer(this,from,this,to,stack.Count/2);
            return false;
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
