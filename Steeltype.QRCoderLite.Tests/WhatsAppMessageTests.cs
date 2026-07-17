using System.ComponentModel;
using Shouldly;
using Xunit;

namespace Steeltype.QRCoderLite.Tests
{

    public class WhatsAppMessageTests
    {

        [Fact]
        [Category("PayloadGenerator/WhatsAppMessage")]
        public void whatsapp_generator_can_generate_payload_simple()
        {
            var number = "491601234567";
            var msg = "This is a sample message with Umlauts: Ä,ö, ü and ß.";
            var generator = new PayloadGenerator.WhatsAppMessage(number, msg);

            generator
                .ToString()
                .ShouldBe("https://wa.me/491601234567?text=This%20is%20a%20sample%20message%20with%20Umlauts%3A%20%C3%84%2C%C3%B6%2C%20%C3%BC%20and%20%C3%9F.");
        }

        [Fact]
        [Category("PayloadGenerator/WhatsAppMessage")]
        public void whatsapp_should_add_unused_params()
        {
            var msg = "This is a sample message with Umlauts: Ä,ö, ü and ß.";
            var generator = new PayloadGenerator.WhatsAppMessage(msg);

            generator
                .ToString()
                .ShouldBe("https://wa.me/?text=This%20is%20a%20sample%20message%20with%20Umlauts%3A%20%C3%84%2C%C3%B6%2C%20%C3%BC%20and%20%C3%9F.");
        }

        [Fact]
        [Category("PayloadGenerator/WhatsAppMessage")]
        public void whatsapp_should_cleanup_phonenumber_1()
        {
            var number = "+49(160)1234567";
            var msg = "This is a sample message with Umlauts: Ä,ö, ü and ß.";
            var generator = new PayloadGenerator.WhatsAppMessage(number, msg);

            generator
                .ToString()
                .ShouldBe("https://wa.me/491601234567?text=This%20is%20a%20sample%20message%20with%20Umlauts%3A%20%C3%84%2C%C3%B6%2C%20%C3%BC%20and%20%C3%9F.");
        }

        [Fact]
        [Category("PayloadGenerator/WhatsAppMessage")]
        public void whatsapp_should_cleanup_phonenumber_2()
        {
            var number = "0049-160-1234 567";
            var msg = "This is a sample message with Umlauts: Ä,ö, ü and ß.";
            var generator = new PayloadGenerator.WhatsAppMessage(number, msg);

            generator
                .ToString()
                .ShouldBe("https://wa.me/491601234567?text=This%20is%20a%20sample%20message%20with%20Umlauts%3A%20%C3%84%2C%C3%B6%2C%20%C3%BC%20and%20%C3%9F.");
        }

        [Fact]
        [Category("PayloadGenerator/WhatsAppMessage")]
        public void whatsapp_should_cleanup_mixed_leading_plus_and_zeros()
        {
            // Cleanup regex ^[0+]+|[ ()-] strips any mix of leading '+' and '0'
            // characters plus all spaces, parentheses, and dashes
            var number = "+0049 (160) 123-4567";
            var msg = "Hello";
            var generator = new PayloadGenerator.WhatsAppMessage(number, msg);

            generator.ToString().ShouldBe("https://wa.me/491601234567?text=Hello");
        }

        [Fact]
        [Category("PayloadGenerator/WhatsAppMessage")]
        public void whatsapp_should_preserve_interior_zeros()
        {
            // Only LEADING zeros are stripped - zeros inside the number must survive
            var number = "49160012345";
            var msg = "Hello";
            var generator = new PayloadGenerator.WhatsAppMessage(number, msg);

            generator.ToString().ShouldBe("https://wa.me/49160012345?text=Hello");
        }
    }
}
