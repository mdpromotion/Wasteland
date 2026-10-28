using System.Collections.Generic;
using _Project.Features.ProceduralWorld.Domain.Persistence;
using NUnit.Framework;

namespace _Project.Tests.Core.Persistence
{
    public class ChunkDeltaMergeTests
    {
        private static WorldItemDelta AddedItem(ulong id, string itemKey = "wood", float x = 1f, float y = 2f, float z = 3f) =>
            new WorldItemDelta(id, DeltaAction.Added, itemKey, x, y, z);

        private static WorldItemDelta RemovedItem(ulong id) =>
            new WorldItemDelta(id, DeltaAction.Removed, null, 0f, 0f, 0f);

        [Test]
        public void Merge_ItemsWithSameId_NewerWins()
        {
            var older = DeltaWithItems(AddedItem(1UL, x: 10f));
            var newer = DeltaWithItems(RemovedItem(1UL));

            ChunkDelta merged = older.Merge(newer);

            Assert.AreEqual(1, merged.ItemDeltas.Count);
            Assert.AreEqual(DeltaAction.Removed, merged.ItemDeltas[0].Action);
        }

        [Test]
        public void Merge_ItemDeltasKeepAllDistinctIds()
        {
            var older = DeltaWithItems(AddedItem(1UL));
            var newer = DeltaWithItems(AddedItem(2UL, itemKey: "apple", x: 5f));

            ChunkDelta merged = older.Merge(newer);

            Assert.AreEqual(2, merged.ItemDeltas.Count);
        }

        [Test]
        public void Merge_VegetationAndItemDeltas_DoNotOverwriteEachOther()
        {
            var vegetation = new List<VegetationInstanceDelta>
            {
                new VegetationInstanceDelta(1UL, DeltaAction.Removed, null)
            };
            var items = new List<WorldItemDelta> { AddedItem(1UL) };

            var merged = new ChunkDelta(new GeneratorVersionStamp(1), vegetation)
                .Merge(DeltaWithItems(items));

            Assert.AreEqual(1, merged.VegetationDeltas.Count);
            Assert.AreEqual(1, merged.ItemDeltas.Count);
        }

        [Test]
        public void Merge_WithEmptyDelta_PreservesOwnSide()
        {
            var withItems = DeltaWithItems(new List<WorldItemDelta> { AddedItem(1UL) });

            Assert.AreSame(withItems, withItems.Merge(ChunkDelta.Empty));
            Assert.AreSame(withItems, ChunkDelta.Empty.Merge(withItems));
        }

        [Test]
        public void Merge_TakesVersionsFromNewerDelta()
        {
            var older = DeltaWithItems(AddedItem(1UL), version: 1);
            var newer = DeltaWithItems(AddedItem(2UL), version: 7);

            ChunkDelta merged = older.Merge(newer);

            Assert.AreEqual(7, merged.Versions.VegetationVersion);
        }

        [Test]
        public void IsEmpty_TrueOnlyWhenBothListsEmpty()
        {
            Assert.IsTrue(ChunkDelta.Empty.IsEmpty);
            Assert.IsFalse(DeltaWithItems(new List<WorldItemDelta> { AddedItem(1UL) }).IsEmpty);
        }

        private static ChunkDelta DeltaWithItems(IReadOnlyList<WorldItemDelta> items, int version = 1) =>
            new ChunkDelta(
                new GeneratorVersionStamp(version),
                new List<VegetationInstanceDelta>(),
                items);

        private static ChunkDelta DeltaWithItems(WorldItemDelta item, int version = 1) =>
            DeltaWithItems(new[] { item }, version);
    }
}
