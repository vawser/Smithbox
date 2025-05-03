using StudioCore.Editor;
using static SoulsFormats.EMEVD;
using static StudioCore.Editors.EmevdEditor.EmevdBank;

namespace StudioCore.Editors.EmevdEditor;

/// <summary>
/// Updates the instruction's ArgData on property change
/// </summary>
public class InstructionArgumentChange : EditorAction
{
    private Instruction Instruction;
    private byte[] OldArgData;
    private byte[] NewArgData;
    private EventScriptInfo Info;

    public InstructionArgumentChange(EventScriptInfo info, Instruction ins, byte[] oldData, byte[] newData)
    {
        Info = info;
        Instruction = ins;
        OldArgData = oldData;
        NewArgData = newData;
    }

    public override ActionEvent Execute()
    {
        Info.IsModified = true;
        Instruction.ArgData = NewArgData;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Info.IsModified = false;
        Instruction.ArgData = OldArgData;

        return ActionEvent.NoEvent;
    }
}