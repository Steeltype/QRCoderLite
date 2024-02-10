namespace Steeltype.QRCoderLite
{
    public static class Utilities
    {
        public static byte[] HexColorToByteArray(string colorString)
        {
            // Remove the leading '#' if present.
            if (colorString.StartsWith("#"))
            {
                colorString = colorString.Substring(1);
            }

            // Support for shorthand 3-character hex codes by expanding them to 6 characters.
            if (colorString.Length == 3)
            {
                colorString = $"{colorString[0]}{colorString[0]}{colorString[1]}{colorString[1]}{colorString[2]}{colorString[2]}";
            }
    
            // Validate the length of the color string to ensure it's either 6 (RGB) or 8 (RGBA) characters.
            if (colorString.Length != 6 && colorString.Length != 8)
            {
                throw new ArgumentException("Hex color string must be 6 (RGB) or 8 (RGBA) characters long after removing the leading '#'.");
            }

            var byteColor = new byte[colorString.Length / 2];
            for (var i = 0; i < byteColor.Length; i++)
            {
                // Use TryParse to safely attempt to parse the hex values. If parsing fails, throw a descriptive exception.
                if (!byte.TryParse(colorString.AsSpan(i * 2, 2), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out byteColor[i]))
                {
                    throw new ArgumentException($"Invalid hex digit found in color string: {colorString.Substring(i * 2, 2)}");
                }
            }
    
            return byteColor;
        }

        public static  byte[] IntTo4Byte(int inp)
        {
            var bytes = new byte[4];
            bytes[3] = (byte)(inp >> 24);
            bytes[2] = (byte)(inp >> 16);
            bytes[1] = (byte)(inp >> 8);
            bytes[0] = (byte)(inp);
            return bytes;
        }
    }
}
