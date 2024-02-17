using System.ComponentModel;
using Shouldly;
using SkiaSharp;
using Steeltype.QRCoderLite.Tests.Helpers;
using Xunit;

namespace Steeltype.QRCoderLite.Tests
{

    public class ArtQRCodeRendererTests
    {


        [Fact]
        [Category("QRRenderer/ArtQRCode")]
        public void can_create_standard_qrcode_graphic()
        {
            var gen = new QRCodeGenerator();
            var data = gen.CreateQrCode("This is a quick test! 123#?", QRCodeGenerator.ECCLevel.H);
            var bmp = new ArtQRCode(data).GetGraphic(10);

            // Export the SKBitmap to a file (For performing live visual checks)
            /* using (var image = SKImage.FromBitmap(bmp))
            using (var imageData = image.Encode(SKEncodedImageFormat.Png, 100))
            using (var stream = File.OpenWrite("test.png"))
            {
                imageData.SaveTo(stream);
            }*/

            var result = HelperFunctions.BitmapToHash(bmp);
            result.ShouldBe("5cc2879bb001ddf9d0f933c33cc1a2fc");
        }

        [Fact]
        [Category("QRRenderer/ArtQRCode")]
        public void can_create_standard_qrcode_graphic_with_custom_finder()
        {
            var gen = new QRCodeGenerator();
            var data = gen.CreateQrCode("This is a quick test! 123#?", QRCodeGenerator.ECCLevel.H);
            var finder = new SKBitmap(15, 15);
            var bmp = new ArtQRCode(data).GetGraphic(10, SKColors.Black, SKColors.White, SKColors.Transparent, finderPatternImage: finder);

            var result = HelperFunctions.BitmapToHash(bmp);
            result.ShouldBe("3326e7df8a019ecd3bf4652666b895fd");
        }

        [Fact]
        [Category("QRRenderer/ArtQRCode")]
        public void can_create_standard_qrcode_graphic_without_quietzone()
        {
            var gen = new QRCodeGenerator();
            var data = gen.CreateQrCode("This is a quick test! 123#?", QRCodeGenerator.ECCLevel.H);
            var bmp = new ArtQRCode(data).GetGraphic(10, SKColors.Black, SKColors.White, SKColors.Transparent, drawQuietZones: false);

            var result = HelperFunctions.BitmapToHash(bmp);
            result.ShouldBe("23bf2a13cc986da7bccb0bab0a28fa9b");
        }

        [Fact]
        [Category("QRRenderer/ArtQRCode")]
        public void can_create_standard_qrcode_graphic_with_background()
        {
            var gen = new QRCodeGenerator();
            var data = gen.CreateQrCode("This is a quick test! 123#?", QRCodeGenerator.ECCLevel.H);

            //Used logo is licensed under public domain. Ref.: https://thenounproject.com/Iconathon1/collection/redefining-women/?i=2909346
            string imagePath = Path.Combine(HelperFunctions.GetAssemblyPath(), "assets", "noun_software engineer_2909346.png");
            var logo = SKBitmap.Decode(imagePath);

            var bmp = new ArtQRCode(data).GetGraphic(logo);

            var result = HelperFunctions.BitmapToHash(bmp);

            result.ShouldBe("dfd551a6d61985b025f848ef1987cde1");
        }

        [Fact]
        [Category("QRRenderer/ArtQRCode")]
        public void should_throw_pixelfactor_oor_exception()
        {
            var gen = new QRCodeGenerator();
            var data = gen.CreateQrCode("This is a quick test! 123#?", QRCodeGenerator.ECCLevel.H);
            var aCode = new ArtQRCode(data);

            var exception = Record.Exception(() => aCode.GetGraphic(10, SKColors.Black, SKColors.White, SKColors.Transparent, pixelSizeFactor: 2));
            Assert.NotNull(exception);
            Assert.IsType<Exception>(exception);
            exception.Message.ShouldBe("The parameter pixelSize must be between 0 and 1. (0-100%)");
        }

        [Fact]
        [Category("QRRenderer/ArtQRCode")]
        public void can_instantate_parameterless()
        {
            var asciiCode = new ArtQRCode();
            asciiCode.ShouldNotBeNull();
            asciiCode.ShouldBeOfType<ArtQRCode>();
        }

        [Fact]
        [Category("QRRenderer/ArtQRCode")]
        public void can_render_artqrcode_from_helper()
        {
            //Create QR code
            var bmp = ArtQRCodeHelper.GetQRCode("A", 10, SKColors.Black, SKColors.White, SKColors.Transparent, QRCodeGenerator.ECCLevel.L);

            var result = HelperFunctions.BitmapToHash(bmp);
            result.ShouldBe("7e20687f239f15a15657e6df34abceeb");
        }
    }
}