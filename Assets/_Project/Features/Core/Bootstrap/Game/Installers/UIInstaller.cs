using _Project.Features.Core.Infrastructure;
using _Project.Features.Player.Application;
using _Project.Features.UI.Hud.CooldownBar;
using _Project.Features.UI.Infrastructure;
using _Project.Features.UI.Menus.DebugMenu;
using _Project.Features.UI.Menus.InGameMenu;
using _Project.Features.UI.Menus.SettingsMenu;
using VContainer;
using VContainer.Unity;

namespace _Project.Features.Core.Bootstrap.Game.Installers
{
    public static class UIInstaller
    {
        public static void Install(IContainerBuilder builder)
        {
            builder.Register<FPSCounter>(Lifetime.Singleton)
                .As<ITickable>()
                .As<IFPSCounter>();

            builder.Register<PlayerPositionService>(Lifetime.Singleton)
                .As<IPlayerPositionService>();

            builder.RegisterComponentInHierarchy<CooldownBarPresenter>();

            builder.RegisterComponentInHierarchy<DebugMenuPresenter>();

            builder.RegisterComponentInHierarchy<InGameMenuPresenter>();

            builder.RegisterComponentInHierarchy<SettingsMenuPresenter>();
        }
    }
}