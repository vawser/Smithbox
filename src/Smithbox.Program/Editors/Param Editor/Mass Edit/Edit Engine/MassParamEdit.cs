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

    // "\;" and "\:" in a script stand for a literal ; or : (which otherwise separate commands and stages). They are
    // swapped for these private-use characters before the script is split, and swapped back where a value is used
    // (selectors and operation arguments). In a regex, \; and \: already meant a literal ; and :, so the meaning
    // there is unchanged.
    private const char EscapedSemicolon = '';
    private const char EscapedColon = '';

    /// <summary>
    /// Hides escaped separators from the script splitter. Call on the whole script before splitting it.
    /// </summary>
    public static string EscapeScript(string script)
    {
        return script?.Replace("\\;", EscapedSemicolon.ToString()).Replace("\\:", EscapedColon.ToString());
    }

    /// <summary>
    /// Turns hidden separators back into ; and : (for a selector or argument about to be used)
    /// </summary>
    public static string Unescape(string text)
    {
        return text?.Replace(EscapedSemicolon, ';').Replace(EscapedColon, ':');
    }

    /// <summary>
    /// Shows hidden separators as the user typed them (\; and \:), for messages
    /// </summary>
    public static string Reescape(string text)
    {
        return text?.Replace(EscapedSemicolon.ToString(), "\\;").Replace(EscapedColon.ToString(), "\\:");
    }

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
