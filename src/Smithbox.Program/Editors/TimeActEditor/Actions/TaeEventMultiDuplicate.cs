using SoulsFormats;
using StudioCore.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TimeActEditor.Actions;

/// <summary>
/// Action: TAE.Event duplicate (multiple)
/// </summary>
public class TaeEventMultiDuplicate : EditorAction
{
    private List<TAE.Event> NewEvents;
    private List<TAE.Event> EventList;
    private List<int> InsertionIndexes;

    public TaeEventMultiDuplicate(List<TAE.Event> newEvents, List<TAE.Event> eventList, List<int> indexList)
    {
        InsertionIndexes = indexList;
        NewEvents = newEvents;
        EventList = eventList;
    }

    public override ActionEvent Execute()
    {
        for (int i = 0; i < InsertionIndexes.Count; i++)
        {
            TAE.Event curNewEvent = NewEvents[i];
            int curIndex = InsertionIndexes[i];

            EventList.Insert(curIndex, curNewEvent);
        }

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        foreach (TAE.Event entry in NewEvents)
        {
            EventList.Remove(entry);
        }

        return ActionEvent.NoEvent;
    }
}