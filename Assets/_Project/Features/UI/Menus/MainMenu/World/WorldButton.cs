using System;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Features.UI.Menus.MainMenu.World
{
    public class WorldButton : MonoBehaviour
    {
        private Button _menuButton;
        
        public event Action ButtonClicked;

        private void Awake()
        {
            _menuButton = GetComponent<Button>();
            _menuButton.onClick.AddListener(OnButtonClick);
        }

        private void OnButtonClick()
        {
            ButtonClicked?.Invoke();
        }

        private void OnDestroy()
        {
            _menuButton.onClick.RemoveAllListeners();
        }
    }
}
