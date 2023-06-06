using L4RH.Model;
using System.Numerics;

namespace L4RH;

public static class VectorsExtensions
{
    public static Vector3 SwapYZ(this Vector3 v)
        => new(v.X, v.Z, v.Y);

    public static Quaternion SwapYZ(this Quaternion q)
        => new(q.X, q.Z, q.Y, q.W);

    public static Vertice SwapYZ(this Vertice v)
        => new(v.X, v.Z, v.Y)
        {
            ColorA = v.ColorA,
            ColorR = v.ColorR,
            ColorG = v.ColorG,
            ColorB = v.ColorB,
            TextureX = v.TextureX,
            TextureY = v.TextureY
        };

    public static float[] ToArray(this Matrix4x4 m)
        => new float[] { m.M11, m.M12, m.M13, m.M14, m.M21, m.M22, m.M23, m.M24, m.M31, m.M32, m.M33, m.M34, m.M41, m.M42, m.M43, m.M44 };
}
