using SoulsFormats;
using StudioCore.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TimeActEditor.Actions;

/// <summary>
/// Action: TAE.Event.EndTime property change
/// </summary>
public class TaeEventEndTimeChange : EditorAction
{
    private TAE.Event Event;
    private object OldValue;
    private object NewValue;

    public TaeEventEndTimeChange(TAE.Event entry, object oldValue, object newValue)
    {
        Event = entry;
        OldValue = oldValue;
        NewValue = newValue;
    }

    public override ActionEvent Execute()
    {
        Event.EndTime = (float)NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Event.EndTime = (float)OldValue;

        return ActionEvent.NoEvent;
    }
}
