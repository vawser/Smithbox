using SoulsFormats;
using StudioCore.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TimeActEditor.Actions;

/// <summary>
/// Action: TAE.Event delete (multiple)
/// </summary>
public class TaeEventMultiDelete : EditorAction
{
    private List<TAE.Event> StoredEvents;
    private List<TAE.Event> EventList;
    private List<int> RemovalIndices;
    private List<int> InsertIndices;

    public TaeEventMultiDelete(List<TAE.Event> storedEvents, List<TAE.Event> eventList, List<int> removalIndices)
    {
        RemovalIndices = removalIndices;
        StoredEvents = storedEvents;
        EventList = eventList;
    }

    public override ActionEvent Execute()
    {
        for (int i = RemovalIndices.Count - 1; i >= 0; i--)
        {
            int curIndex = RemovalIndices[i];
            EventList.RemoveAt(curIndex);
        }

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        for (int i = 0; i < RemovalIndices.Count; i++)
        {
            TAE.Event storedEvent = StoredEvents[i];
            int curIndex = RemovalIndices[i];

            EventList.Insert(curIndex, storedEvent);
        }

        return ActionEvent.NoEvent;
    }
}