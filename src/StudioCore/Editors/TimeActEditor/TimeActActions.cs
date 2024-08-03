using DotNext.Collections.Generic;
using SoulsFormats;
using StudioCore.Editor;
using StudioCore.GraphicsEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SoulsFormats.GPARAM;

namespace StudioCore.Editors.TimeActEditor;

public class EventPropertyChange : EditorAction
{
    private Dictionary<string, object> Parameters;
    private string ParamName;
    private object OldValue;
    private object NewValue;

    public EventPropertyChange(Dictionary<string, object> parameters, string paramName, object oldValue, object newValue)
    {
        Parameters = parameters;
        ParamName = paramName;
        OldValue = oldValue;
        NewValue = newValue;
    }

    public override ActionEvent Execute()
    {
        Parameters[ParamName] = NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Parameters[ParamName] = OldValue;

        return ActionEvent.NoEvent;
    }
}

public class TimeActStartTimePropertyChange : EditorAction
{
    private TAE.Event Event;
    private object OldValue;
    private object NewValue;

    public TimeActStartTimePropertyChange(TAE.Event entry, object oldValue, object newValue)
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

public class TimeActEndTimePropertyChange : EditorAction
{
    private TAE.Event Event;
    private object OldValue;
    private object NewValue;

    public TimeActEndTimePropertyChange(TAE.Event entry, object oldValue, object newValue)
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