using _Project.Features.Items.Domain;
using _Project.Features.ProceduralWorld.Domain.Vegetation;
using NUnit.Framework;

namespace _Project.Tests.ProceduralWorld.Vegetation
{
    public class TreeImpactParticlesPolicyTests
    {
        [Test]
        public void GetParticleCount_FistDamage_MatchesPreviousFixedCount()
        {
            Assert.That(
                TreeImpactParticlesPolicy.GetParticleCount(ItemDefinition.FistDamage),
                Is.EqualTo(8));
        }

        [TestCase(0.5f, 4)]
        [TestCase(2f, 16)]
        [TestCase(3f, 24)]
        public void GetParticleCount_ScalesLinearlyWithDamage(float damage, int expected)
        {
            Assert.That(
                TreeImpactParticlesPolicy.GetParticleCount(damage),
                Is.EqualTo(expected));
        }

        [Test]
        public void GetParticleCount_ZeroDamage_FallsBackToMin()
        {
            Assert.That(
                TreeImpactParticlesPolicy.GetParticleCount(0f),
                Is.EqualTo(TreeImpactParticlesPolicy.MinParticles));
        }

        [Test]
        public void GetParticleCount_NegativeDamage_FallsBackToMin()
        {
            Assert.That(
                TreeImpactParticlesPolicy.GetParticleCount(-1f),
                Is.EqualTo(TreeImpactParticlesPolicy.MinParticles));
        }
    }
}
