using Andre.Formats;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public static class MassParamEdit
{
    public static Dictionary<string, object> massEditVars = new();

    internal static object WithDynamicOf(object instance, Func<dynamic, object> dynamicFunc)
    {
        try
        {
            object result = dynamicFunc(instance);

            // Integer fields round fractional results half away from zero (2.5 -> 3, -2.5 -> -3).
            // Convert.ChangeType alone used banker's rounding, so 7 / 2 gave 4 but 5 / 2 gave 2.
            if (IsIntegerType(instance.GetType()))
            {
                result = result switch
                {
                    double d => Math.Round(d, MidpointRounding.AwayFromZero),
                    float f => Math.Round((double)f, MidpointRounding.AwayFromZero),
                    decimal m => Math.Round(m, MidpointRounding.AwayFromZero),
                    _ => result
                };
            }

            return Convert.ChangeType(result, instance.GetType());
        }
        catch
        {
            // Second try, handle byte[], and casts from numerical values to string which need parsing.
            var ret = dynamicFunc(instance.ToParamEditorString());
            if (instance.GetType() == typeof(byte[]))
            {
                ret = ParamUtils.Dummy8Read((string)ret, ((byte[])instance).Length);
            }

            return Convert.ChangeType(ret, instance.GetType());
        }
    }

    private static bool IsIntegerType(Type type)
    {
        return type == typeof(byte) || type == typeof(sbyte)
            || type == typeof(short) || type == typeof(ushort)
            || type == typeof(int) || type == typeof(uint)
            || type == typeof(long) || type == typeof(ulong);
    }

    public static void AppendParamEditAction(this List<EditorAction> actions, Param.Row row,
        (ParamEditorPseudoColumn, Param.Column) col, object newval)
    {
        if (col.Item1 == ParamEditorPseudoColumn.ID)
        {
            if (!row.ID.Equals(newval))
            {
                actions.Add(new PropertiesChangedAction(row.GetType().GetProperty("ID"), -1, row, newval));
            }
        }
        else if (col.Item1 == ParamEditorPseudoColumn.Name)
        {
            if (row.Name == null || !row.Name.Equals(newval))
            {
                actions.Add(new PropertiesChangedAction(row.GetType().GetProperty("Name"), -1, row, newval));
            }
        }
        else
        {
            Param.Cell handle = row[col.Item2];
            if (!(handle.Value.Equals(newval)
                  || (handle.Value.GetType() == typeof(byte[])
                      && ParamUtils.ByteArrayEquals((byte[])handle.Value, (byte[])newval))))
            {
                actions.Add(new PropertiesChangedAction(handle.GetType().GetProperty("Value"), -1, handle, newval));
            }
        }
    }
}
