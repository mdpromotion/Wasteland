using UnityEngine;
using UnityEngine.UI;

namespace _Project.Features.UI.Menus.MainMenu.World.View
{
    public class WorldTabView : MonoBehaviour
    {
        [SerializeField] private Text worldNameText;
        [SerializeField] private Image worldIconImage;
        [SerializeField] private Button selectButton;
        [SerializeField] private Button deleteButton;

        public void Bind(string worldName, Sprite icon, System.Action onSelect, System.Action onDelete)
        {
            if (worldNameText)
                worldNameText.text = worldName;

            if (worldIconImage)
            {
                worldIconImage.sprite = icon;
                worldIconImage.enabled = icon != null;
            }

            if (selectButton)
            {
                selectButton.onClick.RemoveAllListeners();
                if (onSelect != null)
                    selectButton.onClick.AddListener(() => onSelect());
                if (onDelete != null)
                    deleteButton.onClick.AddListener(() => onDelete());
            }
        }
    }
}