using SoulsFormats;
using StudioCore.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TimeActEditor.Actions;

/// <summary>
/// Action: TAE.Animation.AnimFileName property change
/// </summary>
public class TaeAnimFileNameChange : EditorAction
{
    private TAE.Animation Animation;
    private object OldValue;
    private object NewValue;

    public TaeAnimFileNameChange(TAE.Animation entry, object oldValue, object newValue)
    {
        Animation = entry;
        OldValue = oldValue;
        NewValue = newValue;
    }

    public override ActionEvent Execute()
    {
        Animation.AnimFileName = (string)NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Animation.AnimFileName = (string)OldValue;

        return ActionEvent.NoEvent;
    }
}