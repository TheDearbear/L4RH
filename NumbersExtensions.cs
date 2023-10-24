using System.Numerics;
using System.Runtime.CompilerServices;

namespace L4RH;

public static class NumbersExtensions
{
    public static uint ToBinHash(this string source)
    {
        uint result = uint.MaxValue;

        foreach (char c in source)
            result = c + 33 * result;

        return result;
    }

    public static uint ToBinHash(this byte[] source)
    {
        uint result = uint.MaxValue;

        foreach (byte b in source)
            result = b + 0x33 * result;

        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float[] ToArray(this Matrix4x4 m)
        => new float[] { m.M11, m.M12, m.M13, m.M14, m.M21, m.M22, m.M23, m.M24, m.M31, m.M32, m.M33, m.M34, m.M41, m.M42, m.M43, m.M44 };
}
