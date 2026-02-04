using SoulsFormats;
using StudioCore.Application;
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

    public void HandleMultiselect(int currentSelectionIndex, int currentIndex)
    {
        // Multi-Select: Range Select
        if (InputManager.HasShiftDown())
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
                            if (Parent.Filters.IsFmgEntryFilterMatch(curEntry))
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
