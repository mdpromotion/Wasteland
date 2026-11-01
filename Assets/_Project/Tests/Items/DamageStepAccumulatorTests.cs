using _Project.Features.Items.Application;
using NUnit.Framework;

namespace _Project.Tests.Items
{
    public class DamageStepAccumulatorTests
    {
        [Test]
        public void Add_StepTwoAndDamageOne_ReturnsStepEverySecondHit()
        {
            var accumulator = new DamageStepAccumulator();
            Assert.That(accumulator.Add(1f, 2f), Is.Zero);
            Assert.That(accumulator.Add(1f, 2f), Is.EqualTo(1));
            Assert.That(accumulator.Add(1f, 2f), Is.Zero);
            Assert.That(accumulator.Add(1f, 2f), Is.EqualTo(1));
        }

        [Test]
        public void Add_PreservesRemainderAndReturnsMultipleSteps()
        {
            var accumulator = new DamageStepAccumulator();
            Assert.That(accumulator.Add(5.5f, 2f), Is.EqualTo(2));
            Assert.That(accumulator.Add(0.5f, 2f), Is.EqualTo(1));
        }

        [Test]
        public void Add_NonPositiveDamageDoesNotEarnStep()
        {
            var accumulator = new DamageStepAccumulator();
            Assert.That(accumulator.Add(0f, 1f), Is.Zero);
            Assert.That(accumulator.Add(-1f, 1f), Is.Zero);
        }
    }
}
