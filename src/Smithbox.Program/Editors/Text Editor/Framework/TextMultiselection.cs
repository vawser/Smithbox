using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using System.Collections.Generic;

namespace StudioCore.Editors.TextEditor;

public class TextMultiselection
{
    private TextEditorView Parent;

    public SortedDictionary<int, FMG.Entry> StoredEntries = new();

    public TextMultiselection(TextEditorView view)
    {
        Parent = view;
    }

    private void SetAnchor(List<FMG.Entry> entries, int index)
    {
        if (entries != null && index >= 0 && index < entries.Count)
        {
            AnchorEntry = entries[index];
        }
    }

    public bool HasValidMultiselection()
    {
        if (StoredEntries.Count < 1)
        {
            return false;
        }

        return true;
    }

    public bool IsMultiselected(int index)
    {
        return StoredEntries.ContainsKey(index);
    }

    /// <summary>
    /// Re-key the stored entries by their current index in the FMG, dropping any that
    /// are no longer in it. Indices drift when entries are added, deleted, reordered,
    /// undone, or re-sorted by a save, which made the highlight and Copy Text
    /// act on different rows than the ones originally selected.
    /// </summary>
    public void Revalidate(FMG fmg)
    {
        if (fmg == null)
        {
            StoredEntries.Clear();
            return;
        }

        var isValid = true;

        foreach (var (index, entry) in StoredEntries)
        {
            if (index < 0 || index >= fmg.Entries.Count || fmg.Entries[index] != entry)
            {
                isValid = false;
                break;
            }
        }

        if (isValid)
            return;

        var rebuilt = new SortedDictionary<int, FMG.Entry>();

        foreach (var entry in StoredEntries.Values)
        {
            var newIndex = fmg.Entries.IndexOf(entry);

            if (newIndex != -1)
            {
                rebuilt[newIndex] = entry;
            }
        }

        StoredEntries = rebuilt;
    }

    /// <summary>
    /// Where Shift ranges start: the row last clicked without Shift. Kept by reference, as
    /// indices drift. It used to be the last clicked row, Shift-clicks included, so a second
    /// Shift-click ranged from the first one's end instead of from the original row.
    /// </summary>
    public FMG.Entry AnchorEntry;

    public void HandleMultiselect(int currentSelectionIndex, int currentIndex)
    {
        var entries = Parent.Selection.SelectedFmgWrapper?.File?.Entries;

        var anchorIndex = AnchorEntry != null && entries != null ? entries.IndexOf(AnchorEntry) : -1;

        // No anchor yet (or its entry is gone): range from the current selection
        if (anchorIndex == -1)
            anchorIndex = currentSelectionIndex;

        // Multi-Select: Range Select
        // Needs an anchor: with no current selection (-1), treat it as a normal click
        // rather than starting the range at Entries[-1].
        if (InputManager.HasShiftDown() && anchorIndex >= 0)
        {
            // Shift: the range replaces the selection, as in other list views.
            // Ctrl+Shift: the range is added to it.
            if (!InputManager.HasCtrlDown())
            {
                StoredEntries.Clear();
            }

            var start = anchorIndex;
            var end = currentIndex;

            if (end < start)
            {
                start = currentIndex;
                end = anchorIndex;
            }

            for (int k = start; k <= end; k++)
            {
                if (!StoredEntries.ContainsKey(k))
                {
                    if (Parent.Selection.SelectedFmgWrapper != null && Parent.Selection.SelectedFmgWrapper.File != null)
                    {
                        if (k < Parent.Selection.SelectedFmgWrapper.File.Entries.Count)
                        {
                            var curEntry = Parent.Selection.SelectedFmgWrapper.File.Entries[k];

                            // Only rows that are actually listed (search filter and null-entry toggle)
                            if (Parent.TextEntryList.IsEntryVisible(curEntry))
                            {
                                StoredEntries.Add(k, curEntry);
                            }
                        }
                    }
                }
            }
        }
        // Multi-Select Mode
        else if (InputManager.HasCtrlDown())
        {
            SetAnchor(entries, currentIndex);

            if (StoredEntries.ContainsKey(currentIndex) && StoredEntries.Count > 1)
            {
                StoredEntries.Remove(currentIndex);
            }
            else
            {
                if (!StoredEntries.ContainsKey(currentIndex))
                {
                    if (Parent.Selection.SelectedFmgWrapper != null && Parent.Selection.SelectedFmgWrapper.File != null)
                    {
                        if (currentIndex < Parent.Selection.SelectedFmgWrapper.File.Entries.Count)
                        {
                            var curEntry = Parent.Selection.SelectedFmgWrapper.File.Entries[currentIndex];
                            StoredEntries.Add(currentIndex, curEntry);
                        }
                    }
                }
            }
        }
        // Reset Multi-Selection if normal selection occurs
        else
        {
            SetAnchor(entries, currentIndex);

            StoredEntries.Clear();

            if (Parent.Selection.SelectedFmgWrapper != null && Parent.Selection.SelectedFmgWrapper.File != null)
            {
                if (currentIndex < Parent.Selection.SelectedFmgWrapper.File.Entries.Count)
                {
                    var curEntry = Parent.Selection.SelectedFmgWrapper.File.Entries[currentIndex];
                    StoredEntries.Add(currentIndex, curEntry);
                }
            }
        }
    }
}
