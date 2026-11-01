using System.Collections.Generic;
using System.Reflection;
using _Project.Features.Items.Domain;
using _Project.Features.Items.Infrastructure;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace _Project.Tests.Items
{
    public class HarvestDropTableTests
    {
        private HarvestDropTable _table;
        private ItemDefinitionAsset _wood;
        private ItemDefinitionAsset _apple;

        [SetUp]
        public void SetUp()
        {
            _table = ScriptableObject.CreateInstance<HarvestDropTable>();
            _wood = CreateItem("wood");
            _apple = CreateItem("apple");
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_table);
            Object.DestroyImmediate(_wood);
            Object.DestroyImmediate(_apple);
        }

        [Test]
        public void Roll_UsesOneSharedStepAndIndependentRollValues()
        {
            Configure(1f, ("wood", _wood, 60f), ("apple", _apple, 40f));
            var drops = new List<ItemId>();

            Random.InitState(1234);
            _table.Roll(3f, drops.Add);

            Assert.That(drops.Count, Is.EqualTo(3));
            Assert.That(drops.TrueForAll(id => id == new ItemId("wood") || id == new ItemId("apple")), Is.True);
        }

        [Test]
        public void Roll_TotalBelowHundredLeavesRemainderEmpty()
        {
            Configure(1f, ("wood", _wood, 40f));
            var drops = new List<ItemId>();
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("remaining chance produces no item"));
            Random.InitState(99);
            _table.Roll(1f, drops.Add);

            Assert.That(drops, Is.Empty);
        }

        [Test]
        public void Roll_IgnoresDamageBelowSharedStep()
        {
            Configure(2f, ("wood", _wood, 100f));
            var drops = new List<ItemId>();
            _table.Roll(1.99f, drops.Add);
            Assert.That(drops, Is.Empty);
        }

        private ItemDefinitionAsset CreateItem(string id)
        {
            var item = ScriptableObject.CreateInstance<ItemDefinitionAsset>();
            Set(item, "technicalId", id);
            return item;
        }

        private void Configure(float step, params (string name, ItemDefinitionAsset item, float chance)[] entries)
        {
            var entryType = typeof(HarvestDropTable).GetNestedType("Entry", BindingFlags.NonPublic);
            System.Array configured = System.Array.CreateInstance(entryType, entries.Length);
            for (int i = 0; i < entries.Length; i++)
            {
                var entry = System.Activator.CreateInstance(entryType, true);
                Set(entry, "item", entries[i].item);
                Set(entry, "chance", entries[i].chance);
                configured.SetValue(entry, i);
            }
            Set(_table, "entries", configured);
            Set(_table, "damageStep", step);
        }

        private static void Set(object target, string name, object value) =>
            target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
    }
}

