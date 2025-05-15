using SoulsFormats;
using StudioCore.Editors.TextEditor.Enums;
using StudioCore.Editors.TimeActEditor.Utils;
using StudioCore.TextEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor.Utils;

public static class TextFinder
{
    private static Dictionary<string, TextResult> CachedResults = new();

    private static Dictionary<string, TextResult> CachedGlobalResults = new();

    /// <summary>
    /// Get FMG reference, caching it on initial search, and then accessing cache on future usage
    /// </summary>
    public static TextResult GetTextResult(TextEditorScreen editor, string fmgName, int value, int offset = 0)
    {
        var cacheName = $"{fmgName}{value}{offset}";

        if (CachedResults.ContainsKey(cacheName))
        {
            return CachedResults[cacheName];
        }

        foreach(var (fileEntry, entry) in editor.Project.TextData.PrimaryBank.Entries)
        {
            var containerName = fileEntry.Filename;

            if (entry.ContainerDisplayCategory == CFG.Current.TextEditor_PrimaryCategory)
            {
                foreach (var fmg in entry.FmgWrappers)
                {
                    var enumName = TextUtils.GetFmgInternalName(editor.Project, entry, fmg.ID, fmg.Name);

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

                                if (!CachedResults.ContainsKey(cacheName))
                                {
                                    CachedResults.Add(cacheName, result);
                                }

                                return result;
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
    public static List<TextResult> GetGlobalTextResult(TextEditorScreen editor, string searchTerm, SearchFilterType searchFilterType, SearchMatchType matchType, bool ignoreCase)
    {
        var results = new List<TextResult>();

        foreach (var (fileEntry, entry) in editor.Project.TextData.PrimaryBank.Entries)
        {
            var containerName = fileEntry.Filename;

            if (searchFilterType is SearchFilterType.PrimaryCategory)
            {
                if (entry.ContainerDisplayCategory != CFG.Current.TextEditor_PrimaryCategory)
                {
                    continue;
                }
            }

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
    public static List<ReplacementResult> GetReplacementResult(TextEditorScreen editor, string searchPattern, SearchFilterType searchFilterType, SearchMatchType matchType, bool ignoreCase)
    {
        var results = new List<ReplacementResult>();

        foreach (var (fileEntry, entry) in editor.Project.TextData.PrimaryBank.Entries)
        {
            var containerName = fileEntry.Filename;

            if (searchFilterType is SearchFilterType.PrimaryCategory)
            {
                if (entry.ContainerDisplayCategory != CFG.Current.TextEditor_PrimaryCategory)
                {
                    continue;
                }
            }

            foreach (var fmg in entry.FmgWrappers)
            {
                foreach (var fmgEntry in fmg.File.Entries)
                {
                    var entryText = fmgEntry.Text;
                    var searchText = searchPattern;

                    if (entryText != null)
                    {
                        if (ignoreCase)
                        {
                            entryText = entryText.ToLower();
                            searchText = searchText.ToLower();
                        }

                        if (matchType is SearchMatchType.All or SearchMatchType.Text)
                        {
                            var match = Regex.Match(entryText, searchText);

                            if (match.Success)
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

                    if (matchType is SearchMatchType.All or SearchMatchType.ID)
                    {
                        if (Regex.IsMatch(searchText, @"^\d+$"))
                        {
                            try
                            {
                                var id = int.Parse(searchText);
                                if (fmgEntry.ID == id)
                                {
                                    ReplacementResult result = new();
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

    public ReplacementResult() { }
}