using SoulsFormats;
using StudioCore.Editors.TimeActEditor.Actions;
using StudioCore.Editors.TimeActEditor.Utils;
using StudioCore.TextEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor;

public class TextActionHandler
{
    private TextEditorScreen Screen;

    public TextActionHandler(TextEditorScreen screen)
    {
        Screen = screen;
    }

    public void OnProjectChanged()
    {

    }

    /// <summary>
    /// Duplicate current selection of FMG.Entries
    /// </summary>
    public void DuplicateEntries()
    {
        if (Screen.Selection._selectedFmgEntry == null)
            return;

        if (Screen.Selection._selectedFmgEntryIndex == -1)
            return;

        SortedDictionary<int, FMG.Entry> storedEntries = Screen.Selection.FmgEntryMultiselect.StoredEntries;

        var selectedContainer = Screen.Selection.SelectedContainer;
        var selectedFmg = Screen.Selection.SelectedFmg;

        int lastEntryIdx = -1;

        // Single
        if (storedEntries.Count <= 1)
        {
            FMG.Entry curEntry = selectedFmg.Entries[storedEntries.First().Key];

            // Get next valid ID
            var validID = false;
            var newID = curEntry.ID;

            while (!validID)
            {
                newID++;

                if (!selectedFmg.Entries.Any(e => e.ID == newID))
                {
                    validID = true;
                }
            }

            Screen.EditorActionManager.ExecuteAction(
                new DuplicateFmgEntry(selectedContainer, selectedFmg, curEntry, newID));
        }
        // Multi-Select
        else
        {
            /*
            List<int> insertIndices = new List<int>();
            List<TAE.Event> newEvents = new List<TAE.Event>();

            for (int i = 0; i < animations.Events.Count; i++)
            {
                if (storedEvents.ContainsKey(i))
                {
                    TAE.Event curEvent = animations.Events[i];
                    int insertIdx = animations.Events.IndexOf(curEvent);
                    insertIndices.Add(insertIdx);
                    TAE.Event dupeEvent = curEvent.GetClone(false);
                    newEvents.Add(dupeEvent);

                    lastEventIdx = insertIdx;
                }
            }

            EditorActionManager.ExecuteAction(new TaeEventMultiDuplicate(newEvents, animations.Events, insertIndices));

            // Select last newly duplicated event
            if (lastEventIdx != -1)
            {
                TimeActUtils.SelectNewEvent(lastEventIdx);
            }
            */
        }
    }

    /// <summary>
    /// Delete current selection of FMG.Entries
    /// </summary>
    public void DeleteEntries()
    {

    }
}