using _Project.Features.Persistence.Application;
using _Project.Features.ProceduralWorld.Domain.World;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Project.Features.UI.Application
{
    public interface IPlayWorldUseCase
    {
        void PlayWorld(string name, int seed, bool create = true);
        void DeleteWorld(string name);
    }
    
    public sealed class PlayWorldUseCase : IPlayWorldUseCase
    {
        private readonly LoadSceneController _loadSceneController;
        private readonly IWorldSettingsController _worldSettings;
        private readonly IWorldCatalog _worldCatalog;


        public PlayWorldUseCase(
            LoadSceneController loadSceneController, 
            IWorldSettingsController worldSettings,
            IWorldCatalog worldCatalog)
        {
            _loadSceneController = loadSceneController;
            _worldSettings = worldSettings;
            _worldCatalog = worldCatalog;
        }

        public void PlayWorld(string name, int seed, bool create = true)
        {
            var descriptor = create
                ? _worldCatalog.CreateWorld(name, seed)
                : null;

            _worldSettings.SetName(descriptor?.Name ?? name);
            _worldSettings.SetSeed(descriptor?.Seed ?? seed);

            _loadSceneController.LoadGameScene().Forget();
        }

        public void DeleteWorld(string name)
        {
            _worldCatalog.DeleteWorld(name);
        }
    }
}
