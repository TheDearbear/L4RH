using System.Numerics;

namespace Speed.Math;

public static class NumericsExtensions
{
    public static Vector3 SwapYZ(this Vector3 v)
        => new(v.X, v.Z, v.Y);

    public static Quaternion SwapYZ(this Quaternion q)
        => new(q.X, q.Z, q.Y, q.W);

    public static float[] ToArray(this Matrix4x4 m)
        => new float[] { m.M11, m.M12, m.M13, m.M14, m.M21, m.M22, m.M23, m.M24, m.M31, m.M32, m.M33, m.M34, m.M41, m.M42, m.M43, m.M44 };

    public static bool Decompose(this Matrix4x4 m, out Vector4 projection, out Vector3 translation, out Vector3 scale, out Vector3 shear, out Quaternion rotation)
    {
        Matrix3x3 transform = m;
        var determinant = transform.GetDeterminant();
        projection = Vector4.UnitW;

        if (m.M13 != 0 || m.M24 != 0 || m.M34 != 0 || m.M44 != 1)
        {
            if (determinant == 0)
            {
                projection = default;
                translation = default;
                shear = default;
                scale = default;
                rotation = default;

                return false;
            }

            var inverted = Matrix3x3.Invert(Matrix3x3.Transpose(transform));

            Vector3 vector = new(
                m.M14 * inverted.M11 + m.M24 * inverted.M21 + m.M34 * inverted.M31,
                m.M14 * inverted.M12 + m.M24 * inverted.M22 + m.M34 * inverted.M32,
                m.M14 * inverted.M13 + m.M24 * inverted.M23 + m.M34 * inverted.M33);

            projection = new(
                vector.X,
                vector.Y,
                vector.Z,
                m.M44 - Vector3.Dot(vector, new Vector3(m.M41, m.M42, m.M43)));
        }

        translation = m.Translation;

        if (transform.IsIdentity)
        {
            scale = Vector3.One;
            shear = Vector3.Zero;
            rotation = Quaternion.Identity;

            return true;
        }

        if (determinant == 0)
        {
            projection = default;
            translation = default;
            shear = default;
            scale = default;
            rotation = default;

            return false;
        }

        Matrix3x3 matrix = Matrix3x3.Transpose(transform) * transform;

        int multiplier = determinant < 0 ? -1 : 1;
        float scaleX = MathF.Sqrt(matrix.M11) * multiplier;
        float num4 = matrix.M12 / scaleX;
        float num5 = matrix.M13 / scaleX;
        float scaleY = MathF.Sqrt(matrix.M22 - num4 * num4) * multiplier;
        float num7 = (matrix.M23 - num4 * num5) / scaleY;
        float scaleZ = MathF.Sqrt(matrix.M33 - num5 * num5 - num7 * num7) * multiplier;

        matrix = new Matrix3x3
        {
            M11 = scaleX,
            M12 = num4,
            M13 = num5,
            M22 = scaleY,
            M23 = num7,
            M33 = scaleZ
        };

        matrix = Matrix3x3.Invert(matrix);

        scale = new(scaleX, scaleY, scaleZ);
        shear = new(num4 / scaleY, num5 / scaleZ, num7 / scaleZ);
        rotation = Quaternion.CreateFromRotationMatrix(transform * matrix);

        return true;
    }
}
