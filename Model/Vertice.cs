using System.Diagnostics;
using System.Numerics;

namespace L4RH.Model;

[DebuggerDisplay("{Math.Round(X, 2)} {Math.Round(Y, 2)} {Math.Round(Z, 2)} #{ColorR:X2}{ColorG:X2}{ColorB:X2}{ColorA:X2} {Math.Round(TextureX, 3)} {Math.Round(TextureY, 3)}")]
public struct Vertice
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }

    public float ColorR { get; set; }
    public float ColorG { get; set; }
    public float ColorB { get; set; }
    public float ColorA { get; set; }

    public float TextureX { get; set; }
    public float TextureY { get; set; }

    public Vertice()
    {
        X = default;
        Y = default;
        Z = default;
        
        ColorR = default;
        ColorG = default;
        ColorB = default;
        ColorA = default;
        
        TextureX = default;
        TextureY = default;
    }

    public Vertice(Vector3 vector)
        : this(vector.X, vector.Y, vector.Z) { }
    public Vertice(float x, float y, float z)
        : this()
    {
        X = x;
        Y = y;
        Z = z;
    }

    public static implicit operator Vector3(Vertice vertice)
        => new(vertice.X, vertice.Y, vertice.Z);

    public static implicit operator Vertice(Vector3 vector)
        => new(vector);

    public void Unpack(Vector3 min, Vector3 max)
    {
        X = UnpackFloat(X, min.X, max.X);
        Y = UnpackFloat(Y, min.Y, max.Y, false);
        Z = UnpackFloat(Z, min.Z, max.Z);
    }

    public readonly float[] ToArray() => new[] { X, Y, Z, ColorR, ColorG, ColorB, ColorA, TextureX, TextureY };

    public void SetColor(uint color)
    {
        ColorA = (float)((color >> 24) & 0xFF) / 0xFF;
        ColorR = (float)((color >> 16) & 0xFF) / 0xFF;
        ColorG = (float)((color >> 8) & 0xFF) / 0xFF;
        ColorB = (float) (color & 0xFF) / 0xFF;
    }

    private static float GetPackedFloatSafe(byte[] buf)
    {
        return (float)BitConverter.ToInt16(buf) / 0x8000;
    }

    private static float UnpackFloat(float value, float min, float max, bool reverse = true)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        if (reverse) Array.Reverse(bytes);

        if (value < min || value > max || float.IsNaN(value))
            return GetPackedFloatSafe(bytes);

        if (value >= min && value <= max && !float.IsNaN(value))
            return value;

        var exponent = value == 0 ? 0 : (int)Math.Floor(Math.Log10(Math.Abs(value)));

        return exponent < 0 ? GetPackedFloatSafe(bytes) : value;

    }
}
