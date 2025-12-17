using System.Globalization;
using System.Text;

namespace Steeltype.QRCoderLite
{
    /// <summary>
    /// Generates QR codes as PDF byte arrays without any external dependencies.
    /// The PDF contains vector graphics that scale perfectly at any size.
    /// </summary>
    public sealed class PdfByteQRCode : AbstractQRCode, IDisposable
    {
        // Binary comment to ensure PDF is treated as binary (prevents text mode corruption)
        private static readonly byte[] PdfBinaryComment = { 0x25, 0xe2, 0xe3, 0xcf, 0xd3 };

        /// <summary>
        /// Maximum pixels per module for PDF output.
        /// </summary>
        private const int MaxPixelsPerModule = 1000;

        public PdfByteQRCode() { }

        public PdfByteQRCode(QRCodeData data) : base(data) { }

        private static void ValidatePixelsPerModule(int pixelsPerModule)
        {
            if (pixelsPerModule < 1)
                throw new ArgumentOutOfRangeException(nameof(pixelsPerModule), "Pixels per module must be at least 1.");
            if (pixelsPerModule > MaxPixelsPerModule)
                throw new ArgumentOutOfRangeException(nameof(pixelsPerModule), $"Pixels per module cannot exceed {MaxPixelsPerModule}.");
        }

        /// <summary>
        /// Creates a PDF document with a black and white QR code.
        /// </summary>
        public byte[] GetGraphic(int pixelsPerModule)
            => GetGraphic(pixelsPerModule, "#000000", "#ffffff");

        /// <summary>
        /// Creates a PDF document with the QR code in specified colors.
        /// </summary>
        /// <param name="pixelsPerModule">Size of each module in pixels.</param>
        /// <param name="darkColorHex">Dark module color in hex format (e.g., "#000000").</param>
        /// <param name="lightColorHex">Light module color in hex format (e.g., "#ffffff").</param>
        /// <param name="dpi">DPI for the PDF (default 150).</param>
        public byte[] GetGraphic(int pixelsPerModule, string darkColorHex, string lightColorHex, int dpi = 150)
        {
            ValidatePixelsPerModule(pixelsPerModule);
            if (dpi < 1 || dpi > 2400)
                throw new ArgumentOutOfRangeException(nameof(dpi), "DPI must be between 1 and 2400.");

            var moduleCount = QrCodeData.ModuleMatrix.Count;
            var imgSize = moduleCount * pixelsPerModule;
            var pdfMediaSize = FloatToStr(imgSize * 72f / dpi);

            // Parse colors to PDF RGB format (0.0 to 1.0)
            var darkColor = Utilities.HexColorToByteArray(darkColorHex);
            var lightColor = Utilities.HexColorToByteArray(lightColorHex);
            var darkColorPdf = ToPdfRgb(darkColor);
            var lightColorPdf = ToPdfRgb(lightColor);

            using var stream = new MemoryStream();
            using var writer = new StreamWriter(stream, Encoding.ASCII, 1024, leaveOpen: true);

            var xrefs = new List<long>();

            // PDF header
            writer.Write("%PDF-1.5\r\n");
            writer.Flush();

            // Binary comment
            stream.Write(PdfBinaryComment, 0, PdfBinaryComment.Length);
            writer.WriteLine();
            writer.Flush();

            // Object 1: Catalog
            xrefs.Add(stream.Position);
            writer.Write($"{xrefs.Count} 0 obj\r\n<<\r\n/Type /Catalog\r\n/Pages 2 0 R\r\n>>\r\nendobj\r\n");
            writer.Flush();

            // Object 2: Pages
            xrefs.Add(stream.Position);
            writer.Write($"{xrefs.Count} 0 obj\r\n<<\r\n/Count 1\r\n/Kids [ <<\r\n");
            writer.Write($"/Type /Page\r\n/Parent 2 0 R\r\n");
            writer.Write($"/MediaBox [0 0 {pdfMediaSize} {pdfMediaSize}]\r\n");
            writer.Write("/Resources << /ProcSet [ /PDF ] >>\r\n");
            writer.Write("/Contents 3 0 R\r\n>> ]\r\n>>\r\nendobj\r\n");
            writer.Flush();

            // Build content stream
            var scale = FloatToStr(imgSize * 72f / dpi / moduleCount);
            var pathCommands = BuildModulePath();

            var content = new StringBuilder();
            content.Append("q\r\n"); // Save graphics state
            content.Append($"{scale} 0 0 -{scale} 0 {pdfMediaSize} cm\r\n"); // Transform matrix
            content.Append($"{lightColorPdf} rg\r\n"); // Light color fill
            content.Append($"0 0 {moduleCount} {moduleCount} re\r\n"); // Background rectangle
            content.Append("f\r\n"); // Fill background
            content.Append($"{darkColorPdf} rg\r\n"); // Dark color fill
            content.Append(pathCommands); // Dark module rectangles
            content.Append("f*\r\n"); // Fill with even-odd rule
            content.Append("Q"); // Restore graphics state

            var contentStr = content.ToString();

            // Object 3: Content stream
            xrefs.Add(stream.Position);
            writer.Write($"{xrefs.Count} 0 obj\r\n<< /Length {contentStr.Length} >>\r\nstream\r\n");
            writer.Write(contentStr);
            writer.Write("endstream\r\nendobj\r\n");
            writer.Flush();

            var startxref = (int)stream.Position;

            // Cross-reference table
            writer.Write($"xref\r\n0 {xrefs.Count + 1}\r\n");
            writer.Write("0000000000 65535 f\r\n");
            foreach (var offset in xrefs)
            {
                writer.Write($"{(int)offset:D10} 00000 n\r\n");
            }

            // Trailer
            writer.Write($"trailer\r\n<<\r\n/Size {xrefs.Count + 1}\r\n/Root 1 0 R\r\n>>\r\n");
            writer.Write($"startxref\r\n{startxref}\r\n%%EOF");
            writer.Flush();

            return stream.ToArray();
        }

        /// <summary>
        /// Creates PDF path commands for all dark modules using run-length encoding.
        /// </summary>
        private string BuildModulePath()
        {
            var path = new StringBuilder();
            var matrix = QrCodeData.ModuleMatrix;
            var size = matrix.Count;

            for (int y = 0; y < size; y++)
            {
                int x = 0;
                while (x < size)
                {
                    if (!matrix[y][x])
                    {
                        x++;
                        continue;
                    }

                    // Found dark module - find run length
                    int startX = x;
                    while (x < size && matrix[y][x])
                        x++;

                    // Single rectangle for the run: x y width height re
                    path.Append(startX.ToString(CultureInfo.InvariantCulture));
                    path.Append(' ');
                    path.Append(y.ToString(CultureInfo.InvariantCulture));
                    path.Append(' ');
                    path.Append((x - startX).ToString(CultureInfo.InvariantCulture));
                    path.Append(" 1 re\r\n");
                }
            }

            return path.ToString();
        }

        private static string ToPdfRgb(byte[] rgb)
        {
            const float inv255 = 1f / 255f;
            return $"{FloatToStr(rgb[0] * inv255)} {FloatToStr(rgb[1] * inv255)} {FloatToStr(rgb[2] * inv255)}";
        }

        private static string FloatToStr(float value)
            => value.ToString("0.######", CultureInfo.InvariantCulture);
    }

    public static class PdfByteQRCodeHelper
    {
        public static byte[] GetQRCode(string plainText, int pixelsPerModule, string darkColorHex, string lightColorHex, QRCodeGenerator.ECCLevel eccLevel, bool forceUtf8 = false, bool utf8BOM = false, QRCodeGenerator.EciMode eciMode = QRCodeGenerator.EciMode.Default, int requestedVersion = -1)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(plainText, eccLevel, forceUtf8, utf8BOM, eciMode, requestedVersion);
            using var qrCode = new PdfByteQRCode(qrCodeData);
            return qrCode.GetGraphic(pixelsPerModule, darkColorHex, lightColorHex);
        }
    }
}
