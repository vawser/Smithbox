using Hexa.NET.ImGui;
using System.Globalization;

namespace StudioCore.Editors.EsdEditor;

/// <summary>
/// Text boxes for the Properties window that commit once, when the user finishes editing (Enter or clicking away), so
/// each edit is one undo step (the EMEVD editor's EmevdInput pattern). Only one item is active at a time, so one slot
/// holds the text being typed.
/// </summary>
public class EsdInput
{
    private uint ActiveId;
    private string ActiveText = "";

    /// <summary>
    /// A text box showing a value. Returns true with the typed text when an edit is finished and differs from the value.
    /// </summary>
    public bool Text(string id, string current, out string result, float width = -1, ImGuiInputTextFlags flags = ImGuiInputTextFlags.None)
    {
        result = current;
        var imId = ImGui.GetID(id);
        var shown = ActiveId == imId ? ActiveText : current;

        ImGui.SetNextItemWidth(width);
        var enter = ImGui.InputText(id, ref shown, 4096, flags | ImGuiInputTextFlags.EnterReturnsTrue);
        if (ImGui.IsItemActive())
        {
            ActiveId = imId;
            ActiveText = shown;
        }

        if (!(enter || ImGui.IsItemDeactivatedAfterEdit()) || ActiveId != imId)
            return false;

        ActiveId = 0;
        result = shown;
        return result != current;
    }

    /// <summary>The text being typed in a box, or null when the box isn't being edited</summary>
    public string Editing(string id) => ActiveId != 0 && ActiveId == ImGui.GetID(id) ? ActiveText : null;

    // --- completion (expression boxes) ---

    /// <summary>The identifier left of the cursor in the box being typed in ("" when there is none)</summary>
    public string WordAtCursor { get; private set; } = "";

    /// <summary>The completions offered for WordAtCursor: set by the caller each frame, Tab inserts the highlighted one</summary>
    public List<string> Completions = new();

    /// <summary>The completion ↑ / ↓ have moved to</summary>
    public int Highlight;

    [ThreadStatic] private static EsdInput Current;

    // Kept in a field: the native side holds the pointer while the box is drawn
    private static readonly unsafe ImGuiInputTextCallback CallbackDelegate = OnCallback;

    private static bool IsWordByte(byte b) => b == '_' || b is >= (byte)'0' and <= (byte)'9' or >= (byte)'A' and <= (byte)'Z' or >= (byte)'a' and <= (byte)'z';

    private static unsafe int OnCallback(ImGuiInputTextCallbackData* data)
    {
        var self = Current;
        if (self == null)
            return 0;

        // The word left of the cursor (the buffer is UTF-8; identifiers are ASCII)
        var cursor = data->CursorPos;
        var start = cursor;
        while (start > 0 && IsWordByte(data->Buf[start - 1]))
            start--;

        switch (data->EventFlag)
        {
            case ImGuiInputTextFlags.CallbackAlways:
                self.WordAtCursor = start < cursor ? System.Text.Encoding.ASCII.GetString(data->Buf + start, cursor - start) : "";
                break;

            case ImGuiInputTextFlags.CallbackHistory when self.Completions.Count > 0:
                self.Highlight += data->EventKey == ImGuiKey.UpArrow ? -1 : 1;
                self.Highlight = (self.Highlight % self.Completions.Count + self.Completions.Count) % self.Completions.Count;
                break;

            case ImGuiInputTextFlags.CallbackCompletion when self.Completions.Count > 0:
                {
                    var insert = self.Completions[Math.Clamp(self.Highlight, 0, self.Completions.Count - 1)];
                    var ptr = new ImGuiInputTextCallbackDataPtr(data);
                    ptr.DeleteChars(start, cursor - start);
                    ptr.InsertChars(start, insert);
                    self.Highlight = 0;
                    break;
                }
        }
        return 0;
    }

    /// <summary>
    /// A text box like Text, with completion of the word at the cursor: Tab inserts the highlighted entry of
    /// Completions, ↑ / ↓ move the highlight
    /// </summary>
    public unsafe bool CompletingText(string id, string current, out string result, float width = -1)
    {
        result = current;
        var imId = ImGui.GetID(id);
        var shown = ActiveId == imId ? ActiveText : current;

        ImGui.SetNextItemWidth(width);
        Current = this;
        var flags = ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.CallbackAlways | ImGuiInputTextFlags.CallbackCompletion | ImGuiInputTextFlags.CallbackHistory;
        var enter = ImGui.InputText(id, ref shown, 4096, flags, CallbackDelegate);
        Current = null;

        if (ImGui.IsItemActive())
        {
            if (ActiveId != imId)
            {
                Highlight = 0;
                WordAtCursor = "";
            }
            ActiveId = imId;
            ActiveText = shown;
        }

        if (!(enter || ImGui.IsItemDeactivatedAfterEdit()) || ActiveId != imId)
            return false;

        ActiveId = 0;
        WordAtCursor = "";
        result = shown;
        return result != current;
    }

    /// <summary>A whole-number box (decimal or 0x hex, the s32 range bytecode holds)</summary>
    public bool Int(string id, long current, out long result, float width = -1)
    {
        result = current;
        if (!Text(id, current.ToString(CultureInfo.InvariantCulture), out var text, width, ImGuiInputTextFlags.AutoSelectAll))
            return false;

        if (!ParseInt(text, out var value))
        {
            Reject(LOC.Get("ESD_Properties_Not_An_Int", text));
            return false;
        }

        result = value;
        return value != current;
    }

    /// <summary>A number box for float and double literals</summary>
    public bool Number(string id, double current, out double result, float width = -1)
    {
        result = current;
        if (!Text(id, EsdFormat.Number(current), out var text, width, ImGuiInputTextFlags.AutoSelectAll))
            return false;

        if (!double.TryParse(text.Trim().TrimEnd('f', 'F'), NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
        {
            Reject(LOC.Get("ESD_Properties_Not_A_Number", text));
            return false;
        }

        result = value;
        return BitConverter.DoubleToInt64Bits(value) != BitConverter.DoubleToInt64Bits(current);
    }

    /// <summary>
    /// A combo of an enum's named values. A value the enum doesn't name is listed too, so it stays selectable.
    /// Commits on selection.
    /// </summary>
    public static bool Enum(string id, EsdEnumDef def, long current, out long result, float width = -1)
    {
        result = current;

        string Label(long v)
        {
            var name = def.ValueName(v);
            return name != null ? $"{name} ({v})" : LOC.Get("ESD_Properties_Enum_Unnamed", v);
        }

        var changed = false;
        ImGui.SetNextItemWidth(width);
        if (ImGui.BeginCombo(id, Label(current), ImGuiComboFlags.HeightLarge))
        {
            if (!def.Values.ContainsKey(current))
                ImGui.Selectable(Label(current), true);

            foreach (var v in def.Values.Keys.OrderBy(k => k))
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

    /// <summary>A whole number in decimal or 0x hex, within the s32 range bytecode holds</summary>
    public static bool ParseInt(string text, out long value)
    {
        text = text.Trim();
        var ok = text.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
            ? long.TryParse(text.AsSpan(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value)
            : long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
        return ok && value >= int.MinValue && value <= int.MaxValue;
    }

    public static void Reject(string reason)
    {
        Smithbox.Log(typeof(EsdInput), reason, Microsoft.Extensions.Logging.LogLevel.Warning);
    }
}
