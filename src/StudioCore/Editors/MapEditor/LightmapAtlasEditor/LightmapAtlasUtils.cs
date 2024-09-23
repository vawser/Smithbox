using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.LightmapAtlasEditor;

public static class LightmapAtlasUtils
{
    public static void DuplicateEntries(MapEditorScreen Screen, LightmapAtlasScreen AtlasScreen)
    {
        if (AtlasScreen.CurrentParent == null)
            return;

        if (AtlasScreen.CurrentEntry == null)
            return;

        if (AtlasScreen.CurrentEntryKey == -1)
            return;

        AtlasScreen.CurrentParent.IsModified = true;

        SortedDictionary<int, BTAB.Entry> storedEntries = AtlasScreen.LightmapMultiselect.StoredEntries;
        var parent = AtlasScreen.CurrentParent;

        int lastIdx = -1;

        // Single
        if (storedEntries.Count <= 1)
        {
            BTAB.Entry curEntry = parent.LightmapAtlas.Entries[storedEntries.First().Key];
            int insertIdx = parent.LightmapAtlas.Entries.IndexOf(curEntry);
            BTAB.Entry dupeEntry = curEntry.GetClone();

            Screen.EditorActionManager.ExecuteAction(new BtabEntryDuplicate(dupeEntry, parent.LightmapAtlas.Entries, insertIdx));
        }
        // Multi-Select
        else
        {
            List<int> insertIndices = new List<int>();
            List<BTAB.Entry> newEntries = new List<BTAB.Entry>();

            for (int i = 0; i < parent.LightmapAtlas.Entries.Count; i++)
            {
                if (storedEntries.ContainsKey(i))
                {
                    BTAB.Entry curEntry = parent.LightmapAtlas.Entries[i];
                    int insertIdx = parent.LightmapAtlas.Entries.IndexOf(curEntry);
                    insertIndices.Add(insertIdx);
                    BTAB.Entry dupeEvent = curEntry.GetClone();
                    newEntries.Add(dupeEvent);

                    lastIdx = insertIdx;
                }
            }

            Screen.EditorActionManager.ExecuteAction(new BtabEntryMultiDuplicate(newEntries, parent.LightmapAtlas.Entries, insertIndices));

            // Select last newly duplicated event
            if (lastIdx != -1)
            {
                LightmapAtlasUtils.SelectNewEntry(lastIdx);
            }
        }
    }
    public static void DeleteEntries(MapEditorScreen Screen, LightmapAtlasScreen AtlasScreen)
    {
        if (AtlasScreen.CurrentParent == null)
            return;

        if (AtlasScreen.CurrentEntry == null)
            return;

        if (AtlasScreen.CurrentEntryKey == -1)
            return;

        AtlasScreen.CurrentParent.IsModified = true;

        SortedDictionary<int, BTAB.Entry> storedEntries = AtlasScreen.LightmapMultiselect.StoredEntries;
        var parent = AtlasScreen.CurrentParent;

        // Single
        if (storedEntries.Count <= 1)
        {
            BTAB.Entry curEntry = parent.LightmapAtlas.Entries[storedEntries.First().Key];
            int removeIdx = parent.LightmapAtlas.Entries.IndexOf(curEntry);
            BTAB.Entry storedEntry = curEntry.GetClone();

            Screen.EditorActionManager.ExecuteAction(new BtabEntryDelete(storedEntry, parent.LightmapAtlas.Entries, removeIdx));
        }
        // Multi-Select
        else
        {
            List<int> removeIndices = new List<int>();
            List<BTAB.Entry> removedEntries = new List<BTAB.Entry>();

            for (int i = 0; i < parent.LightmapAtlas.Entries.Count; i++)
            {
                if (storedEntries.ContainsKey(i))
                {
                    BTAB.Entry curEntry = parent.LightmapAtlas.Entries[i];
                    int removeIdx = parent.LightmapAtlas.Entries.IndexOf(curEntry);
                    BTAB.Entry storedEntry = curEntry.GetClone();

                    removeIndices.Add(removeIdx);
                    removedEntries.Add(storedEntry);
                }
            }

            Screen.EditorActionManager.ExecuteAction(new BtabEntryMultiDelete(removedEntries, parent.LightmapAtlas.Entries, removeIndices));
        }

        AtlasScreen.LightmapMultiselect.Reset();
    }

    public static void SelectNewEntry(int targetIndex)
    {
        var handler = Smithbox.EditorHandler.MapEditor.LightmapAtlasEditor;
        handler.LightmapMultiselect.StoredEntries.Clear();

        for (int i = 0; i < handler.CurrentParent.LightmapAtlas.Entries.Count; i++)
        {
            var curEntry = handler.CurrentEntry;

            if (i == targetIndex)
            {
                handler.CurrentEntry = curEntry;
                handler.CurrentEntryKey = i;
                handler.LightmapMultiselect.StoredEntries.Add(i, handler.CurrentEntry);
                break;
            }
        }
    }
}
