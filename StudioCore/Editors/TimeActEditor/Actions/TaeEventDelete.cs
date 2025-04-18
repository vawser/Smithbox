using SoulsFormats;
using StudioCore.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TimeActEditor.Actions;

/// <summary>
/// Action: TAE.Event delete
/// </summary>
public class TaeEventDelete : EditorAction
{
    private TAE.Event StoredEvent;
    private List<TAE.Event> EventList;
    private int RemovalIndex;

    public TaeEventDelete(TAE.Event entryOldEvent, List<TAE.Event> eventList, int index)
    {
        RemovalIndex = index;
        StoredEvent = entryOldEvent;
        EventList = eventList;
    }

    public override ActionEvent Execute()
    {
        EventList.RemoveAt(RemovalIndex);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        EventList.Insert(RemovalIndex, StoredEvent);

        return ActionEvent.NoEvent;
    }
}