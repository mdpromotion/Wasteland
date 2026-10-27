using UnityEngine;
using UnityEngine.UI;
using _Project.Features.Items.Infrastructure;

namespace _Project.Features.Items.Presentation
{
    [DisallowMultipleComponent]
    public sealed class WorldItemPickupBillboard : MonoBehaviour
    {
        private ItemDefinitionAsset _itemDefinition;
        private HotbarHudSettings _settings;
        private Transform _billboardTransform;
        private Canvas _canvas;
        private UnityEngine.Camera _camera;

        public void Initialize(ItemDefinitionAsset itemDefinition, HotbarHudSettings settings)
        {
            _itemDefinition = itemDefinition;
            _settings = settings;
            _camera = UnityEngine.Camera.main;
            CreateBillboard();
            SetVisible(false);
        }

        private void CreateBillboard()
        {
            var canvasObject = new GameObject(
                "Pickup Billboard",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(Image),
                typeof(Outline));
            _billboardTransform = canvasObject.transform;
            _billboardTransform.SetParent(transform, false);

            RectTransform panel = canvasObject.GetComponent<RectTransform>();
            panel.sizeDelta = _settings.PickupBillboardSize;
            panel.localScale = Vector3.one * _settings.PickupBillboardWorldScale;

            _canvas = canvasObject.GetComponent<Canvas>();
            _canvas.renderMode = RenderMode.WorldSpace;
            _canvas.worldCamera = _camera;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 10f;

            Image background = canvasObject.GetComponent<Image>();
            background.color = _settings.SlotColor;
            background.raycastTarget = false;

            Outline outline = canvasObject.GetComponent<Outline>();
            outline.effectColor = _settings.HighlightColor;
            outline.effectDistance = new Vector2(2f, 2f);

            CreateItemIcon(panel);
            CreateTitle(panel);
            CreatePickupPrompt(panel);
        }

        private void CreateItemIcon(RectTransform panel)
        {
            var iconBackgroundObject = new GameObject(
                "Item Icon Background",
                typeof(RectTransform),
                typeof(Image));
            RectTransform iconBackgroundRect = iconBackgroundObject.GetComponent<RectTransform>();
            iconBackgroundRect.SetParent(panel, false);
            iconBackgroundRect.anchorMin = new Vector2(0f, 0.5f);
            iconBackgroundRect.anchorMax = new Vector2(0f, 0.5f);
            iconBackgroundRect.pivot = new Vector2(0f, 0.5f);
            iconBackgroundRect.sizeDelta = Vector2.one * _settings.PickupIconSize;
            iconBackgroundRect.anchoredPosition = new Vector2(12f, 0f);

            Image iconBackground = iconBackgroundObject.GetComponent<Image>();
            iconBackground.color = _itemDefinition.HeldColor;
            iconBackground.raycastTarget = false;

            if (_itemDefinition.Icon == null)
                return;

            var iconObject = new GameObject("Item Icon", typeof(RectTransform), typeof(Image));
            RectTransform iconRect = iconObject.GetComponent<RectTransform>();
            iconRect.SetParent(iconBackgroundRect, false);
            iconRect.anchorMin = new Vector2(0.08f, 0.08f);
            iconRect.anchorMax = new Vector2(0.92f, 0.92f);
            iconRect.offsetMin = Vector2.zero;
            iconRect.offsetMax = Vector2.zero;

            Image icon = iconObject.GetComponent<Image>();
            icon.sprite = _itemDefinition.Icon;
            icon.preserveAspect = true;
            icon.raycastTarget = false;
        }

        private void CreateTitle(RectTransform panel)
        {
            Text title = CreateText(
                panel,
                "Item Name",
                _itemDefinition.DisplayName,
                _settings.PickupTitleFontSize,
                TextAnchor.MiddleLeft);
            RectTransform rect = title.rectTransform;
            rect.anchorMin = new Vector2(0.25f, 0.46f);
            rect.anchorMax = new Vector2(0.97f, 0.95f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private void CreatePickupPrompt(RectTransform panel)
        {
            var keyObject = new GameObject("Pickup Key", typeof(RectTransform), typeof(Image), typeof(Outline));
            RectTransform keyRect = keyObject.GetComponent<RectTransform>();
            keyRect.SetParent(panel, false);
            keyRect.anchorMin = new Vector2(0.25f, 0.08f);
            keyRect.anchorMax = new Vector2(0.39f, 0.42f);
            keyRect.offsetMin = Vector2.zero;
            keyRect.offsetMax = Vector2.zero;

            Image keyBackground = keyObject.GetComponent<Image>();
            keyBackground.color = _settings.SelectedSlotColor;
            keyBackground.raycastTarget = false;

            Outline keyOutline = keyObject.GetComponent<Outline>();
            keyOutline.effectColor = _settings.HighlightColor;
            keyOutline.effectDistance = Vector2.one;

            Text key = CreateText(keyRect, "Key E", "E", _settings.PickupPromptFontSize, TextAnchor.MiddleCenter);
            key.rectTransform.anchorMin = Vector2.zero;
            key.rectTransform.anchorMax = Vector2.one;
            key.rectTransform.offsetMin = Vector2.zero;
            key.rectTransform.offsetMax = Vector2.zero;

            Text prompt = CreateText(
                panel,
                "Pickup Text",
                _settings.PickupPromptText,
                _settings.PickupPromptFontSize,
                TextAnchor.MiddleLeft);
            RectTransform promptRect = prompt.rectTransform;
            promptRect.anchorMin = new Vector2(0.43f, 0.08f);
            promptRect.anchorMax = new Vector2(0.97f, 0.42f);
            promptRect.offsetMin = Vector2.zero;
            promptRect.offsetMax = Vector2.zero;
        }

        private Text CreateText(
            RectTransform parent,
            string objectName,
            string value,
            int fontSize,
            TextAnchor alignment)
        {
            var textObject = new GameObject(objectName, typeof(RectTransform), typeof(Text));
            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);

            Text text = textObject.GetComponent<Text>();
            text.font = _settings.Font != null
                ? _settings.Font
                : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.color = Color.white;
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.raycastTarget = false;
            text.text = value;
            return text;
        }

        public void SetVisible(bool visible)
        {
            if (_canvas != null)
                _canvas.enabled = visible;
        }

        private void LateUpdate()
        {
            if (_canvas == null || !_canvas.enabled)
                return;

            if (!_camera)
                _camera = UnityEngine.Camera.main;
            if (!_camera)
                return;
            if (_canvas.worldCamera != _camera)
                _canvas.worldCamera = _camera;

            _billboardTransform.position = transform.position + _settings.PickupBillboardOffset;
            _billboardTransform.rotation = Quaternion.LookRotation(
                _camera.transform.position - _billboardTransform.position,
                Vector3.up);
        }
    }
}
