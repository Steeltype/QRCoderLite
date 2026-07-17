using System.ComponentModel;
using System.Globalization;
using Shouldly;
using Xunit;

namespace Steeltype.QRCoderLite.Tests
{

    public class BitcoinLikeCryptoCurrencyAddressTests
    {
        private const string BitcoinAddressValue = "175tWpb8K1S7NmH4Zx6rewF9WQrcZv245W";
        private const string BitcoinCashAddressValue = "qqtlfk37qyey50f4wfuhc7jw85zsdp8s2swffjk890";
        private const string LitecoinAddressValue = "LY1t7iLnwtPCb1DPZP38FA835XzFqXBq54";

        [Fact]
        [Category("PayloadGenerator/BitcoinAddress")]
        public void bitcoin_address_generator_can_generate_address()
        {
            var amount = .123;
            var label = "Some Label to Encode";
            var message = "Some Message to Encode";

            var generator = new PayloadGenerator.BitcoinAddress(BitcoinAddressValue, amount, label, message);

            generator
                .ToString()
                .ShouldBe("bitcoin:175tWpb8K1S7NmH4Zx6rewF9WQrcZv245W?label=Some%20Label%20to%20Encode&message=Some%20Message%20to%20Encode&amount=.123");
        }

        [Fact]
        [Category("PayloadGenerator/BitcoinAddress")]
        public void bitcoin_address_generator_can_generate_address_without_optional_parts()
        {
            var generator = new PayloadGenerator.BitcoinAddress(BitcoinAddressValue);

            // No amount, label or message: the payload is just scheme + address, without a query separator.
            generator
                .ToString()
                .ShouldBe("bitcoin:175tWpb8K1S7NmH4Zx6rewF9WQrcZv245W");
        }

        [Fact]
        [Category("PayloadGenerator/BitcoinAddress")]
        public void bitcoin_address_generator_should_skip_missing_label()
        {
            var amount = .123;
            var message = "Some Message to Encode";

            var generator = new PayloadGenerator.BitcoinAddress(BitcoinAddressValue, amount, null, message);

            var result = generator.ToString();
            result.ShouldNotContain("label");
            result.ShouldContain("message=Some%20Message%20to%20Encode");
        }

        [Fact]
        [Category("PayloadGenerator/BitcoinAddress")]
        public void bitcoin_address_generator_should_skip_missing_message()
        {
            var amount = .123;

            var generator = new PayloadGenerator.BitcoinAddress(BitcoinAddressValue, amount);

            generator
                .ToString()
                .ShouldNotContain("message");
        }

        [Fact]
        [Category("PayloadGenerator/BitcoinAddress")]
        public void bitcoin_address_generator_should_treat_empty_strings_as_omitted()
        {
            var amount = .123;

            var generator = new PayloadGenerator.BitcoinAddress(BitcoinAddressValue, amount, string.Empty, string.Empty);

            // Empty label/message must behave exactly like null: skipped entirely.
            generator
                .ToString()
                .ShouldBe("bitcoin:175tWpb8K1S7NmH4Zx6rewF9WQrcZv245W?amount=.123");
        }

        [Fact]
        [Category("PayloadGenerator/BitcoinAddress")]
        public void bitcoin_address_generator_should_round_to_satoshi()
        {
            var amount = .123456789;

            var generator = new PayloadGenerator.BitcoinAddress(BitcoinAddressValue, amount);

            // "#.########" keeps at most 8 fractional digits (1 satoshi = 1e-8 BTC).
            generator
                .ToString()
                .ShouldContain("amount=.12345679");
        }

        [Fact]
        [Category("PayloadGenerator/BitcoinAddress")]
        public void bitcoin_address_generator_formats_amount_without_leading_zero()
        {
            var amount = .123;

            var generator = new PayloadGenerator.BitcoinAddress(BitcoinAddressValue, amount);

            // "#.########" suppresses the leading zero: 0.123 renders as ".123".
            generator
                .ToString()
                .ShouldBe("bitcoin:175tWpb8K1S7NmH4Zx6rewF9WQrcZv245W?amount=.123");
        }

        [Theory]
        [Category("PayloadGenerator/BitcoinAddress")]
        [InlineData(13d, "13")]
        [InlineData(20.3d, "20.3")]
        [InlineData(0.00000001d, ".00000001")]
        public void bitcoin_address_generator_formats_amounts_with_expected_precision(double amount, string expectedAmount)
        {
            var generator = new PayloadGenerator.BitcoinAddress(BitcoinAddressValue, amount);

            generator
                .ToString()
                .ShouldBe($"bitcoin:175tWpb8K1S7NmH4Zx6rewF9WQrcZv245W?amount={expectedAmount}");
        }

        [Fact]
        [Category("PayloadGenerator/BitcoinAddress")]
        public void bitcoin_address_generator_uri_escapes_label_and_message()
        {
            var label = "Käse & Brot";
            var message = "50% off = deal#1";

            var generator = new PayloadGenerator.BitcoinAddress(BitcoinAddressValue, null, label, message);

            // Uri.EscapeDataString: UTF-8 percent-encoding, reserved characters included.
            generator
                .ToString()
                .ShouldBe("bitcoin:175tWpb8K1S7NmH4Zx6rewF9WQrcZv245W?label=K%C3%A4se%20%26%20Brot&message=50%25%20off%20%3D%20deal%231");
        }

        [Fact]
        [Category("PayloadGenerator/BitcoinAddress")]
        public void bitcoin_address_generator_disregards_current_culture()
        {
            var previousCulture = CultureInfo.CurrentCulture;
            var previousUICulture = CultureInfo.CurrentUICulture;
            try
            {
                // de-DE uses a comma decimal separator; the payload must stay invariant.
                CultureInfo.CurrentCulture = new CultureInfo("de-DE");
                CultureInfo.CurrentUICulture = new CultureInfo("de-DE");

                var amount = .123;

                var generator = new PayloadGenerator.BitcoinAddress(BitcoinAddressValue, amount);

                generator
                    .ToString()
                    .ShouldBe("bitcoin:175tWpb8K1S7NmH4Zx6rewF9WQrcZv245W?amount=.123");
            }
            finally
            {
                CultureInfo.CurrentCulture = previousCulture;
                CultureInfo.CurrentUICulture = previousUICulture;
            }
        }

        [Fact]
        [Category("PayloadGenerator/BitcoinCashAddress")]
        public void bitcoincash_address_generator_can_generate_address()
        {
            var amount = .123;
            var label = "Some Label to Encode";
            var message = "Some Message to Encode";

            var generator = new PayloadGenerator.BitcoinCashAddress(BitcoinCashAddressValue, amount, label, message);

            generator
                .ToString()
                .ShouldBe("bitcoincash:qqtlfk37qyey50f4wfuhc7jw85zsdp8s2swffjk890?label=Some%20Label%20to%20Encode&message=Some%20Message%20to%20Encode&amount=.123");
        }

        [Fact]
        [Category("PayloadGenerator/BitcoinCashAddress")]
        public void bitcoincash_address_generator_should_skip_missing_label()
        {
            var amount = .123;
            var message = "Some Message to Encode";

            var generator = new PayloadGenerator.BitcoinCashAddress(BitcoinCashAddressValue, amount, null, message);

            generator
                .ToString()
                .ShouldNotContain("label");
        }

        [Fact]
        [Category("PayloadGenerator/BitcoinCashAddress")]
        public void bitcoincash_address_generator_should_round_to_satoshi()
        {
            var amount = .123456789;

            var generator = new PayloadGenerator.BitcoinCashAddress(BitcoinCashAddressValue, amount);

            generator
                .ToString()
                .ShouldContain("amount=.12345679");
        }

        [Fact]
        [Category("PayloadGenerator/LitecoinAddress")]
        public void litecoin_address_generator_can_generate_address()
        {
            var amount = .123;
            var label = "Some Label to Encode";
            var message = "Some Message to Encode";

            var generator = new PayloadGenerator.LitecoinAddress(LitecoinAddressValue, amount, label, message);

            generator
                .ToString()
                .ShouldBe("litecoin:LY1t7iLnwtPCb1DPZP38FA835XzFqXBq54?label=Some%20Label%20to%20Encode&message=Some%20Message%20to%20Encode&amount=.123");
        }

        [Fact]
        [Category("PayloadGenerator/LitecoinAddress")]
        public void litecoin_address_generator_should_skip_missing_message()
        {
            var amount = .123;

            var generator = new PayloadGenerator.LitecoinAddress(LitecoinAddressValue, amount);

            generator
                .ToString()
                .ShouldNotContain("message");
        }

        [Fact]
        [Category("PayloadGenerator/LitecoinAddress")]
        public void litecoin_address_generator_should_round_to_satoshi()
        {
            var amount = .123456789;

            var generator = new PayloadGenerator.LitecoinAddress(LitecoinAddressValue, amount);

            generator
                .ToString()
                .ShouldContain("amount=.12345679");
        }

        [Theory]
        [Category("PayloadGenerator/BitcoinLikeCryptoCurrencyAddress")]
        [InlineData(PayloadGenerator.BitcoinLikeCryptoCurrencyAddress.BitcoinLikeCryptoCurrencyType.Bitcoin, "bitcoin")]
        [InlineData(PayloadGenerator.BitcoinLikeCryptoCurrencyAddress.BitcoinLikeCryptoCurrencyType.BitcoinCash, "bitcoincash")]
        [InlineData(PayloadGenerator.BitcoinLikeCryptoCurrencyAddress.BitcoinLikeCryptoCurrencyType.Litecoin, "litecoin")]
        public void base_class_uses_lowercase_scheme_per_currency(
            PayloadGenerator.BitcoinLikeCryptoCurrencyAddress.BitcoinLikeCryptoCurrencyType currencyType, string expectedScheme)
        {
            var generator = new PayloadGenerator.BitcoinLikeCryptoCurrencyAddress(currencyType, "SomeAddress123");

            generator
                .ToString()
                .ShouldBe($"{expectedScheme}:SomeAddress123");
        }

        [Fact]
        [Category("PayloadGenerator/BitcoinLikeCryptoCurrencyAddress")]
        public void base_class_passes_address_through_unmodified()
        {
            // Addresses are case-sensitive (Base58/Bech32) and must not be escaped or re-cased.
            var mixedCaseAddress = "1BoatSLRHtKNngkdXEeobR76b53LETtpyT";

            var generator = new PayloadGenerator.BitcoinLikeCryptoCurrencyAddress(
                PayloadGenerator.BitcoinLikeCryptoCurrencyAddress.BitcoinLikeCryptoCurrencyType.Bitcoin, mixedCaseAddress);

            generator
                .ToString()
                .ShouldBe("bitcoin:1BoatSLRHtKNngkdXEeobR76b53LETtpyT");
        }
    }
}
