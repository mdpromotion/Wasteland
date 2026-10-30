using System;
using System.Collections.Generic;
using _Project.Features.Items.Domain;
using _Project.Features.Items.Infrastructure;
using UnityEngine;
using Object = UnityEngine.Object;

namespace _Project.Features.Items.Presentation
{
    /// <summary>
    /// Provides ready-to-display GameObjects for items equipped by the player.
    /// Implementations own the lifecycle of the created visuals; callers never
    /// destroy or reparent the instances returned by <see cref="Acquire"/>.
    /// </summary>
    public interface IHeldItemVisualFactory : IDisposable
    {
        /// <summary>Returns an active visual instance for the given definition.</summary>
        GameObject Acquire(ItemDefinitionAsset definition, HeldItemConfig config);

        /// <summary>Marks the instance for the given item as unused and hides it.</summary>
        void Release(ItemId itemId);
    }
}
