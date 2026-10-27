using _Project.Features.Items.Domain;
using _Project.Features.Items.Application;
using VContainer;
using VContainer.Unity;

namespace _Project.Features.Core.Bootstrap.Game.Installers
{
    public static class InventoryInstaller
    {
        public static void Install(IContainerBuilder builder)
        {
            var catalog = new ItemCatalog(new[]
            {
                new ItemDefinition(ItemId.IronAxe, "Железный топор", 1, canChopTrees: true, toolDamage: 1f),
                new ItemDefinition(ItemId.Wood, "Древесина", 50),
                new ItemDefinition(ItemId.Apple, "Яблоко", 50, canConsume: true)
            });

            Inventory inventory = StartingInventoryFactory.Create(catalog);

            builder.RegisterInstance(catalog).As<IItemCatalog>();
            builder.RegisterInstance(inventory);
            builder.RegisterEntryPoint<HotbarInputController>();
        }
    }
}
