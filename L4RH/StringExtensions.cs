namespace L4RH;

public static class StringExtensions
{
    public static string Repeat(this string str, int count)
    {
        if (count <= 0) return string.Empty;
        if (count == 1) return str;

        string result = string.Empty;
        for (int i = 0; i < count; i++)
            result += str;

        return result;
    }

    public static string ConvertToString(this IEnumerable<char> value)
    {
        return string.Concat(value);
    }

    public static uint SpeedHash(this string str)
    {
        uint result = uint.MaxValue;

        foreach (char c in str)
            result = c + 33 * result;

        return result;
    }
}
