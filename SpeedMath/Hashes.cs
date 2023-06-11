namespace Speed.Math;

public static class Hashes
{
    public static uint Bin(string source)
    {
        uint result = uint.MaxValue;

        foreach (char c in source)
            result = c + 33 * result;

        return result;
    }
}
