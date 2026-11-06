using System.Numerics;
using Havok.Reflection;

namespace Havok.Formats;

/// <summary>
/// Field access on one object of a class set, without System.Reflection (class sets generate these accessors).
/// </summary>
public interface IHavokFieldAccess
{
    /// <summary>
    /// The object the fields belong to.
    /// </summary>
    object Object { get; }

    bool TryGetField(string fieldName, out object? value);

    bool TrySetField(string fieldName, object? value);
}

/// <summary>
/// One SDK's set of C# classes, as the format engines see it. The engines work on <see cref="HavokType" /> and plain
/// objects, so a class set (today HKLib's hk2018/hk2019 classes, later the generated Havok.hkXXXX classes) only has to
/// supply its registry, object creation and field access.
/// </summary>
public interface IHavokClassSet
{
    /// <summary>
    /// A short name for messages, e.g. "HKLib hk2018".
    /// </summary>
    string Name { get; }

    /// <summary>
    /// The registry the files are read and written against, with the class set's C# types bound to it.
    /// </summary>
    HavokTypeRegistry Registry { get; }

    /// <summary>
    /// Creates an object of a record type and returns its field access.
    /// </summary>
    IHavokFieldAccess Instantiate(HavokType type);

    /// <summary>
    /// Field access for an existing object of a record type.
    /// </summary>
    IHavokFieldAccess Access(object havokObject, HavokType type);

    /// <summary>
    /// Whether the value is one of the class set's Havok objects (records), as opposed to a primitive or a list.
    /// </summary>
    bool IsHavokObject(object value);

    /// <summary>
    /// The class set's 3x3 matrix type (stored as 3 rows of 4 floats), or null if it has none.
    /// </summary>
    Type? Matrix3x3Type { get; }

    object ToMatrix3x3(Matrix4x4 matrix);

    Matrix4x4 FromMatrix3x3(object matrix);
}
