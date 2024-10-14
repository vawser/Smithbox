using global::StudioCore.Configuration;
using SoulsFormats;
using StudioCore.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace StudioCore.Editors.TextEditor;

public class TextMultiselection
{
    public SortedDictionary<int, FMG.Entry> StoredEntries = new();

    private KeyBind AssociatedKeybind;

    public TextMultiselection(KeyBind associatedKeyBind)
    {
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

    public void HandleMultiselect(int currentSelectionIndex, int currentIndex, FMG.Entry entry)
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
                    StoredEntries.Add(k, entry);
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
                    StoredEntries.Add(currentIndex, entry);
            }
        }
        // Reset Multi-Selection if normal selection occurs
        else
        {
            StoredEntries.Clear();
            StoredEntries.Add(currentIndex, entry);
        }
    }
}
