using System.Collections.Generic;

namespace StudioCore.Editors.TextEditor;

/// <summary>
/// The entries last copied with Copy Entries, shared by every view, so entries can be
/// pasted into another FMG, container or language.
/// </summary>
public static class TextEntryClipboard
{
    public enum GroupSlot
    {
        Title,
        Summary,
        Description,
        Effect
    }

    public class CopiedEntry
    {
        public int ID;
        public string Text;

        /// <summary>
        /// The grouped texts (title, summary, ...) when the entry was copied with grouping on;
        /// null otherwise. A slot is only present if the entry had it.
        /// </summary>
        public Dictionary<GroupSlot, string> Group;
    }

    public static List<CopiedEntry> Entries { get; private set; } = new();

    public static bool HasEntries => Entries.Count > 0;

    public static void Set(List<CopiedEntry> entries)
    {
        Entries = entries ?? new();
    }
}
