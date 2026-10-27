using _Project.Features.Items.Domain;

namespace _Project.Features.Items.Application
{
    public interface IItemCollector
    {
        bool TryCollect(ItemId itemId);
        void RegisterNearbyPickup(IItemPickupTarget pickup);
        void UnregisterNearbyPickup(IItemPickupTarget pickup);
        bool TryCollectNearestPickup();
    }

    public interface IItemPickupTarget
    {
        float Distance { get; }
        bool TryCollect(IItemCollector collector);
    }
}
