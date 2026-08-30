using System;
using System.IO;
using _Project.Features.Persistence.Application;
using _Project.Features.Persistence.Domain;
using NUnit.Framework;

namespace _Project.Tests.Core.Persistence
{
    public class PalRegionFileStoreTests
    {
        private PalRegionFileStore _store;
        private string _regionsDir;

        [SetUp]
        public void SetUp()
        {
            _store = new PalRegionFileStore();
            _regionsDir = Path.Combine(UnityEngine.Application.persistentDataPath, "Regions");
            
            if (Directory.Exists(_regionsDir))
                Directory.Delete(_regionsDir, recursive: true);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_regionsDir))
                Directory.Delete(_regionsDir, recursive: true);
        }

        [Test]
        public void ReadSlot_OnNonExistentRegion_ReturnsMissing_AndDoesNotCreateFile()
        {
            var result = _store.ReadSlot(0, 0, 0);

            Assert.AreEqual(PalSlotState.Missing, result.State);
            Assert.IsNull(result.Payload);
            Assert.IsFalse(Directory.Exists(_regionsDir));
        }

        [Test]
        public void WriteThenRead_ReturnsSamePayload()
        {
            var payload = new byte[] { 1, 2, 3, 4, 5 };

            _store.WriteSlot(0, 0, 42, payload);
            var result = _store.ReadSlot(0, 0, 42);

            Assert.AreEqual(PalSlotState.Present, result.State);
            Assert.AreEqual(payload, result.Payload);
        }

        [Test]
        public void ReadSlot_UnwrittenSlotInExistingRegion_ReturnsMissing()
        {
            _store.WriteSlot(0, 0, 5, new byte[] { 9 });

            var result = _store.ReadSlot(0, 0, 6);

            Assert.AreEqual(PalSlotState.Missing, result.State);
        }

        [Test]
        public void WriteEmptyPayload_IsPresentAndDistinctFromMissing()
        {
            _store.WriteSlot(0, 0, 10, Array.Empty<byte>());

            var result = _store.ReadSlot(0, 0, 10);

            Assert.AreEqual(PalSlotState.Present, result.State);
            Assert.IsNotNull(result.Payload);
            Assert.AreEqual(0, result.Payload.Length);
        }

        [Test]
        public void DeleteSlot_ReturnsTombstoned_NotMissing()
        {
            _store.WriteSlot(0, 0, 3, new byte[] { 7, 7 });
            _store.DeleteSlot(0, 0, 3);

            var result = _store.ReadSlot(0, 0, 3);

            Assert.AreEqual(PalSlotState.Tombstoned, result.State);
            Assert.IsNull(result.Payload);
        }

        [Test]
        public void DeleteSlot_OnNonExistentRegion_IsNoOp()
        {
            Assert.DoesNotThrow(() => _store.DeleteSlot(0, 0, 0));
            Assert.IsFalse(Directory.Exists(_regionsDir));
        }

        [Test]
        public void WriteTwice_ToSameSlot_LatestWriteWins()
        {
            _store.WriteSlot(0, 0, 8, new byte[] { 1 });
            _store.WriteSlot(0, 0, 8, new byte[] { 2, 2, 2 });

            var result = _store.ReadSlot(0, 0, 8);

            Assert.AreEqual(PalSlotState.Present, result.State);
            CollectionAssert.AreEqual(new byte[] { 2, 2, 2 }, result.Payload);
        }

        [Test]
        public void WriteTwice_ToSameSlot_AppendsRatherThanOverwritingInPlace()
        {
            var path = Path.Combine(_regionsDir, "r.0.0.pal");

            _store.WriteSlot(0, 0, 0, new byte[] { 1, 1, 1 });
            var lengthAfterFirstWrite = new FileInfo(path).Length;

            _store.WriteSlot(0, 0, 0, new byte[] { 2, 2, 2 });
            var lengthAfterSecondWrite = new FileInfo(path).Length;

            Assert.Greater(lengthAfterSecondWrite, lengthAfterFirstWrite);
        }

        [Test]
        public void DifferentSlots_InSameRegion_AreIndependent()
        {
            _store.WriteSlot(0, 0, 0, new byte[] { 1 });
            _store.WriteSlot(0, 0, 255, new byte[] { 2 });

            var first = _store.ReadSlot(0, 0, 0);
            var last = _store.ReadSlot(0, 0, 255);
            var untouched = _store.ReadSlot(0, 0, 128);

            Assert.AreEqual(PalSlotState.Present, first.State);
            Assert.AreEqual(PalSlotState.Present, last.State);
            Assert.AreEqual(PalSlotState.Missing, untouched.State);
        }

        [Test]
        public void NegativeRegionCoordinates_RoundTripCorrectly()
        {
            _store.WriteSlot(-3, 5, 1, new byte[] { 42 });

            var result = _store.ReadSlot(-3, 5, 1);

            Assert.AreEqual(PalSlotState.Present, result.State);
            CollectionAssert.AreEqual(new byte[] { 42 }, result.Payload);
        }

        [Test]
        public void CorruptedPayloadBytes_AreDetectedAsCorrupted()
        {
            var path = Path.Combine(_regionsDir, "r.0.0.pal");
            _store.WriteSlot(0, 0, 0, new byte[] { 10, 20, 30 });

            using (var fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite))
            {
                fs.Seek(-1, SeekOrigin.End);
                fs.WriteByte(99);
            }

            var result = _store.ReadSlot(0, 0, 0);

            Assert.AreEqual(PalSlotState.Corrupted, result.State);
            Assert.IsNull(result.Payload);
        }

        [Test]
        public void OutOfRangeSlotIndex_ThrowsOnWriteAndRead()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _store.WriteSlot(0, 0, -1, new byte[] { 1 }));
            Assert.Throws<ArgumentOutOfRangeException>(() => _store.WriteSlot(0, 0, 256, new byte[] { 1 }));
            Assert.Throws<ArgumentOutOfRangeException>(() => _store.ReadSlot(0, 0, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => _store.ReadSlot(0, 0, 256));
        }

        [Test]
        public void GarbageFileWithWrongMagic_ThrowsInvalidDataException()
        {
            Directory.CreateDirectory(_regionsDir);
            File.WriteAllBytes(Path.Combine(_regionsDir, "r.0.0.pal"), new byte[64]);

            Assert.Throws<InvalidDataException>(() => _store.ReadSlot(0, 0, 0));
        }
    }
}