using _Project.Features.Items.Application;
using _Project.Features.Persistence.Application;
using _Project.Features.Persistence.Domain;
using _Project.Features.Persistence.Infrastructure;
using NUnit.Framework;

namespace _Project.Tests.Core.Persistence
{
    public sealed class PlayerInventorySaveMapperTests
    {
        private const string TestWorld = "PlayerInventorySaveMapperTests";
        private const string TestPlayerId = "PlayerInventorySaveMapperTests_Player";

        [SetUp]
        public void SetUp()
        {
            Cleanup();
        }

        [TearDown]
        public void TearDown()
        {
            Cleanup();
        }

        [Test]
        public void ApplyToSaveData_FillsInventoryFieldsAndBumpsSchema()
        {
            var data = new PlayerSaveData();
            var snapshot = new InventorySnapshot(3, new[]
            {
                new InventorySlotSnapshot(0, "iron_axe", 1),
                new InventorySlotSnapshot(1, "wood", 12)
            });

            PlayerInventorySaveMapper.ApplyToSaveData(snapshot, data);

            Assert.That(data.SchemaVersion, Is.EqualTo(PlayerInventorySaveMapper.InventorySchemaVersion));
            Assert.That(data.SelectedSlot, Is.EqualTo(3));
            Assert.That(data.Inventory, Has.Length.EqualTo(2));
            Assert.That(data.Inventory[0].Slot, Is.EqualTo(0));
            Assert.That(data.Inventory[0].ItemId, Is.EqualTo("iron_axe"));
            Assert.That(data.Inventory[0].Count, Is.EqualTo(1));
            Assert.That(data.Inventory[1].Count, Is.EqualTo(12));
        }

        [Test]
        public void ApplyToSaveData_EmptySnapshot_ClearsInventoryBlocks()
        {
            var data = new PlayerSaveData
            {
                Inventory = new[] { new InventoryStackSaveData { Slot = 0, ItemId = "wood", Count = 1 } },
                SelectedSlot = 4
            };

            PlayerInventorySaveMapper.ApplyToSaveData(InventorySnapshot.Empty, data);

            Assert.That(data.Inventory, Is.Null);
            Assert.That(data.SelectedSlot, Is.EqualTo(0));
        }

        [Test]
        public void ToSnapshot_PreInventorySave_ReturnsNull()
        {
            // PlayerSaveData as deserialized from a schema-1 file: no Inventory block.
            var oldSave = new PlayerSaveData { PlayerId = "player", SchemaVersion = 1 };

            Assert.That(PlayerInventorySaveMapper.ToSnapshot(oldSave), Is.Null);
            Assert.That(PlayerInventorySaveMapper.ToSnapshot(null), Is.Null);
        }

        [Test]
        public void ToSnapshot_MapsAllEntries()
        {
            var save = new PlayerSaveData
            {
                SelectedSlot = 2,
                Inventory = new[]
                {
                    new InventoryStackSaveData { Slot = 1, ItemId = "wood", Count = 40 },
                    new InventoryStackSaveData { Slot = 2, ItemId = "apple", Count = 3 }
                }
            };

            InventorySnapshot snapshot = PlayerInventorySaveMapper.ToSnapshot(save);

            Assert.That(snapshot.SelectedSlot, Is.EqualTo(2));
            Assert.That(snapshot.Stacks.Count, Is.EqualTo(2));
            Assert.That(snapshot.Stacks[0].Slot, Is.EqualTo(1));
            Assert.That(snapshot.Stacks[0].ItemId, Is.EqualTo("wood"));
            Assert.That(snapshot.Stacks[1].Count, Is.EqualTo(3));
        }

        [Test]
        public void SaveAndLoad_RoundTripsThroughFileStore()
        {
            var writer = new PlayerFileStore(new JsonFileStore(), new JsonFileStore());
            var snapshot = new InventorySnapshot(1, new[]
            {
                new InventorySlotSnapshot(0, "iron_axe", 1),
                new InventorySlotSnapshot(1, "wood", 50),
                new InventorySlotSnapshot(2, "apple", 3)
            });
            var original = new PlayerSaveData
            {
                PlayerId = TestPlayerId,
                X = 10.5,
                Y = 2f,
                Z = 20.25,
                Yaw = 0.5f,
                Pitch = 0.25f,
                SavedAtTicks = 1234567890
            };
            PlayerInventorySaveMapper.ApplyToSaveData(snapshot, original);

            writer.SavePlayer(TestWorld, TestPlayerId, original);

            bool loaded = writer.TryReadPlayer(TestWorld, TestPlayerId, out PlayerSaveData restored);
            Assert.That(loaded, Is.True);
            Assert.That(restored.X, Is.EqualTo(10.5));

            InventorySnapshot restoredSnapshot = PlayerInventorySaveMapper.ToSnapshot(restored);
            Assert.That(restoredSnapshot, Is.Not.Null);
            Assert.That(restoredSnapshot.SelectedSlot, Is.EqualTo(1));
            Assert.That(restoredSnapshot.Stacks.Count, Is.EqualTo(3));
            Assert.That(restoredSnapshot.Stacks[2].ItemId, Is.EqualTo("apple"));
            Assert.That(restoredSnapshot.Stacks[2].Count, Is.EqualTo(3));
        }

        private static void Cleanup()
        {
            string path = System.IO.Path.Combine(
                UnityEngine.Application.persistentDataPath,
                "Worlds",
                TestWorld);

            if (System.IO.Directory.Exists(path))
                System.IO.Directory.Delete(path, true);
        }
    }
}
