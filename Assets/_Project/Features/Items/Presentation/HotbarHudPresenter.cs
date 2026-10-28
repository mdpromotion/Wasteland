using System;
using System.Collections.Generic;
using System.Text;
using _Project.Features.Items.Domain;
using UnityEngine;
using VContainer.Unity;

namespace _Project.Features.Items.Presentation
{
    public sealed class HotbarHudPresenter : IStartable, ITickable, IDisposable
    {
        private const string HandsEmptyText = "Hands: Empty";
        private const string CountPrefix = "×";

        private readonly Inventory _inventory;
        private readonly HotbarHudView _view;
        private readonly IItemIconProvider _itemIconProvider;
        private readonly StringBuilder _textBuffer = new StringBuilder();
        private readonly string[] _countTexts = new string[Inventory.HotbarSize];
        private readonly int[] _cachedCounts = new int[Inventory.HotbarSize];
        private int _lastSelectedSlot;
        private float _selectionToastTimeRemaining;

        public HotbarHudPresenter(
            Inventory inventory,
            HotbarHudView view,
            IItemIconProvider itemIconProvider)
        {
            _inventory = inventory;
            _view = view;
            _itemIconProvider = itemIconProvider;
        }

        public void Start()
        {
            _view.SetVisible(true);
            _lastSelectedSlot = _inventory.SelectedSlot;
            _inventory.Changed += Render;
            Render();
        }

        private void Render()
        {
            for (int slot = 0; slot < Inventory.HotbarSize; slot++)
            {
                InventoryStack stack = _inventory.GetSlot(slot);
                bool selected = slot == _inventory.SelectedSlot;
                Sprite icon = stack == null ? null : _itemIconProvider.GetIcon(stack.Item.Id);
                _view.SetSlot(
                    slot,
                    selected ? _view.SelectedSlotColor : _view.SlotColor,
                    selected,
                    icon,
                    stack == null ? string.Empty : GetCountText(slot, stack.Count));
            }

            ItemDefinition selectedItem = _inventory.SelectedItem;
            _view.SetHeldItem(FormatHeldItem(selectedItem));

            if (_lastSelectedSlot == _inventory.SelectedSlot)
                return;

            _lastSelectedSlot = _inventory.SelectedSlot;
            ShowSelectionToast(selectedItem);
        }

        private string GetCountText(int slot, int count)
        {
            if (_countTexts[slot] != null && _cachedCounts[slot] == count)
                return _countTexts[slot];

            _cachedCounts[slot] = count;
            _countTexts[slot] = string.Concat(CountPrefix, count.ToString());
            return _countTexts[slot];
        }

        private void ShowSelectionToast(ItemDefinition item)
        {
            if (item == null)
            {
                _selectionToastTimeRemaining = 0f;
                _view.ShowSelectionToast(string.Empty);
                return;
            }

            _view.ShowSelectionToast(item.DisplayName);
            _selectionToastTimeRemaining = _view.SelectionToastDuration;
        }

        private string FormatHeldItem(ItemDefinition item)
        {
            if (item == null)
                return HandsEmptyText;

            _textBuffer.Clear();
            _textBuffer.Append(item.DisplayName);
            IReadOnlyList<ItemStat> stats = item.GetInspectableStats();
            for (int i = 0; i < stats.Count; i++)
                _textBuffer.Append("\n").Append(stats[i].Label).Append(": ").Append(stats[i].Value);
            return _textBuffer.ToString();
        }

        public void Tick()
        {
            if (_selectionToastTimeRemaining <= 0f)
                return;

            _selectionToastTimeRemaining = Mathf.Max(
                0f,
                _selectionToastTimeRemaining - Time.unscaledDeltaTime);
            float fadeDuration = _view.SelectionToastFadeDuration;
            float alpha = fadeDuration <= 0f
                ? 0f
                : _selectionToastTimeRemaining <= fadeDuration
                    ? _selectionToastTimeRemaining / fadeDuration
                    : 1f;
            _view.SetSelectionToastAlpha(alpha);
        }

        public void Dispose()
        {
            _inventory.Changed -= Render;
        }
    }
}