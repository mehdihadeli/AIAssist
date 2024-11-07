namespace BuildingBlocks.Utils;

public static class NumbersUtilities
{
    public static string FormatMetric(this int number)
    {
        if (number >= 1_000_000)
            return (number / 1_000_000.0).ToString("0.#") + "M";
        if (number >= 1_000)
            return (number / 1_000.0).ToString("0.#") + "k";
        return number.ToString();
    }

    public static string FormatCommas(this int number)
    {
        return number.ToString("N0");
    }

    public static string FormatCommas(this decimal number)
    {
        return number.ToString("N0");
    }
}
