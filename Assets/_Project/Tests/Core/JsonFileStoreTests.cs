using NUnit.Framework;
using System.IO;
using _Project.Features.Persistence.Infrastructure;
using UnityEngine;

namespace _Project.Tests.Core.Persistence
{
    public sealed class JsonFileStoreTests
    {
        private JsonFileStore _store;

        [SetUp]
        public void SetUp()
        {
            _store = new JsonFileStore();
        }

        [TearDown]
        public void TearDown()
        {
            string rootPath = Application.persistentDataPath;

            foreach (string file in Directory.GetFiles(rootPath, "JsonFileStoreTests_*"))
            {
                File.Delete(file);
            }
        }

        [Test]
        public void Write_CreatesJsonFile()
        {
            var data = new TestData
            {
                Name = "Test",
                Value = 42
            };

            _store.Write("JsonFileStoreTests_Write", data);

            string path = Path.Combine(
                Application.persistentDataPath,
                "JsonFileStoreTests_Write.json");

            Assert.That(File.Exists(path), Is.True);
        }

        [Test]
        public void WriteAndRead_PreservesData()
        {
            var expected = new TestData
            {
                Name = "Test",
                Value = 42
            };

            _store.Write("JsonFileStoreTests_Read", expected);

            bool result = _store.TryRead(
                "JsonFileStoreTests_Read",
                out TestData actual);

            Assert.That(result, Is.True);
            Assert.That(actual.Name, Is.EqualTo(expected.Name));
            Assert.That(actual.Value, Is.EqualTo(expected.Value));
        }

        [Test]
        public void WriteAndRead_PreservesEnumAsString()
        {
            var expected = new TestDataWithEnum
            {
                Status = TestStatus.Completed
            };

            _store.Write("JsonFileStoreTests_Enum", expected);

            bool result = _store.TryRead(
                "JsonFileStoreTests_Enum",
                out TestDataWithEnum actual);

            Assert.That(result, Is.True);
            Assert.That(actual.Status, Is.EqualTo(TestStatus.Completed));
        }

        [Test]
        public void TryRead_ReturnsFalseWhenFileDoesNotExist()
        {
            bool result = _store.TryRead(
                "JsonFileStoreTests_Missing",
                out TestData data);

            Assert.That(result, Is.False);
            Assert.That(data, Is.Null);
        }

        [Test]
        public void TryRead_ReturnsFalseWhenJsonIsInvalid()
        {
            string path = Path.Combine(
                Application.persistentDataPath,
                "JsonFileStoreTests_Invalid.json");

            File.WriteAllText(path, "{ invalid json");

            bool result = _store.TryRead(
                "JsonFileStoreTests_Invalid",
                out TestData data);

            Assert.That(result, Is.False);
            Assert.That(data, Is.Null);
        }

        [Test]
        public void Write_OverwritesExistingData()
        {
            var first = new TestData
            {
                Name = "First",
                Value = 1
            };

            var second = new TestData
            {
                Name = "Second",
                Value = 2
            };

            _store.Write("JsonFileStoreTests_Overwrite", first);
            _store.Write("JsonFileStoreTests_Overwrite", second);

            bool result = _store.TryRead(
                "JsonFileStoreTests_Overwrite",
                out TestData actual);

            Assert.That(result, Is.True);
            Assert.That(actual.Name, Is.EqualTo("Second"));
            Assert.That(actual.Value, Is.EqualTo(2));
        }

        private sealed class TestData
        {
            public string Name;
            public int Value;
        }

        private sealed class TestDataWithEnum
        {
            public TestStatus Status;
        }

        private enum TestStatus
        {
            Pending,
            Completed
        }
    }
}