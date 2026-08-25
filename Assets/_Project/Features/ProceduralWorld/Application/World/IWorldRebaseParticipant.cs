using UnityEngine;

namespace _Project.Features.ProceduralWorld.Application.World
{
    /// <summary>
    /// Receives notifications when the local Unity world origin is rebased.
    /// </summary>
    /// <remarks>
    /// Participants use the supplied world-space delta to update any state that depends
    /// on Unity object positions or the local world origin.
    /// </remarks>
    public interface IWorldRebaseParticipant
    {
        /// <summary>
        /// Determines the notification order relative to other rebase participants.
        /// Lower values are notified first.
        /// </summary>
        int Order { get; }

        /// <summary>
        /// Notifies the participant that loaded world objects have been shifted by the specified delta.
        /// </summary>
        /// <param name="delta">World-space translation applied during the rebase.</param>
        void OnWorldRebased(Vector3 delta);
    }
}