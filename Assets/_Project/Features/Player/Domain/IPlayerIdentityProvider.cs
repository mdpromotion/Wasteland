using System;

namespace _Project.Features.Player.Domain
{
    public interface IPlayerIdentityProvider
    {
        string GetPlayerId();
    }
    
    // TODO: Nicknames system. Now it's temporary stub.
    public sealed class LocalMachinePlayerIdentityProvider : IPlayerIdentityProvider
    {
        public string GetPlayerId() => Environment.UserName;
    }
}