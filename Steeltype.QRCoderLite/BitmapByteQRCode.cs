namespace Steeltype.QRCoderLite
{
    /// <summary>
    /// Generates QR codes as BMP (bitmap) byte arrays without any external dependencies.
    /// </summary>
    public sealed class BitmapByteQRCode : AbstractQRCode, IDisposable
    {
        // BMP file header constants
        private static readonly byte[] BmpSignature = { 0x42, 0x4D }; // "BM"
        private static readonly byte[] BmpHeaderPart2 = { 0x00, 0x00, 0x00, 0x00, 0x36, 0x00, 0x00, 0x00, 0x28, 0x00, 0x00, 0x00 };
        private static readonly byte[] BmpHeaderEnd = { 0x01, 0x00, 0x18, 0x00 }; // 1 plane, 24-bit color

        /// <summary>
        /// Maximum pixels per module to prevent memory exhaustion (limits output to ~100 MB).
        /// </summary>
        private const int MaxPixelsPerModule = 100;

        public BitmapByteQRCode() { }

        public BitmapByteQRCode(QRCodeData data) : base(data) { }

        /// <summary>
        /// Returns the QR code as a BMP byte array (black and white).
        /// </summary>
        public byte[] GetGraphic(int pixelsPerModule)
            => GetGraphic(pixelsPerModule, new byte[] { 0x00, 0x00, 0x00 }, new byte[] { 0xFF, 0xFF, 0xFF });

        /// <summary>
        /// Returns the QR code as a BMP byte array with custom colors (HTML hex format).
        /// </summary>
        public byte[] GetGraphic(int pixelsPerModule, string darkColorHtmlHex, string lightColorHtmlHex)
            => GetGraphic(pixelsPerModule, Utilities.HexColorToByteArray(darkColorHtmlHex), Utilities.HexColorToByteArray(lightColorHtmlHex));

        /// <summary>
        /// Returns the QR code as a BMP byte array with custom RGB colors.
        /// </summary>
        public byte[] GetGraphic(int pixelsPerModule, byte[] darkColorRgb, byte[] lightColorRgb)
        {
            if (pixelsPerModule < 1)
                throw new ArgumentOutOfRangeException(nameof(pixelsPerModule), "Pixels per module must be at least 1.");
            if (pixelsPerModule > MaxPixelsPerModule)
                throw new ArgumentOutOfRangeException(nameof(pixelsPerModule), $"Pixels per module cannot exceed {MaxPixelsPerModule}.");

            var sideLength = QrCodeData.ModuleMatrix.Count * pixelsPerModule;

            // Pre-calculate color bytes for a full module width (BGR format for BMP)
            var moduleDark = new byte[pixelsPerModule * 3];
            var moduleLight = new byte[pixelsPerModule * 3];
            for (int i = 0; i < pixelsPerModule * 3; i += 3)
            {
                moduleDark[i] = darkColorRgb[2];     // Blue
                moduleDark[i + 1] = darkColorRgb[1]; // Green
                moduleDark[i + 2] = darkColorRgb[0]; // Red
                moduleLight[i] = lightColorRgb[2];
                moduleLight[i + 1] = lightColorRgb[1];
                moduleLight[i + 2] = lightColorRgb[0];
            }

            // BMP rows must be padded to 4-byte boundaries
            var rowPadding = sideLength % 4;

            // Calculate file size: 54-byte header + pixel data + row padding
            var fileSize = 54 + (3 * sideLength * sideLength) + (sideLength * rowPadding);

            var bmp = new byte[fileSize];
            var ix = 0;

            // Write BMP signature "BM"
            Array.Copy(BmpSignature, 0, bmp, ix, BmpSignature.Length);
            ix += BmpSignature.Length;

            // Write file size
            WriteInt32(bmp, ix, fileSize);
            ix += 4;

            // Write header part 2 (reserved bytes + data offset + DIB header size)
            Array.Copy(BmpHeaderPart2, 0, bmp, ix, BmpHeaderPart2.Length);
            ix += BmpHeaderPart2.Length;

            // Write width and height
            WriteInt32(bmp, ix, sideLength);
            ix += 4;
            WriteInt32(bmp, ix, sideLength);
            ix += 4;

            // Write header end (planes + bit depth)
            Array.Copy(BmpHeaderEnd, 0, bmp, ix, BmpHeaderEnd.Length);
            ix += BmpHeaderEnd.Length;

            // Skip remaining header bytes (compression, size, resolution, colors) - all zeros
            ix += 24;

            // Draw QR code (BMP stores rows bottom-to-top)
            for (var row = sideLength - 1; row >= 0; row -= pixelsPerModule)
            {
                var moduleRow = (row + pixelsPerModule) / pixelsPerModule - 1;

                // Write first pixel row of this module row
                var rowStart = ix;
                for (var col = 0; col < sideLength; col += pixelsPerModule)
                {
                    var moduleCol = (col + pixelsPerModule) / pixelsPerModule - 1;
                    var isDark = QrCodeData.ModuleMatrix[moduleRow][moduleCol];
                    Array.Copy(isDark ? moduleDark : moduleLight, 0, bmp, ix, moduleDark.Length);
                    ix += moduleDark.Length;
                }

                // Add row padding
                ix += rowPadding;
                var rowLength = ix - rowStart;

                // Copy the row for remaining pixel rows in this module
                for (var repeat = 1; repeat < pixelsPerModule; repeat++)
                {
                    Array.Copy(bmp, rowStart, bmp, ix, rowLength);
                    ix += rowLength;
                }
            }

            return bmp;
        }

        private static void WriteInt32(byte[] buffer, int offset, int value)
        {
            buffer[offset] = (byte)value;
            buffer[offset + 1] = (byte)(value >> 8);
            buffer[offset + 2] = (byte)(value >> 16);
            buffer[offset + 3] = (byte)(value >> 24);
        }
    }

    public static class BitmapByteQRCodeHelper
    {
        public static byte[] GetQRCode(string plainText, int pixelsPerModule, byte[] darkColorRgb, byte[] lightColorRgb, QRCodeGenerator.ECCLevel eccLevel, bool forceUtf8 = false, bool utf8BOM = false, QRCodeGenerator.EciMode eciMode = QRCodeGenerator.EciMode.Default, int requestedVersion = -1)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(plainText, eccLevel, forceUtf8, utf8BOM, eciMode, requestedVersion);
            using var qrCode = new BitmapByteQRCode(qrCodeData);
            return qrCode.GetGraphic(pixelsPerModule, darkColorRgb, lightColorRgb);
        }

        public static byte[] GetQRCode(string plainText, int pixelsPerModule, string darkColorHtmlHex, string lightColorHtmlHex, QRCodeGenerator.ECCLevel eccLevel, bool forceUtf8 = false, bool utf8BOM = false, QRCodeGenerator.EciMode eciMode = QRCodeGenerator.EciMode.Default, int requestedVersion = -1)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(plainText, eccLevel, forceUtf8, utf8BOM, eciMode, requestedVersion);
            using var qrCode = new BitmapByteQRCode(qrCodeData);
            return qrCode.GetGraphic(pixelsPerModule, darkColorHtmlHex, lightColorHtmlHex);
        }
    }
}
