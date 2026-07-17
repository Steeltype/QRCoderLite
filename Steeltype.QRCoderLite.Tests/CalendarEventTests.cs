using System.ComponentModel;
using System.Globalization;
using Shouldly;
using Xunit;

namespace Steeltype.QRCoderLite.Tests
{

    public class CalendarEventTests
    {

        [Fact]
        [Category("PayloadGenerator/CalendarEvent")]
        public void calendarevent_should_build_universal()
        {
            var subject = "Release party";
            var description = "A small party for the new QRCoder. Bring some beer!";
            var location = "Programmer's paradise, Beachtown, Paradise";
            var alldayEvent = false;
            var begin = new DateTime(2016, 01, 03, 12, 00, 00);
            var end = new DateTime(2016, 01, 03, 14, 30, 00);
            var encoding = PayloadGenerator.CalendarEvent.EventEncoding.Universal;

            var generator = new PayloadGenerator.CalendarEvent(subject, description, location, begin, end, alldayEvent, encoding);

            generator.ToString().ShouldBe($"BEGIN:VEVENT{Environment.NewLine}SUMMARY:Release party{Environment.NewLine}DESCRIPTION:A small party for the new QRCoder. Bring some beer!{Environment.NewLine}LOCATION:Programmer's paradise, Beachtown, Paradise{Environment.NewLine}DTSTART:20160103T120000{Environment.NewLine}DTEND:20160103T143000{Environment.NewLine}END:VEVENT");
        }

        [Fact]
        [Category("PayloadGenerator/CalendarEvent")]
        public void calendarevent_should_build_ical()
        {
            var subject = "Release party";
            var description = "A small party for the new QRCoder. Bring some beer!";
            var location = "Programmer's paradise, Beachtown, Paradise";
            var alldayEvent = false;
            var begin = new DateTime(2016, 01, 03, 12, 00, 00);
            var end = new DateTime(2016, 01, 03, 14, 30, 00);
            var encoding = PayloadGenerator.CalendarEvent.EventEncoding.iCalComplete;

            var generator = new PayloadGenerator.CalendarEvent(subject, description, location, begin, end, alldayEvent, encoding);

            generator.ToString().ShouldBe($"BEGIN:VCALENDAR{Environment.NewLine}VERSION:2.0{Environment.NewLine}BEGIN:VEVENT{Environment.NewLine}SUMMARY:Release party{Environment.NewLine}DESCRIPTION:A small party for the new QRCoder. Bring some beer!{Environment.NewLine}LOCATION:Programmer's paradise, Beachtown, Paradise{Environment.NewLine}DTSTART:20160103T120000{Environment.NewLine}DTEND:20160103T143000{Environment.NewLine}END:VEVENT{Environment.NewLine}END:VCALENDAR");
        }

        [Fact]
        [Category("PayloadGenerator/CalendarEvent")]
        public void calendarevent_should_build_universal_with_utc_datetime()
        {
            // DateTimeKind.Utc times must carry the iCalendar 'Z' suffix,
            // otherwise consumers decode them as floating local time.
            var subject = "Release party";
            var description = "A small party for the new QRCoder. Bring some beer!";
            var location = "Programmer's paradise, Beachtown, Paradise";
            var alldayEvent = false;
            var begin = new DateTime(2016, 01, 03, 12, 00, 00, DateTimeKind.Utc);
            var end = new DateTime(2016, 01, 03, 14, 30, 00, DateTimeKind.Utc);
            var encoding = PayloadGenerator.CalendarEvent.EventEncoding.Universal;

            var generator = new PayloadGenerator.CalendarEvent(subject, description, location, begin, end, alldayEvent, encoding);

            generator.ToString().ShouldBe($"BEGIN:VEVENT{Environment.NewLine}SUMMARY:Release party{Environment.NewLine}DESCRIPTION:A small party for the new QRCoder. Bring some beer!{Environment.NewLine}LOCATION:Programmer's paradise, Beachtown, Paradise{Environment.NewLine}DTSTART:20160103T120000Z{Environment.NewLine}DTEND:20160103T143000Z{Environment.NewLine}END:VEVENT");
        }

        [Fact]
        [Category("PayloadGenerator/CalendarEvent")]
        public void calendarevent_should_build_ical_with_utc_datetime()
        {
            var subject = "Release party";
            var description = "A small party for the new QRCoder. Bring some beer!";
            var location = "Programmer's paradise, Beachtown, Paradise";
            var alldayEvent = false;
            var begin = new DateTime(2016, 01, 03, 12, 00, 00, DateTimeKind.Utc);
            var end = new DateTime(2016, 01, 03, 14, 30, 00, DateTimeKind.Utc);
            var encoding = PayloadGenerator.CalendarEvent.EventEncoding.iCalComplete;

            var generator = new PayloadGenerator.CalendarEvent(subject, description, location, begin, end, alldayEvent, encoding);

            generator.ToString().ShouldBe($"BEGIN:VCALENDAR{Environment.NewLine}VERSION:2.0{Environment.NewLine}BEGIN:VEVENT{Environment.NewLine}SUMMARY:Release party{Environment.NewLine}DESCRIPTION:A small party for the new QRCoder. Bring some beer!{Environment.NewLine}LOCATION:Programmer's paradise, Beachtown, Paradise{Environment.NewLine}DTSTART:20160103T120000Z{Environment.NewLine}DTEND:20160103T143000Z{Environment.NewLine}END:VEVENT{Environment.NewLine}END:VCALENDAR");
        }

        [Fact]
        [Category("PayloadGenerator/CalendarEvent")]
        public void calendarevent_should_not_add_z_for_local_datetime()
        {
            // Only DateTimeKind.Utc gets the 'Z' suffix; Local (like Unspecified)
            // is emitted as floating local time without conversion.
            var subject = "Release party";
            var description = "A small party for the new QRCoder. Bring some beer!";
            var location = "Programmer's paradise, Beachtown, Paradise";
            var alldayEvent = false;
            var begin = new DateTime(2016, 01, 03, 12, 00, 00, DateTimeKind.Local);
            var end = new DateTime(2016, 01, 03, 14, 30, 00, DateTimeKind.Local);
            var encoding = PayloadGenerator.CalendarEvent.EventEncoding.Universal;

            var generator = new PayloadGenerator.CalendarEvent(subject, description, location, begin, end, alldayEvent, encoding);

            generator.ToString().ShouldBe($"BEGIN:VEVENT{Environment.NewLine}SUMMARY:Release party{Environment.NewLine}DESCRIPTION:A small party for the new QRCoder. Bring some beer!{Environment.NewLine}LOCATION:Programmer's paradise, Beachtown, Paradise{Environment.NewLine}DTSTART:20160103T120000{Environment.NewLine}DTEND:20160103T143000{Environment.NewLine}END:VEVENT");
        }

        [Fact]
        [Category("PayloadGenerator/CalendarEvent")]
        public void calendarevent_should_build_allday()
        {
            var subject = "Release party";
            var description = "A small party for the new QRCoder. Bring some beer!";
            var location = "Programmer's paradise, Beachtown, Paradise";
            var alldayEvent = true;
            var begin = new DateTime(2016, 01, 03);
            var end = new DateTime(2016, 01, 03);
            var encoding = PayloadGenerator.CalendarEvent.EventEncoding.Universal;

            var generator = new PayloadGenerator.CalendarEvent(subject, description, location, begin, end, alldayEvent, encoding);

            generator.ToString().ShouldBe($"BEGIN:VEVENT{Environment.NewLine}SUMMARY:Release party{Environment.NewLine}DESCRIPTION:A small party for the new QRCoder. Bring some beer!{Environment.NewLine}LOCATION:Programmer's paradise, Beachtown, Paradise{Environment.NewLine}DTSTART:20160103{Environment.NewLine}DTEND:20160103{Environment.NewLine}END:VEVENT");
        }

        [Fact]
        [Category("PayloadGenerator/CalendarEvent")]
        public void calendarevent_should_build_allday_without_z_for_utc_datetime()
        {
            // All-day events use the date-only format; the 'Z' suffix never
            // applies because DTSTART/DTEND carry no time component.
            var subject = "Release party";
            var description = "A small party for the new QRCoder. Bring some beer!";
            var location = "Programmer's paradise, Beachtown, Paradise";
            var alldayEvent = true;
            var begin = new DateTime(2016, 01, 03, 00, 00, 00, DateTimeKind.Utc);
            var end = new DateTime(2016, 01, 03, 00, 00, 00, DateTimeKind.Utc);
            var encoding = PayloadGenerator.CalendarEvent.EventEncoding.Universal;

            var generator = new PayloadGenerator.CalendarEvent(subject, description, location, begin, end, alldayEvent, encoding);

            generator.ToString().ShouldBe($"BEGIN:VEVENT{Environment.NewLine}SUMMARY:Release party{Environment.NewLine}DESCRIPTION:A small party for the new QRCoder. Bring some beer!{Environment.NewLine}LOCATION:Programmer's paradise, Beachtown, Paradise{Environment.NewLine}DTSTART:20160103{Environment.NewLine}DTEND:20160103{Environment.NewLine}END:VEVENT");
        }

        [Fact]
        [Category("PayloadGenerator/CalendarEvent")]
        public void calendarevent_should_care_empty_fields()
        {
            var subject = "Release party";
            var description = "";
            var location = string.Empty;
            var alldayEvent = false;
            var begin = new DateTime(2016, 01, 03, 12, 00, 00);
            var end = new DateTime(2016, 01, 03, 14, 30, 00);
            var encoding = PayloadGenerator.CalendarEvent.EventEncoding.Universal;

            var generator = new PayloadGenerator.CalendarEvent(subject, description, location, begin, end, alldayEvent, encoding);

            generator.ToString().ShouldBe($"BEGIN:VEVENT{Environment.NewLine}SUMMARY:Release party{Environment.NewLine}DTSTART:20160103T120000{Environment.NewLine}DTEND:20160103T143000{Environment.NewLine}END:VEVENT");
        }

        [Fact]
        [Category("PayloadGenerator/CalendarEvent")]
        public void calendarevent_should_add_unused_params()
        {
            // Omitting the encoding parameter defaults to Universal.
            var subject = "Release party";
            var description = "A small party for the new QRCoder. Bring some beer!";
            var location = "Programmer's paradise, Beachtown, Paradise";
            var alldayEvent = false;
            var begin = new DateTime(2016, 01, 03, 12, 00, 00);
            var end = new DateTime(2016, 01, 03, 14, 30, 00);

            var generator = new PayloadGenerator.CalendarEvent(subject, description, location, begin, end, alldayEvent);

            generator.ToString().ShouldBe($"BEGIN:VEVENT{Environment.NewLine}SUMMARY:Release party{Environment.NewLine}DESCRIPTION:A small party for the new QRCoder. Bring some beer!{Environment.NewLine}LOCATION:Programmer's paradise, Beachtown, Paradise{Environment.NewLine}DTSTART:20160103T120000{Environment.NewLine}DTEND:20160103T143000{Environment.NewLine}END:VEVENT");
        }

        [Fact]
        [Category("PayloadGenerator/CalendarEvent")]
        public void calendarevent_should_use_gregorian_calendar_under_thai_culture()
        {
            // th-TH defaults to the Thai Buddhist calendar (2016 CE = 2559 BE).
            // DTSTART/DTEND must stay Gregorian regardless of the current culture,
            // so the payload must be identical to the invariant-culture output.
            var previousCulture = CultureInfo.CurrentCulture;
            var previousUICulture = CultureInfo.CurrentUICulture;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("th-TH");
                CultureInfo.CurrentUICulture = new CultureInfo("th-TH");

                var subject = "Release party";
                var description = "A small party for the new QRCoder. Bring some beer!";
                var location = "Programmer's paradise, Beachtown, Paradise";
                var alldayEvent = false;
                var begin = new DateTime(2016, 01, 03, 12, 00, 00);
                var end = new DateTime(2016, 01, 03, 14, 30, 00);
                var encoding = PayloadGenerator.CalendarEvent.EventEncoding.Universal;

                var generator = new PayloadGenerator.CalendarEvent(subject, description, location, begin, end, alldayEvent, encoding);

                generator.ToString().ShouldBe($"BEGIN:VEVENT{Environment.NewLine}SUMMARY:Release party{Environment.NewLine}DESCRIPTION:A small party for the new QRCoder. Bring some beer!{Environment.NewLine}LOCATION:Programmer's paradise, Beachtown, Paradise{Environment.NewLine}DTSTART:20160103T120000{Environment.NewLine}DTEND:20160103T143000{Environment.NewLine}END:VEVENT");
            }
            finally
            {
                CultureInfo.CurrentCulture = previousCulture;
                CultureInfo.CurrentUICulture = previousUICulture;
            }
        }

        [Fact]
        [Category("PayloadGenerator/CalendarEvent")]
        public void calendarevent_should_build_allday_gregorian_under_thai_culture()
        {
            // The date-only all-day format must also resist the Buddhist calendar
            // (a culture-sensitive format would emit 25590103 instead of 20160103).
            var previousCulture = CultureInfo.CurrentCulture;
            var previousUICulture = CultureInfo.CurrentUICulture;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("th-TH");
                CultureInfo.CurrentUICulture = new CultureInfo("th-TH");

                var subject = "Release party";
                var description = "A small party for the new QRCoder. Bring some beer!";
                var location = "Programmer's paradise, Beachtown, Paradise";
                var alldayEvent = true;
                var begin = new DateTime(2016, 01, 03);
                var end = new DateTime(2016, 01, 03);
                var encoding = PayloadGenerator.CalendarEvent.EventEncoding.Universal;

                var generator = new PayloadGenerator.CalendarEvent(subject, description, location, begin, end, alldayEvent, encoding);

                generator.ToString().ShouldBe($"BEGIN:VEVENT{Environment.NewLine}SUMMARY:Release party{Environment.NewLine}DESCRIPTION:A small party for the new QRCoder. Bring some beer!{Environment.NewLine}LOCATION:Programmer's paradise, Beachtown, Paradise{Environment.NewLine}DTSTART:20160103{Environment.NewLine}DTEND:20160103{Environment.NewLine}END:VEVENT");
            }
            finally
            {
                CultureInfo.CurrentCulture = previousCulture;
                CultureInfo.CurrentUICulture = previousUICulture;
            }
        }
    }
}
