using System;
using System.Collections.Generic;

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
    /// Immutable snapshot of everything that diverges from the deterministic baseline
    /// for a single chunk.
    /// </summary>
    public sealed class ChunkDelta
    {
        public GeneratorVersionStamp Versions { get; }
        public IReadOnlyList<VegetationInstanceDelta> VegetationDeltas { get; }

        public ChunkDelta(GeneratorVersionStamp versions, IReadOnlyList<VegetationInstanceDelta> vegetationDeltas)
        {
            Versions = versions;
            VegetationDeltas = vegetationDeltas;
        }

        public bool IsEmpty => VegetationDeltas.Count == 0;

        public static readonly ChunkDelta Empty =
            new ChunkDelta(default, Array.Empty<VegetationInstanceDelta>());
    }
}