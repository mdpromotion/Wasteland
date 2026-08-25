using System;

namespace _Project.Features.Player.Application
{
    public interface IPlayerController
    {
        void SetLookYaw(float yawDelta);
        void Freeze(bool state);
        
        float LookYaw { get; }

        bool Prepare();
        void Ready();

        event Action OnJumped;
        event Action OnLanded;
    }
}