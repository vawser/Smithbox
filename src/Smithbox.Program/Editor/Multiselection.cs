using StudioCore.Configuration;
using System.Collections.Generic;

namespace StudioCore.Editor.Multiselection;

public class Multiselection
{
    public List<int> StoredIndices = new List<int>();

    private KeyBind AssociatedKeybind;

    public Multiselection(KeyBind associatedKeyBind)
    {
        AssociatedKeybind = associatedKeyBind;
    }

    public bool HasValidMultiselection()
    {
        if (StoredIndices.Count < 1)
        {
            return false;
        }

        return true;
    }

    public bool IsMultiselected(int index)
    {
        return StoredIndices.Contains(index);
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
                if (StoredIndices.Contains(k))
                {
                    StoredIndices.Remove(k);
                }
                else
                {
                    StoredIndices.Add(k);
                }
            }
        }
        // Multi-Select Mode
        else if (InputTracker.GetKey(AssociatedKeybind))
        {
            if (StoredIndices.Contains(currentIndex))
            {
                StoredIndices.Remove(currentIndex);
            }
            else
            {
                StoredIndices.Add(currentIndex);
            }
        }
        // Reset Multi-Selection if normal selection occurs
        else
        {
            StoredIndices = new List<int>();
        }
    }
}
