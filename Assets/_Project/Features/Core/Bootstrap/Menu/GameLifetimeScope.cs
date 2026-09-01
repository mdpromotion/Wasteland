using _Project.Features.UI.Menus.MainMenu;
using _Project.Features.UI.Menus.MainMenu.World;
using _Project.Features.UI.Menus.SettingsMenu;
using VContainer;
using VContainer.Unity;

namespace _Project.Features.Core.Bootstrap.Menu
{
    public class GameLifetimeScope : ChildLifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponentInHierarchy<MainMenuPresenter>();
            builder.RegisterComponentInHierarchy<WorldMenuPresenter>();
            builder.RegisterComponentInHierarchy<SettingsMenuPresenter>();
        }
    }
}
