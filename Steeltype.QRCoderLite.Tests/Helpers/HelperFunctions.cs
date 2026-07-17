using System.Buffers.Binary;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

namespace Steeltype.QRCoderLite.Tests.Helpers
{
    public static class HelperFunctions
    {
        public static string GetAssemblyPath()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        public static string ByteArrayToHash(byte[] data)
        {
            var md5 = MD5.Create();
            var hash = md5.ComputeHash(data);
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

        public static string StringToHash(string data)
        {
            return ByteArrayToHash(Encoding.UTF8.GetBytes(data));
        }

        /// <summary>
        /// Platform-stable hash of a PNG's decoded content: IHDR/PLTE/tRNS chunk data plus the
        /// INFLATED IDAT payload. Raw-file hashes are not portable because DeflateStream emits
        /// different (equally valid) compressed bytes per platform; the decompressed scanlines
        /// and header/palette data are identical everywhere.
        /// </summary>
        public static string PngContentHash(byte[] png)
        {
            using var content = new MemoryStream();
            using var idat = new MemoryStream();
            var pos = 8; // skip PNG signature
            while (pos + 8 <= png.Length)
            {
                var length = BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(pos, 4));
                var type = Encoding.ASCII.GetString(png, pos + 4, 4);
                var dataStart = pos + 8;
                switch (type)
                {
                    case "IHDR":
                    case "PLTE":
                    case "tRNS":
                        content.Write(Encoding.ASCII.GetBytes(type));
                        content.Write(png, dataStart, length);
                        break;
                    case "IDAT":
                        idat.Write(png, dataStart, length);
                        break;
                }
                pos = dataStart + length + 4; // skip data + CRC
            }
            content.Write(Encoding.ASCII.GetBytes("IDAT"));
            idat.Position = 0;
            using (var inflate = new ZLibStream(idat, CompressionMode.Decompress))
                inflate.CopyTo(content);
            return ByteArrayToHash(content.ToArray());
        }
    }
}
