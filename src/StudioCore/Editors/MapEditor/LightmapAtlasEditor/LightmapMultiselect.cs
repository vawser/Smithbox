using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Editors.TimeActEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.LightmapAtlasEditor;

public class LightmapMultiselect
{
    public SortedDictionary<int, BTAB.Entry> StoredLightmapEntries = new();

    private LightmapAtlasScreen Screen;
    public LightmapMultiselect(LightmapAtlasScreen screen)
    {
        Screen = screen;
    }

    public void Reset()
    {
        StoredLightmapEntries.Clear();
    }

    public bool IsLightmapSelected(int index)
    {
        if (StoredLightmapEntries.ContainsKey(index))
            return true;

        return false;
    }

    public void LightMapSelect(int currentSelectionIndex, int currentIndex)
    {
        var lightmap = Screen._selectedEntry;

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
                if (!StoredLightmapEntries.ContainsKey(k))
                    StoredLightmapEntries.Add(k, lightmap);
            }
        }
        // Multi-Select Mode
        else if (InputTracker.GetKey(KeyBindings.Current.MAP_LightmapAtlas_Multiselect))
        {
            if (StoredLightmapEntries.ContainsKey(currentIndex) && StoredLightmapEntries.Count > 1)
            {
                StoredLightmapEntries.Remove(currentIndex);
            }
            else
            {
                if (!StoredLightmapEntries.ContainsKey(currentIndex))
                    StoredLightmapEntries.Add(currentIndex, lightmap);
            }
        }
        // Reset Multi-Selection if normal selection occurs
        else
        {
            StoredLightmapEntries.Clear();
            StoredLightmapEntries.Add(currentIndex, lightmap);
        }
    }
}
