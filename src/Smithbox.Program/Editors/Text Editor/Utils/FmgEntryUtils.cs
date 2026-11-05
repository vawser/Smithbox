using SoulsFormats;
using StudioCore.Editors.Common;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.TextEditor;

public static class FmgEntryUtils
{
    /// <summary>
    /// The index at which an entry with the passed ID keeps the list in ID order:
    /// after any entries with the same ID, otherwise at the end.
    /// Worked out at execute time by the add/duplicate actions, because the list can be
    /// reordered between building and running an action (FMG.Write sorts it in place on save).
    /// </summary>
    public static int GetSortedInsertionIndex(FMG fmg, int id)
    {
        for (int i = 0; i < fmg.Entries.Count; i++)
        {
            if (fmg.Entries[i].ID > id)
                return i;
        }

        return fmg.Entries.Count;
    }

}
