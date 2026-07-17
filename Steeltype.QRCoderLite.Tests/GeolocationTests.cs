using System.ComponentModel;
using System.Globalization;
using Shouldly;
using Xunit;

namespace Steeltype.QRCoderLite.Tests
{

    public class GeolocationTests
    {

        [Fact]
        [Category("PayloadGenerator/Geolocation")]
        public void geolocation_should_build_type_GEO()
        {
            var latitude = "51.227741";
            var longitude = "6.773456";
            var encoding = PayloadGenerator.Geolocation.GeolocationEncoding.GEO;

            var generator = new PayloadGenerator.Geolocation(latitude, longitude, encoding);

            generator.ToString().ShouldBe("geo:51.227741,6.773456");
        }

        [Fact]
        [Category("PayloadGenerator/Geolocation")]
        public void geolocation_should_build_type_GoogleMaps()
        {
            var latitude = "51.227741";
            var longitude = "6.773456";
            var encoding = PayloadGenerator.Geolocation.GeolocationEncoding.GoogleMaps;

            var generator = new PayloadGenerator.Geolocation(latitude, longitude, encoding);

            generator.ToString().ShouldBe("http://maps.google.com/maps?q=51.227741,6.773456");
        }

        [Fact]
        [Category("PayloadGenerator/Geolocation")]
        public void geolocation_should_escape_input()
        {
            var latitude = "51,227741";
            var longitude = "6,773456";
            var encoding = PayloadGenerator.Geolocation.GeolocationEncoding.GEO;

            var generator = new PayloadGenerator.Geolocation(latitude, longitude, encoding);

            generator.ToString().ShouldBe("geo:51.227741,6.773456");
        }

        [Fact]
        [Category("PayloadGenerator/Geolocation")]
        public void geolocation_should_add_unused_params()
        {
            var latitude = "51.227741";
            var longitude = "6.773456";

            var generator = new PayloadGenerator.Geolocation(latitude, longitude);

            generator.ToString().ShouldBe("geo:51.227741,6.773456");
        }

        [Fact]
        [Category("PayloadGenerator/Geolocation")]
        public void geolocation_should_accept_comma_decimals_for_googlemaps()
        {
            var latitude = "51,227741";
            var longitude = "6,773456";
            var encoding = PayloadGenerator.Geolocation.GeolocationEncoding.GoogleMaps;

            var generator = new PayloadGenerator.Geolocation(latitude, longitude, encoding);

            generator.ToString().ShouldBe("http://maps.google.com/maps?q=51.227741,6.773456");
        }

        [Fact]
        [Category("PayloadGenerator/Geolocation")]
        public void geolocation_should_reject_non_numeric_latitude()
        {
            var exception = Should.Throw<ArgumentOutOfRangeException>(
                () => new PayloadGenerator.Geolocation("not-a-number", "6.773456"));

            exception.ParamName.ShouldBe("latitude");
        }

        [Fact]
        [Category("PayloadGenerator/Geolocation")]
        public void geolocation_should_reject_non_numeric_longitude()
        {
            var exception = Should.Throw<ArgumentOutOfRangeException>(
                () => new PayloadGenerator.Geolocation("51.227741", "not-a-number"));

            exception.ParamName.ShouldBe("longitude");
        }

        [Theory]
        [Category("PayloadGenerator/Geolocation")]
        [InlineData("90.000001")]
        [InlineData("-90.000001")]
        [InlineData("91")]
        [InlineData("-91")]
        public void geolocation_should_reject_out_of_range_latitude(string latitude)
        {
            var exception = Should.Throw<ArgumentOutOfRangeException>(
                () => new PayloadGenerator.Geolocation(latitude, "6.773456"));

            exception.ParamName.ShouldBe("latitude");
        }

        [Theory]
        [Category("PayloadGenerator/Geolocation")]
        [InlineData("180.000001")]
        [InlineData("-180.000001")]
        [InlineData("181")]
        [InlineData("-181")]
        public void geolocation_should_reject_out_of_range_longitude(string longitude)
        {
            var exception = Should.Throw<ArgumentOutOfRangeException>(
                () => new PayloadGenerator.Geolocation("51.227741", longitude));

            exception.ParamName.ShouldBe("longitude");
        }

        [Theory]
        [Category("PayloadGenerator/Geolocation")]
        [InlineData("90", "180")]
        [InlineData("-90", "-180")]
        [InlineData("90", "-180")]
        [InlineData("-90", "180")]
        public void geolocation_should_accept_boundary_values(string latitude, string longitude)
        {
            var generator = new PayloadGenerator.Geolocation(latitude, longitude);

            generator.ToString().ShouldBe($"geo:{latitude},{longitude}");
        }

        [Theory]
        [Category("PayloadGenerator/Geolocation")]
        [InlineData(null, "6.773456")]
        [InlineData("", "6.773456")]
        [InlineData("51.227741", null)]
        [InlineData("51.227741", "   ")]
        public void geolocation_should_reject_null_or_empty_coordinates(string latitude, string longitude)
        {
            Should.Throw<ArgumentException>(
                () => new PayloadGenerator.Geolocation(latitude, longitude));
        }

        [Fact]
        [Category("PayloadGenerator/Geolocation")]
        public void geolocation_should_be_culture_invariant()
        {
            var currentCulture = CultureInfo.CurrentCulture;
            var currentUiCulture = CultureInfo.CurrentUICulture;
            try
            {
                // de-DE uses ',' as decimal separator; parsing/validation must stay invariant
                CultureInfo.CurrentCulture = new CultureInfo("de-DE");
                CultureInfo.CurrentUICulture = new CultureInfo("de-DE");

                var generator = new PayloadGenerator.Geolocation("51.227741", "6.773456");
                generator.ToString().ShouldBe("geo:51.227741,6.773456");

                // Out-of-range detection must also hold under a comma-decimal culture
                Should.Throw<ArgumentOutOfRangeException>(
                    () => new PayloadGenerator.Geolocation("90.5", "6.773456"));
            }
            finally
            {
                CultureInfo.CurrentCulture = currentCulture;
                CultureInfo.CurrentUICulture = currentUiCulture;
            }
        }
    }
}
