using Shouldly;
using Steeltype.QRCoderLite.Tests.Helpers;
using Xunit;

namespace Steeltype.QRCoderLite.Tests
{
    /****************************************************************************************************
     * Note: Test cases compare the outcome visually even if it's slower than a byte-wise compare.
     *       This is necessary, because the Deflate implementation differs on the different target
     *       platforms and thus the outcome, even if visually identical, differs. Thus only a visual
     *       test method makes sense. In addition bytewise differences shouldn't be important, if the
     *       visual outcome is identical and thus the qr code is identical/scannable.
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

            var result = HelperFunctions.ByteArrayToHash(pngCodeGfx);
            result.ShouldBe("1fc35c3bea6fad47427143ce716c83b8");
        }

        [Fact]
        [Category("QRRenderer/PngByteQRCode")]
        public void can_render_pngbyte_qrcode_color()
        {
            //Create QR code
            var gen = new QRCodeGenerator();
            var data = gen.CreateQrCode("This is a quick test! 123#?", QRCodeGenerator.ECCLevel.L);
            var pngCodeGfx = new PngByteQRCode(data).GetGraphic(5, new byte[] { 255, 0, 0 }, new byte[] { 0, 0, 255 });

            var result = HelperFunctions.ByteArrayToHash(pngCodeGfx);
            result.ShouldBe("0144b1d40aa6eeb6cb07df42822ea0a7");
        }


        [Fact]
        [Category("QRRenderer/PngByteQRCode")]
        public void can_render_pngbyte_qrcode_color_with_alpha()
        {
            //Create QR code
            var gen = new QRCodeGenerator();
            var data = gen.CreateQrCode("This is a quick test! 123#?", QRCodeGenerator.ECCLevel.L);
            var pngCodeGfx = new PngByteQRCode(data).GetGraphic(5, new byte[] { 255, 255, 255, 127 }, new byte[] { 0, 0, 255 });

            var result = HelperFunctions.ByteArrayToHash(pngCodeGfx);
            result.ShouldBe("627ce564fb5e17be42e4a85e907a17b5");
        }

        [Fact]
        [Category("QRRenderer/PngByteQRCode")]
        public void can_render_pngbyte_qrcode_color_without_quietzones()
        {
            //Create QR code
            var gen = new QRCodeGenerator();
            var data = gen.CreateQrCode("This is a quick test! 123#?", QRCodeGenerator.ECCLevel.L);
            var pngCodeGfx = new PngByteQRCode(data).GetGraphic(5, new byte[] { 255, 255, 255, 127 }, new byte[] { 0, 0, 255 }, false);

            var result = HelperFunctions.ByteArrayToHash(pngCodeGfx);
            result.ShouldBe("07f760b3eb54901840b094d31e299713");
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

            var result = HelperFunctions.ByteArrayToHash(pngCodeGfx);
            result.ShouldBe("c562388f4f3cf13a299b469a3e3b852f");
        }

        [Fact]
        [Category("QRRenderer/PngByteQRCode")]
        public void can_render_pngbyte_qrcode_from_helper_2()
        {
            //Create QR code                   
            var pngCodeGfx = PngByteQRCodeHelper.GetQRCode("This is a quick test! 123#?", 5, new byte[] { 255, 255, 255, 127 }, new byte[] { 0, 0, 255 }, QRCodeGenerator.ECCLevel.L);

            var result = HelperFunctions.ByteArrayToHash(pngCodeGfx);
            result.ShouldBe("627ce564fb5e17be42e4a85e907a17b5");
        }

    }
}



