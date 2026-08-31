using System;
using _Project.Features.Persistence.Application;
using _Project.Features.ProceduralWorld.Domain.World;
using _Project.Features.UI.Application;
using _Project.Features.UI.Menus.MainMenu.View;
using _Project.Features.UI.Menus.MainMenu.World.View;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using VContainer;

namespace _Project.Features.UI.Menus.MainMenu.World
{
    public class WorldMenuPresenter : MonoBehaviour
    {
        [SerializeField] private MainMenuView worldMenuView;
        [SerializeField] private SeedFieldView seedField;
        [SerializeField] private WorldFieldView worldField;
        [SerializeField] private CreateWorldButton createWorldButton;
        
        private LoadSceneController _loadSceneController;
        private IWorldSettingsController _worldSettings;
        private IWorldCatalog _worldCatalog;
        
        private bool _isLoading;
        
        [Inject]
        public void Construct(LoadSceneController loadSceneController, IWorldSettingsController worldSettings, IWorldCatalog worldCatalog)
        {
            _loadSceneController = loadSceneController;
            _worldSettings = worldSettings;
            _worldCatalog = worldCatalog;
        }

        private void Start()
        {
            _isLoading = false;
            
            worldMenuView.MenuToggled += OnMenuToggled;
            createWorldButton.ButtonClicked += OnButtonClicked;
        }

        private void OnMenuToggled(bool value)
        {
            var availableWorldName = _worldCatalog.GetAvailableWorldName();
            worldField.SetName(availableWorldName);
        }

        private void OnButtonClicked()
        {
            if (_isLoading)
                return;

            if (!worldField.TryGetName(out var worldName))
                worldName = _worldCatalog.GetAvailableWorldName();
            
            if (!seedField.TryGetSeed(out var seed))
                seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            
            var descriptor = _worldCatalog.CreateWorld(worldName, seed);
            
            _worldSettings.SetName(descriptor.Name);
            _worldSettings.SetSeed(descriptor.Seed);
            
            _loadSceneController.LoadGameScene().Forget();
            _isLoading = true;
        }

        private void OnDestroy()
        {
            worldMenuView.MenuToggled -= OnMenuToggled;
            createWorldButton.ButtonClicked -= OnButtonClicked;
        }
    }
}
