using Spectre.Console;

namespace BuildingBlocks.Utils;

public class ColorConverter
{
    public static (byte R, byte G, byte B) HexToRgb(string hexColor)
    {
        // Remove the '#' character if present
        if (hexColor.StartsWith("#"))
        {
            hexColor = hexColor.Substring(1);
        }

        // Ensure the hex code is 6 characters long
        if (hexColor.Length != 6)
        {
            throw new ArgumentException("Invalid hex color code. Must be 6 characters long.");
        }

        // Parse the R, G, B components as bytes
        byte r = Convert.ToByte(hexColor.Substring(0, 2), 16);
        byte g = Convert.ToByte(hexColor.Substring(2, 2), 16);
        byte b = Convert.ToByte(hexColor.Substring(4, 2), 16);

        return (r, g, b);
    }

    public static Color HexToColor(string hexColor)
    {
        var (r, g, b) = HexToRgb(hexColor);

        return new Color(r, g, b);
    }
}
