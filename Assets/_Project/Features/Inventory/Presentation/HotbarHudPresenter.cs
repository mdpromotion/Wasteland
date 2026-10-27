using System;
using _Project.Features.Items.Domain;
using UnityEngine;
using UnityEngine.UI;
using VContainer.Unity;

namespace _Project.Features.Items.Presentation
{
    public sealed class HotbarHudPresenter : IStartable, IDisposable
    {
        private static readonly Color SlotColor = new Color(0.08f, 0.1f, 0.12f, 0.88f);
        private static readonly Color SelectedSlotColor = new Color(0.16f, 0.2f, 0.22f, 0.96f);
        private static readonly Color HighlightColor = new Color(0.95f, 0.72f, 0.24f, 1f);

        private readonly Inventory _inventory;
        private readonly Image[] _slotBackgrounds = new Image[Inventory.HotbarSize];
        private readonly Outline[] _slotOutlines = new Outline[Inventory.HotbarSize];
        private readonly Text[] _itemLabels = new Text[Inventory.HotbarSize];
        private readonly Text[] _countLabels = new Text[Inventory.HotbarSize];
        private GameObject _root;

        public HotbarHudPresenter(Inventory inventory)
        {
            _inventory = inventory;
        }

        public void Start()
        {
            Canvas canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogWarning("Inventory hotbar could not find a Canvas in the game scene.");
                return;
            }

            CreateHotbar(canvas.transform);
            _inventory.Changed += Render;
            Render();
        }

        private void CreateHotbar(Transform canvas)
        {
            _root = new GameObject("Inventory Hotbar", typeof(RectTransform));
            RectTransform root = _root.GetComponent<RectTransform>();
            root.SetParent(canvas, false);
            root.anchorMin = new Vector2(0.5f, 0f);
            root.anchorMax = new Vector2(0.5f, 0f);
            root.pivot = new Vector2(0.5f, 0f);
            root.sizeDelta = new Vector2(720f, 98f);
            root.anchoredPosition = new Vector2(0f, 20f);

            for (int slot = 0; slot < Inventory.HotbarSize; slot++)
                CreateSlot(root, slot);
        }

        private void CreateSlot(RectTransform parent, int slot)
        {
            var slotObject = new GameObject($"Slot {slot + 1}", typeof(RectTransform), typeof(Image), typeof(Outline));
            RectTransform rect = slotObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.sizeDelta = new Vector2(72f, 82f);
            rect.anchoredPosition = new Vector2((slot - 4) * 79f, 8f);

            _slotBackgrounds[slot] = slotObject.GetComponent<Image>();
            _slotBackgrounds[slot].color = SlotColor;
            _slotBackgrounds[slot].raycastTarget = false;

            _slotOutlines[slot] = slotObject.GetComponent<Outline>();
            _slotOutlines[slot].effectColor = HighlightColor;
            _slotOutlines[slot].effectDistance = new Vector2(2f, 2f);

            CreateLabel(rect, $"Key {slot + 1}", (slot + 1).ToString(), 0.76f, 1f, 12, TextAnchor.UpperCenter);
            _itemLabels[slot] = CreateLabel(rect, $"Item {slot + 1}", string.Empty, 0.25f, 0.74f, 11, TextAnchor.MiddleCenter);
            _countLabels[slot] = CreateLabel(rect, $"Count {slot + 1}", string.Empty, 0f, 0.24f, 12, TextAnchor.LowerRight);
        }

        private static Text CreateLabel(
            RectTransform parent,
            string name,
            string value,
            float minY,
            float maxY,
            int fontSize,
            TextAnchor alignment)
        {
            var labelObject = new GameObject(name, typeof(RectTransform), typeof(Text));
            RectTransform rect = labelObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.06f, minY);
            rect.anchorMax = new Vector2(0.94f, maxY);
            rect.offsetMin = new Vector2(0f, 2f);
            rect.offsetMax = new Vector2(0f, -2f);

            Text label = labelObject.GetComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            label.text = value;
            label.fontSize = fontSize;
            label.color = Color.white;
            label.alignment = alignment;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Truncate;
            label.raycastTarget = false;
            return label;
        }

        private void Render()
        {
            for (int slot = 0; slot < Inventory.HotbarSize; slot++)
            {
                InventoryStack stack = _inventory.GetSlot(slot);
                bool selected = slot == _inventory.SelectedSlot;

                _slotBackgrounds[slot].color = selected ? SelectedSlotColor : SlotColor;
                _slotOutlines[slot].enabled = selected;
                _itemLabels[slot].text = stack == null ? string.Empty : stack.Item.DisplayName;
                _countLabels[slot].text = stack == null ? string.Empty : $"×{stack.Count}";
            }
        }

        public void Dispose()
        {
            _inventory.Changed -= Render;
            if (_root != null)
                UnityEngine.Object.Destroy(_root);
        }
    }
}
