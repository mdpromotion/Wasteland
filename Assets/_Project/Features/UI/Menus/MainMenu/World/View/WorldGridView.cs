using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using _Project.Features.Persistence.Domain;

namespace _Project.Features.UI.Menus.MainMenu.World.View
{
    [RequireComponent(typeof(GridLayoutGroup))]
    public class WorldGridView : MonoBehaviour
    {
        [SerializeField] private GameObject worldTabPrefab;
        [SerializeField] private Transform content;

        private GridLayoutGroup _gridLayoutGroup;
        private readonly List<WorldTabView> _spawnedTabs = new();

        private void Awake()
        {
            _gridLayoutGroup = GetComponent<GridLayoutGroup>();
            if (!content)
                content = transform;
        }

        public void BuildGrid(IReadOnlyList<WorldDescriptor> worlds, System.Action<WorldDescriptor> onWorldSelected, System.Action<WorldDescriptor> onDelete)
        {
            Clear();

            foreach (var world in worlds)
            {
                var tabObject = Instantiate(worldTabPrefab, content);
                var tabView = tabObject.GetComponent<WorldTabView>();

                if (!tabView)
                {
                    Debug.LogError($"[{nameof(WorldGridView)}] Prefab {worldTabPrefab.name} has no {nameof(WorldTabView)} component.");
                    continue;
                }

                tabView.Bind(
                    worldName: world.Name,
                    icon: null,
                    onSelect: () => onWorldSelected?.Invoke(world),
                    onDelete: () => onDelete?.Invoke(world)
                );

                _spawnedTabs.Add(tabView);
            }
        }

        public void Clear()
        {
            foreach (var tab in _spawnedTabs)
            {
                if (tab != null)
                    Destroy(tab.gameObject);
            }

            _spawnedTabs.Clear();
        }
    }
}