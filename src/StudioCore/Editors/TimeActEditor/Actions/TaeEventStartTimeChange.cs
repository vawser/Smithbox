using SoulsFormats;
using StudioCore.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TimeActEditor.Actions;

/// <summary>
/// Action: TAE.Event.StartTime property change
/// </summary>
public class TaeEventStartTimeChange : EditorAction
{
    private TAE.Event Event;
    private object OldValue;
    private object NewValue;

    public TaeEventStartTimeChange(TAE.Event entry, object oldValue, object newValue)
    {
        Event = entry;
        OldValue = oldValue;
        NewValue = newValue;
    }

    public override ActionEvent Execute()
    {
        Event.StartTime = (float)NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Event.StartTime = (float)OldValue;

        return ActionEvent.NoEvent;
    }
}