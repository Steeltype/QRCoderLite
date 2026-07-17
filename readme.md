# QRCoderLite

## Info

QRCoderLite is a lightweight, **zero-dependency** QR code generator for .NET 8, written in pure C#. It is a stripped-down derivative of the popular [QRCoder](https://github.com/codebude/QRCoder) library (now maintained at [Shane32/QRCoder](https://github.com/Shane32/QRCoder)), aiming for a small, easy-to-audit footprint with no external packages — no System.Drawing, no SkiaSharp, no native dependencies.

The library intentionally tracks a reduced feature set rather than following upstream. Correctness fixes from upstream are cherry-picked when they apply to the retained code.

## Legal information and credits

QRCoderLite is a project by Steeltype LLC under the [BSD-3-Clause](https://opensource.org/license/bsd-3-clause/) license. The original QRCoder library from which it is derived is under the MIT license courtesy of Raffael Herrmann.

* * *

## Usage

Four lines to generate a QR code as a PNG byte array:

```csharp
using var qrGenerator = new QRCodeGenerator();
using var qrCodeData = qrGenerator.CreateQrCode("The text which should be encoded.", QRCodeGenerator.ECCLevel.Q);
using var qrCode = new PngByteQRCode(qrCodeData);
byte[] qrCodePng = qrCode.GetGraphic(20);
```

Every renderer also has a static helper for one-line generation:

```csharp
byte[] qrCodePng = PngByteQRCodeHelper.GetQRCode("The text which should be encoded.", 20, QRCodeGenerator.ECCLevel.Q);
```

### Renderers

All renderers consume the `QRCodeData` produced by `QRCodeGenerator.CreateQrCode(...)`:

| Renderer | Output | Notes |
| --- | --- | --- |
| `PngByteQRCode` | `byte[]` (PNG) | Optional RGB(A) colors via byte arrays |
| `SvgQRCode` | `string` (SVG) | Hex colors, sizing modes, optional embedded center logo (`byte[]` + MIME type) |
| `AsciiQRCode` | `string` / `string[]` | Terminal output with configurable module strings |
| `Base64QRCode` | `string` (base64 PNG) | Wraps `PngByteQRCode` |
| `BitmapByteQRCode` | `byte[]` (BMP) | Dependency-free bitmap writer |
| `PdfByteQRCode` | `byte[]` (PDF) | Vector output, configurable DPI |

Color parameters use HTML hex notation (`"#000000"`, shorthand `"#000"`, or 8-digit RGBA) or raw byte arrays depending on the renderer — see each `GetGraphic` overload.

```csharp
// SVG with colors and an embedded logo
using var svgQrCode = new SvgQRCode(qrCodeData);
string svg = svgQrCode.GetGraphic(10, "#000000", "#ffffff", logoBytes: myPngBytes, logoSizePercent: 15, logoMimeType: "image/png");

// ASCII for terminals
using var asciiQrCode = new AsciiQRCode(qrCodeData);
string ascii = asciiQrCode.GetGraphic(1);
```

### Input limits

Renderer inputs are validated and throw `ArgumentOutOfRangeException` outside these bounds:

- `pixelsPerModule`: 1–100 (PNG, BMP, ASCII `repeatPerModule`) or 1–1000 (SVG, PDF)
- PDF `dpi`: 1–2400
- SVG logo: max 5 MB, `logoSizePercent` 1–50
- `QRCodeData` raw-data deserialization is hardened: 10 MB decompression cap, signature and size validation

### Encoding notes

- Text that fits ISO-8859-1 is encoded as ISO-8859-1 by default; anything else uses UTF-8. Pass `forceUtf8: true` or an explicit `eciMode` to control this.
- `EciMode.Iso8859_2` requires the `System.Text.Encoding.CodePages` package and a `CodePagesEncodingProvider.Instance` registration on modern .NET; without it, `Encoding.GetEncoding("ISO-8859-2")` throws. The library itself does not carry the dependency.

## PayloadGenerator — QR code payloads

`PayloadGenerator` builds the structured payload strings that trigger special scanner behavior (join WiFi, add contact, open URL, ...):

```csharp
var wifiPayload = new PayloadGenerator.WiFi("MyWiFi-SSID", "MyWiFi-Pass", PayloadGenerator.WiFi.Authentication.WPA);
using var qrCodeData = qrGenerator.CreateQrCode(wifiPayload); // payload defaults: version auto, ECC M, ECI auto
// or: qrGenerator.CreateQrCode(wifiPayload.ToString(), QRCodeGenerator.ECCLevel.Q);
// or override the payload's ECC level: qrGenerator.CreateQrCode(wifiPayload, QRCodeGenerator.ECCLevel.Q);
```

Supported payload types:

* Bitcoin-like cryptocurrency payment address (`BitcoinAddress`, `BitcoinCashAddress`, `LitecoinAddress`)
* `Bookmark`
* `CalendarEvent` (iCal/vEvent; UTC times emit the `Z` suffix)
* `ContactData` (MeCard and vCard 2.1/3.0/4.0)
* `Geolocation` (GEO or Google Maps link; validates coordinate ranges)
* `Mail` (mailto/MATMSG/SMTP)
* `OneTimePassword` (TOTP/HOTP for authenticator apps; SHA1/SHA256/SHA512)
* `PhoneNumber`
* `SMS` (SMS/SMSTO/iOS variants)
* `Url`
* `WhatsAppMessage`
* `WiFi` (WEP/WPA/WPA2/nopass)
