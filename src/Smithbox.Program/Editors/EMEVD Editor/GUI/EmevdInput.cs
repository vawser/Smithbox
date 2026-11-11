using Hexa.NET.ImGui;
using System.Globalization;

namespace StudioCore.Editors.EmevdEditor;

/// <summary>
/// Value inputs that commit once, when the user finishes editing (Enter or clicking away), so each edit is one undo
/// step. Integers are typed as text, so the whole u32 range fits (ImGui's int box stops at 2^31 - 1, and EMEVD uses
/// 4294967295 for "none"); "0x" prefixes hex. Only one item is active at a time, so one slot is enough.
/// </summary>
public class EmevdInput
{
    private uint ActiveId;
    private float ActiveFloat;
    private string ActiveText = "";

    /// <summary>An integer box. Returns true with the new value when an edit is committed within [min, max].</summary>
    public bool Int(string id, long current, long min, long max, out long result, float width = -1)
    {
        result = current;
        var imId = ImGui.GetID(id);
        var shown = ActiveId == imId ? ActiveText : current.ToString(CultureInfo.InvariantCulture);

        ImGui.SetNextItemWidth(width);
        if (ImGui.InputText(id, ref shown, 32, ImGuiInputTextFlags.AutoSelectAll))
        {
            ActiveId = imId;
            ActiveText = shown;
        }

        if (!ImGui.IsItemDeactivatedAfterEdit() || ActiveId != imId)
            return false;

        ActiveId = 0;

        if (!TryParse(ActiveText, out var value))
        {
            Reject(LOC.Get("EMEVD_Edit_Not_A_Number", ActiveText));
            return false;
        }

        if (value < min || value > max)
        {
            Reject(LOC.Get("EMEVD_Edit_Out_Of_Range", value, min, max));
            return false;
        }

        result = value;
        return result != current;
    }

    public bool Float(string id, float current, out float result, float width = -1)
    {
        result = current;
        var imId = ImGui.GetID(id);
        var shown = ActiveId == imId ? ActiveFloat : current;

        ImGui.SetNextItemWidth(width);
        if (ImGui.InputFloat(id, ref shown, 0, 0, "%g"))
        {
            ActiveId = imId;
            ActiveFloat = shown;
        }

        if (!ImGui.IsItemDeactivatedAfterEdit() || ActiveId != imId)
            return false;

        ActiveId = 0;
        result = ActiveFloat;

        // Compare bits, so -0 vs 0 still counts as an edit
        return BitConverter.SingleToInt32Bits(result) != BitConverter.SingleToInt32Bits(current);
    }

    /// <summary>
    /// A combo of an enum's named values. A value the enum doesn't name is listed too, so it stays selectable.
    /// Commits on selection.
    /// </summary>
    public bool Enum(string id, EmevdEnumDef def, long current, long min, long max, out long result, float width = -1)
    {
        result = current;

        string Label(long v)
        {
            var name = def.ValueName(v);
            return name != null ? $"{v}: {name}" : LOC.Get("EMEVD_Edit_Enum_Unnamed", v);
        }

        var changed = false;

        ImGui.SetNextItemWidth(width);
        if (ImGui.BeginCombo(id, Label(current)))
        {
            if (!def.Values.ContainsKey(current))
                ImGui.Selectable(Label(current), true);

            foreach (var v in def.Values.Keys.Where(k => k >= min && k <= max).OrderBy(k => k))
            {
                if (ImGui.Selectable($"{Label(v)}##{v}", v == current) && v != current)
                {
                    result = v;
                    changed = true;
                }

                if (v == current)
                    ImGui.SetItemDefaultFocus();
            }

            ImGui.EndCombo();
        }

        return changed;
    }

    /// <summary>A hex byte string box ("0A FF 12" or "0aff12"). Commits on Enter or when the box loses focus.</summary>
    public bool HexBytes(string id, string current, out byte[] result, float width = -1)
    {
        result = null;
        var imId = ImGui.GetID(id);
        var shown = ActiveId == imId ? ActiveText : current;

        ImGui.SetNextItemWidth(width);
        if (ImGui.InputText(id, ref shown, 1024, ImGuiInputTextFlags.CharsHexadecimal | ImGuiInputTextFlags.AutoSelectAll))
        {
            ActiveId = imId;
            ActiveText = shown;
        }

        if (!ImGui.IsItemDeactivatedAfterEdit() || ActiveId != imId)
            return false;

        ActiveId = 0;

        var digits = new string(ActiveText.Where(Uri.IsHexDigit).ToArray());
        if (digits.Length == 0 || digits.Length % 2 != 0)
        {
            Reject(LOC.Get("EMEVD_Edit_Hex_Invalid", ActiveText));
            return false;
        }

        result = new byte[digits.Length / 2];
        for (var i = 0; i < result.Length; i++)
            result[i] = byte.Parse(digits.AsSpan(i * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);

        return true;
    }

    private static bool TryParse(string text, out long value)
    {
        text = text.Trim();

        if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            return long.TryParse(text.AsSpan(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value);

        return long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
    }

    private static void Reject(string reason)
    {
        Smithbox.Log(typeof(EmevdInput), reason, Microsoft.Extensions.Logging.LogLevel.Warning);
    }
}
