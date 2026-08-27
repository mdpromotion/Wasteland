namespace _Project.Features.Core.Persistence.Regions
{
    internal static class Crc32
    {
        private static readonly uint[] Table = BuildTable();

        private static uint[] BuildTable()
        {
            const uint poly = 0xEDB88320;
            var table = new uint[256];

            for (uint i = 0; i < 256; i++)
            {
                var c = i;
                for (var k = 0; k < 8; k++)
                {
                    c = (c & 1) != 0 ? poly ^ (c >> 1) : c >> 1;
                }
                table[i] = c;
            }

            return table;
        }

        public static uint Compute(byte[] data)
        {
            if (data == null || data.Length == 0)
                return 0;

            var crc = 0xFFFFFFFF;

            foreach (var b in data)
            {
                crc = Table[(crc ^ b) & 0xFF] ^ (crc >> 8);
            }

            return crc ^ 0xFFFFFFFF;
        }
    }
}