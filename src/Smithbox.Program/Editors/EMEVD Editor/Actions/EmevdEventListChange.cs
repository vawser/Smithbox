using SoulsFormats;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.EmevdEditor;

/// <summary>Adds an event to a file at an index, or removes one; undo does the opposite at the same place</summary>
public class EmevdEventListChange : EditorAction
{
    private readonly EmevdFileWrapper File;
    private readonly EMEVD.Event Event;
    private readonly bool Add;
    private int Index;

    public static EmevdEventListChange Insert(EmevdFileWrapper file, EMEVD.Event ev, int index) => new(file, ev, true, index);

    public static EmevdEventListChange Remove(EmevdFileWrapper file, EMEVD.Event ev) => new(file, ev, false, file.Emevd.Events.IndexOf(ev));

    private EmevdEventListChange(EmevdFileWrapper file, EMEVD.Event ev, bool add, int index)
    {
        File = file;
        Event = ev;
        Add = add;
        Index = index;
    }

    public override ActionEvent Execute()
    {
        var events = File.Emevd.Events;

        if (Add)
            events.Insert(Math.Clamp(Index, 0, events.Count), Event);
        else
            events.RemoveAt(Index);

        File.MarkChanged();
        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        var events = File.Emevd.Events;

        if (Add)
            events.Remove(Event);
        else
            events.Insert(Math.Clamp(Index, 0, events.Count), Event);

        File.MarkUndone();
        return ActionEvent.NoEvent;
    }
}
