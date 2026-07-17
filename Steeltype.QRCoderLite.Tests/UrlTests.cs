using System.ComponentModel;
using Shouldly;
using Xunit;

namespace Steeltype.QRCoderLite.Tests
{

    public class UrlTests
    {

        [Fact]
        [Category("PayloadGenerator/Url")]
        public void url_should_build_http()
        {
            var url = "http://code-bude.net";

            var generator = new PayloadGenerator.Url(url);

            generator.ToString().ShouldBe("http://code-bude.net");
        }

        [Fact]
        [Category("PayloadGenerator/Url")]
        public void url_should_build_https()
        {
            var url = "https://code-bude.net";

            var generator = new PayloadGenerator.Url(url);

            generator.ToString().ShouldBe("https://code-bude.net");
        }

        [Fact]
        [Category("PayloadGenerator/Url")]
        public void url_should_build_https_all_caps()
        {
            // Requires case-insensitive scheme detection - an all-caps scheme
            // must not be double-prefixed with "http://"
            var url = "HTTPS://CODE-BUDE.NET";

            var generator = new PayloadGenerator.Url(url);

            generator.ToString().ShouldBe("HTTPS://CODE-BUDE.NET");
        }

        [Fact]
        [Category("PayloadGenerator/Url")]
        public void url_should_build_https_mixed_case()
        {
            var url = "HtTpS://code-bude.net";

            var generator = new PayloadGenerator.Url(url);

            generator.ToString().ShouldBe("HtTpS://code-bude.net");
        }

        [Fact]
        [Category("PayloadGenerator/Url")]
        public void url_should_add_http()
        {
            var url = "code-bude.net";

            var generator = new PayloadGenerator.Url(url);

            generator.ToString().ShouldBe("http://code-bude.net");
        }
    }
}
