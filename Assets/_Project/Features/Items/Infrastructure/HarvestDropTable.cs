using System;
using System.Collections.Generic;
using _Project.Features.Items.Domain;
using UnityEngine;

namespace _Project.Features.Items.Infrastructure
{
    [CreateAssetMenu(menuName = "Project/Items/Harvest Drop Table", fileName = "HarvestDropTable")]
    public sealed class HarvestDropTable : ScriptableObject
    {
        [Serializable]
        private sealed class Entry
        {
            [SerializeField] private ItemDefinitionAsset item;
            [SerializeField, Range(0f, 100f)] private float chance = 100f;
            public ItemDefinitionAsset Item => item;
            public float Chance => chance;
        }

        [SerializeField] private Entry[] entries = Array.Empty<Entry>();
        [SerializeField, Min(0.01f)] private float damageStep = 1f;
        public float DamageStep => Mathf.Max(0.01f, damageStep);

        public void Roll(float damage, Action<ItemId> onDrop)
        {
            if (damage <= 0f || onDrop == null || damageStep <= 0f) return;
            int rolls = Mathf.FloorToInt(damage / damageStep);
            float total = 0f;
            foreach (Entry entry in entries ?? Array.Empty<Entry>())
                if (entry != null && entry.Item != null) total += Mathf.Max(0f, entry.Chance);
            float cap = Mathf.Min(total, 100f);
            if (total > 100f) Debug.LogWarning($"Harvest drop chances on '{name}' total {total:0.##}% and were capped at 100%.", this);
            else if (total < 100f) Debug.LogWarning($"Harvest drop chances on '{name}' total {total:0.##}%; remaining chance produces no item.", this);

            var validEntries = new List<Entry>();
            foreach (Entry entry in entries ?? Array.Empty<Entry>())
                if (entry != null && entry.Item != null && entry.Chance > 0f) validEntries.Add(entry);
            var cumulativeChances = new float[validEntries.Count];
            float cumulative = 0f;
            for (int i = 0; i < validEntries.Count; i++)
            {
                cumulative += Mathf.Max(0f, validEntries[i].Chance);
                cumulativeChances[i] = cumulative;
            }

            for (int roll = 0; roll < rolls; roll++)
            {
                float rollValue = UnityEngine.Random.value * 100f;
                if (rollValue >= cap) continue;
                for (int i = 0; i < validEntries.Count; i++)
                    if (rollValue < cumulativeChances[i]) { onDrop(validEntries[i].Item.Id); break; }
            }
        }
    }
}
