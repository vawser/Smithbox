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
public static class ParamFieldUtils
{
    /// <summary>
    /// The ImGui format for showing <paramref name="f"/> with as many decimals as it has (at least
    /// <paramref name="min"/>, at most <paramref name="max"/>). The default cap was 6 decimals, which shows values
    /// such as 0.0000005 as 0.000, and editing any digit then wrote the rounded value back. A float only carries
    /// about 7 significant digits, so 10 decimals covers every value a param is likely to hold.
    /// </summary>
    public static string ImGui_InputFloatFormat(float f, int min = 3, int max = 10)
    {
        // Huge values have no decimals worth showing (and don't fit in a decimal)
        if (float.IsNaN(f) || float.IsInfinity(f) || Math.Abs(f) >= 1e15f)
            return $"%.{min}f";

        // The shortest text that reads back as the same float ("0.1", "5E-07"), then written out without an
        // exponent ("0.0000005"). Going through double instead would show 0.1f as 0.1000000015.
        var invariant = System.Globalization.CultureInfo.InvariantCulture;
        var shortest = f.ToString("R", invariant);
        if (!decimal.TryParse(shortest, System.Globalization.NumberStyles.Float, invariant, out var exact))
            return $"%.{max}f";

        var text = exact.ToString(invariant);

        var decimals = 0;
        var point = text.IndexOf('.');

        if (point >= 0)
        {
            decimals = text.TrimEnd('0').Length - point - 1;
        }

        return $"%.{Math.Clamp(decimals, min, max)}f";
    }
    /// <summary>
    /// After a search box: outline it in the error colour while its search failed
    /// </summary>
    public static void OutlineSearchBoxOnError(string searchError)
    {
        if (string.IsNullOrEmpty(searchError))
            return;

        ImGui.GetWindowDrawList().AddRect(ImGui.GetItemRectMin(), ImGui.GetItemRectMax(),
            ImGui.GetColorU32(UI.Current.ImGui_Invalid_Text_Color));
    }

    /// <summary>
    /// A warning icon on the same line, with the search failure in its tooltip. The search headers have a fixed
    /// height, so the message lives in the tooltip rather than on a line of its own.
    /// </summary>
    public static void DisplaySearchError(string searchError)
    {
        if (string.IsNullOrEmpty(searchError))
            return;

        ImGui.SameLine();
        ImGui.AlignTextToFramePadding();
        ImGui.TextColored(UI.Current.ImGui_Invalid_Text_Color, Icons.ExclamationTriangle);

        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip($"{LOC.Get("PARAM_SE_Error_Search_Header")}\n{searchError}");
        }
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

        if (!printType)
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
