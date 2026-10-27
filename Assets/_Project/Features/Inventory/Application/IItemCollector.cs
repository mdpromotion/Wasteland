using _Project.Features.Items.Domain;

namespace _Project.Features.Items.Application
{
    public interface IItemCollector
    {
        bool TryCollect(ItemId itemId);
    }
}
