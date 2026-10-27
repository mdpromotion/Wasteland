using _Project.Features.Core.Infrastructure;
using _Project.Features.Items.Presentation;
using _Project.Features.Player.Application;
using _Project.Features.UI.Hud.CooldownBar;
using _Project.Features.UI.Infrastructure;
using _Project.Features.UI.Menus.DebugMenu;
using _Project.Features.UI.Menus.InGameMenu;
using _Project.Features.UI.Menus.SettingsMenu;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace _Project.Features.Core.Bootstrap.Game.Installers
{
    public static class UIInstaller
    {
        public static void Install(
            IContainerBuilder builder,
            Transform gameSceneRoot,
            Sprite ironAxeIcon,
            Sprite woodIcon,
            Sprite appleIcon)
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

            builder.RegisterInstance(new HotbarHudRoot(gameSceneRoot));
            builder.RegisterInstance(new ItemIconProvider(ironAxeIcon, woodIcon, appleIcon))
                .As<IItemIconProvider>();
            builder.RegisterEntryPoint<HotbarHudPresenter>();
        }
    }
}
