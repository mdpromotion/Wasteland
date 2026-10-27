using _Project.Features.Items.Domain;
using _Project.Features.Items.Application;
using _Project.Features.Items.Infrastructure;
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
            StartingInventoryAsset startingInventoryAsset,
            ItemDropSettings itemDropSettings)
        {
            if (catalogAsset == null)
                throw new ArgumentNullException(nameof(catalogAsset));
            if (startingInventoryAsset == null)
                throw new ArgumentNullException(nameof(startingInventoryAsset));
            if (itemDropSettings == null)
                throw new ArgumentNullException(nameof(itemDropSettings));

            ItemCatalog catalog = catalogAsset.CreateDomainCatalog();

            Inventory inventory = startingInventoryAsset.Create(catalog);

            builder.RegisterInstance(catalogAsset);
            builder.RegisterInstance(itemDropSettings);
            builder.RegisterInstance(catalog).As<IItemCatalog>();
            builder.RegisterInstance(inventory);
            builder.RegisterEntryPoint<HotbarInputController>();
            builder.Register<ItemDropService>(Lifetime.Singleton)
                .As<IItemDropService>();
            builder.RegisterEntryPoint<DropItemController>();
        }
    }
}
