namespace Wildfolk
{
    public readonly struct ItemStack
    {
        public ItemDefinition Item { get; }
        public int Count { get; }
        public bool IsEmpty => Item == null || Count == 0;

        public ItemStack(ItemDefinition item, int count)
        {
            Item = item;
            Count = count;
        }
    }
}
