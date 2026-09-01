using System;
using UnityEngine;

namespace _Project.Features.Player.Application
{
    public interface IPlayerController
    {
        void SetLookYaw(float yawDelta);
        void Freeze(bool state);
        
        Vector3 LastKnownPosition { get; }
        float LookYaw { get; }
        void SyncYaw(float yaw);

        bool Prepare();
        void Ready();

        event Action OnJumped;
        event Action OnLanded;
    }
}