using SkiaSharp;
using System.Security.Cryptography;
using System.Text;


namespace Steeltype.QRCoderLite.Tests.Helpers
{
    public static class HelperFunctions
    {
        public static string GetAssemblyPath()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        public static string BitmapToHash(SKBitmap bmp)
        {
            byte[] imgBytes;
            using (var image = SKImage.FromBitmap(bmp))
            using (var data = image.Encode(SKEncodedImageFormat.Png, 100)) // Encode the image to PNG
            using (var ms = new MemoryStream())
            {
                data.SaveTo(ms); // Save the encoded image data to the MemoryStream
                imgBytes = ms.ToArray();
            }
            return ByteArrayToHash(imgBytes);
        }

        public static string ByteArrayToHash(byte[] data)
        {
            var md5 = MD5.Create();
            var hash = md5.ComputeHash(data);
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

        public static string StringToHash(string data)
        {
            return ByteArrayToHash(Encoding.UTF8.GetBytes(data));
        }
    }
}
