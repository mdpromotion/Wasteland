using _Project.Features.Persistence.Domain;

namespace _Project.Features.Persistence.Application
{
    public interface IPlayerSaveWriter
    {
        void SavePlayer(string worldName, string playerId, PlayerSaveData data);
    }

    public interface IPlayerSaveReader
    {
        bool TryReadPlayer(string worldName, string playerId, out PlayerSaveData data);
    }
}