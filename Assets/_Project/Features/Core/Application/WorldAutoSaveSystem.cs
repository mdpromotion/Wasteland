using UnityEngine;
using VContainer.Unity;
using _Project.Features.Persistence.Application;
using _Project.Features.ProceduralWorld.Application.Persistence;

namespace _Project.Features.Core.Application
{
    public sealed class WorldAutoSaveSystem : ITickable
    {
        private readonly IGameSaveService _saveService;

        private readonly float _intervalSeconds;
        private float _timer;

        private bool _armed;

        public WorldAutoSaveSystem(IGameSaveService saveService)
        {
            _saveService  = saveService;

            _intervalSeconds = 60f;
        }

        public void Arm() => _armed = true;

        public void Tick()
        {
            if (!_armed)
                return;

            _timer += Time.deltaTime;
            if (_timer < _intervalSeconds)
                return;

            _timer = 0f;

            _saveService.SaveAll();
        }
    }
}