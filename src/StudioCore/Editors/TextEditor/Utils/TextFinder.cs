using SoulsFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor.Utils;

public static class TextFinder
{
    private static Dictionary<string, TextResult> CachedResults = new();

    private static Dictionary<string, TextResult> CachedGlobalResults = new();

    /// <summary>
    /// Get FMG reference, caching it on initial search, and then accessing cache on future usage
    /// </summary>
    public static TextResult GetTextResult(string fmgName, int value, int offset = 0)
    {
        var cacheName = $"{fmgName}{value}{offset}";

        if (CachedResults.ContainsKey(cacheName))
        {
            return CachedResults[cacheName];
        }

        if (!TextBank.PrimaryBankLoaded)
        {
            return null;
        }

        foreach(var (path, entry) in TextBank.FmgBank)
        {
            var containerName = Path.GetFileName(path);

            if (entry.Category == CFG.Current.TextEditor_PrimaryCategory)
            {
                foreach (var fmg in entry.FmgInfos)
                {
                    var enumName = TextUtils.GetFmgInternalName(entry, fmg.ID, fmg.Name);

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
                                result.Info = entry;
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
    public static List<TextResult> GetGlobalTextResult(string searchTerm, SearchFilterType searchFilterType, bool ignoreCase)
    {
        var results = new List<TextResult>();

        if (!TextBank.PrimaryBankLoaded)
        {
            return results;
        }

        foreach (var (path, entry) in TextBank.FmgBank)
        {
            var containerName = Path.GetFileName(path);

            if (searchFilterType is SearchFilterType.PrimaryCategory)
            {
                if (entry.Category != CFG.Current.TextEditor_PrimaryCategory)
                {
                    continue;
                }
            }

            foreach (var fmg in entry.FmgInfos)
            {
                foreach (var fmgEntry in fmg.File.Entries)
                {
                    var entryText = fmgEntry.Text;
                    var searchText = searchTerm;

                    if (entryText == null)
                        continue;

                    if (ignoreCase)
                    {
                        entryText = entryText.ToLower();
                        searchText = searchText.ToLower();
                    }

                    if(entryText.Contains(searchText))
                    {
                        TextResult result = new();
                        result.ContainerName = containerName;
                        result.Info = entry;
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
    public TextContainerInfo Info { get; set; }
    public int FmgID { get; set; }
    public string FmgName { get; set; }
    public FMG Fmg { get; set; }
    public int FmgEntryID { get; set; }
    public FMG.Entry Entry { get; set; }

    public TextResult() { }
}