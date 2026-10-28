using System.Collections.Generic;
using System.Linq;
using _Project.Features.Items.Application;
using _Project.Features.Items.Presentation;
using NUnit.Framework;

namespace _Project.Tests.Items
{
    public sealed class PickupPromptSelectionTests
    {
        [Test]
        public void SelectNearest_EmptyCollection_ClearsResult()
        {
            var visible = new List<IItemPickupTarget> { new FakePickup(1f, "a") };

            PickupPromptSelection.SelectNearest(
                new List<IItemPickupTarget>(),
                3,
                visible);

            Assert.That(visible, Is.Empty);
        }

        [Test]
        public void SelectNearest_FewerThanLimit_ShowsAllSortedByDistance()
        {
            var candidates = new List<IItemPickupTarget>
            {
                new FakePickup(7f, "far"),
                new FakePickup(2f, "near"),
                new FakePickup(4f, "mid")
            };
            var visible = new List<IItemPickupTarget>();

            PickupPromptSelection.SelectNearest(candidates, 6, visible);

            CollectionAssert.AreEqual(
                new[] { "near", "mid", "far" },
                visible.Select(p => ((FakePickup)p).Name));
        }

        [Test]
        public void SelectNearest_OverLimit_KeepsOnlyNearest()
        {
            var candidates = new List<IItemPickupTarget>
            {
                new FakePickup(9f, "a9"),
                new FakePickup(1f, "a1"),
                new FakePickup(5f, "a5"),
                new FakePickup(3f, "a3"),
                new FakePickup(7f, "a7")
            };
            var visible = new List<IItemPickupTarget>();

            PickupPromptSelection.SelectNearest(candidates, 2, visible);

            CollectionAssert.AreEqual(
                new[] { "a1", "a3" },
                visible.Select(p => ((FakePickup)p).Name));
        }

        [Test]
        public void SelectNearest_SkipsUnavailableTargets()
        {
            var candidates = new List<IItemPickupTarget>
            {
                null,
                new FakePickup(float.NaN, "nan"),
                new FakePickup(float.PositiveInfinity, "infinite"),
                new FakePickup(-1f, "negative"),
                new FakePickup(2f, "valid")
            };
            var visible = new List<IItemPickupTarget>();

            PickupPromptSelection.SelectNearest(candidates, 6, visible);

            CollectionAssert.AreEqual(new[] { "valid" }, visible.Select(p => ((FakePickup)p).Name));
        }

        [Test]
        public void SelectNearest_InsertingCloserPickup_EvictsFarthest()
        {
            var visible = new List<IItemPickupTarget>();
            var candidates = new List<IItemPickupTarget>
            {
                new FakePickup(10f, "first"),
                new FakePickup(8f, "second"),
                new FakePickup(0.5f, "late-arriving")
            };

            PickupPromptSelection.SelectNearest(candidates, 2, visible);

            CollectionAssert.AreEqual(
                new[] { "late-arriving", "second" },
                visible.Select(p => ((FakePickup)p).Name));
        }

        [Test]
        public void SelectNearest_ZeroLimit_KeepsNothing()
        {
            var candidates = new List<IItemPickupTarget> { new FakePickup(1f, "a") };
            var visible = new List<IItemPickupTarget>();

            PickupPromptSelection.SelectNearest(candidates, 0, visible);

            Assert.That(visible, Is.Empty);
        }

        [Test]
        public void SelectNearest_ReusesProvidedList()
        {
            var visible = new List<IItemPickupTarget>();

            PickupPromptSelection.SelectNearest(new List<IItemPickupTarget> { new FakePickup(1f, "a") }, 3, visible);
            List<IItemPickupTarget> firstInstance = visible;

            PickupPromptSelection.SelectNearest(new List<IItemPickupTarget> { new FakePickup(2f, "b") }, 3, visible);

            Assert.That(ReferenceEquals(firstInstance, visible), Is.True);
            CollectionAssert.AreEqual(
                new[] { "b" },
                visible.Select(p => ((FakePickup)p).Name));
        }

        private sealed class FakePickup : IItemPickupTarget
        {
            public string Name { get; }
            private readonly float _distance;

            public FakePickup(float distance, string name)
            {
                _distance = distance;
                Name = name;
            }

            public float Distance => _distance;

            public bool TryCollect(IItemCollector collector)
            {
                return false;
            }

            public void SetPromptVisible(bool visible)
            {
            }
        }
    }
}
