using System.IO;

namespace _Project.Features.Persistence.Infrastructure
{
    internal static class PalRegionFormat
    {
        public static readonly byte[] Magic = { (byte)'P', (byte)'A', (byte)'L', (byte)'M' };

        public const int Version = 1;
        
        public const int RegionSize = 16;
        public const int SlotCount = RegionSize * RegionSize;

        public const int HeaderSize = 16;
        public const int EntrySize = 28;

        public static int IndexTableSize => SlotCount * EntrySize;

        public static long EntryOffset(int slotIndex) =>
            HeaderSize + (long)slotIndex * EntrySize;
    }

    internal enum PalSlotFlags : byte
    {
        Empty = 0,
        Present = 1,
        Tombstone = 2
    }
    
    internal struct PalSlotEntry
    {
        public long Offset;
        public int Length;
        public long TimestampTicks;
        public uint Checksum;
        public PalSlotFlags Flags;

        public static readonly PalSlotEntry Empty = new PalSlotEntry
        {
            Offset = 0,
            Length = 0,
            TimestampTicks = 0,
            Checksum = 0,
            Flags = PalSlotFlags.Empty
        };

        public void WriteTo(BinaryWriter writer)
        {
            writer.Write(Offset);
            writer.Write(Length);
            writer.Write(TimestampTicks);
            writer.Write(Checksum);
            writer.Write((byte)Flags);
            writer.Write((byte)0);
            writer.Write((byte)0);
            writer.Write((byte)0);
        }

        public static PalSlotEntry ReadFrom(BinaryReader reader)
        {
            var entry = new PalSlotEntry
            {
                Offset = reader.ReadInt64(),
                Length = reader.ReadInt32(),
                TimestampTicks = reader.ReadInt64(),
                Checksum = reader.ReadUInt32(),
                Flags = (PalSlotFlags)reader.ReadByte()
            };

            reader.ReadByte();
            reader.ReadByte();
            reader.ReadByte();

            return entry;
        }
    }
}