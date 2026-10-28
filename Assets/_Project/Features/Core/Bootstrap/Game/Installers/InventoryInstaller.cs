using _Project.Features.Items.Domain;
using _Project.Features.Items.Application;
using _Project.Features.Items.Infrastructure;
using _Project.Features.ProceduralWorld.Application.Chunks;
using _Project.Features.ProceduralWorld.Application.World;
using System;
using VContainer;
using VContainer.Unity;

namespace _Project.Features.Core.Bootstrap.Game.Installers
{
    public static class InventoryInstaller
    {
        public static void Install(
            IContainerBuilder builder,
            ItemCatalogAsset catalogAsset,
            ItemDropSettings itemDropSettings)
        {
            if (catalogAsset == null)
                throw new ArgumentNullException(nameof(catalogAsset));
            if (itemDropSettings == null)
                throw new ArgumentNullException(nameof(itemDropSettings));

            ItemCatalog catalog = catalogAsset.CreateDomainCatalog();

            Inventory inventory = catalogAsset.CreateStartingInventory(catalog);

            builder.RegisterInstance(catalogAsset);
            builder.RegisterInstance(itemDropSettings);
            builder.RegisterInstance(catalog).As<IItemCatalog>();
            builder.RegisterInstance(inventory);
            builder.Register<InventorySaveAdapter>(Lifetime.Singleton)
                .As<IInventoryStatePersistence>();
            builder.RegisterEntryPoint<InventoryInputController>();

            // Dropped item persistence: entity factory shared by the player drop
            // flow and the chunk respawn pipeline, plus a rebase participant that
            // keeps live entities glued to their terrain.
            builder.Register<WorldItemRegistry>(Lifetime.Singleton).AsSelf();
            builder.Register<WorldItemSpawner>(Lifetime.Singleton)
                .As<IWorldItemSpawner>()
                .AsSelf();
            builder.Register<WorldItemSpawnStage>(Lifetime.Singleton)
                .As<IWorldItemSpawnStage>();
            builder.Register<WorldItemRebaseSync>(Lifetime.Singleton)
                .As<IWorldRebaseParticipant>();

            builder.Register<ItemDropService>(Lifetime.Singleton)
                .As<IItemDropService>();
        }
    }
}
