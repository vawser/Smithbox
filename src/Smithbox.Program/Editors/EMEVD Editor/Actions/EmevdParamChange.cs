using SoulsFormats;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.EmevdEditor;

/// <summary>
/// Links an arg to an event parameter slot, changes its slot, or unlinks it: replaces the event parameter that
/// writes an instruction's arg (or adds one, or removes it). Undo puts the old parameter back at its list position,
/// so an untouched event writes back in its original order.
/// </summary>
public class EmevdParamChange : EditorAction, IEmevdEdit
{
    private readonly EmevdFileWrapper File;
    private readonly EMEVD.Event Event;
    private readonly EMEVD.Parameter OldParam;
    private readonly EMEVD.Parameter NewParam;
    private int Index = -1;

    /// <param name="oldParam">The parameter now writing the arg, or null to add one</param>
    /// <param name="newParam">The parameter to write it with, or null to unlink the arg</param>
    public EmevdParamChange(EmevdFileWrapper file, EMEVD.Event ev, EMEVD.Parameter oldParam, EMEVD.Parameter newParam)
    {
        File = file;
        Event = ev;
        OldParam = oldParam;
        NewParam = newParam;
    }

    public bool HasEffect => OldParam != null || NewParam != null;

    public override ActionEvent Execute()
    {
        if (OldParam != null)
        {
            Index = Event.Parameters.IndexOf(OldParam);

            if (NewParam != null)
                Event.Parameters[Index] = NewParam;
            else
                Event.Parameters.RemoveAt(Index);
        }
        else
        {
            // New links go after the other parameters of the same instruction, then by target byte, as vanilla
            // orders them
            Index = Event.Parameters.FindIndex(p => p.InstructionIndex > NewParam.InstructionIndex
                || (p.InstructionIndex == NewParam.InstructionIndex && p.TargetStartByte > NewParam.TargetStartByte));

            if (Index < 0)
                Index = Event.Parameters.Count;

            Event.Parameters.Insert(Index, NewParam);
        }

        File.MarkChanged();
        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        if (OldParam != null)
        {
            if (NewParam != null)
                Event.Parameters[Index] = OldParam;
            else
                Event.Parameters.Insert(Index, OldParam);
        }
        else
        {
            Event.Parameters.RemoveAt(Index);
        }

        File.MarkUndone();
        return ActionEvent.NoEvent;
    }
}
