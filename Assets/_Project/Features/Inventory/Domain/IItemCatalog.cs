namespace _Project.Features.Items.Domain
{
    public interface IItemCatalog
    {
        ItemDefinition Get(ItemId id);
    }
}
