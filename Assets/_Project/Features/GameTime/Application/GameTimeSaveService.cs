using _Project.Features.GameTime.Domain;
using _Project.Features.Persistence.Application;
using _Project.Features.ProceduralWorld.Domain.World;

namespace _Project.Features.GameTime.Application
{
    public interface IGameTimeSaveService
    {
        void Save();
        void Load();
    }

    public sealed class GameTimeSaveService : IGameTimeSaveService
    {
        private readonly Domain.GameTime _gameTime;
        private readonly IWorldReader _worldReader;
        private readonly IWorldWriter _worldWriter;
        private readonly IWorldSettings _worldSettings;

        public GameTimeSaveService(
            Domain.GameTime gameTime,
            IWorldReader worldReader,
            IWorldWriter worldWriter,
            IWorldSettings worldSettings)
        {
            _gameTime = gameTime;
            _worldReader = worldReader;
            _worldWriter = worldWriter;
            _worldSettings = worldSettings;
        }

        public void Save()
        {
            _worldWriter.SaveCurrentTick(
                _worldSettings.Name,
                _gameTime.CurrentTime);
        }

        public void Load()
        {
            var world = _worldReader.ReadWorld(_worldSettings.Name);

            float currentTick = world.CurrentTick ?? _gameTime.GetDefaultTime();

            _gameTime.SetTime(currentTick);
        }
    }
}