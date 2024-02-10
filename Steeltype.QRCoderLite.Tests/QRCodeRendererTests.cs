using Shouldly;
using SkiaSharp;
using Steeltype.QRCoderLite.Tests.Helpers;
using Xunit;

namespace Steeltype.QRCoderLite.Tests
{

    public class QRCodeRendererTests
    {
        [Fact]
        [Category("QRRenderer/QRCode")]
        public void can_create_qrcode_standard_graphic()
        {
            var data = QRCodeGenerator.GenerateQrCode("This is a quick test! 123#?", QRCodeGenerator.ECCLevel.H);
            var bmp = new QRCode(data).GetGraphic(10);

            var result = HelperFunctions.BitmapToHash(bmp);
            result.ShouldBe("1e0afd60c239d24be2ce0f8286a16918");
        }

        [Fact]
        [Category("QRRenderer/QRCode")]
        public void can_create_qrcode_standard_graphic_hex()
        {
            var data = QRCodeGenerator.GenerateQrCode("This is a quick test! 123#?", QRCodeGenerator.ECCLevel.H);
            var bmp = new QRCode(data).GetGraphic(10, "#000000", "#ffffff");

            var result = HelperFunctions.BitmapToHash(bmp);
            result.ShouldBe("1e0afd60c239d24be2ce0f8286a16918");
        }


        [Fact]
        [Category("QRRenderer/QRCode")]
        public void can_create_qrcode_standard_graphic_without_quietzones()
        {
            var data = QRCodeGenerator.GenerateQrCode("This is a quick test! 123#?", QRCodeGenerator.ECCLevel.H);
            var bmp = new QRCode(data).GetGraphic(5, SKColors.Black, SKColors.White, false);

            var result = HelperFunctions.BitmapToHash(bmp);
            result.ShouldBe("78f6af3170e47f3e930dfc05fa4f0cce");
        }


        [Fact]
        [Category("QRRenderer/QRCode")]
        public void can_create_qrcode_with_transparent_logo_graphic()
        {
            //Create QR code
            var data = QRCodeGenerator.GenerateQrCode("This is a quick test! 123#?", QRCodeGenerator.ECCLevel.H);

            string imagePath = Path.Combine(HelperFunctions.GetAssemblyPath(), "assets", "noun_software engineer_2909346.png");
            var logo = SKBitmap.Decode(imagePath);

            var bmp = new QRCode(data).GetGraphic(10, SKColors.Black, SKColors.Transparent, icon: logo);
            //Used logo is licensed under public domain. Ref.: https://thenounproject.com/Iconathon1/collection/redefining-women/?i=2909346
            var result = HelperFunctions.BitmapToHash(bmp);
            result.ShouldBe("c20da0015d039b92e8e652183643f101");
        }

        [Fact]
        [Category("QRRenderer/QRCode")]
        public void can_create_qrcode_with_non_transparent_logo_graphic()
        {
            //Create QR code
            var data = QRCodeGenerator.GenerateQrCode("This is a quick test! 123#?", QRCodeGenerator.ECCLevel.H);

            //Used logo is licensed under public domain. Ref.: https://thenounproject.com/Iconathon1/collection/redefining-women/?i=2909346
            string imagePath = Path.Combine(HelperFunctions.GetAssemblyPath(), "assets", "noun_software engineer_2909346.png");
            var logo = SKBitmap.Decode(imagePath);

            var bmp = new QRCode(data).GetGraphic(10, SKColors.Black, SKColors.White, icon: logo);

            var result = HelperFunctions.BitmapToHash(bmp);
            result.ShouldBe("478b8f52349924cbb067255b35e66df9");
        }

        [Fact]
        [Category("QRRenderer/QRCode")]
        public void can_create_qrcode_with_logo_and_with_transparent_border()
        {
            //Create QR code
            var data = QRCodeGenerator.GenerateQrCode("This is a quick test! 123#?", QRCodeGenerator.ECCLevel.H);

            //Used logo is licensed under public domain. Ref.: https://thenounproject.com/Iconathon1/collection/redefining-women/?i=2909346
            string imagePath = Path.Combine(HelperFunctions.GetAssemblyPath(), "assets", "noun_software engineer_2909346.png");
            var logo = SKBitmap.Decode(imagePath);

            var bmp = new QRCode(data).GetGraphic(10, SKColors.Black, SKColors.Transparent, icon: logo, iconBorderWidth: 6);
            var result = HelperFunctions.BitmapToHash(bmp);
            result.ShouldBe("c20da0015d039b92e8e652183643f101");
        }

        [Fact]
        [Category("QRRenderer/QRCode")]
        public void can_create_qrcode_with_logo_and_with_standard_border()
        {
            //Create QR code
            var data = QRCodeGenerator.GenerateQrCode("This is a quick test! 123#?", QRCodeGenerator.ECCLevel.H);

            //Used logo is licensed under public domain. Ref.: https://thenounproject.com/Iconathon1/collection/redefining-women/?i=2909346
            string imagePath = Path.Combine(HelperFunctions.GetAssemblyPath(), "assets", "noun_software engineer_2909346.png");
            var logo = SKBitmap.Decode(imagePath);

            var bmp = new QRCode(data).GetGraphic(10, SKColors.Black, SKColors.White, icon: logo, iconBorderWidth: 6);
            var result = HelperFunctions.BitmapToHash(bmp);
            result.ShouldBe("478b8f52349924cbb067255b35e66df9");
        }

        [Fact]
        [Category("QRRenderer/QRCode")]
        public void can_create_qrcode_with_logo_and_with_custom_border()
        {
            //Create QR code
            var data = QRCodeGenerator.GenerateQrCode("This is a quick test! 123#?", QRCodeGenerator.ECCLevel.H);

            //Used logo is licensed under public domain. Ref.: https://thenounproject.com/Iconathon1/collection/redefining-women/?i=2909346
            string imagePath = Path.Combine(HelperFunctions.GetAssemblyPath(), "assets", "noun_software engineer_2909346.png");
            var logo = SKBitmap.Decode(imagePath);

            var bmp = new QRCode(data).GetGraphic(10, SKColors.Black, SKColors.Transparent, icon: logo, iconBorderWidth: 6, iconBackgroundColor: SKColors.DarkGreen);
            var result = HelperFunctions.BitmapToHash(bmp);
            result.ShouldBe("72705f15b9373b8d4e3b52c1a160d866");
        }


        [Fact]
        [Category("QRRenderer/QRCode")]
        public void can_instantate_qrcode_parameterless()
        {
            var svgCode = new QRCode();
            svgCode.ShouldNotBeNull();
            svgCode.ShouldBeOfType<QRCode>();
        }
    }
}