namespace Wildfolk
{
    public readonly struct ItemStack
    {
        public ItemDefinition Item { get; }
        public int Count { get; }
        public int Charges {get;}
        public LiquidState Quality {get;}
        public bool IsEmpty => Item == null || Count == 0;

        public ItemStack(ItemDefinition item, int count,int charges=0,LiquidState quality=LiquidState.Empty)
        {
            Item = item;
            Count = count;Charges=charges;Quality=quality;
        }
    }
}
