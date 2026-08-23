using _Project.Features.ProceduralWorld.Domain.World;
using NUnit.Framework;
using UnityEngine;

namespace _Project.Tests.ProceduralWorld.World
{
    public sealed class WorldSettingsTests
    {
        [Test]
        public void Constructor_CopiesValuesFromConfig()
        {
            WorldSettingsConfig config =
                ScriptableObject.CreateInstance<WorldSettingsConfig>();

            config.Octaves = 6;
            config.Scale = 42.5f;
            config.Persistence = 0.65f;
            config.Lacunarity = 2.1f;
            config.RedistributionPower = 1.7f;

            var settings = new WorldSettings(config);

            Assert.That(settings.Octaves, Is.EqualTo(6));
            Assert.That(settings.Scale, Is.EqualTo(42.5f));
            Assert.That(settings.Persistence, Is.EqualTo(0.65f));
            Assert.That(settings.Lacunarity, Is.EqualTo(2.1f));
            Assert.That(
                settings.RedistributionPower,
                Is.EqualTo(1.7f));
        }

        [Test]
        public void SetSeed_ChangesSeed()
        {
            WorldSettingsConfig config =
                ScriptableObject.CreateInstance<WorldSettingsConfig>();

            var settings = new WorldSettings(config);

            settings.SetSeed(12345);

            Assert.That(settings.Seed, Is.EqualTo(12345));
        }
    }
}