using System.ComponentModel;
using Shouldly;
using Steeltype.QRCoderLite;
using Xunit;

namespace Steeltype.QRCoderLite.Tests
{
    public class OneTimePasswordTests
    {
        [Fact]
        [Category("PayloadGenerator/OneTimePassword")]
        public void one_time_password_generator_time_based_generates_with_standard_options()
        {
            var pg = new PayloadGenerator.OneTimePassword
            {
                Secret = "pwq6 5q55",
                Issuer = "Google",
                Label = "test@google.com",
            };

            pg.ToString().ShouldBe("otpauth://totp/Google:test%40google.com?secret=pwq65q55&issuer=Google");
        }

        [Fact]
        [Category("PayloadGenerator/OneTimePassword")]
        public void one_time_password_generator_time_based_generates_with_standard_options_escapes_issuer_and_label()
        {
            var pg = new PayloadGenerator.OneTimePassword
            {
                Secret = "pwq6 5q55",
                Issuer = "Google Google",
                Label = "test/test@google.com",
            };

            pg.ToString().ShouldBe("otpauth://totp/Google%20Google:test%2Ftest%40google.com?secret=pwq65q55&issuer=Google%20Google");
        }

        [Fact]
        [Category("PayloadGenerator/OneTimePassword")]
        public void one_time_password_generator_hmac_based_generates_with_standard_options()
        {
            var pg = new PayloadGenerator.OneTimePassword
            {
                Secret = "pwq6 5q55",
                Issuer = "Google",
                Label = "test@google.com",
                Type = PayloadGenerator.OneTimePassword.OneTimePasswordAuthType.HOTP,
                Counter = 500,
            };

            pg.ToString().ShouldBe("otpauth://hotp/Google:test%40google.com?secret=pwq65q55&issuer=Google&counter=500");
        }

        [Fact]
        [Category("PayloadGenerator/OneTimePassword")]
        public void one_time_password_generator_hmac_based_generates_with_standard_options_escapes_issuer_and_label()
        {
            var pg = new PayloadGenerator.OneTimePassword
            {
                Secret = "pwq6 5q55",
                Issuer = "Google Google",
                Label = "test/test@google.com",
                Type = PayloadGenerator.OneTimePassword.OneTimePasswordAuthType.HOTP,
                Counter = 500,
            };

            pg.ToString().ShouldBe("otpauth://hotp/Google%20Google:test%2Ftest%40google.com?secret=pwq65q55&issuer=Google%20Google&counter=500");
        }

        [Fact]
        [Category("PayloadGenerator/OneTimePassword")]
        public void one_time_password_generator_totp_with_sha256_algorithm()
        {
            var pg = new PayloadGenerator.OneTimePassword
            {
                Secret = "pwq6 5q55",
                Issuer = "Google",
                Label = "test@google.com",
                AuthAlgorithm = PayloadGenerator.OneTimePassword.OneTimePasswordAuthAlgorithm.SHA256,
            };

            pg.ToString().ShouldBe("otpauth://totp/Google:test%40google.com?secret=pwq65q55&issuer=Google&algorithm=SHA256");
        }

        [Fact]
        [Category("PayloadGenerator/OneTimePassword")]
        public void one_time_password_generator_totp_with_sha512_algorithm()
        {
            var pg = new PayloadGenerator.OneTimePassword
            {
                Secret = "pwq6 5q55",
                Issuer = "Google",
                Label = "test@google.com",
                AuthAlgorithm = PayloadGenerator.OneTimePassword.OneTimePasswordAuthAlgorithm.SHA512,
            };

            pg.ToString().ShouldBe("otpauth://totp/Google:test%40google.com?secret=pwq65q55&issuer=Google&algorithm=SHA512");
        }

        [Fact]
        [Category("PayloadGenerator/OneTimePassword")]
        public void one_time_password_generator_hotp_with_sha256_algorithm()
        {
            var pg = new PayloadGenerator.OneTimePassword
            {
                Secret = "pwq6 5q55",
                Issuer = "Google",
                Label = "test@google.com",
                Type = PayloadGenerator.OneTimePassword.OneTimePasswordAuthType.HOTP,
                Counter = 100,
                AuthAlgorithm = PayloadGenerator.OneTimePassword.OneTimePasswordAuthAlgorithm.SHA256,
            };

            pg.ToString().ShouldBe("otpauth://hotp/Google:test%40google.com?secret=pwq65q55&issuer=Google&algorithm=SHA256&counter=100");
        }

        [Fact]
        [Category("PayloadGenerator/OneTimePassword")]
        public void one_time_password_generator_totp_with_8_digits()
        {
            var pg = new PayloadGenerator.OneTimePassword
            {
                Secret = "pwq6 5q55",
                Issuer = "Google",
                Label = "test@google.com",
                Digits = 8,
            };

            pg.ToString().ShouldBe("otpauth://totp/Google:test%40google.com?secret=pwq65q55&issuer=Google&digits=8");
        }

        [Fact]
        [Category("PayloadGenerator/OneTimePassword")]
        public void one_time_password_generator_hotp_with_8_digits()
        {
            var pg = new PayloadGenerator.OneTimePassword
            {
                Secret = "pwq6 5q55",
                Issuer = "Google",
                Label = "test@google.com",
                Type = PayloadGenerator.OneTimePassword.OneTimePasswordAuthType.HOTP,
                Counter = 250,
                Digits = 8,
            };

            pg.ToString().ShouldBe("otpauth://hotp/Google:test%40google.com?secret=pwq65q55&issuer=Google&digits=8&counter=250");
        }

        [Fact]
        [Category("PayloadGenerator/OneTimePassword")]
        public void one_time_password_generator_totp_with_custom_period()
        {
            var pg = new PayloadGenerator.OneTimePassword
            {
                Secret = "pwq6 5q55",
                Issuer = "Google",
                Label = "test@google.com",
                Period = 60,
            };

            pg.ToString().ShouldBe("otpauth://totp/Google:test%40google.com?secret=pwq65q55&issuer=Google&period=60");
        }

        [Fact]
        [Category("PayloadGenerator/OneTimePassword")]
        public void one_time_password_generator_totp_with_all_custom_parameters()
        {
            var pg = new PayloadGenerator.OneTimePassword
            {
                Secret = "pwq6 5q55",
                Issuer = "Google",
                Label = "test@google.com",
                AuthAlgorithm = PayloadGenerator.OneTimePassword.OneTimePasswordAuthAlgorithm.SHA512,
                Digits = 8,
                Period = 60,
            };

            pg.ToString().ShouldBe("otpauth://totp/Google:test%40google.com?secret=pwq65q55&issuer=Google&algorithm=SHA512&digits=8&period=60");
        }

        [Fact]
        [Category("PayloadGenerator/OneTimePassword")]
        public void one_time_password_generator_hotp_with_all_custom_parameters()
        {
            var pg = new PayloadGenerator.OneTimePassword
            {
                Secret = "pwq6 5q55",
                Issuer = "Google",
                Label = "test@google.com",
                Type = PayloadGenerator.OneTimePassword.OneTimePasswordAuthType.HOTP,
                Counter = 999,
                AuthAlgorithm = PayloadGenerator.OneTimePassword.OneTimePasswordAuthAlgorithm.SHA256,
                Digits = 7,
            };

            pg.ToString().ShouldBe("otpauth://hotp/Google:test%40google.com?secret=pwq65q55&issuer=Google&algorithm=SHA256&digits=7&counter=999");
        }

        [Fact]
        [Category("PayloadGenerator/OneTimePassword")]
        public void one_time_password_generator_hotp_with_default_counter()
        {
            var pg = new PayloadGenerator.OneTimePassword
            {
                Secret = "pwq6 5q55",
                Issuer = "Google",
                Label = "test@google.com",
                Type = PayloadGenerator.OneTimePassword.OneTimePasswordAuthType.HOTP,
                // Counter not set, should default to 1
            };

            pg.ToString().ShouldBe("otpauth://hotp/Google:test%40google.com?secret=pwq65q55&issuer=Google&counter=1");
        }

        [Fact]
        [Category("PayloadGenerator/OneTimePassword")]
        public void one_time_password_generator_totp_default_period_not_included()
        {
            var pg = new PayloadGenerator.OneTimePassword
            {
                Secret = "pwq6 5q55",
                Issuer = "Google",
                Label = "test@google.com",
                Period = 30, // Default value, should not be included in output
            };

            pg.ToString().ShouldBe("otpauth://totp/Google:test%40google.com?secret=pwq65q55&issuer=Google");
        }

        [Fact]
        [Category("PayloadGenerator/OneTimePassword")]
        public void one_time_password_generator_totp_default_digits_not_included()
        {
            var pg = new PayloadGenerator.OneTimePassword
            {
                Secret = "pwq6 5q55",
                Issuer = "Google",
                Label = "test@google.com",
                Digits = 6, // Default value, should not be included in output
            };

            pg.ToString().ShouldBe("otpauth://totp/Google:test%40google.com?secret=pwq65q55&issuer=Google");
        }

        [Fact]
        [Category("PayloadGenerator/OneTimePassword")]
        public void one_time_password_generator_totp_default_algorithm_not_included()
        {
            var pg = new PayloadGenerator.OneTimePassword
            {
                Secret = "pwq6 5q55",
                Issuer = "Google",
                Label = "test@google.com",
                AuthAlgorithm = PayloadGenerator.OneTimePassword.OneTimePasswordAuthAlgorithm.SHA1, // Default value, should not be included in output
            };

            pg.ToString().ShouldBe("otpauth://totp/Google:test%40google.com?secret=pwq65q55&issuer=Google");
        }

        [Fact]
        [Category("PayloadGenerator/OneTimePassword")]
        public void one_time_password_generator_should_throw_when_secret_is_null()
        {
            var pg = new PayloadGenerator.OneTimePassword
            {
                Secret = null!,
                Issuer = "Google",
                Label = "test@google.com",
            };

            Should.Throw<InvalidOperationException>(() => pg.ToString())
                .Message.ShouldBe("Secret must be a filled out base32 encoded string");
        }

        [Fact]
        [Category("PayloadGenerator/OneTimePassword")]
        public void one_time_password_generator_should_throw_when_secret_is_empty()
        {
            var pg = new PayloadGenerator.OneTimePassword
            {
                Secret = "",
                Issuer = "Google",
                Label = "test@google.com",
            };

            Should.Throw<InvalidOperationException>(() => pg.ToString())
                .Message.ShouldBe("Secret must be a filled out base32 encoded string");
        }

        [Fact]
        [Category("PayloadGenerator/OneTimePassword")]
        public void one_time_password_generator_should_throw_when_secret_is_whitespace()
        {
            var pg = new PayloadGenerator.OneTimePassword
            {
                Secret = "   ",
                Issuer = "Google",
                Label = "test@google.com",
            };

            Should.Throw<InvalidOperationException>(() => pg.ToString())
                .Message.ShouldBe("Secret must be a filled out base32 encoded string");
        }

        [Fact]
        [Category("PayloadGenerator/OneTimePassword")]
        public void one_time_password_generator_should_throw_when_issuer_contains_colon()
        {
            var pg = new PayloadGenerator.OneTimePassword
            {
                Secret = "pwq6 5q55",
                Issuer = "Google:Company",
                Label = "test@google.com",
            };

            // Fork message differs from upstream ("must not have a ':'")
            Should.Throw<InvalidOperationException>(() => pg.ToString())
                .Message.ShouldBe("Issuer must not contain ':'");
        }

        [Fact]
        [Category("PayloadGenerator/OneTimePassword")]
        public void one_time_password_generator_should_throw_when_label_contains_colon()
        {
            var pg = new PayloadGenerator.OneTimePassword
            {
                Secret = "pwq6 5q55",
                Issuer = "Google",
                Label = "test:user@google.com",
            };

            // Fork message differs from upstream ("must not have a ':'")
            Should.Throw<InvalidOperationException>(() => pg.ToString())
                .Message.ShouldBe("Label must not contain ':'");
        }

        [Fact]
        [Category("PayloadGenerator/OneTimePassword")]
        public void one_time_password_generator_totp_should_throw_when_period_is_null()
        {
            var pg = new PayloadGenerator.OneTimePassword
            {
                Secret = "pwq6 5q55",
                Issuer = "Google",
                Label = "test@google.com",
                Period = null,
            };

            // Fork message differs from upstream ("Period must be set when using OneTimePasswordAuthType.TOTP")
            Should.Throw<InvalidOperationException>(() => pg.ToString())
                .Message.ShouldBe("Period must be set for TOTP");
        }

        [Fact]
        [Category("PayloadGenerator/OneTimePassword")]
        public void one_time_password_generator_totp_with_only_issuer()
        {
            var pg = new PayloadGenerator.OneTimePassword
            {
                Secret = "pwq6 5q55",
                Issuer = "Google",
            };

            pg.ToString().ShouldBe("otpauth://totp/Google?secret=pwq65q55&issuer=Google");
        }

        [Fact]
        [Category("PayloadGenerator/OneTimePassword")]
        public void one_time_password_generator_hotp_with_only_issuer()
        {
            var pg = new PayloadGenerator.OneTimePassword
            {
                Secret = "pwq6 5q55",
                Issuer = "Google",
                Type = PayloadGenerator.OneTimePassword.OneTimePasswordAuthType.HOTP,
                Counter = 100,
            };

            pg.ToString().ShouldBe("otpauth://hotp/Google?secret=pwq65q55&issuer=Google&counter=100");
        }

        [Fact]
        [Category("PayloadGenerator/OneTimePassword")]
        public void one_time_password_generator_totp_with_only_label()
        {
            // Fork-fixed behavior: a set Label is emitted even when Issuer is null.
            // Upstream silently drops the label in this case.
            var pg = new PayloadGenerator.OneTimePassword
            {
                Secret = "pwq6 5q55",
                Label = "test@google.com",
            };

            pg.ToString().ShouldBe("otpauth://totp/test%40google.com?secret=pwq65q55");
        }

        [Fact]
        [Category("PayloadGenerator/OneTimePassword")]
        public void one_time_password_generator_hotp_with_only_label()
        {
            // Fork-fixed behavior: a set Label is emitted even when Issuer is null.
            // Upstream silently drops the label in this case.
            var pg = new PayloadGenerator.OneTimePassword
            {
                Secret = "pwq6 5q55",
                Label = "test@google.com",
                Type = PayloadGenerator.OneTimePassword.OneTimePasswordAuthType.HOTP,
                Counter = 42,
            };

            pg.ToString().ShouldBe("otpauth://hotp/test%40google.com?secret=pwq65q55&counter=42");
        }

        [Fact]
        [Category("PayloadGenerator/OneTimePassword")]
        public void one_time_password_generator_totp_with_no_issuer_or_label()
        {
            var pg = new PayloadGenerator.OneTimePassword
            {
                Secret = "pwq6 5q55",
            };

            pg.ToString().ShouldBe("otpauth://totp/?secret=pwq65q55");
        }

        [Fact]
        [Category("PayloadGenerator/OneTimePassword")]
        public void one_time_password_generator_hotp_with_no_issuer_or_label()
        {
            var pg = new PayloadGenerator.OneTimePassword
            {
                Secret = "pwq6 5q55",
                Type = PayloadGenerator.OneTimePassword.OneTimePasswordAuthType.HOTP,
                Counter = 50,
            };

            pg.ToString().ShouldBe("otpauth://hotp/?secret=pwq65q55&counter=50");
        }

        [Fact]
        [Category("PayloadGenerator/OneTimePassword")]
        public void one_time_password_generator_hotp_with_zero_counter()
        {
            var pg = new PayloadGenerator.OneTimePassword
            {
                Secret = "pwq6 5q55",
                Issuer = "Google",
                Label = "test@google.com",
                Type = PayloadGenerator.OneTimePassword.OneTimePasswordAuthType.HOTP,
                Counter = 0,
            };

            pg.ToString().ShouldBe("otpauth://hotp/Google:test%40google.com?secret=pwq65q55&issuer=Google&counter=0");
        }

        [Fact]
        [Category("PayloadGenerator/OneTimePassword")]
        public void one_time_password_generator_hotp_with_sha512_algorithm()
        {
            var pg = new PayloadGenerator.OneTimePassword
            {
                Secret = "pwq6 5q55",
                Issuer = "Google",
                Label = "test@google.com",
                Type = PayloadGenerator.OneTimePassword.OneTimePasswordAuthType.HOTP,
                Counter = 200,
                AuthAlgorithm = PayloadGenerator.OneTimePassword.OneTimePasswordAuthAlgorithm.SHA512,
            };

            pg.ToString().ShouldBe("otpauth://hotp/Google:test%40google.com?secret=pwq65q55&issuer=Google&algorithm=SHA512&counter=200");
        }

        [Fact]
        [Category("PayloadGenerator/OneTimePassword")]
        public void one_time_password_generator_should_strip_spaces_from_secret()
        {
            var pg = new PayloadGenerator.OneTimePassword
            {
                Secret = "pwq6 5q55 abcd efgh",
                Issuer = "Google",
                Label = "test@google.com",
            };

            pg.ToString().ShouldBe("otpauth://totp/Google:test%40google.com?secret=pwq65q55abcdefgh&issuer=Google");
        }

        [Fact]
        [Category("PayloadGenerator/OneTimePassword")]
        public void one_time_password_generator_totp_with_special_characters_in_label()
        {
            var pg = new PayloadGenerator.OneTimePassword
            {
                Secret = "pwq6 5q55",
                Issuer = "Google",
                Label = "test+user@google.com",
            };

            pg.ToString().ShouldBe("otpauth://totp/Google:test%2Buser%40google.com?secret=pwq65q55&issuer=Google");
        }

        [Fact]
        [Category("PayloadGenerator/OneTimePassword")]
        public void one_time_password_generator_totp_with_special_characters_in_issuer()
        {
            var pg = new PayloadGenerator.OneTimePassword
            {
                Secret = "pwq6 5q55",
                Issuer = "Google & Company",
                Label = "test@google.com",
            };

            pg.ToString().ShouldBe("otpauth://totp/Google%20%26%20Company:test%40google.com?secret=pwq65q55&issuer=Google%20%26%20Company");
        }

        [Fact]
        [Category("PayloadGenerator/OneTimePassword")]
        public void one_time_password_generator_hotp_with_large_counter()
        {
            var pg = new PayloadGenerator.OneTimePassword
            {
                Secret = "pwq6 5q55",
                Issuer = "Google",
                Label = "test@google.com",
                Type = PayloadGenerator.OneTimePassword.OneTimePasswordAuthType.HOTP,
                Counter = 999999999,
            };

            pg.ToString().ShouldBe("otpauth://hotp/Google:test%40google.com?secret=pwq65q55&issuer=Google&counter=999999999");
        }

        [Fact]
        [Category("PayloadGenerator/OneTimePassword")]
        public void one_time_password_generator_totp_with_large_period()
        {
            var pg = new PayloadGenerator.OneTimePassword
            {
                Secret = "pwq6 5q55",
                Issuer = "Google",
                Label = "test@google.com",
                Period = 3600,
            };

            pg.ToString().ShouldBe("otpauth://totp/Google:test%40google.com?secret=pwq65q55&issuer=Google&period=3600");
        }

        [Fact]
        [Category("PayloadGenerator/OneTimePassword")]
        public void one_time_password_generator_totp_with_large_digits()
        {
            var pg = new PayloadGenerator.OneTimePassword
            {
                Secret = "pwq6 5q55",
                Issuer = "Google",
                Label = "test@google.com",
                Digits = 10,
            };

            pg.ToString().ShouldBe("otpauth://totp/Google:test%40google.com?secret=pwq65q55&issuer=Google&digits=10");
        }
    }
}
