using System;
using System.Collections.Generic;
using _Project.Features.Items.Application;
using _Project.Features.Items.Infrastructure;
using UnityEngine;
using VContainer.Unity;

namespace _Project.Features.Items.Presentation
{
    /// <summary>
    /// Caps how many pickup prompts can be shown at once. Each frame it selects the
    /// nearest pickups from <see cref="IPickupPromptProvider"/> and toggles their
    /// prompts; omitted pickups turn their billboards off.
    /// </summary>
    public sealed class PickupPromptPresenter : IInitializable, ITickable, IDisposable
    {
        private readonly IPickupPromptProvider _provider;
        private readonly ItemDropSettings _settings;
        private readonly List<IItemPickupTarget> _visible = new List<IItemPickupTarget>();

        public PickupPromptPresenter(IPickupPromptProvider provider, ItemDropSettings settings)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public void Initialize()
        {
        }

        public void Tick()
        {
            PickupPromptSelection.SelectNearest(_provider.NearbyPickups, _settings.MaxVisibleBillboards, _visible);

            IReadOnlyCollection<IItemPickupTarget> nearby = _provider.NearbyPickups;
            foreach (IItemPickupTarget pickup in nearby)
            {
                if (pickup == null)
                    continue;

                bool show = _visible.Contains(pickup);
                pickup.SetPromptVisible(show);
            }
        }

        public void Dispose()
        {
            _visible.Clear();
        }
    }
}
