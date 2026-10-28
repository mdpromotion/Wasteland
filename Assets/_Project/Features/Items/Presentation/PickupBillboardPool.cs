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
            Transform root = AliveRoot;
            if (root != null)
                billboard.Root.transform.SetParent(root, false);

            billboard.Root.SetActive(false);
            _free.Add(billboard);
        }

        /// <summary>
        /// The pool root transform when it already exists. Deliberately never
        /// creates the root GameObject: re-creating it during scene teardown
        /// (a pickup's OnDestroy releasing its billboard after the pool root was
        /// destroyed) leaks a fresh object into a closing scene.
        /// </summary>
        private Transform AliveRoot => _root == null ? null : _root;

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
            PickupBillboardTemplate template = _settings.PickupBillboardPrefab;
            if (template == null)
                throw new InvalidOperationException(
                    "Pickup Billboard prefab is not assigned in HotbarHudSettings.");

            PickupBillboardTemplate instance = UnityEngine.Object.Instantiate(template, Root);
            instance.name = "Pickup Billboard";

            RectTransform panel = (RectTransform)instance.transform;
            panel.sizeDelta = _settings.PickupBillboardSize;
            panel.localScale = Vector3.one * _settings.PickupBillboardWorldScale;

            Canvas canvas = instance.Canvas;
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = UnityEngine.Camera.main;

            return new PooledBillboard(
                instance.gameObject,
                canvas,
                instance.IconBackground,
                instance.Icon,
                instance.Title,
                instance.Prompt);
        }
    }

    /// <summary>A borrowed billboard canvas plus the handles needed to refresh it.</summary>
    public sealed class PooledBillboard
    {
        public GameObject Root { get; }
        public Canvas Canvas { get; }
        public Image IconBackground { get; }
        public Image Icon { get; }
        public Text Title { get; }
        public Text Prompt { get; }

        public PooledBillboard(
            GameObject root,
            Canvas canvas,
            Image iconBackground,
            Image icon,
            Text title,
            Text prompt)
        {
            Root = root;
            Canvas = canvas;
            IconBackground = iconBackground;
            Icon = icon;
            Title = title;
            Prompt = prompt;
        }

        /// <summary>Fills the billboard with a specific item's content.</summary>
        public void Configure(ItemDefinitionAsset definition, HotbarHudSettings settings)
        {
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
