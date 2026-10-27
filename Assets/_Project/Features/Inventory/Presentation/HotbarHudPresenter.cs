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
        private readonly Inventory _inventory;
        private readonly HotbarHudView _view;
        private readonly IItemIconProvider _itemIconProvider;
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
                string count = stack == null ? string.Empty : $"×{stack.Count}";
                _view.SetSlot(
                    slot,
                    selected ? _view.SelectedSlotColor : _view.SlotColor,
                    selected,
                    icon,
                    count);
            }

            ItemDefinition selectedItem = _inventory.SelectedItem;
            _view.SetHeldItem(FormatHeldItem(selectedItem));

            if (_lastSelectedSlot == _inventory.SelectedSlot)
                return;

            _lastSelectedSlot = _inventory.SelectedSlot;
            ShowSelectionToast(selectedItem);
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

        private static string FormatHeldItem(ItemDefinition item)
        {
            if (item == null)
                return "Hands: Empty";

            var text = new StringBuilder(item.DisplayName);
            IReadOnlyList<ItemStat> stats = item.GetInspectableStats();
            for (int i = 0; i < stats.Count; i++)
                text.Append("\n").Append(stats[i].Label).Append(": ").Append(stats[i].Value);
            return text.ToString();
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