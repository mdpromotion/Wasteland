namespace _Project.Features.Core.Application
{
    public interface IGameSessionSaveController
    {
        void ArmAutoSave();
        void SaveOnExit();
    }

    public sealed class GameSessionSaveController : IGameSessionSaveController
    {
        private readonly IGameSaveService _saveService;
        private readonly WorldAutoSaveSystem _autoSaveSystem;

        public GameSessionSaveController(
            IGameSaveService saveService,
            WorldAutoSaveSystem autoSaveSystem)
        {
            _saveService = saveService;
            _autoSaveSystem = autoSaveSystem;
        }

        public void ArmAutoSave() => _autoSaveSystem.Arm();

        public void SaveOnExit() => _saveService.SaveAll();
    }
}