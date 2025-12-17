using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Steeltype.QRCoderLite
{
    public static class PayloadGenerator
    {
        public abstract class Payload
        {
            public virtual int Version => -1;
            public virtual QRCodeGenerator.ECCLevel EccLevel => QRCodeGenerator.ECCLevel.M;
            public virtual QRCodeGenerator.EciMode EciMode => QRCodeGenerator.EciMode.Default;
            public abstract override string ToString();
        }

        public class WiFi : Payload
        {
            private readonly string ssid, password, authenticationMode;
            private readonly bool isHiddenSsid;

            /// <summary>
            /// Generates a WiFi payload. Scanned by a QR Code scanner app, the device will connect to the WiFi.
            /// </summary>
            /// <param name="ssid">SSID of the WiFi network</param>
            /// <param name="password">Password of the WiFi network</param>
            /// <param name="authenticationMode">Authentication mode (WEP, WPA, WPA2)</param>
            /// <param name="isHiddenSSID">Set flag, if the WiFi network hides its SSID</param>
            /// <param name="escapeHexStrings">Set flag, if ssid/password is delivered as HEX string. Note: May not be supported on iOS devices.</param>
            public WiFi(string ssid, string password, Authentication authenticationMode, bool isHiddenSSID = false, bool escapeHexStrings = true)
            {
                this.ssid = EscapeInput(ssid);
                this.ssid = escapeHexStrings && IsHexStyle(this.ssid) ? "\"" + this.ssid + "\"" : this.ssid;
                this.password = EscapeInput(password);
                this.password = escapeHexStrings && IsHexStyle(this.password) ? "\"" + this.password + "\"" : this.password;
                this.authenticationMode = authenticationMode.ToString();
                isHiddenSsid = isHiddenSSID;
            }

            public override string ToString()
            {
                return
                    $"WIFI:T:{authenticationMode};S:{ssid};P:{password};{(isHiddenSsid ? "H:true" : string.Empty)};";
            }

            public enum Authentication
            {
                WEP,
                WPA,
                Nopass
            }
        }

        public class Mail : Payload
        {
            private readonly string mailReceiver, subject, message;
            private readonly MailEncoding encoding;

            /// <summary>
            /// Creates an email payload with subject and message/text
            /// </summary>
            /// <param name="mailReceiver">Receiver's email address</param>
            /// <param name="subject">Subject line of the email</param>
            /// <param name="message">Message content of the email</param>
            /// <param name="encoding">Payload encoding type. Choose dependent on your QR Code scanner app.</param>
            public Mail(string mailReceiver = null, string subject = null, string message = null, MailEncoding encoding = MailEncoding.MAILTO)
            {
                this.mailReceiver = mailReceiver;
                this.subject = subject;
                this.message = message;
                this.encoding = encoding;
            }

            public override string ToString()
            {
                var returnVal = string.Empty;
                switch (encoding)
                {
                    case MailEncoding.MAILTO:
                        var parts = new List<string>();
                        if (!string.IsNullOrEmpty(subject))
                            parts.Add("subject=" + Uri.EscapeDataString(subject));
                        if (!string.IsNullOrEmpty(message))
                            parts.Add("body=" + Uri.EscapeDataString(message));
                        var queryString = parts.Any() ? $"?{string.Join("&", parts.ToArray())}" : "";
                        returnVal = $"mailto:{mailReceiver}{queryString}";
                        break;
                    case MailEncoding.MATMSG:
                        returnVal = $"MATMSG:TO:{mailReceiver};SUB:{EscapeInput(subject)};BODY:{EscapeInput(message)};;";
                        break;
                    case MailEncoding.SMTP:
                        returnVal = $"SMTP:{mailReceiver}:{EscapeInput(subject, true)}:{EscapeInput(message, true)}";
                        break;
                }
                return returnVal;
            }

            public enum MailEncoding
            {
                MAILTO,
                MATMSG,
                SMTP
            }
        }

        public class SMS : Payload
        {
            private readonly string number, subject;
            private readonly SMSEncoding encoding;

            /// <summary>
            /// Creates a SMS payload without text
            /// </summary>
            /// <param name="number">Receiver phone number</param>
            /// <param name="encoding">Encoding type</param>
            public SMS(string number, SMSEncoding encoding = SMSEncoding.SMS)
            {
                this.number = number;
                subject = string.Empty;
                this.encoding = encoding;
            }

            /// <summary>
            /// Creates a SMS payload with text (subject)
            /// </summary>
            /// <param name="number">Receiver phone number</param>
            /// <param name="subject">Text of the SMS</param>
            /// <param name="encoding">Encoding type</param>
            public SMS(string number, string subject, SMSEncoding encoding = SMSEncoding.SMS)
            {
                this.number = number;
                this.subject = subject;
                this.encoding = encoding;
            }

            public override string ToString()
            {
                var returnVal = string.Empty;
                switch (encoding)
                {
                    case SMSEncoding.SMS:
                        var queryString = string.Empty;
                        if (!string.IsNullOrEmpty(subject))
                            queryString = $"?body={Uri.EscapeDataString(subject)}";
                        returnVal = $"sms:{number}{queryString}";
                        break;
                    case SMSEncoding.SMS_iOS:
                        var queryStringiOS = string.Empty;
                        if (!string.IsNullOrEmpty(subject))
                            queryStringiOS = $";body={Uri.EscapeDataString(subject)}";
                        returnVal = $"sms:{number}{queryStringiOS}";
                        break;
                    case SMSEncoding.SMSTO:
                        returnVal = $"SMSTO:{number}:{subject}";
                        break;
                }
                return returnVal;
            }

            public enum SMSEncoding
            {
                SMS,
                SMSTO,
                SMS_iOS
            }
        }

        public class Geolocation : Payload
        {
            private readonly string latitude, longitude;
            private readonly GeolocationEncoding encoding;

            /// <summary>
            /// Generates a geo location payload. Supports raw location (GEO encoding) or Google Maps link (GoogleMaps encoding)
            /// </summary>
            /// <param name="latitude">Latitude with . as splitter (-90 to 90)</param>
            /// <param name="longitude">Longitude with . as splitter (-180 to 180)</param>
            /// <param name="encoding">Encoding type - GEO or GoogleMaps</param>
            public Geolocation(string latitude, string longitude, GeolocationEncoding encoding = GeolocationEncoding.GEO)
            {
                if (string.IsNullOrWhiteSpace(latitude))
                    throw new ArgumentException("Latitude cannot be null or empty.", nameof(latitude));
                if (string.IsNullOrWhiteSpace(longitude))
                    throw new ArgumentException("Longitude cannot be null or empty.", nameof(longitude));

                this.latitude = latitude.Replace(",", ".");
                this.longitude = longitude.Replace(",", ".");

                // Validate that coordinates are numeric and within valid ranges
                if (!double.TryParse(this.latitude, NumberStyles.Float, CultureInfo.InvariantCulture, out var lat) ||
                    lat < -90 || lat > 90)
                    throw new ArgumentOutOfRangeException(nameof(latitude), "Latitude must be a number between -90 and 90.");

                if (!double.TryParse(this.longitude, NumberStyles.Float, CultureInfo.InvariantCulture, out var lon) ||
                    lon < -180 || lon > 180)
                    throw new ArgumentOutOfRangeException(nameof(longitude), "Longitude must be a number between -180 and 180.");

                this.encoding = encoding;
            }

            public override string ToString()
            {
                switch (encoding)
                {
                    case GeolocationEncoding.GEO:
                        return $"geo:{latitude},{longitude}";
                    case GeolocationEncoding.GoogleMaps:
                        return $"http://maps.google.com/maps?q={Uri.EscapeDataString(latitude)},{Uri.EscapeDataString(longitude)}";
                    default:
                        return "geo:";
                }
            }

            public enum GeolocationEncoding
            {
                GEO,
                GoogleMaps
            }
        }

        public class PhoneNumber : Payload
        {
            private readonly string number;

            /// <summary>
            /// Generates a phone call payload
            /// </summary>
            /// <param name="number">Phonenumber of the receiver</param>
            public PhoneNumber(string number)
            {
                this.number = number;
            }

            public override string ToString()
            {
                return $"tel:{number}";
            }
        }

        public class Url : Payload
        {
            private readonly string url;

            /// <summary>
            /// Generates a link. If not given, http/https protocol will be added.
            /// </summary>
            /// <param name="url">Link url target</param>
            public Url(string url)
            {
                this.url = url;
            }

            public override string ToString()
            {
                return (!url.StartsWith("http") ? "http://" + url : url);
            }
        }

        public class WhatsAppMessage : Payload
        {
            private readonly string number, message;

            /// <summary>
            /// Let's you compose a WhatApp message and send it the receiver number.
            /// </summary>
            /// <param name="number">Receiver phone number where the number is a full phone number in international format.
            /// Omit any zeroes, brackets, or dashes when adding the phone number in international format.
            /// Use: 1XXXXXXXXXX | Don't use: +001-(XXX)XXXXXXX
            /// </param>
            /// <param name="message">The message</param>
            public WhatsAppMessage(string number, string message)
            {
                this.number = number;
                this.message = message;
            }

            /// <summary>
            /// Let's you compose a WhatApp message. When scanned the user is asked to choose a contact who will receive the message.
            /// </summary>
            /// <param name="message">The message</param>
            public WhatsAppMessage(string message)
            {
                number = string.Empty;
                this.message = message;
            }

            public override string ToString()
            {
                var cleanedPhone = Regex.Replace(number ?? string.Empty, @"^[0+]+|[ ()-]", string.Empty, RegexOptions.None, TimeSpan.FromMilliseconds(100));
                return $"https://wa.me/{cleanedPhone}?text={Uri.EscapeDataString(message ?? string.Empty)}";
            }
        }

        public class Bookmark : Payload
        {
            private readonly string url, title;

            /// <summary>
            /// Generates a bookmark payload. Scanned by an QR Code reader, this one creates a browser bookmark.
            /// </summary>
            /// <param name="url">Url of the bookmark</param>
            /// <param name="title">Title of the bookmark</param>
            public Bookmark(string url, string title)
            {
                this.url = EscapeInput(url);
                this.title = EscapeInput(title);
            }

            public override string ToString()
            {
                return $"MEBKM:TITLE:{title};URL:{url};;";
            }
        }

        public class ContactData : Payload
        {
            private readonly string firstname;
            private readonly string lastname;
            private readonly string nickname;
            private readonly string org;
            private readonly string orgTitle;
            private readonly string phone;
            private readonly string mobilePhone;
            private readonly string workPhone;
            private readonly string email;
            private readonly DateTime? birthday;
            private readonly string website;
            private readonly string street;
            private readonly string houseNumber;
            private readonly string city;
            private readonly string zipCode;
            private readonly string stateRegion;
            private readonly string country;
            private readonly string note;
            private readonly ContactOutputType outputType;
            private readonly AddressOrder addressOrder;

            /// <summary>
            /// Generates a vCard or meCard contact dataset
            /// </summary>
            /// <param name="outputType">Payload output type</param>
            /// <param name="firstname">The firstname</param>
            /// <param name="lastname">The lastname</param>
            /// <param name="nickname">The display name</param>
            /// <param name="phone">Normal phone number</param>
            /// <param name="mobilePhone">Mobile phone</param>
            /// <param name="workPhone">Office phone number</param>
            /// <param name="email">E-Mail address</param>
            /// <param name="birthday">Birthday</param>
            /// <param name="website">Website / Homepage</param>
            /// <param name="street">Street</param>
            /// <param name="houseNumber">House number</param>
            /// <param name="city">City</param>
            /// <param name="stateRegion">State or Region</param>
            /// <param name="zipCode">Zip code</param>
            /// <param name="country">Country</param>
            /// <param name="addressOrder">The address order format to use</param>
            /// <param name="note">Memo text / notes</param>
            /// <param name="org">Organisation/Company</param>
            /// <param name="orgTitle">Organisation/Company Title</param>
            public ContactData(ContactOutputType outputType, string firstname, string lastname, string nickname = null, string phone = null, string mobilePhone = null, string workPhone = null, string email = null, DateTime? birthday = null, string website = null, string street = null, string houseNumber = null, string city = null, string zipCode = null, string country = null, string note = null, string stateRegion = null, AddressOrder addressOrder = AddressOrder.Default, string org = null, string orgTitle = null)
            {
                this.firstname = firstname;
                this.lastname = lastname;
                this.nickname = nickname;
                this.org = org;
                this.orgTitle = orgTitle;
                this.phone = phone;
                this.mobilePhone = mobilePhone;
                this.workPhone = workPhone;
                this.email = email;
                this.birthday = birthday;
                this.website = website;
                this.street = street;
                this.houseNumber = houseNumber;
                this.city = city;
                this.stateRegion = stateRegion;
                this.zipCode = zipCode;
                this.country = country;
                this.addressOrder = addressOrder;
                this.note = note;
                this.outputType = outputType;
            }

            public override string ToString()
            {
                var payload = string.Empty;
                if (outputType == ContactOutputType.MeCard)
                {
                    payload += "MECARD+\r\n";
                    if (!string.IsNullOrEmpty(firstname) && !string.IsNullOrEmpty(lastname))
                        payload += $"N:{lastname}, {firstname}\r\n";
                    else if (!string.IsNullOrEmpty(firstname) || !string.IsNullOrEmpty(lastname))
                        payload += $"N:{firstname}{lastname}\r\n";
                    if (!string.IsNullOrEmpty(org))
                        payload += $"ORG:{org}\r\n";
                    if (!string.IsNullOrEmpty(orgTitle))
                        payload += $"TITLE:{orgTitle}\r\n";
                    if (!string.IsNullOrEmpty(phone))
                        payload += $"TEL:{phone}\r\n";
                    if (!string.IsNullOrEmpty(mobilePhone))
                        payload += $"TEL:{mobilePhone}\r\n";
                    if (!string.IsNullOrEmpty(workPhone))
                        payload += $"TEL:{workPhone}\r\n";
                    if (!string.IsNullOrEmpty(email))
                        payload += $"EMAIL:{email}\r\n";
                    if (!string.IsNullOrEmpty(note))
                        payload += $"NOTE:{note}\r\n";
                    if (birthday != null)
                        payload += $"BDAY:{((DateTime)birthday).ToString("yyyyMMdd")}\r\n";
                    var addressString = string.Empty;
                    if (addressOrder == AddressOrder.Default)
                    {
                        addressString = $"ADR:,,{(!string.IsNullOrEmpty(street) ? street + " " : "")}{(!string.IsNullOrEmpty(houseNumber) ? houseNumber : "")},{(!string.IsNullOrEmpty(zipCode) ? zipCode : "")},{(!string.IsNullOrEmpty(city) ? city : "")},{(!string.IsNullOrEmpty(stateRegion) ? stateRegion : "")},{(!string.IsNullOrEmpty(country) ? country : "")}\r\n";
                    }
                    else
                    {
                        addressString = $"ADR:,,{(!string.IsNullOrEmpty(houseNumber) ? houseNumber + " " : "")}{(!string.IsNullOrEmpty(street) ? street : "")},{(!string.IsNullOrEmpty(city) ? city : "")},{(!string.IsNullOrEmpty(stateRegion) ? stateRegion : "")},{(!string.IsNullOrEmpty(zipCode) ? zipCode : "")},{(!string.IsNullOrEmpty(country) ? country : "")}\r\n";
                    }
                    payload += addressString;
                    if (!string.IsNullOrEmpty(website))
                        payload += $"URL:{website}\r\n";
                    if (!string.IsNullOrEmpty(nickname))
                        payload += $"NICKNAME:{nickname}\r\n";
                    payload = payload.Trim(new char[] { '\r', '\n' });
                }
                else
                {
                    var version = outputType.ToString().Substring(5);
                    if (version.Length > 1)
                        version = version.Insert(1, ".");
                    else
                        version += ".0";

                    payload += "BEGIN:VCARD\r\n";
                    payload += $"VERSION:{version}\r\n";

                    payload += $"N:{(!string.IsNullOrEmpty(lastname) ? lastname : "")};{(!string.IsNullOrEmpty(firstname) ? firstname : "")};;;\r\n";
                    payload += $"FN:{(!string.IsNullOrEmpty(firstname) ? firstname + " " : "")}{(!string.IsNullOrEmpty(lastname) ? lastname : "")}\r\n";
                    if (!string.IsNullOrEmpty(org))
                    {
                        payload += $"ORG:" + org + "\r\n";
                    }
                    if (!string.IsNullOrEmpty(orgTitle))
                    {
                        payload += $"TITLE:" + orgTitle + "\r\n";
                    }
                    if (!string.IsNullOrEmpty(phone))
                    {
                        payload += $"TEL;";
                        if (outputType == ContactOutputType.VCard21)
                            payload += $"HOME;VOICE:{phone}";
                        else if (outputType == ContactOutputType.VCard3)
                            payload += $"TYPE=HOME,VOICE:{phone}";
                        else
                            payload += $"TYPE=home,voice;VALUE=uri:tel:{phone}";
                        payload += "\r\n";
                    }

                    if (!string.IsNullOrEmpty(mobilePhone))
                    {
                        payload += $"TEL;";
                        if (outputType == ContactOutputType.VCard21)
                            payload += $"HOME;CELL:{mobilePhone}";
                        else if (outputType == ContactOutputType.VCard3)
                            payload += $"TYPE=HOME,CELL:{mobilePhone}";
                        else
                            payload += $"TYPE=home,cell;VALUE=uri:tel:{mobilePhone}";
                        payload += "\r\n";
                    }

                    if (!string.IsNullOrEmpty(workPhone))
                    {
                        payload += $"TEL;";
                        if (outputType == ContactOutputType.VCard21)
                            payload += $"WORK;VOICE:{workPhone}";
                        else if (outputType == ContactOutputType.VCard3)
                            payload += $"TYPE=WORK,VOICE:{workPhone}";
                        else
                            payload += $"TYPE=work,voice;VALUE=uri:tel:{workPhone}";
                        payload += "\r\n";
                    }

                    payload += "ADR;";
                    if (outputType == ContactOutputType.VCard21)
                        payload += "HOME;PREF:";
                    else if (outputType == ContactOutputType.VCard3)
                        payload += "TYPE=HOME,PREF:";
                    else
                        payload += "TYPE=home,pref:";
                    var addressString = string.Empty;
                    if (addressOrder == AddressOrder.Default)
                    {
                        addressString = $";;{(!string.IsNullOrEmpty(street) ? street + " " : "")}{(!string.IsNullOrEmpty(houseNumber) ? houseNumber : "")};{(!string.IsNullOrEmpty(zipCode) ? zipCode : "")};{(!string.IsNullOrEmpty(city) ? city : "")};{(!string.IsNullOrEmpty(stateRegion) ? stateRegion : "")};{(!string.IsNullOrEmpty(country) ? country : "")}\r\n";
                    }
                    else
                    {
                        addressString = $";;{(!string.IsNullOrEmpty(houseNumber) ? houseNumber + " " : "")}{(!string.IsNullOrEmpty(street) ? street : "")};{(!string.IsNullOrEmpty(city) ? city : "")};{(!string.IsNullOrEmpty(stateRegion) ? stateRegion : "")};{(!string.IsNullOrEmpty(zipCode) ? zipCode : "")};{(!string.IsNullOrEmpty(country) ? country : "")}\r\n";
                    }
                    payload += addressString;

                    if (birthday != null)
                        payload += $"BDAY:{((DateTime)birthday).ToString("yyyyMMdd")}\r\n";
                    if (!string.IsNullOrEmpty(website))
                        payload += $"URL:{website}\r\n";
                    if (!string.IsNullOrEmpty(email))
                        payload += $"EMAIL:{email}\r\n";
                    if (!string.IsNullOrEmpty(note))
                        payload += $"NOTE:{note}\r\n";
                    if (outputType != ContactOutputType.VCard21 && !string.IsNullOrEmpty(nickname))
                        payload += $"NICKNAME:{nickname}\r\n";

                    payload += "END:VCARD";
                }

                return payload;
            }

            /// <summary>
            /// Possible output types. Either vCard 2.1, vCard 3.0, vCard 4.0 or MeCard.
            /// </summary>
            public enum ContactOutputType
            {
                MeCard,
                VCard21,
                VCard3,
                VCard4
            }

            /// <summary>
            /// define the address format
            /// Default: European format, ([Street] [House Number] and [Postal Code] [City]
            /// Reversed: North American and others format ([House Number] [Street] and [City] [Postal Code])
            /// </summary>
            public enum AddressOrder
            {
                Default,
                Reversed
            }
        }

        public class CalendarEvent : Payload
        {
            private readonly string subject, description, location, start, end;
            private readonly EventEncoding encoding;

            /// <summary>
            /// Generates a calender entry/event payload.
            /// </summary>
            /// <param name="subject">Subject/title of the calender event</param>
            /// <param name="description">Description of the event</param>
            /// <param name="location">Location (lat:long or address) of the event</param>
            /// <param name="start">Start time of the event</param>
            /// <param name="end">End time of the event</param>
            /// <param name="allDayEvent">Is it a full day event?</param>
            /// <param name="encoding">Type of encoding (universal or iCal)</param>
            public CalendarEvent(string subject, string description, string location, DateTime start, DateTime end, bool allDayEvent, EventEncoding encoding = EventEncoding.Universal)
            {
                this.subject = subject;
                this.description = description;
                this.location = location;
                this.encoding = encoding;
                var dtFormat = allDayEvent ? "yyyyMMdd" : "yyyyMMddTHHmmss";
                this.start = start.ToString(dtFormat);
                this.end = end.ToString(dtFormat);
            }

            public override string ToString()
            {
                var vEvent = $"BEGIN:VEVENT{Environment.NewLine}";
                vEvent += $"SUMMARY:{subject}{Environment.NewLine}";
                vEvent += !string.IsNullOrEmpty(description) ? $"DESCRIPTION:{description}{Environment.NewLine}" : "";
                vEvent += !string.IsNullOrEmpty(location) ? $"LOCATION:{location}{Environment.NewLine}" : "";
                vEvent += $"DTSTART:{start}{Environment.NewLine}";
                vEvent += $"DTEND:{end}{Environment.NewLine}";
                vEvent += "END:VEVENT";

                if (encoding == EventEncoding.iCalComplete)
                    vEvent = $@"BEGIN:VCALENDAR{Environment.NewLine}VERSION:2.0{Environment.NewLine}{vEvent}{Environment.NewLine}END:VCALENDAR";

                return vEvent;
            }

            public enum EventEncoding
            {
                iCalComplete,
                Universal
            }
        }

        /// <summary>
        /// Generates a payload for One Time Password (OTP) used in 2FA apps like Google Authenticator.
        /// </summary>
        public class OneTimePassword : Payload
        {
            /// <summary>The type of OTP (TOTP or HOTP).</summary>
            public OneTimePasswordAuthType Type { get; set; } = OneTimePasswordAuthType.TOTP;

            /// <summary>The secret key (base32 encoded) used for OTP generation.</summary>
            public string Secret { get; set; }

            /// <summary>The hashing algorithm (SHA1, SHA256, SHA512).</summary>
            public OneTimePasswordAuthAlgorithm AuthAlgorithm { get; set; } = OneTimePasswordAuthAlgorithm.SHA1;

            /// <summary>The issuer (usually the service or company name).</summary>
            public string Issuer { get; set; }

            /// <summary>The label (usually the user's email or username).</summary>
            public string Label { get; set; }

            /// <summary>The number of digits in the OTP (default is 6).</summary>
            public int Digits { get; set; } = 6;

            /// <summary>The counter value for HOTP (only used if Type is HOTP).</summary>
            public int? Counter { get; set; }

            /// <summary>The period in seconds for TOTP (default is 30).</summary>
            public int? Period { get; set; } = 30;

            public override string ToString()
            {
                return Type switch
                {
                    OneTimePasswordAuthType.TOTP => BuildTotpString(),
                    OneTimePasswordAuthType.HOTP => BuildHotpString(),
                    _ => throw new ArgumentOutOfRangeException()
                };
            }

            private string BuildTotpString()
            {
                if (Period == null)
                    throw new InvalidOperationException("Period must be set for TOTP");

                var sb = new StringBuilder("otpauth://totp/");
                BuildCommonFields(sb);

                if (Period != 30)
                    sb.Append("&period=" + Period);

                return sb.ToString();
            }

            private string BuildHotpString()
            {
                var sb = new StringBuilder("otpauth://hotp/");
                BuildCommonFields(sb);
                sb.Append("&counter=" + (Counter ?? 1));
                return sb.ToString();
            }

            private void BuildCommonFields(StringBuilder sb)
            {
                if (string.IsNullOrWhiteSpace(Secret))
                    throw new InvalidOperationException("Secret must be a filled out base32 encoded string");

                var strippedSecret = Secret.Replace(" ", "");
                string escapedIssuer = null;
                string escapedLabel = null;

                if (!string.IsNullOrWhiteSpace(Issuer))
                {
                    if (Issuer.Contains(':'))
                        throw new InvalidOperationException("Issuer must not contain ':'");
                    escapedIssuer = Uri.EscapeDataString(Issuer);
                }

                if (!string.IsNullOrWhiteSpace(Label))
                {
                    if (Label.Contains(':'))
                        throw new InvalidOperationException("Label must not contain ':'");
                    escapedLabel = Uri.EscapeDataString(Label);
                }

                if (escapedLabel != null && escapedIssuer != null)
                    sb.Append(escapedIssuer + ":" + escapedLabel);
                else if (escapedIssuer != null)
                    sb.Append(escapedIssuer);

                sb.Append("?secret=" + strippedSecret);

                if (escapedIssuer != null)
                    sb.Append("&issuer=" + escapedIssuer);

                if (AuthAlgorithm != OneTimePasswordAuthAlgorithm.SHA1)
                    sb.Append("&algorithm=" + AuthAlgorithm);

                if (Digits != 6)
                    sb.Append("&digits=" + Digits);
            }

            public enum OneTimePasswordAuthType
            {
                /// <summary>Time-based One-Time Password</summary>
                TOTP,
                /// <summary>HMAC-based One-Time Password</summary>
                HOTP
            }

            public enum OneTimePasswordAuthAlgorithm
            {
                SHA1,
                SHA256,
                SHA512
            }
        }

        /// <summary>
        /// Generates a payload for Bitcoin-like cryptocurrency payment addresses.
        /// </summary>
        public class BitcoinLikeCryptoCurrencyAddress : Payload
        {
            private readonly BitcoinLikeCryptoCurrencyType _currencyType;
            private readonly string _address;
            private readonly string _label, _message;
            private readonly double? _amount;

            /// <summary>
            /// Generates a Bitcoin-like cryptocurrency payment payload.
            /// </summary>
            /// <param name="currencyType">Type of cryptocurrency (Bitcoin, BitcoinCash, Litecoin).</param>
            /// <param name="address">The cryptocurrency address of the payment receiver.</param>
            /// <param name="amount">The amount of coins to transfer.</param>
            /// <param name="label">A reference label.</param>
            /// <param name="message">A reference text or message.</param>
            public BitcoinLikeCryptoCurrencyAddress(BitcoinLikeCryptoCurrencyType currencyType, string address, double? amount = null, string label = null, string message = null)
            {
                _currencyType = currencyType;
                _address = address;
                _amount = amount;
                _label = string.IsNullOrEmpty(label) ? null : Uri.EscapeDataString(label);
                _message = string.IsNullOrEmpty(message) ? null : Uri.EscapeDataString(message);
            }

            public override string ToString()
            {
                var queryParts = new List<string>();

                if (_label != null)
                    queryParts.Add("label=" + _label);
                if (_message != null)
                    queryParts.Add("message=" + _message);
                if (_amount.HasValue)
                    queryParts.Add("amount=" + _amount.Value.ToString("#.########", CultureInfo.InvariantCulture));

                var query = queryParts.Count > 0 ? "?" + string.Join("&", queryParts) : "";
                var scheme = _currencyType.ToString().ToLowerInvariant();

                return $"{scheme}:{_address}{query}";
            }

            public enum BitcoinLikeCryptoCurrencyType
            {
                Bitcoin,
                BitcoinCash,
                Litecoin
            }
        }

        /// <summary>
        /// Generates a Bitcoin payment address payload.
        /// </summary>
        public class BitcoinAddress : BitcoinLikeCryptoCurrencyAddress
        {
            public BitcoinAddress(string address, double? amount = null, string label = null, string message = null)
                : base(BitcoinLikeCryptoCurrencyType.Bitcoin, address, amount, label, message) { }
        }

        /// <summary>
        /// Generates a Bitcoin Cash payment address payload.
        /// </summary>
        public class BitcoinCashAddress : BitcoinLikeCryptoCurrencyAddress
        {
            public BitcoinCashAddress(string address, double? amount = null, string label = null, string message = null)
                : base(BitcoinLikeCryptoCurrencyType.BitcoinCash, address, amount, label, message) { }
        }

        /// <summary>
        /// Generates a Litecoin payment address payload.
        /// </summary>
        public class LitecoinAddress : BitcoinLikeCryptoCurrencyAddress
        {
            public LitecoinAddress(string address, double? amount = null, string label = null, string message = null)
                : base(BitcoinLikeCryptoCurrencyType.Litecoin, address, amount, label, message) { }
        }

        private static string EscapeInput(string inp, bool simple = false)
        {
            if (string.IsNullOrEmpty(inp))
                return inp ?? string.Empty;

            char[] forbiddenChars = simple
                ? new[] { ':' }
                : new[] { '\\', ';', ',', ':' };

            foreach (var c in forbiddenChars)
            {
                inp = inp.Replace(c.ToString(), "\\" + c);
            }
            return inp;
        }

        private static bool IsHexStyle(string inp)
        {
            if (string.IsNullOrEmpty(inp))
                return false;
            return Regex.IsMatch(inp, @"\A(0[xX])?[0-9a-fA-F]+\Z", RegexOptions.None, TimeSpan.FromMilliseconds(100));
        }
    }
}
