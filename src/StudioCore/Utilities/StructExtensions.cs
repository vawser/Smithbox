using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Numerics;
using System.Reflection;

namespace StudioCore.Utilities;

public static class StructExtensions
{
    public static Vector3 XYZ(this Vector4 v)
    {
        return new Vector3(v.X, v.Y, v.Z);
    }

    /// <summary>
    ///     A generic extension method that aids in reflecting
    ///     and retrieving any attribute that is applied to an `Enum`.
    /// </summary>
    public static TAttribute GetAttribute<TAttribute>(this Enum enumValue)
        where TAttribute : Attribute
    {
        return enumValue.GetType()
            .GetMember(enumValue.ToString())
            .First()
            .GetCustomAttribute<TAttribute>();
    }

    public static string GetDisplayName(this Enum enumValue)
    {
        return enumValue.GetAttribute<DisplayAttribute>().Name;
    }
}
