using SoulsFormats;
using System.Collections.Generic;
using System.Linq;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Utilities;

namespace StudioCore.Editors.TextEditor;

public class TextActionHandler
{
    private TextEditorView Parent;
    private ProjectEntry Project;

    public TextActionHandler(TextEditorView view, ProjectEntry project)
    {
        Parent = view;
        Project = project;
    }

    /// <summary>
    /// Add title entry (used for associated entries)
    /// </summary>
    public void AddTitleEntry(TextFmgWrapper curFmgWrapper, FMG.Entry curEntry)
    {
        var selectedContainer = Parent.Selection.SelectedContainerWrapper;

        var wrapper = Parent.EntryGroupManager.GetAssociatedTitleWrapper(curFmgWrapper.ID);

        if (wrapper != null)
        {
            var newEntry = new FMG.Entry(wrapper.File, curEntry.ID, "");

            newEntry.ID = curEntry.ID;

            var action = new AddAssociatedEntry(Parent, selectedContainer, wrapper.File, newEntry);
            Parent.ActionManager.ExecuteAction(action);
        }
    }

    /// <summary>
    /// Add summary entry (used for associated entries)
    /// </summary>
    public void AddSummaryEntry(TextFmgWrapper curFmgWrapper, FMG.Entry curEntry)
    {
        var selectedContainer = Parent.Selection.SelectedContainerWrapper;

        var wrapper = Parent.EntryGroupManager.GetAssociatedSummaryWrapper(curFmgWrapper.ID);

        if (wrapper != null)
        {
            var newEntry = new FMG.Entry(wrapper.File, curEntry.ID, "");

            newEntry.ID = curEntry.ID;

            var action = new AddAssociatedEntry(Parent, selectedContainer, wrapper.File, newEntry);
            Parent.ActionManager.ExecuteAction(action);
        }
    }

    /// <summary>
    /// Add description entry (used for associated entries)
    /// </summary>
    public void AddDescriptionEntry(TextFmgWrapper curFmgWrapper, FMG.Entry curEntry)
    {
        var selectedContainer = Parent.Selection.SelectedContainerWrapper;

        var wrapper = Parent.EntryGroupManager.GetAssociatedDescriptionWrapper(curFmgWrapper.ID);

        if (wrapper != null)
        {
            var newEntry = new FMG.Entry(wrapper.File, curEntry.ID, "");

            newEntry.ID = curEntry.ID;

            var action = new AddAssociatedEntry(Parent, selectedContainer, wrapper.File, newEntry);
            Parent.ActionManager.ExecuteAction(action);
        }
    }

    /// <summary>
    /// Add effect entry (used for associated entries)
    /// </summary>
    public void AddEffectEntry(TextFmgWrapper curFmgWrapper, FMG.Entry curEntry)
    {
        var selectedContainer = Parent.Selection.SelectedContainerWrapper;

        var wrapper = Parent.EntryGroupManager.GetAssociatedEffectWrapper(curFmgWrapper.ID);

        if (wrapper != null)
        {
            var newEntry = new FMG.Entry(wrapper.File, curEntry.ID, "");

            newEntry.ID = curEntry.ID;

            var action = new AddAssociatedEntry(Parent, selectedContainer, wrapper.File, newEntry);
            Parent.ActionManager.ExecuteAction(action);
        }
    }

    /// <summary>
    /// Duplicate current selection of FMG.Entries
    /// </summary>
    public void DuplicateEntries()
    {
        if (!FocusManager.IsFocus(EditorFocusContext.TextEditor_EntryList))
            return;

        if (Parent.Selection._selectedFmgEntry == null)
            return;

        if (Parent.Selection._selectedFmgEntryIndex == -1)
            return;

        Parent.Selection.RevalidateEntrySelection();

        SortedDictionary<int, FMG.Entry> storedEntries = Parent.Selection.FmgEntryMultiselect.StoredEntries;

        List<EditorAction> actions = new List<EditorAction>();

        var newId = -1;
        var claimedIds = new HashSet<int>();

        // One ID index for the whole operation, built before any action runs
        var lookup = new FmgEntryLookup();

        foreach (var entry in storedEntries)
        {
            var curEntry = entry.Value;

            // Get next valid ID: free in every FMG the duplicate will be written to
            // (the selected FMG plus its title/summary/description/effect FMGs),
            // and not already used by an earlier duplicate in this batch.
            var startId = newId == -1 ? curEntry.ID : newId;
            newId = GetNextValidId(GetGroupFmgs(curEntry, lookup), startId, claimedIds, lookup);
            claimedIds.Add(newId);

            // If this is enabled, just use the current entry ID
            if(CFG.Current.TextEditor_Text_Entry_List_Ignore_ID_Check)
            {
                newId = curEntry.ID;
            }

            var actionList = ProcessDuplicate(curEntry, newId, lookup);
            foreach(var action in actionList)
            {
                actions.Add(action);
            }
        }

        // Reverse so ID order makes sense
        actions.Reverse();

        var groupAction = new FmgGroupedAction(actions);
        Parent.ActionManager.ExecuteAction(groupAction);
    }

    /// <remarks>
    /// No focus check: this is only reachable from the duplicate popup, which the
    /// entry-list shortcut opens. Hover-based focus changed as soon as the mouse crossed
    /// another panel on the way to Submit, which made Submit silently do nothing.
    /// </remarks>
    public void DuplicateEntriesPopup(int offset, int amount, bool autoAdjust)
    {
        if (Parent.Selection._selectedFmgEntry == null)
            return;

        if (Parent.Selection._selectedFmgEntryIndex == -1)
            return;

        Parent.Selection.RevalidateEntrySelection();

        SortedDictionary<int, FMG.Entry> storedEntries = Parent.Selection.FmgEntryMultiselect.StoredEntries;

        List<EditorAction> actions = new List<EditorAction>();

        var skippedCollisions = 0;
        var claimedIds = new HashSet<int>();

        // One ID index for the whole operation, built before any action runs
        var lookup = new FmgEntryLookup();

        foreach (var entry in storedEntries)
        {
            var curFmg = entry.Value.Parent;
            var curEntry = entry.Value;
            var groupFmgs = GetGroupFmgs(curEntry, lookup);

            for (int i = 0; i < amount; i++)
            {
                var newId = curEntry.ID + offset;

                if (autoAdjust)
                {
                    newId = curEntry.ID + (offset * (i + 1));
                }

                // Respect Allow Duplicate ID: don't create an entry whose ID already exists
                // in any FMG of the group, or that another duplicate in this batch claimed.
                if (!CFG.Current.TextEditor_Text_Entry_Allow_Duplicate_ID)
                {
                    if (!IsIdFree(groupFmgs, newId, lookup: lookup) || !claimedIds.Add(newId))
                    {
                        skippedCollisions++;
                        continue;
                    }
                }

                var actionList = ProcessDuplicate(curEntry, newId, lookup);
                foreach (var action in actionList)
                {
                    actions.Add(action);
                }
            }
        }

        if (skippedCollisions > 0)
        {
            Smithbox.LogWarning(this, LOC.Get("TEXT_DuplicateModal_Skipped_Collisions", skippedCollisions));
        }

        if (actions.Count == 0)
            return;

        // Reverse so ID order makes sense
        actions.Reverse();

        var groupAction = new FmgGroupedAction(actions);
        Parent.ActionManager.ExecuteAction(groupAction);
    }

    /// <summary>
    /// The FMGs a duplicate or ID change of this entry writes to: its own FMG, plus the
    /// associated title/summary/description/effect FMGs when the entry is grouped.
    /// </summary>
    public List<FMG> GetGroupFmgs(FMG.Entry entry, FmgEntryLookup lookup = null)
    {
        var fmgs = new List<FMG>();

        if (entry?.Parent != null)
            fmgs.Add(entry.Parent);

        var selectedFmg = Parent.Selection.SelectedFmgWrapper;
        var group = Parent.EntryGroupManager.GetEntryGroup(entry, lookup);

        if (selectedFmg != null && group != null && group.SupportsGrouping && CFG.Current.TextEditor_Text_Entry_Enable_Grouped_Entries)
        {
            var groupManager = Parent.EntryGroupManager;
            var associated = new[]
            {
                groupManager.GetAssociatedTitleWrapper(selectedFmg.ID),
                groupManager.GetAssociatedSummaryWrapper(selectedFmg.ID),
                groupManager.GetAssociatedDescriptionWrapper(selectedFmg.ID),
                groupManager.GetAssociatedEffectWrapper(selectedFmg.ID)
            };

            foreach (var wrapper in associated)
            {
                if (wrapper?.File != null && !fmgs.Contains(wrapper.File))
                    fmgs.Add(wrapper.File);
            }
        }

        return fmgs;
    }

    public static bool IsIdFree(List<FMG> fmgs, int id, FMG.Entry ignoreEntry = null, FmgEntryLookup lookup = null)
    {
        // Indexed path for bulk operations (no ignoreEntry needed there)
        if (lookup != null && ignoreEntry == null)
        {
            foreach (var fmg in fmgs)
            {
                if (lookup.Find(fmg, id) != null)
                    return false;
            }

            return true;
        }

        foreach (var fmg in fmgs)
        {
            foreach (var curEntry in fmg.Entries)
            {
                if (curEntry.ID == id && curEntry != ignoreEntry)
                    return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Returns the name of the first FMG in the list that already contains the ID, or null.
    /// </summary>
    public string FindIdCollision(List<FMG> fmgs, int id, FMG.Entry ignoreEntry = null)
    {
        foreach (var fmg in fmgs)
        {
            foreach (var curEntry in fmg.Entries)
            {
                if (curEntry.ID == id && curEntry != ignoreEntry)
                    return fmg.Name;
            }
        }

        return null;
    }

    private List<EditorAction> ProcessDuplicate(FMG.Entry curEntry, int newId, FmgEntryLookup lookup = null)
    {
        List<EditorAction> actions = new List<EditorAction>();

        var selectedContainer = Parent.Selection.SelectedContainerWrapper;
        var selectedFmg = curEntry.Parent;
        var fmgEntryGroup = Parent.EntryGroupManager.GetEntryGroup(curEntry, lookup);

        if (fmgEntryGroup != null)
        {
            if (newId != -1)
            {
                // Duplicate all related entries together based on selection
                if (fmgEntryGroup.SupportsGrouping && CFG.Current.TextEditor_Text_Entry_Enable_Grouped_Entries)
                {
                    if (fmgEntryGroup.Title != null)
                    {
                        var titleEntry = fmgEntryGroup.Title;
                        var titleFmg = titleEntry.Parent;

                        actions.Add(new DuplicateFmgEntry(Parent, selectedContainer, titleFmg, titleEntry, newId));
                    }

                    if (fmgEntryGroup.Summary != null)
                    {
                        var summaryEntry = fmgEntryGroup.Summary;
                        var summaryFmg = summaryEntry.Parent;

                        actions.Add(new DuplicateFmgEntry(Parent, selectedContainer, summaryFmg, summaryEntry, newId));
                    }

                    if (fmgEntryGroup.Description != null)
                    {
                        var descriptionEntry = fmgEntryGroup.Description;
                        var descriptionFmg = descriptionEntry.Parent;

                        actions.Add(new DuplicateFmgEntry(Parent, selectedContainer, descriptionFmg, descriptionEntry, newId));
                    }

                    if (fmgEntryGroup.Effect != null)
                    {
                        var effectEntry = fmgEntryGroup.Effect;
                        var effectFmg = effectEntry.Parent;

                        actions.Add(new DuplicateFmgEntry(Parent, selectedContainer, effectFmg, effectEntry, newId));
                    }
                }
                // Otherwise just duplicate selection
                else
                {
                    actions.Add(new DuplicateFmgEntry(Parent, selectedContainer, selectedFmg, curEntry, newId));
                }
            }
        }

        return actions;
    }

    /// <summary>
    /// Delete current selection of FMG.Entries.
    /// Deleting two or more selected entries asks for confirmation first
    /// (single deletes stay instant; Ctrl+Z covers mistakes).
    /// </summary>
    public void DeleteEntries()
    {
        if (!FocusManager.IsFocus(EditorFocusContext.TextEditor_EntryList))
            return;

        if (Parent.Selection._selectedFmgEntry == null)
            return;

        if (Parent.Selection._selectedFmgEntryIndex == -1)
            return;

        Parent.Selection.RevalidateEntrySelection();

        var storedEntries = Parent.Selection.FmgEntryMultiselect.StoredEntries;

        if (storedEntries.Count <= 0)
            return;

        if (storedEntries.Count >= 2)
        {
            // Count what the delete will remove, including associated entries in linked FMGs
            var lookup = new FmgEntryLookup();
            var totalDeletes = 0;

            foreach (var entry in storedEntries.Values)
            {
                totalDeletes += ProcessDelete(entry, lookup).Count;
            }

            var associatedCount = Math.Max(0, totalDeletes - storedEntries.Count);

            Parent.TextDeleteConfirmPopup.Request(storedEntries.Count, associatedCount);
            return;
        }

        ExecuteDelete();
    }

    /// <summary>
    /// Delete the selected entries without asking. Called directly for single deletes,
    /// and by the confirmation popup once the user confirms.
    /// </summary>
    public void ExecuteDelete()
    {
        Parent.Selection.RevalidateEntrySelection();

        if (Parent.Selection.FmgEntryMultiselect.StoredEntries.Count <= 0)
            return;

        SortedDictionary<int, FMG.Entry> storedEntries = Parent.Selection.FmgEntryMultiselect.StoredEntries;

        // Where to put the selection afterwards: the row that ends up below the deleted ones
        var firstDeletedIndex = storedEntries.Keys.First();

        // One ID index for the whole operation, built before any action runs
        var lookup = new FmgEntryLookup();

        List<EditorAction> actions = new List<EditorAction>();

        foreach (var entry in storedEntries)
        {
            var curFmg = entry.Value.Parent;
            var curEntry = entry.Value;

            var actionList = ProcessDelete(curEntry, lookup);
            foreach (var action in actionList)
            {
                actions.Add(action);
            }
        }

        // Reverse so ID order makes sense
        actions.Reverse();

        var groupAction = new FmgGroupedAction(actions);
        Parent.ActionManager.ExecuteAction(groupAction);

        // Don't leave the deleted entry selected (and editable) in the Contents panel
        Parent.Selection.SelectEntryNearIndex(firstDeletedIndex);
    }

    private List<EditorAction> ProcessDelete(FMG.Entry curEntry, FmgEntryLookup lookup = null)
    {
        List<EditorAction> actions = new List<EditorAction>();

        var selectedContainer = Parent.Selection.SelectedContainerWrapper;
        var selectedFmg = curEntry.Parent;
        var fmgEntryGroup = Parent.EntryGroupManager.GetEntryGroup(curEntry, lookup);

        if (fmgEntryGroup != null)
        {
            // Delete all related entries together based on selection
            if (fmgEntryGroup.SupportsGrouping && CFG.Current.TextEditor_Text_Entry_Enable_Grouped_Entries)
            {
                if (fmgEntryGroup.Title != null)
                {
                    var titleEntry = fmgEntryGroup.Title;
                    var titleFmg = titleEntry.Parent;

                    actions.Add(new DeleteFmgEntry(Parent, selectedContainer, titleFmg, titleEntry));
                }

                if (fmgEntryGroup.Summary != null)
                {
                    var summaryEntry = fmgEntryGroup.Summary;
                    var summaryFmg = summaryEntry.Parent;

                    actions.Add(new DeleteFmgEntry(Parent, selectedContainer, summaryFmg, summaryEntry));
                }

                if (fmgEntryGroup.Description != null)
                {
                    var descriptionEntry = fmgEntryGroup.Description;
                    var descriptionFmg = descriptionEntry.Parent;

                    actions.Add(new DeleteFmgEntry(Parent, selectedContainer, descriptionFmg, descriptionEntry));
                }

                if (fmgEntryGroup.Effect != null)
                {
                    var effectEntry = fmgEntryGroup.Effect;
                    var effectFmg = effectEntry.Parent;

                    actions.Add(new DeleteFmgEntry(Parent, selectedContainer, effectFmg, effectEntry));
                }
            }
            // Otherwise just duplicate selection
            else
            {
                actions.Add(new DeleteFmgEntry(Parent, selectedContainer, selectedFmg, curEntry));
            }
        }

        return actions;
    }

    /// <summary>
    /// The first ID above startId that's free in every passed FMG and not in claimedIds.
    /// </summary>
    public int GetNextValidId(List<FMG> fmgs, int startId, HashSet<int> claimedIds = null, FmgEntryLookup lookup = null)
    {
        var newID = startId;

        while (true)
        {
            newID++;

            if (claimedIds != null && claimedIds.Contains(newID))
                continue;

            if (IsIdFree(fmgs, newID, lookup: lookup))
                return newID;
        }
    }

    /// <summary>
    /// Copy the currently selected Text Entries to the clipboard.
    /// Runs on the UI thread: it used to run as a background task, reading the
    /// entry list while the UI could change it and setting the clipboard off-thread.
    /// </summary>
    public void CopyEntryTextToClipboard(bool includeID)
    {
        var fmg = Parent.Selection.SelectedFmgWrapper?.File;
        if (fmg == null)
            return;

        // Copy the entries that are selected now, not whatever moved into their old rows
        Parent.Selection.RevalidateEntrySelection();

        var lines = new List<string>();

        for (int i = 0; i < fmg.Entries.Count; i++)
        {
            var entry = fmg.Entries[i];

            if (!Parent.Selection.FmgEntryMultiselect.IsMultiselected(i))
                continue;

            if (!Parent.TextEntryList.IsEntryVisible(entry))
                continue;

            var newText = $"{entry.Text}";

            if (CFG.Current.TextEditor_Text_Clipboard_Escape_New_Lines)
            {
                newText = newText.Replace("\n", "\\n");
            }

            lines.Add(includeID ? $"{entry.ID} {newText}" : newText);
        }

        PlatformUtils.Instance.SetClipboardText(string.Join("\n", lines));

        Smithbox.Log<TextActionHandler>(LOC.Get("TEXT_Tools_Copy_Entry_Text_to_Clipboard_Result"));
    }
}