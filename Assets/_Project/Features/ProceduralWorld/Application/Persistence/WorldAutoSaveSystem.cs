using System;
using UnityEngine;
using VContainer.Unity;

namespace _Project.Features.ProceduralWorld.Application.Persistence
{
    public sealed class WorldAutoSaveSystem : ITickable, IDisposable
    {
        private readonly IWorldSaveService _saveService;
        private readonly IDirtyChunkRegistry _dirtyRegistry;
        private readonly float _intervalSeconds;
        private float _timer;

        public WorldAutoSaveSystem(
            IWorldSaveService saveService,
            IDirtyChunkRegistry dirtyRegistry)
        {
            _saveService = saveService;
            _dirtyRegistry = dirtyRegistry;
            _intervalSeconds = 60;
        }

        public void Tick()
        {
            if (!_dirtyRegistry.HasDirtyChunks)
                return;

            _timer += Time.deltaTime;
            if (_timer < _intervalSeconds)
                return;

            _timer = 0f;
            _saveService.SaveAllDirty();
        }

        public void Dispose()
        {
            if (_dirtyRegistry.HasDirtyChunks)
                _saveService.SaveAllDirty();
        }
    }
}