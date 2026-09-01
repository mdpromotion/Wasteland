using System.Collections.Generic;
using _Project.Features.Persistence.Application;
using _Project.Features.Persistence.Domain;
using _Project.Features.UI.Application;
using _Project.Features.UI.Menus.MainMenu.View;
using _Project.Features.UI.Menus.MainMenu.World.View;
using UnityEngine;
using VContainer;

namespace _Project.Features.UI.Menus.MainMenu.World
{
    /// <summary>
    /// Handles the world selection/creation menu: displays existing worlds,
    /// lets the player create a new one or pick an existing one to play.
    /// </summary>
    public class WorldMenuPresenter : MonoBehaviour
    {
        [SerializeField] private MainMenuView worldMenuView;
        [SerializeField] private SeedFieldView seedField;
        [SerializeField] private WorldFieldView worldField;

        [SerializeField] private WorldButton createWorldButton;
        [SerializeField] private WorldButton chooseCreateWorldButton;

        [SerializeField] private WorldGridView worldGridView;

        /// <summary>menus[0] = "create world" screen, menus[1] = "world list" screen.</summary>
        [SerializeField] private GameObject[] menus;

        private IPlayWorldUseCase _playWorldUseCase;
        private IWorldCatalog _worldCatalog;

        private bool _isLoading;
        private List<WorldDescriptor> _worlds = new();

        [Inject]
        public void Construct(IPlayWorldUseCase playWorldUseCase, IWorldCatalog worldCatalog)
        {
            _playWorldUseCase = playWorldUseCase;
            _worldCatalog = worldCatalog;
        }

        private void Start()
        {
            _isLoading = false;
            Subscribe();
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        private void Subscribe()
        {
            worldMenuView.MenuToggled += OnMenuToggled;
            createWorldButton.ButtonClicked += OnCreateWorldButtonClicked;
            chooseCreateWorldButton.ButtonClicked += OnChooseCreateWorldButtonClicked;
        }

        private void Unsubscribe()
        {
            worldMenuView.MenuToggled -= OnMenuToggled;
            createWorldButton.ButtonClicked -= OnCreateWorldButtonClicked;
            chooseCreateWorldButton.ButtonClicked -= OnChooseCreateWorldButtonClicked;
        }

        private void OnMenuToggled(bool isOpen)
        {
            if (!isOpen) return;

            PrefillAvailableWorldName();
            RefreshWorldGrid();
            UpdateWorldMenuVisibility();
        }

        private void OnChooseCreateWorldButtonClicked()
        {
            ShowCreateWorldMenu();
            PrefillAvailableWorldName();
        }

        private void OnCreateWorldButtonClicked()
        {
            if (_isLoading) return;

            var worldName = ResolveWorldName();
            var seed = ResolveSeed();

            _playWorldUseCase.PlayWorld(worldName, seed);
            _isLoading = true;
        }

        private void OnWorldSelected(WorldDescriptor world)
        {
            if (_isLoading) return;

            _playWorldUseCase.PlayWorld(world.Name, world.Seed, create: false);
            _isLoading = true;
        }

        private void OnWorldDeleted(WorldDescriptor world)
        {
            _worldCatalog.DeleteWorld(world.Name);
            RefreshWorldGrid();
        }

        /// <summary>Sets the world name field to the next available default name.</summary>
        private void PrefillAvailableWorldName()
        {
            worldField.SetName(_worldCatalog.GetAvailableWorldName());
        }

        /// <summary>Returns the name typed by the player, or a generated default if empty/invalid.</summary>
        private string ResolveWorldName()
        {
            return worldField.TryGetName(out var worldName)
                ? worldName
                : _worldCatalog.GetAvailableWorldName();
        }

        /// <summary>Returns the seed typed by the player, or a random one if empty/invalid.</summary>
        private int ResolveSeed()
        {
            return seedField.TryGetSeed(out var seed)
                ? seed
                : Random.Range(int.MinValue, int.MaxValue);
        }

        private void RefreshWorldGrid()
        {
            var worldNames = _worldCatalog.GetWorldNames();
            _worlds = new List<WorldDescriptor>(worldNames.Length);

            foreach (var worldName in worldNames)
                _worlds.Add(_worldCatalog.GetWorld(worldName));

            worldGridView.BuildGrid(_worlds, OnWorldSelected, OnWorldDeleted);
        }

        private void UpdateWorldMenuVisibility()
        {
            bool hasWorlds = _worlds.Count > 0;
            SetMenuActive(createMenuActive: !hasWorlds, listMenuActive: hasWorlds);
        }

        private void ShowCreateWorldMenu()
        {
            SetMenuActive(createMenuActive: true, listMenuActive: false);
        }

        private void SetMenuActive(bool createMenuActive, bool listMenuActive)
        {
            menus[0].SetActive(createMenuActive);
            menus[1].SetActive(listMenuActive);
        }
    }
}