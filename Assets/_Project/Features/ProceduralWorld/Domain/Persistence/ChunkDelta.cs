using System;
using System.Collections.Generic;
using System.Linq;

namespace _Project.Features.ProceduralWorld.Domain.Persistence
{
    /// <summary>
    /// Identifies which generator produced the baseline this delta was recorded against.
    /// </summary>
    public readonly struct GeneratorVersionStamp
    {
        public readonly int VegetationVersion;

        public GeneratorVersionStamp(int vegetationVersion) => VegetationVersion = vegetationVersion;
    }

    public enum DeltaAction : byte
    {
        Removed = 0,
        Modified = 1,
        Added = 2
    }

    public readonly struct VegetationInstanceDelta
    {
        public readonly ulong Id;
        public readonly DeltaAction Action;
        public readonly byte[] ExtraData;

        public VegetationInstanceDelta(ulong id, DeltaAction action, byte[] extraData)
        {
            Id = id;
            Action = action;
            ExtraData = extraData;
        }
    }

    /// <summary>
    /// A persisted change to a dropped world item (an object lying on the ground).
    /// Position is stored relative to the owning chunk origin, so the record stays
    /// valid across floating-origin rebases.
    /// </summary>
    public readonly struct WorldItemDelta
    {
        public readonly ulong Id;
        public readonly DeltaAction Action;
        public readonly string ItemKey;
        public readonly float X;
        public readonly float Y;
        public readonly float Z;

        public WorldItemDelta(ulong id, DeltaAction action, string itemKey, float x, float y, float z)
        {
            Id = id;
            Action = action;
            ItemKey = itemKey;
            X = x;
            Y = y;
            Z = z;
        }
    }

    /// <summary>
    /// Immutable snapshot of everything that diverges from the deterministic baseline
    /// for a single chunk.
    /// </summary>
    public sealed class ChunkDelta
    {
        public GeneratorVersionStamp Versions { get; }
        public IReadOnlyList<VegetationInstanceDelta> VegetationDeltas { get; }
        public IReadOnlyList<WorldItemDelta> ItemDeltas { get; }

        public ChunkDelta(
            GeneratorVersionStamp versions,
            IReadOnlyList<VegetationInstanceDelta> vegetationDeltas)
            : this(versions, vegetationDeltas, Array.Empty<WorldItemDelta>())
        {
        }

        public ChunkDelta(
            GeneratorVersionStamp versions,
            IReadOnlyList<VegetationInstanceDelta> vegetationDeltas,
            IReadOnlyList<WorldItemDelta> itemDeltas)
        {
            Versions = versions;
            VegetationDeltas = vegetationDeltas;
            ItemDeltas = itemDeltas;
        }

        public bool IsEmpty => VegetationDeltas.Count == 0 && ItemDeltas.Count == 0;

        public static readonly ChunkDelta Empty =
            new ChunkDelta(default, Array.Empty<VegetationInstanceDelta>());

        public ChunkDelta Merge(ChunkDelta newer)
        {
            if (this.IsEmpty) return newer;
            if (newer.IsEmpty) return this;

            return new ChunkDelta(
                newer.Versions,
                MergeByNewerId(this.VegetationDeltas, newer.VegetationDeltas, delta => delta.Id),
                MergeByNewerId(this.ItemDeltas, newer.ItemDeltas, delta => delta.Id));
        }

        /// <summary>
        /// Merges two delta lists keyed by id: entries from <paramref name="newer"/>
        /// override entries from <paramref name="older"/> with the same id.
        /// </summary>
        private static List<TDelta> MergeByNewerId<TDelta>(
            IReadOnlyList<TDelta> older,
            IReadOnlyList<TDelta> newer,
            Func<TDelta, ulong> keySelector)
        {
            var merged = new Dictionary<ulong, TDelta>(older.Count + newer.Count);

            foreach (var delta in older)
                merged[keySelector(delta)] = delta;

            foreach (var delta in newer)
                merged[keySelector(delta)] = delta;

            return merged.Values.ToList();
        }
    }
}
