using SoulsFormats;
using StudioCore.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TimeActEditor.Actions;

/// <summary>
/// Action: TAE.Event create
/// </summary>
public class TaeEventCreate : EditorAction
{
    private TAE.Event NewEvent;
    private List<TAE.Event> EventList;
    private int InsertionIndex;

    public TaeEventCreate(TAE.Event entryNewEvent, List<TAE.Event> eventList, int index)
    {
        InsertionIndex = index;
        NewEvent = entryNewEvent;
        EventList = eventList;
    }

    public override ActionEvent Execute()
    {
        EventList.Insert(InsertionIndex, NewEvent);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        EventList.RemoveAt(InsertionIndex);

        return ActionEvent.NoEvent;
    }
}