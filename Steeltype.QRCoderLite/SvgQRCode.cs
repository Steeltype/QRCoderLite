using System.Globalization;
using System.Text;

namespace Steeltype.QRCoderLite
{
    /// <summary>
    /// Generates QR codes as SVG (Scalable Vector Graphics) strings.
    /// SVG output is ideal for web use as it scales to any size without quality loss.
    /// </summary>
    public sealed class SvgQRCode : AbstractQRCode, IDisposable
    {
        /// <summary>
        /// Maximum pixels per module for SVG output.
        /// </summary>
        private const int MaxPixelsPerModule = 1000;

        /// <summary>
        /// Maximum logo size in bytes (5 MB).
        /// </summary>
        private const int MaxLogoSizeBytes = 5 * 1024 * 1024;

        public SvgQRCode() { }

        public SvgQRCode(QRCodeData data) : base(data) { }

        private static void ValidatePixelsPerModule(int pixelsPerModule)
        {
            if (pixelsPerModule < 1)
                throw new ArgumentOutOfRangeException(nameof(pixelsPerModule), "Pixels per module must be at least 1.");
            if (pixelsPerModule > MaxPixelsPerModule)
                throw new ArgumentOutOfRangeException(nameof(pixelsPerModule), $"Pixels per module cannot exceed {MaxPixelsPerModule}.");
        }

        /// <summary>
        /// Returns a scalable black and white QR code as an SVG string.
        /// </summary>
        public string GetGraphic(int pixelsPerModule, bool drawQuietZones = true)
            => GetGraphic(pixelsPerModule, "#000000", "#ffffff", drawQuietZones);

        /// <summary>
        /// Returns a QR code as an SVG string with custom colors.
        /// </summary>
        /// <param name="pixelsPerModule">The size of each module in pixels (determines overall SVG size).</param>
        /// <param name="darkColorHex">Color for dark modules in hex format (e.g., "#000000").</param>
        /// <param name="lightColorHex">Color for light modules in hex format (e.g., "#ffffff").</param>
        /// <param name="drawQuietZones">Whether to include the quiet zone border.</param>
        /// <param name="sizingMode">How to specify the SVG size.</param>
        public string GetGraphic(int pixelsPerModule, string darkColorHex, string lightColorHex, bool drawQuietZones = true, SizingMode sizingMode = SizingMode.WidthHeightAttribute)
        {
            ValidatePixelsPerModule(pixelsPerModule);
            int offset = drawQuietZones ? 0 : 4;
            int moduleCount = QrCodeData.ModuleMatrix.Count - (drawQuietZones ? 0 : offset * 2);
            int size = moduleCount * pixelsPerModule;

            var svg = new StringBuilder();

            // SVG header
            svg.Append("<svg xmlns=\"http://www.w3.org/2000/svg\" version=\"1.1\" shape-rendering=\"crispEdges\"");
            svg.Append($" viewBox=\"0 0 {moduleCount} {moduleCount}\"");

            if (sizingMode == SizingMode.WidthHeightAttribute)
            {
                svg.Append($" width=\"{size}\" height=\"{size}\"");
            }
            svg.AppendLine(">");

            // Background
            if (!string.IsNullOrEmpty(lightColorHex) && lightColorHex.ToLower() != "transparent")
            {
                svg.AppendLine($"<rect x=\"0\" y=\"0\" width=\"{moduleCount}\" height=\"{moduleCount}\" fill=\"{lightColorHex}\"/>");
            }

            // Dark modules as a single path for efficiency
            svg.Append($"<path fill=\"{darkColorHex}\" d=\"");

            for (int y = 0; y < moduleCount; y++)
            {
                for (int x = 0; x < moduleCount; x++)
                {
                    if (QrCodeData.ModuleMatrix[y + offset][x + offset])
                    {
                        svg.Append($"M{x},{y}h1v1h-1z");
                    }
                }
            }

            svg.AppendLine("\"/>");
            svg.Append("</svg>");

            return svg.ToString();
        }

        /// <summary>
        /// Returns a QR code as an SVG string with an embedded logo.
        /// </summary>
        /// <param name="pixelsPerModule">The size of each module in pixels.</param>
        /// <param name="darkColorHex">Color for dark modules in hex format.</param>
        /// <param name="lightColorHex">Color for light modules in hex format.</param>
        /// <param name="logoBytes">PNG or other image bytes to embed as logo.</param>
        /// <param name="logoSizePercent">Size of logo as percentage of QR code (default 15%).</param>
        /// <param name="drawQuietZones">Whether to include the quiet zone border.</param>
        /// <param name="logoMimeType">MIME type of the logo (default "image/png").</param>
        public string GetGraphic(int pixelsPerModule, string darkColorHex, string lightColorHex, byte[] logoBytes, int logoSizePercent = 15, bool drawQuietZones = true, string logoMimeType = "image/png")
        {
            ValidatePixelsPerModule(pixelsPerModule);

            if (logoBytes == null || logoBytes.Length == 0)
                throw new ArgumentException("Logo bytes cannot be null or empty.", nameof(logoBytes));
            if (logoBytes.Length > MaxLogoSizeBytes)
                throw new ArgumentException($"Logo size exceeds maximum allowed size of {MaxLogoSizeBytes / (1024 * 1024)} MB.", nameof(logoBytes));
            if (logoSizePercent < 1 || logoSizePercent > 50)
                throw new ArgumentOutOfRangeException(nameof(logoSizePercent), "Logo size percent must be between 1 and 50.");

            int offset = drawQuietZones ? 0 : 4;
            int moduleCount = QrCodeData.ModuleMatrix.Count - (drawQuietZones ? 0 : offset * 2);
            int size = moduleCount * pixelsPerModule;

            // Calculate logo position and size
            float logoSize = moduleCount * logoSizePercent / 100f;
            float logoPos = (moduleCount - logoSize) / 2f;

            var svg = new StringBuilder();

            // SVG header with xlink for image support
            svg.Append("<svg xmlns=\"http://www.w3.org/2000/svg\" xmlns:xlink=\"http://www.w3.org/1999/xlink\" version=\"1.1\" shape-rendering=\"crispEdges\"");
            svg.Append($" viewBox=\"0 0 {moduleCount} {moduleCount}\"");
            svg.AppendLine($" width=\"{size}\" height=\"{size}\">");

            // Background
            if (!string.IsNullOrEmpty(lightColorHex) && lightColorHex.ToLower() != "transparent")
            {
                svg.AppendLine($"<rect x=\"0\" y=\"0\" width=\"{moduleCount}\" height=\"{moduleCount}\" fill=\"{lightColorHex}\"/>");
            }

            // Dark modules as path, skipping the logo area
            svg.Append($"<path fill=\"{darkColorHex}\" d=\"");

            for (int y = 0; y < moduleCount; y++)
            {
                for (int x = 0; x < moduleCount; x++)
                {
                    // Skip modules in the logo area
                    if (x >= logoPos && x < logoPos + logoSize && y >= logoPos && y < logoPos + logoSize)
                        continue;

                    if (QrCodeData.ModuleMatrix[y + offset][x + offset])
                    {
                        svg.Append($"M{x},{y}h1v1h-1z");
                    }
                }
            }

            svg.AppendLine("\"/>");

            // Logo background (white rectangle behind logo)
            svg.AppendLine($"<rect x=\"{F(logoPos)}\" y=\"{F(logoPos)}\" width=\"{F(logoSize)}\" height=\"{F(logoSize)}\" fill=\"{lightColorHex}\"/>");

            // Embedded logo image
            string logoBase64 = Convert.ToBase64String(logoBytes);
            svg.AppendLine($"<image x=\"{F(logoPos)}\" y=\"{F(logoPos)}\" width=\"{F(logoSize)}\" height=\"{F(logoSize)}\" xlink:href=\"data:{logoMimeType};base64,{logoBase64}\"/>");

            svg.Append("</svg>");

            return svg.ToString();
        }

        /// <summary>
        /// Format float for SVG (invariant culture, minimal precision).
        /// </summary>
        private static string F(float value) => value.ToString("0.##", CultureInfo.InvariantCulture);

        /// <summary>
        /// Defines how the SVG size is specified.
        /// </summary>
        public enum SizingMode
        {
            /// <summary>
            /// Include width and height attributes (fixed pixel size).
            /// </summary>
            WidthHeightAttribute = 0,

            /// <summary>
            /// Only use viewBox (SVG scales to fit container).
            /// </summary>
            ViewBoxAttribute = 1
        }
    }

    public static class SvgQRCodeHelper
    {
        public static string GetQRCode(string plainText, int pixelsPerModule, string darkColorHex, string lightColorHex, QRCodeGenerator.ECCLevel eccLevel, bool forceUtf8 = false, bool utf8BOM = false, QRCodeGenerator.EciMode eciMode = QRCodeGenerator.EciMode.Default, int requestedVersion = -1, bool drawQuietZones = true)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(plainText, eccLevel, forceUtf8, utf8BOM, eciMode, requestedVersion);
            using var qrCode = new SvgQRCode(qrCodeData);
            return qrCode.GetGraphic(pixelsPerModule, darkColorHex, lightColorHex, drawQuietZones);
        }
    }
}
