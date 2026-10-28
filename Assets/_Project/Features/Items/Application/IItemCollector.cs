using System.Collections.Generic;
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

        /// <summary>
        /// Requested visibility of the pickup prompt. Actual budget (how many
        /// prompts may be shown at once) is owned by <see cref="IPickupPromptProvider"/>.
        /// </summary>
        void SetPromptVisible(bool visible);
    }

    /// <summary>
    /// Exposes the set of pickups currently overlapping the player, feeding the
    /// prompt visibility cap. Implemented by PlayerInteractionController.
    /// </summary>
    public interface IPickupPromptProvider
    {
        IReadOnlyCollection<IItemPickupTarget> NearbyPickups { get; }
    }
}
