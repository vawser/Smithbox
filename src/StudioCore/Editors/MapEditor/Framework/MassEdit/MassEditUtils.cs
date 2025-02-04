using StudioCore.Editors.MapEditor.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.Framework.MassEdit;

public static class MassEditUtils
{
    /// <summary>
    /// Returns true if the passed type is a numeric type suitable for mathematical conditions / operations
    /// </summary>
    public static bool IsNumericType(Type valueType)
    {
        if (valueType == typeof(byte) ||
            valueType == typeof(sbyte) ||
            valueType == typeof(short) ||
            valueType == typeof(ushort) ||
            valueType == typeof(int) ||
            valueType == typeof(uint) ||
            valueType == typeof(long) ||
            valueType == typeof(float) ||
            valueType == typeof(double))
        {
            return true;
        }

        return false;
    }
}
