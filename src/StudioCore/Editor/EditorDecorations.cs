using Hexa.NET.ImGui;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;

namespace StudioCore.Editor;

public class EditorDecorations
{
    public static bool HelpIcon(string id, ref string hint, bool canEdit)
    {
        if (hint == null)
        {
            return false;
        }

        return UIHints.AddImGuiHintButton(id, ref hint, canEdit, true); //presently a hack, move code here
    }

    public static bool ImguiTableSeparator()
    {
        var lastCol = false;
        var cols = ImGui.TableGetColumnCount();
        ImGui.TableNextRow();
        for (var i = 0; i < cols; i++)
        {
            if (ImGui.TableNextColumn())
            {
                ImGui.Separator();
                lastCol = true;
            }
        }

        return lastCol;
    }

    public static bool ImGuiTableStdColumns(string id, int cols, bool fixVerticalPadding)
    {
        Vector2 oldPad = ImGui.GetStyle().CellPadding;
        if (fixVerticalPadding)
        {
            ImGui.GetStyle().CellPadding = new Vector2(oldPad.X, 0);
        }

        var v = ImGui.BeginTable(id, cols,
            ImGuiTableFlags.Resizable | ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.SizingStretchSame |
            ImGuiTableFlags.ScrollY);

        if (fixVerticalPadding)
        {
            ImGui.GetStyle().CellPadding = oldPad;
        }

        return v;
    }

    public static void PinListReorderOptions<T>(List<T> sourceListToModify, T currentElement)
    {
        int indexOfPin = sourceListToModify.IndexOf(currentElement);
        if (indexOfPin > 0 && ImGui.Selectable("Move pin up"))
        {
            T prevKey = sourceListToModify[indexOfPin - 1];
            sourceListToModify[indexOfPin] = prevKey;
            sourceListToModify[indexOfPin - 1] = currentElement;
        }
        if (indexOfPin >= 0 && indexOfPin < sourceListToModify.Count - 1 && ImGui.Selectable("Move pin down"))
        {
            T nextKey = sourceListToModify[indexOfPin + 1];
            sourceListToModify[indexOfPin] = nextKey;
            sourceListToModify[indexOfPin + 1] = currentElement;
        }
    }

    /// <summary>
    ///     Displays information about the provided property.
    /// </summary>
    public static void ImGui_DisplayPropertyInfo(PropertyInfo prop)
    {
        ImGui_DisplayPropertyInfo(prop.PropertyType, prop.Name, true, true);
    }

    /// <summary>
    ///     Displays information about the provided property.
    /// </summary>
    public static void ImGui_DisplayPropertyInfo(System.Type propType, string fieldName, bool printName, bool printType, string altName = null, int arrayLength = -1, int bitSize = -1)
    {
        if (!string.IsNullOrWhiteSpace(altName))
        {
            fieldName += $"  /  {altName}";
        }

        if (CFG.Current.Param_FieldContextMenu_Name && printName)
        {
            ImGui.TextColored(new Vector4(1.0f, 0.7f, 0.4f, 1.0f), Utils.ImGuiEscape(fieldName, "", true));
        }

        if (CFG.Current.Param_FieldContextMenu_Split && !printType)
        {
            return;
        }

        if (bitSize != -1)
        {
            var str = $"Bitfield Type within: {fieldName}";
            var min = 0;
            var max = (2ul << (bitSize - 1)) - 1;
            str += $" (Min {min}, Max {max})";
            ImGui.TextColored(new Vector4(.4f, 1f, .7f, 1f), str);
        }
        else
        {
            if (propType.IsArray)
            {
                var str = $"Array Type: {propType.Name}";
                if (arrayLength > 0)
                {
                    str += $" (Length: {arrayLength})";
                }

                propType = propType.GetElementType();

                ImGui.TextColored(new Vector4(.4f, 1f, .7f, 1f), str);
            }

            if (propType.IsValueType)
            {
                var str = $"Value Type: {propType.Name}";
                var min = propType.GetField("MinValue")?.GetValue(propType);
                var max = propType.GetField("MaxValue")?.GetValue(propType);
                if (min != null && max != null)
                {
                    str += $" (Min {min}, Max {max})";
                }

                ImGui.TextColored(new Vector4(.4f, 1f, .7f, 1f), str);
            }
            else if (propType == typeof(string))
            {
                var str = $"String Type: {propType.Name}";
                if (arrayLength > 0)
                {
                    str += $" (Length: {arrayLength})";
                }

                ImGui.TextColored(new Vector4(.4f, 1f, .7f, 1f), str);
            }
        }
    }
}
