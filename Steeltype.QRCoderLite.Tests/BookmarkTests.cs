using System.ComponentModel;
using Shouldly;
using Xunit;

namespace Steeltype.QRCoderLite.Tests
{

    public class BookmarkTests
    {

        [Fact]
        [Category("PayloadGenerator/Bookmark")]
        public void bookmark_should_build()
        {
            var url = "http://code-bude.net";
            var title = "A nerd's blog";

            var generator = new PayloadGenerator.Bookmark(url, title);

            generator.ToString().ShouldBe("MEBKM:TITLE:A nerd's blog;URL:http\\://code-bude.net;;");
        }

        [Fact]
        [Category("PayloadGenerator/Bookmark")]
        public void bookmark_should_escape_input()
        {
            var url = "http://code-bude.net/fake,url.html";
            var title = "A nerd's blog: \\All;the;things\\";

            var generator = new PayloadGenerator.Bookmark(url, title);

            generator.ToString().ShouldBe("MEBKM:TITLE:A nerd's blog\\: \\\\All\\;the\\;things\\\\;URL:http\\://code-bude.net/fake\\,url.html;;");
        }

        [Fact]
        [Category("PayloadGenerator/Bookmark")]
        public void bookmark_should_handle_null_inputs()
        {
            // Fork-specific: EscapeInput null-guards, so null url/title
            // produce empty fields instead of a NullReferenceException
            var generator = new PayloadGenerator.Bookmark(null, null);

            generator.ToString().ShouldBe("MEBKM:TITLE:;URL:;;");
        }
    }
}
