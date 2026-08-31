using System;
using System.IO;
using _Project.Features.Persistence.Application;
using _Project.Features.Persistence.Infrastructure;
using _Project.Features.ProceduralWorld.Domain.World;

namespace _Project.Features.Persistence.Domain
{
    public class PalRegionFileStore : IPalRegionWriter, IPalRegionReader
    {
        private readonly IWorldSettings _worldSettings;

        public PalRegionFileStore(IWorldSettings worldSettings)
        {
            _worldSettings = worldSettings;
        }


        public void WriteSlot(int regionX, int regionZ, int slotIndex, byte[] payload)
        {
            ValidateSlotIndex(slotIndex);
            payload ??= Array.Empty<byte>();

            var path = GetRegionPath(regionX, regionZ);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);

            using var fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);

            if (fs.Length == 0)
            {
                InitializeNewFile(fs);
            }
            else
            {
                ValidateHeader(fs);
            }
            
            fs.Seek(0, SeekOrigin.End);
            var appendOffset = fs.Position;
            fs.Write(payload, 0, payload.Length);
            fs.Flush();

            var entry = new PalSlotEntry
            {
                Offset = appendOffset,
                Length = payload.Length,
                TimestampTicks = DateTime.UtcNow.Ticks,
                Checksum = Crc32.Compute(payload),
                Flags = PalSlotFlags.Present
            };

            WriteEntry(fs, slotIndex, entry);
        }

        public void DeleteSlot(int regionX, int regionZ, int slotIndex)
        {
            ValidateSlotIndex(slotIndex);

            var path = GetRegionPath(regionX, regionZ);
            if (!File.Exists(path))
                return;

            using var fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
            ValidateHeader(fs);

            var entry = new PalSlotEntry
            {
                Offset = 0,
                Length = 0,
                TimestampTicks = DateTime.UtcNow.Ticks,
                Checksum = 0,
                Flags = PalSlotFlags.Tombstone
            };

            WriteEntry(fs, slotIndex, entry);
        }

        public PalSlotReadResult ReadSlot(int regionX, int regionZ, int slotIndex)
        {
            ValidateSlotIndex(slotIndex);

            var path = GetRegionPath(regionX, regionZ);
            if (!File.Exists(path))
                return PalSlotReadResult.Missing;

            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            ValidateHeader(fs);

            var entry = ReadEntry(fs, slotIndex);

            switch (entry.Flags)
            {
                case PalSlotFlags.Empty:
                    return PalSlotReadResult.Missing;

                case PalSlotFlags.Tombstone:
                    return PalSlotReadResult.Tombstoned;

                case PalSlotFlags.Present:
                {
                    var payload = new byte[entry.Length];
                    if (entry.Length > 0)
                    {
                        fs.Seek(entry.Offset, SeekOrigin.Begin);
                        var read = fs.Read(payload, 0, entry.Length);
                        if (read != entry.Length)
                            return new PalSlotReadResult(PalSlotState.Corrupted, null);
                    }

                    var checksum = Crc32.Compute(payload);
                    return checksum == entry.Checksum
                        ? new PalSlotReadResult(PalSlotState.Present, payload)
                        : new PalSlotReadResult(PalSlotState.Corrupted, null);
                }

                default:
                    return new PalSlotReadResult(PalSlotState.Corrupted, null);
            }
        }

        private static void InitializeNewFile(FileStream fs)
        {
            fs.Seek(0, SeekOrigin.Begin);
            using var writer = new BinaryWriter(fs, System.Text.Encoding.UTF8, true);

            writer.Write(PalRegionFormat.Magic);
            writer.Write(PalRegionFormat.Version);
            writer.Write((byte)PalRegionFormat.RegionSize);
            writer.Write((byte)0);
            writer.Write((byte)0);
            writer.Write((byte)0);
            writer.Write((byte)0);
            writer.Write((byte)0);
            writer.Write((byte)0);
            writer.Write((byte)0);

            for (var i = 0; i < PalRegionFormat.SlotCount; i++)
            {
                PalSlotEntry.Empty.WriteTo(writer);
            }

            writer.Flush();
        }

        private static void ValidateHeader(FileStream fs)
        {
            if (fs.Length < PalRegionFormat.HeaderSize + PalRegionFormat.IndexTableSize)
                throw new InvalidDataException($"'{fs.Name}' is smaller than a valid .pal header + index table.");

            fs.Seek(0, SeekOrigin.Begin);
            using var reader = new BinaryReader(fs, System.Text.Encoding.UTF8, true);

            var magic = reader.ReadBytes(4);
            for (var i = 0; i < 4; i++)
            {
                if (magic[i] != PalRegionFormat.Magic[i])
                    throw new InvalidDataException($"'{fs.Name}' does not start with the PALM magic bytes.");
            }

            var version = reader.ReadInt32();
            if (version != PalRegionFormat.Version)
                throw new InvalidDataException($"'{fs.Name}' has unsupported .pal version {version}.");

            var regionSize = reader.ReadByte();
            if (regionSize != PalRegionFormat.RegionSize)
                throw new InvalidDataException($"'{fs.Name}' has region size {regionSize}, expected {PalRegionFormat.RegionSize}.");
        }

        private static PalSlotEntry ReadEntry(FileStream fs, int slotIndex)
        {
            fs.Seek(PalRegionFormat.EntryOffset(slotIndex), SeekOrigin.Begin);
            using var reader = new BinaryReader(fs, System.Text.Encoding.UTF8, true);
            return PalSlotEntry.ReadFrom(reader);
        }

        private static void WriteEntry(FileStream fs, int slotIndex, PalSlotEntry entry)
        {
            fs.Seek(PalRegionFormat.EntryOffset(slotIndex), SeekOrigin.Begin);
            using var writer = new BinaryWriter(fs, System.Text.Encoding.UTF8, true);
            entry.WriteTo(writer);
            writer.Flush();
        }

        private static void ValidateSlotIndex(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= PalRegionFormat.SlotCount)
                throw new ArgumentOutOfRangeException(nameof(slotIndex),
                    $"Slot index must be in [0, {PalRegionFormat.SlotCount}).");
        }

        private string GetRegionPath(int regionX, int regionZ) =>
            Path.Combine(
                UnityEngine.Application.persistentDataPath,
                "Worlds",
                _worldSettings.Name,
                "Regions",
                $"r.{regionX}.{regionZ}.pal");
    }
}