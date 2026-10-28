using System;

namespace _Project.Features.Items.Domain
{
    /// <summary>
    /// Player-facing quick access bar. It does not own stacks; it selects a slot index
    /// within a container, so future storage can hold more slots than the hotbar shows.
    /// </summary>
    public sealed class Hotbar
    {
        public const int Size = 9;

        public event Action Changed;

        private int _selectedSlot;

        public int SelectedSlot => _selectedSlot;

        public bool SelectSlot(int slot)
        {
            ValidateSlot(slot);
            if (_selectedSlot == slot)
                return false;

            _selectedSlot = slot;
            Changed?.Invoke();
            return true;
        }

        private static void ValidateSlot(int slot)
        {
            if (slot < 0 || slot >= Hotbar.Size)
                throw new ArgumentOutOfRangeException(nameof(slot));
        }
    }
}
