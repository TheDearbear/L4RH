// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See https://github.com/microsoft/referencesource/blob/master/LICENSE.txt for full license information.

using System.Globalization;
using System.Numerics;

namespace Speed.Math;

/// <summary>
/// A structure encapsulating a 3x3 matrix.
/// </summary>
public struct Matrix3x3 : IEquatable<Matrix3x3>
{
    #region Public Fields
    /// <summary>
    /// Value at row 1, column 1 of the matrix.
    /// </summary>
    public float M11;
    /// <summary>
    /// Value at row 1, column 2 of the matrix.
    /// </summary>
    public float M12;
    /// <summary>
    /// Value at row 1, column 3 of the matrix.
    /// </summary>
    public float M13;

    /// <summary>
    /// Value at row 2, column 1 of the matrix.
    /// </summary>
    public float M21;
    /// <summary>
    /// Value at row 2, column 2 of the matrix.
    /// </summary>
    public float M22;
    /// <summary>
    /// Value at row 2, column 3 of the matrix.
    /// </summary>
    public float M23;

    /// <summary>
    /// Value at row 3, column 1 of the matrix.
    /// </summary>
    public float M31;
    /// <summary>
    /// Value at row 3, column 2 of the matrix.
    /// </summary>
    public float M32;
    /// <summary>
    /// Value at row 3, column 3 of the matrix.
    /// </summary>
    public float M33;
    #endregion Public Fields

    private static readonly Matrix3x3 _identity = new
    (
        1f, 0f, 0f,
        0f, 1f, 0f,
        0f, 0f, 1f
    );

    /// <summary>
    /// Returns the multiplicative identity matrix.
    /// </summary>
    public static Matrix3x3 Identity
    {
        get { return _identity; }
    }

    /// <summary>
    /// Returns whether the matrix is the identity matrix.
    /// </summary>
    public bool IsIdentity
    {
        get
        {
            return M11 == 1 && M22 == 1 && M33 == 1 && // Check diagonal element first for early out.
                    M12 == 0f && M13 == 0f &&
                    M21 == 0f && M23 == 0f &&
                    M31 == 0f && M32 == 0f;
        }
    }

    /// <summary>
    /// Constructs a Matrix3x3 from the given components.
    /// </summary>
    public Matrix3x3(float m11, float m12, float m13,
                        float m21, float m22, float m23,
                        float m31, float m32, float m33)
    {
        M11 = m11;
        M12 = m12;
        M13 = m13;

        M21 = m21;
        M22 = m22;
        M23 = m23;

        M31 = m31;
        M32 = m32;
        M33 = m33;
    }

    /// <summary>
    /// Creates a scaling matrix.
    /// </summary>
    /// <param name="xScale">Value to scale by on the X-axis.</param>
    /// <param name="yScale">Value to scale by on the Y-axis.</param>
    /// <param name="zScale">Value to scale by on the Z-axis.</param>
    /// <returns>The scaling matrix.</returns>
    public static Matrix3x3 CreateScale(float xScale, float yScale, float zScale)
    {
        Matrix3x3 result;

        result.M11 = xScale;
        result.M12 = 0.0f;
        result.M13 = 0.0f;
        result.M21 = 0.0f;
        result.M22 = yScale;
        result.M23 = 0.0f;
        result.M31 = 0.0f;
        result.M32 = 0.0f;
        result.M33 = zScale;

        return result;
    }

    /// <summary>
    /// Creates a scaling matrix.
    /// </summary>
    /// <param name="scales">The vector containing the amount to scale by on each axis.</param>
    /// <returns>The scaling matrix.</returns>
    public static Matrix3x3 CreateScale(Vector3 scales)
        => CreateScale(scales.X, scales.Y, scales.Z);

    /// <summary>
    /// Creates a uniform scaling matrix that scales equally on each axis.
    /// </summary>
    /// <param name="scale">The uniform scaling factor.</param>
    /// <returns>The scaling matrix.</returns>
    public static Matrix3x3 CreateScale(float scale)
        => CreateScale(scale, scale, scale);

    /// <summary>
    /// Creates a matrix for rotating points around the X-axis.
    /// </summary>
    /// <param name="radians">The amount, in radians, by which to rotate around the X-axis.</param>
    /// <returns>The rotation matrix.</returns>
    public static Matrix3x3 CreateRotationX(float radians)
    {
        Matrix3x3 result;

        float c = MathF.Cos(radians);
        float s = MathF.Sin(radians);

        // [  1  0  0 ]
        // [  0  c  s ]
        // [  0 -s  c ]
        result.M11 = 1.0f;
        result.M12 = 0.0f;
        result.M13 = 0.0f;
        result.M21 = 0.0f;
        result.M22 = c;
        result.M23 = s;
        result.M31 = 0.0f;
        result.M32 = -s;
        result.M33 = c;

        return result;
    }

    /// <summary>
    /// Creates a matrix for rotating points around the Y-axis.
    /// </summary>
    /// <param name="radians">The amount, in radians, by which to rotate around the Y-axis.</param>
    /// <returns>The rotation matrix.</returns>
    public static Matrix3x3 CreateRotationY(float radians)
    {
        Matrix3x3 result;

        float c = MathF.Cos(radians);
        float s = MathF.Sin(radians);

        // [  c  0 -s ]
        // [  0  1  0 ]
        // [  s  0  c ]
        result.M11 = c;
        result.M12 = 0.0f;
        result.M13 = -s;
        result.M21 = 0.0f;
        result.M22 = 1.0f;
        result.M23 = 0.0f;
        result.M31 = s;
        result.M32 = 0.0f;
        result.M33 = c;

        return result;
    }

    /// <summary>
    /// Creates a matrix for rotating points around the Z-axis.
    /// </summary>
    /// <param name="radians">The amount, in radians, by which to rotate around the Z-axis.</param>
    /// <returns>The rotation matrix.</returns>
    public static Matrix3x3 CreateRotationZ(float radians)
    {
        Matrix3x3 result;

        float c = MathF.Cos(radians);
        float s = MathF.Sin(radians);

        // [  c  s  0 ]
        // [ -s  c  0 ]
        // [  0  0  1 ]
        result.M11 = c;
        result.M12 = s;
        result.M13 = 0.0f;
        result.M21 = -s;
        result.M22 = c;
        result.M23 = 0.0f;
        result.M31 = 0.0f;
        result.M32 = 0.0f;
        result.M33 = 1.0f;

        return result;
    }

    /// <summary>
    /// Creates a matrix that rotates around an arbitrary vector.
    /// </summary>
    /// <param name="axis">The axis to rotate around.</param>
    /// <param name="angle">The angle to rotate around the given axis, in radians.</param>
    /// <returns>The rotation matrix.</returns>
    public static Matrix3x3 CreateFromAxisAngle(Vector3 axis, float angle)
    {
        // a: angle
        // x, y, z: unit vector for axis.
        //
        // Rotation matrix M can compute by using below equation.
        //
        //        T               T
        //  M = uu + (cos a)( I-uu ) + (sin a)S
        //
        // Where:
        //
        //  u = ( x, y, z )
        //
        //      [  0 -z  y ]
        //  S = [  z  0 -x ]
        //      [ -y  x  0 ]
        //
        //      [ 1 0 0 ]
        //  I = [ 0 1 0 ]
        //      [ 0 0 1 ]
        //
        //
        //     [  xx+cosa*(1-xx)   yx-cosa*yx-sina*z zx-cosa*xz+sina*y ]
        // M = [ xy-cosa*yx+sina*z    yy+cosa(1-yy)  yz-cosa*yz-sina*x ]
        //     [ zx-cosa*zx-sina*y zy-cosa*zy+sina*x   zz+cosa*(1-zz)  ]
        //
        float x = axis.X, y = axis.Y, z = axis.Z;
        float sa = MathF.Sin(angle), ca = MathF.Cos(angle);
        float xx = x * x, yy = y * y, zz = z * z;
        float xy = x * y, xz = x * z, yz = y * z;

        Matrix3x3 result;

        result.M11 = xx + ca * (1.0f - xx);
        result.M12 = xy - ca * xy + sa * z;
        result.M13 = xz - ca * xz - sa * y;

        result.M21 = xy - ca * xy - sa * z;
        result.M22 = yy + ca * (1.0f - yy);
        result.M23 = yz - ca * yz + sa * x;

        result.M31 = xz - ca * xz + sa * y;
        result.M32 = yz - ca * yz - sa * x;
        result.M33 = zz + ca * (1.0f - zz);

        return result;
    }

    /// <summary>
    /// Creates a rotation matrix from the given Quaternion rotation value.
    /// </summary>
    /// <param name="quaternion">The source Quaternion.</param>
    /// <returns>The rotation matrix.</returns>
    public static Matrix3x3 CreateFromQuaternion(Quaternion quaternion)
    {
        Matrix3x3 result;

        float xx = quaternion.X * quaternion.X;
        float yy = quaternion.Y * quaternion.Y;
        float zz = quaternion.Z * quaternion.Z;

        float xy = quaternion.X * quaternion.Y;
        float wz = quaternion.Z * quaternion.W;
        float xz = quaternion.Z * quaternion.X;
        float wy = quaternion.Y * quaternion.W;
        float yz = quaternion.Y * quaternion.Z;
        float wx = quaternion.X * quaternion.W;

        result.M11 = 1.0f - 2.0f * (yy + zz);
        result.M12 = 2.0f * (xy + wz);
        result.M13 = 2.0f * (xz - wy);
        result.M21 = 2.0f * (xy - wz);
        result.M22 = 1.0f - 2.0f * (zz + xx);
        result.M23 = 2.0f * (yz + wx);
        result.M31 = 2.0f * (xz + wy);
        result.M32 = 2.0f * (yz - wx);
        result.M33 = 1.0f - 2.0f * (yy + xx);

        return result;
    }

    /// <summary>
    /// Creates a rotation matrix from the specified yaw, pitch, and roll.
    /// </summary>
    /// <param name="yaw">Angle of rotation, in radians, around the Y-axis.</param>
    /// <param name="pitch">Angle of rotation, in radians, around the X-axis.</param>
    /// <param name="roll">Angle of rotation, in radians, around the Z-axis.</param>
    /// <returns>The rotation matrix.</returns>
    public static Matrix3x3 CreateFromYawPitchRoll(float yaw, float pitch, float roll)
    {
        Quaternion q = Quaternion.CreateFromYawPitchRoll(yaw, pitch, roll);

        return CreateFromQuaternion(q);
    }

    /// <summary>
    /// Calculates the determinant of the matrix.
    /// </summary>
    /// <returns>The determinant of the matrix.</returns>
    public float GetDeterminant()
    {
        // | a b c |     | e f |     | d f |     | d e |
        // | d e f | = a | h i | - b | g i | + c | g h |
        // | g h i |
        //
        //   | e f |
        // a | h i | = a ( ei - fh )
        //
        //   | d f |     
        // b | g i | = b ( di - fg )
        //
        //   | d e |
        // c | g h | = c ( dh - eg )
        //
        // Cost of operation
        // 5 adds and 9 muls.
        //
        // add: 3 + 2 = 5
        // mul: 3 + 3 + 3 = 9

        float a = M11, b = M12, c = M13;
        float d = M21, e = M22, f = M23;
        float g = M31, h = M32, i = M33;

        return a * (e * i - f * h) -
                b * (d * i - f * g) +
                c * (d * h - e * g);
    }

    /// <summary>
    /// Calculates the inverse of the given matrix. Result will contain the inverted matrix.
    /// </summary>
    /// <param name="matrix">The source matrix to invert.</param>
    /// <returns>The inverted matrix.</returns>
    public static Matrix3x3 Invert(Matrix3x3 matrix)
    {
        Matrix3x3 result;

        result.M11 = matrix.M22 * matrix.M33 - matrix.M23 * matrix.M32;
        result.M12 = matrix.M13 * matrix.M32 - matrix.M12 * matrix.M33;
        result.M13 = matrix.M12 * matrix.M23 - matrix.M13 * matrix.M22;
        result.M21 = matrix.M23 * matrix.M31 - matrix.M21 * matrix.M33;
        result.M22 = matrix.M11 * matrix.M33 - matrix.M13 * matrix.M31;
        result.M23 = matrix.M13 * matrix.M21 - matrix.M11 * matrix.M23;
        result.M31 = matrix.M21 * matrix.M32 - matrix.M22 * matrix.M31;
        result.M32 = matrix.M12 * matrix.M31 - matrix.M11 * matrix.M32;
        result.M33 = matrix.M11 * matrix.M22 - matrix.M12 * matrix.M21;

        return result * (1f / matrix.GetDeterminant());
    }

    /// <summary>
    /// Attempts to extract the scale, translation, and rotation components from the given scale/rotation/translation matrix.
    /// If successful, the out parameters will contained the extracted values.
    /// </summary>
    /// <param name="matrix">The source matrix.</param>
    /// <param name="scale">The scaling component of the transformation matrix.</param>
    /// <param name="rotation">The rotation component of the transformation matrix.</param>
    /// <param name="translation">The translation component of the transformation matrix (Always <see cref="Vector3.Zero"/>)</param>
    /// <returns>True if the source matrix was successfully decomposed; False otherwise.</returns>
    public static bool Decompose(Matrix3x3 matrix, out Vector3 scale, out Quaternion rotation, out Vector3 translation)
        => Matrix4x4.Decompose(matrix, out scale, out rotation, out translation);

    /// <summary>
    /// Transposes the rows and columns of a matrix.
    /// </summary>
    /// <param name="matrix">The source matrix.</param>
    /// <returns>The transposed matrix.</returns>
    public static Matrix3x3 Transpose(Matrix3x3 matrix)
    {
        Matrix3x3 result;

        result.M11 = matrix.M11;
        result.M12 = matrix.M21;
        result.M13 = matrix.M31;
        result.M21 = matrix.M12;
        result.M22 = matrix.M22;
        result.M23 = matrix.M32;
        result.M31 = matrix.M13;
        result.M32 = matrix.M23;
        result.M33 = matrix.M33;

        return result;
    }

    /// <summary>
    /// Linearly interpolates between the corresponding values of two matrices.
    /// </summary>
    /// <param name="matrix1">The first source matrix.</param>
    /// <param name="matrix2">The second source matrix.</param>
    /// <param name="amount">The relative weight of the second source matrix.</param>
    /// <returns>The interpolated matrix.</returns>
    public static Matrix3x3 Lerp(Matrix3x3 matrix1, Matrix3x3 matrix2, float amount)
    {
        Matrix3x3 result;

        // First row
        result.M11 = matrix1.M11 + (matrix2.M11 - matrix1.M11) * amount;
        result.M12 = matrix1.M12 + (matrix2.M12 - matrix1.M12) * amount;
        result.M13 = matrix1.M13 + (matrix2.M13 - matrix1.M13) * amount;

        // Second row
        result.M21 = matrix1.M21 + (matrix2.M21 - matrix1.M21) * amount;
        result.M22 = matrix1.M22 + (matrix2.M22 - matrix1.M22) * amount;
        result.M23 = matrix1.M23 + (matrix2.M23 - matrix1.M23) * amount;

        // Third row
        result.M31 = matrix1.M31 + (matrix2.M31 - matrix1.M31) * amount;
        result.M32 = matrix1.M32 + (matrix2.M32 - matrix1.M32) * amount;
        result.M33 = matrix1.M33 + (matrix2.M33 - matrix1.M33) * amount;

        return result;
    }

    /// <summary>
    /// Returns a new matrix with the negated elements of the given matrix.
    /// </summary>
    /// <param name="value">The source matrix.</param>
    /// <returns>The negated matrix.</returns>
    public static Matrix3x3 Negate(Matrix3x3 value)
    {
        Matrix3x3 result;

        result.M11 = -value.M11;
        result.M12 = -value.M12;
        result.M13 = -value.M13;
        result.M21 = -value.M21;
        result.M22 = -value.M22;
        result.M23 = -value.M23;
        result.M31 = -value.M31;
        result.M32 = -value.M32;
        result.M33 = -value.M33;

        return result;
    }

    /// <summary>
    /// Adds two matrices together.
    /// </summary>
    /// <param name="value1">The first source matrix.</param>
    /// <param name="value2">The second source matrix.</param>
    /// <returns>The resulting matrix.</returns>
    public static Matrix3x3 Add(Matrix3x3 value1, Matrix3x3 value2)
    {
        Matrix3x3 result;

        result.M11 = value1.M11 + value2.M11;
        result.M12 = value1.M12 + value2.M12;
        result.M13 = value1.M13 + value2.M13;
        result.M21 = value1.M21 + value2.M21;
        result.M22 = value1.M22 + value2.M22;
        result.M23 = value1.M23 + value2.M23;
        result.M31 = value1.M31 + value2.M31;
        result.M32 = value1.M32 + value2.M32;
        result.M33 = value1.M33 + value2.M33;

        return result;
    }

    /// <summary>
    /// Subtracts the second matrix from the first.
    /// </summary>
    /// <param name="value1">The first source matrix.</param>
    /// <param name="value2">The second source matrix.</param>
    /// <returns>The result of the subtraction.</returns>
    public static Matrix3x3 Subtract(Matrix3x3 value1, Matrix3x3 value2)
    {
        Matrix3x3 result;

        result.M11 = value1.M11 - value2.M11;
        result.M12 = value1.M12 - value2.M12;
        result.M13 = value1.M13 - value2.M13;
        result.M21 = value1.M21 - value2.M21;
        result.M22 = value1.M22 - value2.M22;
        result.M23 = value1.M23 - value2.M23;
        result.M31 = value1.M31 - value2.M31;
        result.M32 = value1.M32 - value2.M32;
        result.M33 = value1.M33 - value2.M33;

        return result;
    }

    /// <summary>
    /// Multiplies a matrix by another matrix.
    /// </summary>
    /// <param name="value1">The first source matrix.</param>
    /// <param name="value2">The second source matrix.</param>
    /// <returns>The result of the multiplication.</returns>
    public static Matrix3x3 Multiply(Matrix3x3 value1, Matrix3x3 value2)
    {
        Matrix3x3 result;

        // First row
        result.M11 = value1.M11 * value2.M11 + value1.M12 * value2.M21 + value1.M13 * value2.M31;
        result.M12 = value1.M11 * value2.M12 + value1.M12 * value2.M22 + value1.M13 * value2.M32;
        result.M13 = value1.M11 * value2.M13 + value1.M12 * value2.M23 + value1.M13 * value2.M33;

        // Second row
        result.M21 = value1.M21 * value2.M11 + value1.M22 * value2.M21 + value1.M23 * value2.M31;
        result.M22 = value1.M21 * value2.M12 + value1.M22 * value2.M22 + value1.M23 * value2.M32;
        result.M23 = value1.M21 * value2.M13 + value1.M22 * value2.M23 + value1.M23 * value2.M33;

        // Third row
        result.M31 = value1.M31 * value2.M11 + value1.M32 * value2.M21 + value1.M33 * value2.M31;
        result.M32 = value1.M31 * value2.M12 + value1.M32 * value2.M22 + value1.M33 * value2.M32;
        result.M33 = value1.M31 * value2.M13 + value1.M32 * value2.M23 + value1.M33 * value2.M33;

        return result;
    }

    /// <summary>
    /// Multiplies a matrix by a scalar value.
    /// </summary>
    /// <param name="value1">The source matrix.</param>
    /// <param name="value2">The scaling factor.</param>
    /// <returns>The scaled matrix.</returns>
    public static Matrix3x3 Multiply(Matrix3x3 value1, float value2)
    {
        Matrix3x3 result;

        result.M11 = value1.M11 * value2;
        result.M12 = value1.M12 * value2;
        result.M13 = value1.M13 * value2;
        result.M21 = value1.M21 * value2;
        result.M22 = value1.M22 * value2;
        result.M23 = value1.M23 * value2;
        result.M31 = value1.M31 * value2;
        result.M32 = value1.M32 * value2;
        result.M33 = value1.M33 * value2;

        return result;
    }

    /// <summary>
    /// Multiplies a matrix by a vector.
    /// </summary>
    /// <param name="value1">The source matrix.</param>
    /// <param name="value2">The scaling vector.</param>
    /// <returns>The scaled vector.</returns>
    public static Vector3 Multiply(Matrix3x3 value1, Vector3 value2)
    {
        Vector3 result;

        result.X = value1.M11 * value2.X + value1.M12 * value2.X + value1.M13 * value2.X;
        result.Y = value1.M21 * value2.Y + value1.M22 * value2.Y + value1.M23 * value2.Y;
        result.Z = value1.M31 * value2.Z + value1.M32 * value2.Z + value1.M33 * value2.Z;

        return result;
    }

    /// <summary>
    /// Returns a new matrix with the negated elements of the given matrix.
    /// </summary>
    /// <param name="value">The source matrix.</param>
    /// <returns>The negated matrix.</returns>
    public static Matrix3x3 operator -(Matrix3x3 value)
    {
        Matrix3x3 m;

        m.M11 = -value.M11;
        m.M12 = -value.M12;
        m.M13 = -value.M13;
        m.M21 = -value.M21;
        m.M22 = -value.M22;
        m.M23 = -value.M23;
        m.M31 = -value.M31;
        m.M32 = -value.M32;
        m.M33 = -value.M33;

        return m;
    }

    /// <summary>
    /// Adds two matrices together.
    /// </summary>
    /// <param name="value1">The first source matrix.</param>
    /// <param name="value2">The second source matrix.</param>
    /// <returns>The resulting matrix.</returns>
    public static Matrix3x3 operator +(Matrix3x3 value1, Matrix3x3 value2)
        => Add(value1, value2);

    /// <summary>
    /// Subtracts the second matrix from the first.
    /// </summary>
    /// <param name="value1">The first source matrix.</param>
    /// <param name="value2">The second source matrix.</param>
    /// <returns>The result of the subtraction.</returns>
    public static Matrix3x3 operator -(Matrix3x3 value1, Matrix3x3 value2)
        => Subtract(value1, value2);

    /// <summary>
    /// Multiplies a matrix by another matrix.
    /// </summary>
    /// <param name="value1">The first source matrix.</param>
    /// <param name="value2">The second source matrix.</param>
    /// <returns>The result of the multiplication.</returns>
    public static Matrix3x3 operator *(Matrix3x3 value1, Matrix3x3 value2)
        => Multiply(value1, value2);

    /// <summary>
    /// Multiplies a matrix by a scalar value.
    /// </summary>
    /// <param name="value1">The source matrix.</param>
    /// <param name="value2">The scaling factor.</param>
    /// <returns>The scaled matrix.</returns>
    public static Matrix3x3 operator *(Matrix3x3 value1, float value2)
        => Multiply(value1, value2);

    /// <summary>
    /// Returns a boolean indicating whether the given two matrices are equal.
    /// </summary>
    /// <param name="value1">The first matrix to compare.</param>
    /// <param name="value2">The second matrix to compare.</param>
    /// <returns>True if the given matrices are equal; False otherwise.</returns>
    public static bool operator ==(Matrix3x3 value1, Matrix3x3 value2)
    {
        return value1.M11 == value2.M11 && value1.M22 == value2.M22 && value1.M33 == value2.M33 && // Check diagonal element first for early out.
                                            value1.M12 == value2.M12 && value1.M13 == value2.M13 &&
                                            value1.M21 == value2.M21 && value1.M23 == value2.M23 &&
                                            value1.M31 == value2.M31 && value1.M32 == value2.M32;
    }

    /// <summary>
    /// Returns a boolean indicating whether the given two matrices are not equal.
    /// </summary>
    /// <param name="value1">The first matrix to compare.</param>
    /// <param name="value2">The second matrix to compare.</param>
    /// <returns>True if the given matrices are not equal; False if they are equal.</returns>
    public static bool operator !=(Matrix3x3 value1, Matrix3x3 value2)
    {
        return value1.M11 != value2.M11 || value1.M12 != value2.M12 || value1.M13 != value2.M13 ||
                value1.M21 != value2.M21 || value1.M22 != value2.M22 || value1.M23 != value2.M23 ||
                value1.M31 != value2.M31 || value1.M32 != value2.M32 || value1.M33 != value2.M33;
    }

    public static implicit operator Matrix3x3(Matrix4x4 value)
    {
        Matrix3x3 result;

        result.M11 = value.M11;
        result.M12 = value.M12;
        result.M13 = value.M13;
        result.M21 = value.M21;
        result.M22 = value.M22;
        result.M23 = value.M23;
        result.M31 = value.M31;
        result.M32 = value.M32;
        result.M33 = value.M33;

        return result;
    }

    public static implicit operator Matrix4x4(Matrix3x3 value)
    {
        Matrix4x4 result;

        result.M11 = value.M11;
        result.M12 = value.M12;
        result.M13 = value.M13;
        result.M14 = 0.0f;
        result.M21 = value.M21;
        result.M22 = value.M22;
        result.M23 = value.M23;
        result.M24 = 0.0f;
        result.M31 = value.M31;
        result.M32 = value.M32;
        result.M33 = value.M33;
        result.M34 = 0.0f;
        result.M41 = 0.0f;
        result.M42 = 0.0f;
        result.M43 = 0.0f;
        result.M44 = 1.0f;

        return result;
    }

    /// <summary>
    /// Returns a boolean indicating whether this matrix instance is equal to the other given matrix.
    /// </summary>
    /// <param name="other">The matrix to compare this instance to.</param>
    /// <returns>True if the matrices are equal; False otherwise.</returns>
    public bool Equals(Matrix3x3 other)
    {
        return this == other;
    }

    /// <summary>
    /// Returns a boolean indicating whether the given Object is equal to this matrix instance.
    /// </summary>
    /// <param name="obj">The Object to compare against.</param>
    /// <returns>True if the Object is equal to this matrix; False otherwise.</returns>
    public override bool Equals(object? obj)
        => obj is not null && obj is Matrix3x3 mat && Equals(mat);

    /// <summary>
    /// Returns a String representing this matrix instance.
    /// </summary>
    /// <returns>The string representation.</returns>
    public override string ToString()
    {
        CultureInfo ci = CultureInfo.CurrentCulture;

        return string.Format(ci, "{{ {{M11:{0} M12:{1} M13:{2}}} {{M21:{3} M22:{4} M23:{5}}} {{M31:{6} M32:{7} M33:{8}}} }}",
                                M11.ToString(ci), M12.ToString(ci), M13.ToString(ci),
                                M21.ToString(ci), M22.ToString(ci), M23.ToString(ci),
                                M31.ToString(ci), M32.ToString(ci), M33.ToString(ci));
    }

    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    /// <returns>The hash code.</returns>
    public override int GetHashCode()
    {
        return M11.GetHashCode() + M12.GetHashCode() + M13.GetHashCode() +
                M21.GetHashCode() + M22.GetHashCode() + M23.GetHashCode() +
                M31.GetHashCode() + M32.GetHashCode() + M33.GetHashCode();
    }
}
