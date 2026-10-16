using System;

namespace Wildfolk
{
    public enum ExchangeResult { Success, MissingMaterials, NoSpace }

    public sealed class InventoryModel
    {
        private readonly ItemStack[] slots;
        public int Capacity => slots.Length;
        public bool IsReadOnly {get;set;}
        internal void ApplyReplicaSlot(int index,ItemStack value){slots[index]=value;Changed?.Invoke();}
        internal void ApplyReplicaSlots(ItemStack[] values){Array.Copy(values,slots,Capacity);Changed?.Invoke();}
        public bool MoveStackAmount(int from,int to,int amount)=>Transfer(this,from,this,to,amount);
        public bool TryDropSlot(int index,ItemDefinition expected,int count,Func<bool> createDrop)
        {
            if(IsReadOnly || index<0 || index>=Capacity || count<1 || createDrop==null)return false;
            var original=slots[index];if(original.IsEmpty || original.Item!=expected || count>original.Count)return false;
            slots[index]=count==original.Count ? default : new ItemStack(expected,original.Count-count);
            var committed=false;
            try{committed=createDrop();return committed;}
            finally{if(!committed)slots[index]=original;else Changed?.Invoke();}
        }
        public event Action Changed;

        public InventoryModel(int capacity)
        {
            if (capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity));
            slots = new ItemStack[capacity];
        }
        public ItemStack GetSlot(int index) => slots[index];
        public SavedStack[] Capture()
        {var result=new SavedStack[Capacity];for(var i=0;i<Capacity;i++)result[i]=new SavedStack{Item=slots[i].IsEmpty ? null : slots[i].Item.Id,Count=slots[i].Count};return result;}
        public void Restore(SavedStack[] state,ContentCatalog catalog)
        {
            if(IsReadOnly)throw new InvalidOperationException("Replica inventory is read-only.");
            if(state==null || state.Length!=Capacity)throw new ArgumentException("Inventory capacity mismatch");
            var next=new ItemStack[Capacity];
            for(var i=0;i<Capacity;i++)
            {
                if(state[i]==null || state[i].Count<0)throw new ArgumentException("Invalid saved stack");
                if(state[i].Count==0){if(!string.IsNullOrEmpty(state[i].Item))throw new ArgumentException("Invalid empty stack");continue;}
                var item=catalog.Item(state[i].Item);if(state[i].Count>item.MaxStack)throw new ArgumentException("Invalid stack size");next[i]=new ItemStack(item,state[i].Count);
            }
            Array.Copy(next,slots,Capacity);Changed?.Invoke();
        }
        public long CountItem(ItemDefinition item)
        {
            long total = 0;
            foreach (var slot in slots) if (!slot.IsEmpty && slot.Item.Id == item.Id) total += slot.Count;
            return total;
        }
        public int Add(ItemDefinition item, int count)
        {
            Validate(item, count);
            if(IsReadOnly)return 0;
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
            if(source==null || target==null || source.IsReadOnly || target.IsReadOnly || !CanTransfer(source,from,target,to,amount)) return false;
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
            if(IsReadOnly)return false;
            var slot = slots[index];
            if (slot.IsEmpty || slot.Item.Id != expected.Id || slot.Count < count) return false;
            slots[index] = slot.Count == count ? default : new ItemStack(slot.Item, slot.Count - count);
            Changed?.Invoke();
            return true;
        }
        public bool ReplaceConsumedSlot(int index,ItemDefinition expected,ItemDefinition returned)
        {
            if(IsReadOnly)return false;
            Validate(expected,1);Validate(returned,1);
            var stack=slots[index];if(stack.IsEmpty || stack.Item.Id!=expected.Id)return false;
            var next=(ItemStack[])slots.Clone();
            next[index]=stack.Count==1 ? default : new ItemStack(stack.Item,stack.Count-1);
            if(next[index].IsEmpty)next[index]=new ItemStack(returned,1);
            else if(AddTo(next,returned,1)!=1)return false;
            Array.Copy(next,slots,slots.Length);Changed?.Invoke();return true;
        }
        public bool TryAddAt(int index,ItemDefinition item)
        {
            if(IsReadOnly)return false;
            Validate(item,1);var slot=slots[index];
            if(!slot.IsEmpty && (slot.Item.Id!=item.Id || slot.Count>=item.MaxStack))return false;
            slots[index]=new ItemStack(item,slot.IsEmpty ? 1 : slot.Count+1);Changed?.Invoke();return true;
        }
        public ExchangeResult GetExchangeStatus(ItemAmount[] costs, ItemDefinition output, int count)
            => PrepareExchange(costs, output, count, out _);

        public ExchangeResult TryExchange(ItemAmount[] costs, ItemDefinition output, int count)
        {
            if(IsReadOnly)return ExchangeResult.NoSpace;
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
            if(IsReadOnly)return false;
            if(costs==null) throw new ArgumentNullException(nameof(costs));
            var next=(ItemStack[])slots.Clone();
            if(!RemoveCosts(next,costs)) return false;
            Array.Copy(next,slots,slots.Length);Changed?.Invoke();return true;
        }
        public bool TryAddAmounts(ItemAmount[] amounts)
        {
            if(IsReadOnly)return false;
            var next=(ItemStack[])slots.Clone();
            foreach(var amount in amounts){Validate(amount.Item,amount.Count);if(AddTo(next,amount.Item,amount.Count)!=amount.Count)return false;}
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
