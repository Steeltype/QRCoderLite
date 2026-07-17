using System.ComponentModel;
using Shouldly;
using Xunit;

namespace Steeltype.QRCoderLite.Tests
{

    public class PhoneNumberTests
    {

        [Fact]
        [Category("PayloadGenerator/PhoneNumber")]
        public void phonenumber_should_build()
        {
            var number = "+495321123456";

            var generator = new PayloadGenerator.PhoneNumber(number);

            generator.ToString().ShouldBe("tel:+495321123456");
        }

        [Fact]
        [Category("PayloadGenerator/PhoneNumber")]
        public void phonenumber_should_pass_number_through_verbatim()
        {
            // PhoneNumber performs no cleanup - formatting characters are preserved as given
            var number = "+49 (5321) 123-456";

            var generator = new PayloadGenerator.PhoneNumber(number);

            generator.ToString().ShouldBe("tel:+49 (5321) 123-456");
        }
    }
}
