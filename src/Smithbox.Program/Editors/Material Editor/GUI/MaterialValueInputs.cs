using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Utilities;
using System.Numerics;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.MaterialEditor;

/// <summary>
/// The material param inputs that need more than a number box: colours, and drag speeds that suit the value
/// </summary>
public static class MaterialValueInputs
{
    // snp_0_color_3, as the shaders name colour inputs; Float5 ones are RGB, alpha and an intensity, which the
    // renderer multiplies emissive colours by (MaterialDefinition.ResolveTextureInputs)
    private static readonly Regex ShaderColor = new(@"(^|_)snp_\d+_color_\d+$", RegexOptions.Compiled);

    /// <summary>
    /// Whether a param holds a colour: a shader colour input, or an engine one named *Color (g_DiffuseMapColor)
    /// </summary>
    public static bool IsColor(string name) =>
        name != null && (ShaderColor.IsMatch(name) || name.EndsWith("Color", StringComparison.Ordinal));

    /// <summary>
    /// A drag speed of about 1% of the value, so small values (colours, strengths) and large ones (tiling,
    /// distances) both move at a usable rate
    /// </summary>
    public static float Speed(float value) => MathF.Max(MathF.Abs(value) * 0.01f, 0.001f);
    public static float Speed(Vector2 v) => Speed(MathF.Max(MathF.Abs(v.X), MathF.Abs(v.Y)));
    public static float Speed(Vector3 v) => Speed(MathF.Max(MathF.Abs(v.X), MathF.Max(MathF.Abs(v.Y), MathF.Abs(v.Z))));
    public static float Speed(Vector4 v) => Speed(MathF.Max(MathF.Max(MathF.Abs(v.X), MathF.Abs(v.Y)), MathF.Max(MathF.Abs(v.Z), MathF.Abs(v.W))));

    /// <summary>
    /// A colour picker for a colour param of 3, 4 or 5 floats. The fifth float (intensity) gets its own drag.
    /// Values over 1 are allowed (HDR), as the game's colours use them.
    /// </summary>
    public static bool TryColor(object sourceObj, object oldval, out object newval, out (bool, bool) result)
    {
        newval = null;
        result = (false, false);

        var name = sourceObj switch
        {
            MATBIN.Param p => p.Name,
            MTD.Param p => p.Name,
            _ => null
        };

        if (!IsColor(name) || oldval is not float[] { Length: >= 3 and <= 5 } values)
            return false;

        var flags = ImGuiColorEditFlags.Float | ImGuiColorEditFlags.Hdr;
        var changed = false;
        var committed = false;
        var edited = (float[])values.Clone();

        if (values.Length == 3)
        {
            var color = new Vector3(values[0], values[1], values[2]);

            ImGui.SetNextItemWidth(-1);
            if (ImGui.ColorEdit3("##value", ref color, flags))
            {
                edited[0] = color.X; edited[1] = color.Y; edited[2] = color.Z;
                changed = true;
            }
            committed |= ImGui.IsItemDeactivatedAfterEdit();
        }
        else
        {
            var color = new Vector4(values[0], values[1], values[2], values[3]);

            ImGui.SetNextItemWidth(-1);
            if (ImGui.ColorEdit4("##value", ref color, flags | ImGuiColorEditFlags.AlphaPreviewHalf))
            {
                edited[0] = color.X; edited[1] = color.Y; edited[2] = color.Z; edited[3] = color.W;
                changed = true;
            }
            committed |= ImGui.IsItemDeactivatedAfterEdit();

            if (values.Length == 5)
            {
                var intensity = values[4];

                ImGui.SetNextItemWidth(-1);
                if (ImGui.DragFloat("##value_intensity", ref intensity, Speed(intensity), 0, float.MaxValue,
                        Utils.ImGui_InputFloatFormat(intensity)))
                {
                    edited[4] = intensity;
                    changed = true;
                }
                committed |= ImGui.IsItemDeactivatedAfterEdit();
                GUI.Tooltip(LOC.Get("MAT_Properties_Color_Intensity_TT"));
            }
        }

        if (changed)
            newval = edited;

        result = (changed, committed || !ImGui.IsAnyItemActive());
        return true;
    }

    /// <summary>
    /// The blend mode combo for a MATBIN's g_BlendMode, which the renderer reads like the MTD one
    /// (0 opaque, 1 alpha test, 2 alpha blend, 4 add, 5 subtract)
    /// </summary>
    public static bool TryBlendMode(object sourceObj, object oldval, out object newval, out (bool, bool) result)
    {
        newval = null;
        result = (false, false);

        if (sourceObj is not MATBIN.Param { Name: "g_BlendMode", Type: MATBIN.ParamType.Int } || oldval is not int current)
            return false;

        Array enumVals = Enum.GetValues(typeof(MTD.BlendMode));
        var enumNames = Enum.GetNames(typeof(MTD.BlendMode));
        var intVals = new int[enumVals.Length];

        for (var i = 0; i < enumVals.Length; i++)
        {
            intVals[i] = (int)enumVals.GetValue(i);
        }

        // A value the enum doesn't name is left to the number box
        if (!intVals.Contains(current))
            return false;

        var changed = false;

        ImGui.SetNextItemWidth(-1);
        if (Utils.EnumEditor(enumVals, enumNames, (MTD.BlendMode)current, out var val, intVals))
        {
            newval = Convert.ToInt32(val);
            changed = true;
        }

        result = (changed, ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive());
        return true;
    }
}
