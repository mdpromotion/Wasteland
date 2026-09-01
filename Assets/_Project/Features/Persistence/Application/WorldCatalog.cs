using System;
using _Project.Features.Persistence.Domain;

namespace _Project.Features.Persistence.Application
{
    public interface IWorldCatalog
    {
        string[] GetWorldNames();
        WorldDescriptor GetWorld(string worldName);
        WorldDescriptor CreateWorld(string worldName, int seed);
        void DeleteWorld(string worldName);
        string GetAvailableWorldName();
    }
    
    public sealed class WorldCatalog : IWorldCatalog
    {
        private const string DefaultBaseName = "My-World";

        private readonly IWorldReader _reader;
        private readonly IWorldWriter _writer;

        public WorldCatalog(IWorldReader reader, IWorldWriter writer)
        {
            _reader = reader;
            _writer = writer;
        }
        
        public string[] GetWorldNames() => _reader.GetWorldNames();

        public WorldDescriptor GetWorld(string worldName) => _reader.ReadWorld(worldName);
        
        public WorldDescriptor CreateWorld(string worldName, int seed)
        {
            if (string.IsNullOrWhiteSpace(worldName))
                throw new ArgumentException("World name must not be empty.", nameof(worldName));

            if (_reader.WorldExists(worldName))
                throw new InvalidOperationException($"World '{worldName}' already exists.");

            _writer.CreateWorld(worldName, seed);
            return _reader.ReadWorld(worldName);
        }

        public void DeleteWorld(string worldName)
        {
            if (string.IsNullOrWhiteSpace(worldName))
                throw new ArgumentException("World name must not be empty.", nameof(worldName));

            if (!_reader.WorldExists(worldName))
                throw new InvalidOperationException($"World '{worldName}' does not exist.");

            _writer.DeleteWorld(worldName);
        }
        
        public string GetAvailableWorldName()
        {
            var existing = _reader.GetWorldNames();

            if (!ContainsIgnoreCase(existing, DefaultBaseName))
                return DefaultBaseName;

            var i = 1;
            while (true)
            {
                var candidate = DefaultBaseName + "-" + i;
                if (!ContainsIgnoreCase(existing, candidate))
                    return candidate;

                i++;
            }
        }

        private static bool ContainsIgnoreCase(string[] values, string target)
        {
            for (var i = 0; i < values.Length; i++)
            {
                if (string.Equals(values[i], target, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }
    }
}