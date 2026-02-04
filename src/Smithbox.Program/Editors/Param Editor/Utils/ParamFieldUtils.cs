using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;
public class ParamFieldUtils
{
    public static string ImGui_InputFloatFormat(float f, int min = 3, int max = 6)
    {
        var split = f.ToString("F6").TrimEnd('0').Split('.');
        return $"%.{Math.Clamp(split.Last().Length, min, max)}f";
    }
    public static void ImGui_DisplayPropertyInfo(PropertyInfo prop)
    {
        ImGui_DisplayPropertyInfo(prop.PropertyType, prop.Name, true, true);
    }

    public static void ImGui_DisplayPropertyInfo(System.Type propType, string fieldName, bool printName, bool printType, string altName = null, int arrayLength = -1, int bitSize = -1)
    {
        if (!string.IsNullOrWhiteSpace(altName))
        {
            fieldName += $"  /  {altName}";
        }

        if (printName)
        {
            ImGui.TextColored(new Vector4(1.0f, 0.7f, 0.4f, 1.0f), Utils.ImGuiEscape(fieldName, "", true));
        }

        if (CFG.Current.ParamEditor_Field_Context_Split && !printType)
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
}
