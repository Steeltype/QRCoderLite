namespace Steeltype.QRCoderLite
{
    public class Base64QRCode : AbstractQRCode, IDisposable
    {
        private readonly PngByteQRCode qr;

        /// <summary>
        /// Constructor without params to be used in COM Objects connections
        /// </summary>
        public Base64QRCode()
        {
            qr = new PngByteQRCode();
        }

        public Base64QRCode(QRCodeData data) : base(data)
        {
            qr = new PngByteQRCode(data);
        }

        public override void SetQRCodeData(QRCodeData data)
        {
            qr.SetQRCodeData(data);
        }

        /// <summary>
        /// Returns the QR code as a base64-encoded PNG string (black and white).
        /// </summary>
        public string GetGraphic(int pixelsPerModule, bool drawQuietZones = true)
        {
            return Convert.ToBase64String(qr.GetGraphic(pixelsPerModule, drawQuietZones));
        }

        /// <summary>
        /// Returns the QR code as a base64-encoded PNG string with custom colors.
        /// Colors are specified as HTML hex strings (e.g., "#000000" for black).
        /// </summary>
        public string GetGraphic(int pixelsPerModule, string darkColorHtmlHex, string lightColorHtmlHex, bool drawQuietZones = true)
        {
            var darkColor = Utilities.HexColorToByteArray(darkColorHtmlHex);
            var lightColor = Utilities.HexColorToByteArray(lightColorHtmlHex);
            return Convert.ToBase64String(qr.GetGraphic(pixelsPerModule, darkColor, lightColor, drawQuietZones));
        }

        /// <summary>
        /// Returns the QR code as a base64-encoded PNG string with custom RGBA colors.
        /// </summary>
        public string GetGraphic(int pixelsPerModule, byte[] darkColorRgba, byte[] lightColorRgba, bool drawQuietZones = true)
        {
            return Convert.ToBase64String(qr.GetGraphic(pixelsPerModule, darkColorRgba, lightColorRgba, drawQuietZones));
        }
    }

    public static class Base64QRCodeHelper
    {
        public static string GetQRCode(string plainText, int pixelsPerModule, string darkColorHtmlHex, string lightColorHtmlHex, QRCodeGenerator.ECCLevel eccLevel, bool forceUtf8 = false, bool utf8BOM = false, QRCodeGenerator.EciMode eciMode = QRCodeGenerator.EciMode.Default, int requestedVersion = -1, bool drawQuietZones = true)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(plainText, eccLevel, forceUtf8, utf8BOM, eciMode, requestedVersion);
            using var qrCode = new Base64QRCode(qrCodeData);
            return qrCode.GetGraphic(pixelsPerModule, darkColorHtmlHex, lightColorHtmlHex, drawQuietZones);
        }
    }
}
