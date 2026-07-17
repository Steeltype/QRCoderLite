using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Shouldly;
using Xunit;

namespace Steeltype.QRCoderLite.Tests
{
    /****************************************************************************************************
     * Structural tests for the dependency-free renderers. Instead of comparing against frozen
     * output blobs, these tests verify the renderers' output against the format specifications
     * and against the QRCodeData.ModuleMatrix they were generated from. They lock in the
     * orientation fix in AsciiQRCode (ModuleMatrix must be indexed [y][x], not [x][y]) and the
     * PDF page-tree fix (indirect Page object referenced from /Kids, spec-correct /Length and
     * xref offsets).
     ****************************************************************************************************/
    public class RendererStructuralTests
    {
        private const string SvgNamespace = "http://www.w3.org/2000/svg";
        private const string XlinkNamespace = "http://www.w3.org/1999/xlink";

        private static QRCodeData CreateData(string text = "A05", QRCodeGenerator.ECCLevel ecc = QRCodeGenerator.ECCLevel.Q)
        {
            var gen = new QRCodeGenerator();
            return gen.CreateQrCode(text, ecc);
        }

        private static int ReadInt32Le(byte[] buffer, int offset)
        {
            return buffer[offset]
                | (buffer[offset + 1] << 8)
                | (buffer[offset + 2] << 16)
                | (buffer[offset + 3] << 24);
        }

        //---------------------------------------------------------------------
        // (a) ASCII orientation — regression lock for the [x][y] transpose fix
        //---------------------------------------------------------------------

        [Fact]
        [Category("QRRenderer/AsciiQRCode")]
        public void ascii_line_by_line_graphic_matches_module_matrix_cell_for_cell()
        {
            using var data = CreateData();
            var matrix = data.ModuleMatrix;
            var side = matrix.Count;

            // Guard: the matrix must be asymmetric, otherwise a transposed renderer
            // would still pass the cell-for-cell comparison below.
            var asymmetric = false;
            for (var y = 0; y < side && !asymmetric; y++)
            {
                for (var x = 0; x < side && !asymmetric; x++)
                {
                    if (matrix[y][x] != matrix[x][y])
                        asymmetric = true;
                }
            }
            asymmetric.ShouldBeTrue("test input produced a symmetric matrix; pick different input text");

            var lines = new AsciiQRCode(data).GetLineByLineGraphic(1, "X", " ");

            lines.Length.ShouldBe(side);
            for (var y = 0; y < side; y++)
            {
                var expected = new StringBuilder(side);
                for (var x = 0; x < side; x++)
                {
                    expected.Append(matrix[y][x] ? 'X' : ' ');
                }
                lines[y].ShouldBe(expected.ToString(), $"row {y} must equal ModuleMatrix[{y}][x] for all x");
            }
        }

        //---------------------------------------------------------------------
        // (b) SVG structure
        //---------------------------------------------------------------------

        [Fact]
        [Category("QRRenderer/SvgQRCode")]
        public void svg_output_parses_as_xml_with_viewbox_and_path()
        {
            using var data = CreateData();
            var side = data.ModuleMatrix.Count;
            var svg = new SvgQRCode(data).GetGraphic(5);

            var doc = XDocument.Parse(svg);
            doc.Root.ShouldNotBeNull();
            doc.Root!.Name.LocalName.ShouldBe("svg");
            doc.Root.Name.NamespaceName.ShouldBe(SvgNamespace);

            var viewBox = doc.Root.Attribute("viewBox");
            viewBox.ShouldNotBeNull();
            viewBox!.Value.ShouldBe($"0 0 {side} {side}");

            XNamespace ns = SvgNamespace;
            var path = doc.Root.Elements(ns + "path").ToList();
            path.Count.ShouldBe(1);
            path[0].Attribute("d")!.Value.ShouldStartWith("M");
        }

        [Fact]
        [Category("QRRenderer/SvgQRCode")]
        public void svg_logo_overload_embeds_base64_image_with_default_png_mime_type()
        {
            using var data = CreateData();
            var logoBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x01, 0x02, 0x03 };
            var svg = new SvgQRCode(data).GetGraphic(5, "#000000", "#ffffff", logoBytes);

            var doc = XDocument.Parse(svg);
            XNamespace ns = SvgNamespace;
            XNamespace xlink = XlinkNamespace;

            var images = doc.Root!.Elements(ns + "image").ToList();
            images.Count.ShouldBe(1);

            var href = images[0].Attribute(xlink + "href");
            href.ShouldNotBeNull();
            href!.Value.ShouldBe($"data:image/png;base64,{Convert.ToBase64String(logoBytes)}");
        }

        [Fact]
        [Category("QRRenderer/SvgQRCode")]
        public void svg_logo_overload_respects_custom_mime_type()
        {
            using var data = CreateData();
            var logoBytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x10, 0x20, 0x30 };
            var svg = new SvgQRCode(data).GetGraphic(5, "#000000", "#ffffff", logoBytes, logoMimeType: "image/jpeg");

            var doc = XDocument.Parse(svg);
            XNamespace ns = SvgNamespace;
            XNamespace xlink = XlinkNamespace;

            var href = doc.Root!.Elements(ns + "image").Single().Attribute(xlink + "href");
            href.ShouldNotBeNull();
            href!.Value.ShouldBe($"data:image/jpeg;base64,{Convert.ToBase64String(logoBytes)}");
        }

        //---------------------------------------------------------------------
        // (c) PDF — regression lock for the page-tree fix
        //---------------------------------------------------------------------

        [Fact]
        [Category("QRRenderer/PdfByteQRCode")]
        public void pdf_contains_header_and_correct_page_tree()
        {
            using var data = CreateData();
            var bytes = new PdfByteQRCode(data).GetGraphic(5);
            var pdf = Encoding.ASCII.GetString(bytes);

            pdf.ShouldStartWith("%PDF");
            pdf.ShouldContain("/Type /Pages");
            pdf.ShouldContain("/Kids [ 3 0 R ]");
            pdf.ShouldContain("/Type /Page");
        }

        [Fact]
        [Category("QRRenderer/PdfByteQRCode")]
        public void pdf_length_entry_matches_actual_stream_byte_count()
        {
            using var data = CreateData();
            var bytes = new PdfByteQRCode(data).GetGraphic(5);
            var pdf = Encoding.ASCII.GetString(bytes);

            // ASCII decoding maps every byte to exactly one char, so string
            // indexes equal byte offsets.
            pdf.Length.ShouldBe(bytes.Length);

            var lengthMatch = Regex.Match(pdf, @"/Length (\d+) >>\r\nstream\r\n");
            lengthMatch.Success.ShouldBeTrue("PDF must contain a /Length entry followed by the stream keyword");
            var declaredLength = int.Parse(lengthMatch.Groups[1].Value);

            var streamStart = lengthMatch.Index + lengthMatch.Length;
            var endstreamIndex = pdf.IndexOf("endstream", streamStart, StringComparison.Ordinal);
            endstreamIndex.ShouldBeGreaterThan(streamStart);

            // The EOL before 'endstream' is excluded from /Length (ISO 32000 sec. 7.3.8.1).
            pdf.Substring(endstreamIndex - 2, 2).ShouldBe("\r\n");
            var actualLength = endstreamIndex - 2 - streamStart;

            declaredLength.ShouldBe(actualLength);
        }

        [Fact]
        [Category("QRRenderer/PdfByteQRCode")]
        public void pdf_xref_table_offsets_point_at_object_definitions()
        {
            using var data = CreateData();
            var bytes = new PdfByteQRCode(data).GetGraphic(5);
            var pdf = Encoding.ASCII.GetString(bytes);

            var xrefMatch = Regex.Match(pdf, @"xref\r\n0 (\d+)\r\n((?:\d{10} \d{5} [fn]\r\n)+)");
            xrefMatch.Success.ShouldBeTrue("PDF must contain an xref table");

            var declaredCount = int.Parse(xrefMatch.Groups[1].Value);
            declaredCount.ShouldBe(5); // free entry + catalog + pages + page + content stream

            var entries = xrefMatch.Groups[2].Value
                .Split("\r\n", StringSplitOptions.RemoveEmptyEntries)
                .ToList();
            entries.Count.ShouldBe(declaredCount);

            entries[0].ShouldBe("0000000000 65535 f");
            for (var objectNumber = 1; objectNumber < declaredCount; objectNumber++)
            {
                var parts = entries[objectNumber].Split(' ');
                parts[2].ShouldBe("n");
                var offset = int.Parse(parts[0]);
                var expectedHeader = $"{objectNumber} 0 obj";
                pdf.Substring(offset, expectedHeader.Length)
                    .ShouldBe(expectedHeader, $"xref offset {offset} for object {objectNumber} must point at its definition");
            }
        }

        [Fact]
        [Category("QRRenderer/PdfByteQRCode")]
        public void pdf_startxref_offset_points_at_xref_keyword()
        {
            using var data = CreateData();
            var bytes = new PdfByteQRCode(data).GetGraphic(5);
            var pdf = Encoding.ASCII.GetString(bytes);

            var startxrefMatch = Regex.Match(pdf, @"startxref\r\n(\d+)\r\n%%EOF");
            startxrefMatch.Success.ShouldBeTrue("PDF must end with startxref <offset> %%EOF");

            var offset = int.Parse(startxrefMatch.Groups[1].Value);
            pdf.Substring(offset, 4).ShouldBe("xref");
        }

        //---------------------------------------------------------------------
        // (d) BMP structure and pixel decode
        //---------------------------------------------------------------------

        [Fact]
        [Category("QRRenderer/BitmapByteQRCode")]
        public void bmp_signature_and_declared_file_size_are_correct()
        {
            using var data = CreateData();
            var bmp = new BitmapByteQRCode(data).GetGraphic(1);

            bmp[0].ShouldBe((byte)'B');
            bmp[1].ShouldBe((byte)'M');
            ReadInt32Le(bmp, 2).ShouldBe(bmp.Length);
        }

        [Fact]
        [Category("QRRenderer/BitmapByteQRCode")]
        public void bmp_pixel_rows_decode_to_module_matrix()
        {
            using var data = CreateData();
            var matrix = data.ModuleMatrix;
            var side = matrix.Count;
            var bmp = new BitmapByteQRCode(data).GetGraphic(1);

            // 54-byte header: width at offset 18, height at offset 22 (both LE).
            ReadInt32Le(bmp, 18).ShouldBe(side);
            ReadInt32Le(bmp, 22).ShouldBe(side);

            // Rows are padded to 4-byte boundaries and stored bottom-up in BGR order.
            var rowPadding = (4 - (side * 3) % 4) % 4;
            var stride = side * 3 + rowPadding;
            bmp.Length.ShouldBe(54 + side * stride);

            for (var y = 0; y < side; y++)
            {
                var rowOffset = 54 + (side - 1 - y) * stride;
                var expected = new StringBuilder(side);
                var actual = new StringBuilder(side);
                for (var x = 0; x < side; x++)
                {
                    var b = bmp[rowOffset + x * 3];
                    var g = bmp[rowOffset + x * 3 + 1];
                    var r = bmp[rowOffset + x * 3 + 2];

                    // Default palette is pure black on pure white.
                    g.ShouldBe(b);
                    r.ShouldBe(b);
                    (b == 0x00 || b == 0xFF).ShouldBeTrue($"pixel ({x},{y}) must be pure black or pure white but was 0x{b:X2}");

                    expected.Append(matrix[y][x] ? 'X' : ' ');
                    actual.Append(b == 0x00 ? 'X' : ' ');
                }
                actual.ToString().ShouldBe(expected.ToString(), $"BMP row for matrix row {y} must match ModuleMatrix[{y}]");
            }
        }

        //---------------------------------------------------------------------
        // (e) Base64QRCode delegates to PngByteQRCode
        //---------------------------------------------------------------------

        [Fact]
        [Category("QRRenderer/Base64QRCode")]
        public void base64_qrcode_decodes_to_exact_pngbyte_qrcode_bytes()
        {
            using var data = CreateData("This is a quick test! 123#?", QRCodeGenerator.ECCLevel.L);
            var base64 = new Base64QRCode(data).GetGraphic(5);
            var pngBytes = new PngByteQRCode(data).GetGraphic(5);

            Convert.FromBase64String(base64).ShouldBe(pngBytes);
        }

        [Fact]
        [Category("QRRenderer/Base64QRCode")]
        public void base64_qrcode_color_overload_decodes_to_exact_pngbyte_qrcode_bytes()
        {
            using var data = CreateData("This is a quick test! 123#?", QRCodeGenerator.ECCLevel.L);
            var darkColor = new byte[] { 255, 0, 0 };
            var lightColor = new byte[] { 0, 0, 255 };

            var base64 = new Base64QRCode(data).GetGraphic(5, darkColor, lightColor);
            var pngBytes = new PngByteQRCode(data).GetGraphic(5, darkColor, lightColor);

            Convert.FromBase64String(base64).ShouldBe(pngBytes);
        }
    }
}
