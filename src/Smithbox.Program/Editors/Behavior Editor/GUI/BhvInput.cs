using Hexa.NET.ImGui;
using System.Globalization;

namespace StudioCore.Editors.BehaviorEditor;

/// <summary>
/// Value inputs that commit once, when the user finishes editing (Enter or clicking away), so each edit is one undo
/// step. ImGui writes the typed value only on frames where the text changes, while the caller re-reads the field
/// every frame, so the value being typed is kept here until it's committed. Only one item is active at a time, so
/// one slot is enough.
/// </summary>
public class BhvInput
{
    private uint ActiveId;
    private int ActiveInt;
    private float ActiveFloat;
    private string ActiveText = "";

    /// <summary>
    /// An integer box. Returns true with the new value when an edit is committed within [min, max] and passes
    /// <paramref name="validate"/> (which returns an error, or null). A rejected value is logged and dropped.
    /// </summary>
    public bool Int(string id, long current, long min, long max, out long result, Func<long, string> validate = null, float width = -1)
    {
        result = current;
        var imId = ImGui.GetID(id);
        var shown = ActiveId == imId ? ActiveInt : (int)Math.Clamp(current, int.MinValue, int.MaxValue);

        ImGui.SetNextItemWidth(width);
        if (ImGui.InputInt(id, ref shown, 0, 0))
        {
            ActiveId = imId;
            ActiveInt = shown;
        }

        if (!ImGui.IsItemDeactivatedAfterEdit() || ActiveId != imId)
            return false;

        ActiveId = 0;

        if (ActiveInt < min || ActiveInt > max)
        {
            Reject(LOC.Get("BHV_Edit_Out_Of_Range", ActiveInt, min, max));
            return false;
        }

        var error = validate?.Invoke(ActiveInt);
        if (error != null)
        {
            Reject(error);
            return false;
        }

        result = ActiveInt;
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

        // Compare bits, so -0 vs 0 and NaN payloads still count as edits
        return BitConverter.SingleToInt32Bits(result) != BitConverter.SingleToInt32Bits(current);
    }

    /// <summary>
    /// A combo of an enum's named values. A value the enum doesn't name is listed too, so it stays selectable.
    /// Commits on selection.
    /// </summary>
    public bool Enum(string id, BhvEnums enums, string enumName, int current, out int result, float width = -1)
    {
        result = current;

        var values = enums?.GetValues(enumName);
        if (values == null || values.Count == 0)
        {
            return Int(id, current, byte.MinValue, byte.MaxValue, out var r, null, width) && Assign(out result, (int)r);
        }

        string Label(int v) => values.TryGetValue(v, out var name) ? $"{v}: {name}" : LOC.Get("BHV_Edit_Enum_Unnamed", v);

        var changed = false;

        ImGui.SetNextItemWidth(width);
        if (ImGui.BeginCombo(id, Label(current)))
        {
            if (!values.ContainsKey(current))
                ImGui.Selectable(Label(current), true);

            foreach (var v in values.Keys.OrderBy(k => k))
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

    /// <summary>
    /// A hex byte string box ("0A FF 12" or "0aff12"). Commits on Enter or when the box loses focus.
    /// </summary>
    public bool HexBytes(string id, string current, out byte[] result, float width = -1)
    {
        result = null;
        var imId = ImGui.GetID(id);
        var shown = ActiveId == imId ? ActiveText : current;

        ImGui.SetNextItemWidth(width);
        if (ImGui.InputText(id, ref shown, 256, ImGuiInputTextFlags.CharsHexadecimal | ImGuiInputTextFlags.AutoSelectAll))
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
            Reject(LOC.Get("BHV_Edit_Hex_Invalid", ActiveText));
            return false;
        }

        result = new byte[digits.Length / 2];
        for (int i = 0; i < result.Length; i++)
            result[i] = byte.Parse(digits.AsSpan(i * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);

        return true;
    }

    private static bool Assign(out int target, int value)
    {
        target = value;
        return true;
    }

    private static void Reject(string reason)
    {
        Smithbox.Log(typeof(BhvInput), reason, Microsoft.Extensions.Logging.LogLevel.Warning);
    }
}
