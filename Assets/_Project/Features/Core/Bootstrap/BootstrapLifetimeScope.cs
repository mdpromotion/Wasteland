using _Project.Features.Core.Bootstrap.Installers;
using _Project.Features.Core.Infrastructure;
using _Project.Features.Graphics.Infrastucture;
using _Project.Features.ProceduralWorld.Domain.World;
using _Project.Features.UI.Infrastructure;
using _Project.Features.UI.Menus.LoadingScreen.View;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace _Project.Features.Core.Bootstrap
{
    public class BootstrapLifetimeScope : LifetimeScope
    {
        public static BootstrapLifetimeScope Instance { get; private set; }

        [SerializeField] private LoadingScreenView loadingScreenView;
        [SerializeField] private SceneDatabase sceneDatabase;
        [SerializeField] private GraphicsQualityConfig qualityConfig;
        [SerializeField] private WorldSettingsConfig worldSettingsConfig;
        [SerializeField] private GraphicsConfigResolver graphicsConfigResolver;
        [SerializeField] private FogConfig fogConfig;

        protected override void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            base.Awake();
        }

        protected override void Configure(IContainerBuilder builder)
        {
            CoreInstaller.Install(builder, sceneDatabase, loadingScreenView);

            InputInstaller.Install(builder);

            JsonPersistenceInstaller.Install(builder);

            WorldCatalogInstaller.Install(builder);

            WorldSettingsInstaller.Install(builder, worldSettingsConfig);

            GraphicsInstaller.Install(
                builder,
                qualityConfig,
                fogConfig,
                graphicsConfigResolver);
        }
    }
}