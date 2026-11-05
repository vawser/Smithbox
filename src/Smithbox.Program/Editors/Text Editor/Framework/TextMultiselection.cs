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

    public void HandleMultiselect(int currentSelectionIndex, int currentIndex)
    {
        // Multi-Select: Range Select
        // Needs an anchor: with no current selection (-1), treat it as a normal click
        // rather than starting the range at Entries[-1].
        if (InputManager.HasShiftDown() && currentSelectionIndex >= 0)
        {
            var start = currentSelectionIndex;
            var end = currentIndex;

            if (end < start)
            {
                start = currentIndex;
                end = currentSelectionIndex;
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
