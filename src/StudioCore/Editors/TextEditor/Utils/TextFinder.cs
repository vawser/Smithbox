using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor.Utils;

public static class TextFinder
{
    private static Dictionary<string, TextResult> CachedResults = new();

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
            if (entry.Category == TextBank.PrimaryCategory)
            {
                foreach (var fmg in entry.FmgInfos)
                {
                    var enumName = TextUtils.GetFmgInternalName(entry, fmg.ID);

                    // Contains here to capture the _DLC, _DLC1 and _DLC2 fmgs
                    if (enumName.Contains(fmgName))
                    {
                        foreach (var fmgEntry in fmg.File.Entries)
                        {
                            var entryId = Math.Abs(value) + offset;

                            if (fmgEntry.ID == entryId)
                            {
                                TextResult result = new();
                                result.Info = entry;
                                result.FmgName = fmg.Name;
                                result.Fmg = fmg.File;
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
}

public class TextResult
{
    public TextContainerInfo Info { get; set; }
    public string FmgName { get; set; }
    public FMG Fmg { get; set; }
    public FMG.Entry Entry { get; set; }

    public TextResult() { }
}