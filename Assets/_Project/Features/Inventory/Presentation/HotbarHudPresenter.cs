using System;
using _Project.Features.Items.Domain;
using UnityEngine;
using UnityEngine.UI;
using VContainer.Unity;

namespace _Project.Features.Items.Presentation
{
    public sealed class HotbarHudPresenter : IStartable, ITickable, IDisposable
    {
        private const float SelectionToastDuration = 2.5f;
        private const float SelectionToastFadeDuration = 0.45f;
        private static readonly Color SlotColor = new Color(0.08f, 0.1f, 0.12f, 0.88f);
        private static readonly Color SelectedSlotColor = new Color(0.16f, 0.2f, 0.22f, 0.96f);
        private static readonly Color HighlightColor = new Color(0.95f, 0.72f, 0.24f, 1f);

        private readonly Inventory _inventory;
        private readonly HotbarHudRoot _hotbarHudRoot;
        private readonly IItemIconProvider _itemIconProvider;
        private readonly Image[] _slotBackgrounds = new Image[Inventory.HotbarSize];
        private readonly Image[] _slotIcons = new Image[Inventory.HotbarSize];
        private readonly Outline[] _slotOutlines = new Outline[Inventory.HotbarSize];
        private readonly Text[] _countLabels = new Text[Inventory.HotbarSize];
        private GameObject _canvasObject;
        private CanvasGroup _selectionToastGroup;
        private Text _selectionToastLabel;
        private Text _heldItemLabel;
        private int _lastSelectedSlot;
        private float _selectionToastTimeRemaining;

        public HotbarHudPresenter(
            Inventory inventory,
            HotbarHudRoot hotbarHudRoot,
            IItemIconProvider itemIconProvider)
        {
            _inventory = inventory;
            _hotbarHudRoot = hotbarHudRoot;
            _itemIconProvider = itemIconProvider;
        }

        public void Start()
        {
            Canvas canvas = CreateCanvas();

            CreateHotbar(canvas.transform);
            CreateSelectionToast(canvas.transform);
            CreateHeldItemLabel(canvas.transform);
            _lastSelectedSlot = _inventory.SelectedSlot;
            _inventory.Changed += Render;
            Render();
        }

        private Canvas CreateCanvas()
        {
            _canvasObject = new GameObject(
                "Inventory Canvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));

            _canvasObject.transform.SetParent(_hotbarHudRoot.Transform, false);

            Canvas canvas = _canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = _canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            return canvas;
        }

        private void CreateHotbar(Transform canvas)
        {
            var hotbarObject = new GameObject("Inventory Hotbar", typeof(RectTransform));
            RectTransform root = hotbarObject.GetComponent<RectTransform>();
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
            _slotIcons[slot] = CreateSlotIcon(rect, slot);
            _countLabels[slot] = CreateLabel(rect, $"Count {slot + 1}", string.Empty, 0f, 0.24f, 12, TextAnchor.LowerRight);
        }

        private static Image CreateSlotIcon(RectTransform parent, int slot)
        {
            var iconObject = new GameObject($"Item Icon {slot + 1}", typeof(RectTransform), typeof(Image));
            RectTransform rect = iconObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.12f, 0.22f);
            rect.anchorMax = new Vector2(0.88f, 0.76f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image icon = iconObject.GetComponent<Image>();
            icon.preserveAspect = true;
            icon.raycastTarget = false;
            icon.enabled = false;
            return icon;
        }

        private void CreateSelectionToast(Transform canvas)
        {
            var toastObject = new GameObject("Selected Item Toast", typeof(RectTransform), typeof(CanvasGroup), typeof(Text));
            RectTransform rect = toastObject.GetComponent<RectTransform>();
            rect.SetParent(canvas, false);
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.sizeDelta = new Vector2(480f, 42f);
            rect.anchoredPosition = new Vector2(0f, 124f);

            _selectionToastGroup = toastObject.GetComponent<CanvasGroup>();
            _selectionToastGroup.alpha = 0f;
            _selectionToastLabel = ConfigureLabel(toastObject.GetComponent<Text>(), 24, TextAnchor.MiddleCenter);
        }

        private void CreateHeldItemLabel(Transform canvas)
        {
            var heldItemObject = new GameObject("Held Item Label", typeof(RectTransform), typeof(Text));
            RectTransform rect = heldItemObject.GetComponent<RectTransform>();
            rect.SetParent(canvas, false);
            rect.anchorMin = new Vector2(1f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(1f, 0f);
            rect.sizeDelta = new Vector2(520f, 42f);
            rect.anchoredPosition = new Vector2(-24f, 24f);

            _heldItemLabel = ConfigureLabel(heldItemObject.GetComponent<Text>(), 20, TextAnchor.LowerRight);
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

            Text label = ConfigureLabel(labelObject.GetComponent<Text>(), fontSize, alignment);
            label.text = value;
            return label;
        }

        private static Text ConfigureLabel(Text label, int fontSize, TextAnchor alignment)
        {
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
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
                _slotIcons[slot].sprite = stack == null ? null : _itemIconProvider.GetIcon(stack.Item.Id);
                _slotIcons[slot].enabled = _slotIcons[slot].sprite != null;
                _countLabels[slot].text = stack == null ? string.Empty : $"×{stack.Count}";
            }

            ItemDefinition selectedItem = _inventory.SelectedItem;
            _heldItemLabel.text = selectedItem == null
                ? "В руках: —"
                : $"В руках: {selectedItem.DisplayName}";

            if (_lastSelectedSlot != _inventory.SelectedSlot)
            {
                _lastSelectedSlot = _inventory.SelectedSlot;
                ShowSelectionToast(selectedItem);
            }
        }

        private void ShowSelectionToast(ItemDefinition item)
        {
            if (item == null)
            {
                _selectionToastLabel.text = string.Empty;
                _selectionToastGroup.alpha = 0f;
                _selectionToastTimeRemaining = 0f;
                return;
            }

            _selectionToastLabel.text = item.DisplayName;
            _selectionToastGroup.alpha = 1f;
            _selectionToastTimeRemaining = SelectionToastDuration;
        }

        public void Tick()
        {
            if (_selectionToastTimeRemaining <= 0f)
                return;

            _selectionToastTimeRemaining = Mathf.Max(0f, _selectionToastTimeRemaining - Time.unscaledDeltaTime);
            _selectionToastGroup.alpha = _selectionToastTimeRemaining <= SelectionToastFadeDuration
                ? _selectionToastTimeRemaining / SelectionToastFadeDuration
                : 1f;

            if (_selectionToastTimeRemaining <= 0f)
                _selectionToastLabel.text = string.Empty;
        }

        public void Dispose()
        {
            _inventory.Changed -= Render;
            if (_canvasObject != null)
                UnityEngine.Object.Destroy(_canvasObject);
        }
    }
}
