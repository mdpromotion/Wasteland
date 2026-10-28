namespace _Project.Features.Items.Domain
{
    public interface IItemCatalog
    {
        ItemDefinition Get(ItemId id);

        /// <summary>
        /// Safe lookup used by save/loading paths where stored IDs may no longer exist.
        /// </summary>
        bool TryGet(ItemId id, out ItemDefinition definition);
    }
}
