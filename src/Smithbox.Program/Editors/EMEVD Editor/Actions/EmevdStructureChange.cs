using SoulsFormats;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.EmevdEditor;

/// <summary>
/// A structural edit of one event (move, delete, insert, paste): swaps in the new instruction and parameter lists
/// and writes the skip counts the edit rewrote. Undo puts the old lists and bytes back. The lists are replaced, never
/// changed in place, and old Parameter objects are never touched, so undo restores the event exactly.
/// </summary>
public class EmevdStructureChange : EditorAction
{
    private readonly EmevdFileWrapper File;
    private readonly EMEVD.Event Event;

    private readonly List<EMEVD.Instruction> OldInstructions;
    private readonly List<EMEVD.Parameter> OldParameters;
    private readonly List<EMEVD.Instruction> NewInstructions;
    private readonly List<EMEVD.Parameter> NewParameters;
    private readonly List<(EMEVD.Instruction Instr, int Offset, byte[] Old, byte[] New)> Patches = new();

    public EmevdStructureChange(EmevdFileWrapper file, EMEVD.Event ev, EmevdEditResult result)
    {
        File = file;
        Event = ev;

        OldInstructions = ev.Instructions;
        OldParameters = ev.Parameters;
        NewInstructions = result.Instructions;
        NewParameters = result.Parameters;

        foreach (var (instr, offset, bytes) in result.Patches)
            Patches.Add((instr, offset, instr.ArgData[offset..(offset + bytes.Length)], bytes));
    }

    public override ActionEvent Execute()
    {
        Event.Instructions = NewInstructions;
        Event.Parameters = NewParameters;

        foreach (var (instr, offset, _, bytes) in Patches)
            bytes.CopyTo(instr.ArgData, offset);

        File.MarkChanged();
        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Event.Instructions = OldInstructions;
        Event.Parameters = OldParameters;

        foreach (var (instr, offset, bytes, _) in Patches)
            bytes.CopyTo(instr.ArgData, offset);

        File.MarkUndone();
        return ActionEvent.NoEvent;
    }
}
