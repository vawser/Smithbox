using System;
using System.Collections.Generic;

namespace StudioCore.Editors.TextEditor;

/// <summary>
/// The entries last copied with Copy Entries, shared by every view, so entries can be
/// pasted into another FMG, container or language.
/// </summary>
public static class TextEntryClipboard
{
    public class CopiedEntry
    {
        public int ID;
        public string Text;

        /// <summary>
        /// The grouped texts by slot meaning (e.g. "Title", "Summary", "Lore") when the entry was
        /// copied with grouping on; null otherwise. Paste puts each into the target group's slot
        /// with the same meaning, so groups defined with different slot orders still match.
        /// </summary>
        public Dictionary<string, string> Group;
    }

    public static Dictionary<string, string> NewGroupTexts() => new(StringComparer.OrdinalIgnoreCase);

    public static List<CopiedEntry> Entries { get; private set; } = new();

    public static bool HasEntries => Entries.Count > 0;

    public static void Set(List<CopiedEntry> entries)
    {
        Entries = entries ?? new();
    }
}
