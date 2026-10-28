using UnityEngine;

namespace _Project.Features.Player.Domain
{
    /// <summary>
    /// Read only view over the physical player state.
    /// <para>
    /// Availability: during scene teardown or before the player rig is created, a
    /// subscriber such as the world streamer may hold a destroyed (Unity fake-null)
    /// reference. <see cref="IsAvailable"/> collapses that into one safe predicate;
    /// the value getters must not throw, so they return neutral defaults
    /// (zeroed positions, identity forward) while unavailable.
    /// </para>
    /// </summary>
    public interface IPlayerReadOnly
    {
        /// <summary>
        /// True while the backing rig exists. Callers should gate streaming and
        /// physics-following behavior on this instead of dereferencing first.
        /// </summary>
        bool IsAvailable { get; }
        Vector3 Position { get; }
        Vector3 Velocity { get; }
        Vector3 Forward { get; }
    }
}
