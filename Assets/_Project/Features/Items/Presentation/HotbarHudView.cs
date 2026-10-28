using _Project.Features.Items.Domain;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Features.Items.Presentation
{
    public sealed class HotbarHudView : MonoBehaviour
    {
        [SerializeField] private Image[] slotBackgrounds = new Image[Inventory.HotbarSize];
        [SerializeField] private Image[] slotIcons = new Image[Inventory.HotbarSize];
        [SerializeField] private Outline[] slotOutlines = new Outline[Inventory.HotbarSize];
        [SerializeField] private Text[] countLabels = new Text[Inventory.HotbarSize];
        [SerializeField] private CanvasGroup selectionToastGroup;
        [SerializeField] private Text selectionToastLabel;
        [SerializeField] private Text heldItemLabel;
        [SerializeField] private Color slotColor = new Color(0.08f, 0.1f, 0.12f, 0.88f);
        [SerializeField] private Color selectedSlotColor = new Color(0.16f, 0.2f, 0.22f, 0.96f);
        [SerializeField, Min(0f)] private float selectionToastDuration = 2.5f;
        [SerializeField, Min(0f)] private float selectionToastFadeDuration = 0.45f;

        public float SelectionToastDuration => selectionToastDuration;
        public float SelectionToastFadeDuration => selectionToastFadeDuration;
        public Color SlotColor => slotColor;
        public Color SelectedSlotColor => selectedSlotColor;

        public void SetSlot(int slot, Color backgroundColor, bool selected, Sprite icon, string count)
        {
            slotBackgrounds[slot].color = backgroundColor;
            slotOutlines[slot].enabled = selected;
            slotIcons[slot].sprite = icon;
            slotIcons[slot].enabled = icon != null;
            countLabels[slot].text = count;
        }

        public void SetHeldItem(string text)
        {
            heldItemLabel.text = text;
        }

        public void ShowSelectionToast(string text)
        {
            selectionToastLabel.text = text;
            selectionToastGroup.alpha = string.IsNullOrEmpty(text) || selectionToastDuration <= 0f ? 0f : 1f;
        }

        public void SetSelectionToastAlpha(float alpha)
        {
            selectionToastGroup.alpha = Mathf.Clamp01(alpha);
            if (selectionToastGroup.alpha <= 0f)
                selectionToastLabel.text = string.Empty;
        }

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }
    }
}
