using System.ComponentModel;
using System.IO.Compression;
using System.Text;
using Shouldly;
using Xunit;

namespace Steeltype.QRCoderLite.Tests
{

    public class QRCodeDataTests
    {
        // -------------------------------------------------------------------
        // Round-trip: GetRawData -> new QRCodeData(bytes, mode)
        // -------------------------------------------------------------------

        [Theory]
        [Category("QRCodeData/RoundTrip")]
        [InlineData(QRCodeData.Compression.Uncompressed, 3)]
        [InlineData(QRCodeData.Compression.Deflate, 3)]
        [InlineData(QRCodeData.Compression.GZip, 3)]
        [InlineData(QRCodeData.Compression.Uncompressed, 32)]
        [InlineData(QRCodeData.Compression.Deflate, 32)]
        [InlineData(QRCodeData.Compression.GZip, 32)]
        public void can_save_and_load_qrcode_data(QRCodeData.Compression compressionMode, int requestedVersion)
        {
            var gen = new QRCodeGenerator();
            var originalQrData = gen.CreateQrCode("QRCoderLite", QRCodeGenerator.ECCLevel.Q, requestedVersion: requestedVersion);

            var rawData = originalQrData.GetRawData(compressionMode);
            var reloadedQrData = new QRCodeData(rawData, compressionMode);

            reloadedQrData.Version.ShouldBe(requestedVersion);
            reloadedQrData.Version.ShouldBe(originalQrData.Version);
            reloadedQrData.ModuleMatrix.Count.ShouldBe(originalQrData.ModuleMatrix.Count);
            MatrixToString(reloadedQrData).ShouldBe(MatrixToString(originalQrData));

            // Byte-level identity: re-serializing the reloaded matrix reproduces the
            // exact uncompressed raw data of the original.
            reloadedQrData.GetRawData(QRCodeData.Compression.Uncompressed)
                .ShouldBe(originalQrData.GetRawData(QRCodeData.Compression.Uncompressed));
        }

        // -------------------------------------------------------------------
        // Raw-data validation (fork-specific hardening)
        // -------------------------------------------------------------------

        [Fact]
        [Category("QRCodeData/Validation")]
        public void null_raw_data_throws()
        {
            Should.Throw<ArgumentNullException>(() => new QRCodeData(null!, QRCodeData.Compression.Uncompressed));
        }

        [Fact]
        [Category("QRCodeData/Validation")]
        public void raw_data_shorter_than_header_throws()
        {
            var ex = Should.Throw<ArgumentException>(() => new QRCodeData(new byte[4], QRCodeData.Compression.Uncompressed));
            ex.Message.ShouldContain("too short");
        }

        [Fact]
        [Category("QRCodeData/Validation")]
        public void wrong_signature_throws()
        {
            var rawData = CraftRawData(29);
            rawData[0] = (byte)'A';
            rawData[1] = (byte)'B';
            rawData[2] = (byte)'C';

            var ex = Should.Throw<InvalidDataException>(() => new QRCodeData(rawData, QRCodeData.Compression.Uncompressed));
            ex.Message.ShouldContain("QRR");
        }

        [Fact]
        [Category("QRCodeData/Validation")]
        public void side_length_below_minimum_throws()
        {
            // 28 is below the minimum of 29 (version 1 with quiet zone).
            var rawData = CraftRawData(28);

            Should.Throw<ArgumentOutOfRangeException>(() => new QRCodeData(rawData, QRCodeData.Compression.Uncompressed));
        }

        [Theory]
        [Category("QRCodeData/Validation")]
        [InlineData(30)]
        [InlineData(31)]
        [InlineData(32)]
        public void side_length_not_congruent_with_qr_size_throws(int sideLen)
        {
            // Valid serialized side lengths are 29 + 4 * (version - 1); anything
            // in between is corrupt data.
            var rawData = CraftRawData(sideLen);

            var ex = Should.Throw<InvalidDataException>(() => new QRCodeData(rawData, QRCodeData.Compression.Uncompressed));
            ex.Message.ShouldContain("Not a valid QR code size");
        }

        [Theory]
        [Category("QRCodeData/Validation")]
        [InlineData(29, 1)]
        [InlineData(33, 2)]
        public void valid_side_length_is_accepted(int sideLen, int expectedVersion)
        {
            var rawData = CraftRawData(sideLen);

            var qrData = new QRCodeData(rawData, QRCodeData.Compression.Uncompressed);

            qrData.Version.ShouldBe(expectedVersion);
            qrData.ModuleMatrix.Count.ShouldBe(sideLen);
            qrData.ModuleMatrix[0].Count.ShouldBe(sideLen);
        }

        [Fact]
        [Category("QRCodeData/Validation")]
        public void side_length_above_maximum_throws()
        {
            // 186 is above the maximum of 185 (version 40 with quiet zone). The
            // side length is validated before the payload, so a bare header suffices.
            var rawData = new byte[] { 0x51, 0x52, 0x52, 0x00, 186 };

            Should.Throw<ArgumentOutOfRangeException>(() => new QRCodeData(rawData, QRCodeData.Compression.Uncompressed));
        }

        [Fact]
        [Category("QRCodeData/Validation")]
        public void insufficient_data_bits_throws()
        {
            // A side length of 29 needs (29 * 29 + 7) / 8 = 106 payload bytes; supply fewer.
            var rawData = CraftRawData(29, payloadLength: 100);

            var ex = Should.Throw<InvalidDataException>(() => new QRCodeData(rawData, QRCodeData.Compression.Uncompressed));
            ex.Message.ShouldContain("Insufficient data");
        }

        [Fact]
        [Category("QRCodeData/Validation")]
        public void decompression_bomb_throws()
        {
            // ~11 MB of zeroes gzip-compresses to a few KB but inflates past the
            // 10 MB decompression limit. The whole gzip stream is the raw data.
            var elevenMegabytesOfZeroes = new byte[11 * 1024 * 1024];
            byte[] bomb;
            using (var output = new MemoryStream())
            {
                using (var gzipStream = new GZipStream(output, CompressionMode.Compress, true))
                {
                    gzipStream.Write(elevenMegabytesOfZeroes, 0, elevenMegabytesOfZeroes.Length);
                }
                bomb = output.ToArray();
            }

            var ex = Should.Throw<InvalidDataException>(() => new QRCodeData(bomb, QRCodeData.Compression.GZip));
            ex.Message.ShouldContain("maximum allowed size");
        }

        // -------------------------------------------------------------------
        // Helpers
        // -------------------------------------------------------------------

        /// <summary>
        /// Builds raw QR data: 'QRR', 0x00, sideLen header followed by a zeroed
        /// payload of (sideLen * sideLen + 7) / 8 bytes unless overridden.
        /// </summary>
        private static byte[] CraftRawData(int sideLen, int? payloadLength = null)
        {
            var payloadBytes = payloadLength ?? (sideLen * sideLen + 7) / 8;
            var rawData = new byte[5 + payloadBytes];
            rawData[0] = 0x51; // 'Q'
            rawData[1] = 0x52; // 'R'
            rawData[2] = 0x52; // 'R'
            rawData[3] = 0x00;
            rawData[4] = (byte)sideLen;
            return rawData;
        }

        private static string MatrixToString(QRCodeData qrData)
        {
            var sb = new StringBuilder();
            foreach (var row in qrData.ModuleMatrix)
            {
                foreach (bool module in row)
                    sb.Append(module ? '1' : '0');
            }
            return sb.ToString();
        }
    }
}
