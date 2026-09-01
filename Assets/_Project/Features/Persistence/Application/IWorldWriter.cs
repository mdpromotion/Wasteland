using _Project.Features.Persistence.Domain;

namespace _Project.Features.Persistence.Application
{
    public interface IWorldWriter
    {
        void CreateWorld(string worldName, int seed);
        void DeleteWorld(string worldName);
    }

    public interface IWorldReader
    {
        bool WorldExists(string worldName);
        
        string[] GetWorldNames();

        WorldDescriptor ReadWorld(string worldName);
    }
}