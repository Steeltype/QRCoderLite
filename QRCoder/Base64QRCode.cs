using SkiaSharp;

namespace Steeltype.QRCoderLite
{
    public class Base64QRCode : AbstractQRCode, IDisposable
    {
        private QRCode qr;

        /// <summary>
        /// Constructor without params to be used in COM Objects connections
        /// </summary>
        public Base64QRCode() {
            qr = new QRCode();
        }

        public Base64QRCode(QRCodeData data) : base(data) {
            qr = new QRCode(data);
        }

        public override void SetQRCodeData(QRCodeData data) {
            this.qr.SetQRCodeData(data);
        }

        public string GetGraphic(int pixelsPerModule)
        {
            return this.GetGraphic(pixelsPerModule, SKColors.Black, SKColors.White, true);
        }


        public string GetGraphic(int pixelsPerModule, string darkColorHtmlHex, string lightColorHtmlHex, bool drawQuietZones = true, SKEncodedImageFormat imgType = SKEncodedImageFormat.Png)
        {
            return this.GetGraphic(pixelsPerModule, SKColor.Parse(darkColorHtmlHex), SKColor.Parse(lightColorHtmlHex), drawQuietZones, imgType);
        }

        public string GetGraphic(int pixelsPerModule, SKColor darkColor, SKColor lightColor, bool drawQuietZones = true, SKEncodedImageFormat imgType = SKEncodedImageFormat.Png)
        {
            var base64 = string.Empty;
            using SKBitmap bmp = qr.GetGraphic(pixelsPerModule, darkColor, lightColor, drawQuietZones);
            base64 = BitmapToBase64(bmp, imgType);
            return base64;
        }

        public string GetGraphic(int pixelsPerModule, SKColor darkColor, SKColor lightColor, SKBitmap icon, int iconSizePercent = 15, int iconBorderWidth = 6, bool drawQuietZones = true, SKEncodedImageFormat imgType = SKEncodedImageFormat.Png)
        {
            var base64 = string.Empty;
            using SKBitmap bmp = qr.GetGraphic(pixelsPerModule, darkColor, lightColor, icon, iconSizePercent, iconBorderWidth, drawQuietZones);
            base64 = BitmapToBase64(bmp, imgType);
            return base64;
        }

        private string BitmapToBase64(SKBitmap bmp, SKEncodedImageFormat imgFormat)
        {
            using var image = SKImage.FromBitmap(bmp);
            using var data = image.Encode(imgFormat, 100);
            using MemoryStream memoryStream = new MemoryStream();
            // Write the image data to a memory stream
            data.SaveTo(memoryStream);

            return Convert.ToBase64String(memoryStream.ToArray());
        }
    }
    
    public static class Base64QRCodeHelper
    {
        public static string GetQRCode(string plainText, int pixelsPerModule, string darkColorHtmlHex, string lightColorHtmlHex, QRCodeGenerator.ECCLevel eccLevel, bool forceUtf8 = false, bool utf8BOM = false, QRCodeGenerator.EciMode eciMode = QRCodeGenerator.EciMode.Default, int requestedVersion = -1, bool drawQuietZones = true, SKEncodedImageFormat imgType = SKEncodedImageFormat.Png)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(plainText, eccLevel, forceUtf8, utf8BOM, eciMode, requestedVersion);
            using var qrCode = new Base64QRCode(qrCodeData);
            return qrCode.GetGraphic(pixelsPerModule, darkColorHtmlHex, lightColorHtmlHex, drawQuietZones, imgType);
        }
    }
}