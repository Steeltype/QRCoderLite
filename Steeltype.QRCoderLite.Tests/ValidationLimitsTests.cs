using System.ComponentModel;
using Shouldly;
using Xunit;

namespace Steeltype.QRCoderLite.Tests
{
    /****************************************************************************************************
     * Boundary tests for the input validation limits introduced in commit 10e3273
     * ("Add input validation and limits for QR code generation"). Each limit gets a
     * boundary pair: the extreme accepted value succeeds, the first rejected value throws.
     *
     * Limits under test:
     *   AsciiQRCode.GetGraphic / GetLineByLineGraphic : repeatPerModule 1..100
     *   PngByteQRCode.GetGraphic (both overloads)     : pixelsPerModule 1..100
     *   BitmapByteQRCode.GetGraphic                   : pixelsPerModule 1..100
     *   PdfByteQRCode.GetGraphic                      : pixelsPerModule 1..1000, dpi 1..2400
     *   SvgQRCode.GetGraphic (both overloads)         : pixelsPerModule 1..1000
     *   SvgQRCode.GetGraphic (logo overload)          : logoBytes non-null/non-empty/<= 5 MB,
     *                                                   logoSizePercent 1..50
     *
     * All range violations throw ArgumentOutOfRangeException; logo byte[] violations throw
     * ArgumentException (verified against the source). "Ok" cases use the smallest QR data
     * (version 1) and only assert that non-empty output was produced.
     ****************************************************************************************************/
    public class ValidationLimitsTests
    {
        private const int MaxLogoSizeBytes = 5 * 1024 * 1024;

        private static QRCodeData CreateSmallestQrCodeData()
        {
            using var gen = new QRCodeGenerator();
            return gen.CreateQrCode("A", QRCodeGenerator.ECCLevel.L);
        }

        #region AsciiQRCode - repeatPerModule 1..100

        [Fact]
        [Category("Validation/AsciiQRCode")]
        public void ascii_repeat_per_module_lower_bound_1_is_accepted()
        {
            var asciiQrCode = new AsciiQRCode(CreateSmallestQrCodeData());

            var graphic = asciiQrCode.GetGraphic(1);

            graphic.ShouldNotBeNullOrEmpty();
        }

        [Fact]
        [Category("Validation/AsciiQRCode")]
        public void ascii_repeat_per_module_upper_bound_100_is_accepted()
        {
            var asciiQrCode = new AsciiQRCode(CreateSmallestQrCodeData());

            // Single-char module strings keep the output small even at the maximum repeat count.
            var lines = asciiQrCode.GetLineByLineGraphic(100, "X", " ");

            lines.ShouldNotBeNull();
            lines.ShouldNotBeEmpty();
        }

        [Fact]
        [Category("Validation/AsciiQRCode")]
        public void ascii_repeat_per_module_0_throws()
        {
            var asciiQrCode = new AsciiQRCode(CreateSmallestQrCodeData());

            var ex = Should.Throw<ArgumentOutOfRangeException>(() => asciiQrCode.GetGraphic(0));
            ex.ParamName.ShouldBe("repeatPerModule");
        }

        [Fact]
        [Category("Validation/AsciiQRCode")]
        public void ascii_repeat_per_module_101_throws()
        {
            var asciiQrCode = new AsciiQRCode(CreateSmallestQrCodeData());

            var ex = Should.Throw<ArgumentOutOfRangeException>(() => asciiQrCode.GetLineByLineGraphic(101));
            ex.ParamName.ShouldBe("repeatPerModule");
        }

        #endregion

        #region PngByteQRCode - pixelsPerModule 1..100

        [Fact]
        [Category("Validation/PngByteQRCode")]
        public void png_pixels_per_module_lower_bound_1_is_accepted()
        {
            var pngQrCode = new PngByteQRCode(CreateSmallestQrCodeData());

            var png = pngQrCode.GetGraphic(1);

            png.ShouldNotBeNull();
            png.ShouldNotBeEmpty();
        }

        [Fact]
        [Category("Validation/PngByteQRCode")]
        public void png_pixels_per_module_upper_bound_100_is_accepted()
        {
            var pngQrCode = new PngByteQRCode(CreateSmallestQrCodeData());

            var png = pngQrCode.GetGraphic(100);

            png.ShouldNotBeNull();
            png.ShouldNotBeEmpty();
        }

        [Fact]
        [Category("Validation/PngByteQRCode")]
        public void png_pixels_per_module_0_throws()
        {
            var pngQrCode = new PngByteQRCode(CreateSmallestQrCodeData());

            var ex = Should.Throw<ArgumentOutOfRangeException>(() => pngQrCode.GetGraphic(0));
            ex.ParamName.ShouldBe("pixelsPerModule");
        }

        [Fact]
        [Category("Validation/PngByteQRCode")]
        public void png_pixels_per_module_101_throws()
        {
            var pngQrCode = new PngByteQRCode(CreateSmallestQrCodeData());

            var ex = Should.Throw<ArgumentOutOfRangeException>(() => pngQrCode.GetGraphic(101));
            ex.ParamName.ShouldBe("pixelsPerModule");
        }

        [Fact]
        [Category("Validation/PngByteQRCode")]
        public void png_color_overload_pixels_per_module_0_throws()
        {
            var pngQrCode = new PngByteQRCode(CreateSmallestQrCodeData());

            var ex = Should.Throw<ArgumentOutOfRangeException>(
                () => pngQrCode.GetGraphic(0, new byte[] { 0, 0, 0 }, new byte[] { 255, 255, 255 }));
            ex.ParamName.ShouldBe("pixelsPerModule");
        }

        [Fact]
        [Category("Validation/PngByteQRCode")]
        public void png_color_overload_pixels_per_module_101_throws()
        {
            var pngQrCode = new PngByteQRCode(CreateSmallestQrCodeData());

            var ex = Should.Throw<ArgumentOutOfRangeException>(
                () => pngQrCode.GetGraphic(101, new byte[] { 0, 0, 0 }, new byte[] { 255, 255, 255 }));
            ex.ParamName.ShouldBe("pixelsPerModule");
        }

        #endregion

        #region BitmapByteQRCode - pixelsPerModule 1..100

        [Fact]
        [Category("Validation/BitmapByteQRCode")]
        public void bitmap_pixels_per_module_lower_bound_1_is_accepted()
        {
            var bitmapQrCode = new BitmapByteQRCode(CreateSmallestQrCodeData());

            var bmp = bitmapQrCode.GetGraphic(1);

            bmp.ShouldNotBeNull();
            bmp.ShouldNotBeEmpty();
        }

        [Fact]
        [Category("Validation/BitmapByteQRCode")]
        public void bitmap_pixels_per_module_upper_bound_100_is_accepted()
        {
            var bitmapQrCode = new BitmapByteQRCode(CreateSmallestQrCodeData());

            var bmp = bitmapQrCode.GetGraphic(100);

            bmp.ShouldNotBeNull();
            bmp.ShouldNotBeEmpty();
        }

        [Fact]
        [Category("Validation/BitmapByteQRCode")]
        public void bitmap_pixels_per_module_0_throws()
        {
            var bitmapQrCode = new BitmapByteQRCode(CreateSmallestQrCodeData());

            var ex = Should.Throw<ArgumentOutOfRangeException>(() => bitmapQrCode.GetGraphic(0));
            ex.ParamName.ShouldBe("pixelsPerModule");
        }

        [Fact]
        [Category("Validation/BitmapByteQRCode")]
        public void bitmap_pixels_per_module_101_throws()
        {
            var bitmapQrCode = new BitmapByteQRCode(CreateSmallestQrCodeData());

            var ex = Should.Throw<ArgumentOutOfRangeException>(() => bitmapQrCode.GetGraphic(101));
            ex.ParamName.ShouldBe("pixelsPerModule");
        }

        #endregion

        #region PdfByteQRCode - pixelsPerModule 1..1000, dpi 1..2400

        [Fact]
        [Category("Validation/PdfByteQRCode")]
        public void pdf_pixels_per_module_lower_bound_1_is_accepted()
        {
            var pdfQrCode = new PdfByteQRCode(CreateSmallestQrCodeData());

            var pdf = pdfQrCode.GetGraphic(1);

            pdf.ShouldNotBeNull();
            pdf.ShouldNotBeEmpty();
        }

        [Fact]
        [Category("Validation/PdfByteQRCode")]
        public void pdf_pixels_per_module_upper_bound_1000_is_accepted()
        {
            var pdfQrCode = new PdfByteQRCode(CreateSmallestQrCodeData());

            // The PDF output is vector-based, so the maximum pixelsPerModule stays cheap.
            var pdf = pdfQrCode.GetGraphic(1000);

            pdf.ShouldNotBeNull();
            pdf.ShouldNotBeEmpty();
        }

        [Fact]
        [Category("Validation/PdfByteQRCode")]
        public void pdf_pixels_per_module_0_throws()
        {
            var pdfQrCode = new PdfByteQRCode(CreateSmallestQrCodeData());

            var ex = Should.Throw<ArgumentOutOfRangeException>(() => pdfQrCode.GetGraphic(0));
            ex.ParamName.ShouldBe("pixelsPerModule");
        }

        [Fact]
        [Category("Validation/PdfByteQRCode")]
        public void pdf_pixels_per_module_1001_throws()
        {
            var pdfQrCode = new PdfByteQRCode(CreateSmallestQrCodeData());

            var ex = Should.Throw<ArgumentOutOfRangeException>(() => pdfQrCode.GetGraphic(1001));
            ex.ParamName.ShouldBe("pixelsPerModule");
        }

        [Fact]
        [Category("Validation/PdfByteQRCode")]
        public void pdf_dpi_lower_bound_1_is_accepted()
        {
            var pdfQrCode = new PdfByteQRCode(CreateSmallestQrCodeData());

            var pdf = pdfQrCode.GetGraphic(1, "#000000", "#ffffff", 1);

            pdf.ShouldNotBeNull();
            pdf.ShouldNotBeEmpty();
        }

        [Fact]
        [Category("Validation/PdfByteQRCode")]
        public void pdf_dpi_upper_bound_2400_is_accepted()
        {
            var pdfQrCode = new PdfByteQRCode(CreateSmallestQrCodeData());

            var pdf = pdfQrCode.GetGraphic(1, "#000000", "#ffffff", 2400);

            pdf.ShouldNotBeNull();
            pdf.ShouldNotBeEmpty();
        }

        [Fact]
        [Category("Validation/PdfByteQRCode")]
        public void pdf_dpi_0_throws()
        {
            var pdfQrCode = new PdfByteQRCode(CreateSmallestQrCodeData());

            var ex = Should.Throw<ArgumentOutOfRangeException>(
                () => pdfQrCode.GetGraphic(1, "#000000", "#ffffff", 0));
            ex.ParamName.ShouldBe("dpi");
        }

        [Fact]
        [Category("Validation/PdfByteQRCode")]
        public void pdf_dpi_2401_throws()
        {
            var pdfQrCode = new PdfByteQRCode(CreateSmallestQrCodeData());

            var ex = Should.Throw<ArgumentOutOfRangeException>(
                () => pdfQrCode.GetGraphic(1, "#000000", "#ffffff", 2401));
            ex.ParamName.ShouldBe("dpi");
        }

        #endregion

        #region SvgQRCode - pixelsPerModule 1..1000

        [Fact]
        [Category("Validation/SvgQRCode")]
        public void svg_pixels_per_module_lower_bound_1_is_accepted()
        {
            var svgQrCode = new SvgQRCode(CreateSmallestQrCodeData());

            var svg = svgQrCode.GetGraphic(1);

            svg.ShouldNotBeNullOrEmpty();
        }

        [Fact]
        [Category("Validation/SvgQRCode")]
        public void svg_pixels_per_module_upper_bound_1000_is_accepted()
        {
            var svgQrCode = new SvgQRCode(CreateSmallestQrCodeData());

            // The SVG output is vector-based, so the maximum pixelsPerModule stays cheap.
            var svg = svgQrCode.GetGraphic(1000);

            svg.ShouldNotBeNullOrEmpty();
        }

        [Fact]
        [Category("Validation/SvgQRCode")]
        public void svg_pixels_per_module_0_throws()
        {
            var svgQrCode = new SvgQRCode(CreateSmallestQrCodeData());

            var ex = Should.Throw<ArgumentOutOfRangeException>(() => svgQrCode.GetGraphic(0));
            ex.ParamName.ShouldBe("pixelsPerModule");
        }

        [Fact]
        [Category("Validation/SvgQRCode")]
        public void svg_pixels_per_module_1001_throws()
        {
            var svgQrCode = new SvgQRCode(CreateSmallestQrCodeData());

            var ex = Should.Throw<ArgumentOutOfRangeException>(() => svgQrCode.GetGraphic(1001));
            ex.ParamName.ShouldBe("pixelsPerModule");
        }

        [Fact]
        [Category("Validation/SvgQRCode")]
        public void svg_logo_overload_pixels_per_module_0_throws()
        {
            var svgQrCode = new SvgQRCode(CreateSmallestQrCodeData());

            // pixelsPerModule is validated before the logo arguments in the logo overload.
            var ex = Should.Throw<ArgumentOutOfRangeException>(
                () => svgQrCode.GetGraphic(0, "#000000", "#ffffff", new byte[] { 1, 2, 3 }));
            ex.ParamName.ShouldBe("pixelsPerModule");
        }

        #endregion

        #region SvgQRCode - logoBytes null/empty/> 5 MB

        [Fact]
        [Category("Validation/SvgQRCode")]
        public void svg_logo_null_throws()
        {
            var svgQrCode = new SvgQRCode(CreateSmallestQrCodeData());

            var ex = Should.Throw<ArgumentException>(
                () => svgQrCode.GetGraphic(1, "#000000", "#ffffff", (byte[])null));
            ex.ParamName.ShouldBe("logoBytes");
        }

        [Fact]
        [Category("Validation/SvgQRCode")]
        public void svg_logo_empty_throws()
        {
            var svgQrCode = new SvgQRCode(CreateSmallestQrCodeData());

            var ex = Should.Throw<ArgumentException>(
                () => svgQrCode.GetGraphic(1, "#000000", "#ffffff", Array.Empty<byte>()));
            ex.ParamName.ShouldBe("logoBytes");
        }

        [Fact]
        [Category("Validation/SvgQRCode")]
        public void svg_logo_exactly_5_mb_is_accepted()
        {
            var svgQrCode = new SvgQRCode(CreateSmallestQrCodeData());

            // The logo is embedded as base64 without image decoding, so arbitrary bytes are fine.
            var svg = svgQrCode.GetGraphic(1, "#000000", "#ffffff", new byte[MaxLogoSizeBytes]);

            svg.ShouldNotBeNullOrEmpty();
        }

        [Fact]
        [Category("Validation/SvgQRCode")]
        public void svg_logo_over_5_mb_throws()
        {
            var svgQrCode = new SvgQRCode(CreateSmallestQrCodeData());

            var ex = Should.Throw<ArgumentException>(
                () => svgQrCode.GetGraphic(1, "#000000", "#ffffff", new byte[MaxLogoSizeBytes + 1]));
            ex.ParamName.ShouldBe("logoBytes");
        }

        #endregion

        #region SvgQRCode - logoSizePercent 1..50

        [Fact]
        [Category("Validation/SvgQRCode")]
        public void svg_logo_size_percent_lower_bound_1_is_accepted()
        {
            var svgQrCode = new SvgQRCode(CreateSmallestQrCodeData());

            var svg = svgQrCode.GetGraphic(1, "#000000", "#ffffff", new byte[] { 1, 2, 3 }, 1);

            svg.ShouldNotBeNullOrEmpty();
        }

        [Fact]
        [Category("Validation/SvgQRCode")]
        public void svg_logo_size_percent_upper_bound_50_is_accepted()
        {
            var svgQrCode = new SvgQRCode(CreateSmallestQrCodeData());

            var svg = svgQrCode.GetGraphic(1, "#000000", "#ffffff", new byte[] { 1, 2, 3 }, 50);

            svg.ShouldNotBeNullOrEmpty();
        }

        [Fact]
        [Category("Validation/SvgQRCode")]
        public void svg_logo_size_percent_0_throws()
        {
            var svgQrCode = new SvgQRCode(CreateSmallestQrCodeData());

            var ex = Should.Throw<ArgumentOutOfRangeException>(
                () => svgQrCode.GetGraphic(1, "#000000", "#ffffff", new byte[] { 1, 2, 3 }, 0));
            ex.ParamName.ShouldBe("logoSizePercent");
        }

        [Fact]
        [Category("Validation/SvgQRCode")]
        public void svg_logo_size_percent_51_throws()
        {
            var svgQrCode = new SvgQRCode(CreateSmallestQrCodeData());

            var ex = Should.Throw<ArgumentOutOfRangeException>(
                () => svgQrCode.GetGraphic(1, "#000000", "#ffffff", new byte[] { 1, 2, 3 }, 51));
            ex.ParamName.ShouldBe("logoSizePercent");
        }

        #endregion
    }
}
