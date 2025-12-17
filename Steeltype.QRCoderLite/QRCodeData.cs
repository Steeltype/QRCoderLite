using System.Collections;
using System.IO.Compression;

namespace Steeltype.QRCoderLite
{
    public class QRCodeData : IDisposable
    {
        public List<BitArray> ModuleMatrix { get; private set; }

        public QRCodeData(int version)
        {
            Version = version;
            var size = ModulesPerSideFromVersion(version);
            ModuleMatrix = new List<BitArray>();
            for (var i = 0; i < size; i++)
                ModuleMatrix.Add(new BitArray(size));
        }

        /// <summary>
        /// Maximum decompressed size to prevent decompression bomb attacks (10 MB).
        /// </summary>
        private const int MaxDecompressedSize = 10 * 1024 * 1024;

        /// <summary>
        /// Maximum QR code version (40) has 177x177 modules.
        /// </summary>
        private const int MaxSideLength = 177 + 8; // Version 40 + quiet zone

        public QRCodeData(byte[] rawData, Compression compressMode)
        {
            ArgumentNullException.ThrowIfNull(rawData);

            if (rawData.Length < 5)
                throw new ArgumentException("Raw data too short to contain valid QR code data.", nameof(rawData));

            var bytes = new List<byte>(rawData);

            //Decompress with size limit to prevent decompression bombs
            if (compressMode == Compression.Deflate)
            {
                using var input = new MemoryStream(bytes.ToArray());
                using var output = new MemoryStream();
                using (var decompressedStream = new DeflateStream(input, CompressionMode.Decompress))
                {
                    CopyToWithLimit(decompressedStream, output, MaxDecompressedSize);
                }
                bytes = new List<byte>(output.ToArray());
            }
            else if (compressMode == Compression.GZip)
            {
                using var input = new MemoryStream(bytes.ToArray());
                using var output = new MemoryStream();
                using (var decompressedStream = new GZipStream(input, CompressionMode.Decompress))
                {
                    CopyToWithLimit(decompressedStream, output, MaxDecompressedSize);
                }
                bytes = new List<byte>(output.ToArray());
            }

            if (bytes.Count < 5 || bytes[0] != 0x51 || bytes[1] != 0x52 || bytes[2] != 0x52)
                throw new InvalidDataException("Invalid raw data file. File type doesn't match \"QRR\".");

            //Set QR code version with bounds checking
            var sideLen = (int)bytes[4];
            if (sideLen < 21 || sideLen > MaxSideLength)
                throw new ArgumentOutOfRangeException(nameof(rawData), $"Invalid QR code side length: {sideLen}. Must be between 21 and {MaxSideLength}.");

            bytes.RemoveRange(0, 5);
            Version = (sideLen - 21 - 8) / 4 + 1;

            if (Version < 1 || Version > 40)
                throw new ArgumentOutOfRangeException(nameof(rawData), $"Invalid QR code version: {Version}. Must be between 1 and 40.");

            //Verify we have enough data
            var requiredBits = sideLen * sideLen;
            var availableBits = bytes.Count * 8;
            if (availableBits < requiredBits)
                throw new InvalidDataException($"Insufficient data for QR code. Need {requiredBits} bits, have {availableBits}.");

            //Unpack
            var modules = new Queue<bool>(8 * bytes.Count);
            foreach (var b in bytes)
            {
                for (var i = 7; i >= 0; i--)
                {
                    modules.Enqueue((b & (1 << i)) != 0);
                }
            }

            //Build module matrix
            ModuleMatrix = new List<BitArray>(sideLen);
            for (var y = 0; y < sideLen; y++)
            {
                ModuleMatrix.Add(new BitArray(sideLen));
                for (var x = 0; x < sideLen; x++)
                {
                    ModuleMatrix[y][x] = modules.Dequeue();
                }
            }
        }

        private static void CopyToWithLimit(Stream source, Stream destination, int maxBytes)
        {
            var buffer = new byte[81920];
            int bytesRead;
            var totalBytesRead = 0;

            while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
            {
                totalBytesRead += bytesRead;
                if (totalBytesRead > maxBytes)
                    throw new InvalidDataException($"Decompressed data exceeds maximum allowed size of {maxBytes} bytes.");

                destination.Write(buffer, 0, bytesRead);
            }
        }

        public byte[] GetRawData(Compression compressMode)
        {
            var bytes = new List<byte>();

            //Add header - signature ("QRR")
            bytes.AddRange(new byte[] { 0x51, 0x52, 0x52, 0x00 });

            //Add header - rowsize
            bytes.Add((byte)ModuleMatrix.Count);

            //Build data queue
            var dataQueue = new Queue<int>();
            foreach (var row in ModuleMatrix)
            {
                foreach (var module in row)
                {
                    dataQueue.Enqueue((bool)module ? 1 : 0);
                }
            }
            for (var i = 0; i < 8 - (ModuleMatrix.Count * ModuleMatrix.Count) % 8; i++)
            {
                dataQueue.Enqueue(0);
            }

            //Process queue
            while (dataQueue.Count > 0)
            {
                byte b = 0;
                for (var i = 7; i >= 0; i--)
                {
                    b += (byte)(dataQueue.Dequeue() << i);
                }
                bytes.Add(b);
            }
            var rawData = bytes.ToArray();

            //Compress stream (optional)
            if (compressMode == Compression.Deflate)
            {
                using var output = new MemoryStream();
                using (var decompressedStream = new DeflateStream(output, CompressionMode.Compress))
                {
                    decompressedStream.Write(rawData, 0, rawData.Length);
                }
                rawData = output.ToArray();
            }
            else if (compressMode == Compression.GZip)
            {
                using var output = new MemoryStream();
                using (var gzipStream = new GZipStream(output, CompressionMode.Compress, true))
                {
                    gzipStream.Write(rawData, 0, rawData.Length);
                }
                rawData = output.ToArray();
            }
            return rawData;
        }

        public void SaveRawData(string filePath, Compression compressMode)
        {
            File.WriteAllBytes(filePath, GetRawData(compressMode));
        }

        public int Version { get; private set; }

        private static int ModulesPerSideFromVersion(int version)
        {
            return 21 + (version - 1) * 4;
        }

        public void Dispose()
        {
            ModuleMatrix = null;
            Version = 0;

        }

        public enum Compression
        {
            Uncompressed,
            Deflate,
            GZip
        }
    }
}
