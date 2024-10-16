using SoulsFormats;
using StudioCore.Editor;
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

        List<EditorAction> actions = new List<EditorAction>();

        var newId = -1;

        foreach (var entry in storedEntries)
        {
            var curFmg = entry.Value.Parent;
            var curEntry = entry.Value;

            // Get next valid ID
            if (newId == -1)
            {
                newId = GetNextValidId(curFmg, curEntry);
            }
            else
            {
                newId = GetNextValidId(curFmg, curEntry, newId);
            }

            var actionList = ProcessDuplicate(curEntry, newId);
            foreach(var action in actionList)
            {
                actions.Add(action);
            }
        }

        // Reverse so ID order makes sense
        actions.Reverse();

        var groupAction = new FmgGroupedAction(actions);
        Screen.EditorActionManager.ExecuteAction(groupAction);
    }

    private List<EditorAction> ProcessDuplicate(FMG.Entry curEntry, int newId)
    {
        List<EditorAction> actions = new List<EditorAction>();

        var selectedContainer = Screen.Selection.SelectedContainer;
        var selectedFmg = curEntry.Parent;
        var fmgEntryGroup = Screen.EntryGroupManager.GetEntryGroup(curEntry);

        if (newId != -1)
        {
            // Duplicate all related entries together based on selection
            if (fmgEntryGroup != null)
            {
                if (fmgEntryGroup.Title != null)
                {
                    var titleEntry = fmgEntryGroup.Title;
                    var titleFmg = titleEntry.Parent;

                    actions.Add(new DuplicateFmgEntry(selectedContainer, titleFmg, titleEntry, newId));
                }

                if (fmgEntryGroup.Summary != null)
                {
                    var summaryEntry = fmgEntryGroup.Summary;
                    var summaryFmg = summaryEntry.Parent;

                    actions.Add(new DuplicateFmgEntry(selectedContainer, summaryFmg, summaryEntry, newId));
                }

                if (fmgEntryGroup.Description != null)
                {
                    var descriptionEntry = fmgEntryGroup.Description;
                    var descriptionFmg = descriptionEntry.Parent;

                    actions.Add(new DuplicateFmgEntry(selectedContainer, descriptionFmg, descriptionEntry, newId));
                }

                if (fmgEntryGroup.Effect != null)
                {
                    var effectEntry = fmgEntryGroup.Effect;
                    var effectFmg = effectEntry.Parent;

                    actions.Add(new DuplicateFmgEntry(selectedContainer, effectFmg, effectEntry, newId));
                }
            }
            // Otherwise just duplicate selection
            else
            {
                actions.Add(new DuplicateFmgEntry(selectedContainer, selectedFmg, curEntry, newId));
            }
        }

        return actions;
    }

    /// <summary>
    /// Delete current selection of FMG.Entries
    /// </summary>
    public void DeleteEntries()
    {
        if (Screen.Selection._selectedFmgEntry == null)
            return;

        if (Screen.Selection._selectedFmgEntryIndex == -1)
            return;

        SortedDictionary<int, FMG.Entry> storedEntries = Screen.Selection.FmgEntryMultiselect.StoredEntries;

        List<EditorAction> actions = new List<EditorAction>();

        foreach (var entry in storedEntries)
        {
            var curFmg = entry.Value.Parent;
            var curEntry = entry.Value;

            var actionList = ProcessDelete(curEntry);
            foreach (var action in actionList)
            {
                actions.Add(action);
            }
        }

        // Reverse so ID order makes sense
        actions.Reverse();

        var groupAction = new FmgGroupedAction(actions);
        Screen.EditorActionManager.ExecuteAction(groupAction);
    }

    private List<EditorAction> ProcessDelete(FMG.Entry curEntry)
    {
        List<EditorAction> actions = new List<EditorAction>();

        var selectedContainer = Screen.Selection.SelectedContainer;
        var selectedFmg = curEntry.Parent;
        var fmgEntryGroup = Screen.EntryGroupManager.GetEntryGroup(curEntry);

        // Delete all related entries together based on selection
        if (fmgEntryGroup != null)
        {
            if (fmgEntryGroup.Title != null)
            {
                var titleEntry = fmgEntryGroup.Title;
                var titleFmg = titleEntry.Parent;

                actions.Add(new DeleteFmgEntry(selectedContainer, titleFmg, titleEntry));
            }

            if (fmgEntryGroup.Summary != null)
            {
                var summaryEntry = fmgEntryGroup.Summary;
                var summaryFmg = summaryEntry.Parent;

                actions.Add(new DeleteFmgEntry(selectedContainer, summaryFmg, summaryEntry));
            }

            if (fmgEntryGroup.Description != null)
            {
                var descriptionEntry = fmgEntryGroup.Description;
                var descriptionFmg = descriptionEntry.Parent;

                actions.Add(new DeleteFmgEntry(selectedContainer, descriptionFmg, descriptionEntry));
            }

            if (fmgEntryGroup.Effect != null)
            {
                var effectEntry = fmgEntryGroup.Effect;
                var effectFmg = effectEntry.Parent;

                actions.Add(new DeleteFmgEntry(selectedContainer, effectFmg, effectEntry));
            }
        }
        // Otherwise just duplicate selection
        else
        {
            actions.Add(new DeleteFmgEntry(selectedContainer, selectedFmg, curEntry));
        }

        return actions;
    }

    public int GetNextValidId(FMG selectedFmg, FMG.Entry curEntry, int newId = -1)
    {
        var validID = false;

        var newID = newId;

        if (newId == -1)
            newID = curEntry.ID;

        while (!validID)
        {
            newID++;

            if (!selectedFmg.Entries.Any(e => e.ID == newID))
            {
                validID = true;
            }
        }

        return newID;
    }
}