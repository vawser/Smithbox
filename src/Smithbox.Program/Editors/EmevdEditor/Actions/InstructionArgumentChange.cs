using StudioCore.Editor;
using static SoulsFormats.EMEVD;

namespace StudioCore.EventScriptEditorNS;

/// <summary>
/// Updates the instruction's ArgData on property change
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