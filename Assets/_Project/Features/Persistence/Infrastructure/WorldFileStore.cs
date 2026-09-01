using System;
using System.Collections.Generic;
using System.IO;
using _Project.Features.Persistence.Application;
using _Project.Features.Persistence.Domain;

namespace _Project.Features.Persistence.Infrastructure
{
    public sealed class WorldFileStore : IWorldReader, IWorldWriter
    {
        private const string WorldFileName = "world";

        private readonly string _worldsRoot =
            Path.Combine(UnityEngine.Application.persistentDataPath, "Worlds");

        private readonly IJsonReader _jsonReader;
        private readonly IJsonWriter _jsonWriter;

        public WorldFileStore(IJsonReader jsonReader, IJsonWriter jsonWriter)
        {
            _jsonReader = jsonReader;
            _jsonWriter = jsonWriter;
        }

        public void CreateWorld(string worldName, int seed)
        {
            var worldDir = GetWorldDir(worldName);

            if (Directory.Exists(worldDir))
                throw new InvalidOperationException($"World folder '{worldName}' already exists.");

            var meta = new WorldMetadataDto
            {
                Name = worldName,
                Seed = seed,
                CreatedAtTicks = DateTime.UtcNow.Ticks
            };

            _jsonWriter.Write(GetCategory(worldName), meta);
        }

        public bool WorldExists(string worldName) =>
            File.Exists(GetWorldJsonPath(worldName));

        public string[] GetWorldNames()
        {
            if (!Directory.Exists(_worldsRoot))
                return Array.Empty<string>();

            var dirs = Directory.GetDirectories(_worldsRoot);
            var names = new List<string>(dirs.Length);

            for (var i = 0; i < dirs.Length; i++)
            {
                var name = Path.GetFileName(dirs[i]);
                if (File.Exists(Path.Combine(dirs[i], WorldFileName + ".json")))
                    names.Add(name);
            }

            return names.ToArray();
        }

        public WorldDescriptor ReadWorld(string worldName)
        {
            if (!_jsonReader.TryRead<WorldMetadataDto>(GetCategory(worldName), out var dto))
                throw new FileNotFoundException($"World '{worldName}' has no {WorldFileName}.json.");

            return new WorldDescriptor(dto.Name, dto.Seed, dto.CreatedAtTicks);
        }
        
        public void DeleteWorld(string worldName)
        {
            var worldDir = GetWorldDir(worldName);

            if (!Directory.Exists(worldDir))
                throw new InvalidOperationException($"World folder '{worldName}' does not exist.");

            Directory.Delete(worldDir, recursive: true);
        }

        private string GetCategory(string worldName) =>
            Path.Combine("Worlds", worldName, WorldFileName);

        private string GetWorldDir(string worldName) =>
            Path.Combine(_worldsRoot, worldName);

        private string GetWorldJsonPath(string worldName) =>
            Path.Combine(GetWorldDir(worldName), WorldFileName + ".json");

        private sealed class WorldMetadataDto
        {
            public string Name { get; set; }
            public int Seed { get; set; }
            public long CreatedAtTicks { get; set; }
        }
    }
}