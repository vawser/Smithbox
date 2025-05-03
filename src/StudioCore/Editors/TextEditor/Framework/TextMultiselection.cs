using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.TextEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace StudioCore.Editors.TextEditor;

public class TextMultiselection
{
    public SortedDictionary<int, FMG.Entry> StoredEntries = new();

    private TextEditorScreen Editor;
    private KeyBind AssociatedKeybind;

    public TextMultiselection(TextEditorScreen screen, KeyBind associatedKeyBind)
    {
        Editor = screen;
        AssociatedKeybind = associatedKeyBind;
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
        if (InputTracker.GetKey(Veldrid.Key.LShift))
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
                    if (Editor.Selection.SelectedFmgWrapper != null && Editor.Selection.SelectedFmgWrapper.File != null)
                    {
                        if (k < Editor.Selection.SelectedFmgWrapper.File.Entries.Count)
                        {
                            var curEntry = Editor.Selection.SelectedFmgWrapper.File.Entries[k];
                            if (Editor.Filters.IsFmgEntryFilterMatch(curEntry))
                            {
                                StoredEntries.Add(k, curEntry);
                            }
                        }
                    }
                }
            }
        }
        // Multi-Select Mode
        else if (InputTracker.GetKey(AssociatedKeybind))
        {
            if (StoredEntries.ContainsKey(currentIndex) && StoredEntries.Count > 1)
            {
                StoredEntries.Remove(currentIndex);
            }
            else
            {
                if (!StoredEntries.ContainsKey(currentIndex))
                {
                    if (Editor.Selection.SelectedFmgWrapper != null &&  Editor.Selection.SelectedFmgWrapper.File != null)
                    {
                        if (currentIndex < Editor.Selection.SelectedFmgWrapper.File.Entries.Count)
                        {
                            var curEntry = Editor.Selection.SelectedFmgWrapper.File.Entries[currentIndex];
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

            if (Editor.Selection.SelectedFmgWrapper != null && Editor.Selection.SelectedFmgWrapper.File != null)
            {
                if (currentIndex < Editor.Selection.SelectedFmgWrapper.File.Entries.Count)
                {
                    var curEntry = Editor.Selection.SelectedFmgWrapper.File.Entries[currentIndex];
                    StoredEntries.Add(currentIndex, curEntry);
                }
            }
        }
    }
}
