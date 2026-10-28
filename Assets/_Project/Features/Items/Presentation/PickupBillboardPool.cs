using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using _Project.Features.Items.Infrastructure;

namespace _Project.Features.Items.Presentation
{
    /// <summary>
    /// Runtime pool of pickup billboard canvases. Billboard content is refreshed on
    /// acquire, so one canvas object can serve many dropped items over time while
    /// the number of live UGUI canvases stays bounded.
    /// </summary>
    public sealed class PickupBillboardPool
    {
        private readonly HotbarHudSettings _settings;
        private readonly List<PooledBillboard> _free = new List<PooledBillboard>();
        private readonly List<PooledBillboard> _inUse = new List<PooledBillboard>();
        private Transform _root;

        public PickupBillboardPool(HotbarHudSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public PooledBillboard Acquire(Transform parent)
        {
            PooledBillboard billboard = TakeFree() ?? Create();

            billboard.Root.transform.SetParent(parent, false);
            billboard.Root.SetActive(true);
            _inUse.Add(billboard);
            return billboard;
        }

        public void Release(PooledBillboard billboard)
        {
            if (billboard == null || !_inUse.Remove(billboard))
                return;

            billboard.Canvas.enabled = false;
            billboard.Root.transform.SetParent(Root, false);
            billboard.Root.SetActive(false);
            _free.Add(billboard);
        }

        private PooledBillboard TakeFree()
        {
            if (_free.Count == 0)
                return null;

            PooledBillboard last = _free[_free.Count - 1];
            _free.RemoveAt(_free.Count - 1);
            return last;
        }

        private Transform Root
        {
            get
            {
                if (_root == null)
                    _root = new GameObject("Pickup Billboard Pool").transform;

                return _root;
            }
        }

        private PooledBillboard Create()
        {
            var root = new GameObject(
                "Pickup Billboard",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(Image),
                typeof(Outline));

            RectTransform panel = root.GetComponent<RectTransform>();
            panel.sizeDelta = _settings.PickupBillboardSize;
            panel.localScale = Vector3.one * _settings.PickupBillboardWorldScale;

            var canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = UnityEngine.Camera.main;

            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 10f;

            var background = root.GetComponent<Image>();
            background.raycastTarget = false;

            Outline outline = root.GetComponent<Outline>();
            outline.effectColor = _settings.HighlightColor;
            outline.effectDistance = new Vector2(2f, 2f);

            PooledImage iconBackground = CreateIconBackground(panel);
            PooledIcon icon = CreateIcon(iconBackground.Rect);

            PooledText title = CreateText(panel, "Item Name", _settings.PickupTitleFontSize, TextAnchor.MiddleLeft);
            title.Rect.anchorMin = new Vector2(0.25f, 0.46f);
            title.Rect.anchorMax = new Vector2(0.97f, 0.95f);

            PooledImage key = CreateKeyBackground(panel);
            PooledText keyLabel = CreateText(key.Rect, "Key E", _settings.PickupPromptFontSize, TextAnchor.MiddleCenter);
            keyLabel.Rect.anchorMin = Vector2.zero;
            keyLabel.Rect.anchorMax = Vector2.one;

            PooledText prompt = CreateText(panel, "Pickup Text", _settings.PickupPromptFontSize, TextAnchor.MiddleLeft);
            prompt.Rect.anchorMin = new Vector2(0.43f, 0.08f);
            prompt.Rect.anchorMax = new Vector2(0.97f, 0.42f);

            return new PooledBillboard(
                root,
                canvas,
                background,
                iconBackground.Image,
                icon.Image,
                title.Text,
                prompt.Text);
        }

        private PooledImage CreateIconBackground(RectTransform panel)
        {
            var iconBackgroundObject = new GameObject(
                "Item Icon Background",
                typeof(RectTransform),
                typeof(Image));

            RectTransform rect = iconBackgroundObject.GetComponent<RectTransform>();
            rect.SetParent(panel, false);
            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.anchorMax = new Vector2(0f, 0.5f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.sizeDelta = Vector2.one * _settings.PickupIconSize;
            rect.anchoredPosition = new Vector2(12f, 0f);

            Image image = iconBackgroundObject.GetComponent<Image>();
            image.raycastTarget = false;
            return new PooledImage(rect, image);
        }

        private static PooledIcon CreateIcon(RectTransform iconBackground)
        {
            var iconObject = new GameObject("Item Icon", typeof(RectTransform), typeof(Image));
            RectTransform rect = iconObject.GetComponent<RectTransform>();
            rect.SetParent(iconBackground, false);
            rect.anchorMin = new Vector2(0.08f, 0.08f);
            rect.anchorMax = new Vector2(0.92f, 0.92f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image image = iconObject.GetComponent<Image>();
            image.preserveAspect = true;
            image.raycastTarget = false;
            image.enabled = false;
            return new PooledIcon(rect, image);
        }

        private PooledImage CreateKeyBackground(RectTransform panel)
        {
            var keyObject = new GameObject(
                "Pickup Key",
                typeof(RectTransform),
                typeof(Image),
                typeof(Outline));

            RectTransform rect = keyObject.GetComponent<RectTransform>();
            rect.SetParent(panel, false);
            rect.anchorMin = new Vector2(0.25f, 0.08f);
            rect.anchorMax = new Vector2(0.39f, 0.42f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image image = keyObject.GetComponent<Image>();
            image.color = _settings.SelectedSlotColor;
            image.raycastTarget = false;

            Outline outline = keyObject.GetComponent<Outline>();
            outline.effectColor = _settings.HighlightColor;
            outline.effectDistance = Vector2.one;
            return new PooledImage(rect, image);
        }

        private static PooledText CreateText(
            RectTransform parent,
            string objectName,
            int fontSize,
            TextAnchor alignment)
        {
            var textObject = new GameObject(objectName, typeof(RectTransform), typeof(Text));
            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Text text = textObject.GetComponent<Text>();
            text.font = Fonts.Resolve();
            text.fontSize = fontSize;
            text.color = Color.white;
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.raycastTarget = false;
            return new PooledText(rect, text);
        }

        private readonly struct PooledImage
        {
            public RectTransform Rect { get; }
            public Image Image { get; }

            public PooledImage(RectTransform rect, Image image)
            {
                Rect = rect;
                Image = image;
            }
        }

        private readonly struct PooledText
        {
            public RectTransform Rect { get; }
            public Text Text { get; }

            public PooledText(RectTransform rect, Text text)
            {
                Rect = rect;
                Text = text;
            }
        }

        private readonly struct PooledIcon
        {
            public RectTransform Rect { get; }
            public Image Image { get; }

            public PooledIcon(RectTransform rect, Image image)
            {
                Rect = rect;
                Image = image;
            }
        }

        private static class Fonts
        {
            public static Font Resolve()
            {
                return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }
        }
    }

    /// <summary>A borrowed billboard canvas plus the handles needed to refresh it.</summary>
    public sealed class PooledBillboard
    {
        public GameObject Root { get; }
        public Canvas Canvas { get; }
        public Image Background { get; }
        public Image IconBackground { get; }
        public Image Icon { get; }
        public Text Title { get; }
        public Text Prompt { get; }

        public PooledBillboard(
            GameObject root,
            Canvas canvas,
            Image background,
            Image iconBackground,
            Image icon,
            Text title,
            Text prompt)
        {
            Root = root;
            Canvas = canvas;
            Background = background;
            IconBackground = iconBackground;
            Icon = icon;
            Title = title;
            Prompt = prompt;
        }

        /// <summary>Fills the billboard with a specific item's content.</summary>
        public void Configure(ItemDefinitionAsset definition, HotbarHudSettings settings)
        {
            Background.color = settings.SlotColor;
            IconBackground.color = definition.HeldColor;

            if (definition.Icon != null)
            {
                Icon.sprite = definition.Icon;
                Icon.enabled = true;
            }
            else
            {
                Icon.enabled = false;
            }

            Title.text = definition.DisplayName;
            Prompt.text = settings.PickupPromptText;
        }
    }
}
