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

        SortedDictionary<int, FMG.Entry> storedEntries = Parent.Selection.FmgEntryMultiselect.StoredEntries;

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

            // If this is enabled, just use the current entry ID
            if(CFG.Current.TextEditor_Text_Entry_List_Ignore_ID_Check)
            {
                newId = curEntry.ID;
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
        Parent.ActionManager.ExecuteAction(groupAction);
    }

    public void DuplicateEntriesPopup(int offset, int amount, bool autoAdjust)
    {
        if (!FocusManager.IsFocus(EditorFocusContext.TextEditor_EntryList))
            return;

        if (Parent.Selection._selectedFmgEntry == null)
            return;

        if (Parent.Selection._selectedFmgEntryIndex == -1)
            return;

        SortedDictionary<int, FMG.Entry> storedEntries = Parent.Selection.FmgEntryMultiselect.StoredEntries;

        List<EditorAction> actions = new List<EditorAction>();

        foreach (var entry in storedEntries)
        {
            var curFmg = entry.Value.Parent;
            var curEntry = entry.Value;

            for (int i = 0; i < amount; i++)
            {
                var newId = curEntry.ID + offset;

                if (autoAdjust)
                {
                    newId = curEntry.ID + (offset * (i + 1));
                }

                var actionList = ProcessDuplicate(curEntry, newId);
                foreach (var action in actionList)
                {
                    actions.Add(action);
                }
            }
        }

        // Reverse so ID order makes sense
        actions.Reverse();

        var groupAction = new FmgGroupedAction(actions);
        Parent.ActionManager.ExecuteAction(groupAction);
    }

    private List<EditorAction> ProcessDuplicate(FMG.Entry curEntry, int newId)
    {
        List<EditorAction> actions = new List<EditorAction>();

        var selectedContainer = Parent.Selection.SelectedContainerWrapper;
        var selectedFmg = curEntry.Parent;
        var fmgEntryGroup = Parent.EntryGroupManager.GetEntryGroup(curEntry);

        if (newId != -1)
        {
            // Duplicate all related entries together based on selection
            if (fmgEntryGroup.SupportsGrouping)
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

        return actions;
    }

    /// <summary>
    /// Delete current selection of FMG.Entries
    /// </summary>
    public void DeleteEntries()
    {
        if (!FocusManager.IsFocus(EditorFocusContext.TextEditor_EntryList))
            return;

        if (Parent.Selection._selectedFmgEntry == null)
            return;

        if (Parent.Selection._selectedFmgEntryIndex == -1)
            return;

        if (Parent.Selection.FmgEntryMultiselect.StoredEntries.Count <= 0)
            return;

        SortedDictionary<int, FMG.Entry> storedEntries = Parent.Selection.FmgEntryMultiselect.StoredEntries;

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
        Parent.ActionManager.ExecuteAction(groupAction);

        Parent.Selection.FmgEntryMultiselect.StoredEntries.Clear();
    }

    private List<EditorAction> ProcessDelete(FMG.Entry curEntry)
    {
        List<EditorAction> actions = new List<EditorAction>();

        var selectedContainer = Parent.Selection.SelectedContainerWrapper;
        var selectedFmg = curEntry.Parent;
        var fmgEntryGroup = Parent.EntryGroupManager.GetEntryGroup(curEntry);

        // Delete all related entries together based on selection
        if (fmgEntryGroup.SupportsGrouping)
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

    private bool IsCurrentlyCopyingContents = false;

    /// <summary>
    /// Copy the currently selected Text Entries to the clipboard
    /// </summary>
    public void CopyEntryTextToClipboard(bool includeID)
    {
        if (!IsCurrentlyCopyingContents)
        {
            IsCurrentlyCopyingContents = true;

            TaskManager.LiveTask task = new(
                "textEditor_copyTextEntryContents",
                "Text Editor",
                "selected text entry has been copied to the clipboard.",
                "clipboard copy failed.",
                TaskManager.RequeueType.None,
                false,
                () =>
                {
                    var AlterCopyTextAssignment = false;
                    var copyText = "";

                    if (Parent.Selection.SelectedFmgWrapper != null && Parent.Selection.SelectedFmgWrapper.File != null)
                    {
                        for (int i = 0; i < Parent.Selection.SelectedFmgWrapper.File.Entries.Count; i++)
                        {
                            var entry = Parent.Selection.SelectedFmgWrapper.File.Entries[i];

                            if (Parent.Filters.IsFmgEntryFilterMatch(entry))
                            {
                                if (Parent.Selection.FmgEntryMultiselect.IsMultiselected(i))
                                {
                                    var newText = $"{entry.Text}";

                                    if (CFG.Current.TextEditor_Text_Clipboard_Escape_New_Lines)
                                    {
                                        newText = $"{entry.Text}".Replace("\n", "\\n");
                                    }

                                    if (AlterCopyTextAssignment)
                                    {
                                        if (includeID)
                                        {
                                            copyText = $"{copyText}\n{entry.ID} {newText}";
                                        }
                                        else
                                        {
                                            copyText = $"{copyText}\n{newText}";
                                        }
                                    }
                                    else
                                    {
                                        AlterCopyTextAssignment = true;

                                        if (includeID)
                                        {
                                            copyText = $"{entry.ID} {newText}";
                                        }
                                        else
                                        {
                                            copyText = $"{newText}";
                                        }
                                    }
                                }
                            }
                        }
                    }

                    PlatformUtils.Instance.SetClipboardText(copyText);
                    PlatformUtils.Instance.MessageBox("Text Entry Contents copied to clipboard", "Clipboard", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    IsCurrentlyCopyingContents = false;
                }
            );

            TaskManager.Run(task);
        }
    }
}