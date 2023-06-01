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
        => new(v.X, v.Z, v.Y);

    public static float[] ToArray(this Matrix4x4 m)
        => new float[] { m.M11, m.M12, m.M13, m.M14, m.M21, m.M22, m.M23, m.M24, m.M31, m.M32, m.M33, m.M34, m.M41, m.M42, m.M43, m.M44 };

    public static Vector4 GetColumn(this Matrix4x4 m, int column)
    {
        if (column < 1 && column > 4)
            throw new ArgumentOutOfRangeException(nameof(column));

        return column switch
        {
            1 => new(m.M11, m.M21, m.M31, m.M41),
            2 => new(m.M12, m.M22, m.M32, m.M42),
            3 => new(m.M13, m.M23, m.M33, m.M43),
            _ => new(m.M14, m.M24, m.M34, m.M44) // 4
        };
    }

    public static Vector4 GetRow(this Matrix4x4 m, int row)
    {
        if (row < 1 && row > 4)
            throw new ArgumentOutOfRangeException(nameof(row));

        return row switch
        {
            1 => new(m.M11, m.M12, m.M13, m.M14),
            2 => new(m.M21, m.M22, m.M23, m.M24),
            3 => new(m.M31, m.M32, m.M33, m.M34),
            _ => new(m.M41, m.M42, m.M43, m.M44) // 4
        };
    }

    public static void SetColumn(this Matrix4x4 m, int column, Vector4 value)
    {
        if (column < 1 && column > 4)
            throw new ArgumentOutOfRangeException(nameof(column));

        switch (column)
        {
            case 1:
                m.M11 = value.X;
                m.M21 = value.Y;
                m.M31 = value.Z;
                m.M41 = value.W;
                break;

            case 2:
                m.M12 = value.X;
                m.M22 = value.Y;
                m.M32 = value.Z;
                m.M42 = value.W;
                break;

            case 3:
                m.M13 = value.X;
                m.M23 = value.Y;
                m.M33 = value.Z;
                m.M43 = value.W;
                break;

            default: // 4
                m.M14 = value.X;
                m.M24 = value.Y;
                m.M34 = value.Z;
                m.M44 = value.W;
                break;
        }
    }

    public static void SetRow(this Matrix4x4 m, int row, Vector4 value)
    {
        if (row < 1 && row > 4)
            throw new ArgumentOutOfRangeException(nameof(row));

        switch (row)
        {
            case 1:
                m.M11 = value.X;
                m.M12 = value.Y;
                m.M13 = value.Z;
                m.M14 = value.W;
                break;

            case 2:
                m.M21 = value.X;
                m.M22 = value.Y;
                m.M23 = value.Z;
                m.M24 = value.W;
                break;

            case 3:
                m.M31 = value.X;
                m.M32 = value.Y;
                m.M33 = value.Z;
                m.M34 = value.W;
                break;

            default: // 4
                m.M41 = value.X;
                m.M42 = value.Y;
                m.M43 = value.Z;
                m.M44 = value.W;
                break;
        }
    }
}
