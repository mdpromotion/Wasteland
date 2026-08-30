using _Project.Features.Graphics.Domain;
using _Project.Features.Persistence.Infrastructure;

namespace _Project.Features.Graphics.Infrastucture
{
    public interface IGraphicsSettingsRepository
    {
        GraphicsData Load();
        void Save(GraphicsData data);
    }

    public class GraphicsSettingsRepository : IGraphicsSettingsRepository
    {
        private const string Category = "graphics";
        private const GraphicsType FallbackPreset = GraphicsType.Medium;

        private readonly IJsonReader _reader;
        private readonly IJsonWriter _writer;
        private readonly IGraphicsConfigResolver _resolver;

        private GraphicsData? _cache;

        public GraphicsSettingsRepository(
            IJsonReader reader,
            IJsonWriter writer,
            IGraphicsConfigResolver resolver)
        {
            _reader = reader;
            _writer = writer;
            _resolver = resolver;
        }

        public GraphicsData Load()
        {
            if (_cache.HasValue)
                return _cache.Value;

            if (_reader.TryRead<GraphicsData>(Category, out var data))
            {
                _cache = data;
                return data;
            }

            var fallback = _resolver.GetDefaultGraphicsData(FallbackPreset) ?? default;
            _cache = fallback;
            return fallback;
        }

        public void Save(GraphicsData data)
        {
            _cache = data;
            _writer.Write(Category, data);
        }
    }
}