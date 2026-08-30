using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace _Project.Features.Persistence.Infrastructure
{
    public interface IJsonWriter
    {
        void Write<T>(string category, T data);
    }

    public interface IJsonReader
    {
        bool TryRead<T>(string category, out T data);
    }

    public class JsonFileStore : IJsonWriter, IJsonReader
    {
        private readonly string _rootPath = UnityEngine.Application.persistentDataPath;

        private static readonly JsonSerializerSettings SerializerSettings =
            new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                Converters =
                {
                    new StringEnumConverter()
                }
            };

        public void Write<T>(string category, T data)
        {
            var path = GetPath(category);
            var tmpPath = path + ".tmp";

            var json = JsonConvert.SerializeObject(
                data,
                SerializerSettings);

            File.WriteAllText(tmpPath, json);

            if (File.Exists(path))
                File.Delete(path);

            File.Move(tmpPath, path);
        }

        public bool TryRead<T>(string category, out T data)
        {
            var path = GetPath(category);

            data = default;

            if (!File.Exists(path))
            {
                return false;
            }

            try
            {
                var json = File.ReadAllText(path);
                
                data = JsonConvert.DeserializeObject<T>(
                    json,
                    SerializerSettings);

                return true;
            }
            catch (Exception e)
            {
                data = default;
                return false;
            }
        }

        private string GetPath(string category) =>
            Path.Combine(_rootPath, category + ".json");
    }
}