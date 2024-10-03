using StudioCore.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using static SoulsFormats.EMEVD;

namespace StudioCore.Editors.EmevdEditor;

/// <summary>
/// Instruction - Property Change (Generic)
/// </summary>
public class InstructionPropertyChange : EditorAction
{
    private object Argument;
    private object OldValue;
    private object NewValue;

    public InstructionPropertyChange(object arg, object newValue)
    {
        Argument = arg;
        OldValue = arg;
        NewValue = newValue;
    }

    public override ActionEvent Execute()
    {
        Argument = NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Argument = OldValue;

        return ActionEvent.NoEvent;
    }
}

/// <summary>
/// Instruction - Property Change (Generic)
/// </summary>
public class InstructionArgumentChange : EditorAction
{
    private Instruction Instruction;
    private byte[] OldArgData;
    private byte[] NewArgData;

    public InstructionArgumentChange(Instruction ins, byte[] oldData, byte[] newData)
    {
        Instruction = ins;
        OldArgData = oldData;
        NewArgData = newData;
    }

    public override ActionEvent Execute()
    {
        Instruction.ArgData = NewArgData;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Instruction.ArgData = OldArgData;

        return ActionEvent.NoEvent;
    }
}