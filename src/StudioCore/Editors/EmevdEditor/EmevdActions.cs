using StudioCore.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.EmevdEditor;

/// <summary>
/// Instruction - Property Change (Generic)
/// </summary>
public class InstructionPropertyChange : EditorAction
{
    private ArgDataObject DataObject;
    private object OldValue;
    private object NewValue;

    public InstructionPropertyChange(ArgDataObject dataObject, object oldValue, object newValue)
    {
        DataObject = dataObject;
        OldValue = oldValue;
        NewValue = newValue;
    }

    public override ActionEvent Execute()
    {
        DataObject.ArgObject = NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        DataObject.ArgObject = OldValue;

        return ActionEvent.NoEvent;
    }
}