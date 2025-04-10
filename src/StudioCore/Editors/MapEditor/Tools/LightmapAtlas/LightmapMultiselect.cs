using SoulsFormats;
using StudioCore.Configuration;
using System.Collections.Generic;

namespace StudioCore.Editors.MapEditor.Tools.LightmapAtlasEditor;

public class LightmapMultiselect
{
    public SortedDictionary<int, BTAB.Entry> StoredEntries = new();

    private LightmapAtlasView Screen;
    public LightmapMultiselect(LightmapAtlasView screen)
    {
        Screen = screen;
    }

    public void Reset()
    {
        Screen.CurrentEntry = null;
        Screen.CurrentEntryKey = -1;
        StoredEntries.Clear();
    }

    public bool IsSelected(int index)
    {
        if (StoredEntries.ContainsKey(index))
            return true;

        return false;
    }

    public bool IsCurrentSelection(int index)
    {
        if (Screen.CurrentEntryKey == index)
            return true;

        return false;
    }

    public void HandleSelection(int currentSelectionIndex, int currentIndex, BTAB.Entry currentEntry)
    {
        // Multi-Select: Range Select
        if (InputTracker.GetKey(Veldrid.Key.LShift))
        {
            int start = currentSelectionIndex;
            int end = currentIndex;

            if (end < start)
            {
                start = currentIndex;
                end = currentSelectionIndex;
            }

            for (int k = start; k <= end; k++)
            {
                if (!StoredEntries.ContainsKey(k))
                    StoredEntries.Add(k, currentEntry);
            }
        }
        // Multi-Select Mode
        else if (InputTracker.GetKey(KeyBindings.Current.MAP_LightmapAtlas_Multiselect))
        {
            if (StoredEntries.ContainsKey(currentIndex) && StoredEntries.Count > 1)
            {
                StoredEntries.Remove(currentIndex);
            }
            else
            {
                if (!StoredEntries.ContainsKey(currentIndex))
                    StoredEntries.Add(currentIndex, currentEntry);
            }
        }
        // Reset Multi-Selection if normal selection occurs
        else
        {
            StoredEntries.Clear();
            StoredEntries.Add(currentIndex, currentEntry);
        }
    }
}
