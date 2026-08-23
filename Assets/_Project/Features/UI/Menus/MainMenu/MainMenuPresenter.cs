using System;
using System.Collections.Generic;
using _Project.Features.ProceduralWorld.Domain.World;
using _Project.Features.UI.Application;
using _Project.Features.UI.Infrastructure;
using _Project.Features.UI.Menus.MainMenu.View;
using _Project.Features.UI.Menus.SettingsMenu;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;
using Random = System.Random;

namespace _Project.Features.UI.Menus.MainMenu
{
    public enum MenuType
    {
        WorldMenu,
        StartGame,
        Settings
    }

    [Serializable]
    public struct MenuEntry
    {
        public MenuType menuType;
        public MainMenuView mainMenuView;
        public bool isVisibleWhenActive;
    }
    
    public class MainMenuPresenter : MonoBehaviour
    {
        [SerializeField] private MenuButton[] buttons;
        [SerializeField] private List<MenuEntry> menus;
        [SerializeField] private SettingsMenuView settingsMenuView;
        [SerializeField] private SeedFieldView seedField;
        
        private LoadSceneController _loadSceneController;
        private SceneTransitionService _sceneTransitionService;
        private IWorldSettingsController _worldSettings;

        private bool _isLoading;
        private bool _isWorldMenuVisible;
        private bool _isSettingsMenuVisible;

        [Inject]
        public void Construct(LoadSceneController loadSceneController, SceneTransitionService sceneTransitionService, IWorldSettingsController worldSettings)
        {
            _loadSceneController = loadSceneController;
            _sceneTransitionService = sceneTransitionService;
            _worldSettings = worldSettings;
        }

        public void Start()
        {
            _isLoading = false;
            
            HandleWorldMenu();
            
            foreach (var button in buttons)
            {
                button.ButtonClicked += OnButtonClicked;
            }
            
            settingsMenuView.ToggleMenuRequested += OnStateChanged;
            
            _sceneTransitionService.CompleteAsync().Forget();
        }

        public void OnButtonClicked(MenuType buttonType)
        {
            switch (buttonType)
            {
                case MenuType.WorldMenu:
                    HandleWorldMenu();
                    break;
                case MenuType.StartGame:
                    HandleStartGame();
                    break;
                case MenuType.Settings:
                    HandleSettings();
                    break;
            }
        }


        private void HandleWorldMenu()
        {
            _isWorldMenuVisible= !_isWorldMenuVisible;
            SetMenuState(MenuType.WorldMenu, _isWorldMenuVisible);
        }

        private void HandleStartGame()
        {
            if (_isLoading)
                return;

            if (!seedField.TryGetSeed(out var seed))
                seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            
            _worldSettings.SetSeed(seed);
            
            _loadSceneController.LoadGameScene().Forget();
            _isLoading = true;
        }

        private void HandleSettings()
        {
            _isSettingsMenuVisible = !_isSettingsMenuVisible;
            settingsMenuView.Toggle(_isSettingsMenuVisible);
        }

        private void OnStateChanged(bool state)
        {
            _isSettingsMenuVisible = state;
        }
        
        private void SetMenuState(MenuType menuType, bool state)
        {
            foreach (var menu in menus)
            {
                menu.mainMenuView.ToggleMenu(false);
                
                if (menu.menuType != menuType)
                    continue;
                
                bool isVisible = menu.isVisibleWhenActive != state;

                menu.mainMenuView.ToggleMenu(isVisible);
            }
        }

        public void OnDestroy()
        {
            foreach (var button in buttons)
            {
                button.ButtonClicked -= OnButtonClicked;
            }
            settingsMenuView.ToggleMenuRequested -= OnStateChanged;
        }
    }
}
