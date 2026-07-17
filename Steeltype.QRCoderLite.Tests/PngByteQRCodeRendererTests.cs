using System.ComponentModel;
using Shouldly;
using Steeltype.QRCoderLite.Tests.Helpers;
using Xunit;

namespace Steeltype.QRCoderLite.Tests
{
    /****************************************************************************************************
     * Note: Expectations are hashes of the DECODED PNG content (IHDR/PLTE/tRNS chunk data plus the
     *       inflated IDAT scanlines) via HelperFunctions.PngContentHash, NOT of the raw file bytes.
     *       DeflateStream emits different (equally valid) compressed bytes per platform, so raw-file
     *       hashes fail cross-platform CI; the decoded content is identical everywhere.
     ****************************************************************************************************/
    public class PngByteQRCodeRendererTests
    {
        [Fact]
        [Category("QRRenderer/PngByteQRCode")]
        public void can_render_pngbyte_qrcode_blackwhite()
        {
            //Create QR code
            var gen = new QRCodeGenerator();
            var data = gen.CreateQrCode("This is a quick test! 123#?", QRCodeGenerator.ECCLevel.L);
            var pngCodeGfx = new PngByteQRCode(data).GetGraphic(5);

            var result = HelperFunctions.PngContentHash(pngCodeGfx);
            result.ShouldBe("34492655df1be8746cf2bdabf9da773a");
        }

        [Fact]
        [Category("QRRenderer/PngByteQRCode")]
        public void can_render_pngbyte_qrcode_color()
        {
            //Create QR code
            var gen = new QRCodeGenerator();
            var data = gen.CreateQrCode("This is a quick test! 123#?", QRCodeGenerator.ECCLevel.L);
            var pngCodeGfx = new PngByteQRCode(data).GetGraphic(5, new byte[] { 255, 0, 0 }, new byte[] { 0, 0, 255 });

            var result = HelperFunctions.PngContentHash(pngCodeGfx);
            result.ShouldBe("a8266054916dc674e0a6dcf40bce6b80");
        }


        [Fact]
        [Category("QRRenderer/PngByteQRCode")]
        public void can_render_pngbyte_qrcode_color_with_alpha()
        {
            //Create QR code
            var gen = new QRCodeGenerator();
            var data = gen.CreateQrCode("This is a quick test! 123#?", QRCodeGenerator.ECCLevel.L);
            var pngCodeGfx = new PngByteQRCode(data).GetGraphic(5, new byte[] { 255, 255, 255, 127 }, new byte[] { 0, 0, 255 });

            var result = HelperFunctions.PngContentHash(pngCodeGfx);
            result.ShouldBe("b20d400246e34bb03078123663e58aee");
        }

        [Fact]
        [Category("QRRenderer/PngByteQRCode")]
        public void can_render_pngbyte_qrcode_color_without_quietzones()
        {
            //Create QR code
            var gen = new QRCodeGenerator();
            var data = gen.CreateQrCode("This is a quick test! 123#?", QRCodeGenerator.ECCLevel.L);
            var pngCodeGfx = new PngByteQRCode(data).GetGraphic(5, new byte[] { 255, 255, 255, 127 }, new byte[] { 0, 0, 255 }, false);

            var result = HelperFunctions.PngContentHash(pngCodeGfx);
            result.ShouldBe("2c284646c4f4e338e181da5c075689e2");
        }

        [Fact]
        [Category("QRRenderer/PngByteQRCode")]
        public void can_instantate_pngbyte_qrcode_parameterless()
        {
            var pngCode = new PngByteQRCode();
            pngCode.ShouldNotBeNull();
            pngCode.ShouldBeOfType<PngByteQRCode>();
        }

        [Fact]
        [Category("QRRenderer/PngByteQRCode")]
        public void can_render_pngbyte_qrcode_from_helper()
        {
            //Create QR code                   
            var pngCodeGfx = PngByteQRCodeHelper.GetQRCode("This is a quick test! 123#?", QRCodeGenerator.ECCLevel.L, 10);

            var result = HelperFunctions.PngContentHash(pngCodeGfx);
            result.ShouldBe("d0dc2df14b0a0b65b11de1230962485c");
        }

        [Fact]
        [Category("QRRenderer/PngByteQRCode")]
        public void can_render_pngbyte_qrcode_from_helper_2()
        {
            //Create QR code                   
            var pngCodeGfx = PngByteQRCodeHelper.GetQRCode("This is a quick test! 123#?", 5, new byte[] { 255, 255, 255, 127 }, new byte[] { 0, 0, 255 }, QRCodeGenerator.ECCLevel.L);

            var result = HelperFunctions.PngContentHash(pngCodeGfx);
            result.ShouldBe("b20d400246e34bb03078123663e58aee");
        }

    }
}



