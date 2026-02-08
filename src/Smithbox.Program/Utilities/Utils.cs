using DotNext;
using Hexa.NET.ImGui;
using Microsoft.Win32;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.Viewport;
using StudioCore.Keybinds;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using Veldrid;
using Veldrid.Utilities;

namespace StudioCore.Utilities;

public static class Utils
{
    public const float Pi = (float)Math.PI;
    public const float PiOver2 = (float)Math.PI / 2.0f;
    public const float Rad2Deg = 180.0f / Pi;
    public const float Deg2Rad = Pi / 180.0f;

    private static readonly char[] _dirSep = { '\\', '/' };

    public static float DegToRadians(float deg)
    {
        return deg * Pi / 180.0f;
    }

    public static float RadiansToDeg(float rad)
    {
        return rad * 180.0f / Pi;
    }

    public static float Clamp(float f, float min, float max)
    {
        if (f < min)
        {
            return min;
        }

        if (f > max)
        {
            return max;
        }

        return f;
    }

    public static float Lerp(float a, float b, float d)
    {
        return (a * (1.0f - d)) + (b * d);
    }

    public static bool IsPowerTwo(uint a)
    {
        if (a > 0)
        {
            while (a % 2 == 0)
            {
                a >>= 1;
            }

            if (a == 1)
            {
                return true;
            }
        }

        return false;
    }

    public static Matrix4x4 Inverse(this Matrix4x4 src)
    {
        Matrix4x4.Invert(src, out Matrix4x4 result);
        return result;
    }


    // Vector rotation functions from John Alexiou at https://stackoverflow.com/questions/69245724/rotate-a-vector-around-an-axis-in-3d-space
    /// <summary>
    ///     Rotates a vector using the Rodriguez rotation formula
    ///     about an arbitrary axis.
    /// </summary>
    public static Vector3 RotateVector(Vector3 vector, Vector3 axis, float angle)
    {
        Vector3 vxp = Vector3.Cross(axis, vector);
        Vector3 vxvxp = Vector3.Cross(axis, vxp);
        return vector + ((float)Math.Sin(angle) * vxp) + ((1 - (float)Math.Cos(angle)) * vxvxp);
    }

    /// <summary>
    ///     Rotates a vector about a point in space.
    /// </summary>
    /// <returns>The rotated vector</returns>
    public static Vector3 RotateVectorAboutPoint(Vector3 vector, Vector3 pivot, Vector3 axis, float angle)
    {
        return pivot + RotateVector(vector - pivot, axis, angle);
    }


    private static double GetColorComponent(double temp1, double temp2, double temp3)
    {
        double num;
        temp3 = MoveIntoRange(temp3);
        if (temp3 < 0.166666666666667)
        {
            num = temp1 + ((temp2 - temp1) * 6 * temp3);
        }
        else if (temp3 >= 0.5)
        {
            num = temp3 >= 0.666666666666667 ? temp1 : temp1 + ((temp2 - temp1) * (0.666666666666667 - temp3) * 6);
        }
        else
        {
            num = temp2;
        }

        return num;
    }

    private static double GetTemp2(float H, float S, float L)
    {
        double temp2;
        temp2 = L >= 0.5 ? L + S - (L * S) : L * (1 + (double)S);
        return temp2;
    }

    /// <summary>
    ///     Derived from https://stackoverflow.com/a/1626232
    /// </summary>
    public static Vector3 ColorToHSV(Color color)
    {
        int max = Math.Max(color.R, Math.Max(color.G, color.B));
        int min = Math.Min(color.R, Math.Min(color.G, color.B));

        var hue = color.GetHue();
        var saturation = max == 0 ? 0 : 1.0f - (1.0f * min / max);
        var value = max / 255.0f;

        return new Vector3(hue, saturation, value);
    }

    /// <summary>
    ///     Derived from https://stackoverflow.com/a/1626232
    /// </summary>
    public static Color ColorFromHSV(Vector3 hsv)
    {
        var hue = hsv.X;
        var saturation = hsv.Y;
        var value = hsv.Z;

        var hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
        var f = (hue / 60) - (float)Math.Floor(hue / 60);

        value *= 255.0f;
        var v = Convert.ToInt32(value);
        var p = Convert.ToInt32(value * (1 - saturation));
        var q = Convert.ToInt32(value * (1 - (f * saturation)));
        var t = Convert.ToInt32(value * (1 - ((1 - f) * saturation)));

        if (hi == 0)
        {
            return Color.FromArgb(255, v, t, p);
        }

        if (hi == 1)
        {
            return Color.FromArgb(255, q, v, p);
        }

        if (hi == 2)
        {
            return Color.FromArgb(255, p, v, t);
        }

        if (hi == 3)
        {
            return Color.FromArgb(255, p, q, v);
        }

        if (hi == 4)
        {
            return Color.FromArgb(255, t, p, v);
        }

        return Color.FromArgb(255, v, p, q);
    }

    private static double MoveIntoRange(double temp3)
    {
        if (temp3 < 0)
        {
            temp3 += 1;
        }
        else if (temp3 > 1)
        {
            temp3 -= 1;
        }

        return temp3;
    }

    public static Matrix4x4 GetBoneWorldMatrix(FLVER.Node bone, List<FLVER.Node> bones)
    {
        Matrix4x4 matrix = bone.ComputeLocalTransform();
        while (bone.ParentIndex != -1)
        {
            bone = bones[bone.ParentIndex];
            matrix *= bone.ComputeLocalTransform();
        }

        return matrix;
    }

    public static void PrintBoneInfo(List<FLVER.Node> bones, bool debug)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine();
        sb.AppendLine("==== BONE INFO START ====");
        for (int i = 0; i < bones.Count; i++)
        {
            var bone = bones[i];
            sb.AppendLine($"[{i}] {bone.Name}");
            sb.AppendLine($"\tTranslation: {bone.Translation}");
            sb.AppendLine($"\tRotation: {bone.Rotation}");
            sb.AppendLine($"\tScale: {bone.Scale}");
            sb.AppendLine($"\tParent Index: {bone.ParentIndex}");
            sb.AppendLine($"\tChild Index: {bone.FirstChildIndex}");
            sb.AppendLine($"\tNext Sibling Index: {bone.NextSiblingIndex}");
            sb.AppendLine($"\tPrevious Sibling Index: {bone.PreviousSiblingIndex}");
            sb.AppendLine($"\tLocal Transform: {bone.ComputeLocalTransform()}");
            sb.AppendLine($"\tWorld Transform: {GetBoneWorldMatrix(bone, bones)}");
        }
        sb.AppendLine("==== BONE INFO END ====");
        sb.AppendLine();

        Smithbox.Log(typeof(Utils), sb.ToString());
    }

    public static void PrintTransformInfo(Matrix4x4[] transforms, bool debug)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine();
        sb.AppendLine("==== TRANSFORM INFO START ====");
        for (int i = 0; i < transforms.Length; i++)
        {
            sb.AppendLine($"{transforms[i]}");
        }
        sb.AppendLine("==== TRANSFORM INFO END ====");
        sb.AppendLine();

        Smithbox.Log(typeof(Utils), sb.ToString());
    }

    public static void setRegistry(string name, string value)
    {
#if WINDOWS
        RegistryKey rkey = Registry.CurrentUser.CreateSubKey(@"Software\Smithbox");
        rkey.SetValue(name, value);
#endif
    }

    public static string readRegistry(string name)
    {
#if WINDOWS
        RegistryKey rkey = Registry.CurrentUser.CreateSubKey(@"Software\Smithbox");
        var v = rkey.GetValue(name);
        return v == null ? null : v.ToString();
#else
        return null;
#endif
    }

    /// <summary>
    ///     Replace # with fullwidth # to prevent ImGui from hiding text when detecting ## and ###.
    ///     Optionally replaces %, which is only an issue with certain imgui elements.
    /// </summary>
    public static string ImGuiEscape(string str, string nullStr = "", bool percent = false)
    {
        if (str == null)
        {
            return nullStr;
        }

        str = str.Replace("#", "\xFF03"); // FF03 is eastern block #

        if (percent)
        {
            str = str.Replace("%", "%%");
        }

        return str;
    }

    public static bool EnumEditor(Array enumVals, string[] enumNames, object oldval, out object val, int[] intVals)
    {
        val = null;

        for (var i = 0; i < enumNames.Length; i++)
        {
            enumNames[i] = $"{intVals[i]}: {enumNames[i]}";
        }

        int index = Array.IndexOf(enumVals, oldval);

        if (ImGui.Combo("##", ref index, enumNames, enumNames.Length))
        {
            val = enumVals.GetValue(index);
            return true;
        }

        return false;
    }

    public static void ImGuiGenericHelpPopup(string buttonText, string imguiID, string displayText)
    {
        if (ImGui.Button(buttonText + "##" + imguiID, DPI.StandardButtonSize))
        {
            ImGui.OpenPopup(imguiID);
        }

        if (ImGui.BeginPopup(imguiID))
        {
            ImGui.Text(displayText);
            ImGui.EndPopup();
        }
    }

    public static void ImGui_InputUint(string text, ref uint val)
    {
        var strval = $@"{val}";
        if (ImGui.InputText(text, ref strval, 16))
        {
            var res = uint.TryParse(strval, out var refval);
            if (res)
            {
                val = refval;
            }
        }
    }

    /// <summary>
    ///     Inserts new lines into a string to make it fit in the specified UI width.
    /// </summary>
    public static string ImGui_WordWrapString(string text, float uiWidth, int maxLines = 3)
    {
        var textWidth = ImGui.CalcTextSize(text).X;

        // Determine how many line breaks are needed
        var rowNum = float.Ceiling(textWidth / uiWidth);
        if (rowNum > maxLines)
        {
            rowNum = maxLines;
        }

        // Insert line breaks into text
        for (float iRow = 1; iRow < rowNum; iRow++)
        {
            var pos_default = (int)(text.Length * (iRow / rowNum));
            int pos_final;
            var iPos = 0;
            var sign = 1;
            while (true)
            {
                // Find position in string to insert new line without interrupting any words
                pos_final = pos_default + (iPos * sign);
                if (pos_final <= pos_default * 0.7f || pos_final >= pos_default * 1.3f)
                {
                    // Couldn't find empty position within limited range, insert at fractional position instead.
                    text = text.Insert(pos_default, "-\n ");
                    break;
                }

                if (text[pos_final] is ' ' or '-')
                {
                    text = text.Insert(pos_final, "\n");
                    break;
                }

                sign *= -1;
                if (sign == -1)
                {
                    iPos++;
                }
            }
        }

        return text;
    }

    /// <summary>
    ///     Generates display format for ImGui float input.
    ///     Made to display trailing zeroes even if value is an integer,
    ///     and limit number of decimals to appropriate values.
    /// </summary>
    public static string ImGui_InputFloatFormat(float f, int min = 3, int max = 6)
    {
        var split = f.ToString("F6").TrimEnd('0').Split('.');
        return $"%.{Math.Clamp(split.Last().Length, min, max)}f";
    }

    /// <summary>
    ///     Returns string representing version of param or regulation.
    /// </summary>
    public static string ParseParamVersion(ulong version)
    {
        string verStr = version.ToString();
        if (verStr.Length == 7 || verStr.Length == 8)
        {
            char major = verStr[0];
            string minor = verStr[1..3];
            char patch = verStr[3];
            string rev = verStr[4..];
            return $"{major}.{minor}.{patch}.{rev}";
        }

        return "Unknown version format";
    }

    public static int ParseHexFromString(string str)
    {
        return int.Parse(str.Replace("0x", ""), System.Globalization.NumberStyles.HexNumber);
    }

    public static string ParseRegulationVersion(ulong version)
    {
        string verStr = version.ToString();
        if (verStr.Length != 8)
        {
            return "Unknown Version";
        }
        char major = verStr[0];
        string minor = verStr[1..3];
        char patch = verStr[3];
        string rev = verStr[4..];

        return $"{major}.{minor}.{patch}.{rev}";
    }


    public static Vector3 GetDecimalColor(Color color)
    {
        float r = Convert.ToSingle(color.R);
        float g = Convert.ToSingle(color.G);
        float b = Convert.ToSingle(color.B);
        Vector3 vec = new Vector3((r / 255), (g / 255), (b / 255));

        //throw new NotImplementedException($"{vec}");

        return vec;
    }

    public static Vector3 GetDecimalColor(Vector3 color)
    {
        float r = Convert.ToSingle(color.X);
        float g = Convert.ToSingle(color.Y);
        float b = Convert.ToSingle(color.Z);
        Vector3 vec = new Vector3((r / 255), (g / 255), (b / 255));

        //throw new NotImplementedException($"{vec}");

        return vec;
    }

    public static Vector3 GetRgbColor(Vector3 color)
    {
        float r = Convert.ToSingle(color.X);
        float g = Convert.ToSingle(color.Y);
        float b = Convert.ToSingle(color.Z);
        Vector3 vec = new Vector3((r * 255), (g * 255), (b * 255));

        //throw new NotImplementedException($"{vec}");

        return vec;
    }

    public static int GenerateRandomInt(RandomNumberGenerator randomSource, int min, int max)
    {
        double randomValue = randomSource.NextDouble();

        Smithbox.Log(typeof(Utils), $"randomValue: {randomValue}");

        int diff = max - min;

        Smithbox.Log(typeof(Utils), $"diff: {diff}");
        // In-case the order is swapped
        if (max < min)
            diff = min - max;

        double tResult = (diff * randomValue);

        Smithbox.Log(typeof(Utils), $"tResult: {tResult}");
        Smithbox.Log(typeof(Utils), $"tResult Rounded: {(int)Math.Round(tResult)}");
        return (int)Math.Round(tResult);
    }

    public static double GenerateRandomDouble(RandomNumberGenerator randomSource, double min, double max)
    {
        double randomValue = randomSource.NextDouble();

        double diff = max - min;

        // In-case the order is swapped
        if (max < min)
            diff = min - max;

        return (diff * randomValue);
    }
}
