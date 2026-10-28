namespace _Project.Features.Items.Domain
{
    public sealed class InventoryStack
    {
        public ItemDefinition Item { get; }
        public int Count { get; }

        public InventoryStack(ItemDefinition item, int count)
        {
            Item = item;
            Count = count;
        }
    }
}
