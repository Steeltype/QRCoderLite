using static Steeltype.QRCoderLite.QRCodeGenerator;

namespace Steeltype.QRCoderLite
{
    public class BitmapByteQRCode : AbstractQRCode, IDisposable
    {
        public BitmapByteQRCode(QRCodeData data) : base(data) { }

        public byte[] GetGraphic(int pixelsPerModule)
        {
            return GetGraphic(pixelsPerModule, new byte[] { 0x00, 0x00, 0x00 }, new byte[] { 0xFF, 0xFF, 0xFF });
        }

        public byte[] GetGraphic(int pixelsPerModule, string darkColorHtmlHex, string lightColorHtmlHex)
        {
            return GetGraphic(pixelsPerModule, Utilities.HexColorToByteArray(darkColorHtmlHex), Utilities.HexColorToByteArray(lightColorHtmlHex));
        }

        private readonly byte[] BITMAP_HEADER_START = new byte[]
        {
            // BMP Signature
            0x42, // 'B'
            0x4D, // 'M'
            // File size (set to 0 here, indicating it will be filled in later)
            0x4C, 0x00, 0x00, 0x00, 
            // Reserved fields (unused and set to zero)
            0x00, 0x00, 
            0x00, 0x00, 
            // Offset where the pixel array (bitmap data) can be found
            0x1E, 0x00, 0x00, 0x00,
            // Header size (12 bytes for BITMAPCOREHEADER)
            0x0C, 0x00, 0x00, 0x00,
            // Following would continue with width, height, planes, bitCount, etc., based on specific BMP format
        };

        private readonly byte[] BITMAP_HEADER_END = new byte[]
        {
            0x01, 0x00, // Planes (1)
            0x18, 0x00 // Bits per pixel (24)
        };

        // Create a bitmap file header for the given width and height.
        private List<byte> CreateBitmapHeader(int width, int height)
        {
            var header = new List<byte>(BITMAP_HEADER_START);
            header.AddRange(Utilities.IntTo4Byte(width));
            header.AddRange(Utilities.IntTo4Byte(height));
            header.AddRange(BITMAP_HEADER_END);
            return header;
        }

        public byte[] GetGraphic(int pixelsPerModule, byte[] darkColorRgb, byte[] lightColorRgb)
        {
            var sideLength = QrCodeData.ModuleMatrix.Count * pixelsPerModule;

            var moduleDark = darkColorRgb.Reverse();
            var moduleLight = lightColorRgb.Reverse();

            var bmp = CreateBitmapHeader(sideLength, sideLength);

            //draw qr code
            var modulesCount = QrCodeData.ModuleMatrix.Count; // Total number of modules per side in the QR code.

            // Iterate over each module in the QR code.
            for (var moduleX = modulesCount - 1; moduleX >= 0; moduleX--)
            {
                for (var moduleY = 0; moduleY < modulesCount; moduleY++)
                {
                    // Determine if the current module is dark or light.
                    var isModuleDark = QrCodeData.ModuleMatrix[moduleX][moduleY];

                    // For each pixel within a module (both X and Y directions).
                    for (var pixelX = 0; pixelX < pixelsPerModule; pixelX++)
                    {
                        for (var pixelY = 0; pixelY < pixelsPerModule; pixelY++)
                        {
                            // Add the appropriate color bytes for this pixel.
                            bmp.AddRange(isModuleDark ? moduleDark : moduleLight);
                        }
                    }
                }

                // Handle padding for QR codes where the side length isn't evenly divisible by 4.
                if (sideLength % 4 == 0) continue;

                for (var i = 0; i < sideLength % 4; i++)
                {
                    bmp.Add(0x00); // Pad with zeros to align to a 4-byte boundary.
                }
            }

            //finalize with terminator
            bmp.AddRange(new byte[] { 0x00, 0x00 });

            return bmp.ToArray();
        }
    }

    public static class BitmapByteQRCodeHelper
    {
        public static byte[] GetQRCode(string plainText, int pixelsPerModule, string darkColorHtmlHex,
            string lightColorHtmlHex, ECCLevel eccLevel, bool forceUtf8 = false, bool utf8BOM = false,
            EciMode eciMode = EciMode.Default, int requestedVersion = -1)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(plainText, eccLevel, forceUtf8, utf8BOM, eciMode,
                requestedVersion);
            using var qrCode = new BitmapByteQRCode(qrCodeData);
            return qrCode.GetGraphic(pixelsPerModule, darkColorHtmlHex, lightColorHtmlHex);
        }

        public static byte[] GetQRCode(string txt, ECCLevel eccLevel, int size)
        {
            using var qrGen = new QRCodeGenerator();
            using var qrCode = qrGen.CreateQrCode(txt, eccLevel);
            using var qrBmp = new BitmapByteQRCode(qrCode);
            return qrBmp.GetGraphic(size);
        }
    }
}
