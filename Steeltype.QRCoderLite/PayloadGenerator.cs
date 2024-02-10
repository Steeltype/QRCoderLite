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

        public class MMS : Payload
        {
            private readonly string number, subject;
            private readonly MMSEncoding encoding;

            /// <summary>
            /// Creates a MMS payload without text
            /// </summary>
            /// <param name="number">Receiver phone number</param>
            /// <param name="encoding">Encoding type</param>
            public MMS(string number, MMSEncoding encoding = MMSEncoding.MMS)
            {
                this.number = number;
                subject = string.Empty;
                this.encoding = encoding;
            }

            /// <summary>
            /// Creates a MMS payload with text (subject)
            /// </summary>
            /// <param name="number">Receiver phone number</param>
            /// <param name="subject">Text of the MMS</param>
            /// <param name="encoding">Encoding type</param>
            public MMS(string number, string subject, MMSEncoding encoding = MMSEncoding.MMS)
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
                    case MMSEncoding.MMSTO:
                        var queryStringMmsTo = string.Empty;
                        if (!string.IsNullOrEmpty(subject))
                            queryStringMmsTo = $"?subject={Uri.EscapeDataString(subject)}";
                        returnVal = $"mmsto:{number}{queryStringMmsTo}";
                        break;
                    case MMSEncoding.MMS:
                        var queryStringMms = string.Empty;
                        if (!string.IsNullOrEmpty(subject))
                            queryStringMms = $"?body={Uri.EscapeDataString(subject)}";
                        returnVal = $"mms:{number}{queryStringMms}";
                        break;
                }
                return returnVal;
            }

            public enum MMSEncoding
            {
                MMS,
                MMSTO
            }
        }

        public class Geolocation : Payload
        {
            private readonly string latitude, longitude;
            private readonly GeolocationEncoding encoding;

            /// <summary>
            /// Generates a geo location payload. Supports raw location (GEO encoding) or Google Maps link (GoogleMaps encoding)
            /// </summary>
            /// <param name="latitude">Latitude with . as splitter</param>
            /// <param name="longitude">Longitude with . as splitter</param>
            /// <param name="encoding">Encoding type - GEO or GoogleMaps</param>
            public Geolocation(string latitude, string longitude, GeolocationEncoding encoding = GeolocationEncoding.GEO)
            {
                this.latitude = latitude.Replace(",", ".");
                this.longitude = longitude.Replace(",", ".");
                this.encoding = encoding;
            }

            public override string ToString()
            {
                switch (encoding)
                {
                    case GeolocationEncoding.GEO:
                        return $"geo:{latitude},{longitude}";
                    case GeolocationEncoding.GoogleMaps:
                        return $"http://maps.google.com/maps?q={latitude},{longitude}";
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

        public class SkypeCall : Payload
        {
            private readonly string skypeUsername;

            /// <summary>
            /// Generates a Skype call payload
            /// </summary>
            /// <param name="skypeUsername">Skype username which will be called</param>
            public SkypeCall(string skypeUsername)
            {
                this.skypeUsername = skypeUsername;
            }

            public override string ToString()
            {
                return $"skype:{skypeUsername}?call";
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
            /// <param name="number">Receiver phone number where the <number> is a full phone number in international format. 
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
                var cleanedPhone = Regex.Replace(number, @"^[0+]+|[ ()-]", string.Empty);
                return ($"https://wa.me/{cleanedPhone}?text={Uri.EscapeDataString(message)}");
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

        public class BitcoinLikeCryptoCurrencyAddress : Payload
        {
            private readonly BitcoinLikeCryptoCurrencyType currencyType;
            private readonly string address, label, message;
            private readonly double? amount;

            /// <summary>
            /// Generates a Bitcoin like crypto-currency payment payload. QR Codes with this payload can open a payment app.
            /// </summary>
            /// <param name="currencyName">Bitcoin like crypto-currency address of the payment receiver</param>
            /// <param name="address">Bitcoin like crypto-currency address of the payment receiver</param>
            /// <param name="amount">Amount of coins to transfer</param>
            /// <param name="label">Reference label</param>
            /// <param name="message">Reference text aka message</param>
            public BitcoinLikeCryptoCurrencyAddress(BitcoinLikeCryptoCurrencyType currencyType, string address, double? amount, string label = null, string message = null)
            {
                this.currencyType = currencyType;
                this.address = address;

                if (!string.IsNullOrEmpty(label))
                {
                    this.label = Uri.EscapeDataString(label);
                }

                if (!string.IsNullOrEmpty(message))
                {
                    this.message = Uri.EscapeDataString(message);
                }

                this.amount = amount;
            }

            public override string ToString()
            {
                string query = null;

                var queryValues = new KeyValuePair<string, string>[]{
                  new(nameof(label), label),
                  new(nameof(message), message),
                  new(nameof(amount), amount.HasValue ? amount.Value.ToString("#.########", CultureInfo.InvariantCulture) : null)
                };

                if (queryValues.Any(keyPair => !string.IsNullOrEmpty(keyPair.Value)))
                {
                    query = "?" + string.Join("&", queryValues
                        .Where(keyPair => !string.IsNullOrEmpty(keyPair.Value))
                        .Select(keyPair => $"{keyPair.Key}={keyPair.Value}")
                        .ToArray());
                }

                return $"{Enum.GetName(typeof(BitcoinLikeCryptoCurrencyType), currencyType).ToLower()}:{address}{query}";
            }

            public enum BitcoinLikeCryptoCurrencyType
            {
                Bitcoin,
                BitcoinCash,
                Litecoin
            }
        }

        public class BitcoinAddress : BitcoinLikeCryptoCurrencyAddress
        {
            public BitcoinAddress(string address, double? amount, string label = null, string message = null)
                : base(BitcoinLikeCryptoCurrencyType.Bitcoin, address, amount, label, message) { }
        }

        public class BitcoinCashAddress : BitcoinLikeCryptoCurrencyAddress
        {
            public BitcoinCashAddress(string address, double? amount, string label = null, string message = null)
                : base(BitcoinLikeCryptoCurrencyType.BitcoinCash, address, amount, label, message) { }
        }

        public class LitecoinAddress : BitcoinLikeCryptoCurrencyAddress
        {
            public LitecoinAddress(string address, double? amount, string label = null, string message = null)
                : base(BitcoinLikeCryptoCurrencyType.Litecoin, address, amount, label, message) { }
        }

        public class SwissQrCode : Payload
        {
            //Keep in mind, that the ECC level has to be set to "M" when generating a SwissQrCode!
            //SwissQrCode specification: 
            //    - (de) https://www.paymentstandards.ch/dam/downloads/ig-qr-bill-de.pdf
            //    - (en) https://www.paymentstandards.ch/dam/downloads/ig-qr-bill-en.pdf
            //Changes between version 1.0 and 2.0: https://www.paymentstandards.ch/dam/downloads/change-documentation-qrr-de.pdf

            private const string Br = "\r\n";
            private readonly string alternativeProcedure1, alternativeProcedure2;
            private readonly Iban iban;
            private readonly decimal? amount;
            private readonly Contact creditor, ultimateCreditor, debitor;
            private readonly Currency currency;
            private readonly DateTime? requestedDateOfPayment;
            private readonly Reference reference;
            private readonly AdditionalInformation additionalInformation;

            /// <summary>
            /// Generates the payload for a SwissQrCode v2.0. (Don't forget to use ECC-Level=M, EncodingMode=UTF-8 and to set the Swiss flag icon to the final QR code.)
            /// </summary>
            /// <param name="iban">IBAN object</param>
            /// <param name="currency">Currency (either EUR or CHF)</param>
            /// <param name="creditor">Creditor (payee) information</param>
            /// <param name="reference">Reference information</param>
            /// <param name="debitor">Debitor (payer) information</param>
            /// <param name="amount">Amount</param>
            /// <param name="requestedDateOfPayment">Requested date of debitor's payment</param>
            /// <param name="ultimateCreditor">Ultimate creditor information (use only in consultation with your bank - for future use only!)</param>
            /// <param name="alternativeProcedure1">Optional command for alternative processing mode - line 1</param>
            /// <param name="alternativeProcedure2">Optional command for alternative processing mode - line 2</param>
            public SwissQrCode(Iban iban, Currency currency, Contact creditor, Reference reference, AdditionalInformation additionalInformation = null, Contact debitor = null, decimal? amount = null, DateTime? requestedDateOfPayment = null, Contact ultimateCreditor = null, string alternativeProcedure1 = null, string alternativeProcedure2 = null)
            {
                this.iban = iban;

                this.creditor = creditor;
                this.ultimateCreditor = ultimateCreditor;

                this.additionalInformation = additionalInformation != null ? additionalInformation : new AdditionalInformation();

                if (amount != null && amount.ToString().Length > 12)
                    throw new SwissQrCodeException("Amount (including decimals) must be shorter than 13 places.");
                this.amount = amount;

                this.currency = currency;
                this.requestedDateOfPayment = requestedDateOfPayment;
                this.debitor = debitor;

                if (iban.IsQrIban && reference.RefType != Reference.ReferenceType.QRR)
                    throw new SwissQrCodeException("If QR-IBAN is used, you have to choose \"QRR\" as reference type!");
                if (!iban.IsQrIban && reference.RefType == Reference.ReferenceType.QRR)
                    throw new SwissQrCodeException("If non QR-IBAN is used, you have to choose either \"SCOR\" or \"NON\" as reference type!");
                this.reference = reference;

                if (alternativeProcedure1 != null && alternativeProcedure1.Length > 100)
                    throw new SwissQrCodeException("Alternative procedure information block 1 must be shorter than 101 chars.");
                this.alternativeProcedure1 = alternativeProcedure1;
                if (alternativeProcedure2 != null && alternativeProcedure2.Length > 100)
                    throw new SwissQrCodeException("Alternative procedure information block 2 must be shorter than 101 chars.");
                this.alternativeProcedure2 = alternativeProcedure2;
            }

            public class AdditionalInformation
            {
                private readonly string unstructuredMessage, billInformation;

                /// <summary>
                /// Creates an additional information object. Both parameters are optional and must be shorter than 141 chars in combination.
                /// </summary>
                /// <param name="unstructuredMessage">Unstructured text message</param>
                /// <param name="billInformation">Bill information</param>
                public AdditionalInformation(string unstructuredMessage = null, string billInformation = null)
                {
                    if (((unstructuredMessage != null ? unstructuredMessage.Length : 0) + (billInformation != null ? billInformation.Length : 0)) > 140)
                        throw new SwissQrCodeAdditionalInformationException("Unstructured message and bill information must be shorter than 141 chars in total/combined.");
                    this.unstructuredMessage = unstructuredMessage;
                    this.billInformation = billInformation;
                    Trailer = "EPD";
                }

                public string UnstructuredMessage => !string.IsNullOrEmpty(unstructuredMessage) ? unstructuredMessage.Replace("\n", "") : null;

                public string BillInformation => !string.IsNullOrEmpty(billInformation) ? billInformation.Replace("\n", "") : null;

                public string Trailer { get; }

                public class SwissQrCodeAdditionalInformationException : Exception
                {
                    public SwissQrCodeAdditionalInformationException()
                    {
                    }

                    public SwissQrCodeAdditionalInformationException(string message)
                        : base(message)
                    {
                    }

                    public SwissQrCodeAdditionalInformationException(string message, Exception inner)
                        : base(message, inner)
                    {
                    }
                }
            }

            public class Reference
            {
                private readonly ReferenceType referenceType;
                private readonly string reference;
                private readonly ReferenceTextType? referenceTextType;

                /// <summary>
                /// Creates a reference object which must be passed to the SwissQrCode instance
                /// </summary>
                /// <param name="referenceType">Type of the reference (QRR, SCOR or NON)</param>
                /// <param name="reference">Reference text</param>
                /// <param name="referenceTextType">Type of the reference text (QR-reference or Creditor Reference)</param>                
                public Reference(ReferenceType referenceType, string reference = null, ReferenceTextType? referenceTextType = null)
                {
                    this.referenceType = referenceType;
                    this.referenceTextType = referenceTextType;

                    if (referenceType == ReferenceType.NON && reference != null)
                        throw new SwissQrCodeReferenceException("Reference is only allowed when referenceType not equals \"NON\"");
                    if (referenceType != ReferenceType.NON && reference != null && referenceTextType == null)
                        throw new SwissQrCodeReferenceException("You have to set an ReferenceTextType when using the reference text.");
                    if (referenceTextType == ReferenceTextType.QrReference && reference != null && (reference.Length > 27))
                        throw new SwissQrCodeReferenceException("QR-references have to be shorter than 28 chars.");
                    if (referenceTextType == ReferenceTextType.QrReference && reference != null && !Regex.IsMatch(reference, @"^[0-9]+$"))
                        throw new SwissQrCodeReferenceException("QR-reference must exist out of digits only.");
                    if (referenceTextType == ReferenceTextType.QrReference && reference != null && !ChecksumMod10(reference))
                        throw new SwissQrCodeReferenceException("QR-references is invalid. Checksum error.");
                    if (referenceTextType == ReferenceTextType.CreditorReferenceIso11649 && reference != null && (reference.Length > 25))
                        throw new SwissQrCodeReferenceException("Creditor references (ISO 11649) have to be shorter than 26 chars.");

                    this.reference = reference;
                }

                public ReferenceType RefType => referenceType;

                public string ReferenceText => !string.IsNullOrEmpty(reference) ? reference.Replace("\n", "") : null;

                /// <summary>
                /// Reference type. When using a QR-IBAN you have to use either "QRR" or "SCOR"
                /// </summary>
                public enum ReferenceType
                {
                    QRR,
                    SCOR,
                    NON
                }

                public enum ReferenceTextType
                {
                    QrReference,
                    CreditorReferenceIso11649
                }

                public class SwissQrCodeReferenceException : Exception
                {
                    public SwissQrCodeReferenceException()
                    {
                    }

                    public SwissQrCodeReferenceException(string message)
                        : base(message)
                    {
                    }

                    public SwissQrCodeReferenceException(string message, Exception inner)
                        : base(message, inner)
                    {
                    }
                }
            }

            public class Iban
            {
                private readonly string iban;
                private readonly IbanType ibanType;

                /// <summary>
                /// IBAN object with type information
                /// </summary>
                /// <param name="iban">IBAN</param>
                /// <param name="ibanType">Type of IBAN (normal or QR-IBAN)</param>
                public Iban(string iban, IbanType ibanType)
                {
                    if (ibanType == IbanType.Iban && !IsValidIban(iban))
                        throw new SwissQrCodeIbanException("The IBAN entered isn't valid.");
                    if (ibanType == IbanType.QrIban && !IsValidQrIban(iban))
                        throw new SwissQrCodeIbanException("The QR-IBAN entered isn't valid.");
                    if (!iban.StartsWith("CH") && !iban.StartsWith("LI"))
                        throw new SwissQrCodeIbanException("The IBAN must start with \"CH\" or \"LI\".");
                    this.iban = iban;
                    this.ibanType = ibanType;
                }

                public bool IsQrIban => ibanType == IbanType.QrIban;

                public override string ToString()
                {
                    return iban.Replace("-", "").Replace("\n", "").Replace(" ", "");
                }

                public enum IbanType
                {
                    Iban,
                    QrIban
                }

                public class SwissQrCodeIbanException : Exception
                {
                    public SwissQrCodeIbanException()
                    {
                    }

                    public SwissQrCodeIbanException(string message)
                        : base(message)
                    {
                    }

                    public SwissQrCodeIbanException(string message, Exception inner)
                        : base(message, inner)
                    {
                    }
                }
            }

            public class Contact
            {
                private static readonly HashSet<string> TwoLetterCodes = ValidTwoLetterCodes();
                private const string Br = "\r\n";
                private readonly string name;
                private readonly string streetOrAddressline1;
                private readonly string houseNumberOrAddressline2;
                private readonly string zipCode;
                private readonly string city;
                private readonly string country;
                private readonly AddressType adrType;

                public static Contact WithStructuredAddress(string name, string zipCode, string city, string country, string street = null, string houseNumber = null)
                {
                    return new Contact(name, zipCode, city, country, street, houseNumber, AddressType.StructuredAddress);
                }

                public static Contact WithCombinedAddress(string name, string country, string addressLine1, string addressLine2)
                {
                    return new Contact(name, null, null, country, addressLine1, addressLine2, AddressType.CombinedAddress);
                }

                private Contact(string name, string zipCode, string city, string country, string streetOrAddressline1, string houseNumberOrAddressline2, AddressType addressType)
                {
                    //Pattern extracted from https://qr-validation.iso-payments.ch as explained in https://github.com/codebude/QRCoder/issues/97
                    var charsetPattern = @"^([a-zA-Z0-9\.,;:'\ \+\-/\(\)?\*\[\]\{\}\\`´~ ]|[!""#%&<>÷=@_$£]|[àáâäçèéêëìíîïñòóôöùúûüýßÀÁÂÄÇÈÉÊËÌÍÎÏÒÓÔÖÙÚÛÜÑ])*$";

                    adrType = addressType;

                    if (string.IsNullOrEmpty(name))
                        throw new SwissQrCodeContactException("Name must not be empty.");
                    if (name.Length > 70)
                        throw new SwissQrCodeContactException("Name must be shorter than 71 chars.");
                    if (!Regex.IsMatch(name, charsetPattern))
                        throw new SwissQrCodeContactException($"Name must match the following pattern as defined in pain.001: {charsetPattern}");
                    this.name = name;

                    if (AddressType.StructuredAddress == adrType)
                    {
                        if (!string.IsNullOrEmpty(streetOrAddressline1) && (streetOrAddressline1.Length > 70))
                            throw new SwissQrCodeContactException("Street must be shorter than 71 chars.");
                        if (!string.IsNullOrEmpty(streetOrAddressline1) && !Regex.IsMatch(streetOrAddressline1, charsetPattern))
                            throw new SwissQrCodeContactException($"Street must match the following pattern as defined in pain.001: {charsetPattern}");
                        this.streetOrAddressline1 = streetOrAddressline1;

                        if (!string.IsNullOrEmpty(houseNumberOrAddressline2) && houseNumberOrAddressline2.Length > 16)
                            throw new SwissQrCodeContactException("House number must be shorter than 17 chars.");
                        this.houseNumberOrAddressline2 = houseNumberOrAddressline2;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(streetOrAddressline1) && (streetOrAddressline1.Length > 70))
                            throw new SwissQrCodeContactException("Address line 1 must be shorter than 71 chars.");
                        if (!string.IsNullOrEmpty(streetOrAddressline1) && !Regex.IsMatch(streetOrAddressline1, charsetPattern))
                            throw new SwissQrCodeContactException($"Address line 1 must match the following pattern as defined in pain.001: {charsetPattern}");
                        this.streetOrAddressline1 = streetOrAddressline1;

                        if (string.IsNullOrEmpty(houseNumberOrAddressline2))
                            throw new SwissQrCodeContactException("Address line 2 must be provided for combined addresses (address line-based addresses).");
                        if (!string.IsNullOrEmpty(houseNumberOrAddressline2) && (houseNumberOrAddressline2.Length > 70))
                            throw new SwissQrCodeContactException("Address line 2 must be shorter than 71 chars.");
                        if (!string.IsNullOrEmpty(houseNumberOrAddressline2) && !Regex.IsMatch(houseNumberOrAddressline2, charsetPattern))
                            throw new SwissQrCodeContactException($"Address line 2 must match the following pattern as defined in pain.001: {charsetPattern}");
                        this.houseNumberOrAddressline2 = houseNumberOrAddressline2;
                    }

                    if (AddressType.StructuredAddress == adrType)
                    {
                        if (string.IsNullOrEmpty(zipCode))
                            throw new SwissQrCodeContactException("Zip code must not be empty.");
                        if (zipCode.Length > 16)
                            throw new SwissQrCodeContactException("Zip code must be shorter than 17 chars.");
                        if (!Regex.IsMatch(zipCode, charsetPattern))
                            throw new SwissQrCodeContactException($"Zip code must match the following pattern as defined in pain.001: {charsetPattern}");
                        this.zipCode = zipCode;

                        if (string.IsNullOrEmpty(city))
                            throw new SwissQrCodeContactException("City must not be empty.");
                        if (city.Length > 35)
                            throw new SwissQrCodeContactException("City name must be shorter than 36 chars.");
                        if (!Regex.IsMatch(city, charsetPattern))
                            throw new SwissQrCodeContactException($"City name must match the following pattern as defined in pain.001: {charsetPattern}");
                        this.city = city;
                    }
                    else
                    {
                        this.zipCode = this.city = string.Empty;
                    }

                    if (!IsValidTwoLetterCode(country))
                        throw new SwissQrCodeContactException("Country must be a valid \"two letter\" country code as defined by  ISO 3166-1, but it isn't.");

                    this.country = country;
                }

                private static bool IsValidTwoLetterCode(string code) => TwoLetterCodes.Contains(code);

                private static HashSet<string> ValidTwoLetterCodes()
                {
                    var codes = new string[] { "AF", "AL", "DZ", "AS", "AD", "AO", "AI", "AQ", "AG", "AR", "AM", "AW", "AU", "AT", "AZ", "BS", "BH", "BD", "BB", "BY", "BE", "BZ", "BJ", "BM", "BT", "BO", "BQ", "BA", "BW", "BV", "BR", "IO", "BN", "BG", "BF", "BI", "CV", "KH", "CM", "CA", "KY", "CF", "TD", "CL", "CN", "CX", "CC", "CO", "KM", "CG", "CD", "CK", "CR", "CI", "HR", "CU", "CW", "CY", "CZ", "DK", "DJ", "DM", "DO", "EC", "EG", "SV", "GQ", "ER", "EE", "SZ", "ET", "FK", "FO", "FJ", "FI", "FR", "GF", "PF", "TF", "GA", "GM", "GE", "DE", "GH", "GI", "GR", "GL", "GD", "GP", "GU", "GT", "GG", "GN", "GW", "GY", "HT", "HM", "VA", "HN", "HK", "HU", "IS", "IN", "ID", "IR", "IQ", "IE", "IM", "IL", "IT", "JM", "JP", "JE", "JO", "KZ", "KE", "KI", "KP", "KR", "KW", "KG", "LA", "LV", "LB", "LS", "LR", "LY", "LI", "LT", "LU", "MO", "MG", "MW", "MY", "MV", "ML", "MT", "MH", "MQ", "MR", "MU", "YT", "MX", "FM", "MD", "MC", "MN", "ME", "MS", "MA", "MZ", "MM", "NA", "NR", "NP", "NL", "NC", "NZ", "NI", "NE", "NG", "NU", "NF", "MP", "MK", "NO", "OM", "PK", "PW", "PS", "PA", "PG", "PY", "PE", "PH", "PN", "PL", "PT", "PR", "QA", "RE", "RO", "RU", "RW", "BL", "SH", "KN", "LC", "MF", "PM", "VC", "WS", "SM", "ST", "SA", "SN", "RS", "SC", "SL", "SG", "SX", "SK", "SI", "SB", "SO", "ZA", "GS", "SS", "ES", "LK", "SD", "SR", "SJ", "SE", "CH", "SY", "TW", "TJ", "TZ", "TH", "TL", "TG", "TK", "TO", "TT", "TN", "TR", "TM", "TC", "TV", "UG", "UA", "AE", "GB", "US", "UM", "UY", "UZ", "VU", "VE", "VN", "VG", "VI", "WF", "EH", "YE", "ZM", "ZW", "AX" };
                    return new HashSet<string>(codes, StringComparer.OrdinalIgnoreCase);
                }

                public override string ToString()
                {
                    var contactData = $"{(AddressType.StructuredAddress == adrType ? "S" : "K")}{Br}"; // AdrTp
                    contactData += name.Replace("\n", "") + Br; // Name
                    contactData += (!string.IsNullOrEmpty(streetOrAddressline1) ? streetOrAddressline1.Replace("\n", "") : string.Empty) + Br; // StrtNmOrAdrLine1
                    contactData += (!string.IsNullOrEmpty(houseNumberOrAddressline2) ? houseNumberOrAddressline2.Replace("\n", "") : string.Empty) + Br; // BldgNbOrAdrLine2
                    contactData += zipCode.Replace("\n", "") + Br; // PstCd
                    contactData += city.Replace("\n", "") + Br; // TwnNm
                    contactData += country + Br; // Ctry
                    return contactData;
                }

                public enum AddressType
                {
                    StructuredAddress,
                    CombinedAddress
                }

                public class SwissQrCodeContactException : Exception
                {
                    public SwissQrCodeContactException()
                    {
                    }

                    public SwissQrCodeContactException(string message)
                        : base(message)
                    {
                    }

                    public SwissQrCodeContactException(string message, Exception inner)
                        : base(message, inner)
                    {
                    }
                }
            }

            public override string ToString()
            {
                //Header "logical" element
                var swissQrCodePayload = "SPC" + Br; //QRType
                swissQrCodePayload += "0200" + Br; //Version
                swissQrCodePayload += "1" + Br; //Coding

                //CdtrInf "logical" element
                swissQrCodePayload += iban + Br; //IBAN


                //Cdtr "logical" element
                swissQrCodePayload += creditor.ToString();

                //UltmtCdtr "logical" element
                //Since version 2.0 ultimate creditor was marked as "for future use" and has to be delivered empty in any case!
                swissQrCodePayload += string.Concat(Enumerable.Repeat(Br, 7).ToArray());

                //CcyAmtDate "logical" element
                //Amoutn has to use . as decimal seperator in any case. See https://www.paymentstandards.ch/dam/downloads/ig-qr-bill-en.pdf page 27.
                swissQrCodePayload += (amount != null ? $"{amount:0.00}".Replace(",", ".") : string.Empty) + Br; //Amt
                swissQrCodePayload += currency + Br; //Ccy                
                //Removed in S-QR version 2.0
                //SwissQrCodePayload += (requestedDateOfPayment != null ?  ((DateTime)requestedDateOfPayment).ToString("yyyy-MM-dd") : string.Empty) + br; //ReqdExctnDt

                //UltmtDbtr "logical" element
                if (debitor != null)
                    swissQrCodePayload += debitor.ToString();
                else
                    swissQrCodePayload += string.Concat(Enumerable.Repeat(Br, 7).ToArray());


                //RmtInf "logical" element
                swissQrCodePayload += reference.RefType + Br; //Tp
                swissQrCodePayload += (!string.IsNullOrEmpty(reference.ReferenceText) ? reference.ReferenceText : string.Empty) + Br; //Ref


                //AddInf "logical" element
                swissQrCodePayload += (!string.IsNullOrEmpty(additionalInformation.UnstructuredMessage) ? additionalInformation.UnstructuredMessage : string.Empty) + Br; //Ustrd
                swissQrCodePayload += additionalInformation.Trailer + Br; //Trailer
                swissQrCodePayload += (!string.IsNullOrEmpty(additionalInformation.BillInformation) ? additionalInformation.BillInformation : string.Empty) + Br; //StrdBkgInf

                //AltPmtInf "logical" element
                if (!string.IsNullOrEmpty(alternativeProcedure1))
                    swissQrCodePayload += alternativeProcedure1.Replace("\n", "") + Br; //AltPmt
                if (!string.IsNullOrEmpty(alternativeProcedure2))
                    swissQrCodePayload += alternativeProcedure2.Replace("\n", "") + Br; //AltPmt

                //S-QR specification 2.0, chapter 4.2.3
                if (swissQrCodePayload.EndsWith(Br))
                    swissQrCodePayload = swissQrCodePayload.Remove(swissQrCodePayload.Length - Br.Length);

                return swissQrCodePayload;
            }
            
            /// <summary>
            /// ISO 4217 currency codes
            /// </summary>
            public enum Currency
            {
                CHF = 756,
                EUR = 978
            }

            public class SwissQrCodeException : Exception
            {
                public SwissQrCodeException()
                {
                }

                public SwissQrCodeException(string message)
                    : base(message)
                {
                }

                public SwissQrCodeException(string message, Exception inner)
                    : base(message, inner)
                {
                }
            }
        }

        public class Girocode : Payload
        {
            //Keep in mind, that the ECC level has to be set to "M" when generating a Girocode!
            //Girocode specification: http://www.europeanpaymentscouncil.eu/index.cfm/knowledge-bank/epc-documents/quick-response-code-guidelines-to-enable-data-capture-for-the-initiation-of-a-sepa-credit-transfer/epc069-12-quick-response-code-guidelines-to-enable-data-capture-for-the-initiation-of-a-sepa-credit-transfer1/

            private const string Br = "\n";
            private readonly string iban, bic, name, purposeOfCreditTransfer, remittanceInformation, messageToGirocodeUser;
            private readonly decimal amount;
            private readonly GirocodeVersion version;
            private readonly GirocodeEncoding encoding;
            private readonly TypeOfRemittance typeOfRemittance;


            /// <summary>
            /// Generates the payload for a Girocode (QR-Code with credit transfer information).
            /// Attention: When using Girocode payload, QR code must be generated with ECC level M!
            /// </summary>
            /// <param name="iban">Account number of the Beneficiary. Only IBAN is allowed.</param>
            /// <param name="bic">BIC of the Beneficiary Bank.</param>
            /// <param name="name">Name of the Beneficiary.</param>
            /// <param name="amount">Amount of the Credit Transfer in Euro.
            /// (Amount must be more than 0.01 and less than 999999999.99)</param>
            /// <param name="remittanceInformation">Remittance Information (Purpose-/reference text). (optional)</param>
            /// <param name="typeOfRemittance">Type of remittance information. Either structured (e.g. ISO 11649 RF Creditor Reference) and max. 35 chars or unstructured and max. 140 chars.</param>
            /// <param name="purposeOfCreditTransfer">Purpose of the Credit Transfer (optional)</param>
            /// <param name="messageToGirocodeUser">Beneficiary to originator information. (optional)</param>
            /// <param name="version">Girocode version. Either 001 or 002. Default: 001.</param>
            /// <param name="encoding">Encoding of the Girocode payload. Default: ISO-8859-1</param>
            public Girocode(string iban, string bic, string name, decimal amount, string remittanceInformation = "", TypeOfRemittance typeOfRemittance = TypeOfRemittance.Unstructured, string purposeOfCreditTransfer = "", string messageToGirocodeUser = "", GirocodeVersion version = GirocodeVersion.Version1, GirocodeEncoding encoding = GirocodeEncoding.ISO_8859_1)
            {
                this.version = version;
                this.encoding = encoding;
                if (!IsValidIban(iban))
                    throw new GirocodeException("The IBAN entered isn't valid.");
                this.iban = iban.Replace(" ", "").ToUpper();
                if (!IsValidBic(bic))
                    throw new GirocodeException("The BIC entered isn't valid.");
                this.bic = bic.Replace(" ", "").ToUpper();
                if (name.Length > 70)
                    throw new GirocodeException("(Payee-)Name must be shorter than 71 chars.");
                this.name = name;
                if (amount.ToString().Replace(",", ".").Contains(".") && amount.ToString().Replace(",", ".").Split('.')[1].TrimEnd('0').Length > 2)
                    throw new GirocodeException("Amount must have less than 3 digits after decimal point.");
                if (amount < 0.01m || amount > 999999999.99m)
                    throw new GirocodeException("Amount has to at least 0.01 and must be smaller or equal to 999999999.99.");
                this.amount = amount;
                if (purposeOfCreditTransfer.Length > 4)
                    throw new GirocodeException("Purpose of credit transfer can only have 4 chars at maximum.");
                this.purposeOfCreditTransfer = purposeOfCreditTransfer;
                if (typeOfRemittance == TypeOfRemittance.Unstructured && remittanceInformation.Length > 140)
                    throw new GirocodeException("Unstructured reference texts have to shorter than 141 chars.");
                if (typeOfRemittance == TypeOfRemittance.Structured && remittanceInformation.Length > 35)
                    throw new GirocodeException("Structured reference texts have to shorter than 36 chars.");
                this.typeOfRemittance = typeOfRemittance;
                this.remittanceInformation = remittanceInformation;
                if (messageToGirocodeUser.Length > 70)
                    throw new GirocodeException("Message to the Girocode-User reader texts have to shorter than 71 chars.");
                this.messageToGirocodeUser = messageToGirocodeUser;
            }

            public override string ToString()
            {
                var girocodePayload = "BCD" + Br;
                girocodePayload += ((version == GirocodeVersion.Version1) ? "001" : "002") + Br;
                girocodePayload += (int)encoding + 1 + Br;
                girocodePayload += "SCT" + Br;
                girocodePayload += bic + Br;
                girocodePayload += name + Br;
                girocodePayload += iban + Br;
                girocodePayload += $"EUR{amount:0.00}".Replace(",", ".") + Br;
                girocodePayload += purposeOfCreditTransfer + Br;
                girocodePayload += ((typeOfRemittance == TypeOfRemittance.Structured)
                    ? remittanceInformation
                    : string.Empty) + Br;
                girocodePayload += ((typeOfRemittance == TypeOfRemittance.Unstructured)
                    ? remittanceInformation
                    : string.Empty) + Br;
                girocodePayload += messageToGirocodeUser;

                return ConvertStringToEncoding(girocodePayload, encoding.ToString().Replace("_", "-"));
            }

            public enum GirocodeVersion
            {
                Version1,
                Version2
            }

            public enum TypeOfRemittance
            {
                Structured,
                Unstructured
            }

            public enum GirocodeEncoding
            {
                UTF_8,
                ISO_8859_1,
                ISO_8859_2,
                ISO_8859_4,
                ISO_8859_5,
                ISO_8859_7,
                ISO_8859_10,
                ISO_8859_15
            }

            public class GirocodeException : Exception
            {
                public GirocodeException()
                {
                }

                public GirocodeException(string message)
                    : base(message)
                {
                }

                public GirocodeException(string message, Exception inner)
                    : base(message, inner)
                {
                }
            }
        }

        public class BezahlCode : Payload
        {
            //BezahlCode specification: http://www.bezahlcode.de/wp-content/uploads/BezahlCode_TechDok.pdf

            private readonly string name, iban, bic, account, bnc, sepaReference, reason, creditorId, mandateId, periodicTimeunit;
            private readonly decimal amount;
            private readonly int postingKey, periodicTimeunitRotation;
            private readonly Currency currency;
            private readonly AuthorityType authority;
            private readonly DateTime executionDate, dateOfSignature, periodicFirstExecutionDate, periodicLastExecutionDate;


            /// <summary>
            /// Constructor for contact data
            /// </summary>
            /// <param name="authority">Type of the bank transfer</param>
            /// <param name="name">Name of the receiver (Empfänger)</param>
            /// <param name="account">Bank account (Kontonummer)</param>
            /// <param name="bnc">Bank institute (Bankleitzahl)</param>
            /// <param name="iban">IBAN</param>
            /// <param name="bic">BIC</param>
            /// <param name="reason">Reason (Verwendungszweck)</param>
            public BezahlCode(AuthorityType authority, string name, string account = "", string bnc = "", string iban = "", string bic = "", string reason = "") : this(authority, name, account, bnc, iban, bic, 0, string.Empty, 0, null, null, string.Empty, string.Empty, null, reason, 0, string.Empty, Currency.EUR, null, 1)
            {
            }


            /// <summary>
            /// Constructor for non-SEPA payments
            /// </summary>
            /// <param name="authority">Type of the bank transfer</param>
            /// <param name="name">Name of the receiver (Empfänger)</param>
            /// <param name="account">Bank account (Kontonummer)</param>
            /// <param name="bnc">Bank institute (Bankleitzahl)</param>
            /// <param name="amount">Amount (Betrag)</param>
            /// <param name="periodicTimeunit">Unit of intervall for payment ('M' = monthly, 'W' = weekly)</param>
            /// <param name="periodicTimeunitRotation">Intervall for payment. This value is combined with 'periodicTimeunit'</param>
            /// <param name="periodicFirstExecutionDate">Date of first periodic execution</param>
            /// <param name="periodicLastExecutionDate">Date of last periodic execution</param>
            /// <param name="reason">Reason (Verwendungszweck)</param>
            /// <param name="postingKey">Transfer Key (Textschlüssel, z.B. Spendenzahlung = 69)</param>
            /// <param name="currency">Currency (Währung)</param>
            /// <param name="executionDate">Execution date (Ausführungsdatum)</param>
            public BezahlCode(AuthorityType authority, string name, string account, string bnc, decimal amount, string periodicTimeunit = "", int periodicTimeunitRotation = 0, DateTime? periodicFirstExecutionDate = null, DateTime? periodicLastExecutionDate = null, string reason = "", int postingKey = 0, Currency currency = Currency.EUR, DateTime? executionDate = null) : this(authority, name, account, bnc, string.Empty, string.Empty, amount, periodicTimeunit, periodicTimeunitRotation, periodicFirstExecutionDate, periodicLastExecutionDate, string.Empty, string.Empty, null, reason, postingKey, string.Empty, currency, executionDate, 2)
            {
            }

            /// <summary>
            /// Constructor for SEPA payments
            /// </summary>
            /// <param name="authority">Type of the bank transfer</param>
            /// <param name="name">Name of the receiver (Empfänger)</param>
            /// <param name="iban">IBAN</param>
            /// <param name="bic">BIC</param>
            /// <param name="amount">Amount (Betrag)</param>
            /// <param name="periodicTimeunit">Unit of intervall for payment ('M' = monthly, 'W' = weekly)</param>
            /// <param name="periodicTimeunitRotation">Intervall for payment. This value is combined with 'periodicTimeunit'</param>
            /// <param name="periodicFirstExecutionDate">Date of first periodic execution</param>
            /// <param name="periodicLastExecutionDate">Date of last periodic execution</param>
            /// <param name="creditorId">Creditor id (Gläubiger ID)</param>
            /// <param name="mandateId">Manadate id (Mandatsreferenz)</param>
            /// <param name="dateOfSignature">Signature date (Erteilungsdatum des Mandats)</param>
            /// <param name="reason">Reason (Verwendungszweck)</param>
            /// <param name="postingKey">Transfer Key (Textschlüssel, z.B. Spendenzahlung = 69)</param>
            /// <param name="sepaReference">SEPA reference (SEPA-Referenz)</param>
            /// <param name="currency">Currency (Währung)</param>
            /// <param name="executionDate">Execution date (Ausführungsdatum)</param>
            public BezahlCode(AuthorityType authority, string name, string iban, string bic, decimal amount, string periodicTimeunit = "", int periodicTimeunitRotation = 0, DateTime? periodicFirstExecutionDate = null, DateTime? periodicLastExecutionDate = null, string creditorId = "", string mandateId = "", DateTime? dateOfSignature = null, string reason = "", string sepaReference = "", Currency currency = Currency.EUR, DateTime? executionDate = null) : this(authority, name, string.Empty, string.Empty, iban, bic, amount, periodicTimeunit, periodicTimeunitRotation, periodicFirstExecutionDate, periodicLastExecutionDate, creditorId, mandateId, dateOfSignature, reason, 0, sepaReference, currency, executionDate, 3)
            {
            }

            /// <summary>
            /// Generic constructor. Please use specific (non-SEPA or SEPA) constructor
            /// </summary>
            /// <param name="authority">Type of the bank transfer</param>
            /// <param name="name">Name of the receiver (Empfänger)</param>
            /// <param name="account">Bank account (Kontonummer)</param>
            /// <param name="bnc">Bank institute (Bankleitzahl)</param>
            /// <param name="iban">IBAN</param>
            /// <param name="bic">BIC</param>
            /// <param name="amount">Amount (Betrag)</param>
            /// <param name="periodicTimeunit">Unit of intervall for payment ('M' = monthly, 'W' = weekly)</param>
            /// <param name="periodicTimeunitRotation">Intervall for payment. This value is combined with 'periodicTimeunit'</param>
            /// <param name="periodicFirstExecutionDate">Date of first periodic execution</param>
            /// <param name="periodicLastExecutionDate">Date of last periodic execution</param>
            /// <param name="creditorId">Creditor id (Gläubiger ID)</param>
            /// <param name="mandateId">Manadate id (Mandatsreferenz)</param>
            /// <param name="dateOfSignature">Signature date (Erteilungsdatum des Mandats)</param>
            /// <param name="reason">Reason (Verwendungszweck)</param>
            /// <param name="postingKey">Transfer Key (Textschlüssel, z.B. Spendenzahlung = 69)</param>
            /// <param name="sepaReference">SEPA reference (SEPA-Referenz)</param>
            /// <param name="currency">Currency (Währung)</param>
            /// <param name="executionDate">Execution date (Ausführungsdatum)</param>
            /// <param name="internalMode">Only used for internal state handdling</param>
            public BezahlCode(AuthorityType authority, string name, string account, string bnc, string iban, string bic, decimal amount, string periodicTimeunit = "", int periodicTimeunitRotation = 0, DateTime? periodicFirstExecutionDate = null, DateTime? periodicLastExecutionDate = null, string creditorId = "", string mandateId = "", DateTime? dateOfSignature = null, string reason = "", int postingKey = 0, string sepaReference = "", Currency currency = Currency.EUR, DateTime? executionDate = null, int internalMode = 0)
            {
                //Loaded via "contact-constructor"
                if (internalMode == 1)
                {
                    if (authority != AuthorityType.Contact && authority != AuthorityType.ContactV2)
                        throw new BezahlCodeException("The constructor without an amount may only ne used with authority types 'contact' and 'contact_v2'.");
                    if (authority == AuthorityType.Contact && (string.IsNullOrEmpty(account) || string.IsNullOrEmpty(bnc)))
                        throw new BezahlCodeException("When using authority type 'contact' the parameters 'account' and 'bnc' must be set.");

                    if (authority != AuthorityType.ContactV2)
                    {
                        var oldFilled = (!string.IsNullOrEmpty(account) && !string.IsNullOrEmpty(bnc));
                        var newFilled = (!string.IsNullOrEmpty(iban) && !string.IsNullOrEmpty(bic));
                        if ((!oldFilled && !newFilled) || (oldFilled && newFilled))
                            throw new BezahlCodeException("When using authority type 'contact_v2' either the parameters 'account' and 'bnc' or the parameters 'iban' and 'bic' must be set. Leave the other parameter pair empty.");
                    }
                }
                else if (internalMode == 2)
                {
#pragma warning disable CS0612
                    if (authority != AuthorityType.Periodicsinglepayment && authority != AuthorityType.Singledirectdebit && authority != AuthorityType.Singlepayment)
                        throw new BezahlCodeException("The constructor with 'account' and 'bnc' may only be used with 'non SEPA' authority types. Either choose another authority type or switch constructor.");
                    if (authority == AuthorityType.Periodicsinglepayment && (string.IsNullOrEmpty(periodicTimeunit) || periodicTimeunitRotation == 0))
                        throw new BezahlCodeException("When using 'periodicsinglepayment' as authority type, the parameters 'periodicTimeunit' and 'periodicTimeunitRotation' must be set.");
#pragma warning restore CS0612
                }
                else if (internalMode == 3)
                {
                    if (authority != AuthorityType.Periodicsinglepaymentsepa && authority != AuthorityType.Singledirectdebitsepa && authority != AuthorityType.Singlepaymentsepa)
                        throw new BezahlCodeException("The constructor with 'iban' and 'bic' may only be used with 'SEPA' authority types. Either choose another authority type or switch constructor.");
                    if (authority == AuthorityType.Periodicsinglepaymentsepa && (string.IsNullOrEmpty(periodicTimeunit) || periodicTimeunitRotation == 0))
                        throw new BezahlCodeException("When using 'periodicsinglepaymentsepa' as authority type, the parameters 'periodicTimeunit' and 'periodicTimeunitRotation' must be set.");
                }

                this.authority = authority;

                if (name.Length > 70)
                    throw new BezahlCodeException("(Payee-)Name must be shorter than 71 chars.");
                this.name = name;

                if (reason.Length > 27)
                    throw new BezahlCodeException("Reasons texts have to be shorter than 28 chars.");
                this.reason = reason;

                var oldWayFilled = (!string.IsNullOrEmpty(account) && !string.IsNullOrEmpty(bnc));
                var newWayFilled = (!string.IsNullOrEmpty(iban) && !string.IsNullOrEmpty(bic));

                //Non-SEPA payment types
#pragma warning disable CS0612
                if (authority == AuthorityType.Periodicsinglepayment || authority == AuthorityType.Singledirectdebit || authority == AuthorityType.Singlepayment || authority == AuthorityType.Contact || (authority == AuthorityType.ContactV2 && oldWayFilled))
                {
#pragma warning restore CS0612
                    if (!Regex.IsMatch(account.Replace(" ", ""), @"^[0-9]{1,9}$"))
                        throw new BezahlCodeException("The account entered isn't valid.");
                    this.account = account.Replace(" ", "").ToUpper();
                    if (!Regex.IsMatch(bnc.Replace(" ", ""), @"^[0-9]{1,9}$"))
                        throw new BezahlCodeException("The bnc entered isn't valid.");
                    this.bnc = bnc.Replace(" ", "").ToUpper();

                    if (authority != AuthorityType.Contact && authority != AuthorityType.ContactV2)
                    {
                        if (postingKey < 0 || postingKey >= 100)
                            throw new BezahlCodeException("PostingKey must be within 0 and 99.");
                        this.postingKey = postingKey;
                    }
                }

                //SEPA payment types
                if (authority == AuthorityType.Periodicsinglepaymentsepa || authority == AuthorityType.Singledirectdebitsepa || authority == AuthorityType.Singlepaymentsepa || (authority == AuthorityType.ContactV2 && newWayFilled))
                {
                    if (!IsValidIban(iban))
                        throw new BezahlCodeException("The IBAN entered isn't valid.");
                    this.iban = iban.Replace(" ", "").ToUpper();
                    if (!IsValidBic(bic))
                        throw new BezahlCodeException("The BIC entered isn't valid.");
                    this.bic = bic.Replace(" ", "").ToUpper();

                    if (authority != AuthorityType.ContactV2)
                    {
                        if (sepaReference.Length > 35)
                            throw new BezahlCodeException("SEPA reference texts have to be shorter than 36 chars.");
                        this.sepaReference = sepaReference;

                        if (!string.IsNullOrEmpty(creditorId) && !Regex.IsMatch(creditorId.Replace(" ", ""), @"^[a-zA-Z]{2,2}[0-9]{2,2}([A-Za-z0-9]|[\+|\?|/|\-|:|\(|\)|\.|,|']){3,3}([A-Za-z0-9]|[\+|\?|/|\-|:|\(|\)|\.|,|']){1,28}$"))
                            throw new BezahlCodeException("The creditorId entered isn't valid.");
                        this.creditorId = creditorId;
                        if (!string.IsNullOrEmpty(mandateId) && !Regex.IsMatch(mandateId.Replace(" ", ""), @"^([A-Za-z0-9]|[\+|\?|/|\-|:|\(|\)|\.|,|']){1,35}$"))
                            throw new BezahlCodeException("The mandateId entered isn't valid.");
                        this.mandateId = mandateId;
                        if (dateOfSignature != null)
                            this.dateOfSignature = (DateTime)dateOfSignature;
                    }
                }

                //Checks for all payment types
                if (authority != AuthorityType.Contact && authority != AuthorityType.ContactV2)
                {
                    if (amount.ToString().Replace(",", ".").Contains(".") && amount.ToString().Replace(",", ".").Split('.')[1].TrimEnd('0').Length > 2)
                        throw new BezahlCodeException("Amount must have less than 3 digits after decimal point.");
                    if (amount < 0.01m || amount > 999999999.99m)
                        throw new BezahlCodeException("Amount has to at least 0.01 and must be smaller or equal to 999999999.99.");
                    this.amount = amount;

                    this.currency = currency;

                    if (executionDate == null)
                        this.executionDate = DateTime.Now;
                    else
                    {
                        if (DateTime.Today.Ticks > executionDate.Value.Ticks)
                            throw new BezahlCodeException("Execution date must be today or in future.");
                        this.executionDate = (DateTime)executionDate;
                    }
#pragma warning disable CS0612
                    if (authority == AuthorityType.Periodicsinglepayment || authority == AuthorityType.Periodicsinglepaymentsepa)
#pragma warning restore CS0612
                    {
                        if (periodicTimeunit.ToUpper() != "M" && periodicTimeunit.ToUpper() != "W")
                            throw new BezahlCodeException("The periodicTimeunit must be either 'M' (monthly) or 'W' (weekly).");
                        this.periodicTimeunit = periodicTimeunit;
                        if (periodicTimeunitRotation < 1 || periodicTimeunitRotation > 52)
                            throw new BezahlCodeException("The periodicTimeunitRotation must be 1 or greater. (It means repeat the payment every 'periodicTimeunitRotation' weeks/months.");
                        this.periodicTimeunitRotation = periodicTimeunitRotation;
                        if (periodicFirstExecutionDate != null)
                            this.periodicFirstExecutionDate = (DateTime)periodicFirstExecutionDate;
                        if (periodicLastExecutionDate != null)
                            this.periodicLastExecutionDate = (DateTime)periodicLastExecutionDate;
                    }

                }



            }

            public override string ToString()
            {
                var bezahlCodePayload = $"bank://{authority}?";

                bezahlCodePayload += $"name={Uri.EscapeDataString(name)}&";

                if (authority != AuthorityType.Contact && authority != AuthorityType.ContactV2)
                {
                    //Handle what is same for all payments
#pragma warning disable CS0612
                    if (authority == AuthorityType.Periodicsinglepayment || authority == AuthorityType.Singledirectdebit || authority == AuthorityType.Singlepayment)
#pragma warning restore CS0612
                    {
                        bezahlCodePayload += $"account={account}&";
                        bezahlCodePayload += $"bnc={bnc}&";
                        if (postingKey > 0)
                            bezahlCodePayload += $"postingkey={postingKey}&";
                    }
                    else
                    {
                        bezahlCodePayload += $"iban={iban}&";
                        bezahlCodePayload += $"bic={bic}&";

                        if (!string.IsNullOrEmpty(sepaReference))
                            bezahlCodePayload += $"separeference={Uri.EscapeDataString(sepaReference)}&";

                        if (authority == AuthorityType.Singledirectdebitsepa)
                        {
                            if (!string.IsNullOrEmpty(creditorId))
                                bezahlCodePayload += $"creditorid={Uri.EscapeDataString(creditorId)}&";
                            if (!string.IsNullOrEmpty(mandateId))
                                bezahlCodePayload += $"mandateid={Uri.EscapeDataString(mandateId)}&";
                            if (dateOfSignature != DateTime.MinValue)
                                bezahlCodePayload += $"dateofsignature={dateOfSignature.ToString("ddMMyyyy")}&";
                        }
                    }
                    bezahlCodePayload += $"amount={amount:0.00}&".Replace(".", ",");

                    if (!string.IsNullOrEmpty(reason))
                        bezahlCodePayload += $"reason={Uri.EscapeDataString(reason)}&";
                    bezahlCodePayload += $"currency={currency}&";
                    bezahlCodePayload += $"executiondate={executionDate.ToString("ddMMyyyy")}&";
#pragma warning disable CS0612
                    if (authority == AuthorityType.Periodicsinglepayment || authority == AuthorityType.Periodicsinglepaymentsepa)
                    {
                        bezahlCodePayload += $"periodictimeunit={periodicTimeunit}&";
                        bezahlCodePayload += $"periodictimeunitrotation={periodicTimeunitRotation}&";
                        if (periodicFirstExecutionDate != DateTime.MinValue)
                            bezahlCodePayload += $"periodicfirstexecutiondate={periodicFirstExecutionDate.ToString("ddMMyyyy")}&";
                        if (periodicLastExecutionDate != DateTime.MinValue)
                            bezahlCodePayload += $"periodiclastexecutiondate={periodicLastExecutionDate.ToString("ddMMyyyy")}&";
                    }
#pragma warning restore CS0612
                }
                else
                {
                    //Handle what is same for all contacts
                    if (authority == AuthorityType.Contact)
                    {
                        bezahlCodePayload += $"account={account}&";
                        bezahlCodePayload += $"bnc={bnc}&";
                    }
                    else if (authority == AuthorityType.ContactV2)
                    {
                        if (!string.IsNullOrEmpty(account) && !string.IsNullOrEmpty(bnc))
                        {
                            bezahlCodePayload += $"account={account}&";
                            bezahlCodePayload += $"bnc={bnc}&";
                        }
                        else
                        {
                            bezahlCodePayload += $"iban={iban}&";
                            bezahlCodePayload += $"bic={bic}&";
                        }
                    }

                    if (!string.IsNullOrEmpty(reason))
                        bezahlCodePayload += $"reason={Uri.EscapeDataString(reason)}&";
                }

                return bezahlCodePayload.Trim('&');
            }

            /// <summary>
            /// ISO 4217 currency codes
            /// </summary>
            public enum Currency
            {
                AED = 784,
                AFN = 971,
                ALL = 008,
                AMD = 051,
                ANG = 532,
                AOA = 973,
                ARS = 032,
                AUD = 036,
                AWG = 533,
                AZN = 944,
                BAM = 977,
                BBD = 052,
                BDT = 050,
                BGN = 975,
                BHD = 048,
                BIF = 108,
                BMD = 060,
                BND = 096,
                BOB = 068,
                BOV = 984,
                BRL = 986,
                BSD = 044,
                BTN = 064,
                BWP = 072,
                BYR = 974,
                BZD = 084,
                CAD = 124,
                CDF = 976,
                CHE = 947,
                CHF = 756,
                CHW = 948,
                CLF = 990,
                CLP = 152,
                CNY = 156,
                COP = 170,
                COU = 970,
                CRC = 188,
                CUC = 931,
                CUP = 192,
                CVE = 132,
                CZK = 203,
                DJF = 262,
                DKK = 208,
                DOP = 214,
                DZD = 012,
                EGP = 818,
                ERN = 232,
                ETB = 230,
                EUR = 978,
                FJD = 242,
                FKP = 238,
                GBP = 826,
                GEL = 981,
                GHS = 936,
                GIP = 292,
                GMD = 270,
                GNF = 324,
                GTQ = 320,
                GYD = 328,
                HKD = 344,
                HNL = 340,
                HRK = 191,
                HTG = 332,
                HUF = 348,
                IDR = 360,
                ILS = 376,
                INR = 356,
                IQD = 368,
                IRR = 364,
                ISK = 352,
                JMD = 388,
                JOD = 400,
                JPY = 392,
                KES = 404,
                KGS = 417,
                KHR = 116,
                KMF = 174,
                KPW = 408,
                KRW = 410,
                KWD = 414,
                KYD = 136,
                KZT = 398,
                LAK = 418,
                LBP = 422,
                LKR = 144,
                LRD = 430,
                LSL = 426,
                LYD = 434,
                MAD = 504,
                MDL = 498,
                MGA = 969,
                MKD = 807,
                MMK = 104,
                MNT = 496,
                MOP = 446,
                MRO = 478,
                MUR = 480,
                MVR = 462,
                MWK = 454,
                MXN = 484,
                MXV = 979,
                MYR = 458,
                MZN = 943,
                NAD = 516,
                NGN = 566,
                NIO = 558,
                NOK = 578,
                NPR = 524,
                NZD = 554,
                OMR = 512,
                PAB = 590,
                PEN = 604,
                PGK = 598,
                PHP = 608,
                PKR = 586,
                PLN = 985,
                PYG = 600,
                QAR = 634,
                RON = 946,
                RSD = 941,
                RUB = 643,
                RWF = 646,
                SAR = 682,
                SBD = 090,
                SCR = 690,
                SDG = 938,
                SEK = 752,
                SGD = 702,
                SHP = 654,
                SLL = 694,
                SOS = 706,
                SRD = 968,
                SSP = 728,
                STD = 678,
                SVC = 222,
                SYP = 760,
                SZL = 748,
                THB = 764,
                TJS = 972,
                TMT = 934,
                TND = 788,
                TOP = 776,
                TRY = 949,
                TTD = 780,
                TWD = 901,
                TZS = 834,
                UAH = 980,
                UGX = 800,
                USD = 840,
                USN = 997,
                UYI = 940,
                UYU = 858,
                UZS = 860,
                VEF = 937,
                VND = 704,
                VUV = 548,
                WST = 882,
                XAF = 950,
                XAG = 961,
                XAU = 959,
                XBA = 955,
                XBB = 956,
                XBC = 957,
                XBD = 958,
                XCD = 951,
                XDR = 960,
                XOF = 952,
                XPD = 964,
                XPF = 953,
                XPT = 962,
                XSU = 994,
                XTS = 963,
                XUA = 965,
                XXX = 999,
                YER = 886,
                ZAR = 710,
                ZMW = 967,
                ZWL = 932
            }


            /// <summary>
            /// Operation modes of the BezahlCode
            /// </summary>
            public enum AuthorityType
            {
                /// <summary>
                /// Single payment (Überweisung)
                /// </summary>
                [Obsolete]
                Singlepayment,
                /// <summary>
                /// Single SEPA payment (SEPA-Überweisung)
                /// </summary>
                Singlepaymentsepa,
                /// <summary>
                /// Single debit (Lastschrift)
                /// </summary>
                [Obsolete]
                Singledirectdebit,
                /// <summary>
                /// Single SEPA debit (SEPA-Lastschrift)
                /// </summary>
                Singledirectdebitsepa,
                /// <summary>
                /// Periodic payment (Dauerauftrag)
                /// </summary>
                [Obsolete]
                Periodicsinglepayment,
                /// <summary>
                /// Periodic SEPA payment (SEPA-Dauerauftrag)
                /// </summary>
                Periodicsinglepaymentsepa,
                /// <summary>
                /// Contact data
                /// </summary>
                Contact,
                /// <summary>
                /// Contact data V2
                /// </summary>
                ContactV2
            }

            public class BezahlCodeException : Exception
            {
                public BezahlCodeException()
                {
                }

                public BezahlCodeException(string message)
                    : base(message)
                {
                }

                public BezahlCodeException(string message, Exception inner)
                    : base(message, inner)
                {
                }
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

        public class OneTimePassword : Payload
        {
            //https://github.com/google/google-authenticator/wiki/Key-Uri-Format
            public OneTimePasswordAuthType Type { get; set; } = OneTimePasswordAuthType.TOTP;
            public string Secret { get; set; }

            public OneTimePasswordAuthAlgorithm AuthAlgorithm { get; set; } = OneTimePasswordAuthAlgorithm.SHA1;

            [Obsolete("This property is obsolete, use " + nameof(AuthAlgorithm) + " instead", false)]
            public OoneTimePasswordAuthAlgorithm Algorithm
            {
                get => (OoneTimePasswordAuthAlgorithm)Enum.Parse(typeof(OoneTimePasswordAuthAlgorithm), AuthAlgorithm.ToString());
                set => AuthAlgorithm = (OneTimePasswordAuthAlgorithm)Enum.Parse(typeof(OneTimePasswordAuthAlgorithm), value.ToString());
            }

            public string Issuer { get; set; }
            public string Label { get; set; }
            public int Digits { get; set; } = 6;
            public int? Counter { get; set; } = null;
            public int? Period { get; set; } = 30;

            public enum OneTimePasswordAuthType
            {
                TOTP,
                HOTP,
            }

            public enum OneTimePasswordAuthAlgorithm
            {
                SHA1,
                SHA256,
                SHA512,
            }

            [Obsolete("This enum is obsolete, use " + nameof(OneTimePasswordAuthAlgorithm) + " instead", false)]
            public enum OoneTimePasswordAuthAlgorithm
            {
                SHA1,
                SHA256,
                SHA512,
            }

            public override string ToString()
            {
                switch (Type)
                {
                    case OneTimePasswordAuthType.TOTP:
                        return TimeToString();
                    case OneTimePasswordAuthType.HOTP:
                        return HmacToString();
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            // Note: Issuer:Label must only contain 1 : if either of the Issuer or the Label has a : then it is invalid.
            // Defaults are 6 digits and 30 for Period
            private string HmacToString()
            {
                var sb = new StringBuilder("otpauth://hotp/");
                ProcessCommonFields(sb);
                var actualCounter = Counter ?? 1;
                sb.Append("&counter=" + actualCounter);
                return sb.ToString();
            }

            private string TimeToString()
            {
                if (Period == null)
                {
                    throw new Exception("Period must be set when using OneTimePasswordAuthType.TOTP");
                }

                var sb = new StringBuilder("otpauth://totp/");

                ProcessCommonFields(sb);

                if (Period != 30)
                {
                    sb.Append("&period=" + Period);
                }

                return sb.ToString();
            }

            private void ProcessCommonFields(StringBuilder sb)
            {
                if (String.IsNullOrWhiteSpace(Secret))
                {
                    throw new Exception("Secret must be a filled out base32 encoded string");
                }
                var strippedSecret = Secret.Replace(" ", "");
                string escapedIssuer = null;
                string label = null;

                if (!String.IsNullOrWhiteSpace(Issuer))
                {
                    if (Issuer.Contains(":"))
                    {
                        throw new Exception("Issuer must not have a ':'");
                    }
                    escapedIssuer = Uri.EscapeDataString(Issuer);
                }

                if (!String.IsNullOrWhiteSpace(Label) && Label.Contains(":"))
                {
                    throw new Exception("Label must not have a ':'");
                }

                if (Label != null && Issuer != null)
                {
                    label = Issuer + ":" + Label;
                }
                else if (Issuer != null)
                {
                    label = Issuer;
                }

                if (label != null)
                {
                    sb.Append(label);
                }

                sb.Append("?secret=" + strippedSecret);

                if (escapedIssuer != null)
                {
                    sb.Append("&issuer=" + escapedIssuer);
                }

                if (Digits != 6)
                {
                    sb.Append("&digits=" + Digits);
                }
            }
        }

        public class ShadowSocksConfig : Payload
        {
            private readonly string hostname, password, tag, methodStr, parameter;
            private readonly Method method;
            private readonly int port;
            private readonly Dictionary<string, string> encryptionTexts = new() {
                { "Chacha20IetfPoly1305", "chacha20-ietf-poly1305" },
                { "Aes128Gcm", "aes-128-gcm" },
                { "Aes192Gcm", "aes-192-gcm" },
                { "Aes256Gcm", "aes-256-gcm" },

                { "XChacha20IetfPoly1305", "xchacha20-ietf-poly1305" },

                { "Aes128Cfb", "aes-128-cfb" },
                { "Aes192Cfb", "aes-192-cfb" },
                { "Aes256Cfb", "aes-256-cfb" },
                { "Aes128Ctr", "aes-128-ctr" },
                { "Aes192Ctr", "aes-192-ctr" },
                { "Aes256Ctr", "aes-256-ctr" },
                { "Camellia128Cfb", "camellia-128-cfb" },
                { "Camellia192Cfb", "camellia-192-cfb" },
                { "Camellia256Cfb", "camellia-256-cfb" },
                { "Chacha20Ietf", "chacha20-ietf" },

                { "Aes256Cb", "aes-256-cfb" },

                { "Aes128Ofb", "aes-128-ofb" },
                { "Aes192Ofb", "aes-192-ofb" },
                { "Aes256Ofb", "aes-256-ofb" },
                { "Aes128Cfb1", "aes-128-cfb1" },
                { "Aes192Cfb1", "aes-192-cfb1" },
                { "Aes256Cfb1", "aes-256-cfb1" },
                { "Aes128Cfb8", "aes-128-cfb8" },
                { "Aes192Cfb8", "aes-192-cfb8" },
                { "Aes256Cfb8", "aes-256-cfb8" },

                { "Chacha20", "chacha20" },
                { "BfCfb", "bf-cfb" },
                { "Rc4Md5", "rc4-md5" },
                { "Salsa20", "salsa20" },

                { "DesCfb", "des-cfb" },
                { "IdeaCfb", "idea-cfb" },
                { "Rc2Cfb", "rc2-cfb" },
                { "Cast5Cfb", "cast5-cfb" },
                { "Salsa20Ctr", "salsa20-ctr" },
                { "Rc4", "rc4" },
                { "SeedCfb", "seed-cfb" },
                { "Table", "table" }
            };

            /// <summary>
            /// Generates a ShadowSocks proxy config payload.
            /// </summary>
            /// <param name="hostname">Hostname of the ShadowSocks proxy</param>
            /// <param name="port">Port of the ShadowSocks proxy</param>
            /// <param name="password">Password of the SS proxy</param>
            /// <param name="method">Encryption type</param>
            /// <param name="tag">Optional tag line</param>
            public ShadowSocksConfig(string hostname, int port, string password, Method method, string tag = null) :
                this(hostname, port, password, method, null, tag)
            { }

            public ShadowSocksConfig(string hostname, int port, string password, Method method, string plugin, string pluginOption, string tag = null) :
                this(hostname, port, password, method, new Dictionary<string, string>
                {
                    ["plugin"] = plugin + (
                    string.IsNullOrEmpty(pluginOption)
                    ? ""
                    : $";{pluginOption}"
                )
                }, tag)
            { }
            private readonly Dictionary<string, string> urlEncodeTable = new()
            {
                [" "] = "+",
                ["\0"] = "%00",
                ["\t"] = "%09",
                ["\n"] = "%0a",
                ["\r"] = "%0d",
                ["\""] = "%22",
                ["#"] = "%23",
                ["$"] = "%24",
                ["%"] = "%25",
                ["&"] = "%26",
                ["'"] = "%27",
                ["+"] = "%2b",
                [","] = "%2c",
                ["/"] = "%2f",
                [":"] = "%3a",
                [";"] = "%3b",
                ["<"] = "%3c",
                ["="] = "%3d",
                [">"] = "%3e",
                ["?"] = "%3f",
                ["@"] = "%40",
                ["["] = "%5b",
                ["\\"] = "%5c",
                ["]"] = "%5d",
                ["^"] = "%5e",
                ["`"] = "%60",
                ["{"] = "%7b",
                ["|"] = "%7c",
                ["}"] = "%7d",
                ["~"] = "%7e",
            };

            private string UrlEncode(string i)
            {
                var j = i;
                foreach (var kv in urlEncodeTable)
                {
                    j = j.Replace(kv.Key, kv.Value);
                }
                return j;
            }

            public ShadowSocksConfig(string hostname, int port, string password, Method method, Dictionary<string, string> parameters, string tag = null)
            {
                this.hostname = Uri.CheckHostName(hostname) == UriHostNameType.IPv6
                    ? $"[{hostname}]"
                    : hostname;
                if (port < 1 || port > 65535)
                    throw new ShadowSocksConfigException("Value of 'port' must be within 0 and 65535.");
                this.port = port;
                this.password = password;
                this.method = method;
                methodStr = encryptionTexts[method.ToString()];
                this.tag = tag;

                if (parameters != null)
                    parameter =
                        string.Join("&",
                        parameters.Select(
                            kv => $"{UrlEncode(kv.Key)}={UrlEncode(kv.Value)}"
                        ).ToArray());
            }

            public override string ToString()
            {
                if (string.IsNullOrEmpty(parameter))
                {
                    var connectionString = $"{methodStr}:{password}@{hostname}:{port}";
                    var connectionStringEncoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(connectionString));
                    return $"ss://{connectionStringEncoded}{(!string.IsNullOrEmpty(tag) ? $"#{tag}" : string.Empty)}";
                }
                var authString = $"{methodStr}:{password}";
                var authStringEncoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(authString))
                    .Replace('+', '-')
                    .Replace('/', '_')
                    .TrimEnd('=');
                return $"ss://{authStringEncoded}@{hostname}:{port}/?{parameter}{(!string.IsNullOrEmpty(tag) ? $"#{tag}" : string.Empty)}";
            }

            public enum Method
            {
                // AEAD
                Chacha20IetfPoly1305,
                Aes128Gcm,
                Aes192Gcm,
                Aes256Gcm,
                // AEAD, not standard
                XChacha20IetfPoly1305,
                // Stream cipher
                Aes128Cfb,
                Aes192Cfb,
                Aes256Cfb,
                Aes128Ctr,
                Aes192Ctr,
                Aes256Ctr,
                Camellia128Cfb,
                Camellia192Cfb,
                Camellia256Cfb,
                Chacha20Ietf,
                // alias of Aes256Cfb
                Aes256Cb,
                // Stream cipher, not standard
                Aes128Ofb,
                Aes192Ofb,
                Aes256Ofb,
                Aes128Cfb1,
                Aes192Cfb1,
                Aes256Cfb1,
                Aes128Cfb8,
                Aes192Cfb8,
                Aes256Cfb8,
                // Stream cipher, deprecated
                Chacha20,
                BfCfb,
                Rc4Md5,
                Salsa20,
                // Not standard and not in acitve use
                DesCfb,
                IdeaCfb,
                Rc2Cfb,
                Cast5Cfb,
                Salsa20Ctr,
                Rc4,
                SeedCfb,
                Table
            }

            public class ShadowSocksConfigException : Exception
            {
                public ShadowSocksConfigException()
                {
                }

                public ShadowSocksConfigException(string message)
                    : base(message)
                {
                }

                public ShadowSocksConfigException(string message, Exception inner)
                    : base(message, inner)
                {
                }
            }
        }

        public class MoneroTransaction : Payload
        {
            private readonly string address, txPaymentId, recipientName, txDescription;
            private readonly float? txAmount;

            /// <summary>
            /// Creates a monero transaction payload
            /// </summary>
            /// <param name="address">Receiver's monero address</param>
            /// <param name="txAmount">Amount to transfer</param>
            /// <param name="txPaymentId">Payment id</param>
            /// <param name="recipientName">Recipient's name</param>
            /// <param name="txDescription">Reference text / payment description</param>
            public MoneroTransaction(string address, float? txAmount = null, string txPaymentId = null, string recipientName = null, string txDescription = null)
            {
                if (string.IsNullOrEmpty(address))
                    throw new MoneroTransactionException("The address is mandatory and has to be set.");
                this.address = address;
                if (txAmount != null && txAmount <= 0)
                    throw new MoneroTransactionException("Value of 'txAmount' must be greater than 0.");
                this.txAmount = txAmount;
                this.txPaymentId = txPaymentId;
                this.recipientName = recipientName;
                this.txDescription = txDescription;
            }

            public override string ToString()
            {
                var moneroUri = $"monero://{address}{(!string.IsNullOrEmpty(txPaymentId) || !string.IsNullOrEmpty(recipientName) || !string.IsNullOrEmpty(txDescription) || txAmount != null ? "?" : string.Empty)}";
                moneroUri += (!string.IsNullOrEmpty(txPaymentId) ? $"tx_payment_id={Uri.EscapeDataString(txPaymentId)}&" : string.Empty);
                moneroUri += (!string.IsNullOrEmpty(recipientName) ? $"recipient_name={Uri.EscapeDataString(recipientName)}&" : string.Empty);
                moneroUri += (txAmount != null ? $"tx_amount={txAmount.ToString().Replace(",", ".")}&" : string.Empty);
                moneroUri += (!string.IsNullOrEmpty(txDescription) ? $"tx_description={Uri.EscapeDataString(txDescription)}" : string.Empty);
                return moneroUri.TrimEnd('&');
            }


            public class MoneroTransactionException : Exception
            {
                public MoneroTransactionException()
                {
                }

                public MoneroTransactionException(string message)
                    : base(message)
                {
                }

                public MoneroTransactionException(string message, Exception inner)
                    : base(message, inner)
                {
                }
            }
        }

        public class SlovenianUpnQr : Payload
        {
            //Keep in mind, that the ECC level has to be set to "M", version to 15 and ECI to EciMode.Iso8859_2 when generating a SlovenianUpnQr!
            //SlovenianUpnQr specification: https://www.upn-qr.si/uploads/files/NavodilaZaProgramerjeUPNQR.pdf

            private readonly string payerName;
            private readonly string payerAddress;
            private readonly string payerPlace;
            private readonly string amount;
            private readonly string code;
            private readonly string purpose;
            private readonly string deadLine;
            private readonly string recipientIban;
            private readonly string recipientName;
            private readonly string recipientAddress;
            private readonly string recipientPlace;
            private readonly string recipientSiModel;
            private readonly string recipientSiReference;

            public override int Version => 15;
            public override QRCodeGenerator.ECCLevel EccLevel => QRCodeGenerator.ECCLevel.M;
            public override QRCodeGenerator.EciMode EciMode => QRCodeGenerator.EciMode.Iso8859_2;

            private string LimitLength(string value, int maxLength)
            {
                return (value.Length <= maxLength) ? value : value.Substring(0, maxLength);
            }

            public SlovenianUpnQr(string payerName, string payerAddress, string payerPlace, string recipientName, string recipientAddress, string recipientPlace, string recipientIban, string description, double amount, string recipientSiModel = "SI00", string recipientSiReference = "", string code = "OTHR") :
                this(payerName, payerAddress, payerPlace, recipientName, recipientAddress, recipientPlace, recipientIban, description, amount, null, recipientSiModel, recipientSiReference, code)
            { }

            public SlovenianUpnQr(string payerName, string payerAddress, string payerPlace, string recipientName, string recipientAddress, string recipientPlace, string recipientIban, string description, double amount, DateTime? deadline, string recipientSiModel = "SI99", string recipientSiReference = "", string code = "OTHR")
            {
                this.payerName = LimitLength(payerName.Trim(), 33);
                this.payerAddress = LimitLength(payerAddress.Trim(), 33);
                this.payerPlace = LimitLength(payerPlace.Trim(), 33);
                this.amount = FormatAmount(amount);
                this.code = LimitLength(code.Trim().ToUpper(), 4);
                purpose = LimitLength(description.Trim(), 42);
                deadLine = (deadline == null) ? "" : deadline?.ToString("dd.MM.yyyy");
                this.recipientIban = LimitLength(recipientIban.Trim(), 34);
                this.recipientName = LimitLength(recipientName.Trim(), 33);
                this.recipientAddress = LimitLength(recipientAddress.Trim(), 33);
                this.recipientPlace = LimitLength(recipientPlace.Trim(), 33);
                this.recipientSiModel = LimitLength(recipientSiModel.Trim().ToUpper(), 4);
                this.recipientSiReference = LimitLength(recipientSiReference.Trim(), 22);
            }

            private string FormatAmount(double amount)
            {
                var amt = (int)Math.Round(amount * 100.0);
                return $"{amt:00000000000}";
            }

            private int CalculateChecksum()
            {
                var cs = 5 + payerName.Length; //5 = UPNQR constant Length
                cs += payerAddress.Length;
                cs += payerPlace.Length;
                cs += amount.Length;
                cs += code.Length;
                cs += purpose.Length;
                cs += deadLine.Length;
                cs += recipientIban.Length;
                cs += recipientName.Length;
                cs += recipientAddress.Length;
                cs += recipientPlace.Length;
                cs += recipientSiModel.Length;
                cs += recipientSiReference.Length;
                cs += 19;
                return cs;
            }

            public override string ToString()
            {
                var sb = new StringBuilder();
                sb.Append("UPNQR");
                sb.Append('\n').Append('\n').Append('\n').Append('\n').Append('\n');
                sb.Append(payerName).Append('\n');
                sb.Append(payerAddress).Append('\n');
                sb.Append(payerPlace).Append('\n');
                sb.Append(amount).Append('\n').Append('\n').Append('\n');
                sb.Append(code.ToUpper()).Append('\n');
                sb.Append(purpose).Append('\n');
                sb.Append(deadLine).Append('\n');
                sb.Append(recipientIban.ToUpper()).Append('\n');
                sb.Append(recipientSiModel).Append(recipientSiReference).Append('\n');
                sb.Append(recipientName).Append('\n');
                sb.Append(recipientAddress).Append('\n');
                sb.Append(recipientPlace).Append('\n');
                sb.AppendFormat("{0:000}", CalculateChecksum()).Append('\n');
                return sb.ToString();
            }
        }

        public class RussiaPaymentOrder : Payload
        {
            // Specification of RussianPaymentOrder
            //https://docs.cntd.ru/document/1200110981
            //https://roskazna.gov.ru/upload/iblock/5fa/gost_r_56042_2014.pdf
            //https://sbqr.ru/standard/files/standart.pdf

            // Specification of data types described in the above standard
            // https://gitea.sergeybochkov.com/bochkov/emuik/src/commit/d18f3b550f6415ea4a4a5e6097eaab4661355c72/template/ed

            // Tool for QR validation
            // https://www.sbqr.ru/validator/index.html

            //base
            private readonly CharacterSets characterSet;
            private readonly MandatoryFields mFields;
            private readonly OptionalFields oFields;
            private string separator = "|";

            private RussiaPaymentOrder()
            {
                mFields = new MandatoryFields();
                oFields = new OptionalFields();
            }

            /// <summary>
            /// Generates a RussiaPaymentOrder payload
            /// </summary>            
            /// <param name="name">Name of the payee (Наименование получателя платежа)</param>
            /// <param name="personalAcc">Beneficiary account number (Номер счета получателя платежа)</param>
            /// <param name="bankName">Name of the beneficiary's bank (Наименование банка получателя платежа)</param>
            /// <param name="BIC">BIC (БИК)</param>
            /// <param name="correspAcc">Box number / account payee's bank (Номер кор./сч. банка получателя платежа)</param>
            /// <param name="optionalFields">An (optional) object of additional fields</param>
            /// <param name="characterSet">Type of encoding (default UTF-8)</param>
            public RussiaPaymentOrder(string name, string personalAcc, string bankName, string BIC, string correspAcc, OptionalFields optionalFields = null, CharacterSets characterSet = CharacterSets.Utf8) : this()
            {
                this.characterSet = characterSet;
                mFields.Name = ValidateInput(name, "Name", @"^.{1,160}$");
                mFields.PersonalAcc = ValidateInput(personalAcc, "PersonalAcc", @"^[1-9]\d{4}[0-9ABCEHKMPTX]\d{14}$");
                mFields.BankName = ValidateInput(bankName, "BankName", @"^.{1,45}$");
                mFields.BIC = ValidateInput(BIC, "BIC", @"^\d{9}$");
                mFields.CorrespAcc = ValidateInput(correspAcc, "CorrespAcc", @"^[1-9]\d{4}[0-9ABCEHKMPTX]\d{14}$");

                if (optionalFields != null)
                    oFields = optionalFields;
            }

            /// <summary>
            /// Returns payload as string.
            /// </summary>
            /// <remarks>⚠ Attention: If CharacterSets was set to windows-1251 or koi8-r you should use ToBytes() instead of ToString() and pass the bytes to CreateQrCode()!</remarks>
            /// <returns></returns>
            public override string ToString()
            {
                var cp = characterSet.ToString().Replace("_", "-");
                var bytes = ToBytes();

                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                return Encoding.GetEncoding(cp).GetString(bytes);
            }

            /// <summary>
            /// Returns payload as byte[].
            /// </summary>
            /// <remarks>Should be used if CharacterSets equals windows-1251 or koi8-r</remarks>
            /// <returns></returns>

            public byte[] ToBytes()
            {
                //Calculate the separator
                separator = DetermineSeparator();

                //Create the payload string
                var ret = $"ST0001" + ((int)characterSet) + //(separator != "|" ? separator : "") + 
                          $"{separator}Name={mFields.Name}" +
                          $"{separator}PersonalAcc={mFields.PersonalAcc}" +
                          $"{separator}BankName={mFields.BankName}" +
                          $"{separator}BIC={mFields.BIC}" +
                          $"{separator}CorrespAcc={mFields.CorrespAcc}";

                //Add optional fields, if filled
                var optionalFieldsList = GetOptionalFieldsAsList();
                if (optionalFieldsList.Count > 0)
                    ret += $"|{string.Join("|", optionalFieldsList.ToArray())}";
                ret += separator;

                //Encode return string as byte[] with correct CharacterSet
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                var cp = characterSet.ToString().Replace("_", "-");
                var bytesOut = Encoding.Convert(Encoding.UTF8, Encoding.GetEncoding(cp), Encoding.UTF8.GetBytes(ret));
                if (bytesOut.Length > 300)
                    throw new RussiaPaymentOrderException($"Data too long. Payload must not exceed 300 bytes, but actually is {bytesOut.Length} bytes long. Remove additional data fields or shorten strings/values.");
                return bytesOut;
            }


            /// <summary>
            /// Determines a valid separator
            /// </summary>
            /// <returns></returns>
            private string DetermineSeparator()
            {
                // See chapter 5.2.1 of Standard (https://sbqr.ru/standard/files/standart.pdf)

                var mandatoryValues = GetMandatoryFieldsAsList();
                var optionalValues = GetOptionalFieldsAsList();

                // Possible candidates for field separation
                var separatorCandidates = new string[] { "|", "#", ";", ":", "^", "_", "~", "{", "}", "!", "#", "$", "%", "&", "(", ")", "*", "+", ",", "/", "@" };
                foreach (var sepCandidate in separatorCandidates)
                {
                    if (!mandatoryValues.Any(x => x.Contains(sepCandidate)) && !optionalValues.Any(x => x.Contains(sepCandidate)))
                        return sepCandidate;
                }
                throw new RussiaPaymentOrderException("No valid separator found.");
            }

            /// <summary>
            /// Takes all optional fields that are not null and returns their string representation
            /// </summary>
            /// <returns>A List of strings</returns>
            private List<string> GetOptionalFieldsAsList()
            {
                return oFields.GetType().GetProperties()
                        .Where(field => field.GetValue(oFields, null) != null)
                        .Select(field =>
                        {
                            var objValue = field.GetValue(oFields, null);
                            var value = field.PropertyType == typeof(DateTime?) ? ((DateTime)objValue).ToString("dd.MM.yyyy") : objValue.ToString();
                            return $"{field.Name}={value}";
                        })
                        .ToList();
            }


            /// <summary>
            /// Takes all mandatory fields that are not null and returns their string representation
            /// </summary>
            /// <returns>A List of strings</returns>
            private List<string> GetMandatoryFieldsAsList()
            {
                return mFields.GetType().GetFields()
                        .Where(field => field.GetValue(mFields) != null)
                        .Select(field =>
                        {
                            var objValue = field.GetValue(mFields);
                            var value = field.FieldType == typeof(DateTime?) ? ((DateTime)objValue).ToString("dd.MM.yyyy") : objValue.ToString();
                            return $"{field.Name}={value}";
                        })
                        .ToList();
            }

            /// <summary>
            /// Validates a string against a given Regex pattern. Returns input if it matches the Regex expression (=valid) or throws Exception in case there's a mismatch
            /// </summary>
            /// <param name="input">String to be validated</param>
            /// <param name="fieldName">Name/descriptor of the string to be validated</param>
            /// <param name="pattern">A regex pattern to be used for validation</param>
            /// <param name="errorText">An optional error text. If null, a standard error text is generated</param>
            /// <returns>Input value (in case it is valid)</returns>
            private static string ValidateInput(string input, string fieldName, string pattern, string errorText = null)
            {
                return ValidateInput(input, fieldName, new string[] { pattern }, errorText);
            }

            /// <summary>
            /// Validates a string against one or more given Regex patterns. Returns input if it matches all regex expressions (=valid) or throws Exception in case there's a mismatch
            /// </summary>
            /// <param name="input">String to be validated</param>
            /// <param name="fieldName">Name/descriptor of the string to be validated</param>
            /// <param name="patterns">An array of regex patterns to be used for validation</param>
            /// <param name="errorText">An optional error text. If null, a standard error text is generated</param>
            /// <returns>Input value (in case it is valid)</returns>
            private static string ValidateInput(string input, string fieldName, string[] patterns, string errorText = null)
            {
                if (input == null)
                    throw new RussiaPaymentOrderException($"The input for '{fieldName}' must not be null.");
                foreach (var pattern in patterns)
                {
                    if (!Regex.IsMatch(input, pattern))
                        throw new RussiaPaymentOrderException(errorText ?? $"The input for '{fieldName}' ({input}) doesn't match the pattern {pattern}");
                }
                return input;
            }

            private class MandatoryFields
            {
                public string Name;
                public string PersonalAcc;
                public string BankName;
                public string BIC;
                public string CorrespAcc;
            }

            public class OptionalFields
            {
                private string sum;
                /// <summary>
                /// Payment amount, in kopecks (FTI’s Amount.)
                /// <para>Сумма платежа, в копейках</para>
                /// </summary>
                public string Sum
                {
                    get => sum;
                    set => sum = ValidateInput(value, "Sum", @"^\d{1,18}$");
                }

                private string purpose;
                /// <summary>
                /// Payment name (purpose)
                /// <para>Наименование платежа (назначение)</para>
                /// </summary>
                public string Purpose
                {
                    get => purpose;
                    set => purpose = ValidateInput(value, "Purpose", @"^.{1,160}$");
                }

                private string payeeInn;
                /// <summary>
                /// Payee's INN (Resident Tax Identification Number; Text, up to 12 characters.)
                /// <para>ИНН получателя платежа</para>
                /// </summary>
                public string PayeeINN
                {
                    get => payeeInn;
                    set => payeeInn = ValidateInput(value, "PayeeINN", @"^.{1,12}$");
                }

                private string payerInn;
                /// <summary>
                /// Payer's INN (Resident Tax Identification Number; Text, up to 12 characters.)
                /// <para>ИНН плательщика</para>
                /// </summary>
                public string PayerINN
                {
                    get => payerInn;
                    set => payerInn = ValidateInput(value, "PayerINN", @"^.{1,12}$");
                }

                private string drawerStatus;
                /// <summary>
                /// Status compiler payment document
                /// <para>Статус составителя платежного документа</para>
                /// </summary>
                public string DrawerStatus
                {
                    get => drawerStatus;
                    set => drawerStatus = ValidateInput(value, "DrawerStatus", @"^.{1,2}$");
                }

                private string kpp;
                /// <summary>
                /// KPP of the payee (Tax Registration Code; Text, up to 9 characters.)
                /// <para>КПП получателя платежа</para>
                /// </summary>
                public string KPP
                {
                    get => kpp;
                    set => kpp = ValidateInput(value, "KPP", @"^.{1,9}$");
                }

                private string cbc;
                /// <summary>
                /// CBC
                /// <para>КБК</para>
                /// </summary>
                public string CBC
                {
                    get => cbc;
                    set => cbc = ValidateInput(value, "CBC", @"^.{1,20}$");
                }

                private string oktmo;
                /// <summary>
                /// All-Russian classifier territories of municipal formations
                /// <para>Общероссийский классификатор территорий муниципальных образований</para>
                /// </summary>
                public string OKTMO
                {
                    get => oktmo;
                    set => oktmo = ValidateInput(value, "OKTMO", @"^.{1,11}$");
                }

                private string paytReason;
                /// <summary>
                /// Basis of tax payment
                /// <para>Основание налогового платежа</para>
                /// </summary>
                public string PaytReason
                {
                    get => paytReason;
                    set => paytReason = ValidateInput(value, "PaytReason", @"^.{1,2}$");
                }

                private string taxPeriod;
                /// <summary>
                /// Taxable period
                /// <para>Налоговый период</para>
                /// </summary>
                public string TaxPeriod
                {
                    get => taxPeriod;
                    set => taxPeriod = ValidateInput(value, "ТaxPeriod", @"^.{1,10}$");
                }

                private string docNo;
                /// <summary>
                /// Document number
                /// <para>Номер документа</para>
                /// </summary>
                public string DocNo
                {
                    get => docNo;
                    set => docNo = ValidateInput(value, "DocNo", @"^.{1,15}$");
                }

                /// <summary>
                /// Document date
                /// <para>Дата документа</para>
                /// </summary>
                public DateTime? DocDate { get; set; }

                private string taxPaytKind;
                /// <summary>
                /// Payment type
                /// <para>Тип платежа</para>
                /// </summary>
                public string TaxPaytKind
                {
                    get => taxPaytKind;
                    set => taxPaytKind = ValidateInput(value, "TaxPaytKind", @"^.{1,2}$");
                }

                /**************************************************************************
                 * The following fields are no further specified in the standard
                 * document (https://sbqr.ru/standard/files/standart.pdf) thus there
                 * is no addition input validation implemented.
                 * **************************************************************************/

                /// <summary>
                /// Payer's surname
                /// <para>Фамилия плательщика</para>
                /// </summary>
                public string LastName { get; set; }

                /// <summary>
                /// Payer's name
                /// <para>Имя плательщика</para>
                /// </summary>
                public string FirstName { get; set; }

                /// <summary>
                /// Payer's patronymic
                /// <para>Отчество плательщика</para>
                /// </summary>
                public string MiddleName { get; set; }

                /// <summary>
                /// Payer's address
                /// <para>Адрес плательщика</para>
                /// </summary>
                public string PayerAddress { get; set; }

                /// <summary>
                /// Personal account of a budget recipient
                /// <para>Лицевой счет бюджетного получателя</para>
                /// </summary>
                public string PersonalAccount { get; set; }

                /// <summary>
                /// Payment document index
                /// <para>Индекс платежного документа</para>
                /// </summary>
                public string DocIdx { get; set; }

                /// <summary>
                /// Personal account number in the personalized accounting system in the Pension Fund of the Russian Federation - SNILS
                /// <para>№ лицевого счета в системе персонифицированного учета в ПФР - СНИЛС</para>
                /// </summary>
                public string PensAcc { get; set; }

                /// <summary>
                /// Number of contract
                /// <para>Номер договора</para>
                /// </summary>
                public string Contract { get; set; }

                /// <summary>
                /// Personal account number of the payer in the organization (in the accounting system of the PU)
                /// <para>Номер лицевого счета плательщика в организации (в системе учета ПУ)</para>
                /// </summary>
                public string PersAcc { get; set; }

                /// <summary>
                /// Apartment number
                /// <para>Номер квартиры</para>
                /// </summary>
                public string Flat { get; set; }

                /// <summary>
                /// Phone number
                /// <para>Номер телефона</para>
                /// </summary>
                public string Phone { get; set; }

                /// <summary>
                /// DUL payer type
                /// <para>Вид ДУЛ плательщика</para>
                /// </summary>
                public string PayerIdType { get; set; }

                /// <summary>
                /// DUL number of the payer
                /// <para>Номер ДУЛ плательщика</para>
                /// </summary>
                public string PayerIdNum { get; set; }

                /// <summary>
                /// FULL NAME. child / student
                /// <para>Ф.И.О. ребенка/учащегося</para>
                /// </summary>
                public string ChildFio { get; set; }

                /// <summary>
                /// Date of birth
                /// <para>Дата рождения</para>
                /// </summary>
                public DateTime? BirthDate { get; set; }

                /// <summary>
                /// Due date / Invoice date
                /// <para>Срок платежа/дата выставления счета</para>
                /// </summary>
                public string PaymTerm { get; set; }

                /// <summary>
                /// Payment period
                /// <para>Период оплаты</para>
                /// </summary>
                public string PaymPeriod { get; set; }

                /// <summary>
                /// Payment type
                /// <para>Вид платежа</para>
                /// </summary>
                public string Category { get; set; }

                /// <summary>
                /// Service code / meter name
                /// <para>Код услуги/название прибора учета</para>
                /// </summary>
                public string ServiceName { get; set; }

                /// <summary>
                /// Metering device number
                /// <para>Номер прибора учета</para>
                /// </summary>
                public string CounterId { get; set; }

                /// <summary>
                /// Meter reading
                /// <para>Показание прибора учета</para>
                /// </summary>
                public string CounterVal { get; set; }

                /// <summary>
                /// Notification, accrual, account number
                /// <para>Номер извещения, начисления, счета</para>
                /// </summary>
                public string QuittId { get; set; }

                /// <summary>
                /// Date of notification / accrual / invoice / resolution (for traffic police)
                /// <para>Дата извещения/начисления/счета/постановления (для ГИБДД)</para>
                /// </summary>
                public DateTime? QuittDate { get; set; }

                /// <summary>
                /// Institution number (educational, medical)
                /// <para>Номер учреждения (образовательного, медицинского)</para>
                /// </summary>
                public string InstNum { get; set; }

                /// <summary>
                /// Kindergarten / school class number
                /// <para>Номер группы детсада/класса школы</para>
                /// </summary>
                public string ClassNum { get; set; }

                /// <summary>
                /// Full name of the teacher, specialist providing the service
                /// <para>ФИО преподавателя, специалиста, оказывающего услугу</para>
                /// </summary>
                public string SpecFio { get; set; }

                /// <summary>
                /// Insurance / additional service amount / Penalty amount (in kopecks)
                /// <para>Сумма страховки/дополнительной услуги/Сумма пени (в копейках)</para>
                /// </summary>
                public string AddAmount { get; set; }

                /// <summary>
                /// Resolution number (for traffic police)
                /// <para>Номер постановления (для ГИБДД)</para>
                /// </summary>
                public string RuleId { get; set; }

                /// <summary>
                /// Enforcement Proceedings Number
                /// <para>Номер исполнительного производства</para>
                /// </summary>
                public string ExecId { get; set; }

                /// <summary>
                /// Type of payment code (for example, for payments to Rosreestr)
                /// <para>Код вида платежа (например, для платежей в адрес Росреестра)</para>
                /// </summary>
                public string RegType { get; set; }

                /// <summary>
                /// Unique accrual identifier
                /// <para>Уникальный идентификатор начисления</para>
                /// </summary>
                public string UIN { get; set; }

                /// <summary>
                /// The technical code recommended by the service provider. Maybe used by the receiving organization to call the appropriate processing IT system.
                /// <para>Технический код, рекомендуемый для заполнения поставщиком услуг. Может использоваться принимающей организацией для вызова соответствующей обрабатывающей ИТ-системы.</para>
                /// </summary>
                public TechCode? TechCode { get; set; }
            }

            /// <summary>            
            /// (List of values of the technical code of the payment)
            /// <para>Перечень значений технического кода платежа</para>
            /// </summary>
            public enum TechCode
            {
                Мобильная_связь_стационарный_телефон = 01, // Mobile communication / landline phone
                Коммунальные_услуги_ЖКХAFN = 02, // Utility services / Housing and communal services
                ГИБДД_налоги_пошлины_бюджетные_платежи = 03, // Traffic police / taxes / duties / budget payments
                Охранные_услуги = 04, // Security services
                Услуги_оказываемые_УФМС = 05, // Services provided by the Federal Migration Service
                ПФР = 06, // Pension Fund of Russia
                Погашение_кредитов = 07, // Loan repayment
                Образовательные_учреждения = 08, // Educational institutions
                Интернет_и_ТВ = 09, // Internet and TV
                Электронные_деньги = 10, // Electronic money
                Отдых_и_путешествия = 11, // Recreation and travel
                Инвестиции_и_страхование = 12, // Investments and insurance
                Спорт_и_здоровье = 13, // Sports and health
                Благотворительные_и_общественные_организации = 14, // Charitable and community organizations
                Прочие_услуги = 15 // Other services
            }

            public enum CharacterSets
            {
                Windows1251 = 1,       // Encoding.GetEncoding("windows-1251")
                Utf8 = 2,              // Encoding.UTF8                          
                Koi8R = 3              // Encoding.GetEncoding("koi8-r")
            }

            public class RussiaPaymentOrderException : Exception
            {
                public RussiaPaymentOrderException(string message)
                    : base(message)
                {
                }
            }

        }

        private static bool IsValidIban(string iban)
        {
            // Remove spaces and dashes to clean up the IBAN for processing.
            var ibanCleared = iban.ToUpper().Replace(" ", "").Replace("-", "");

            // Validate the structure: country code (2 letters), checksum (2 digits), and the basic bank account number (BBAN) up to 30 alphanumeric characters.
            var structurallyValid = Regex.IsMatch(ibanCleared, @"^[A-Z]{2}\d{2}[A-Z0-9]{16,30}$");

            if (!structurallyValid)
            {
                // If the IBAN doesn't follow the basic structure, it's invalid.
                return false;
            }

            // Rearrange the IBAN for checksum calculation: move the first 4 characters to the end.
            var rearrangedIban = $"{ibanCleared.Substring(4)}{ibanCleared.Substring(0, 4)}";

            // Convert letters to numbers (A=10, B=11, ..., Z=35) and concatenate.
            var numericIban = rearrangedIban.Select(c => char.IsLetter(c) ? (c - 55).ToString() : c.ToString())
                .Aggregate(string.Empty, (current, next) => current + next);

            // Calculate the checksum using modulo 97.
            var checksumValid = CalculateIbanChecksum(numericIban) == 1;

            // The IBAN is valid if both the structure and checksum are correct.
            return checksumValid;
        }

        private static int CalculateIbanChecksum(string numericIban)
        {
            var remainder = 0;
            var position = 0;

            // Process the numeric IBAN in chunks to avoid overflow issues.
            while (position < numericIban.Length)
            {
                var length = Math.Min(numericIban.Length - position, 9); // Take chunks of up to 9 digits to fit in an int.
                var number = int.Parse(remainder + numericIban.Substring(position, length), CultureInfo.InvariantCulture);
                remainder = number % 97; // Modulo 97 operation.
                position += length;
            }

            return remainder;
        }

        private static bool IsValidQrIban(string iban)
        {
            var foundQrIid = false;
            try
            {
                var ibanCleared = iban.ToUpper().Replace(" ", "").Replace("-", "");
                var possibleQrIid = Convert.ToInt32(ibanCleared.Substring(4, 5));
                foundQrIid = possibleQrIid is >= 30000 and <= 31999;
            }
            catch
            {
                // ignored
            }

            return IsValidIban(iban) && foundQrIid;
        }

        private static bool IsValidBic(string bic)
        {
            return Regex.IsMatch(bic.Replace(" ", ""), @"^([a-zA-Z]{4}[a-zA-Z]{2}[a-zA-Z0-9]{2}([a-zA-Z0-9]{3})?)$");
        }

        private static string ConvertStringToEncoding(string message, string encoding)
        {
            var iso = Encoding.GetEncoding(encoding);
            var utf8 = Encoding.UTF8;
            var utfBytes = utf8.GetBytes(message);
            var isoBytes = Encoding.Convert(utf8, iso, utfBytes);
            return iso.GetString(isoBytes, 0, isoBytes.Length);
        }

        private static string EscapeInput(string inp, bool simple = false)
        {
            char[] forbiddenChars = { '\\', ';', ',', ':' };
            if (simple)
            {
                forbiddenChars = new char[1] { ':' };
            }
            foreach (var c in forbiddenChars)
            {
                inp = inp.Replace(c.ToString(), "\\" + c);
            }
            return inp;
        }
        
        public static bool ChecksumMod10(string digits)
        {
            // Validate input is not null, empty, and has sufficient length.
            if (string.IsNullOrEmpty(digits) || digits.Length < 2)
                return false;

            // Modulus 10 weightings
            var mods = new[] { 0, 9, 4, 6, 8, 2, 7, 1, 3, 5 };

            var remainder = 0;
            // Iterate through all digits except the last one.
            for (var i = 0; i < digits.Length - 1; i++)
            {
                // Convert char digit to int (ASCII to numeric value).
                var num = digits[i] - '0'; // More readable than using 48 (ASCII code for '0')
                // Calculate the remainder using the modulus 10 table.
                remainder = mods[(num + remainder) % 10];
            }
    
            // Calculate the final checksum digit.
            var checksum = (10 - remainder) % 10;
            // Compare the calculated checksum to the last digit of the input.
            return digits.Length >= 1 && checksum == digits[^1] - '0';
        }

        private static bool IsHexStyle(string inp)
        {
            // Use a single regex to check for a valid hexadecimal string.
            // The pattern checks for an optional "0x" or "0X" prefix followed by one or more hexadecimal digits.
            return Regex.IsMatch(inp, @"\A(0[xX])?[0-9a-fA-F]+\Z");
        }
    }
}
