using SkiaSharp;

namespace Steeltype.QRCoderLite
{
    public class QRCode : AbstractQRCode, IDisposable
    {
        public QRCode()
        {
        }

        public QRCode(QRCodeData data) : base(data) { }

        public SKBitmap GetGraphic(int pixelsPerModule)
        {
            // Defaulting to black and white colors using SkiaSharp's color representation
            return GetGraphic(pixelsPerModule, SKColors.Black, SKColors.White, true);
        }

        public SKBitmap GetGraphic(int pixelsPerModule, string darkColorHtmlHex, string lightColorHtmlHex, bool drawQuietZones = true)
        {
            // Convert HTML hex color strings to SKColor
            SKColor.TryParse(darkColorHtmlHex, out var darkColor);
            SKColor.TryParse(lightColorHtmlHex, out var lightColor);

            // Call the main GetGraphic method with SKColor parameters
            return GetGraphic(pixelsPerModule, darkColor, lightColor, drawQuietZones);
        }

        public SKBitmap GetGraphic(int pixelsPerModule, SKColor darkColor, SKColor lightColor, bool drawQuietZones = true)
        {
            var size = (QrCodeData.ModuleMatrix.Count - (drawQuietZones ? 0 : 8)) * pixelsPerModule;
            var offset = drawQuietZones ? 0 : 4 * pixelsPerModule;

            var bmp = new SKBitmap(size, size);
            using var canvas = new SKCanvas(bmp);
            canvas.Clear(lightColor); // Set the background color

            var lightPaint = new SKPaint { Color = lightColor };
            var darkPaint = new SKPaint { Color = darkColor };

            for (var x = 0; x < size + offset; x += pixelsPerModule)
            {
                for (var y = 0; y < size + offset; y += pixelsPerModule)
                {
                    var module = QrCodeData.ModuleMatrix[(y + pixelsPerModule) / pixelsPerModule - 1][(x + pixelsPerModule) / pixelsPerModule - 1];
                    var rect = new SKRect(x - offset, y - offset, x - offset + pixelsPerModule, y - offset + pixelsPerModule);

                    canvas.DrawRect(rect, module ? darkPaint : lightPaint);
                }
            }

            return bmp;
        }

        public SKBitmap GetGraphic(int pixelsPerModule, SKColor darkColor, SKColor lightColor, SKBitmap icon = null, int iconSizePercent = 15, int iconBorderWidth = 0, bool drawQuietZones = true, SKColor? iconBackgroundColor = null)
        {
            var size = (QrCodeData.ModuleMatrix.Count - (drawQuietZones ? 0 : 8)) * pixelsPerModule;
            var offset = drawQuietZones ? 0 : 4 * pixelsPerModule;

            var bmp = new SKBitmap(size, size);
            using var canvas = new SKCanvas(bmp);
            canvas.Clear(lightColor);

            var lightPaint = new SKPaint { Color = lightColor };
            var darkPaint = new SKPaint { Color = darkColor };

            for (var x = 0; x < size + offset; x += pixelsPerModule)
            {
                for (var y = 0; y < size + offset; y += pixelsPerModule)
                {
                    var moduleColor = QrCodeData.ModuleMatrix[(y + pixelsPerModule) / pixelsPerModule - 1][(x + pixelsPerModule) / pixelsPerModule - 1] ? darkPaint : lightPaint;
                    canvas.DrawRect(x - offset, y - offset, pixelsPerModule, pixelsPerModule, moduleColor);
                }
            }

            // If the icon is null or the icon size is invalid, return the QR code without the icon
            if (icon == null || iconSizePercent <= 0 || iconSizePercent > 100) return bmp;

            // Calculate the icon's position and size
            var iconDestWidth = iconSizePercent * bmp.Width / 100f;
            var iconDestHeight = iconDestWidth * icon.Height / icon.Width;
            var iconX = (bmp.Width - iconDestWidth) / 2;
            var iconY = (bmp.Height - iconDestHeight) / 2;

            // Draw the icon background if iconBorderWidth and iconBackgroundColor are set
            if (iconBorderWidth > 0 && iconBackgroundColor.HasValue)
            {
                var centerDest = new SKRect(iconX - iconBorderWidth, iconY - iconBorderWidth, iconX + iconDestWidth + iconBorderWidth, iconY + iconDestHeight + iconBorderWidth);
                var iconBgPaint = new SKPaint { Color = iconBackgroundColor.Value, IsAntialias = true };
                var iconPath = CreateRoundedRectanglePath(centerDest, iconBorderWidth * 2);
                canvas.DrawPath(iconPath, iconBgPaint);
            }

            // Draw the icon
            var iconDestRect = new SKRect(iconX, iconY, iconX + iconDestWidth, iconY + iconDestHeight);
            canvas.DrawBitmap(icon, new SKRect(0, 0, icon.Width, icon.Height), iconDestRect, null);

            return bmp;
        }

        private SKPath CreateRoundedRectanglePath(SKRect rect, float cornerRadius)
        {
            var roundedRect = new SKPath();

            // Top left corner
            roundedRect.MoveTo(rect.Left + cornerRadius, rect.Top);
            roundedRect.ArcTo(cornerRadius, cornerRadius, 0, SKPathArcSize.Small, SKPathDirection.Clockwise, rect.Left, rect.Top + cornerRadius);

            // Line to bottom left corner
            roundedRect.LineTo(rect.Left, rect.Bottom - cornerRadius);

            // Bottom left corner
            roundedRect.ArcTo(cornerRadius, cornerRadius, 0, SKPathArcSize.Small, SKPathDirection.Clockwise, rect.Left + cornerRadius, rect.Bottom);

            // Line to bottom right corner
            roundedRect.LineTo(rect.Right - cornerRadius, rect.Bottom);

            // Bottom right corner
            roundedRect.ArcTo(cornerRadius, cornerRadius, 0, SKPathArcSize.Small, SKPathDirection.Clockwise, rect.Right, rect.Bottom - cornerRadius);

            // Line to top right corner
            roundedRect.LineTo(rect.Right, rect.Top + cornerRadius);

            // Top right corner
            roundedRect.ArcTo(cornerRadius, cornerRadius, 0, SKPathArcSize.Small, SKPathDirection.Clockwise, rect.Right - cornerRadius, rect.Top);

            // Closing the path automatically connects back to the start point
            roundedRect.Close();

            return roundedRect;
        }

    }

    public static class QRCodeHelper
    {
        public static SKBitmap GetQRCode(string plainText, int pixelsPerModule, SKColor darkColor, SKColor lightColor, QRCodeGenerator.ECCLevel eccLevel, bool forceUtf8 = false, bool utf8BOM = false, QRCodeGenerator.EciMode eciMode = QRCodeGenerator.EciMode.Default, int requestedVersion = -1, SKBitmap icon = null, int iconSizePercent = 15, int iconBorderWidth = 0, bool drawQuietZones = true)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(plainText, eccLevel, forceUtf8, utf8BOM, eciMode, requestedVersion);
            using var qrCode = new QRCode(qrCodeData);
            return qrCode.GetGraphic(pixelsPerModule, darkColor, lightColor, icon, iconSizePercent, iconBorderWidth, drawQuietZones);
        }
    }
}