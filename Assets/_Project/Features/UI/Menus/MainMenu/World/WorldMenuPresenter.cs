using System;
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
        [SerializeField] private SeedFieldView seedField;
        [SerializeField] private CreateWorldButton createWorldButton;
        
        private LoadSceneController _loadSceneController;
        private IWorldSettingsController _worldSettings;
        
        private bool _isLoading;
        
        [Inject]
        public void Construct(LoadSceneController loadSceneController, IWorldSettingsController worldSettings)
        {
            _loadSceneController = loadSceneController;
            _worldSettings = worldSettings;
        }

        private void Start()
        {
            _isLoading = false;
            
            createWorldButton.ButtonClicked += OnButtonClicked;
        }

        private void OnButtonClicked()
        {
            if (_isLoading)
                return;

            if (!seedField.TryGetSeed(out var seed))
                seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            
            _worldSettings.SetSeed(seed);
            
            _loadSceneController.LoadGameScene().Forget();
            _isLoading = true;
        }

        private void OnDestroy()
        {
            createWorldButton.ButtonClicked -= OnButtonClicked;
        }
    }
}
