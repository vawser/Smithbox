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
    /// Add the missing entry of a group slot (with the group's ID and empty text), from the
    /// Contents panel's "Add ... Entry" button. Replaces the four Add Title/Summary/Description/
    /// Effect methods, as a group can now have up to 10 slots of any meaning.
    /// </summary>
    public void AddGroupEntry(FmgEntryGroupSlot slot, FMG.Entry curEntry)
    {
        if (slot?.Wrapper?.File == null || curEntry == null || slot.Entry != null)
            return;

        var selectedContainer = Parent.Selection.SelectedContainerWrapper;
        var newEntry = new FMG.Entry(slot.Wrapper.File, curEntry.ID, "");

        var action = new AddAssociatedEntry(Parent, selectedContainer, slot.Wrapper.File, newEntry);
        Parent.ActionManager.ExecuteAction(action);
    }

    /// <summary>
    /// Duplicate current selection of FMG.Entries
    /// </summary>
    /// <remarks>
    /// No focus check here: the shortcut checks entry-list focus before calling this. Checking it
    /// here too made Edit ▸ Duplicate silently do nothing after clicking another panel.
    /// </remarks>
    public void DuplicateEntries()
    {
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
            // From each entry's own ID, so each copy stays next to its original (100 → 101,
            // 5000 → 5001). It used to continue from the previous copy's ID (5000 → 102).
            // claimedIds keeps copies in the same batch apart (100 and 101 → 102 and 103).
            newId = GetNextValidId(GetGroupFmgs(curEntry, lookup), curEntry.ID, claimedIds, lookup);
            claimedIds.Add(newId);

            // If this is enabled, just use the current entry ID
            if(CFG.Current.TextEditor.TextEditor_Text_Entry_List_Ignore_ID_Check)
            {
                newId = curEntry.ID;
            }

            var actionList = ProcessDuplicate(curEntry, newId, lookup);
            foreach(var action in actionList)
            {
                actions.Add(action);
            }
        }

        if (actions.Count == 0)
            return;

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
                if (!CFG.Current.TextEditor.TextEditor_Text_Entry_Allow_Duplicate_ID)
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
    /// The FMGs a duplicate or ID change of this entry writes to: its own FMG, plus every
    /// FMG of its group (up to 10 slots) when the entry is grouped.
    /// </summary>
    public List<FMG> GetGroupFmgs(FMG.Entry entry, FmgEntryLookup lookup = null)
    {
        var fmgs = new List<FMG>();

        if (entry?.Parent != null)
            fmgs.Add(entry.Parent);

        var group = Parent.EntryGroupManager.GetEntryGroup(entry, lookup);

        if (group != null && group.SupportsGrouping && CFG.Current.TextEditor.TextEditor_Text_Entry_Enable_Grouped_Entries)
        {
            foreach (var slot in group.Slots)
            {
                if (!fmgs.Contains(slot.Wrapper.File))
                    fmgs.Add(slot.Wrapper.File);
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
        var fmgEntryGroup = Parent.EntryGroupManager.GetEntryGroup(curEntry, lookup);

        if (fmgEntryGroup == null || newId == -1)
            return actions;

        // Duplicate every entry of the group together (each slot that has one)
        if (fmgEntryGroup.SupportsGrouping && CFG.Current.TextEditor.TextEditor_Text_Entry_Enable_Grouped_Entries)
        {
            foreach (var slot in fmgEntryGroup.Slots)
            {
                if (slot.Entry != null)
                {
                    actions.Add(new DuplicateFmgEntry(Parent, selectedContainer, slot.Wrapper.File, slot.Entry, newId));
                }
            }
        }
        // Otherwise just duplicate selection
        else
        {
            actions.Add(new DuplicateFmgEntry(Parent, selectedContainer, curEntry.Parent, curEntry, newId));
        }

        return actions;
    }

    /// <summary>
    /// Delete current selection of FMG.Entries.
    /// Deleting two or more selected entries asks for confirmation first
    /// (single deletes stay instant; Ctrl+Z covers mistakes).
    /// </summary>
    /// <remarks>
    /// No focus check here, for the same reason as DuplicateEntries: the Delete shortcut checks it.
    /// </remarks>
    public void DeleteEntries()
    {
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
        var fmgEntryGroup = Parent.EntryGroupManager.GetEntryGroup(curEntry, lookup);

        if (fmgEntryGroup == null)
            return actions;

        // Delete every entry of the group together (each slot that has one)
        if (fmgEntryGroup.SupportsGrouping && CFG.Current.TextEditor.TextEditor_Text_Entry_Enable_Grouped_Entries)
        {
            foreach (var slot in fmgEntryGroup.Slots)
            {
                if (slot.Entry != null)
                {
                    actions.Add(new DeleteFmgEntry(Parent, selectedContainer, slot.Wrapper.File, slot.Entry));
                }
            }
        }
        // Otherwise just delete the selection
        else
        {
            actions.Add(new DeleteFmgEntry(Parent, selectedContainer, curEntry.Parent, curEntry));
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

    #region Copy / Paste Entries

    /// <summary>
    /// Copy the selected entries for Paste (in this or another FMG, container or language).
    /// With grouping on, the texts of each entry's group slots come along, by slot meaning.
    /// The text also goes to the system clipboard, as Copy Text does.
    /// </summary>
    public void CopyEntries()
    {
        var entries = GetSelectedListedEntries();

        if (entries.Count == 0)
            return;

        var groupingOn = CFG.Current.TextEditor.TextEditor_Text_Entry_Enable_Grouped_Entries;
        var lookup = new FmgEntryLookup();
        var copied = new List<TextEntryClipboard.CopiedEntry>();

        foreach (var entry in entries)
        {
            var item = new TextEntryClipboard.CopiedEntry { ID = entry.ID, Text = entry.Text };

            var group = groupingOn ? Parent.EntryGroupManager.GetEntryGroup(entry, lookup) : null;

            if (group != null && group.SupportsGrouping)
            {
                item.Group = TextEntryClipboard.NewGroupTexts();

                foreach (var slot in group.Slots)
                {
                    if (slot.Entry != null)
                        item.Group.TryAdd(slot.Meaning, slot.Entry.Text);
                }
            }

            copied.Add(item);
        }

        TextEntryClipboard.Set(copied);

        CopyEntryTextToClipboard(CFG.Current.TextEditor.TextEditor_Text_Clipboard_Include_ID);

        Smithbox.Log(this, LOC.Get("TEXT_EntryClipboard_Copied", copied.Count));
    }

    /// <summary>
    /// Paste the copied entries into the selected FMG.
    /// asNew false: each entry is written at its own ID, changing the text of an existing
    /// entry and adding a missing one. asNew true: each is added at the next free ID after its own.
    /// Grouped copies go into the matching associated FMGs when the target FMG has them.
    /// </summary>
    public void PasteEntries(bool asNew)
    {
        var fmgWrapper = Parent.Selection.SelectedFmgWrapper;
        var container = Parent.Selection.SelectedContainerWrapper;

        if (fmgWrapper?.File == null || container == null || !TextEntryClipboard.HasEntries)
            return;

        // The target's group slots by meaning, when grouping is on and the FMG is grouped
        var slotWrappers = new Dictionary<string, TextFmgWrapper>(StringComparer.OrdinalIgnoreCase);

        if (CFG.Current.TextEditor.TextEditor_Text_Entry_Enable_Grouped_Entries)
        {
            foreach (var (slot, wrapper) in Parent.EntryGroupManager.GetGroupWrappers(fmgWrapper.ID))
            {
                if (wrapper?.File != null)
                    slotWrappers.TryAdd(slot.Meaning, wrapper);
            }
        }

        var lookup = new FmgEntryLookup();
        var claimedIds = new HashSet<int>();

        // Entries this paste adds, which the lookup (built before any action runs) can't see
        var pendingAdds = new HashSet<(FMG, int)>();

        var actions = new List<EditorAction>();
        var added = 0;
        var changed = 0;

        FMG.Entry revealEntry = null;

        foreach (var item in TextEntryClipboard.Entries)
        {
            var writes = new List<(FMG Fmg, string Text)>();

            if (slotWrappers.Count > 0 && item.Group != null)
            {
                foreach (var (slot, text) in item.Group)
                {
                    if (slotWrappers.TryGetValue(slot, out var wrapper))
                        writes.Add((wrapper.File, text));
                }
            }
            else
            {
                writes.Add((fmgWrapper.File, item.Text));
            }

            if (writes.Count == 0)
                continue;

            var id = item.ID;

            if (asNew)
            {
                var fmgs = writes.Select(w => w.Fmg).Distinct().ToList();
                id = GetNextValidId(fmgs, item.ID, claimedIds, lookup);
                claimedIds.Add(id);
            }

            foreach (var (fmg, text) in writes)
            {
                if (pendingAdds.Contains((fmg, id)))
                    continue;

                var existing = asNew ? null : lookup.Find(fmg, id);

                if (existing != null)
                {
                    if (existing.Text == text)
                        continue;

                    actions.Add(new ChangeFmgEntryText(Parent, container, existing, text));
                    changed++;

                    if (revealEntry == null && fmg == fmgWrapper.File)
                        revealEntry = existing;
                }
                else
                {
                    var newEntry = new FMG.Entry(fmg, id, text);

                    actions.Add(new AddFmgEntry(Parent, container, newEntry, newEntry, id));
                    pendingAdds.Add((fmg, id));
                    added++;

                    if (revealEntry == null && fmg == fmgWrapper.File)
                        revealEntry = newEntry;
                }
            }
        }

        if (actions.Count == 0)
        {
            // Pasting into the FMG the entries came from changes nothing
            Smithbox.Log(this, LOC.Get("TEXT_EntryClipboard_Nothing_To_Paste"));
            return;
        }

        Parent.ActionManager.ExecuteAction(new FmgGroupedAction(actions));

        if (revealEntry != null)
        {
            Parent.Selection.RevealEntry(fmgWrapper.File.Entries.IndexOf(revealEntry), revealEntry);
        }

        Smithbox.Log(this, LOC.Get("TEXT_EntryClipboard_Pasted", added, changed));
    }

    #endregion

    #region Vanilla

    /// <summary>
    /// Set the selected modified entries back to their vanilla text. Entries unique to the mod
    /// have no vanilla text and are left alone.
    /// </summary>
    public void RevertToVanilla()
    {
        if (!CFG.Current.TextEditor.TextEditor_Include_Vanilla_Cache)
        {
            Smithbox.LogWarning(this, LOC.Get("TEXT_Vanilla_Cache_Disabled"));
            return;
        }

        var diff = Parent.DifferenceManager;
        var container = Parent.Selection.SelectedContainerWrapper;
        var actions = new List<EditorAction>();

        foreach (var entry in GetSelectedListedEntries())
        {
            if (!diff.IsDifferentToVanilla(entry) || diff.IsUniqueToProject(entry))
                continue;

            if (!diff.TryGetVanillaText(entry, out var vanillaText))
                continue;

            actions.Add(new ChangeFmgEntryText(Parent, container, entry, vanillaText));
        }

        if (actions.Count > 0)
        {
            Parent.ActionManager.ExecuteAction(new FmgGroupedAction(actions));
        }

        Smithbox.Log(this, LOC.Get("TEXT_Vanilla_Reverted", actions.Count));
    }

    /// <summary>
    /// Add vanilla entries that the mod has deleted back into the selected FMG, with their vanilla text
    /// </summary>
    public void RestoreDeletedEntries(IEnumerable<FMG.Entry> vanillaEntries)
    {
        var fmg = Parent.Selection.SelectedFmgWrapper?.File;
        var container = Parent.Selection.SelectedContainerWrapper;

        if (fmg == null || container == null)
            return;

        var existingIds = fmg.Entries.Select(e => e.ID).ToHashSet();
        var actions = new List<EditorAction>();

        foreach (var vanillaEntry in vanillaEntries)
        {
            // Already back (or added again since the list was built)
            if (!existingIds.Add(vanillaEntry.ID))
                continue;

            var newEntry = new FMG.Entry(fmg, vanillaEntry.ID, vanillaEntry.Text);
            actions.Add(new AddFmgEntry(Parent, container, newEntry, newEntry, vanillaEntry.ID));
        }

        if (actions.Count == 0)
            return;

        Parent.ActionManager.ExecuteAction(new FmgGroupedAction(actions));

        Smithbox.Log(this, LOC.Get("TEXT_Vanilla_Restored", actions.Count));
    }

    #endregion

    /// <summary>
    /// The selected entries of the selected FMG that are listed (pass the filter), in list order.
    /// Shared by Edit Selection, Copy Entries and Revert to Vanilla, so none of them reach rows
    /// the user can't see.
    /// </summary>
    public List<FMG.Entry> GetSelectedListedEntries()
    {
        var result = new List<FMG.Entry>();

        if (Parent.Selection.SelectedFmgWrapper?.File == null)
            return result;

        Parent.Selection.RevalidateEntrySelection();

        foreach (var entry in Parent.Selection.FmgEntryMultiselect.StoredEntries.Values)
        {
            if (Parent.TextEntryList.IsEntryVisible(entry))
                result.Add(entry);
        }

        return result;
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

            if (CFG.Current.TextEditor.TextEditor_Text_Clipboard_Escape_New_Lines)
            {
                newText = newText.Replace("\n", "\\n");
            }

            lines.Add(includeID ? $"{entry.ID} {newText}" : newText);
        }

        PlatformUtils.Instance.SetClipboardText(string.Join("\n", lines));

        Smithbox.Log<TextActionHandler>(LOC.Get("TEXT_Tools_Copy_Entry_Text_to_Clipboard_Result"));
    }
}