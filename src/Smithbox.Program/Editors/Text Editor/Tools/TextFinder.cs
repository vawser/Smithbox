using SoulsFormats;
using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.TextEditor;

public static class TextFinder
{
    private static Dictionary<string, TextResult> CachedResults = new();

    private static Dictionary<string, TextResult> CachedGlobalResults = new();

    /// <summary>
    /// Bumped whenever text data changes (any text action, via TextContainerWrapper.MarkDirty,
    /// and any FMG load). The cache used to live forever, so the Param Editor kept showing
    /// deleted entries, entries whose ID had changed, and text from a previous project.
    /// </summary>
    private static int DataVersion;
    private static int CachedVersion = -1;

    public static void InvalidateCache()
    {
        System.Threading.Interlocked.Increment(ref DataVersion);
    }

    /// <summary>
    /// Changes whenever text data changes; other per-frame caches can key on it.
    /// </summary>
    public static int CurrentDataVersion => System.Threading.Volatile.Read(ref DataVersion);

    /// <summary>
    /// Get FMG reference, caching it on initial search, and then accessing cache on future usage
    /// </summary>
    public static TextResult GetTextResult(TextEditorView view, string fmgName, int value, int offset = 0)
    {
        var currentVersion = System.Threading.Volatile.Read(ref DataVersion);

        if (CachedVersion != currentVersion)
        {
            CachedResults.Clear();
            CachedVersion = currentVersion;
        }

        // Include the project and primary language, so switching either doesn't return stale text
        var cacheName = $"{view.Project.GetHashCode()}|{CFG.Current.TextEditor_Primary_Category}|{fmgName}|{value}|{offset}";

        // Misses are cached too (as null): every missing reference used to rescan
        // every primary container, every frame.
        if (CachedResults.TryGetValue(cacheName, out var cached))
        {
            return cached;
        }

        var found = FindTextResult(view, fmgName, value, offset);
        CachedResults[cacheName] = found;

        return found;
    }

    private static TextResult FindTextResult(TextEditorView view, string fmgName, int value, int offset)
    {
        // Search the containers the game reads first (e.g. ER item_dlc02), and only fall
        // back to the unused ones (ER item / item_dlc01) if nothing matched. The bank has
        // no defined order, so a single pass could return text from an obsolete container.
        return FindTextResult(view, fmgName, value, offset, searchUnused: false)
            ?? FindTextResult(view, fmgName, value, offset, searchUnused: true);
    }

    private static TextResult FindTextResult(TextEditorView view, string fmgName, int value, int offset, bool searchUnused)
    {
        foreach(var (fileEntry, entry) in view.Project.Handler.TextData.PrimaryBank.Containers)
        {
            var containerName = fileEntry.Filename;

            if (entry.IsContainerUnused() != searchUnused)
                continue;

            if (entry.FmgWrappers == null)
                continue;

            if (entry.ContainerDisplayCategory == CFG.Current.TextEditor_Primary_Category)
            {
                foreach (var fmg in entry.FmgWrappers)
                {
                    if (view.Project.Descriptor.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
                    {
                        if(containerName == fmgName)
                        {
                            foreach (var fmgEntry in fmg.File.Entries)
                            {
                                var entryId = Math.Abs(value) + offset;

                                if (fmgEntry.ID == entryId)
                                {
                                    TextResult result = new();
                                    result.ContainerName = containerName;
                                    result.ContainerWrapper = entry;
                                    result.FmgID = fmg.ID;
                                    result.FmgName = fmg.Name;
                                    result.Fmg = fmg.File;
                                    result.FmgEntryID = fmgEntry.ID;
                                    result.Entry = fmgEntry;

                                    return result;
                                }
                            }
                        }
                    }
                    else
                    {
                        var enumName = TextUtils.GetFmgInternalName(view.Project, entry, fmg.ID, fmg.Name);

                        // Contains here to capture the _DLC, _DLC1 and _DLC2 fmgs
                        if (enumName.Contains(fmgName))
                        {
                            foreach (var fmgEntry in fmg.File.Entries)
                            {
                                var entryId = Math.Abs(value) + offset;

                                if (fmgEntry.ID == entryId)
                                {
                                    TextResult result = new();
                                    result.ContainerName = containerName;
                                    result.ContainerWrapper = entry;
                                    result.FmgID = fmg.ID;
                                    result.FmgName = fmg.Name;
                                    result.Fmg = fmg.File;
                                    result.FmgEntryID = fmgEntry.ID;
                                    result.Entry = fmgEntry;

                                    return result;
                                }
                            }
                        }
                    }
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Get text result for global search.
    /// </summary>
    public static List<TextResult> GetGlobalTextResult(TextEditorView view, string searchTerm, SearchFilterType searchFilterType, SearchMatchType matchType, bool ignoreCase)
    {
        var results = new List<TextResult>();

        foreach (var (fileEntry, entry) in view.Project.Handler.TextData.PrimaryBank.Containers)
        {
            var containerName = fileEntry.Filename;

            if (searchFilterType is SearchFilterType.PrimaryCategory)
            {
                if (entry.ContainerDisplayCategory != CFG.Current.TextEditor_Primary_Category)
                {
                    continue;
                }
            }

            // Languages that haven't been opened yet have no FMGs loaded; load them so
            // "All categories" actually searches them (Global Replace already did this).
            if (entry.FmgWrappers == null || entry.FmgWrappers.Count == 0)
            {
                view.Project.Handler.TextData.PrimaryBank.LoadFmgWrappers(entry);
            }

            if (entry.FmgWrappers == null)
                continue;

            foreach (var fmg in entry.FmgWrappers)
            {
                foreach (var fmgEntry in fmg.File.Entries)
                {
                    var entryText = fmgEntry.Text;
                    var searchText = searchTerm;

                    if (entryText != null)
                    {
                        if (ignoreCase)
                        {
                            entryText = entryText.ToLower();
                            searchText = searchText.ToLower();
                        }

                        if (matchType is SearchMatchType.All or SearchMatchType.Text)
                        {
                            if (entryText.Contains(searchText))
                            {
                                TextResult result = new();
                                result.ContainerName = containerName;
                                result.ContainerWrapper = entry;
                                result.FmgID = fmg.ID;
                                result.FmgName = fmg.Name;
                                result.Fmg = fmg.File;
                                result.FmgEntryID = fmgEntry.ID;
                                result.Entry = fmgEntry;

                                results.Add(result);
                            }
                        }
                    }

                    if (matchType is SearchMatchType.All or SearchMatchType.ID)
                    {
                        if(Regex.IsMatch(searchText, @"^\d+$"))
                        {
                            try
                            {
                                var id = int.Parse(searchText);
                                if (fmgEntry.ID == id)
                                {
                                    TextResult result = new();
                                    result.ContainerName = containerName;
                                    result.ContainerWrapper = entry;
                                    result.FmgID = fmg.ID;
                                    result.FmgName = fmg.Name;
                                    result.Fmg = fmg.File;
                                    result.FmgEntryID = fmgEntry.ID;
                                    result.Entry = fmgEntry;

                                    results.Add(result);
                                }
                            }
                            catch { }
                        }
                    }
                }
            }
        }

        return results;
    }


    /// <summary>
    /// Get text result for global replacement.
    /// </summary>
    /// <summary>
    /// Get text result for global replacement.
    /// The same compiled regex (pattern + options) is used here and when applying,
    /// so the preview lists exactly the entries the replacement will change.
    /// Each entry appears at most once; entries with null text are never included,
    /// since there is no text to replace in.
    /// </summary>
    public static List<ReplacementResult> GetReplacementResult(TextEditorView view, Regex searchRegex, string searchPattern, SearchFilterType searchFilterType, SearchMatchType matchType)
    {
        var results = new List<ReplacementResult>();

        int? searchId = null;
        if (Regex.IsMatch(searchPattern, @"^\d+$") && int.TryParse(searchPattern, out var parsedId))
        {
            searchId = parsedId;
        }

        foreach (var (fileEntry, entry) in view.Project.Handler.TextData.PrimaryBank.Containers)
        {
            var containerName = fileEntry.Filename;

            if (searchFilterType is SearchFilterType.PrimaryCategory)
            {
                if (entry.ContainerDisplayCategory != CFG.Current.TextEditor_Primary_Category)
                {
                    continue;
                }
            }

            // Match the preview list: unused containers are hidden there, so don't change them either.
            if (CFG.Current.TextEditor_Container_List_Hide_Unused_Containers && entry.IsContainerUnused())
            {
                continue;
            }

            if(entry.FmgWrappers == null || entry.FmgWrappers.Count == 0)
            {
                view.Project.Handler.TextData.PrimaryBank.LoadFmgWrappers(entry);
            }

            foreach (var fmg in entry.FmgWrappers)
            {
                foreach (var fmgEntry in fmg.File.Entries)
                {
                    if (fmgEntry.Text == null)
                        continue;

                    Match match = null;
                    var isMatch = false;

                    if (matchType is SearchMatchType.All or SearchMatchType.Text)
                    {
                        match = searchRegex.Match(fmgEntry.Text);
                        isMatch = match.Success;
                    }

                    if (!isMatch && matchType is SearchMatchType.All or SearchMatchType.ID)
                    {
                        isMatch = searchId.HasValue && fmgEntry.ID == searchId.Value;
                    }

                    if (isMatch)
                    {
                        ReplacementResult result = new();
                        result.Match = match;
                        result.ContainerName = containerName;
                        result.ContainerWrapper = entry;
                        result.FmgID = fmg.ID;
                        result.FmgName = fmg.Name;
                        result.Fmg = fmg.File;
                        result.FmgEntryID = fmgEntry.ID;
                        result.Entry = fmgEntry;

                        results.Add(result);
                    }
                }
            }
        }

        return results;
    }
}

public class TextResult
{
    public string ContainerName { get; set; }
    public TextContainerWrapper ContainerWrapper { get; set; }
    public int FmgID { get; set; }
    public string FmgName { get; set; }
    public FMG Fmg { get; set; }
    public int FmgEntryID { get; set; }
    public FMG.Entry Entry { get; set; }

    public TextResult() { }
}

public class ReplacementResult
{
    public string ContainerName { get; set; }
    public Match Match { get; set; }
    public TextContainerWrapper ContainerWrapper { get; set; }
    public int FmgID { get; set; }
    public string FmgName { get; set; }
    public FMG Fmg { get; set; }
    public int FmgEntryID { get; set; }
    public FMG.Entry Entry { get; set; }

    /// <summary>
    /// The text the entry will have after the replacement, as shown in the preview.
    /// </summary>
    public string NewText { get; set; }

    /// <summary>
    /// Whether Apply should change this entry (the preview's checkbox). On by default.
    /// </summary>
    public bool Included { get; set; } = true;

    public ReplacementResult() { }
}