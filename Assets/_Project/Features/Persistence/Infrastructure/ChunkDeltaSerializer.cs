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

            // Item section is appended last so that payloads written before item
            // persistence was introduced stay readable.
            w.Write(delta.ItemDeltas.Count);

            foreach (var d in delta.ItemDeltas)
            {
                w.Write(d.Id);
                w.Write((byte)d.Action);
                w.Write(d.ItemKey ?? string.Empty);
                w.Write(d.X);
                w.Write(d.Y);
                w.Write(d.Z);
            }

            return ms.ToArray();
        }

        public ChunkDelta Deserialize(byte[] payload)
        {
            using var ms = new MemoryStream(payload);
            using var r = new BinaryReader(ms);

            var versions = new GeneratorVersionStamp(r.ReadInt32());
            int vegetationCount = r.ReadInt32();
            var vegetation = new List<VegetationInstanceDelta>(vegetationCount);

            for (int i = 0; i < vegetationCount; i++)
            {
                ulong id = r.ReadUInt64();
                var action = (DeltaAction)r.ReadByte();
                int extraLen = r.ReadInt32();
                byte[] extra = extraLen > 0 ? r.ReadBytes(extraLen) : null;
                vegetation.Add(new VegetationInstanceDelta(id, action, extra));
            }

            var items = ReadItems(r);

            return new ChunkDelta(versions, vegetation, items);
        }

        /// <summary>
        /// Reads the trailing item section. Payloads written before item
        /// persistence was introduced end right after the vegetation deltas,
        /// so an end of stream at the very start of the section means a legacy
        /// payload (empty item list), not corruption.
        /// </summary>
        private static List<WorldItemDelta> ReadItems(BinaryReader reader)
        {
            var items = new List<WorldItemDelta>();

            try
            {
                int count = reader.ReadInt32();

                for (int i = 0; i < count; i++)
                {
                    ulong id = reader.ReadUInt64();
                    var action = (DeltaAction)reader.ReadByte();
                    string itemKey = reader.ReadString();
                    float x = reader.ReadSingle();
                    float y = reader.ReadSingle();
                    float z = reader.ReadSingle();
                    items.Add(new WorldItemDelta(id, action, itemKey, x, y, z));
                }
            }
            catch (EndOfStreamException) when (items.Count == 0 && reader.BaseStream.Position == reader.BaseStream.Length)
            {
                // Legacy payload without the item section.
            }

            return items;
        }
    }
}
