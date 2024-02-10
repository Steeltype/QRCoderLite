using SkiaSharp;
using System.Globalization;
using static Steeltype.QRCoderLite.QRCodeGenerator;

/* This renderer is inspired by RemusVasii: https://github.com/codebude/QRCoder/issues/223 */
namespace Steeltype.QRCoderLite
{
    // ReSharper disable once InconsistentNaming
    public class PdfByteQRCode : AbstractQRCode, IDisposable
    {
        private readonly byte[] pdfBinaryComment = new byte[] { 0x25, 0xe2, 0xe3, 0xcf, 0xd3 };

        /// <summary>
        /// Constructor without params to be used in COM Objects connections
        /// </summary>
        public PdfByteQRCode() { }

        public PdfByteQRCode(QRCodeData data) : base(data) { }

        /// <summary>
        /// Creates a PDF document with given colors DPI and quality
        /// </summary>
        /// <param name="pixelsPerModule"></param>
        /// <param name="darkColorHtmlHex"></param>
        /// <param name="lightColorHtmlHex"></param>
        /// <param name="dpi"></param>
        /// <param name="jpgQuality"></param>
        /// <returns></returns>
        public byte[] GetGraphic(int pixelsPerModule, string darkColorHtmlHex = "#000000", string lightColorHtmlHex = "#ffffff", int dpi = 150, long jpgQuality = 85)
        {
            byte[] jpgArray = null, pngArray = null;
            var imgSize = QrCodeData.ModuleMatrix.Count * pixelsPerModule;
            var pdfMediaSize = (imgSize * 72 / dpi).ToString(CultureInfo.InvariantCulture);

            //Get QR code image
            using (var qrCode = new PngByteQRCode(QrCodeData))
            {
                pngArray = qrCode.GetGraphic(pixelsPerModule, Utilities.HexColorToByteArray(darkColorHtmlHex), Utilities.HexColorToByteArray(lightColorHtmlHex));
            }

            // Convert PNG byte array to SKBitmap
            SKBitmap bitmap;
            using (var msPng = new MemoryStream(pngArray))
            {
                bitmap = SKBitmap.Decode(msPng);
            }

            // Ensure the bitmap was successfully loaded
            if (bitmap == null) throw new InvalidOperationException("Failed to load the PNG image.");

            //Create image and transform to JPG
            using (var image = SKImage.FromBitmap(bitmap))
            {
                // Encode the SKImage to JPEG format with the specified quality
                using var data = image.Encode(SKEncodedImageFormat.Jpeg, (int)jpgQuality);
                using var msJpeg = new MemoryStream();
                data.SaveTo(msJpeg);
                jpgArray = msJpeg.ToArray();
            }

            // Create PDF document
            using var stream = new MemoryStream();
            var writer = new StreamWriter(stream, System.Text.Encoding.GetEncoding("ASCII"));
            var xrefs = new List<long>(); // Cross-reference table for PDF objects

            // Write PDF header
            writer.Write("%PDF-1.5\r\n");
            writer.Flush();

            // Binary comment to ensure the file is treated as binary
            stream.Write(pdfBinaryComment, 0, pdfBinaryComment.Length);
            writer.WriteLine();

            // Add first object: Root Catalog
            writer.Flush(); // Flush to get an accurate position
            xrefs.Add(stream.Position); // Record position for xref table
            writer.Write($"{1} 0 obj\r\n<<\r\n/Type /Catalog\r\n/Pages 2 0 R\r\n>>\r\nendobj\r\n");

            // Add second object: Page Tree (Pages dictionary)
            writer.Flush(); // Flush to get an accurate position
            xrefs.Add(stream.Position); // Record position for xref table
            writer.Write($"{2} 0 obj\r\n<<\r\n/Count 1\r\n/Kids [3 0 R]\r\n>>\r\nendobj\r\n");

            // Add third object: Page
            writer.Flush(); // Flush to get an accurate position
            xrefs.Add(stream.Position); // Record position for xref table
            // Define a page with media size and reference to the content stream and resources
            writer.Write($"{3} 0 obj\r\n<<\r\n/Type /Page\r\n/Parent 2 0 R\r\n/MediaBox [0 0 {pdfMediaSize} {pdfMediaSize}]\r\n/Contents 4 0 R\r\n/Resources << /ProcSet [/PDF /ImageC] /XObject << /Im1 5 0 R >> >>\r\n>>\r\nendobj\r\n");

            // Add fourth object: Content Stream (how to draw the image)
            var contentStream = $"q\r\n{pdfMediaSize} 0 0 {pdfMediaSize} 0 0 cm\r\n/Im1 Do\r\nQ";
            writer.Flush(); // Flush to get an accurate position
            xrefs.Add(stream.Position); // Record position for xref table
            writer.Write($"{4} 0 obj\r\n<< /Length {contentStream.Length} >>\r\nstream\r\n{contentStream}\r\nendstream\r\nendobj\r\n");

            // Add fifth object: Embedded Image
            writer.Flush(); // Flush to get an accurate position
            xrefs.Add(stream.Position); // Record position for xref table
            writer.Write($"{5} 0 obj\r\n<<\r\n/Type /XObject\r\n/Subtype /Image\r\n/Width {imgSize}\r\n/Height {imgSize}\r\n/Length {jpgArray.Length}\r\n/Filter /DCTDecode\r\n/ColorSpace /DeviceRGB\r\n/BitsPerComponent 8\r\n>>\r\nstream\r\n");
            writer.Flush(); // Flush before writing binary data
            stream.Write(jpgArray, 0, jpgArray.Length); // Write the JPEG binary data
            writer.Write("\r\nendstream\r\nendobj\r\n");

            // Finalize document with xref table, trailer, and EOF
            writer.Flush();
            xrefs.Add(stream.Position); // Record position for the length of JPEG data
            writer.Write($"{6} 0 obj\r\n{jpgArray.Length}\r\nendobj\r\n");
            var startxref = stream.Position; // Record start of xref table
            writer.Write("xref\r\n0 " + (xrefs.Count + 1) + "\r\n0000000000 65535 f\r\n");
            foreach (var refValue in xrefs)
                writer.Write($"{refValue:0000000000} 00000 n\r\n"); // Write each object's position
            // Write trailer and EOF
            writer.Write($"trailer\r\n<<\r\n/Size {xrefs.Count + 1}\r\n/Root 1 0 R\r\n>>\r\nstartxref\r\n{startxref}\r\n%%EOF");
            writer.Flush(); // Ensure all data is written to stream

            stream.Position = 0;

            return stream.ToArray();
        }
    }

    public static class PdfByteQRCodeHelper
    {
        public static byte[] GetQRCode(string plainText, int pixelsPerModule, string darkColorHtmlHex,
            string lightColorHtmlHex, ECCLevel eccLevel, bool forceUtf8 = false, bool utf8BOM = false,
            EciMode eciMode = EciMode.Default, int requestedVersion = -1)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(plainText, eccLevel, forceUtf8, utf8BOM, eciMode,
                requestedVersion);
            using var qrCode = new PdfByteQRCode(qrCodeData);
            return qrCode.GetGraphic(pixelsPerModule, darkColorHtmlHex, lightColorHtmlHex);
        }

        public static byte[] GetQRCode(string txt, ECCLevel eccLevel, int size)
        {
            using var qrGen = new QRCodeGenerator();
            using var qrCode = qrGen.CreateQrCode(txt, eccLevel);
            using var qrBmp = new PdfByteQRCode(qrCode);
            return qrBmp.GetGraphic(size);
        }
    }
}