using Shouldly;
using System.ComponentModel;
using System.Text;
using Xunit;

namespace Steeltype.QRCoderLite.Tests
{
    /// <summary>
    /// Covers the public CreateQrCode API surface of QRCodeGenerator: the byte-array overload,
    /// payload plumbing (Version/EccLevel/EciMode), UTF-8 and UTF-8-BOM encoding paths,
    /// capacity overflow errors, fixed-version behavior, and regression tests for the
    /// explicit-ECI encoding and ECI header capacity-reservation fixes.
    /// </summary>
    public class QRCodeGeneratorApiTests
    {
        private static string MatrixToString(QRCodeData data)
        {
            return string.Join("", data.ModuleMatrix.Select(x => x.ToBitString()).ToArray());
        }

        [Fact]
        [Category("QRGenerator/ByteArray")]
        public void can_generate_from_bytes()
        {
            byte[] testData = { 49, 50, 51, 65, 66, 67 }; // "123ABC" as ASCII bytes
            var gen = new QRCodeGenerator();
            var qrData = gen.CreateQrCode(testData, QRCodeGenerator.ECCLevel.L);
            qrData.Version.ShouldBe(1);
            var result = MatrixToString(qrData);
            result.ShouldBe("0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000001111111011001011111110000000010000010010010100000100000000101110101010101011101000000001011101010010010111010000000010111010111000101110100000000100000100000001000001000000001111111010101011111110000000000000000011000000000000000000111100101010010011101000000001011100001001001001110000000010101011111011111110100000000000101000000110000000000000001011001001010100110000000000000000000110001000101000000000111111100110011011110000000001000001001111110111010000000010111010011100100101100000000101110101110010010010000000001011101011010100011000000000010000010110110101000100000000111111101011100010000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000");
        }

        [Fact]
        [Category("QRGenerator/ByteArray")]
        public void byte_array_overload_matches_forced_utf8_string_encoding()
        {
            // forceUtf8 with default ECI mode takes the plain Byte-mode path with UTF-8 bytes
            // and no ECI header, which is bit-for-bit what the byte-array overload produces.
            var gen = new QRCodeGenerator();
            var fromString = gen.CreateQrCode("äöü", QRCodeGenerator.ECCLevel.L, forceUtf8: true);
            var fromBytes = gen.CreateQrCode(Encoding.UTF8.GetBytes("äöü"), QRCodeGenerator.ECCLevel.L);
            MatrixToString(fromString).ShouldBe(MatrixToString(fromBytes));
        }

        [Fact]
        [Category("QRGenerator/TextEncoding")]
        public void utf8_bom_prepends_utf8_preamble()
        {
            var gen = new QRCodeGenerator();
            var withBom = gen.CreateQrCode("äöü", QRCodeGenerator.ECCLevel.L, forceUtf8: true, utf8BOM: true);
            var withoutBom = gen.CreateQrCode("äöü", QRCodeGenerator.ECCLevel.L, forceUtf8: true);
            var bomBytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes("äöü")).ToArray();
            var fromBomBytes = gen.CreateQrCode(bomBytes, QRCodeGenerator.ECCLevel.L);

            MatrixToString(withBom).ShouldBe(MatrixToString(fromBomBytes));
            MatrixToString(withBom).ShouldNotBe(MatrixToString(withoutBom));
        }

        [Fact]
        [Category("QRGenerator/TextEncoding")]
        public void iso_fast_path_ignores_bom_flag()
        {
            // Without forceUtf8 or an explicit ECI mode, ISO-8859-1-representable text takes
            // the ISO fast path, where the UTF-8 BOM flag has no meaning and must not leak in.
            var gen = new QRCodeGenerator();
            var withBomFlag = gen.CreateQrCode("äöü", QRCodeGenerator.ECCLevel.L, false, true);
            var plain = gen.CreateQrCode("äöü", QRCodeGenerator.ECCLevel.L);
            MatrixToString(withBomFlag).ShouldBe(MatrixToString(plain));
        }

        [Fact]
        [Category("QRGenerator/TextEncoding")]
        public void can_encode_utf8()
        {
            var gen = new QRCodeGenerator();
            var qrData = gen.CreateQrCode("https://en.wikipedia.org/wiki/🍕", QRCodeGenerator.ECCLevel.L, true, false, QRCodeGenerator.EciMode.Utf8);
            var result = MatrixToString(qrData);
            result.ShouldBe("0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000011111110101011010011101111111000000001000001001111100001110100000100000000101110101110000011000010111010000000010111010111010111100101011101000000001011101010011010111010101110100000000100000100011010001110010000010000000011111110101010101010101111111000000000000000000101000011100000000000000000111100101011110101011100111010000000001011000101011111010011101010000000001010011101111101001111011101000000000111011011110000010001100000100000000000000010011010101100000000000000000001100110101011011111001101110000000000000011100001010101010110101000000000000111001011100110111111110011000000001110101011001011001000100011000000000000101010100001010111111000000000000010111010101001111100000001110000000000010110100010111111100100010100000000011101111010011101111111101010000000000000000110000001000100010010000000001111111001100011001010101101000000000100000100111111111011000111000000000010111010010100011010111110111000000001011101010110100011100101011000000000101110101100101111100101111010000000010000010111011001111000001101000000001111111011110000100000110101000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000");
        }

        [Fact]
        [Category("QRGenerator/TextEncoding")]
        public void can_encode_utf8_bom()
        {
            var gen = new QRCodeGenerator();
            var qrData = gen.CreateQrCode("https://en.wikipedia.org/wiki/🍕", QRCodeGenerator.ECCLevel.L, true, true, QRCodeGenerator.EciMode.Utf8);
            var result = MatrixToString(qrData);
            result.ShouldBe("0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000011111110010001101010101111111000000001000001011011000110000100000100000000101110100111010101111010111010000000010111010110100100010101011101000000001011101000101111000010101110100000000100000101010000111000010000010000000011111110101010101010101111111000000000000000000001010101110000000000000000111110111110101010100101010100000000000100000110000101000001100101000000000001001001011000011010000111100000000100010001111000001111110111010000000010110111010100011100100101111000000000001010001101101001000010100100000000100001101110011001010000001010000000001011001100011001111111010111000000000010001010101011110010100000100000000100100010000000000010110010000000000010110110010110000101010101100000000001001100100010010100111101101100000000101010110011000111101111100100000000000000000111011110011100011010000000001111111011100110010010101110000000000100000100100110010101000110110000000010111010110010111101111110011000000001011101010100000100010110100000000000101110101001100111110110111100000000010000010111100101111100100001000000001111111011110001110100111000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000");
        }

        [Fact]
        [Category("QRGenerator/Eci")]
        public void explicit_eci_utf8_produces_different_matrix_than_default()
        {
            // Sanity check: an explicit UTF-8 ECI request must differ from the default ISO path
            // (ECI header + UTF-8 bytes vs no header + ISO bytes). NOTE this test alone does NOT
            // lock the fast-path fix — the pre-fix encoder also differed here because it already
            // wrote the ECI header (over wrong ISO bytes). The actual regression lock is the
            // sibling test explicit_eci_utf8_matches_forced_utf8_with_same_eci below.
            var gen = new QRCodeGenerator();
            var defaultMode = gen.CreateQrCode("café", QRCodeGenerator.ECCLevel.L, eciMode: QRCodeGenerator.EciMode.Default);
            var explicitUtf8 = gen.CreateQrCode("café", QRCodeGenerator.ECCLevel.L, eciMode: QRCodeGenerator.EciMode.Utf8);
            MatrixToString(explicitUtf8).ShouldNotBe(MatrixToString(defaultMode));
        }

        [Fact]
        [Category("QRGenerator/Eci")]
        public void explicit_eci_utf8_matches_forced_utf8_with_same_eci()
        {
            // With the fix, an explicit UTF-8 ECI mode takes the UTF-8 encoding path regardless
            // of whether forceUtf8 is set, so both calls must produce identical modules.
            var gen = new QRCodeGenerator();
            var explicitUtf8 = gen.CreateQrCode("café", QRCodeGenerator.ECCLevel.L, eciMode: QRCodeGenerator.EciMode.Utf8);
            var forcedUtf8 = gen.CreateQrCode("café", QRCodeGenerator.ECCLevel.L, forceUtf8: true, eciMode: QRCodeGenerator.EciMode.Utf8);
            MatrixToString(explicitUtf8).ShouldBe(MatrixToString(forcedUtf8));
        }

        [Fact]
        [Category("QRGenerator/Eci")]
        public void eci_header_reservation_pushes_41_digits_to_version_2()
        {
            // 41 digits exceed version 1-L numeric capacity (41) once any ECI reservation is
            // added, so version 2 must be selected. (Both the flat-2 and mode-correct reservation
            // agree here; the true regression lock is the 38/39-digit window test below.)
            var gen = new QRCodeGenerator();
            var qrData = gen.CreateQrCode(new string('7', 41), QRCodeGenerator.ECCLevel.L, eciMode: QRCodeGenerator.EciMode.Utf8);
            qrData.Version.ShouldBe(2);
        }

        [Theory]
        [InlineData(38)]
        [InlineData(39)]
        [Category("QRGenerator/Eci")]
        public void eci_header_reservation_pushes_38_and_39_digits_to_version_2(int digitCount)
        {
            // REGRESSION LOCK for the flat-2-character ECI reservation bug: version selection
            // used to reserve 2 'characters' for the 12-bit ECI header, but 2 numeric chars are
            // only ~6.7 bits. For 38-39 digits the buggy math still chose version 1
            // (38+2=40 <= 41) while the actual encoded stream (mode 4 + ECI 12 + count 10 +
            // data 127/130 bits = 153/156) exceeds the 152-bit v1-L capacity — silently
            // truncating data bits into an undecodable QR. The mode-correct 4-digit reservation
            // (38+4=42 > 41) must select version 2. This Theory FAILS against the pre-fix code.
            var gen = new QRCodeGenerator();
            var qrData = gen.CreateQrCode(new string('7', digitCount), QRCodeGenerator.ECCLevel.L, eciMode: QRCodeGenerator.EciMode.Utf8);
            qrData.Version.ShouldBe(2);
        }

        [Fact]
        [Category("QRGenerator/Eci")]
        public void eci_header_reservation_still_fits_37_digits_in_version_1()
        {
            // 37 digits + 4-digit ECI header reservation = 41, exactly the version 1-L numeric
            // capacity, so version 1 must still be selected (the reservation is not over-broad).
            var gen = new QRCodeGenerator();
            var qrData = gen.CreateQrCode(new string('7', 37), QRCodeGenerator.ECCLevel.L, eciMode: QRCodeGenerator.EciMode.Utf8);
            qrData.Version.ShouldBe(1);
        }

        [Fact]
        [Category("QRGenerator/Eci")]
        public void numeric_41_digits_without_eci_fit_version_1()
        {
            // Baseline for the ECI reservation tests: without an ECI header, 41 digits are
            // exactly the version 1-L numeric capacity.
            var gen = new QRCodeGenerator();
            var qrData = gen.CreateQrCode(new string('7', 41), QRCodeGenerator.ECCLevel.L);
            qrData.Version.ShouldBe(1);
        }

        [Fact]
        [Category("QRGenerator/Capacity")]
        public void data_too_long_throws_for_auto_version()
        {
            // Byte capacity at version 40-L is 2953 bytes; one more must throw.
            var gen = new QRCodeGenerator();
            Should.Throw<ArgumentOutOfRangeException>(() => gen.CreateQrCode(new byte[2954], QRCodeGenerator.ECCLevel.L));
        }

        [Fact]
        [Category("QRGenerator/Capacity")]
        public void max_capacity_byte_payload_fits_version_40()
        {
            var gen = new QRCodeGenerator();
            var qrData = gen.CreateQrCode(new byte[2953], QRCodeGenerator.ECCLevel.L);
            qrData.Version.ShouldBe(40);
        }

        [Fact]
        [Category("QRGenerator/Capacity")]
        public void data_too_long_throws_for_fixed_requested_version()
        {
            // Version 1 at ECC H holds only 17 bytes; this text cannot fit the requested version.
            var gen = new QRCodeGenerator();
            Should.Throw<ArgumentOutOfRangeException>(() => gen.CreateQrCode(
                "THIS TEXT IS DEFINITELY TOO LONG FOR VERSION 1 AT ECC LEVEL H",
                QRCodeGenerator.ECCLevel.H,
                requestedVersion: 1));
        }

        [Fact]
        [Category("QRGenerator/Capacity")]
        public void requested_version_is_respected()
        {
            // "ABC" alone needs only version 1; requesting version 7 must produce a version 7
            // code (45 modules per side plus 4 quiet-zone modules on each edge).
            var gen = new QRCodeGenerator();
            var qrData = gen.CreateQrCode("ABC", QRCodeGenerator.ECCLevel.M, requestedVersion: 7);
            qrData.Version.ShouldBe(7);
            qrData.ModuleMatrix.Count.ShouldBe(4 * 7 + 17 + 8);
        }

        [Fact]
        [Category("QRGenerator/EccLevel")]
        public void ecc_levels_m_and_h_generate_distinct_codes()
        {
            var gen = new QRCodeGenerator();
            var m = gen.CreateQrCode("ECC LEVEL TEST 123", QRCodeGenerator.ECCLevel.M);
            var h = gen.CreateQrCode("ECC LEVEL TEST 123", QRCodeGenerator.ECCLevel.H);

            // 18 alphanumeric chars fit version 1 at M (capacity 20) but need version 2 at H
            // (version 1-H alphanumeric capacity is 10), and the module output must differ.
            m.Version.ShouldBe(1);
            h.Version.ShouldBe(2);
            MatrixToString(m).ShouldNotBe(MatrixToString(h));
        }

        [Fact]
        [Category("QRGenerator/Payload")]
        public void payload_defaults_flow_through_to_generation()
        {
            // The Payload base class defaults to ECC level M, auto version, and default ECI mode.
            var payload = new PlainTextPayload("payload default test");
            var fromPayload = QRCodeGenerator.GenerateQrCode(payload);
            var expected = QRCodeGenerator.GenerateQrCode("payload default test", QRCodeGenerator.ECCLevel.M);
            MatrixToString(fromPayload).ShouldBe(MatrixToString(expected));
        }

        [Fact]
        [Category("QRGenerator/Payload")]
        public void payload_version_ecc_and_eci_reach_generation()
        {
            // A payload's Version, EccLevel, and EciMode overrides must all plumb through to
            // the string-based generation path.
            var payload = new CustomizedPayload("café payload", 4, QRCodeGenerator.ECCLevel.Q, QRCodeGenerator.EciMode.Utf8);
            var gen = new QRCodeGenerator();
            var fromPayload = gen.CreateQrCode(payload);
            var expected = gen.CreateQrCode("café payload", QRCodeGenerator.ECCLevel.Q, eciMode: QRCodeGenerator.EciMode.Utf8, requestedVersion: 4);

            fromPayload.Version.ShouldBe(4);
            MatrixToString(fromPayload).ShouldBe(MatrixToString(expected));
        }

        [Fact]
        [Category("QRGenerator/Payload")]
        public void payload_ecc_override_keeps_payload_version_and_eci()
        {
            // The (Payload, ECCLevel) overload replaces only the ECC level; the payload's
            // Version and EciMode must still be honored.
            var payload = new CustomizedPayload("café payload", 4, QRCodeGenerator.ECCLevel.Q, QRCodeGenerator.EciMode.Utf8);
            var gen = new QRCodeGenerator();
            var fromPayload = gen.CreateQrCode(payload, QRCodeGenerator.ECCLevel.H);
            var expected = gen.CreateQrCode("café payload", QRCodeGenerator.ECCLevel.H, eciMode: QRCodeGenerator.EciMode.Utf8, requestedVersion: 4);

            fromPayload.Version.ShouldBe(4);
            MatrixToString(fromPayload).ShouldBe(MatrixToString(expected));
        }

        /// <summary>
        /// Minimal payload that keeps every Payload base-class default (ECC M, auto version, default ECI).
        /// </summary>
        private sealed class PlainTextPayload : PayloadGenerator.Payload
        {
            private readonly string text;

            public PlainTextPayload(string text)
            {
                this.text = text;
            }

            public override string ToString()
            {
                return text;
            }
        }

        /// <summary>
        /// Payload that overrides Version, EccLevel, and EciMode to verify they reach generation.
        /// </summary>
        private sealed class CustomizedPayload : PayloadGenerator.Payload
        {
            private readonly string text;

            public CustomizedPayload(string text, int version, QRCodeGenerator.ECCLevel eccLevel, QRCodeGenerator.EciMode eciMode)
            {
                this.text = text;
                Version = version;
                EccLevel = eccLevel;
                EciMode = eciMode;
            }

            public override int Version { get; }
            public override QRCodeGenerator.ECCLevel EccLevel { get; }
            public override QRCodeGenerator.EciMode EciMode { get; }

            public override string ToString()
            {
                return text;
            }
        }
    }
}
