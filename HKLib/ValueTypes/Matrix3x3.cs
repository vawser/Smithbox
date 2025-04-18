namespace HKLib.ValueTypes;

/// <summary>
/// Represents a 3x3 matrix of floats, can be implicitly converted to a <see cref="Matrix4x4" />,
/// can be explicitly converted to from a <see cref="Matrix4x4" />
/// </summary>
public struct Matrix3x3
{
    private Matrix4x4 _impl;

    public float M11
    {
        get => _impl.M11;
        set => _impl.M11 = value;
    }

    public float M12
    {
        get => _impl.M12;
        set => _impl.M12 = value;
    }

    public float M13
    {
        get => _impl.M13;
        set => _impl.M13 = value;
    }

    public float M21
    {
        get => _impl.M21;
        set => _impl.M21 = value;
    }

    public float M22
    {
        get => _impl.M22;
        set => _impl.M22 = value;
    }

    public float M23
    {
        get => _impl.M23;
        set => _impl.M23 = value;
    }

    public float M31
    {
        get => _impl.M31;
        set => _impl.M31 = value;
    }

    public float M32
    {
        get => _impl.M32;
        set => _impl.M32 = value;
    }

    public float M33
    {
        get => _impl.M33;
        set => _impl.M33 = value;
    }

    public Matrix3x3(Matrix4x4 matrix)
    {
        _impl = matrix;
        _impl.Translation = new Vector3(0, 0, 0);
    }

    public static implicit operator Matrix4x4(Matrix3x3 matrix)
    {
        return matrix._impl;
    }

    public static explicit operator Matrix3x3(Matrix4x4 matrix)
    {
        return new Matrix3x3(matrix);
    }
}