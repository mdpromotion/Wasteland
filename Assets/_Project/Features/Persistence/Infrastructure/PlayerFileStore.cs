using System.IO;
using _Project.Features.Persistence.Application;
using _Project.Features.Persistence.Domain;

namespace _Project.Features.Persistence.Infrastructure
{
    public sealed class PlayerFileStore : IPlayerSaveReader, IPlayerSaveWriter
    {
        private readonly IJsonReader _jsonReader;
        private readonly IJsonWriter _jsonWriter;

        public PlayerFileStore(IJsonReader jsonReader, IJsonWriter jsonWriter)
        {
            _jsonReader = jsonReader;
            _jsonWriter = jsonWriter;
        }

        public void SavePlayer(string worldName, string playerId, PlayerSaveData data) =>
            _jsonWriter.Write(GetCategory(worldName, playerId), data);

        public bool TryReadPlayer(string worldName, string playerId, out PlayerSaveData data) =>
            _jsonReader.TryRead(GetCategory(worldName, playerId), out data);

        private static string GetCategory(string worldName, string playerId) =>
            Path.Combine("Worlds", worldName, "Players", SanitizeFileName(playerId));

        private static string SanitizeFileName(string name)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');
            return name;
        }
    }
}