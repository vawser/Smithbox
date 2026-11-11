using Hexa.NET.ImGui;
using SoulsFormats;

namespace StudioCore.Editors.EmevdEditor;

/// <summary>
/// The widgets that edit one arg, shared by the Properties table and the block view's popup: its value (enum combo,
/// float or integer box) and which event parameter slot, if any, writes it.
/// </summary>
public class EmevdArgEditor
{
    private readonly EmevdEditorView View;

    public EmevdArgEditor(EmevdEditorView view)
    {
        View = view;
    }

    private EmevdCatalog Catalog => View.Project.Handler.EmevdData?.Catalog;

    /// <summary>
    /// The value box. Disabled when a parameter writes the arg, since the game replaces the stored value when the
    /// event starts.
    /// </summary>
    public void Value(string id, EmevdLine line, EmevdArg arg, float width = -1)
    {
        var file = View.Selection.SelectedFile;

        if (line.Params.TryGetValue(arg.Offset, out var p))
        {
            ImGui.TextColored(EmevdFormat.ParamChipColor, EmevdScript.SlotName(p.SourceStartByte, p.ByteCount));
            GUI.Tooltip(LOC.Get("EMEVD_Properties_Arg_Param_TT", EmevdFormat.RawValue(arg)));
            return;
        }

        if (arg.IsFloat)
        {
            if (View.Input.Float(id, arg.Float, out var f, width))
                View.Edit(new EmevdArgChange(file, line.Instr, arg.Offset, EmevdDecoded.EncodeFloat(f)));
            return;
        }

        var (min, max) = EmevdDecoded.Range(arg.Type);
        var e = Catalog?.FindEnum(arg.Def.EnumKey);
        long value;

        var changed = e != null && e.Values.Count > 0
            ? View.Input.Enum(id, e, arg.Int, min, max, out value, width)
            : View.Input.Int(id, arg.Int, min, max, out value, width);

        if (changed && EmevdDecoded.EncodeInt(arg.Type, value, out var bytes))
            View.Edit(new EmevdArgChange(file, line.Instr, arg.Offset, bytes));
    }

    /// <summary>A raw integer box beside an enum combo, for values the enum doesn't list</summary>
    public void RawInt(string id, EmevdLine line, EmevdArg arg, float width = -1)
    {
        if (arg.IsFloat || line.Params.ContainsKey(arg.Offset))
            return;

        var (min, max) = EmevdDecoded.Range(arg.Type);

        if (View.Input.Int(id, arg.Int, min, max, out var value, width) && EmevdDecoded.EncodeInt(arg.Type, value, out var bytes))
            View.Edit(new EmevdArgChange(View.Selection.SelectedFile, line.Instr, arg.Offset, bytes));
    }

    /// <summary>
    /// A combo choosing the event parameter slot that writes this arg: none, an existing slot of the arg's size, or a
    /// new slot after the last one.
    /// </summary>
    public void Slot(string id, EmevdScript script, EmevdLine line, EmevdArg arg, float width = -1)
    {
        line.Params.TryGetValue(arg.Offset, out var current);

        var label = current != null ? EmevdScript.SlotName(current.SourceStartByte, current.ByteCount) : LOC.Get("EMEVD_Edit_Slot_None");

        ImGui.SetNextItemWidth(width);
        if (!ImGui.BeginCombo(id, label))
        {
            GUI.Tooltip(LOC.Get("EMEVD_Edit_Slot_TT"));
            return;
        }

        EMEVD.Parameter chosen = null;
        var pick = false;

        if (ImGui.Selectable(LOC.Get("EMEVD_Edit_Slot_None"), current == null) && current != null)
            pick = true;

        foreach (var (source, size) in script.Slots.Where(s => s.Size == arg.Size))
        {
            var isCurrent = current != null && current.SourceStartByte == source && current.ByteCount == size;

            if (ImGui.Selectable($"{EmevdScript.SlotName(source, size)}##slot{source}", isCurrent) && !isCurrent)
            {
                chosen = new EMEVD.Parameter(line.Index, arg.Offset, source, size);
                pick = true;
            }
        }

        var next = NextSlot(script, arg.Size);
        if (ImGui.Selectable(LOC.Get("EMEVD_Edit_Slot_New", EmevdScript.SlotName(next, arg.Size))))
        {
            chosen = new EMEVD.Parameter(line.Index, arg.Offset, next, arg.Size);
            pick = true;
        }
        GUI.Tooltip(LOC.Get("EMEVD_Edit_Slot_New_TT"));

        ImGui.EndCombo();

        if (!pick)
            return;

        if (chosen != null)
            chosen.UnkID = current?.UnkID ?? NextUnkId(script.Event);

        View.Edit(new EmevdParamChange(View.Selection.SelectedFile, script.Event, current, chosen));
    }

    /// <summary>
    /// SDT and later number an event's parameters from 1 (UnkID, no known effect); earlier games leave it 0. A new
    /// parameter follows the event's own convention.
    /// </summary>
    private static int NextUnkId(EMEVD.Event ev)
    {
        return ev.Parameters.Any(p => p.UnkID != 0) ? ev.Parameters.Max(p => p.UnkID) + 1 : 0;
    }

    /// <summary>The first source byte after every slot, aligned to the arg's size</summary>
    public static long NextSlot(EmevdScript script, int size)
    {
        var end = script.Slots.Count == 0 ? 0 : script.Slots.Max(s => s.Source + s.Size);
        return (end + size - 1) / size * size;
    }
}
