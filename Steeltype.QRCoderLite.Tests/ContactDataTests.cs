using System.ComponentModel;
using System.Globalization;
using Shouldly;
using Xunit;

namespace Steeltype.QRCoderLite.Tests
{

    public class ContactDataTests
    {

        [Fact]
        [Category("PayloadGenerator/ContactData")]
        public void contactdata_generator_can_generate_payload_simple_mecard()
        {
            var firstname = "John";
            var lastname = "Doe";
            var outputType = PayloadGenerator.ContactData.ContactOutputType.MeCard;

            var generator = new PayloadGenerator.ContactData(outputType, firstname, lastname);

            generator
                .ToString()
                .ShouldBe("MECARD+\r\nN:Doe, John\r\nADR:,,,,,,");
        }

        [Fact]
        [Category("PayloadGenerator/ContactData")]
        public void contactdata_generator_can_generate_payload_full_mecard()
        {
            var firstname = "John";
            var lastname = "Doe";
            var nickname = "Johnny";
            var org = "Johnny's Badass Programming";
            var orgTitle = "Badass Manager";
            var phone = "+4253212222";
            var mobilePhone = "+421701234567";
            var workPhone = "+4253211337";
            var email = "me@john.doe";
            var birthday = new DateTime(1970, 02, 01);
            var website = "http://john.doe";
            var street = "Long street";
            var houseNumber = "42";
            var city = "Super-Town";
            var zipCode = "12345";
            var country = "Starlight Country";
            var note = "Badass programmer.";
            var outputType = PayloadGenerator.ContactData.ContactOutputType.MeCard;

            var generator = new PayloadGenerator.ContactData(outputType, firstname, lastname, nickname, phone, mobilePhone, workPhone, email, birthday, website, street, houseNumber, city, zipCode, country, note, org: org, orgTitle: orgTitle);

            // ADR slot order per RFC 2426 sec. 3.2.1: PO Box; Extended; Street; City; Region; Zip; Country
            generator
                .ToString()
                .ShouldBe("MECARD+\r\nN:Doe, John\r\nORG:Johnny's Badass Programming\r\nTITLE:Badass Manager\r\nTEL:+4253212222\r\nTEL:+421701234567\r\nTEL:+4253211337\r\nEMAIL:me@john.doe\r\nNOTE:Badass programmer.\r\nBDAY:19700201\r\nADR:,,Long street 42,Super-Town,,12345,Starlight Country\r\nURL:http://john.doe\r\nNICKNAME:Johnny");
        }

        [Fact]
        [Category("PayloadGenerator/ContactData")]
        public void contactdata_generator_can_generate_payload_full_mecard_reversed()
        {
            var firstname = "John";
            var lastname = "Doe";
            var nickname = "Johnny";
            var org = "Johnny's Badass Programming";
            var orgTitle = "Badass Manager";
            var phone = "+4253212222";
            var mobilePhone = "+421701234567";
            var workPhone = "+4253211337";
            var email = "me@john.doe";
            var birthday = new DateTime(1970, 02, 01);
            var website = "http://john.doe";
            var street = "Long street";
            var houseNumber = "42";
            var city = "Super-Town";
            var zipCode = "12345";
            var country = "Starlight Country";
            var note = "Badass programmer.";
            var outputType = PayloadGenerator.ContactData.ContactOutputType.MeCard;

            var generator = new PayloadGenerator.ContactData(outputType, firstname, lastname, nickname, phone, mobilePhone, workPhone, email, birthday, website, street, houseNumber, city, zipCode, country, note, addressOrder: PayloadGenerator.ContactData.AddressOrder.Reversed, org: org, orgTitle: orgTitle);

            generator
                .ToString()
                .ShouldBe("MECARD+\r\nN:Doe, John\r\nORG:Johnny's Badass Programming\r\nTITLE:Badass Manager\r\nTEL:+4253212222\r\nTEL:+421701234567\r\nTEL:+4253211337\r\nEMAIL:me@john.doe\r\nNOTE:Badass programmer.\r\nBDAY:19700201\r\nADR:,,42 Long street,Super-Town,,12345,Starlight Country\r\nURL:http://john.doe\r\nNICKNAME:Johnny");
        }

        [Fact]
        [Category("PayloadGenerator/ContactData")]
        public void contactdata_generator_can_generate_payload_full_vcard21()
        {
            var firstname = "John";
            var lastname = "Doe";
            var nickname = "Johnny";
            var org = "Johnny's Badass Programming";
            var orgTitle = "Badass Manager";
            var phone = "+4253212222";
            var mobilePhone = "+421701234567";
            var workPhone = "+4253211337";
            var email = "me@john.doe";
            var birthday = new DateTime(1970, 02, 01);
            var website = "http://john.doe";
            var street = "Long street";
            var houseNumber = "42";
            var city = "Super-Town";
            var zipCode = "12345";
            var country = "Starlight Country";
            var note = "Badass programmer.";
            var outputType = PayloadGenerator.ContactData.ContactOutputType.VCard21;

            var generator = new PayloadGenerator.ContactData(outputType, firstname, lastname, nickname, phone, mobilePhone, workPhone, email, birthday, website, street, houseNumber, city, zipCode, country, note, org: org, orgTitle: orgTitle);

            generator
                .ToString()
                .ShouldBe("BEGIN:VCARD\r\nVERSION:2.1\r\nN:Doe;John;;;\r\nFN:John Doe\r\nORG:Johnny's Badass Programming\r\nTITLE:Badass Manager\r\nTEL;HOME;VOICE:+4253212222\r\nTEL;HOME;CELL:+421701234567\r\nTEL;WORK;VOICE:+4253211337\r\nADR;HOME;PREF:;;Long street 42;Super-Town;;12345;Starlight Country\r\nBDAY:19700201\r\nURL:http://john.doe\r\nEMAIL:me@john.doe\r\nNOTE:Badass programmer.\r\nEND:VCARD");
        }

        [Fact]
        [Category("PayloadGenerator/ContactData")]
        public void contactdata_generator_can_generate_payload_full_vcard3()
        {
            var firstname = "John";
            var lastname = "Doe";
            var nickname = "Johnny";
            var org = "Johnny's Badass Programming";
            var orgTitle = "Badass Manager";
            var phone = "+4253212222";
            var mobilePhone = "+421701234567";
            var workPhone = "+4253211337";
            var email = "me@john.doe";
            var birthday = new DateTime(1970, 02, 01);
            var website = "http://john.doe";
            var street = "Long street";
            var houseNumber = "42";
            var city = "Super-Town";
            var zipCode = "12345";
            var country = "Starlight Country";
            var note = "Badass programmer.";
            var outputType = PayloadGenerator.ContactData.ContactOutputType.VCard3;

            var generator = new PayloadGenerator.ContactData(outputType, firstname, lastname, nickname, phone, mobilePhone, workPhone, email, birthday, website, street, houseNumber, city, zipCode, country, note, org: org, orgTitle: orgTitle);

            generator
                .ToString()
                .ShouldBe("BEGIN:VCARD\r\nVERSION:3.0\r\nN:Doe;John;;;\r\nFN:John Doe\r\nORG:Johnny's Badass Programming\r\nTITLE:Badass Manager\r\nTEL;TYPE=HOME,VOICE:+4253212222\r\nTEL;TYPE=HOME,CELL:+421701234567\r\nTEL;TYPE=WORK,VOICE:+4253211337\r\nADR;TYPE=HOME,PREF:;;Long street 42;Super-Town;;12345;Starlight Country\r\nBDAY:19700201\r\nURL:http://john.doe\r\nEMAIL:me@john.doe\r\nNOTE:Badass programmer.\r\nNICKNAME:Johnny\r\nEND:VCARD");
        }

        [Fact]
        [Category("PayloadGenerator/ContactData")]
        public void contactdata_generator_can_generate_payload_full_vcard4()
        {
            var firstname = "John";
            var lastname = "Doe";
            var nickname = "Johnny";
            var org = "Johnny's Badass Programming";
            var orgTitle = "Badass Manager";
            var phone = "+4253212222";
            var mobilePhone = "+421701234567";
            var workPhone = "+4253211337";
            var email = "me@john.doe";
            var birthday = new DateTime(1970, 02, 01);
            var website = "http://john.doe";
            var street = "Long street";
            var houseNumber = "42";
            var city = "Super-Town";
            var zipCode = "12345";
            var country = "Starlight Country";
            var note = "Badass programmer.";
            var outputType = PayloadGenerator.ContactData.ContactOutputType.VCard4;

            var generator = new PayloadGenerator.ContactData(outputType, firstname, lastname, nickname, phone, mobilePhone, workPhone, email, birthday, website, street, houseNumber, city, zipCode, country, note, org: org, orgTitle: orgTitle);

            generator
                .ToString()
                .ShouldBe("BEGIN:VCARD\r\nVERSION:4.0\r\nN:Doe;John;;;\r\nFN:John Doe\r\nORG:Johnny's Badass Programming\r\nTITLE:Badass Manager\r\nTEL;TYPE=home,voice;VALUE=uri:tel:+4253212222\r\nTEL;TYPE=home,cell;VALUE=uri:tel:+421701234567\r\nTEL;TYPE=work,voice;VALUE=uri:tel:+4253211337\r\nADR;TYPE=home,pref:;;Long street 42;Super-Town;;12345;Starlight Country\r\nBDAY:19700201\r\nURL:http://john.doe\r\nEMAIL:me@john.doe\r\nNOTE:Badass programmer.\r\nNICKNAME:Johnny\r\nEND:VCARD");
        }

        [Fact]
        [Category("PayloadGenerator/ContactData")]
        public void contactdata_generator_can_generate_payload_full_vcard4_reverse()
        {
            var firstname = "John";
            var lastname = "Doe";
            var nickname = "Johnny";
            var org = "Johnny's Badass Programming";
            var orgTitle = "Badass Manager";
            var phone = "+4253212222";
            var mobilePhone = "+421701234567";
            var workPhone = "+4253211337";
            var email = "me@john.doe";
            var birthday = new DateTime(1970, 02, 01);
            var website = "http://john.doe";
            var street = "Long street";
            var houseNumber = "42";
            var city = "Super-Town";
            var zipCode = "12345";
            var country = "Starlight Country";
            var note = "Badass programmer.";
            var outputType = PayloadGenerator.ContactData.ContactOutputType.VCard4;

            var generator = new PayloadGenerator.ContactData(outputType, firstname, lastname, nickname, phone, mobilePhone, workPhone, email, birthday, website, street, houseNumber, city, zipCode, country, note, addressOrder: PayloadGenerator.ContactData.AddressOrder.Reversed, org: org, orgTitle: orgTitle);

            generator
                .ToString()
                .ShouldBe("BEGIN:VCARD\r\nVERSION:4.0\r\nN:Doe;John;;;\r\nFN:John Doe\r\nORG:Johnny's Badass Programming\r\nTITLE:Badass Manager\r\nTEL;TYPE=home,voice;VALUE=uri:tel:+4253212222\r\nTEL;TYPE=home,cell;VALUE=uri:tel:+421701234567\r\nTEL;TYPE=work,voice;VALUE=uri:tel:+4253211337\r\nADR;TYPE=home,pref:;;42 Long street;Super-Town;;12345;Starlight Country\r\nBDAY:19700201\r\nURL:http://john.doe\r\nEMAIL:me@john.doe\r\nNOTE:Badass programmer.\r\nNICKNAME:Johnny\r\nEND:VCARD");
        }

        // Upstream has AddressType (Home/Work/...) tests here. The fork has no AddressType
        // parameter; every vCard address is emitted as the HOME + preferred address. These
        // three tests pin that fork behavior per vCard version.
        [Fact]
        [Category("PayloadGenerator/ContactData")]
        public void contactdata_generator_emits_home_pref_address_vcard21()
        {
            var outputType = PayloadGenerator.ContactData.ContactOutputType.VCard21;

            var generator = new PayloadGenerator.ContactData(
                outputType, "John", "Doe",
                street: "Office Boulevard", houseNumber: "100", city: "Business City",
                zipCode: "54321", country: "Workland");

            generator
                .ToString()
                .ShouldBe("BEGIN:VCARD\r\nVERSION:2.1\r\nN:Doe;John;;;\r\nFN:John Doe\r\nADR;HOME;PREF:;;Office Boulevard 100;Business City;;54321;Workland\r\nEND:VCARD");
        }

        [Fact]
        [Category("PayloadGenerator/ContactData")]
        public void contactdata_generator_emits_home_pref_address_vcard3()
        {
            var outputType = PayloadGenerator.ContactData.ContactOutputType.VCard3;

            var generator = new PayloadGenerator.ContactData(
                outputType, "John", "Doe",
                street: "Office Boulevard", houseNumber: "100", city: "Business City",
                zipCode: "54321", country: "Workland");

            generator
                .ToString()
                .ShouldBe("BEGIN:VCARD\r\nVERSION:3.0\r\nN:Doe;John;;;\r\nFN:John Doe\r\nADR;TYPE=HOME,PREF:;;Office Boulevard 100;Business City;;54321;Workland\r\nEND:VCARD");
        }

        [Fact]
        [Category("PayloadGenerator/ContactData")]
        public void contactdata_generator_emits_home_pref_address_vcard4()
        {
            var outputType = PayloadGenerator.ContactData.ContactOutputType.VCard4;

            var generator = new PayloadGenerator.ContactData(
                outputType, "Jane", "Smith",
                street: "Corporate Street", houseNumber: "200", city: "Metro City",
                zipCode: "98765", country: "Businessland");

            generator
                .ToString()
                .ShouldBe("BEGIN:VCARD\r\nVERSION:4.0\r\nN:Smith;Jane;;;\r\nFN:Jane Smith\r\nADR;TYPE=home,pref:;;Corporate Street 200;Metro City;;98765;Businessland\r\nEND:VCARD");
        }

        // These three tests populate stateRegion so a shifted ADR slot order (the pre-fix
        // bug put city in the region slot) cannot pass unnoticed behind empty fields.
        [Fact]
        [Category("PayloadGenerator/ContactData")]
        public void contactdata_generator_fills_region_slot_mecard()
        {
            var outputType = PayloadGenerator.ContactData.ContactOutputType.MeCard;

            var generator = new PayloadGenerator.ContactData(
                outputType, "John", "Doe",
                street: "Main Street", houseNumber: "1", city: "Springfield",
                zipCode: "62704", country: "USA", stateRegion: "IL");

            generator
                .ToString()
                .ShouldBe("MECARD+\r\nN:Doe, John\r\nADR:,,Main Street 1,Springfield,IL,62704,USA");
        }

        [Fact]
        [Category("PayloadGenerator/ContactData")]
        public void contactdata_generator_fills_region_slot_mecard_reversed()
        {
            var outputType = PayloadGenerator.ContactData.ContactOutputType.MeCard;

            var generator = new PayloadGenerator.ContactData(
                outputType, "John", "Doe",
                street: "Main Street", houseNumber: "1", city: "Springfield",
                zipCode: "62704", country: "USA", stateRegion: "IL",
                addressOrder: PayloadGenerator.ContactData.AddressOrder.Reversed);

            // Reversed only swaps house number and street; city/region/zip/country slots keep RFC 2426 order.
            generator
                .ToString()
                .ShouldBe("MECARD+\r\nN:Doe, John\r\nADR:,,1 Main Street,Springfield,IL,62704,USA");
        }

        [Fact]
        [Category("PayloadGenerator/ContactData")]
        public void contactdata_generator_fills_region_slot_vcard3()
        {
            var outputType = PayloadGenerator.ContactData.ContactOutputType.VCard3;

            var generator = new PayloadGenerator.ContactData(
                outputType, "John", "Doe",
                street: "Main Street", houseNumber: "1", city: "Springfield",
                zipCode: "62704", country: "USA", stateRegion: "IL");

            generator
                .ToString()
                .ShouldBe("BEGIN:VCARD\r\nVERSION:3.0\r\nN:Doe;John;;;\r\nFN:John Doe\r\nADR;TYPE=HOME,PREF:;;Main Street 1;Springfield;IL;62704;USA\r\nEND:VCARD");
        }

        [Fact]
        [Category("PayloadGenerator/ContactData")]
        public void contactdata_generator_emits_nickname_after_address_mecard()
        {
            var outputType = PayloadGenerator.ContactData.ContactOutputType.MeCard;

            var generator = new PayloadGenerator.ContactData(outputType, "John", "Doe", nickname: "Johnny");

            generator
                .ToString()
                .ShouldBe("MECARD+\r\nN:Doe, John\r\nADR:,,,,,,\r\nNICKNAME:Johnny");
        }

        [Fact]
        [Category("PayloadGenerator/ContactData")]
        public void contactdata_generator_omits_nickname_for_vcard21_but_emits_for_vcard3()
        {
            // vCard 2.1 has no NICKNAME property, so the fork drops it there but keeps it for 3.0+.
            var vcard21 = new PayloadGenerator.ContactData(PayloadGenerator.ContactData.ContactOutputType.VCard21, "John", "Doe", nickname: "Johnny");
            var vcard3 = new PayloadGenerator.ContactData(PayloadGenerator.ContactData.ContactOutputType.VCard3, "John", "Doe", nickname: "Johnny");

            vcard21.ToString().ShouldNotContain("NICKNAME");
            vcard3.ToString().ShouldContain("NICKNAME:Johnny");
        }

        [Fact]
        [Category("PayloadGenerator/ContactData")]
        public void contactdata_generator_omits_bday_when_no_birthday_mecard()
        {
            var outputType = PayloadGenerator.ContactData.ContactOutputType.MeCard;

            var generator = new PayloadGenerator.ContactData(outputType, "John", "Doe");

            generator.ToString().ShouldNotContain("BDAY");
        }

        [Fact]
        [Category("PayloadGenerator/ContactData")]
        public void contactdata_generator_omits_bday_when_no_birthday_vcard4()
        {
            var outputType = PayloadGenerator.ContactData.ContactOutputType.VCard4;

            var generator = new PayloadGenerator.ContactData(outputType, "John", "Doe");

            generator.ToString().ShouldNotContain("BDAY");
        }

        [Fact]
        [Category("PayloadGenerator/ContactData")]
        public void contactdata_generator_bday_stays_gregorian_under_thai_buddhist_culture()
        {
            var originalCulture = CultureInfo.CurrentCulture;
            var originalUiCulture = CultureInfo.CurrentUICulture;
            try
            {
                // Force the Thai Buddhist calendar so culture-sensitive date formatting
                // would yield year 2513 instead of 1970 - regardless of platform defaults.
                var thaiCulture = new CultureInfo("th-TH");
                thaiCulture.DateTimeFormat.Calendar = new ThaiBuddhistCalendar();
                CultureInfo.CurrentCulture = thaiCulture;
                CultureInfo.CurrentUICulture = thaiCulture;

                var birthday = new DateTime(1970, 02, 01);

                // Sanity check: culture-sensitive formatting really is Buddhist under this culture.
                birthday.ToString("yyyyMMdd").ShouldBe("25130201");

                var mecard = new PayloadGenerator.ContactData(PayloadGenerator.ContactData.ContactOutputType.MeCard, "John", "Doe", birthday: birthday);
                var vcard4 = new PayloadGenerator.ContactData(PayloadGenerator.ContactData.ContactOutputType.VCard4, "John", "Doe", birthday: birthday);

                // BDAY must stay Gregorian (invariant culture), not the Buddhist year 2513.
                mecard.ToString().ShouldContain("BDAY:19700201");
                vcard4.ToString().ShouldContain("BDAY:19700201");
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
                CultureInfo.CurrentUICulture = originalUiCulture;
            }
        }
    }
}
