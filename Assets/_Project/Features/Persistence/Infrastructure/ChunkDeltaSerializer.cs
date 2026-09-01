using System;
using System.Collections.Generic;
using System.IO;
using _Project.Features.ProceduralWorld.Domain.Persistence;

namespace _Project.Features.Persistence.Infrastructure
{
    public sealed class ChunkDeltaSerializer
    {
        public byte[] Serialize(ChunkDelta delta)
        {
            using var ms = new MemoryStream();
            using var w = new BinaryWriter(ms);

            w.Write(delta.Versions.VegetationVersion);
            w.Write(delta.VegetationDeltas.Count);

            foreach (var d in delta.VegetationDeltas)
            {
                w.Write(d.Id);
                w.Write((byte)d.Action);

                var extra = d.ExtraData ?? Array.Empty<byte>();
                w.Write(extra.Length);
                w.Write(extra);
            }

            return ms.ToArray();
        }

        public ChunkDelta Deserialize(byte[] payload)
        {
            using var ms = new MemoryStream(payload);
            using var r = new BinaryReader(ms);

            var versions = new GeneratorVersionStamp(r.ReadInt32());
            int count = r.ReadInt32();
            var list = new List<VegetationInstanceDelta>(count);

            for (int i = 0; i < count; i++)
            {
                ulong id = r.ReadUInt64();
                var action = (DeltaAction)r.ReadByte();
                int extraLen = r.ReadInt32();
                byte[] extra = extraLen > 0 ? r.ReadBytes(extraLen) : null;
                list.Add(new VegetationInstanceDelta(id, action, extra));
            }

            return new ChunkDelta(versions, list);
        }
    }
}