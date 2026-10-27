using UnityEngine;

namespace _Project.Features.Player.Domain
{
    public interface IPlayerReadOnly
    {
        bool IsAvailable { get; }
        Vector3 Position { get; }
        Vector3 Velocity { get; }
        Vector3 Forward { get; }
    }
}
